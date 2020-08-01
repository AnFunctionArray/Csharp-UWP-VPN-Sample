# Csharp-UWP-VPN-Sample

Don't forget to add the preshared key (in this case superfreevpn.com) in the UWP settings pane for this VPN connection after setting L2TP with preshared key instead of certificate.

There is no otpion to automate this as far as I know.

Also this VPN provide changes their passess frequently so check their website to update them before running.

Also as always the Credentials Manager (search from the start menu) will hold your Vault resource in the Web section. 

And the Event Viewer will tell you detailed information if there is an error. In the Windows Logs/Application with Source RasClient.

The VPN profile will be created on the first run.
