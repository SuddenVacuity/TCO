using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace TCO
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool isGoogleSetup = TCOSheetsInterface.Init();

            InputHook.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (isGoogleSetup == false)
            {
                DialogMessage message = new DialogMessage("Google Sheets Error", "An error occured initializing Google Sheets service.\n\nLogs will not be written to Google Sheets.");
                message.ShowDialog();
            }

            Application.Run(new Form1());

            InputHook.End();
        }
    }
}

