using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace SteamLauncher
{
    class Program
    {
        private static readonly byte[] _salt = Encoding.ASCII.GetBytes("o6806642kbM7c5");
        private const string encryptionPassword = "test";

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
            if (args.Length != 2)
            {
                Console.WriteLine("You've specified insufficient parameters");
                Console.WriteLine("Please, specify following parameters:");
                Console.WriteLine("App you want to launch (name)");
                Console.WriteLine("Username as stored in computername node of xml");
                Environment.Exit(2);
                return;
            }
            string steamPath = ConfigurationManager.AppSettings["Steam.exe Path"].ToLower();
            string steamWorkingFolder = ConfigurationManager.AppSettings["Steam Folder Path"].ToLower();
            string xmlPath = ConfigurationManager.AppSettings["XML Paths"].ToLower();
            string appName = args[0].ToLower();
            string computerName = args[1].ToLower();
            if (computerName != "")
            {
                Program p = new Program(steamPath, xmlPath, appName, computerName, steamWorkingFolder);
            }
            else
            {
                Console.WriteLine("ComputerName(username) is empty");
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
                    list.Add("-login " + DecryptStringAES(detail.login, encryptionPassword));
                    list.Add(DecryptStringAES(detail.password, encryptionPassword));
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
        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using 
        /// DecryptStringAES().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        public static string EncryptStringAES(string plainText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException("plainText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            string outStr = null;                       // Encrypted string to return
            RijndaelManaged aesAlg = null;              // RijndaelManaged object used to encrypt the data.

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            // Return the encrypted bytes from the memory stream.
            return outStr;
        }

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using 
        /// EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        public static string DecryptStringAES(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(sharedSecret))
                throw new ArgumentNullException("sharedSecret");

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _salt);

                // Create the streams used for decryption.                
                byte[] bytes = Convert.FromBase64String(cipherText);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                    aesAlg.Clear();
            }

            return plaintext;
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted byte array");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read byte array properly");
            }

            return buffer;
        }
    }
}
