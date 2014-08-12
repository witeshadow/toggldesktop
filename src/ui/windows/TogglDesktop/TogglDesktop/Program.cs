﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace TogglDesktop
{
    static class Program
    {
        public static bool ShuttingDown = false;
        private const string appGUID = "29067F3B-F706-46CB-92D2-1EA1E72A4CE3";
        public static Bugsnag.Library.BugSnag bugsnag = null;
        private static UInt64 uid = 0;
        private static MainWindowController mainWindowController;

        [DllImport("User32")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("User32")]
        private static extern bool ShowWindow(IntPtr hwnd, int cmdshow);
        private const int SW_RESTORE = 9;

        public static bool IsLoggedIn
        {
            get
            {
                return uid > 0;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Mutex mutex = new Mutex(false, "Global\\" + appGUID))
            {
                if (!mutex.WaitOne(0, false))
                {
                    // See if we get hold of the other process.
                    // If we do, activate it's window and exit.
                    Process current = Process.GetCurrentProcess();
                    Process[] instances = Process.GetProcessesByName(current.ProcessName);
                    foreach (Process p in instances)
                    {
                        if (p.Id != current.Id)
                        {
                            // gotcha
                            ShowWindow(p.MainWindowHandle, SW_RESTORE);
                            SetForegroundWindow(p.MainWindowHandle);
                            return;
                        }
                    }

                    // If not, print an error message and exit.
                    MessageBox.Show("Another copy of Toggl Desktop is already running." +
                        Environment.NewLine + "This copy will now quit.");
                    return;
                }

                 if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();

                bugsnag = new Bugsnag.Library.BugSnag()
                {
                    apiKey = "2a46aa1157256f759053289f2d687c2f",
                    OSVersion = Environment.OSVersion.ToString()
                };

                Toggl.OnLogin += delegate(bool open, UInt64 user_id)
                {
                    uid = user_id;
                };

                Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                mainWindowController = new MainWindowController();
                Application.Run(mainWindowController);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            bugsnag.Notify(e.ExceptionObject as Exception, new
            {
                UserID = uid.ToString()
            });
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            bugsnag.Notify(e.Exception, new
            {
                UserID = uid.ToString()
            });
        }

        public static void Shutdown(int exitCode)
        {
            ShuttingDown = true;

            if (mainWindowController != null)
            {
                mainWindowController.RemoveTrayIcon();
            }

            Toggl.Clear();
            
            Environment.Exit(exitCode);
        }

        public static string Version()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return string.Format("{0}.{1}.{2}",
                versionInfo.ProductMajorPart,
                versionInfo.ProductMinorPart,
                versionInfo.ProductBuildPart);
        }
    }
}
