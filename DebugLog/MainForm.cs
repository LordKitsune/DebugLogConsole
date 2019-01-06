using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Kitsu.DebugLog
{
    public partial class MainForm : Form
    {
        public static TextReader reader;
        public Font boldFont;
        private Thread listener;
        const int WM_USER = 0x400;
        const int EM_HIDESELECTION = WM_USER + 63;


        public MainForm()
        {
            InitializeComponent();
            FormClosing += MainForm_FormClosing;
            Process[] games = Process.GetProcessesByName("Subnautica");
            if (games.Length != 0)
            {
                Process subnautica = games.First();
                subnautica.EnableRaisingEvents = true;
                subnautica.Exited += ((sender, data) =>
                {
                    Environment.Exit(0);
                });
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (listener != null)
                listener.Abort();
        }

        public static void Invoke(Control ctrl, Action action)
        {
            if (ctrl.InvokeRequired)
                ctrl.Invoke(action);
            else
                action();
        }

        private void SetText(string text)
        {
            try
            {
                int errorCode = 0;
                if (int.TryParse(text.Remove(1), out errorCode))
                    text = text.Substring(1);
                string prefix = string.Empty;
                Color logColor = Color.White;
                if (errorCode == 0)
                {
                    prefix = "[LOG]";
                }
                else if (errorCode == 1)
                {
                    prefix = "[WARNING]";
                    logColor = Color.Yellow;
                }
                else if (errorCode == 2)
                {
                    prefix = "[ERROR]";
                    logColor = Color.Red;
                }
                int start = logBox.TextLength;
                logBox.AppendText(prefix + text + Environment.NewLine);
                int end = logBox.TextLength;
                if (errorCode == 2)
                    logBox.SelectionFont = boldFont;
                logBox.Select(start, end - start);
                if (errorCode == 2)
                    logBox.SelectionFont = logBox.Font;
                logBox.SelectionColor = logColor;
                logBox.SelectionLength = 0;
            }
            catch(Exception ex)
            {
                int start = logBox.TextLength;
                logBox.AppendText("[ConsoleException]" + ex.ToString() + Environment.NewLine);
                int end = logBox.TextLength;
                logBox.Select(start, end - start);
                logBox.SelectionColor = Color.DarkRed;
                logBox.SelectionLength = 0;
            }
            
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            listener = new Thread(async() => 
            {
                while (true)
                {
                    await reader.ReadLineAsync();
                    string text = await reader.ReadLineAsync();
                    Invoke(logBox, () => 
                    {
                        SetText(text);
                    });
                }
            });
            listener.Start();
        }
    }
}
