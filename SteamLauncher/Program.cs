using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Configuration;
using System.Diagnostics;

namespace SteamLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Help");
                Console.WriteLine("Please, specify following parameters:");
                Console.WriteLine("App you want to launch (name)");
                Environment.Exit(2);
                return;
            }
            if (args.Length != 1)
            {
                Console.WriteLine("You've specified insufficient parameters");
                Console.WriteLine("Please, specify following parameters:");
                Console.WriteLine("App you want to launch (name)");
                Environment.Exit(2);
                return;
            }
            string steamPath = ConfigurationManager.AppSettings["Steam.exe Path"].ToLower();
            string steamWorkingFolder = ConfigurationManager.AppSettings["Steam FOlder Path"].ToLower();
            string xmlPath = ConfigurationManager.AppSettings["XML Paths"].ToLower();
            string appName = args[0].ToLower();
            string computerName = "";
            try
            {
                computerName = System.Environment.GetEnvironmentVariable("COMPUTERNAME").ToLower();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured while getting computername ", ex);
                Environment.Exit(-1);
            }
            if (computerName != "")
            {
                Program p = new Program(steamPath, xmlPath, appName, computerName, steamWorkingFolder);
            }
            else
            {
                Console.WriteLine("ComputerName is empty ");
                Environment.Exit(-1);
            }

        }

        public Program(string steamPath, string xmlPath, string appName, string computerName, string steamWorkingFolder)
        {
            List<string> readdata = steam_XMLReader(xmlPath, computerName, appName);
            if (readdata.Count != 4)
            {
                Console.WriteLine("Xml is malformed, there is duplicate entries with either computername or appname");
                Environment.Exit(-1);
            }
            readdata.Add(steamPath);
            StartSteam(readdata, steamWorkingFolder);
        }

        public static List<string> steam_XMLReader(string xmlPath, string computerName, string appName)
        {
            List<string> list = new List<string>();
            XDocument doc = XDocument.Load(xmlPath);

            var details = from computer in doc.Root.Elements("computer")
                          select new
                          {
                              computername = (string)computer.Element("computername"),
                              login = (string)computer.Element("steamlogin"),
                              password = (string)computer.Element("steampassword"),
                              apps = from app in computer.Element("apps").Elements("app")
                                     select new
                                     {
                                         appname = (string)app.Attribute("name"),
                                         applaunch = (string)app.Attribute("launch"),
                                         appparameters = (string)app.Attribute("parameters")
                                     }

                          };
            foreach (var detail in details)
            {
                if (detail.computername.ToLower().Equals(computerName.ToLower()))
                {
                    list.Add("-login " + detail.login);
                    list.Add(detail.password);
                    foreach (var app in detail.apps)
                    {
                        if (app.appname.ToLower().Equals(appName.ToLower()))
                        {
                            list.Add("-applaunch " + app.applaunch);
                            list.Add(app.appparameters);
                        }

                    }
                }
            }
            return list;
        }

        public void StartSteam(List<string> data, string workingFolder)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.Arguments = data[2] + " " + data[0] + " " + data[1] + " " + data[3];
            startInfo.FileName = data[4];
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = workingFolder;
            Process.Start(startInfo);
        }

    }
}
