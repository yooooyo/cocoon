using SimpleWifi;
using SimpleWifi.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Networking.Connectivity;
using Windows.Devices.Radios;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Windows.Networking.NetworkOperators;

namespace SimpleWifi.Example
{
	internal class Program
	{
		private static Wifi wifi;
        private static WiFiAdapter firstAdapter;
        private static Radio wifiRadio;
        private static Radio bleRadio;
        private static bool? radiochange;
        private static void Main(string[] args)
		{
            // Init wifi object and event handlers
            wifi = new Wifi();
			wifi.ConnectionStatusChanged += wifi_ConnectionStatusChanged;
            

            if (wifi.NoWifiAvailable)
				Console.WriteLine("\r\n-- NO WIFI CARD WAS FOUND --");

			string command = "";
            if (args.Count() == 1)
            {
                command = args[0];
                Execute(command);
                Thread.Sleep(500);
            }
            else if(args.Count()==5 && args[0].Contains("connect") && args[1].Contains("/ssid:") && args[2].Contains("/password:") && args[3].Contains("/username:") && args[4].Contains("/domain:"))
            {
                var ssid = args[1].Length > "/ssid:".Length ? args[1].Substring(6) : null;
                var password = args[2].Length > "/password:".Length ? args[2].Substring(10) : null;
                var username = args[3].Length> "/username:".Length? args[3].Substring(10) : null;
                var domain =   args[4].Length > "/domain:".Length ? args[4].Substring(8) : null;
                if(ssid != null)
                {
                    Connect(ssid,password,username,domain);
                    Thread.Sleep(2000);
                }
                else
                    Console.WriteLine("\r\nplease enter ssid");
            }
            else
            {
                do
                {

                    Console.WriteLine("\r\n-- COMMAND LIST --");
                    Console.WriteLine("L. List access points");
                    Console.WriteLine("C. Connect");
                    Console.WriteLine("D. Disconnect");
                    Console.WriteLine("S. Status");
                    Console.WriteLine("X. Print profile XML");
                    Console.WriteLine("R. Remove profile");
                    Console.WriteLine("I. Show access point information");

                    Console.WriteLine("SCAN. Rescan access point");
                    Console.WriteLine("WIFI-ON.   Turn on wi-fi radio button");
                    Console.WriteLine("WIFI-OFF.  Turn off wi-fi radio button");
                    Console.WriteLine("BLE-ON.   Turn on bluetooth radio button");
                    Console.WriteLine("BLE-OFF.  Turn off bluetooth radio button");
                    Console.WriteLine("AIR-SWITCH.  Switch airplane mode");
                    Console.WriteLine("AIR-CHECK.   Switch airplane mode");
                    Console.WriteLine("AIR-ON.   Switch airplane mode");
                    Console.WriteLine("AIR-OFF.  Switch airplane mode");
                    Console.WriteLine("HOTSPOT-ON.   Turn hotspot on");
                    Console.WriteLine("HOTSPOT-OFF.  Turn hotspot off");
                    Console.WriteLine("WWAN-FIRMWARE.  Get WWAN Firmware");
                    Console.WriteLine("Q. Quit");
                    Console.WriteLine("");

                    command = Console.ReadLine().ToLower();

                    Execute(command);
                } while (command != "q");

            }
        }


        private static void Execute(string command)
		{
            command.ToLower();

            switch (command)
			{
				case "l":
					List();
					break;
				case "d":
					Disconnect();
					break;
				case "c":
					Connect();
					break;
				case "s":
					Status();
					break;
				case "x":
					ProfileXML();
					break;
				case "r":
					DeleteProfile();
					break;
				case "i":
					ShowInfo();
					break;
                case "scan":
                    ScanWifi();
                    break;
                case "wifi-on":
                    OnOffWifi(true);
                    break;
                case "wifi-off":
                    OnOffWifi(false);
                    break;
                case "ble-on":
                    OnOffBLE(true);
                    break;
                case "ble-off":
                    OnOffBLE(false);
                    break;
                case "air-switch":
                    SwitchAirplane();
                    break;
                case "air-on":
                    SwitchAirplane("on");
                    break;
                case "air-off":
                    SwitchAirplane("off");
                    break;
                case "air-check":
                    SwitchAirplane("check");
                    break;
                case "hotspot-on":
                    EnableHotspot();
                    break;
                case "hotspot-off":
                    DisableHotspot();
                    break;
                case "wwan-firmware":
                    PrintMobileInfo("firmware");
                    break;
                case "wwan-all":
                    PrintMobileInfo("all");
                    break;
                case "q":
					break;
				default:
					Console.WriteLine("\r\nIncorrect command.");
                    Console.WriteLine("or use");
                    Console.WriteLine("connect /ssid:<ssid> /password:<password>.   Connect AP");
                    break;
			}
		}

		private static void Disconnect()
		{
			wifi.Disconnect();
		}

		private static void Status()
		{
			Console.WriteLine("\r\n-- CONNECTION STATUS --");
			if (wifi.ConnectionStatus == WifiStatus.Connected)
				Console.WriteLine("You are connected to a wifi");
			else
				Console.WriteLine("You are not connected to a wifi");
		}

        static MobileBroadbandDeviceInformation getMobileInfo()
        {
            var modem = MobileBroadbandModem.GetDefault();
            if(modem != null)
            {
                var info = modem.DeviceInformation;
                if (info != null) return info;
            }
            Console.WriteLine("You have no wwan module");
            return null;
        }
        
        enum wwanInfoEnum
        {
            cellularclass,
            radiostate,
            deviceid,
            devicetype,
            firmware,
            manufacturer,
            mobileequipmentid,
            model,
            networkdevicestatus,
            revision,
            serialnumber,
            simiccid,
            subscriberid
        }
        static void PrintMobileInfo(string info)
        {
            var infoObj = getMobileInfo();
            if (infoObj != null)
            {
                info = info.ToLower();
                switch (info)
                {
                    case "cellularclass":
                        Console.WriteLine(infoObj.CellularClass.ToString());
                        break;
                    case "radiostate":
                        Console.WriteLine(infoObj.CurrentRadioState.ToString());
                        break;
                    case "deviceid":
                        Console.WriteLine(infoObj.DeviceId);
                        break;
                    case "devicetype":
                        Console.WriteLine(infoObj.DeviceType.ToString());
                        break;
                    case "firmware":
                        Console.WriteLine(infoObj.FirmwareInformation);
                        break;
                    case "manufacturer":
                        Console.WriteLine(infoObj.Manufacturer);
                        break;
                    case "mobileequipmentid":
                        Console.WriteLine(infoObj.MobileEquipmentId);
                        break;
                    case "model":
                        Console.WriteLine(infoObj.Model);
                        break;
                    case "networkdevicestatus":
                        Console.WriteLine(infoObj.NetworkDeviceStatus.ToString());
                        break;
                    case "revision":
                        Console.WriteLine(infoObj.Revision);
                        break;
                    case "serialnumber":
                        Console.WriteLine(infoObj.SerialNumber);
                        break;
                    case "simiccid":
                        Console.WriteLine(infoObj.SimIccId);
                        break;
                    case "subscriberid":
                        Console.WriteLine(infoObj.SubscriberId);
                        break;
                    default:
                        Console.WriteLine("Cellular Class: "+infoObj.CellularClass.ToString());
                        Console.WriteLine("Current Radio State: "+infoObj.CurrentRadioState.ToString());
                        Console.WriteLine("Device ID: "+infoObj.DeviceId);
                        Console.WriteLine("Device Type: "+infoObj.DeviceType.ToString());
                        Console.WriteLine("Firmware: "+infoObj.FirmwareInformation);
                        Console.WriteLine("Manufacture: "+infoObj.Manufacturer);
                        Console.WriteLine("MobileEquipmentId: " + infoObj.MobileEquipmentId);
                        Console.WriteLine("Model: "+infoObj.Model);
                        Console.WriteLine("Network Device Status: " + infoObj.NetworkDeviceStatus.ToString());
                        Console.WriteLine("Revision: "+infoObj.Revision);
                        Console.WriteLine("SerialNumber: " + infoObj.SerialNumber);
                        Console.WriteLine("SimIccId: " + infoObj.SimIccId);
                        Console.WriteLine("SubscriberId: " + infoObj.SubscriberId);
                        break;
                }
            }
        }

		private static IEnumerable<AccessPoint> List()
		{
            ScanWifi();
            Thread.Sleep(1000);
            Console.WriteLine("\r\n-- Access point list --");
			IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints().OrderByDescending(ap => ap.SignalStrength);

			int i = 0;
			foreach (AccessPoint ap in accessPoints)
				Console.WriteLine("{0}. {1} {2}% Connected: {3}", i++, ap.Name, ap.SignalStrength, ap.IsConnected);

			return accessPoints;
		}
        static int ariplane_reg
        {
            get
            {
                return (int)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Control\RadioManagement\SystemRadioState", "", "");
            }
        }
        private static void SwitchAirplane(string option="switch")
        {
            radiochange = null;

            var batch = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "airplaneswitch.bat");
            if (File.Exists(batch))
            {
                try
                {
                    var prestate = ariplane_reg;
                    switch (option)
                    {
                        case "switch":
  
                            if (Process.Start(batch).WaitForExit(5000))
                            {
                                if (prestate != ariplane_reg)
                                {
                                    Console.WriteLine("airplaneswitch.bat operation pass");
                                }
                            }
                            else
                            {
                                Console.WriteLine("airplaneswitch.bat operation fail");
                            }
                            break;
                        case "on":
                            if(prestate == 0)
                            {
                                if (Process.Start(batch).WaitForExit(5000))
                                {
                                    if (prestate != ariplane_reg)
                                    {
                                        Console.WriteLine("airplaneswitch.bat operation pass");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("airplaneswitch.bat operation fail");
                                }
                            }
                            else
                            {
                                Console.WriteLine("already on");
                            }
                            break;
                        case "off":
                            if (prestate == 1)
                            {
                                if (Process.Start(batch).WaitForExit(5000))
                                {
                                    if (prestate != ariplane_reg)
                                    {
                                        Console.WriteLine("airplaneswitch.bat operation pass");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("airplaneswitch.bat operation fail");
                                }
                            }
                            else
                            {
                                Console.WriteLine("already off");
                            }
                            break;
                        case "check":
                            Console.WriteLine(ariplane_reg==0?"off":"on");
                            break;
                    }


                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            else
            {
                Console.WriteLine("Can't find airplaneswitch.bat");
            }
        }
        private static async void OnOffBLE(bool turn)
        {
            radiochange = null;
            var access = await Radio.RequestAccessAsync();
            if (access == RadioAccessStatus.Allowed)
            {
                bleRadio = (await Radio.GetRadiosAsync()).Where(x => x.Kind == RadioKind.Bluetooth).FirstOrDefault();
                bleRadio.StateChanged += WifiRadio_StateChanged;
                if (bleRadio != null)
                {
                    if (turn)
                    {
                        if (bleRadio.State == RadioState.Off)
                        {
                            radiochange = false;
                            await bleRadio.SetStateAsync(RadioState.On);
                            while (radiochange == false) ;
                        }

                    }
                    else
                    {
                        if (bleRadio.State == RadioState.On)
                        {
                            radiochange = false;
                            await bleRadio.SetStateAsync(RadioState.Off);
                            while (radiochange == false) ;
                        }
                    }
                }

            }
            else
            {
                Console.WriteLine("\r\nNot allowed to access Radios");
            }
        }

        private static async void OnOffWifi(bool turn)
        {
            radiochange = null;
            var access = await Radio.RequestAccessAsync();
            if(access == RadioAccessStatus.Allowed)
            {
                wifiRadio = (await Radio.GetRadiosAsync()).Where(x => x.Kind == RadioKind.WiFi).FirstOrDefault();
                wifiRadio.StateChanged += WifiRadio_StateChanged;
                if (wifiRadio != null)
                {
                    if (turn)
                    {
                        if (wifiRadio.State == RadioState.Off)
                        {
                            radiochange = false;
                            await wifiRadio.SetStateAsync(RadioState.On);
                            while (radiochange == false) ;
                        }

                    }
                    else
                    {
                        if (wifiRadio.State == RadioState.On)
                        {
                            radiochange = false;
                            await wifiRadio.SetStateAsync(RadioState.Off);
                            while (radiochange == false) ;
                        }
                    }
                }

            }
            else
            {
                Console.WriteLine("\r\nNot allowed to access Radios");
            }

        }



        private static async void ScanWifi()
        {
            var access = await WiFiAdapter.RequestAccessAsync();
            if (access != WiFiAccessStatus.Allowed)
            {
                Console.WriteLine("\r\nNot allowed to access Adapters");
            }
            else
            {
                var result = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(WiFiAdapter.GetDeviceSelector());
                if (result.Count >= 1)
                {
                    firstAdapter = await WiFiAdapter.FromIdAsync(result[0].Id);
                    firstAdapter.AvailableNetworksChanged += FirstAdapter_AvailableNetworksChanged;
                    for(int i=0; i<2; i++)
                    {
                        await firstAdapter.ScanAsync();
                        Thread.Sleep(2000);
                    }

                }
                else
                {
                    Console.WriteLine("\r\nNo WiFi Adapters detected on this machine");
                }
            }
            
        }
        private static void Connect(string ssid,string password=null, string username=null,string domainname=null)
        {
            var accessPoints = List();

            var selectedAP =  accessPoints.Where(x => x.Name == ssid).FirstOrDefault();
            //Auth

            if (selectedAP != null)
            {
                AuthRequest authRequest = new AuthRequest(selectedAP);
                bool overwrite = false;
                if (authRequest.IsPasswordRequired)
                {
                    if (PasswordPrompt(selectedAP,password))
                    {
                        authRequest.Password = password;
                        if (selectedAP.HasProfile)
                        {
                            overwrite = true;
                        }
                    }

                    if (overwrite)
                    {
                        if (authRequest.IsUsernameRequired)
                        {
                            if(username != null)
                                authRequest.Username = username;
                        }

                        

                        if (authRequest.IsDomainSupported)
                        {
                            if(domainname != null)
                                authRequest.Domain = Console.ReadLine();
                        }
                    }
                }
                selectedAP.ConnectAsync(authRequest, overwrite, OnConnectedComplete);
            }
            else
                Console.WriteLine($"Can't find access point {ssid}");


        }

        private static void Connect()
		{
            var accessPoints = List();

			Console.Write("\r\nEnter the index of the network you wish to connect to: ");

			int selectedIndex = int.Parse(Console.ReadLine());
			if (selectedIndex > accessPoints.ToArray().Length || accessPoints.ToArray().Length == 0)
			{
				Console.Write("\r\nIndex out of bounds");
				return;
			}
			AccessPoint selectedAP = accessPoints.ToList()[selectedIndex];

			// Auth
			AuthRequest authRequest = new AuthRequest(selectedAP);
			bool overwrite = true;

			if (authRequest.IsPasswordRequired)
			{
				if (selectedAP.HasProfile)
					// If there already is a stored profile for the network, we can either use it or overwrite it with a new password.
				{
					Console.Write("\r\nA network profile already exist, do you want to use it (y/n)? ");
					if (Console.ReadLine().ToLower() == "y")
					{
						overwrite = false;
					}
				}

				if (overwrite)
				{
					if (authRequest.IsUsernameRequired)
					{
						Console.Write("\r\nPlease enter a username: ");
						authRequest.Username = Console.ReadLine();
					}

					authRequest.Password = PasswordPrompt(selectedAP);

					if (authRequest.IsDomainSupported)
					{
						Console.Write("\r\nPlease enter a domain: ");
						authRequest.Domain = Console.ReadLine();
					}
				}
			}

			selectedAP.ConnectAsync(authRequest, overwrite, OnConnectedComplete);
		}

		private static string PasswordPrompt(AccessPoint selectedAP)
		{
			string password = string.Empty;

			bool validPassFormat = false;

			while (!validPassFormat)
			{
				Console.Write("\r\nPlease enter the wifi password: ");
				password = Console.ReadLine();

				validPassFormat = selectedAP.IsValidPassword(password);

				if (!validPassFormat)
					Console.WriteLine("\r\nPassword is not valid for this network type.");
			}

			return password;
		}
        private static bool PasswordPrompt(AccessPoint selectedAP,string password)
        {

            bool validPassFormat = false;


            validPassFormat = selectedAP.IsValidPassword(password);

            if (!validPassFormat)
                Console.WriteLine("\r\nPassword is not valid for this network type.");

            return validPassFormat;
        }

        private static void ProfileXML()
		{
			var accessPoints = List();

			Console.Write("\r\nEnter the index of the network you wish to print XML for: ");

			int selectedIndex = int.Parse(Console.ReadLine());
			if (selectedIndex > accessPoints.ToArray().Length || accessPoints.ToArray().Length == 0)
			{
				Console.Write("\r\nIndex out of bounds");
				return;
			}
			AccessPoint selectedAP = accessPoints.ToList()[selectedIndex];

			Console.WriteLine("\r\n{0}\r\n", selectedAP.GetProfileXML());
		}

		private static void DeleteProfile()
		{
			var accessPoints = List();

			Console.Write("\r\nEnter the index of the network you wish to delete the profile: ");

			int selectedIndex = int.Parse(Console.ReadLine());
			if (selectedIndex > accessPoints.ToArray().Length || accessPoints.ToArray().Length == 0)
			{
				Console.Write("\r\nIndex out of bounds");
				return;
			}
			AccessPoint selectedAP = accessPoints.ToList()[selectedIndex];

			selectedAP.DeleteProfile();
			Console.WriteLine("\r\nDeleted profile for: {0}\r\n", selectedAP.Name);
		}


		private static void ShowInfo()
		{
			var accessPoints = List();

			Console.Write("\r\nEnter the index of the network you wish to see info about: ");

			int selectedIndex = int.Parse(Console.ReadLine());
			if (selectedIndex > accessPoints.ToArray().Length || accessPoints.ToArray().Length == 0)
			{
				Console.Write("\r\nIndex out of bounds");
				return;
			}
			AccessPoint selectedAP = accessPoints.ToList()[selectedIndex];

			Console.WriteLine("\r\n{0}\r\n", selectedAP.ToString());
		}
        private static async void EnableHotspot()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            var tetheringManager  = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionProfile);

            if (tetheringManager.TetheringOperationalState == TetheringOperationalState.Off)
            {
                var task = await tetheringManager.StartTetheringAsync();
                if(task.Status == TetheringOperationStatus.Success)
                {
                    Console.WriteLine("Hotspot Turn On");
                }
                else
                {
                    Console.WriteLine($"Hotsopt operation {task.Status}");
                }
            }

        }
        private static async void DisableHotspot()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            var tetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionProfile);

            if (tetheringManager.TetheringOperationalState == TetheringOperationalState.On)
            {
                var task = await tetheringManager.StopTetheringAsync();
                if (task.Status == TetheringOperationStatus.Success)
                {
                    Console.WriteLine("Hotspot Turn Off");
                }
                else
                {
                    Console.WriteLine($"Hotsopt operation {task.Status}");
                }
            }

        }
        private static void WifiRadio_StateChanged(Radio sender, object args)
        {
            Console.WriteLine($"\nRadio {sender.Name} change state {sender.State.ToString()}");
            //throw new NotImplementedException();
        }
        private static void FirstAdapter_AvailableNetworksChanged(WiFiAdapter sender, object args)
        {
            //Console.WriteLine("\nWifi Scanning....");
            //throw new NotImplementedException();
        }
        private static void wifi_ConnectionStatusChanged(object sender, WifiStatusEventArgs e)
		{
			Console.WriteLine("\nNew status: {0}", e.NewStatus.ToString());
		}

		private static void OnConnectedComplete(bool success)
		{
			Console.WriteLine("\nOnConnectedComplete success: {0}", success);
		}


	}
}
