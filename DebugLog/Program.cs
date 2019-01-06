using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;


namespace Kitsu.DebugLog
{
    public class Program
    {

        static string lastString = string.Empty;
        [STAThread]
        public static void Main(string[] args)
        {
            MainForm.reader = Console.In;
            Application.Run(new MainForm());
            /*Process[] games = Process.GetProcessesByName("Subnautica");
            Process subnautica = games.First();
            subnautica.EnableRaisingEvents = true;
            subnautica.Exited += ((sender, data) =>
            {
                Environment.Exit(0);
            });
            Console.Title = "Unity Debug Log";
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadLine();
            Console.WriteLine("==Kitsune's Runtime Unity Debug Log==");
            while (true)
            {
                try
                {
                    string line = Console.ReadLine();
                    lastString = line;
                    string prefix = "[LOG]";
                    if (line.Length > 1)
                    {
                        int errorCode = 0;
                        if (!int.TryParse(line.Remove(1), out errorCode))
                        {
                            errorCode = 0;
                        }
                        if (errorCode == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else if (errorCode == 1)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            prefix = "[WARNING]";
                        }
                        else if (errorCode == 2)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            prefix = "[ERROR]";
                        }

                        Console.WriteLine(prefix + line.Substring(1));
                    }
                }
                catch
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("[LogError]Unable to parse Log");
                }
            }*/
        }
    }
}
