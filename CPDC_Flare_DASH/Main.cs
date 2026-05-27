using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CPDC_Flare_DASH.Models;

namespace CPDC_Flare_DASH
{
    public partial class DataColletionSystem : Form
    {
        public bool IsFileNewCreate = false;
        //console 參數 
        public static DataColletionSystem form1 = null;
        //宣告 function
        ModbusSLV slave = new ModbusSLV();
        ModbusRTU master = new ModbusRTU();
        ReadFile readFile = new ReadFile();
        SQLControl SQLControl = new SQLControl();
        GetHttpData _mGetHttpData;
        SerialPortConnect serial = new SerialPortConnect();
        SerialPortConnect serialRegulate = new SerialPortConnect();
        BackgroundWorker backgroundworker;
        //宣告 Comport  與 地址
        int BaudWaste, BaudGas, BaudDCS, BaudRegulate , BaudFire;
        string ComWaste, ComGas, ComDCS, ComRegulate , ComFire;
        //LNG累積
        double LNGFlowASummaryLast, LNGFlowASummaryNow, LNGFlowBSummaryLast, LNGFlowBSummaryNow;
        //熄火狀態(目前廢置不用)
        private bool _NMHCFlameOutLastState;
        private bool _VOCFlameOutLastState;
        private bool _NMHCFlameOutAlarmAcked;
        private bool _VOCFlameOutAlarmAcked;
        //紀錄訊息狀態
        private bool SQLstatus = false;
        private bool WasteStatus = false;
        private bool GasStatus = false;
        private bool DCSStatus = false;
        private bool RegulateStatus = false;
        private string ErrorCode = "0";
        private int timer_count = 0;
        //紀錄slave connect狀態
        private string MC1 = "", MC2 = "", MC3 = "";
        private int MC_Count = 0;

        AppDomain currentDomain = AppDomain.CurrentDomain;

        double[] GDValues = new double[6];
        int[] GDValueErrors = new int[6];
        private static string address;

        string[] GasArray = { "H2", "O2", "HCN", "Temp", "LNGA", "LNGB" };

        //新增母火
        private bool FireStatus = false;
        SerialPortConnect Fire_serial = new SerialPortConnect();
        string[] FireArray = { "Light1", "Light2", "Light3" };

        public DataColletionSystem()
        {
            InitializeComponent();
            IsFileNewCreate = InitialFileControl.FileCreate(Environment.CurrentDirectory + @"\config.ini", false);
            form1 = this;

            //Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        //視窗關閉
        private void Main_FormClosing(object sender, EventArgs e)
        {
            timer_Refresh.Stop();
            master.Close_Modbus();
            serial.CloseComport();
            serialRegulate.CloseComport();
            slave.Close_Modbus();
            Environment.Exit(Environment.ExitCode);
            this.Dispose();
            this.Close();
        }

        //設定物件
        public static void SetControlInfo(string ControlName, string ControlValue)
        {
            try
            {
                if (ControlName != null && ControlValue != null)
                {
                    var control = form1.Controls.OfType<Label>().FirstOrDefault(c => c.Name == ControlName);
                    control.Invoke((MethodInvoker)(() => control.Text = ControlValue));
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("設定物件" + ControlName, 99);
            }
        }

        public static void Regulate_Insert()
        {
            try
            {
                if (!form1.SQLstatus)
                {
                    form1.Range1Zero_Value.Text = (0.03 + 0.03 * double.Parse(form1.Range1Zero_Value.Text)).ToString("#0.0000");
                    form1.Range1Full_Value.Text = (0.3 + 0.3 * double.Parse(form1.Range1Full_Value.Text)).ToString("#0.0000");
                    form1.Range2Zero_Value.Text = (0.3 + 0.3 * double.Parse(form1.Range2Zero_Value.Text)).ToString("#0.0000");
                    form1.Range2Full_Value.Text = (50 + 50 * double.Parse(form1.Range2Full_Value.Text)).ToString("#0.0000");
                    form1.SQLControl.InsertRegulateDate(form1.Range1Zero_Value.Text, form1.Range2Zero_Value.Text, form1.Range1Full_Value.Text, form1.Range2Full_Value.Text);
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("自動校正上傳" + ex, 99);
            }
        }

        //Log寫入(跨class呼叫用)
        public static void ListBoxAQILogWrite(string Message)
        {
            try
            {
                if (form1 != null)
                {
                    form1.AQILogWrite(Message);
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("Log寫入(跨class呼叫用)", 99);
            }

        }

        //Log寫入
        private void AQILogWrite(object Message)
        {
            Log.LogWrite(Message.ToString(), 1);
            if (Log.isDateChange) //換日後新增log檔
            {
                Log.isDateChange = false;
            }
        }

        //氣體偵測範圍設定
        private void GasValueTile_Click(object sender, EventArgs e)
        {
            RangeSetting rangesetting = new RangeSetting();//產生Setting的物件
            rangesetting.ShowDialog(this);//設定產生Setting的物件為Main的上層，並開啟Setting的物件視窗。由於在Main的程式碼內使用this，所以this為Main的物件本身
        }

        private void 系統設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //暫停連線 
                timer_Refresh.Stop();
                master.Close_Modbus();
                serial.CloseComport();
                serialRegulate.CloseComport();
                //slave.Close_Modbus();
                

                Setting setting = new Setting();//產生Setting的物件
                setting.ShowDialog(this);//設定產生Setting的物件為Main的上層，並開啟Setting的物件視窗。由於在Main的程式碼內使用this，所以this為Main的物件本身

                //視窗關閉觸發,連線恢復並更新
                if (setting.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    if (!timer_Refresh.Enabled)
                        timer_Refresh.Enabled = true;

                    ReconnectAll();
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite("Main-SystemSetting錯誤", 99);
                Log.LogWrite("Main-SystemSetting錯誤" + ex.ToString(), 98);
            }

        }

        //20210925 設定狀態碼頁面
        private void 狀態設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //暫停連線 
                timer_Refresh.Stop();
                /*
                master.Close_Modbus();
                serial.CloseComport();
                serialRegulate.CloseComport();
                slave.Close_Modbus();
                */

                StatusSetting status_setting = new StatusSetting();//產生Setting的物件
                status_setting.ShowDialog(this);//設定產生Setting的物件為Main的上層，並開啟Setting的物件視窗。由於在Main的程式碼內使用this，所以this為Main的物件本身

                //
                //視窗關閉觸發,連線恢復並更新
                if (status_setting.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    if (!timer_Refresh.Enabled)
                        timer_Refresh.Enabled = true;

                    //如果改成XX20 校正中 欄位會變紅色
                    if(InitialFileControl.FileRead("CPDC", "GC1_Status", "null").Substring(2,2) == "20")
                    {
                        lblC1.BackColor = Color.Red;
                        lblC2.BackColor = Color.Red;
                        lblC3.BackColor = Color.Red;
                        lblC4.BackColor = Color.Red;
                        lblC5.BackColor = Color.Red;
                        lblTHC.BackColor = Color.Red;
                    }
                    else if(InitialFileControl.FileRead("CPDC", "GC1_Status", "null").Substring(2, 2) == "10")
                    {
                        lblC1.BackColor = Color.FromArgb(255,200,200,200);
                        lblC2.BackColor = Color.FromArgb(255, 200, 200, 200);
                        lblC3.BackColor = Color.FromArgb(255, 200, 200, 200);
                        lblC4.BackColor = Color.FromArgb(255, 200, 200, 200);
                        lblC5.BackColor = Color.FromArgb(255, 200, 200, 200);
                        lblTHC.BackColor = Color.FromArgb(255, 200, 200, 200);
                    }

                    if (InitialFileControl.FileRead("CPDC", "GC2_Status", "null").Substring(2, 2) == "20")
                    {
                        lblHCN.BackColor = Color.Red;
                        lblC3H6.BackColor = Color.Red;
                        lblACN.BackColor = Color.Red;
                        lblAN.BackColor = Color.Red;

                    }
                    else if (InitialFileControl.FileRead("CPDC", "GC2_Status", "null").Substring(2, 2) == "10")
                    {
                        lblHCN.BackColor = Color.FromArgb(255, 200, 200, 200);
                        lblC3H6.BackColor = Color.FromArgb(255, 200, 200, 200);
                        lblACN.BackColor = Color.FromArgb(255, 200, 200, 200);
                        lblAN.BackColor = Color.FromArgb(255, 200, 200, 200);
                    }

                    /*
                    ReconnectAll();
                    */
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite("Main-StatusSetting錯誤", 99);
                Log.LogWrite("Main-StatusSetting錯誤" + ex.ToString(), 98);
            }
        }


        //刷新數值
        public void ReconnectAll()
        {
            try
            {
                InitialFileReload();

                WasteStatus = master.Console_ModbusRtu(ComWaste, BaudWaste);

                //GAS + 母火
                GasStatus = serial.Console_Connect(ComGas, BaudGas);

                DCSStatus = slave.Consolo_ModbusSlave(ComDCS, BaudDCS);
                RegulateStatus = serialRegulate.Console_Connect(ComRegulate, BaudRegulate);

                slave.DataUpdated += OnModbusDataUpdated;

                readFile.ReadFileInitial(address, "VOC");
                readFile.ReadFileInitial(address, "THC");

                ErrorCode = master.ConsoleRead("labelWasteFlow_Value,labelWasteTemp_Value,ErrorCode");

                labelWasteTotal_Value.Text = SQLControl.GetSQLResult($@"Select  isnull(sum(Value14),0) from  [{ GlobalVariable.CPDCInfo.SQLDataCatelog }].[dbo].[AVG_T60]  
                                                          where RecDateTime >= @DateTime");
                labelWasteTotallast_Value.Text = SQLControl.GetSQLResult_Last_Midnight($@"Select  isnull(sum(Value14),0) from  [{GlobalVariable.CPDCInfo.SQLDataCatelog}].[dbo].[AVG_T60]  
                                                           where RecDateTime between @DateTime and @DateTime_End ");

                labelSiteLNGANow_Value.Text = SQLControl.GetSQLResult_For_LNG_today($@"Select  isnull(sum(Value25),0) from  [{ GlobalVariable.CPDCInfo.SQLDataCatelog }].[dbo].[AVG_T60]  
                                                          where RecDateTime >= @DateTime");
                labelSiteLNGBNow_Value.Text = SQLControl.GetSQLResult_For_LNG_today($@"Select  isnull(sum(Value26),0) from  [{ GlobalVariable.CPDCInfo.SQLDataCatelog }].[dbo].[AVG_T60]  
                                                          where RecDateTime >= @DateTime");

                labelSiteLNGALast_Value.Text = SQLControl.GetSQLResult_Last($@"Select  isnull(sum(Value25),0) from  [{ GlobalVariable.CPDCInfo.SQLDataCatelog }].[dbo].[AVG_T60]  
                                                           where RecDateTime between @DateTime and @DateTime_End ");
                labelSiteLNGBLast_Value.Text = SQLControl.GetSQLResult_Last($@"Select  isnull(sum(Value26),0) from  [{ GlobalVariable.CPDCInfo.SQLDataCatelog }].[dbo].[AVG_T60]  
                                                           where RecDateTime between @DateTime and @DateTime_End ");

                DataSet Regulate = SQLControl.GetDataResult($@"Select top 1 isnull(Range1_Zero,0) as Range1_Zero,isnull(Range1_Full,0) as Range1_Full,isnull(Range2_Zero,0) as Range2_Zero,isnull(Range2_Full,0) as Range2_Full  from  [{ GlobalVariable.CPDCInfo.SQLDataCatelog }].[dbo].[Regulate_Date]  
                                                          order by RecDateTime desc ");
                //Range1Zero_Value.Text = Regulate.Tables[0].Rows[0]["Range1_Zero"].ToString();
                //Range1Full_Value.Text = Regulate.Tables[0].Rows[0]["Range1_Full"].ToString();
                //Range2Zero_Value.Text = Regulate.Tables[0].Rows[0]["Range2_Zero"].ToString();
                //Range2Full_Value.Text = Regulate.Tables[0].Rows[0]["Range2_Full"].ToString();

                //20230522
                Range1Zero_Value.Text = (0.03 + 0.03 * double.Parse(Regulate.Tables[0].Rows[0]["Range1_Zero"].ToString())).ToString("#0.0000");
                Range1Full_Value.Text = (0.3 + 0.3 * double.Parse(Regulate.Tables[0].Rows[0]["Range1_Full"].ToString())).ToString("#0.0000");
                Range2Zero_Value.Text = (0.3 + 0.3 * double.Parse(Regulate.Tables[0].Rows[0]["Range2_Zero"].ToString())).ToString("#0.0000");
                Range2Full_Value.Text = (50 + 50 * double.Parse(Regulate.Tables[0].Rows[0]["Range2_Full"].ToString())).ToString("#0.0000");

                /*
                serial.SendData(GasArray, "Recive");
                Thread.Sleep(1000);
                serial.SendData(FireArray, "FireRecive");

                GasDataUpdated();
                VOCDataUpdated();
                tmrFlowSummary();
                */
            }
            catch (Exception ex)
            {
                Log.LogWrite("Main-ReconnectAll錯誤", 99);
                Log.LogWrite("Main-ReconnectAll錯誤" + ex.ToString(), 98);
            }
        }

        private void 申報設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeclareInput declareInput = new DeclareInput();//產生declareInput的物件
            declareInput.ShowDialog(this);
        }

        private void 系統訊息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemInfo systemInfo = new SystemInfo();//產生declareInput的物件

            systemInfo.WasteStatus = WasteStatus;
            systemInfo.GasStatus = GasStatus;
            systemInfo.DCSStatus = DCSStatus;
            systemInfo.RegulateStatus = RegulateStatus;
            systemInfo.ErrorCode = ErrorCode;
           
            systemInfo.ShowDialog(this);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                Log.LogWrite("程式開始執行",1);

                ResetAll();
                if (IsFileNewCreate)
                {
                    InitialFileControl.FileInitial();
                }
                else
                {
                    var Path = Environment.CurrentDirectory + @"\config.ini";
                    InitialFileControl.Manger(Path);
                }
                Log.LogFileCreate(); //Log記錄檔初始化
                SQLControl.SQLInitial(3); //SQL初始化

                //SQL密碼未加密時進行加密
                if (!InitialFileControl.FileRead("SQLConnect", "SqlConnUserPWD", "null").EndsWith("="))
                {
                    //私鑰轉char array
                    char[] KeyArray = GlobalVariable.Key.ToCharArray();
                    //反轉array
                    Array.Reverse(KeyArray);
                    //轉回string
                    string ReverseKey = new string(KeyArray);
                    //進行加密
                    string Crypt = AESEncryption.AESEncryptBase64(GlobalVariable.SQL.Password, ReverseKey);
                    //寫回config中
                    InitialFileControl.FileWrite("SQLConnect", "SqlConnUserPWD", Crypt);
                }
                if (SQLControl.SQLConnectStatus())
                {
                    SQLControl.GetDataColumn("GC");
                    SQLControl.GetDataColumn("Raw");

                    //20210831測試
                    SQLControl.GetDataColumn("All");
                }

                GlobalVariable.DeclareLoad();

                ReconnectAll();

                /*
                //20220209 測試用
                WriteDeclare writeDeclare = new WriteDeclare();
                writeDeclare.Create_old_file("1000,S2300410,FLL,V109" + "\n" + "2618,A002,1110208,2300,0.00,NA10,,,,0.00" + "\n", DateTime.Now);
                */

                /*
                //20210901 測試用
                DateTime Testing_Time = new DateTime(2022, 7, 1 , 0 , 0 , 0 , 0);
                WriteDeclare writeDeclare = new WriteDeclare();
                writeDeclare.WriteFile_T1440(Testing_Time, SQLControl);
                */

                //20210909 測試
                //SQLControl.Insert_T15(Testing_Time, "123");

                /*
                WriteDeclare writeDeclare = new WriteDeclare();
                writeDeclare.WriteFile_T1440(Testing_Time, SQLControl);
                */

                /*
                SQLControl.Cal_daily_value();
                */

                /*
                WriteDeclare writeDeclare = new WriteDeclare();
                writeDeclare.Create_old_file("123,eiejwoefjijwjiw,NAejfiwe9ew0,ewf,fewi,w0ew0fj0NAje9e0wNA");
                */

                timer_Refresh.Enabled = true;

                //DateTime dateTime = Convert.ToDateTime("2020-11-05 23:30:00");
                //WriteDeclare writeDeclare = new WriteDeclare();
                //writeDeclare.WriteFile_T15(dateTime, SQLControl);
            }
            catch (Exception ex)
            {
                Log.LogWrite("Main錯誤", 99);
                Log.LogWrite("Main錯誤" + ex.ToString(), 98);
            }
        }


        private void OnModbusDataUpdated(object sender, EventArgs e)
        {
            try
            {
                if (base.InvokeRequired)
                {
                    base.BeginInvoke(new MethodInvoker(delegate ()
                    {
                        this.OnModbusDataUpdated(sender, e);
                    }));
                    return;
                }
                
                if (this.slave != null)
                {
                    /*
                    labelPilotLight1_Value.Text = (slave.getHoldingRegister(1001) / 10.0).ToString("#0.0");
                    labelPilotLight2_Value.Text = (slave.getHoldingRegister(1002) / 10.0).ToString("#0.0");
                    labelPilotLight3_Value.Text = (slave.getHoldingRegister(1003) / 10.0).ToString("#0.0");
                    */
                    labelWaterA_Value.Text = (slave.getHoldingRegister(1007) / 10.0).ToString("#0.0");
                    labelWaterB_Value.Text = (slave.getHoldingRegister(1008) / 10.0).ToString("#0.0");
                }
                
            }
            catch (NotImplementedException NO)
            {
                Log.LogWrite(NO.ToString(), 98);
                Log.LogWrite("ModBus_M-接收數值[" + NO.ToString() + "]錯誤 失敗", 99);
            }
            catch (ArgumentOutOfRangeException AO)
            {
                Log.LogWrite(AO.ToString(), 98);
                Log.LogWrite("ModBus_M-接收數值[" + AO.ToString() + "]錯誤 失敗", 99);
            }
            catch (Exception ex)
            {
                Log.LogWrite("Main錯誤", 99);
                Log.LogWrite("Main錯誤" + ex.ToString(), 98);
            }
        }

        //自動校正
        private void Btn_Regulate_Click(object sender, EventArgs e)
        {
            try
            {
                serialRegulate.SendData(GasArray, "Regulate");
                Thread.Sleep(1000);
                if (GlobalVariable.CPDCInfo.Regulate_Status)
                    serialRegulate.DoReceive_Regulate();

                if (!GlobalVariable.CPDCInfo.Regulate_Status)
                    serialRegulate.DoReceive_Regulate();
            }
            catch (Exception ex)
            {
                Log.LogWrite("Main-Regulate錯誤", 99);
                Log.LogWrite("Main-Regulate錯誤" + ex.ToString(), 98);
            }
        }

        private void labelWaterA_Value_Click(object sender, EventArgs e)
        {

        }

        //初始化Comport
        private void InitialFileReload()
        {
            try
            {
                address = InitialFileControl.FileRead("File", "FileAddress", "null");
                ComWaste = InitialFileControl.FileRead("COMWaste", "Comport", "null");
                ComGas = InitialFileControl.FileRead("COMGas", "Comport", "null");
                ComDCS = InitialFileControl.FileRead("COMDCS", "Comport", "null");
                ComRegulate = InitialFileControl.FileRead("COMRegulate", "Comport", "0");
                BaudWaste = int.Parse(InitialFileControl.FileRead("COMWaste", "Baud", "0"));
                BaudGas = int.Parse(InitialFileControl.FileRead("COMGas", "Baud", "0"));
                BaudDCS = int.Parse(InitialFileControl.FileRead("COMDCS", "Baud", "0"));
                BaudRegulate = int.Parse(InitialFileControl.FileRead("COMRegulate", "Baud", "0"));

                //母火
                ComFire = InitialFileControl.FileRead("COMFire", "Comport", "null");
                BaudFire = int.Parse(InitialFileControl.FileRead("COMFire", "Baud", "0"));
            }
            catch (Exception ex)
            {
                Log.LogWrite("Main-InitialFileReload錯誤", 99);
                Log.LogWrite("Main-InitialFileReload錯誤" + ex.ToString(), 98);
            }
        }

        private void 幫助ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        //刷新數值 
        public void timer_Refresh_Tick(object Sender, EventArgs e)
        {
            try
            {
                if (MC_Count >= 10)
                {
                    slave.Close_Modbus();
                    slave = new ModbusSLV();
                    DCSStatus = slave.Consolo_ModbusSlave(ComDCS, BaudDCS);
                    slave.DataUpdated += OnModbusDataUpdated;
                }

                if (GlobalVariable.CPDCInfo.Regulate_Status)
                    serialRegulate.DoReceive_Regulate();


                serial.SendData(GasArray, "Recive");
                Thread.Sleep(1000);
                serial.SendData(FireArray, "FireRecive");
 

                labelWasteTotal_Value.Text = SQLControl.GetSQLResult(@"SELECT isnull(sum(Value14),0)  FROM [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[AVG_T60]  
                                                          where RecDateTime >= @DateTime ");

                //GC檢查並更新數值
                readFile.ReadFileInitial(address, "VOC");
                readFile.ReadFileInitial(address, "THC");

                timer_count++;
                if (timer_count == 2 || timer_count == 4 || timer_count == 6 || timer_count == 8 || timer_count == 10 || timer_count == 12)
                {
                    VOCDataUpdated();
                    tmrFlowSummary();
                    GasDataUpdated();
                }

                
                if (timer_count == 6 || timer_count == 12)
                {
                    if (MC1 != labelPilotLight1_Value.Text || MC2 != labelPilotLight2_Value.Text || MC3 != labelPilotLight3_Value.Text)
                    {
                        MC1 = labelPilotLight1_Value.Text;
                        MC2 = labelPilotLight2_Value.Text;
                        MC3 = labelPilotLight3_Value.Text;
                    }
                    else if (MC1 == "0.0" && MC2 == "0.0" && MC3 == "0.0")
                    {
                    }
                    
                    //20210817用不到 原本是用來數據卡死時 重新連線裝置
                    //else MC_Count++;

                    master.Console_ModbusRtu(ComWaste, BaudWaste);
                    ErrorCode = master.ConsoleRead("labelWasteFlow_Value,labelWasteTemp_Value,ErrorCode");
                }
                

                if (timer_count == 12)
                {
                    //測試用 隨機產生value
                    //Random_testing();

                    //20210929_每次insert前計算熱值 (之後搬進下面if內)
                    lblHCN_Value.Text = (float.Parse(lblHCN_PPM.Text) * 0.000000187 * 101.94).ToString("f4");
                    lblC3H6_Value.Text = (float.Parse(lblC3H6_PPM.Text) * 0.000000187 * 489.723).ToString("f4");
                    lblACN_Value.Text = (float.Parse(lblACN_PPM.Text) * 0.000000187 * 300.2).ToString("f4");
                    lblAN_Value.Text = (float.Parse(lblAN_PPM.Text) * 0.000000187 * 410.61).ToString("f4");
                    lblC1_Value.Text = (float.Parse(lblC1_PPM.Text) * 0.000000187 * 534.688).ToString("f4");
                    lblC2_Value.Text = (float.Parse(lblC2_PPM.Text) * 0.000000187 * 511.173).ToString("f4");
                    lblC3_Value.Text = (float.Parse(lblC3_PPM.Text) * 0.000000187 * 504.721).ToString("f4");
                    lblC4_Value.Text = (float.Parse(lblC4_PPM.Text) * 0.000000187 * 507.77).ToString("f4");
                    lblC5_Value.Text = (float.Parse(lblC5_PPM.Text) * 0.000000187 * 518.62).ToString("f4");
                    float sum = float.Parse(lblHCN_Value.Text) +
                    float.Parse(lblC3H6_Value.Text) +
                    float.Parse(lblACN_Value.Text) +
                    float.Parse(lblAN_Value.Text) +
                    float.Parse(lblC1_Value.Text) +
                    float.Parse(lblC2_Value.Text) +
                    float.Parse(lblC3_Value.Text) +
                    float.Parse(lblC4_Value.Text) +
                    float.Parse(lblC5_Value.Text);

                    label4.Text = sum.ToString("f4");



                    InsertSQL("GC");

                    InsertSQL("Raw");

                    // 計算總熱值 GHV
                    decimal GHV = Convert.ToDecimal(sum);

                    // 抓取 TMP 的值
                    decimal TMP = decimal.Parse(labelSiteTemp_Value.Text);

                    // 抓取各成分的熱值並轉換為 decimal
                    decimal C1 = decimal.Parse(lblC1_Value.Text);
                    decimal C2 = decimal.Parse(lblC2_Value.Text);
                    decimal C3 = decimal.Parse(lblC3_Value.Text);
                    decimal C4 = decimal.Parse(lblC4_Value.Text);
                    decimal C5 = decimal.Parse(lblC5_Value.Text);
                    decimal HCN = decimal.Parse(lblHCN_Value.Text);
                    decimal C3H6 = decimal.Parse(lblC3H6_Value.Text);
                    decimal ACN = decimal.Parse(lblACN_Value.Text);
                    decimal AN = decimal.Parse(lblAN_Value.Text);

                    // 呼叫 InsertRawData_Temporary 方法插入資料
                    SQLControl.InsertRawData_Temporary(C1, C2, C3, C4, C5, HCN, C3H6, ACN, AN, GHV, TMP);

                    if (GlobalVariable.CPDCInfo.GC_File_Status == true)
                    {
                        GlobalVariable.CPDCInfo.GC_File_Status = false;
                    }
                    

                    //15minute average && writeDeclareFile60
                    DateTime dateTime = DateTime.Now;
                    if (dateTime.ToString("HH:mm") == "07:10")
                    {
                        serial.SendData(GasArray, "Regulate");
                    }

                    switch (dateTime.ToString("mm"))
                    {
                        case "00":
                            
                            backgroundworker = new BackgroundWorker();
                            backgroundworker.DoWork += new DoWorkEventHandler(WriteFile);
                            backgroundworker.RunWorkerAsync();
                            break;
                        case "01":
                            labelSiteLNGANow_Value.Text = SQLControl.GetSQLResult_For_LNG_today($@"Select  isnull(sum(Value25),0) from  [{GlobalVariable.CPDCInfo.SQLDataCatelog}].[dbo].[AVG_T60]  
                                                          where RecDateTime >= @DateTime");
                            labelSiteLNGBNow_Value.Text = SQLControl.GetSQLResult_For_LNG_today($@"Select  isnull(sum(Value26),0) from  [{GlobalVariable.CPDCInfo.SQLDataCatelog}].[dbo].[AVG_T60]  
                                                          where RecDateTime >= @DateTime");

                            labelSiteLNGALast_Value.Text = SQLControl.GetSQLResult_Last($@"Select  isnull(sum(Value25),0) from  [{GlobalVariable.CPDCInfo.SQLDataCatelog}].[dbo].[AVG_T60]  
                                                           where RecDateTime between @DateTime and @DateTime_End ");
                            labelSiteLNGBLast_Value.Text = SQLControl.GetSQLResult_Last($@"Select  isnull(sum(Value26),0) from  [{GlobalVariable.CPDCInfo.SQLDataCatelog}].[dbo].[AVG_T60]  
                                                           where RecDateTime between @DateTime and @DateTime_End ");
                            labelWasteTotallast_Value.Text = SQLControl.GetSQLResult_Last_Midnight($@"Select  isnull(sum(Value14),0) from  [{GlobalVariable.CPDCInfo.SQLDataCatelog}].[dbo].[AVG_T60]  
                                                           where RecDateTime between @DateTime and @DateTime_End ");

                            break;
                        case "10":
                        case "15":
                        case "30":
                        case "45":
                            backgroundworker = new BackgroundWorker();
                            backgroundworker.DoWork += new DoWorkEventHandler(WriteFile);
                            backgroundworker.RunWorkerAsync();
                            break;
                    }
                    timer_count = 0;
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite("Main-timer_Refresh錯誤", 99);
                Log.LogWrite("Main-timer_Refresh錯誤" + ex.ToString(), 98);
            }
        }


        //寫入資料庫
        public void InsertSQL(string Type)//1.寫入類型
        {
            try
            {
                if (GlobalVariable.CPDCInfo.SQLDataInserString == null && GlobalVariable.CPDCInfo.SQLDataInserString == null)
                {
                    if (SQLControl.SQLConnectStatus())
                    {
                        SQLControl.GetDataColumn("GC");
                        SQLControl.GetDataColumn("Raw");
                        SQLControl.GetDataColumn("All");
                    }
                    else
                        return;
                }

                string ColumnString = "CNo,PolNo,RecDateTime,";
                switch (Type)
                {
                    case "GC":
                        ColumnString = GlobalVariable.CPDCInfo.SQLDataInserString_GC;
                        break;
                    case "Raw":
                        ColumnString = GlobalVariable.CPDCInfo.SQLDataInserString;
                        break;
                }

                string[] Keys = ColumnString.Split(',');
                string ValueString = "";

                //20210817 新增
                string Status = InitialFileControl.FileRead("CPDC", "Status", "");


                foreach (string Key in Keys)
                {
                    switch (Key)
                    {
                        
                        case "CNo":
                            ValueString += InitialFileControl.FileRead("Declare", "DeclareFlareNo", "") + ",";
                            break;
                        case "PolNo":
                            ValueString += InitialFileControl.FileRead("Declare", "DeclarePolNo", "") + ",";
                            break;
                        case "RecDateTime":
                            DateTime datetime = DateTime.Now;
                            ValueString += datetime.ToString("yyyy-MM-dd HH:mm") + ",";
                            break;
                        case "WasteTotalFlow":

                            //20210817 他直接給0 後面加狀態碼
                            ValueString += "0.0," + Find_Status(Key , "0.0") + ",";
                            break;
                        default:
                            if (Type == "GC")
                            {
                                if(Key == "HCN_heat" || Key == "C3H6_heat" || Key == "ACN_heat" || Key == "AN_heat")
                                {
                                    string[] tmp = Key.Split('_');

                                    var control = form1.Controls.OfType<Label>().FirstOrDefault(c => c.Name == "lbl" + tmp[0] + "_Value");
                                    control.Invoke((MethodInvoker)(() => ValueString += control.Text + ","));

                                    //20210817 新增
                                    ValueString += "NA10,";
                                }
                                else
                                {
                                    var control = form1.Controls.OfType<Label>().FirstOrDefault(c => c.Name == "lbl" + Key + "_PPM");
                                    control.Invoke((MethodInvoker)(() => ValueString += control.Text + ","));

                                    //20210817 新增
                                    ValueString += Find_Status(Key, control.Text) + ",";
                                }
                            }
                            else
                            {
                                var control = form1.Controls.OfType<Label>().FirstOrDefault(c => c.Name == "label" + Key + "_Value");
                                control.Invoke((MethodInvoker)(() => ValueString += control.Text + ","));

                                //20210817 新增
                                ValueString += Find_Status(Key, control.Text) + ",";
                            }
                            break;
                    }
                }
                if (!SQLstatus)
                    SQLControl.InsertToCPDC(ValueString.Substring(0, ValueString.Length - 1), Type);
            }
            catch (Exception ex)
            {
                Log.LogWrite("寫入資料庫錯誤GC" + ex.ToString(), 99);
            }
        }

        public string Find_Status(string Key , string Value)//1.寫入類型
        {
            string[] Fire = { "PilotLight1", "PilotLight2", "PilotLight3" };
            string[] Flow = { "WasteTemp", "WasteFlow" };
            string[] GC1 = { "C1","C2", "C3", "C4", "C5", "THC" };
            string[] GC2 = { "HCN", "C3H8", "C3H6", "HCN2", "ACN", "AN" };

            if(Array.Exists(Fire, x => x == Key))
            {
                string Status = InitialFileControl.FileRead("CPDC", "Status", "");
                if (Status.Substring(2, 2) == "10")
                {
                    if (Value == "0.0" || Value == "0.00")
                    {
                        return Status.Substring(0, 2) + "40";
                    }
                    else
                    {
                        return Status.Substring(0, 2) + "10";
                    }
                }
                else
                {
                    return Status;
                }
            }
            else if(Array.Exists(Flow, x => x == Key))
            {
                string Status = InitialFileControl.FileRead("CPDC", "Flow_Status", "");
                if (Status.Substring(2, 2) == "10")
                {
                    if ((Value == "0.0" || Value == "0.00") && Key != "WasteFlow")
                    {
                        return Status.Substring(0, 2) + "40";
                    }
                    else
                    {
                        return Status.Substring(0, 2) + "10";
                    }
                }
                else
                {
                    return Status;
                }
            }
            else if (Array.Exists(GC1, x => x == Key))
            {
                string Status = InitialFileControl.FileRead("CPDC", "GC1_Status", "");
                if (Status.Substring(2, 2) == "10")
                {
                    if (Value == "0.0" || Value == "0.00")
                    {
                        return Status.Substring(0, 2) + "10";
                    }
                    else
                    {
                        return Status.Substring(0, 2) + "10";
                    }
                }
                else
                {
                    return Status;
                }
            }
            else if (Array.Exists(GC2, x => x == Key))
            {
                string Status = InitialFileControl.FileRead("CPDC", "GC2_Status", "");
                if (Status.Substring(2, 2) == "10")
                {
                    if (Value == "0.0" || Value == "0.00")
                    {
                        return Status.Substring(0, 2) + "10";
                    }
                    else
                    {
                        return Status.Substring(0, 2) + "10";
                    }
                }
                else
                {
                    return Status;
                }
            }
            else
            {
                return "NA10";
            }
        }


        //申報檔案
        private void WriteFile(object sender, EventArgs e)
        {
            try
            {
                SQLstatus = true;
                DateTime dateTime = DateTime.Now;


                //20210826 不需要預存程式~

                if (dateTime.ToString("mm") != "10")
                {
                    SQLControl.Insert_T15(dateTime, "Cal_T15");
                }
                if (dateTime.ToString("mm") == "00")
                {
                    SQLControl.Insert_T60(dateTime, "Cal_T60");
                }


                WriteDeclare writeDeclare = new WriteDeclare();
                if (dateTime.ToString("HH:mm") == "00:00")
                {
                    writeDeclare.WriteFile_T60(dateTime, SQLControl);
                    writeDeclare.WriteFile_T1440(dateTime, SQLControl);
                }
                else if (dateTime.ToString("mm") == "00")
                {
                    writeDeclare.WriteFile_T60(dateTime, SQLControl);
                }
                else if (dateTime.ToString("mm") != "10")
                {
                    writeDeclare.WriteFile_T15(dateTime, SQLControl);
                }
                


                backgroundworker.Dispose();
                SQLstatus = false;
            }
            catch (Exception ex)
            {
                Log.LogWrite("backgroundworker erorr" + ex.ToString(), 99);
            }
        }

        //氣體偵測回傳值
        private void GasDataUpdated()
        {
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    double num = 0;
                    string value = "";
                    string LabelName = "";

                    switch (i)
                    {
                        case 0:
                            LabelName = "HCN";
                            break;
                        case 1:
                            LabelName = "O2";
                            break;
                        case 2:
                            LabelName = "HCN";
                            break;
                        case 3:
                            LabelName = "Temp";
                            break;
                        case 4:
                            LabelName = "LNGA";
                            break;
                        case 5:
                            LabelName = "LNGB";
                            break;
                    }
                    var control = form1.Controls.OfType<Label>().FirstOrDefault(c => c.Name == "labelSite" + LabelName + "_Value");
                    control.Invoke((MethodInvoker)(() => value = control.Text));
                    
                    if (value != "" && value != "0.0")
                        num = double.Parse(value);
                    if (i != 3)
                    {
                        string[] range = InitialFileControl.FileRead("Range", LabelName, "null").Split('~');
                        double persent = (Convert.ToDouble(range[1]) - Convert.ToDouble(range[0])) / 16;
                        //num = num / persent + 4;
                    }
                    if (Math.Abs(GDValues[i] - num) / GDValues[i] > 1.0)
                    {
                        GDValueErrors[i]++;
                    }
                    if (GDValueErrors[i] > 3 || GDValueErrors[i] == 0)
                    {
                        GDValues[i] = num;
                        GDValueErrors[i] = 0;
                    }
                }
                slave.setHoldingRegister(29, (ushort)(GDValues[4] * 10.0));
                slave.setHoldingRegister(30, (ushort)(GDValues[5] * 10.0));
                slave.setHoldingRegister(31, (ushort)(GDValues[1] * 10.0));
                slave.setHoldingRegister(32, (ushort)(GDValues[0] * 10.0));
                slave.setHoldingRegister(33, (ushort)(GDValues[2] * 10.0));
                slave.setHoldingRegister(34, (ushort)(GDValues[3] * 10.0));
            }
            catch (Exception e)
            {
                Log.LogWrite("氣體偵測回傳值錯誤:" + e.ToString(), 99);
            }
        }

        //VOC偵測回傳值
        private void VOCDataUpdated()
        {
            try
            {
                for (int i = 0; i < 12; i++)
                {
                    double num = 0;
                    string label_name = "", value = "";
                    switch (i)
                    {
                        case 0:
                            label_name = "C1";
                            break;
                        case 1:
                            label_name = "C2";
                            break;
                        case 2:
                            label_name = "C3H8";
                            break;
                        case 3:
                            label_name = "C3H6";
                            break;
                        case 4:
                            label_name = "C4";
                            break;
                        case 5:
                            label_name = "C5";
                            break;
                        case 6:
                            label_name = "THC";
                            break;
                        case 7:
                            label_name = "HCN";
                            break;
                        case 8:
                            label_name = "C3";
                            break;
                        case 9:
                            label_name = "HCN2";
                            break;
                        case 10:
                            label_name = "ACN";
                            break;
                        case 11:
                            label_name = "AN";
                            break;
                    }
                    var control = form1.Controls.OfType<Label>().FirstOrDefault(c => c.Name == "lbl" + label_name + "_PPM");
                    control.Invoke((MethodInvoker)(() => value = control.Text));

                    if (value != "" && value != "0.0")
                        num = double.Parse(value) * 10;

                    slave.setHoldingRegister(i * 2 + 1, (ushort)(num / 65536.0));
                    slave.setHoldingRegister(i * 2 + 2, (ushort)(num % 65536.0));
                }
            }
            catch (Exception e)
            {
                Log.LogWrite("VOC偵測回傳值錯誤:" + e.ToString(), 99);
            }
        }

        //流量累計回傳值
        private void tmrFlowSummary()
        {
            try
            {
                LNGFlowASummaryNow += GDValues[4] / 60.0;
                LNGFlowBSummaryNow += GDValues[5] / 60.0;
                if (DateTime.Now.Hour == 7 && DateTime.Now.Minute == 0)
                {
                    LNGFlowASummaryLast = LNGFlowASummaryNow;
                    LNGFlowASummaryNow = 0.0;
                    LNGFlowBSummaryLast = LNGFlowBSummaryNow;
                    LNGFlowBSummaryNow = 0.0;
                }
                slave.setHoldingRegister(35, (ushort)(double.Parse(labelSiteLNGANow_Value.Text) * 10));
                slave.setHoldingRegister(36, (ushort)(double.Parse(labelSiteLNGBNow_Value.Text) * 10));
                slave.setHoldingRegister(37, (ushort)(double.Parse(labelSiteLNGALast_Value.Text) * 10));
                slave.setHoldingRegister(38, (ushort)(double.Parse(labelSiteLNGBLast_Value.Text) * 10));

                //20230613
                double WasteFlow = 0;
                double WasteFlow_Total = 0;
                double WasteFlow_Totallast = 0; 
                double labelPilotLight1 = 0 ;
                double labelPilotLight2 = 0;
                double labelPilotLight3 = 0;

                WasteFlow = double.Parse(labelWasteFlow_Value.Text) * 10;
                WasteFlow_Total = double.Parse(labelWasteTotal_Value.Text) * 10;
                WasteFlow_Totallast = double.Parse(labelWasteTotallast_Value.Text) * 10;
                labelPilotLight1 = double.Parse(labelPilotLight1_Value.Text) * 10;
                labelPilotLight2 = double.Parse(labelPilotLight2_Value.Text) * 10;
                labelPilotLight3 = double.Parse(labelPilotLight3_Value.Text) * 10;

                slave.setHoldingRegister(39, (ushort)(WasteFlow / 65536.0));
                slave.setHoldingRegister(40, (ushort)(WasteFlow % 65536.0));

                slave.setHoldingRegister(41, (ushort)(WasteFlow_Total / 65536.0));
                slave.setHoldingRegister(42, (ushort)(WasteFlow_Total % 65536.0));

                slave.setHoldingRegister(43, (ushort)(WasteFlow_Totallast / 65536.0));
                slave.setHoldingRegister(44, (ushort)(WasteFlow_Totallast % 65536.0));

                slave.setHoldingRegister(45, (ushort)(labelPilotLight1 / 65536.0));
                slave.setHoldingRegister(46, (ushort)(labelPilotLight1 % 65536.0));

                slave.setHoldingRegister(47, (ushort)(labelPilotLight2 / 65536.0));
                slave.setHoldingRegister(48, (ushort)(labelPilotLight2 % 65536.0));

                slave.setHoldingRegister(49, (ushort)(labelPilotLight3 / 65536.0));
                slave.setHoldingRegister(50, (ushort)(labelPilotLight3 % 65536.0));
            }
            catch (Exception ex)
            {
                Log.LogWrite("流量偵測回傳值錯誤:" + ex.ToString(), 99);
            }
        }

        //運轉回傳值?(沒作用)
        private void OnHttpDataUpdated(object sender, EventArgs e)
        {
            if (base.InvokeRequired)
            {
                base.BeginInvoke(new MethodInvoker(delegate ()
                {
                    this.OnHttpDataUpdated(sender, e);
                }));
                return;
            }
            double num = 1200.0;
            double nmhcdata = this._mGetHttpData.getNMHCData();
            double vocdata = this._mGetHttpData.getVOCData();
            if (nmhcdata > num && !this._NMHCFlameOutLastState)
            {
                Log.LogWrite("NMHC 熄火", 1);
                this._NMHCFlameOutAlarmAcked = false;
            }
            if (nmhcdata < num && this._NMHCFlameOutLastState)
            {
                Log.LogWrite("NMHC 正常", 1);
            }
            if (vocdata > num && !this._VOCFlameOutLastState)
            {
                Log.LogWrite("VOC 熄火", 1);
                this._NMHCFlameOutAlarmAcked = false;
            }
            if (vocdata < num && this._VOCFlameOutLastState)
            {
                Log.LogWrite("VOC 正常", 1);
            }

            this.slave.setHoldingRegister(27, Convert.ToUInt16(this._NMHCFlameOutLastState ? 1 : 0));
            this.slave.setHoldingRegister(28, Convert.ToUInt16(this._VOCFlameOutLastState ? 1 : 0));
        }

        //重製欄位數值
        private void ResetAll()
        {
            try
            {
                labelSiteH2_Value.Text = "0.0";
                labelSiteO2_Value.Text = "0.0";
                labelSiteHCN_Value.Text = "0.0";
                labelSiteTemp_Value.Text = "0.0";
                labelSiteLNGA_Value.Text = "0.0";
                labelSiteLNGB_Value.Text = "0.0";

                labelPilotLight1_Value.Text = "0.0";
                labelPilotLight2_Value.Text = "0.0";
                labelPilotLight3_Value.Text = "0.0";
                labelWasteTemp_Value.Text = "0.0";
                labelWasteFlow_Value.Text = "0.0";
                labelWasteTotal_Value.Text = "0.0";
                labelWaterA_Value.Text = "0.0";
                labelWaterB_Value.Text = "0.0";

                Range1Zero_Value.Text = "0.0";
                Range2Zero_Value.Text = "0.0";
                Range1Full_Value.Text = "0.0";
                Range2Full_Value.Text = "0.0";

                lblC1_PPM.Text = "0.00";
                lblC2_PPM.Text = "0.00";
                lblC3_PPM.Text = "0.00";
                lblC4_PPM.Text = "0.00";
                lblC5_PPM.Text = "0.00";
                lblTHC_PPM.Text = "0.00";
                lblHCN_PPM.Text = "0.00";
                lblC3H8_PPM.Text = "0.00";
                lblC3H6_PPM.Text = "0.00";
                lblHCN2_PPM.Text = "0.00";
                lblACN_PPM.Text = "0.00";
                lblAN_PPM.Text = "0.00";

                lblC1_Value.Text = "0.0";
                lblC2_Value.Text = "0.0";
                lblC3_Value.Text = "0.0";
                lblC4_Value.Text = "0.0";
                lblC5_Value.Text = "0.0";
                lblTHC_Value.Text = "0.0";
                lblHCN_Value.Text = "0.0";
                lblC3H8_Value.Text = "0.0";
                lblC3H6_Value.Text = "0.0";
                lblHCN2_Value.Text = "0.0";
                lblACN_Value.Text = "0.0";
                lblAN_Value.Text = "0.0";
            }
            catch (Exception ex)
            {
                Log.LogWrite("Main-ResetAll錯誤", 99);
                Log.LogWrite("Main-ResetAll錯誤" + ex.ToString(), 98);
            }

        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            Log.LogWrite("Main-ThreadException錯誤", 99);
            Log.LogWrite("Main-ThreadException錯誤" + e.Exception.InnerException.ToString(), 98);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            Log.LogWrite("Main-UnhandledException錯誤", 99);
            Log.LogWrite("Main-UnhandledException錯誤" + e.ExceptionObject.ToString(), 98);
        }


    }
}
