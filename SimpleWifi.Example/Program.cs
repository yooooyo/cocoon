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

namespace SimpleWifi.Example
{
	internal class Program
	{
		private static Wifi wifi;
        private static WiFiAdapter firstAdapter;
        private static Radio wifiRadio;
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
                    Console.WriteLine("ON.   Turn on wi-fi radio button");
                    Console.WriteLine("OFF.  Turn off wi-fi radio button");
                    Console.WriteLine("Q. Quit");
                    Console.WriteLine("");

                    command = Console.ReadLine().ToLower();

                    Execute(command);
                } while (command != "q");

            }
        }



        private static void Execute(string command)
		{
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
                case "on":
                    OnOffWifi(true);
                    break;
                case "off":
                    OnOffWifi(false);
                    break;
                case "q":
					break;
				default:
					Console.WriteLine("\r\nIncorrect command.");
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


        private static async void OnOffWifi(bool turn)
        {
            radiochange = null;
            var access = await Radio.RequestAccessAsync();
            if(access == RadioAccessStatus.Allowed)
            {
                wifiRadio = (await Radio.GetRadiosAsync()).Where(x=>x.Kind == RadioKind.WiFi).FirstOrDefault();
                wifiRadio.StateChanged += WifiRadio_StateChanged;
                if (wifiRadio!= null)
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
