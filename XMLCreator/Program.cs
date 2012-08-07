using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml;
using System.IO;
using System.Security.Cryptography;

namespace XMLCreator
{
    class Program
    {
        public static Dictionary<string, List<string>> globalAppsDictionary = new Dictionary<string, List<string>>();
        public static bool isGlobalAppsDefined = false;
        public static int globalApps = 0;
        private static byte[] _salt = Encoding.ASCII.GetBytes("o6806642kbM7c5");
        private const string encryptionPassword = "test";

        static void Main(string[] args)
        {
            string xmlPath = ConfigurationManager.AppSettings["XML Path"].ToLower();

            GlobalAppsDefiner();
            GenerateXML(xmlPath);
        }

        public static void GenerateXML(string xmlPath)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            int computersCount = 0;
            int enteredComputers = 1;
            int appsCount = 0;
            string computerName = string.Empty;
            string steamLogin = string.Empty;
            string steamPassword = string.Empty;
            string recoveryEmail = string.Empty;
            string recoveryEmailPassword = string.Empty;
            XmlDocument doc = new XmlDocument();
            XmlNode xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xmlDeclaration);

            XmlNode steamNode = doc.CreateElement("steam");
            doc.AppendChild(steamNode);
            computersCount = ConsoleInputIntReader("computers(users)");

            while (enteredComputers <= computersCount)
            {
                int enteredAppsCount = 1;
                XmlNode computerNode = doc.CreateElement("computer");
                steamNode.AppendChild(computerNode);

                computerName = ConsoleInputStringReader("computer name (username)", "computer(user)", enteredComputers, false);
                steamLogin = ConsoleInputStringReader("steam login name", "computer(user)", enteredComputers, false);
                steamLogin = EncryptStringAES(steamLogin, encryptionPassword);
                steamPassword = ConsoleInputStringReader("steam password", "computer(user)", enteredComputers, false);
                steamPassword = EncryptStringAES(steamPassword, encryptionPassword);

                XmlNodeWithText(doc, computerNode, "computername", computerName);
                XmlNodeWithText(doc, computerNode, "steamlogin", steamLogin);
                XmlNodeWithText(doc, computerNode, "steampassword", steamPassword);


                XmlNode appsNode = doc.CreateElement("apps");
                if (isGlobalAppsDefined)
                {
                    for (int i = 0; i < globalApps; i++)
                    {
                        foreach (KeyValuePair<string, List<string>> keyValuePair in globalAppsDictionary)
                        {
                            DictionaryFiller(dictionary, keyValuePair.Key, keyValuePair.Value[i]);
                        }
                        XmlNodeWithAttributes(doc, appsNode, "app", dictionary);
                        DictionaryEmptier(dictionary);
                    }
                }
                appsCount = ConsoleInputIntReader("apps");
                while (enteredAppsCount <= appsCount)
                {
                    DictionaryFiller(dictionary, "id", Convert.ToString(enteredAppsCount));
                    DictionaryFiller(dictionary, "name", ConsoleInputStringReader("application name", "application", enteredAppsCount, false));
                    DictionaryFiller(dictionary, "launch", ConsoleInputStringReader("steam application number", dictionary["name"], enteredAppsCount, false));
                    DictionaryFiller(dictionary, "parameters", ConsoleInputStringReader("steam launch parameters", dictionary["name"], enteredAppsCount, true));
                    XmlNodeWithAttributes(doc, appsNode, "app", dictionary);
                    DictionaryEmptier(dictionary);
                    enteredAppsCount = enteredAppsCount + 1;
                }


                computerNode.AppendChild(appsNode);

                XmlNode recoveryNode = doc.CreateElement("recovery");
                recoveryEmail = ConsoleInputStringReader("recovery email", "computer(user)", enteredComputers, true);
                recoveryEmailPassword = ConsoleInputStringReader("recovery email password", "computer(user)", enteredComputers, true);
                XmlNodeWithText(doc, recoveryNode, "email", recoveryEmail);
                XmlNodeWithText(doc, recoveryNode, "password", recoveryEmailPassword);

                computerNode.AppendChild(recoveryNode);
                enteredComputers = enteredComputers + 1;
            }


            doc.Save(xmlPath);
            Console.WriteLine("Encrypted XML path would be saved to path.txt");
            Console.WriteLine("If you want to use it - store in app.config of SteamLauncher.exe, and set value of IsXMLPathEncrypted to true");
            Console.WriteLine("Press any key to continue.");
            FileStream fs = new FileStream("path.txt", FileMode.Create);
            TextWriter tmp = Console.Out;
            StreamWriter sw = new StreamWriter(fs);
            Console.SetOut(sw);
            Console.WriteLine("Encrypted XML path is: {0}", EncryptStringAES(xmlPath, encryptionPassword));
            Console.SetOut(tmp);
            Console.WriteLine("Encrypted XML path is: {0}", EncryptStringAES(xmlPath, encryptionPassword));
            sw.Close();
            Console.ReadLine();
            Environment.Exit(1);
        }

        public static XmlDocument XmlNodeWithText(XmlDocument doc, XmlNode parentNode, string nodeName, string nodeText)
        {
            XmlNode newNode = doc.CreateElement(nodeName);
            newNode.AppendChild(doc.CreateTextNode(nodeText));
            parentNode.AppendChild(newNode);
            return doc;
        }

        public static XmlDocument XmlNodeWithAttributes(XmlDocument doc, XmlNode parentNode, string nodeName, Dictionary<string, string> attributes)
        {
            XmlNode newNode = doc.CreateElement(nodeName);
            foreach (KeyValuePair<string, string> attribute in attributes)
            {
                XmlAttribute newAttribute = doc.CreateAttribute(attribute.Key);
                newAttribute.Value = attribute.Value;
                newNode.Attributes.Append(newAttribute);
            }
            parentNode.AppendChild(newNode);
            return doc;
        }

        public static Dictionary<string, string> DictionaryFiller(Dictionary<string, string> dictionary, string key, string value)
        {
            dictionary.Add(key, value);
            return dictionary;
        }

        public static Dictionary<string, string> DictionaryEmptier(Dictionary<string, string> dictionary)
        {
            dictionary.Clear();
            return dictionary;
        }

        public static string ConsoleInputStringReader(string firstMessageParameter, string secondMesaageparamer, int enteredComputers, bool allowEmptyStrings)
        {
            Console.WriteLine("Enter {0} for {1} {2}:", firstMessageParameter, secondMesaageparamer, enteredComputers);
            string inputString = Console.ReadLine();
            if (allowEmptyStrings)
            {
                return inputString;
            }
            if (inputString == string.Empty)
            {
                Console.WriteLine("Tou've entered empty {0}, please retry", firstMessageParameter);
                inputString = ConsoleInputStringReader(firstMessageParameter, secondMesaageparamer, enteredComputers, false);
            }

            return inputString;
        }

        public static int ConsoleInputIntReader(string firstMessageParameter)
        {
            int inputInt;
            Console.WriteLine("How much {0} you want to create (number please):", firstMessageParameter);
            try
            {
                inputInt = Convert.ToInt32(Console.ReadLine());
            }
            catch (Exception)
            {
                Console.WriteLine("You've entered not number");
                inputInt = ConsoleInputIntReader("computers(users)");
            }
            return inputInt;
        }

        public static Dictionary<string, List<string>> GlobalAppsDicitonaryFiller(Dictionary<string, List<string>> global)
        {
            List<string> ids = new List<string>();
            List<string> names = new List<string>();
            List<string> steamLaunchNumber = new List<string>();
            List<string> steamLaunchParameters = new List<string>();
            for (int i = 1; i <= globalApps; i++)
            {
                ids.Add("global" + Convert.ToString(i));
                names.Add(ConsoleInputStringReader("enter global app name", "for global app", i, false));
                steamLaunchNumber.Add(ConsoleInputStringReader("enter global app steam launch number", "global app " + names[i - 1], i, false));
                steamLaunchParameters.Add(ConsoleInputStringReader("enter global app steam launch parameters", "global app " + names[i - 1], i, true));
            }
            global.Add("id", ids);
            global.Add("name", names);
            global.Add("launch", steamLaunchNumber);
            global.Add("parameters", steamLaunchParameters);
            return global;
        }

        public static void GlobalAppsDefiner()
        {
            globalApps = ConsoleInputIntReader("global apps for all computers(users)");
            if (globalApps != 0)
            {
                isGlobalAppsDefined = true;
                GlobalAppsDicitonaryFiller(globalAppsDictionary);
            }
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

