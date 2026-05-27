using CPDC_Flare_DASH.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CPDC_Flare_DASH
{
    public partial class SystemInfo : Form
    {
        //紀錄訊息狀態
        public bool WasteStatus;
        public bool GasStatus;
        public bool DCSStatus;
        public bool RegulateStatus;
        public string ErrorCode = "";
        private static string LogErrorFilePath;
        private static string LogFileDir;

        public SystemInfo()
        {
            InitializeComponent();
        }

        private void SystemInfo_Load(object sender, EventArgs e)
        {
            var DateTimeYear = DateTime.Now.Year;
            var DateTimeMonth = DateTime.Now.Month;
            var DateTimeDay = DateTime.Now.Day;
            LogFileDir = Environment.CurrentDirectory + @"\Log\" + DateTimeYear + @"\" + DateTimeMonth;
            LogErrorFilePath = LogFileDir + @"\" + DateTimeDay + @"_Error.log";

            listBox_Info.Items.Clear();
            listBox_Info.Items.Add("-------------連線資訊----------------------");
            listBox_Info.Items.Add(string.Format("氣體偵測連線 : {0}", CheckStatus(GasStatus)));
            listBox_Info.Items.Add(string.Format("廢棄偵測連線 : {0}", CheckStatus(WasteStatus)));
            listBox_Info.Items.Add(string.Format("DCS 偵測連線 : {0}", CheckStatus(DCSStatus)));
            listBox_Info.Items.Add(string.Format("自動校正連線 : {0}", CheckStatus(RegulateStatus)));
            listBox_Info.Items.Add(string.Format("SQL資料 連線 : {0}", CheckStatus(GlobalVariable.SQL.Status)));
            listBox_Info.Items.Add("");
            if (ErrorCode != "異常")
            {
                listBox_Info.Items.Add("-----------------數據------------------------");
                listBox_Info.Items.Add(string.Format("ErrorCode    : {0}", ErrorCode));
            }

            string line;
            using (System.IO.StreamReader file =
                new System.IO.StreamReader(LogErrorFilePath, true))
            { 
                listBox_Info.Items.Add("-------------異常訊息----------------------");
                while ((line = file.ReadLine()) != null)
                {
                    listBox_Info.Items.Add(line);
                }
                file.Close();
            } 
        }

        private string CheckStatus(bool status)
        {
            if (status)
                return "正常";
            else
                return "異常";
        }
    }
}
