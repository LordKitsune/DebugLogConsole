using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using UnityEngine;
using System.Xml;
namespace Kitsu.Debug
{
    public static class Main
    {
        public static Process debugLog;
        public static Dictionary<string, bool> Config = new Dictionary<string, bool>();
        public static void Load()
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!File.Exists(dir + "\\settings.xml"))
            {
                XmlDocument xml = new XmlDocument();
                XmlElement element = xml.CreateElement("settings");
                xml.AppendChild(element);
                XmlNode root = xml.FirstChild;
                element = xml.CreateElement("AutoError");
                element.InnerText = "True";
                root.AppendChild(element);
                element = xml.CreateElement("EnableExternalConsole");
                element.InnerText = "True";
                root.AppendChild(element);
                element = xml.CreateElement("DraggableLog");
                element.InnerText = "True";
                root.AppendChild(element);
                element = xml.CreateElement("ResizableLog");
                element.InnerText = "False";
                root.AppendChild(element);
                element = xml.CreateElement("AutoScroll");
                element.InnerText = "True";
                root.AppendChild(element);
                xml.Save(dir + "\\settings.xml");
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(dir + "\\settings.xml");
            foreach(XmlNode node in doc.FirstChild)
            {
                Config.Add(node.Name, Convert.ToBoolean(node.InnerText));
            }
            string path = dir + "\\DebugLog.exe";
            if (Config["EnableExternalConsole"])
            {
                ProcessStartInfo p = new ProcessStartInfo(path);
                p.UseShellExecute = false;
                p.RedirectStandardInput = true;
                debugLog = Process.Start(p);
                debugLog.StandardInput.AutoFlush = true;
            }
            Application.logMessageReceivedThreaded += (log, stacktrace, type) =>
            {
                int code = 0;
                if (type == LogType.Warning)
                    code = 1;
                else if (type == LogType.Error || type == LogType.Exception)
                    code = 2;
                if (code == 2)
                    log += string.Format(" At Stacktrace:{0}", stacktrace);
                if (Config["EnableExternalConsole"])  
                    debugLog.StandardInput.WriteLine(code + log);
                KitsuLog.Log(log, type);
            };
            Harmony.HarmonyInstance.Create("Kitsu.Debug.DebugLog").PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
