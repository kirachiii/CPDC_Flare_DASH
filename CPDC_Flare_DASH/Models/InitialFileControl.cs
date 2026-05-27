using System; 
using System.IO; 
using System.Runtime.InteropServices;
using System.Text; 

namespace CPDC_Flare_DASH.Models
{
    class InitialFileControl
    {
        private static string FilePath;
        private static StringBuilder lpReturnString;
        private static int BufferSize;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string lpString, string lpFileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
         
        public static void Manger(string iniPath)
        {
            FilePath = iniPath;
            BufferSize = 512;
            lpReturnString = new StringBuilder(BufferSize);
            //RemoveBom();
        }

        public static bool FileCreate(string Path, bool ForceCreate)
        {
            try
            {
                if (!File.Exists(Path) || ForceCreate)
                {
                    using (FileStream Fs = File.Create(Path))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            { 
                Log.LogWrite(e.ToString(), 99);
                return false;
            }
            return false;
        }

        public static void FileInitial()
        {
            var Path = Environment.CurrentDirectory + @"\config.ini";
            Manger(Path);
            //資料庫
            FileWrite("SQLConnect", "SqlConnServerIP", "127.0.0.1");
            FileWrite("SQLConnect", "SqlConnServerPort", "1433");
            FileWrite("SQLConnect", "SqlConnServerInstance", "");
            FileWrite("SQLConnect", "SqlConnUserID", "sa");
            FileWrite("SQLConnect", "SqlConnUserPWD", "gjWzHwCnA/5gl2cSMT4M4A==");
            //CPDC資訊
            FileWrite("CPDC", "SqlDataCatelog", "CPDC");
            FileWrite("CPDC", "Status", "10");
            //申報密碼
            FileWrite("Declare", "DeclarePWD", "gjWzHwCnA/5gl2cSMT4M4A==");
            FileWrite("Declare", "DeclareFlareNo", "");
            FileWrite("Declare", "DeclarePolNo", "");
            FileWrite("Declare", "DeclareCNo", "");
            FileWrite("Declare", "DeclareFile", Environment.CurrentDirectory);
            FileWrite("Declare", "DeclareItem1", "Checked,C1");//C1
            FileWrite("Declare", "DeclareItem2", "Checked,C2");//C2
            FileWrite("Declare", "DeclareItem3", "Checked,C3");//C3
            FileWrite("Declare", "DeclareItem4", "Checked,C4");//C4
            FileWrite("Declare", "DeclareItem5", "Checked,C5+");//C5+
            FileWrite("Declare", "DeclareItem6", "Checked,THC");//THC
            FileWrite("Declare", "DeclareItem7", "Checked,HCN");//HCN
            FileWrite("Declare", "DeclareItem8", "Checked,C3H8");//丙烷
            FileWrite("Declare", "DeclareItem9", "Checked,C3H6");//丙烯 
            FileWrite("Declare", "DeclareItem10", "Checked,HCN+");//HCN+
            FileWrite("Declare", "DeclareItem11", "Checked,ACN");//乙腈
            FileWrite("Declare", "DeclareItem12", "Checked,AN");//丙烯睛 
            FileWrite("Declare", "DeclareItem13", "Checked,WasteFlow");//廢棄流量
            FileWrite("Declare", "DeclareItem14", "Checked,WasteTemp");//廢棄溫度
            FileWrite("Declare", "DeclareItem15", "Checked,WaterA");//水封槽液位
            FileWrite("Declare", "DeclareItem16", "Checked,WaterB");//水封槽壓力
            FileWrite("Declare", "DeclareItem17", "Checked,PilotLight1");//母火溫度1
            FileWrite("Declare", "DeclareItem18", "Checked,PilotLight2");//母火溫度2
            FileWrite("Declare", "DeclareItem19", "Checked,PilotLight3");//母火溫度3
            //Comport 廢棄
            FileWrite("COMWaste", "Comport", "COM1");
            FileWrite("COMWaste", "Baud", "9600");
            //Comport DCS
            FileWrite("COMDCS", "Comport", "");
            FileWrite("COMDCS", "Baud", "9600");
            //Comport 氣體
            FileWrite("COMGas", "Comport", "");
            FileWrite("COMGas", "Baud", "9600");
            //Comport 校正
            FileWrite("COMRegulate", "Comport", "");
            FileWrite("COMRegulate", "Baud", "9600");
            //讀取檔案
            FileWrite("File", "FileAddress", @"C:\");
            FileWrite("File", "VOCItem", "");
            FileWrite("File", "THCItem", "");
            //範圍設定
            FileWrite("Range", "HCN", "0~100");
            FileWrite("Range", "O2", "0~30");
            FileWrite("Range", "H2", "0~30");
            FileWrite("Range", "Temp", "0~0");
            FileWrite("Range", "LNGA", "0~70");
            FileWrite("Range", "LNGB", "0~70"); 
        }

        public static string FileRead(string Section, string Key, string DefaultValue)
        {
            lpReturnString.Clear();
            GetPrivateProfileString(Section, Key, DefaultValue, lpReturnString, BufferSize, FilePath);
            return lpReturnString.ToString();
        }

        public static void FileWrite(string Section, string Key, object Value)
        {
            WritePrivateProfileString(Section, Key, Value.ToString(), FilePath);
        }
        
        //移除空格
        public static void RemoveBom()
        {
            string InitialConfig = System.IO.File.ReadAllText(FilePath);
            InitialConfig.Trim(new char[] { '\uFEFF' });
            Encoding utf8WithoutBom = new UTF8Encoding(false);
            System.IO.File.WriteAllText(FilePath, InitialConfig, utf8WithoutBom);
        }
    }
}
