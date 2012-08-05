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
            XmlDocument doc = new XmlDocument();
            XmlNode xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xmlDeclaration);
            
            XmlNode steamNode = doc.CreateElement("steam");
            doc.AppendChild(steamNode);

            XmlNode computerNode = doc.CreateElement("computer");
            steamNode.AppendChild(computerNode);

            XmlNode computerNameNode = doc.CreateElement("computername");
            computerNameNode.AppendChild(doc.CreateTextNode("test"));
            computerNode.AppendChild(computerNameNode);
            XmlNode steamLoginNode = doc.CreateElement("steamlogin");
            steamLoginNode.AppendChild(doc.CreateTextNode("test"));
            computerNode.AppendChild(steamLoginNode);
            XmlNode steamPasswordNode = doc.CreateElement("steampassword");
            steamPasswordNode.AppendChild(doc.CreateTextNode("test"));
            computerNode.AppendChild(steamPasswordNode);
            
            XmlNode appsNode = doc.CreateElement("apps");
            XmlNode appNode = doc.CreateElement("app");
            XmlAttribute appIdAttribute = doc.CreateAttribute("id");
            XmlAttribute appNameAttribute = doc.CreateAttribute("name");
            XmlAttribute appLaunchAttribute = doc.CreateAttribute("launch");
            XmlAttribute appParametersAttribute = doc.CreateAttribute("parameters");
            appIdAttribute.Value = "01";
            appNameAttribute.Value = "alienswarm";
            appLaunchAttribute.Value = "630";
            appParametersAttribute.Value = "";
            appNode.Attributes.Append(appIdAttribute);
            appNode.Attributes.Append(appNameAttribute);
            appNode.Attributes.Append(appLaunchAttribute);
            appNode.Attributes.Append(appParametersAttribute);
            appsNode.AppendChild(appNode);
            computerNode.AppendChild(appsNode);

            XmlNode recoveryNode = doc.CreateElement("recovery");
            XmlNode emailNode = doc.CreateElement("email");
            emailNode.AppendChild(doc.CreateTextNode("test@te.te"));
            recoveryNode.AppendChild(emailNode);
            XmlNode emailpasswordNode = doc.CreateElement("password");
            emailpasswordNode.AppendChild(doc.CreateTextNode("test"));
            recoveryNode.AppendChild(emailpasswordNode);
            computerNode.AppendChild(recoveryNode);
            doc.Save(xmlPath);
        }
    }
}
