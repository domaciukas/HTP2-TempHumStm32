using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace HTP2_TempHumStm32 {
    class NetworkProvider
    {
        public bool ConnectToNetwork(string wifiSsid, string wifiPassword) {
            var isOk = false;
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            
            if (networkInterfaces.Length > 0) {
                // Getting the first interface. Usually there is only one existing.
                NetworkInterface networkInterface = networkInterfaces[0];

                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) {
                    Debug.WriteLine("Network connection is: Wi-Fi");

                    var wirelessConfiguration = Wireless80211Configuration.GetAllWireless80211Configurations()[networkInterface.SpecificConfigId];
                    if (wirelessConfiguration.Ssid != wifiSsid && wirelessConfiguration.Password != wifiPassword)
                    {
                        wirelessConfiguration.Ssid = wifiSsid;
                        wirelessConfiguration.Password = wifiPassword;
                        wirelessConfiguration.SaveConfiguration();
                    }
                } 
                else {
                    Debug.WriteLine("Network connection is: Ethernet");
                    networkInterface.EnableDhcp();
                }

                Debug.WriteLine("Waiting for IP...");
                var retryCount = 0;

                while ((string.IsNullOrEmpty(networkInterface.IPv4Address) || networkInterface.IPv4Address == "0.0.0.0") && retryCount < 60) {
                    retryCount++;
                    Debug.WriteLine("Retrying. Count: " + retryCount);
                    Thread.Sleep(1000);
                }

                if (retryCount == 60 || networkInterface.IPv4Address.Equals("0")) {
                    Debug.WriteLine("No IP address has been acquired. Check your SSID and password or router.");
                } 
                else {
                    isOk = true;
                    Debug.WriteLine("Connected. IP address is " + networkInterface.IPv4Address);
                }
            } 
            else {
                throw new NotSupportedException("ERROR: there is no network interface configured.\r\nOpen the 'Edit Network Configuration' in Device Explorer and configure one.");
            }

            return isOk;
        }
    }
}
