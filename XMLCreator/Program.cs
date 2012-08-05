using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;

namespace XMLCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            string xmlPath = ConfigurationManager.AppSettings["XML Paths"].ToLower();
            GenerateXML(xmlPath);
        }

        public static void GenerateXML(string xmlPath)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            int computersCount = 0;
            int enteredComputers = 1;
            string computerName = string.Empty;
            string steamLogin = string.Empty;
            string steamPassword = string.Empty;
            XmlDocument doc = new XmlDocument();
            XmlNode xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xmlDeclaration);
            
            XmlNode steamNode = doc.CreateElement("steam");
            doc.AppendChild(steamNode);
            Console.WriteLine("How much computers you want to create (number please):");
            string computersCountString = Console.ReadLine();
            try
            {
                computersCount = Convert.ToInt32(computersCountString);
            }
            catch (Exception)
            {
                Console.WriteLine("You have not entered number");
                Environment.Exit(-1);
            }

            while(enteredComputers <= computersCount)
            {
                XmlNode computerNode = doc.CreateElement("computer");
                steamNode.AppendChild(computerNode);

                Console.WriteLine("Enter computer name for computer {0}:", enteredComputers);
                computerName = Console.ReadLine();
                if(computerName == string.Empty)
                {
                    Console.WriteLine("Tou've entered empty computername, please start over");
                    Environment.Exit(-1);
                }
                Console.WriteLine("Enter steam login name for computer {0}:", enteredComputers);
                steamLogin = Console.ReadLine();
                if (steamLogin == string.Empty)
                {
                    Console.WriteLine("Tou've entered empty steam login, please start over");
                    Environment.Exit(-1);
                }
                Console.WriteLine("Enter steam password for computer {0}", enteredComputers);
                steamPassword = Console.ReadLine();
                if (steamPassword == string.Empty)
                {
                    Console.WriteLine("Tou've entered empty steam password, please start over");
                    Environment.Exit(-1);
                }

                XmlNodeWithText(doc, computerNode, "computername", computerName);
                XmlNodeWithText(doc, computerNode, "steamlogin", steamLogin);
                XmlNodeWithText(doc, computerNode, "steampassword", steamPassword);


                XmlNode appsNode = doc.CreateElement("apps");

                DictionaryFiller(dictionary, "id", "01");
                DictionaryFiller(dictionary, "name", "alienswarm");
                DictionaryFiller(dictionary, "launch", "630");
                DictionaryFiller(dictionary, "parameters", "");

                XmlNodeWithAttributes(doc, appsNode, "app", dictionary);
                DictionaryEmptier(dictionary);

                computerNode.AppendChild(appsNode);

                XmlNode recoveryNode = doc.CreateElement("recovery");
                XmlNodeWithText(doc, recoveryNode, "email", "test@te.te");
                XmlNodeWithText(doc, recoveryNode, "password", "test@te.te");

                computerNode.AppendChild(recoveryNode);
                enteredComputers = enteredComputers + 1;
            }


            doc.Save(xmlPath);
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

        public static Dictionary<string, string> DictionaryFiller (Dictionary<string, string> dictionary, string key, string value)
        {
            dictionary.Add(key, value);
            return dictionary;
        }

        public static Dictionary<string, string> DictionaryEmptier (Dictionary<string, string> dictionary)
        {
            dictionary.Clear();
            return dictionary;
        }
    }
}
