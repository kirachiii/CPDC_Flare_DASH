using CPDC_Flare_DASH.Models;
using System;
using System.Threading;
using System.Windows.Forms;

namespace CPDC_Flare_DASH
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false); 
             
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.Run(new DataColletionSystem());
        }
        
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            Log.LogWrite("ThreadException錯誤", 99);
            Log.LogWrite("ThreadException錯誤" + e.Exception.InnerException.ToString(), 98);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            Log.LogWrite("UnhandledException錯誤", 99);
            Log.LogWrite("UnhandledException錯誤" + e.ExceptionObject.ToString(), 98);
            Environment.Exit(e.GetHashCode());
        }
    }
}
