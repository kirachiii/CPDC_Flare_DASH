using System; 
using System.IO; 

namespace CPDC_Flare_DASH.Models
{
    class Log
    {
        private static string LogFileDir;
        private static string LogAQIFilePath;
        private static string LogErrorFilePath;
        private static string LogErrorDetailedFilePath;
        public static DateTime DateNow = DateTime.Now.Date;
        public static bool isDateChange = false;

        public static void LogFileCreate()
        {
            var DateTimeYear = DateTime.Now.Year;
            var DateTimeMonth = DateTime.Now.Month;
            var DateTimeDay = DateTime.Now.Day;
            LogFileDir = Environment.CurrentDirectory + @"\Log\" + DateTimeYear + @"\" + DateTimeMonth;
            LogAQIFilePath = LogFileDir + @"\" + DateTimeDay + @"_CPDC.log";
            LogErrorFilePath = LogFileDir + @"\" + DateTimeDay + @"_Error.log";
            LogErrorDetailedFilePath = LogFileDir + @"\" + DateTimeDay + @"_ErrorDetailed.log";
            try
            {
                if (!File.Exists(LogAQIFilePath))
                {
                    if (!Directory.Exists(LogFileDir)) { Directory.CreateDirectory(LogFileDir); }
                    using (FileStream Fs = File.Create(LogAQIFilePath))
                    {
                        return;
                    }
                }
                if (!File.Exists(LogErrorFilePath))
                {
                    if (!Directory.Exists(LogFileDir)) { Directory.CreateDirectory(LogFileDir); }
                    using (FileStream Fs = File.Create(LogErrorFilePath))
                    {
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Log Create Error : " + e.ToString());
            }
        }

        //command 1:系統執行紀錄 99:錯誤紀錄
        public static void LogWrite(string Message, int command)
        {
            try
            {
                if (DateTime.Now.Date > DateNow)
                {
                    isDateChange = true;
                    DateNow = DateTime.Now.Date;
                    LogFileCreate();
                }
                switch (command)
                {
                    case 1:
                        using (StreamWriter file = new StreamWriter(LogAQIFilePath, true))
                        {
                            file.WriteLine(DateTime.Now + " >> " + Message);
                        }
                        break;
                    case 98:
                        using (StreamWriter file = new StreamWriter(LogErrorDetailedFilePath, true))
                        {
                            file.WriteLine(DateTime.Now + " >> " + Message);
                        }
                        break;
                    case 99:
                        using (StreamWriter file = new StreamWriter(LogErrorFilePath, true))
                        {
                            file.WriteLine(DateTime.Now + " >> " + Message);
                        }
                        break;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Log Error : " + e.ToString());
            }
        }
    }
}
