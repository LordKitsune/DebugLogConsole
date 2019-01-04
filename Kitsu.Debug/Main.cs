using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using UnityEngine;
namespace Kitsu.Debug
{
    public static class Main
    {
        public static Process debugLog;
        public static void Load()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\DebugLog.exe";
            ProcessStartInfo p = new ProcessStartInfo(path);
            p.UseShellExecute = false;
            p.RedirectStandardInput = true;
            debugLog = Process.Start(p);
            debugLog.StandardInput.AutoFlush = true;
            Application.logMessageReceivedThreaded += (log, stacktrace, type) =>
            {
                int code = 0;
                if (type == LogType.Warning)
                    code = 1;
                else if (type == LogType.Error || type == LogType.Exception)
                    code = 2;
                if (code == 2)
                    log += string.Format(" At Stacktrace:{0}", stacktrace);
                debugLog.StandardInput.WriteLine(code + log);
                KitsuLog.Log(log, type);
            };
            Harmony.HarmonyInstance.Create("Kitsu.Debug.DebugLog").PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
