using System;
using Windows.Networking.Vpn;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.System;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.Networking.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using Windows.Foundation.Metadata;

namespace UWP_VPN_Sample
{
    class Program
    {
        static string profilename = "2020C#VPN";

        static string[] servernames = { "us.superfreevpn.com", "uk.superfreevpn.com" }; //not sponsored

        static string username = "free"; //currently we have the same usertname for all servers

        static string[] passess = { "3169", "1413" }; //may wish to update this on
                                                      //as for this free vpn provider is changes quite frequently

        static string vpncredentialsname = "CredentialsVPN";

        static async Task Main(string[] args)
        {
            var vpnmanager = new VpnManagementAgent();
            var profiles = await vpnmanager.GetProfilesAsync();
            IVpnProfile currprofile = null;
            VpnNativeProfile currnativeprofile = null;
            foreach (var profile in profiles)
                if (profile.ProfileName == profilename)
                {
                    currprofile = profile;
                    currnativeprofile = (VpnNativeProfile)currprofile;
                    break;
                }
            if (currprofile == null) //if we weren't able to find our profile
                                     //create a new one
            {
                var nativeprofile = new VpnNativeProfile();
                nativeprofile.RequireVpnClientAppUI = true;
                nativeprofile.NativeProtocolType = VpnNativeProtocolType.L2tp;
                nativeprofile.RoutingPolicyType = VpnRoutingPolicyType.ForceAllTrafficOverVpn;
                nativeprofile.Servers.Add(servernames[0]); //with the first server in the list
                nativeprofile.ProfileName = profilename;

                await vpnmanager.AddProfileFromObjectAsync(nativeprofile);

                currprofile = (IVpnProfile)nativeprofile;
                currnativeprofile = nativeprofile;
            }

            //currnativeprofile.RequireVpnClientAppUI = true;

            //await vpnmanager.UpdateProfileFromObjectAsync(currnativeprofile);

            await vpnmanager.DisconnectProfileAsync(currprofile); //disconnect in case of previous connection

            var currserverindex = Array.FindIndex(servernames, element => element == currnativeprofile.Servers[0]);

            var nextserverindex = servernames.Length > currserverindex + 1 ? currserverindex + 1 : 0;

            //var nextnextserverindex = servernames.Length > nextserverindex + 1 ? nextserverindex + 1 : 0;

            PasswordVault vault;

            IReadOnlyList<PasswordCredential> credentials;

            //bool bHasCreated = false;

            //set up credentials for next use

            vault = new PasswordVault();

            credentials = vault.RetrieveAll();

            if (credentials.Count != 0) //if there is our credential in UWP store delete it 
            {
                foreach (var localcredential in credentials)
                    if (localcredential.Resource == vpncredentialsname)
                    {
                        vault.Remove(localcredential);

                        break;
                    }
            }

            vault.Add(new PasswordCredential(vpncredentialsname, username, passess[nextserverindex]));

            var credential = new PasswordCredential();

            credential.UserName = username;

            credential.Resource = vpncredentialsname;

            credential.RetrievePassword();

            System.Console.WriteLine(credential.Password);

            var localfolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            var rasphonefile = await localfolder.GetFileAsync("rasphone.pbk");

            var rasphonetext = await FileIO.ReadTextAsync(rasphonefile);

            rasphonetext = rasphonetext.Replace(servernames[currserverindex], servernames[nextserverindex]);


            await FileIO.WriteTextAsync(rasphonefile, rasphonetext);

            do //try connecting
                //in a loop
                //sometimes it'll not connect unless you open the connection pane
                //this is an issue which microsoft will never fix
                //so you'll have to coupe with it

            {
                currnativeprofile = new VpnNativeProfile();
                currnativeprofile.ProfileName = profilename;
                //if (currnativeprofile.Servers[0] != servernames[nextserverindex])
                //    throw new System.Exception("Server wasn't successfully updated");

                await vpnmanager.DisconnectProfileAsync(currprofile);
                //await vpnmanager.ConnectProfileAsync(currprofile);
            } while (await vpnmanager.ConnectProfileWithPasswordCredentialAsync(currprofile, credential) != VpnManagementErrorStatus.Ok);

            //that is it we have hopefully (provided the password is correct and the VPN is ok) switched VPN connections

        }
    }
}
