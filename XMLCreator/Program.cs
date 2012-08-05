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
            
            while(enteredComputers <= computersCount)
            {
                int enteredAppsCount = 1;
                XmlNode computerNode = doc.CreateElement("computer");
                steamNode.AppendChild(computerNode);

                computerName = ConsoleInputStringReader("computer name (username)","computer(user)", enteredComputers);
                steamLogin = ConsoleInputStringReader("steam login name", "computer(user)", enteredComputers);
                steamPassword = ConsoleInputStringReader("steam password", "computer(user)", enteredComputers);

                XmlNodeWithText(doc, computerNode, "computername", computerName);
                XmlNodeWithText(doc, computerNode, "steamlogin", steamLogin);
                XmlNodeWithText(doc, computerNode, "steampassword", steamPassword);


                XmlNode appsNode = doc.CreateElement("apps");

                appsCount = ConsoleInputIntReader("apps");
                while (enteredAppsCount<=appsCount)
                {
                    DictionaryFiller(dictionary, "id", Convert.ToString(enteredAppsCount));
                    DictionaryFiller(dictionary, "name", ConsoleInputStringReader("application name", "application", enteredAppsCount));
                    DictionaryFiller(dictionary, "launch", ConsoleInputStringReader("steam application number", dictionary["name"], enteredAppsCount));
                    DictionaryFiller(dictionary, "parameters", ConsoleInputStringReader("steam launch parameters", dictionary["name"], enteredAppsCount));
                    XmlNodeWithAttributes(doc, appsNode, "app", dictionary);
                    DictionaryEmptier(dictionary);
                    enteredAppsCount = enteredAppsCount + 1;
                }


                computerNode.AppendChild(appsNode);

                XmlNode recoveryNode = doc.CreateElement("recovery");
                recoveryEmail = ConsoleInputStringReader("recovery email", "computer(user)", enteredComputers);
                recoveryEmailPassword = ConsoleInputStringReader("recovery email password", "computer(user)", enteredComputers);
                XmlNodeWithText(doc, recoveryNode, "email", recoveryEmail);
                XmlNodeWithText(doc, recoveryNode, "password", recoveryEmailPassword);

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

        public static string ConsoleInputStringReader(string firstMessageParameter, string secondMesaageparamer, int enteredComputers)
        {
            Console.WriteLine("Enter {0} for {1} {2}:",firstMessageParameter, secondMesaageparamer, enteredComputers);
            string inputString = Console.ReadLine();
            if (inputString == string.Empty)
            {
                Console.WriteLine("Tou've entered empty {0}, please retry", firstMessageParameter);
                inputString = ConsoleInputStringReader(firstMessageParameter, secondMesaageparamer, enteredComputers);
            }

            return inputString.ToLower();
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
    }
}
