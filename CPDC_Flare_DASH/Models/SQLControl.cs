using System;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Globalization;

namespace CPDC_Flare_DASH.Models
{
    class SQLControl
    {
        private string ConnectString;
        private SqlConnection conn;
        // command 1:AQI 99:Error
        public void SQLInitial(int command)
        {
            var Path = Environment.CurrentDirectory + @"\config.ini";
            InitialFileControl.Manger(Path);
            // SQL ====== 
            GlobalVariable.SQL.IP = InitialFileControl.FileRead("SQLConnect", "SqlConnServerIP", "null");
            GlobalVariable.SQL.Port = InitialFileControl.FileRead("SQLConnect", "SqlConnServerPort", "null");
            GlobalVariable.SQL.User = InitialFileControl.FileRead("SQLConnect", "SqlConnUserID", "null");
            GlobalVariable.SQL.Password = InitialFileControl.FileRead("SQLConnect", "SqlConnUserPWD", "null");
            GlobalVariable.SQL.Password = InitialFileControl.FileRead("SQLConnect", "SqlConnUserPWD", "null");

            //CPDC=======
            GlobalVariable.CPDCInfo.SQLDataCatelog = InitialFileControl.FileRead("CPDC", "SQLDataCatelog", "null");
            //SQL密碼解密
            if (GlobalVariable.SQL.Password.EndsWith("="))
            {
                char[] KeyArray = GlobalVariable.Key.ToCharArray();
                Array.Reverse(KeyArray);
                string ReverseKey = new string(KeyArray);
                GlobalVariable.SQL.Password = AESEncryption.AESDecryptBase64(GlobalVariable.SQL.Password, ReverseKey);
            }

            ConnectString = @"data source = " + GlobalVariable.SQL.IP + "," + GlobalVariable.SQL.Port + ";user id = " + GlobalVariable.SQL.User + "; password = " + GlobalVariable.SQL.Password + ";Timeout=1";
            GlobalVariable.SQL.Status = SQLConnect(command);
        }

        //SQL連接
        private bool SQLConnect(int command)
        {
            try
            {
                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    return false;
                }
                conn = new SqlConnection(ConnectString);
                conn.Open();
                //Log.LogWrite("SQL Connect Successful", 1);
                switch (command)
                {
                    case 1:
                        //DataColletionSystem.ListBoxAQILogWrite("Connect to SQL");
                        break;
                }
                return true;
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-資料庫連線 失敗", 99);
                return false;
            }
        }

        //SQL連線狀態
        public Boolean SQLConnectStatus()
        {
            if (conn.State == ConnectionState.Open)
                return true;
            else
                return false;
        }

        //SQL斷開連接
        public void SQLDisconnect()
        {
            conn.Close();
        }

        //SQL新增資料
        public void InsertToCPDC(string ValueString, string Type)//1.數值 2.寫入類型
        {
            try
            {
                //數值!!!!!!!!!!!!!!!
                string[] ValueArray = ValueString.Split(',');//偵測數值

                string InsertSQLTable = "", SQLDataValueString = "", ColumnString = "";//1.寫入表格  2.Parameters 3.SQL欄位
                int count = 0;//計數
                switch (Type)//依類型取得對應欄位數值
                {
                    case "GC":
                        InsertSQLTable = "RawData_GC";
                        SQLDataValueString = GlobalVariable.CPDCInfo.SQLDataValueString_GC;
                        for (int i = 0; i < GlobalVariable.SQL.InsertToRawGC.GetLength(0); i++)
                        {
                            ColumnString += "Value" + GlobalVariable.SQL.InsertToRawGC[i, 0] + ",";

                            //20210817 加入status
                            ColumnString += "Status" + GlobalVariable.SQL.InsertToRawGC[i, 0] + ",";
                        }
                        break;
                    case "Raw":
                        InsertSQLTable = "RawData";

                        // @CNo,@PolNo,@RecDateTime"
                        SQLDataValueString = GlobalVariable.CPDCInfo.SQLDataValueString;

                        for (int i = 0; i < GlobalVariable.SQL.InsertToRaw.GetLength(0); i++)
                        {
                            ColumnString += "Value" + GlobalVariable.SQL.InsertToRaw[i, 0] + ",";

                            //20210817 加入status
                            ColumnString += "Status" + GlobalVariable.SQL.InsertToRaw[i, 0] + ",";
                        }
                        break;
                }

                string CommandString = @"Insert Into " + GlobalVariable.CPDCInfo.SQLDataCatelog + ".dbo. " + InsertSQLTable + " (CNo,PolNo,RecDateTime," + ColumnString;
                CommandString = CommandString.Substring(0, CommandString.Length - 1) + ")Values(" + SQLDataValueString + ")";

                SqlCommand Command = new SqlCommand(CommandString, conn);
                Command.CommandTimeout = 5;
                string[] ColumnArray = SQLDataValueString.Split(',');

                foreach (string Key in ColumnArray)
                {
                    // 20210817 新增判斷 @Status
                    if (Key == "@CNo" || Key == "@PolNo" || Key == "@RecDateTime" || (Key.Length >= 7 && Key.Substring(0, 7) == "@Status"))
                    {
                        Command.Parameters.AddWithValue(Key, ValueArray[count]);
                    }
                    else
                    {
                        if (ValueArray[count] == null || ValueArray[count] == "" || ValueArray[count] == "0" || ValueArray[count] == "0.0" || ValueArray[count] == "0.00" )
                        {
                            if(Key == "@WasteFlow") // 0就0 原本寫NULL
                            {
                                Command.Parameters.AddWithValue(Key, 0.00);
                            }
                            else
                            {
                                Command.Parameters.AddWithValue(Key, DBNull.Value);
                            }
                        }
                        else
                        {
                            Command.Parameters.AddWithValue(Key, decimal.Parse(ValueArray[count]));
                        }
                    }
                    count++;
                }

                try
                {
                    if (conn != null && conn.State != ConnectionState.Closed)
                    {
                        Command.ExecuteNonQuery();
                    }
                    else
                    {
                        GlobalVariable.SQL.Status = SQLConnect(1);
                        Command.ExecuteNonQuery();
                    }

                    Command.Dispose();
                    DataColletionSystem.ListBoxAQILogWrite("Process CPDC " + Type +" insert.");
                }
                catch (Exception e)
                {
                    Command.Parameters.Clear();
                    if (e.Message.Contains("重複的索引鍵"))
                    {
                        Log.LogWrite(e.ToString(), 98);
                        Log.LogWrite("SQL-新增資料 失敗 重複的索引鍵", 99);
                        Log.LogWrite(Type + "----" + ValueString, 99);
                    }
                    else if (!e.Message.Contains("重複的索引鍵"))
                    {
                        Log.LogWrite(e.ToString(), 98);
                        Log.LogWrite("SQL-新增資料 失敗 其他", 99);
                        Log.LogWrite(Type + "----" + ValueString, 99);
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-新增資料 失敗", 99);
                Log.LogWrite(Type + "----" + ValueString, 99);
            }
        }

        //SQL取得欄位定義
        public void GetDataColumn(string ColumnType)
        {
            try
            {
                string CommandString = "";
                string ColumnString = "CNo,PolNo,RecDateTime,";

                //20210817 新增 帶狀態碼
                string ColumnString_withStatus = "CNo,PolNo,RecDateTime,";

                DataSet datatable = new DataSet();
                SqlDataAdapter dt;
                int column_count = 0, row_count = 0;
                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    switch (ColumnType)
                    {
                        case "GC":
                            CommandString = @"SELECT ColNo,ParName,ItemNumber,STD FROM " + GlobalVariable.CPDCInfo.SQLDataCatelog + ".dbo. Parameter where ColNo <= 12 or ColNo > 26 order by ColNo ";
                            dt = new SqlDataAdapter(CommandString, conn);
                            dt.Fill(datatable);
                            foreach (DataRow mDr in datatable.Tables[0].Rows)
                            {
                                column_count = 0;
                                foreach (DataColumn mDc in datatable.Tables[0].Columns)
                                {
                                    GlobalVariable.SQL.InsertToRawGC[row_count, column_count] = mDr[mDc].ToString();
                                    column_count++;
                                }
                                row_count++;
                            }
                            for (int i = 0; i < GlobalVariable.SQL.InsertToRawGC.GetLength(0); i++)
                            {
                                if (GlobalVariable.SQL.InsertToRawGC[i, 1] == "HCN+")
                                {
                                    ColumnString += "HCN2,";

                                    // 20210817 新增狀態碼變數 EX @Status_Pilot
                                    ColumnString_withStatus += "HCN2,";
                                    ColumnString_withStatus += "Status_HCN2,";
                                }
                                else if (GlobalVariable.SQL.InsertToRawGC[i, 1] == "C5+")
                                {
                                    ColumnString += "C5,";

                                    // 20210817 新增狀態碼變數 EX @Status_Pilot
                                    ColumnString_withStatus += "C5,";
                                    ColumnString_withStatus += "Status_C5,";
                                }
                                else
                                {
                                    ColumnString += GlobalVariable.SQL.InsertToRawGC[i, 1] + ",";

                                    // 20210817 新增狀態碼變數 EX @Status_Pilot
                                    ColumnString_withStatus += GlobalVariable.SQL.InsertToRawGC[i, 1] + ",";
                                    ColumnString_withStatus += "Status_" + GlobalVariable.SQL.InsertToRawGC[i, 1] + ",";
                                }
                                CommandString += "Value" + GlobalVariable.SQL.InsertToRawGC[i, 0] + ",";
                            }
                            GlobalVariable.CPDCInfo.SQLDataInserString_GC = ColumnString.Substring(0, ColumnString.Length - 1);

                            //GlobalVariable.CPDCInfo.SQLDataValueString_GC = "@" + GlobalVariable.CPDCInfo.SQLDataInserString_GC.Replace(",", ",@");
                            // 20210817 新增狀態碼變數 EX @Status_Pilot
                            GlobalVariable.CPDCInfo.SQLDataValueString_GC = "@" + ColumnString_withStatus.Substring(0, ColumnString_withStatus.Length - 1).Replace(",", ",@");

                            break;
                        case "Raw":
                            CommandString = @"SELECT ColNo,ParName,ItemNumber,STD FROM " + GlobalVariable.CPDCInfo.SQLDataCatelog + ".dbo.Parameter where ColNo > 12 and ColNo < 27  order by ColNo ";
                            dt = new SqlDataAdapter(CommandString, conn);
                            dt.Fill(datatable);
                            foreach (DataRow mDr in datatable.Tables[0].Rows)
                            {
                                column_count = 0;
                                foreach (DataColumn mDc in datatable.Tables[0].Columns)
                                {
                                    GlobalVariable.SQL.InsertToRaw[row_count, column_count] = mDr[mDc].ToString();
                                    column_count++;
                                }
                                row_count++;
                            }
                            for (int i = 0; i < GlobalVariable.SQL.InsertToRaw.GetLength(0); i++)
                            {
                                ColumnString += GlobalVariable.SQL.InsertToRaw[i, 1] + ",";

                                // 20210817 新增狀態碼變數 EX @Status_Pilot
                                ColumnString_withStatus += GlobalVariable.SQL.InsertToRaw[i, 1] + ",";
                                ColumnString_withStatus += "Status_" + GlobalVariable.SQL.InsertToRaw[i, 1] + ",";
                            }
                            GlobalVariable.CPDCInfo.SQLDataInserString = ColumnString.Substring(0, ColumnString.Length - 1);

                            //GlobalVariable.CPDCInfo.SQLDataValueString = "@" + GlobalVariable.CPDCInfo.SQLDataInserString.Replace(",", ",@");
                            // 20210817 新增狀態碼變數 EX @Status_Pilot
                            GlobalVariable.CPDCInfo.SQLDataValueString = "@" + ColumnString_withStatus.Substring(0, ColumnString_withStatus.Length - 1).Replace(",", ",@");
                            break;

                        case "All":
                            CommandString = @"SELECT ColNo,ParName,ItemNumber,STD FROM " + GlobalVariable.CPDCInfo.SQLDataCatelog + ".dbo.Parameter order by ColNo ";
                            dt = new SqlDataAdapter(CommandString, conn);
                            dt.Fill(datatable);
                            foreach (DataRow mDr in datatable.Tables[0].Rows)
                            {
                                column_count = 0;
                                foreach (DataColumn mDc in datatable.Tables[0].Columns)
                                {
                                    GlobalVariable.SQL.InsertToAll[row_count, column_count] = mDr[mDc].ToString();
                                    column_count++;
                                }
                                row_count++;
                            }
                            for (int i = 0; i < GlobalVariable.SQL.InsertToAll.GetLength(0); i++)
                            {
                                if (GlobalVariable.SQL.InsertToAll[i, 1] == "HCN+")
                                {
                                    ColumnString += "HCN2,";

                                    // 20210817 新增狀態碼變數 EX @Status_Pilot
                                    ColumnString_withStatus += "HCN2,";
                                    ColumnString_withStatus += "Status_HCN2,";
                                }
                                else if (GlobalVariable.SQL.InsertToAll[i, 1] == "C5+")
                                {
                                    ColumnString += "C5,";

                                    // 20210817 新增狀態碼變數 EX @Status_Pilot
                                    ColumnString_withStatus += "C5,";
                                    ColumnString_withStatus += "Status_C5,";
                                }
                                else
                                {
                                    ColumnString += GlobalVariable.SQL.InsertToAll[i, 1] + ",";

                                    // 20210817 新增狀態碼變數 EX @Status_Pilot
                                    ColumnString_withStatus += GlobalVariable.SQL.InsertToAll[i, 1] + ",";
                                    ColumnString_withStatus += "Status_" + GlobalVariable.SQL.InsertToAll[i, 1] + ",";
                                }
                            }
                            GlobalVariable.CPDCInfo.SQLDataInserString_All = ColumnString.Substring(0, ColumnString.Length - 1);

                            //GlobalVariable.CPDCInfo.SQLDataValueString = "@" + GlobalVariable.CPDCInfo.SQLDataInserString.Replace(",", ",@");
                            // 20210817 新增狀態碼變數 EX @Status_Pilot
                            GlobalVariable.CPDCInfo.SQLDataValueString_All = "@" + ColumnString_withStatus.Substring(0, ColumnString_withStatus.Length - 1).Replace(",", ",@");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-取得欄位錯誤 失敗", 99);
                throw;
            }
        }

        //SQL取得平均值
        public DataSet GetDataAVG(string ColumnName, string DataTableName, string StartTime, string EndTime)
        {
            try
            {
                string CommandString = "";
                DataSet datatable = new DataSet();
                SqlDataAdapter dt;
                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    /*
                    CommandString = $@"SELECT {ColumnName} FROM { GlobalVariable.CPDCInfo.SQLDataCatelog}.dbo.{ DataTableName }
                                           where RecDateTime > @StartTime 
                                           and RecDateTime < @EndTime 
                                           group by RecDateTime
                                           order by RecDateTime asc";
                    */

                    //20210903 只拿一筆
                    CommandString = $@"SELECT {ColumnName} FROM { GlobalVariable.CPDCInfo.SQLDataCatelog}.dbo.{ DataTableName }
                                           where RecDateTime > @StartTime 
                                           and RecDateTime < @EndTime ";

                    Log.LogWrite(CommandString, 1);
                    Log.LogWrite(StartTime, 1);
                    Log.LogWrite(EndTime, 1);
                    dt = new SqlDataAdapter(CommandString, conn);
                    dt.SelectCommand.Parameters.AddWithValue("@StartTime", StartTime);
                    dt.SelectCommand.Parameters.AddWithValue("@EndTime", EndTime);
                    dt.Fill(datatable);
                    dt.Dispose();

                    return datatable;
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-取得平均值錯誤 失敗", 99);
                return null;
            }
        }

        //20211025 日報新增欄位
        public string Cal_daily_value(DateTime dateTime)
        {
            try
            {
                string File_txt = "";

                string FlareNo = InitialFileControl.FileRead("Declare", "DeclareFlareNo", "");
                string PolNo = InitialFileControl.FileRead("Declare", "DeclarePolNo", "");
                string CNo = InitialFileControl.FileRead("Declare", "DeclareCNo", "");


                string Status = InitialFileControl.FileRead("CPDC", "Status", "00");
                TaiwanCalendar taiwanCalendar = new TaiwanCalendar();

                string sql_string = @"select cast(round(isnull(Value7, 0), 2) as numeric(15, 2)) AS Value7 , Status7,
                                        cast(round(isnull(Value9, 0), 2) as numeric(15, 2)) AS Value9 , Status9,
                                        cast(round(isnull(Value11, 0), 2) as numeric(15, 2)) AS Value11 , Status11,
                                        cast(round(isnull(Value12, 0), 2) as numeric(15, 2)) AS Value12 , Status12,
                                        cast(round(isnull(Value13, 0), 2) as numeric(15, 2)) AS Value13 , Status13,
                                        cast(round(isnull(Value14, 0), 2) as numeric(15, 2)) AS Value14 , Status14,
                                        cast(round(isnull(Value16, 0), 2) as numeric(15, 2)) AS Value16 , Status16,
                                        cast(round(isnull(Value17, 0), 2) as numeric(15, 2)) AS Value17 , Status17,
                                        cast(round(isnull(Value18, 0), 2) as numeric(15, 2)) AS Value18 , Status18
                                        FROM " + GlobalVariable.CPDCInfo.SQLDataCatelog + ".dbo.AVG_T60" +
                                            @" where RecDateTime between @StartTime and @EndTime ";

                //測試 日期多扣一天
                string EndTime = dateTime.ToString("yyyy-MM-dd 00:00:00");
                string StartTime = dateTime.AddDays(-1).ToString("yyyy-MM-dd 00:00:00");

                SqlCommand command = new SqlCommand(sql_string,conn);
                command.Parameters.AddWithValue("@StartTime", StartTime);
                command.Parameters.AddWithValue("@EndTime", EndTime);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    double HCN_AVG = 0.00, C6H6_AVG = 0.00, ACN_AVG = 0.00, AN_AVG = 0.00, WasteTemp_AVG = 0.00, WasteFlow_AVG = 0.00, PilotLight_AVGA = 0.00, PilotLight_AVGB = 0.00, PilotLight_AVGC = 0.00;
                    int HCN_count = 0, C6H6_count = 0, ACN_count = 0, AN_count = 0, WasteTemp_count = 0, WasteFlow_count = 0, PilotLight_count = 0;

                    while (reader.Read())
                    {
                        //HCN 日平均
                        if(reader["Status7"].ToString() == "NA10")
                        {
                            HCN_AVG = HCN_AVG + Convert.ToDouble(reader["Value7"].ToString());
                            HCN_count = HCN_count + 1;
                        }

                        //C6H6 日平均
                        if (reader["Status9"].ToString() == "NA10")
                        {
                            C6H6_AVG = C6H6_AVG + Convert.ToDouble(reader["Value9"].ToString());
                            C6H6_count = C6H6_count + 1;
                        }

                        //ACN 日平均
                        if (reader["Status11"].ToString() == "NA10")
                        {
                            ACN_AVG = ACN_AVG + Convert.ToDouble(reader["Value11"].ToString());
                            ACN_count = ACN_count + 1;
                        }

                        //AN 日平均
                        if (reader["Status12"].ToString() == "NA10")
                        {
                            AN_AVG = AN_AVG + Convert.ToDouble(reader["Value12"].ToString());
                            AN_count = AN_count + 1;
                        }

                        //WasteTemp 日平均
                        if (reader["Status13"].ToString() == "NA10")
                        {
                            WasteTemp_AVG = WasteTemp_AVG + Convert.ToDouble(reader["Value13"].ToString());
                            WasteTemp_count = WasteTemp_count + 1;
                        }

                        //WasteFlow 日平均
                        if (reader["Status14"].ToString() == "NA10")
                        {
                            WasteFlow_AVG = WasteFlow_AVG + Convert.ToDouble(reader["Value14"].ToString());
                            WasteFlow_count = WasteFlow_count + 1;
                        }
                        ////母火 日平均
                        PilotLight_AVGA = PilotLight_AVGA + Convert.ToDouble(reader["Value16"].ToString());
                        PilotLight_AVGB = PilotLight_AVGB + Convert.ToDouble(reader["Value17"].ToString());
                        PilotLight_AVGC = PilotLight_AVGC + Convert.ToDouble(reader["Value18"].ToString());
                        PilotLight_count = PilotLight_count + 1;
                        
                        //if (Convert.ToDouble(reader["Value16"].ToString()) >= Convert.ToDouble(reader["Value17"].ToString()) && Convert.ToDouble(reader["Value16"].ToString()) >= Convert.ToDouble(reader["Value18"].ToString()))
                        //{

                        //}
                        //else if (Convert.ToDouble(reader["Value17"].ToString()) >= Convert.ToDouble(reader["Value16"].ToString()) && Convert.ToDouble(reader["Value17"].ToString()) >= Convert.ToDouble(reader["Value18"].ToString()))
                        //{

                        //}
                        //else if (Convert.ToDouble(reader["Value18"].ToString()) >= Convert.ToDouble(reader["Value16"].ToString()) && Convert.ToDouble(reader["Value18"].ToString()) >= Convert.ToDouble(reader["Value17"].ToString()))
                        //{

                        //}
                    }

                    //先算廢棄流率平均 (VOC日平均會用到)
                    if (WasteFlow_count > 0)
                    {
                        WasteFlow_AVG = WasteFlow_AVG / WasteFlow_count;
                        File_txt = File_txt + "A580," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + WasteFlow_AVG.ToString("0.00") + "\n";
                    }
                    else
                    {
                        File_txt = File_txt + "A580," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                    }

                    //廢棄溫度平均
                    if (WasteTemp_count > 0)
                    {
                        WasteTemp_AVG = WasteTemp_AVG / WasteTemp_count;
                        File_txt = File_txt + "A581," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + WasteTemp_AVG.ToString("0.00") + "\n";
                    }
                    else
                    {
                        File_txt = File_txt + "A581," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                    }

                    //母火溫度平均
                    if (PilotLight_count > 0)
                    {
                        PilotLight_AVGA = PilotLight_AVGA / PilotLight_count;
                        PilotLight_AVGB = PilotLight_AVGB / PilotLight_count;
                        PilotLight_AVGC = PilotLight_AVGC / PilotLight_count;
                        File_txt = File_txt + "A590A," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + PilotLight_AVGA.ToString("0.00") + "\n";
                        File_txt = File_txt + "A590B," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + PilotLight_AVGB.ToString("0.00") + "\n";
                        File_txt = File_txt + "A590C," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + PilotLight_AVGC.ToString("0.00") + "\n";
                    }
                    else
                    {
                        File_txt = File_txt + "A590A," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                        File_txt = File_txt + "A590B," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                        File_txt = File_txt + "A590C," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                    }

                    //HCN
                    if (HCN_count > 0)
                    {
                        HCN_AVG = HCN_AVG / HCN_count;
                        File_txt = File_txt + "5999," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + HCN_AVG.ToString("0.00") + "\n";

                        //算排放量
                        if(WasteFlow_count > 0)
                        {
                            double tmp = HCN_AVG * WasteFlow_AVG * 0.71;
                            File_txt = File_txt + "3999," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + tmp.ToString("0.00") + "\n";
                        }
                        else
                        {
                            File_txt = File_txt + "3999," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                        }
                    }
                    else
                    {
                        File_txt = File_txt + "3999," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                        File_txt = File_txt + "5999," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                    }

                    //C6H6
                    if (C6H6_count > 0)
                    {
                        C6H6_AVG = C6H6_AVG / C6H6_count;
                        File_txt = File_txt + "5258," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + C6H6_AVG.ToString("0.00") + "\n";

                        //算排放量
                        if (WasteFlow_count > 0)
                        {
                            double tmp = C6H6_AVG * WasteFlow_AVG * 0.71;
                            File_txt = File_txt + "3258," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + tmp.ToString("0.00") + "\n";
                        }
                        else
                        {
                            File_txt = File_txt + "3258," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                        }
                    }
                    else
                    {
                        File_txt = File_txt + "3258," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                        File_txt = File_txt + "5258," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                    }

                    //ACN
                    if (ACN_count > 0)
                    {
                        ACN_AVG = ACN_AVG / ACN_count;
                        File_txt = File_txt + "5601," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + ACN_AVG.ToString("0.00") + "\n";

                        //算排放量
                        if (WasteFlow_count > 0)
                        {
                            double tmp = ACN_AVG * WasteFlow_AVG * 0.71;
                            File_txt = File_txt + "3601," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + tmp.ToString("0.00") + "\n";
                        }
                        else
                        {
                            File_txt = File_txt + "3601," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                        }
                    }
                    else
                    {
                        File_txt = File_txt + "3601," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                        File_txt = File_txt + "5601," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                    }

                    //AN
                    if (AN_count > 0)
                    {
                        AN_AVG = AN_AVG / AN_count;
                        File_txt = File_txt + "5602," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + AN_AVG.ToString("0.00") + "\n";

                        //算排放量
                        if (WasteFlow_count > 0)
                        {
                            double tmp = AN_AVG * WasteFlow_AVG * 0.71;
                            File_txt = File_txt + "3602," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + "," + tmp.ToString("0.00") + "\n";
                        }
                        else
                        {
                            File_txt = File_txt + "3602," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                        }
                    }
                    else
                    {
                        File_txt = File_txt + "3602," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                        File_txt = File_txt + "5602," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd") + ",0.00" + "\n";
                    }
                }
                command.Dispose();
                reader.Close();

                //============================

                sql_string = @"select Top 1 * FROM " + GlobalVariable.CPDCInfo.SQLDataCatelog + ".dbo.Regulate_Date" + @" where RecDateTime between @StartTime and @EndTime ";

                SqlCommand command2 = new SqlCommand(sql_string, conn);
                command2.Parameters.AddWithValue("@StartTime", StartTime);
                command2.Parameters.AddWithValue("@EndTime", EndTime);
                SqlDataReader reader2 = command2.ExecuteReader();

                if (reader2.HasRows)
                {
                    while (reader2.Read())
                    {
                        //低流速 0.00 0.07 0.08 0.06
                        double A = 0.04;
                        double B = Convert.ToDouble(reader2["Range1_Zero"]);
                        double C = B - A;
                        double D = (C / 0.3) * 100;

                        double E = 0.29;
                        double F = Convert.ToDouble(reader2["Range1_Full"]);
                        double G = F - E;
                        double H = (G / 100) * 100;

                        File_txt = File_txt + "A480," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd,0500") + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd,0500") + ",0.3,5," + B.ToString("#0.00") + "," + A.ToString("#0.00") + "," + C.ToString("#0.00") + "," + D.ToString("#0.00") + ",5," + F.ToString("#0.00") + "," + E.ToString("#0.00") + "," + G.ToString("#0.00") + "," + H.ToString("#0.00") + "\n";


                        //高流速
                        A = 0.4;
                        B = Convert.ToDouble(reader2["Range2_Zero"]);
                        C = B - A;
                        D = (C / 0.3) * 100;

                        E = 50;
                        F = Convert.ToDouble(reader2["Range2_Full"]);
                        G = F - E;
                        H = (G / 100) * 100;

                        File_txt = File_txt + "A480," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd,0500") + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd,0500") + ",100,5," + B.ToString("#0.00") + "," + A.ToString("#0.00") + "," + C.ToString("#0.00") + "," + D.ToString("#0.00") + ",5," + F.ToString("#0.00") + "," + E.ToString("#0.00") + "," + G.ToString("#0.00") + "," + H.ToString("#0.00") + "\n";
                    }
                }
                else
                {
                    File_txt = File_txt + "A480," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd,0500") + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd,0500") + ",0.3,5,0.00,0.00,0.00,0.00,5,0.00,0.00,0.00,0.00" + "\n";
                    File_txt = File_txt + "A480," + PolNo + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd,0500") + "," + taiwanCalendar.GetYear(dateTime.AddDays(-1)) + dateTime.AddDays(-1).ToString("MMdd,0500") + ",100,5,0.00,0.00,0.00,0.00,5,0.00,0.00,0.00,0.00" + "\n";
                }

                return File_txt;
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(),98);
                Log.LogWrite("日平均值計算 錯誤", 99);
                return "";
            }
        }


        public DataSet GetDataResult(string CommandString)
        {
            try
            {
                SqlCommand Command;
                using (Command = new SqlCommand(CommandString, conn))
                {
                    Command.CommandTimeout = 5;

                    DataSet datatable = new DataSet();
                    SqlDataAdapter dt;

                    dt = new SqlDataAdapter(CommandString, conn);
                    DateTime dateTime = DateTime.Now;
                    Command.Parameters.AddWithValue("@DateTime", dateTime.ToString("yyyy-MM-dd ") + "00:00:00");
                    dt.Fill(datatable);
                    dt.Dispose();

                    return datatable;
                }
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-取得平均值錯誤 失敗", 99);
                return null;
            }
        }

        public DataSet GetDataResult_Last(string CommandString)
        {
            try
            {
                SqlCommand Command;
                using (Command = new SqlCommand(CommandString, conn))
                {
                    Command.CommandTimeout = 5;

                    DataSet datatable = new DataSet();
                    SqlDataAdapter dt;

                    dt = new SqlDataAdapter(CommandString, conn);
                    DateTime dateTime = DateTime.Now;
                    if (Convert.ToInt16(dateTime.ToString("HH")) >= 7)
                    {
                        Command.Parameters.AddWithValue("@DateTime_End", dateTime.ToString("yyyy-MM-dd ") + "07:00:00");
                        Command.Parameters.AddWithValue("@DateTime", dateTime.AddDays(-1).ToString("yyyy-MM-dd ") + "07:00:00");
                    }
                    else
                    {
                        Command.Parameters.AddWithValue("@DateTime_End", dateTime.AddDays(-1).ToString("yyyy-MM-dd ") + "07:00:00");
                        Command.Parameters.AddWithValue("@DateTime", dateTime.AddDays(-2).ToString("yyyy-MM-dd ") + "07:00:00");
                    }
                    dt.Fill(datatable);
                    dt.Dispose();

                    return datatable;
                }
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-查詢SQL結果失敗 失敗", 99);
                return null;
            }
        }

        //SQL查詢單一值
        public string GetSQLResult(string CommandString)
        {
            string result = "0.0";
            SqlCommand Command = new SqlCommand(CommandString, conn);
            Command.CommandTimeout = 5;
            try
            {
                DateTime dateTime = DateTime.Now;
                Command.Parameters.AddWithValue("@DateTime", dateTime.ToString("yyyy-MM-dd ") + "00:00:00");
                result = Double.Parse(Command.ExecuteScalar().ToString()).ToString("#0.0");

            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite(CommandString, 98);
                Log.LogWrite("SQL-查詢SQL結果失敗 失敗", 99);
            }

            Command.Dispose();
            return result;
        }

        //SQL查詢單一值
        public string GetSQLResult_Last(string CommandString)
        {
            string result = "0.0";
            SqlCommand Command = new SqlCommand(CommandString, conn);
            Command.CommandTimeout = 5;
            try
            {
                DateTime dateTime = DateTime.Now;
                if (Convert.ToInt16(dateTime.ToString("HH")) >= 7)
                {
                    Command.Parameters.AddWithValue("@DateTime_End", dateTime.ToString("yyyy-MM-dd ") + "06:00:00");
                    Command.Parameters.AddWithValue("@DateTime", dateTime.AddDays(-1).ToString("yyyy-MM-dd ") + "07:00:00");
                }
                else
                {
                    Command.Parameters.AddWithValue("@DateTime_End", dateTime.AddDays(-1).ToString("yyyy-MM-dd ") + "06:00:00");
                    Command.Parameters.AddWithValue("@DateTime", dateTime.AddDays(-2).ToString("yyyy-MM-dd ") + "07:00:00");
                }
                result = Double.Parse(Command.ExecuteScalar().ToString()).ToString("#0.0");

            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite(CommandString, 98);
                Log.LogWrite("SQL-查詢SQL結果失敗 失敗", 99);
            }

            Command.Dispose();
            return result;
        }


        //20250206 0點昨日廢棄流量累積
        public string GetSQLResult_Last_Midnight(string CommandString)
        {
            string result = "0.0";
            SqlCommand Command = new SqlCommand(CommandString, conn);
            Command.CommandTimeout = 5;
            try
            {
                
                DateTime todayMidnight = DateTime.Today;            
                DateTime yesterdayMidnight = todayMidnight.AddDays(-1); 

                Command.Parameters.AddWithValue("@DateTime", yesterdayMidnight.ToString("yyyy-MM-dd HH:mm:ss"));
                Command.Parameters.AddWithValue("@DateTime_End", todayMidnight.ToString("yyyy-MM-dd HH:mm:ss"));

                result = Double.Parse(Command.ExecuteScalar().ToString()).ToString("#0.0");
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite(CommandString, 98);
                Log.LogWrite("SQL-查詢SQL結果失敗 失敗", 99);
            }
            Command.Dispose();
            return result;
        }

        //20230512 早上7點重製LNG?
        public string GetSQLResult_For_LNG_today(string CommandString)
        {
            string result = "0.0";
            SqlCommand Command = new SqlCommand(CommandString, conn);
            Command.CommandTimeout = 5;
            try
            {
                DateTime dateTime = DateTime.Now;
                if (Convert.ToInt16(dateTime.ToString("HH")) >= 7)
                {
                    Command.Parameters.AddWithValue("@DateTime", dateTime.ToString("yyyy-MM-dd 07:00:00"));
                }
                else
                {
                    Command.Parameters.AddWithValue("@DateTime", dateTime.AddDays(-1).ToString("yyyy-MM-dd 07:00:00"));
                }
                result = Double.Parse(Command.ExecuteScalar().ToString()).ToString("#0.0");

            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite(CommandString, 98);
                Log.LogWrite("SQL-查詢SQL結果失敗 失敗", 99);
            }

            Command.Dispose();
            return result;
        }

        //SQL新增平均值
        public void InsertAVGDate(DateTime dateTime, string table)
        {
            string CommandString = @"EXEC [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[" + table + @"]
                                     @IndexCode = @statuscode,
                                     @IndexProcessTime = @datetime";
            SqlCommand Command = new SqlCommand(CommandString, conn);
            Command.CommandTimeout = 5;
            try
            {
                Command.Parameters.AddWithValue("@statuscode", InitialFileControl.FileRead("CPDC", "Status", "null").Substring(0, 2));
                Command.Parameters.AddWithValue("@datetime", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));

                Command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-新增平均值錯誤 失敗", 99);
            }
            Command.Dispose();
        }

        //SQL新增校正值
        public void InsertRegulateDate(string range1_zero, string range2_zero, string range1_full, string range2_full)
        {
            string CommandString = @"Insert into [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[Regulate_Date] 
                                     ([CNo] ,[PolNo] ,[RecDateTime] ,[Range1_Zero] ,[Range1_Full] ,[Range2_Zero] ,[Range2_Full])
                                     VALUES(
                                     @CNo,@PolNo ,@RecDateTime ,@Range1_Zero ,@Range1_Full ,@Range2_Zero ,@Range2_Full)";
            SqlCommand Command = new SqlCommand(CommandString, conn);
            Command.CommandTimeout = 5;
            try
            {
                Command.Parameters.AddWithValue("@CNo", InitialFileControl.FileRead("Declare", "DeclareFlareNo", ""));
                Command.Parameters.AddWithValue("@PolNo", InitialFileControl.FileRead("Declare", "DeclarePolNo", ""));
                Command.Parameters.AddWithValue("@RecDateTime", GlobalVariable.CPDCInfo.Regulate_Range);
                Command.Parameters.AddWithValue("@Range1_Zero", decimal.Parse(range1_zero));
                Command.Parameters.AddWithValue("@Range2_Zero", decimal.Parse(range2_zero));
                Command.Parameters.AddWithValue("@Range1_Full", decimal.Parse(range1_full));
                Command.Parameters.AddWithValue("@Range2_Full", decimal.Parse(range2_full));

                Command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-新增平均值錯誤 失敗", 99);
            }
            Command.Dispose();
        }

        //20210826 程式內計算T15平均值
        //取出Rawdata and Rawdata_GC 15分鐘內之分鐘值 -> 判斷出各測項insert進T15表之數值and狀態碼
        public void Insert_T15(DateTime dateTime, string table)
        {
            //最後的陣列!!!!!!!!!
            object[] T15_value = new object[28];

            try
            {
                //Value13 ~ Value26 and Status13 ~ Status26 15分鐘內之數值
                string CommandString = @"SELECT Value13,Status13,Value14,Status14,Value15,Status15,Value16,Status16,Value17,Status17,Value18,Status18,Value19,Status19,Value20,Status20,Value21,Status21,Value22,Status22,Value23,Status23,Value24,Status24,Value25,Status25,Value26,Status26 " +
                                    @"FROM [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[RawData] 
                                     WHERE RecDateTime between @start_time and @end_time order by RecDateTime desc";

                SqlCommand Command = new SqlCommand(CommandString, conn);

                Command.Parameters.AddWithValue("@start_time", dateTime.AddMinutes(-15).ToString("yyyy-MM-dd HH:mm:00"));
                Command.Parameters.AddWithValue("@end_time", dateTime);

                Command.CommandTimeout = 5;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(Command);//從Command取得資料存入dataAdapter
                DataTable dt = new DataTable("TableName");//TableName可以隨便取
                dataAdapter.Fill(dt);



                object[][] LoadData = new object[dt.Rows.Count][];
                //將SQL結果放進二維陣列
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    LoadData[i] = new object[dt.Columns.Count];

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        LoadData[i][j] = dt.Rows[i][j].ToString();
                    }
                }

                //Value13 ~ Value26
                for (int column = 1; column < LoadData[0].GetLength(0); column = column + 2)
                {
                    int Useless_data = 0;
                    string[] Status = new string[LoadData.GetLength(0)];

                    //每個測項的15筆狀態碼
                    for (int Row = 0; Row < LoadData.GetLength(0); Row++)
                    {
                        if (LoadData[Row][column].ToString().Substring(2, 2) == "40")
                        {
                            Useless_data += 1;
                        }
                        string status = LoadData[Row][column].ToString();
                        Status[Row] = status; // 後面用來檢查Statue的眾數用
                    }

                    //該15分鐘 缺值太多
                    //Value = 全部平均 , Status = XX40
                    if (Useless_data >= 5)
                    {
                        T15_value[column] = InitialFileControl.FileRead("CPDC", "Status", "").Substring(0, 2) + "40";

                        float useless_Value_total = 0;
                        float useless_Value_count = 0;

                        //把所有非0數值加起來 and 計算筆數
                        for (int Row = 0; Row < LoadData.GetLength(0); Row++)
                        {
                            /*
                            if (LoadData[Row][column - 1].ToString() != "")
                            {
                                useless_Value_total += (float)LoadData[Row][column - 1];
                                useless_Value_count += 1;
                            }
                            */
                            if (LoadData[Row][column - 1].ToString() != "")
                            {
                                useless_Value_total += float.Parse(LoadData[Row][column - 1].ToString());
                                useless_Value_count += 1;
                            }
                        }
                        //檢查是否15筆都是null
                        if (useless_Value_total > 0 && useless_Value_count > 0)
                        {
                            T15_value[column - 1] = useless_Value_total / useless_Value_count;
                        }
                        else
                        {
                            T15_value[column - 1] = "";
                        }
                    }
                    //合格筆數夠 , 找Status眾數 and 該眾數之Value平均
                    else
                    {
                        //找出出現最多次的狀態碼
                        var temp = from p in Status
                                   group p by p.ToString() into g
                                   select new
                                   {
                                       g.Key,
                                       NumProducts = g.Count()
                                   };
                        string Most_status = "";
                        foreach (var x in temp)
                        {
                            Most_status = x.Key.ToString();
                            break;
                        }

                        //Status = 出現最多次狀態碼
                        T15_value[column] = Most_status;

                        float Value_total = 0;
                        float Value_count = 0;

                        //把所有 眾數之狀態碼 的 value 平均
                        for (int Row = 0; Row < LoadData.GetLength(0); Row++)
                        {
                            //該分鐘狀態碼為出現最多次之狀態碼 and 數值不為空
                            if (LoadData[Row][column].ToString() == Most_status && LoadData[Row][column - 1].ToString() != "")
                            {
                                Value_total += Convert.ToSingle(LoadData[Row][column - 1]);
                                Value_count += 1;
                            }
                        }
                        
                        //假如眾數是使用者設定 NA20 but NA20 數值皆是null 無法算平均
                        if(Value_count != 0)
                        {
                            T15_value[column - 1] = Value_total / Value_count;
                        }
                        else
                        {
                            T15_value[column - 1] = "";
                        }
                    }
                }
                Command.Dispose();
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-計算RawData_T15 失敗", 99);
            }

            //=====================================================================
            //最後的GC數值!!!!!!!!!!!!!!!
            object[] T15_value_GC = new object[24];
            try
            {
                string CommandString = @"SELECT TOP 1 Value1,Status1,Value2,Status2,Value3,Status3,Value4,Status4,Value5,Status5,Value6,Status6,Value7,Status7,Value8,Status8,Value9,Status9,Value10,Status10,Value11,Status11,Value12,Status12 " +
                                    @"FROM [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[RawData_GC] 
                                     WHERE RecDateTime between @start_time and @end_time order by RecDateTime desc";

                SqlCommand Command = new SqlCommand(CommandString, conn);
                Command.CommandTimeout = 5;

                Command.Parameters.AddWithValue("@start_time", dateTime.AddSeconds(-1 * dateTime.Second).AddMinutes(-15));
                Command.Parameters.AddWithValue("@end_time", dateTime);

                SqlDataAdapter dataAdapter = new SqlDataAdapter(Command);//從Command取得資料存入dataAdapter
                DataTable dt = new DataTable("TableName");//TableName可以隨便取
                dataAdapter.Fill(dt);

                //將SQL結果放進二維陣列
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        T15_value_GC[j] = dt.Rows[i][j].ToString();
                    }
                }
                Command.Dispose();

            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-計算RawData_GC_T15 失敗", 99);
            }

            //=====================================================================

            //最後的GC數值!!!!!!!!!!!!!!!
            object[] T15_value_GC_heat = new object[8];
            try
            {
                string CommandString = @"SELECT TOP 1 Value27,Status27,Value28,Status28,Value29,Status29,Value30,Status30 " +
                                    @"FROM [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[RawData_GC] 
                                     WHERE RecDateTime between @start_time and @end_time order by RecDateTime desc";

                SqlCommand Command = new SqlCommand(CommandString, conn);
                Command.CommandTimeout = 5;

                Command.Parameters.AddWithValue("@start_time", dateTime.AddSeconds(-1 * dateTime.Second).AddMinutes(-15));
                Command.Parameters.AddWithValue("@end_time", dateTime);

                SqlDataAdapter dataAdapter = new SqlDataAdapter(Command);//從Command取得資料存入dataAdapter
                DataTable dt = new DataTable("TableName");//TableName可以隨便取
                dataAdapter.Fill(dt);

                //將SQL結果放進二維陣列
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        T15_value_GC_heat[j] = dt.Rows[i][j].ToString();
                    }
                }
                Command.Dispose();

            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-計算RawData_GC_T15 熱值 失敗", 99);
            }


            //合併成完整T15 所有測項的數值
            object[] Value_Array = {InitialFileControl.FileRead("Declare", "DeclareFlareNo", "") , InitialFileControl.FileRead( "Declare", "DeclarePolNo", "") , dateTime.AddMinutes(-15).ToString("yyyy-MM-dd HH:mm") };
            Value_Array = Value_Array.Concat(T15_value_GC).ToArray();
            Value_Array = Value_Array.Concat(T15_value).ToArray();
            Value_Array = Value_Array.Concat(T15_value_GC_heat).ToArray();
            

            NewInsert_AVG("T15" , Value_Array);
            Temporary_T15();
        }

        //20210909 程式內計算T60平均值
        //取出 AVG_T15 15分鐘內之分鐘值 -> 判斷出各測項insert進T60表之數值and狀態碼
        public void Insert_T60(DateTime dateTime, string table)
        {
            //最後的陣列!!!!!!!!!
            object[] T60_value = new object[52];

            try
            {
                //Value1 ~ Value26 and Status1 ~ Status26 60分鐘內之數值
                string CommandString = @"SELECT Value1,Status1,Value2,Status2,Value3,Status3,Value4,Status4,Value5,Status5,Value6,Status6,Value7,Status7,Value8,Status8,Value9,Status9,Value10,Status10,Value11,Status11,Value12,Status12,
                                    Value13,Status13,Value14,Status14,Value15,Status15,Value16,Status16,Value17,Status17,Value18,Status18,Value19,Status19,Value20,Status20,Value21,Status21,Value22,Status22,Value23,Status23,Value24,Status24,Value25,Status25,Value26,Status26 " +
                                    @"FROM [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[AVG_T15] 
                                     WHERE RecDateTime between @start_time and @end_time order by RecDateTime desc";

                SqlCommand Command = new SqlCommand(CommandString, conn);

                Command.Parameters.AddWithValue("@start_time", dateTime.AddHours(-1).ToString("yyyy-MM-dd HH:00"));
                Command.Parameters.AddWithValue("@end_time", dateTime);

                Command.CommandTimeout = 5;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(Command);//從Command取得資料存入dataAdapter
                DataTable dt = new DataTable("TableName");//TableName可以隨便取
                dataAdapter.Fill(dt);

                object[][] LoadData = new object[dt.Rows.Count][];
                //將SQL結果放進二維陣列
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    LoadData[i] = new object[dt.Columns.Count];

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        LoadData[i][j] = dt.Rows[i][j].ToString();
                    }
                }

                //Value1 ~ Value26
                for (int column = 1; column < LoadData[0].GetLength(0); column = column + 2)
                {
                    int Useless_data = 0;
                    string[] Status = new string[LoadData.GetLength(0)];

                    //每個測項的4筆狀態碼
                    for (int Row = 0; Row < LoadData.GetLength(0); Row++)
                    {
                        if (LoadData[Row][column].ToString().Substring(2, 2) == "40")
                        {
                            Useless_data += 1;
                        }
                        string status = LoadData[Row][column].ToString();
                        Status[Row] = status; // 後面用來檢查Statue的眾數用
                    }

                    //該60分鐘 缺值太多
                    //Value = 全部平均 , Status = XX40
                    if (Useless_data >= 1)
                    {
                        T60_value[column] = InitialFileControl.FileRead("CPDC", "Status", "").Substring(0, 2) + "40";

                        float useless_Value_total = 0;
                        float useless_Value_count = 0;

                        //把所有非0數值加起來 and 計算筆數
                        /*
                        for (int Row = 0; Row < LoadData.GetLength(0); Row++)
                        {
                            if (LoadData[Row][column - 1].ToString() != "")
                            {
                                useless_Value_total += (float)LoadData[Row][column - 1];
                                useless_Value_count += 1;
                            }
                        }
                        */
                        for (int Row = 0; Row < LoadData.GetLength(0); Row++)
                        {
                            if (LoadData[Row][column - 1].ToString() != "")
                            {
                                useless_Value_total += float.Parse(LoadData[Row][column - 1].ToString());
                                useless_Value_count += 1;
                            }
                        }


                        //檢查是否4筆都是null -> 總和沒東西
                        if (useless_Value_total > 0 && useless_Value_count > 0)
                        {
                            T60_value[column - 1] = useless_Value_total / useless_Value_count;
                        }
                        else
                        {
                            T60_value[column - 1] = "";
                        }
                    }
                    //合格筆數夠 , 找Status眾數 and 該眾數之Value平均
                    else
                    {
                        //找出出現最多次的狀態碼
                        var temp = from p in Status
                                   group p by p.ToString() into g
                                   select new
                                   {
                                       g.Key,
                                       NumProducts = g.Count()
                                   };
                        string Most_status = "";
                        foreach (var x in temp)
                        {
                            Most_status = x.Key.ToString();
                            break; 
                        }

                        //Status = 出現最多次狀態碼
                        T60_value[column] = Most_status;

                        float Value_total = 0;
                        float Value_count = 0;

                        //把所有 眾數之狀態碼 的 value 平均
                        for (int Row = 0; Row < LoadData.GetLength(0); Row++)
                        {
                            //該分鐘狀態碼為出現最多次之狀態碼 and 數值不為空
                            if (LoadData[Row][column].ToString() == Most_status && LoadData[Row][column - 1].ToString() != "")
                            {
                                Value_total += Convert.ToSingle(LoadData[Row][column - 1]);
                                Value_count += 1;
                            }
                        }

                        //假如眾數是使用者設定 NA20 but NA20 數值皆是null 無法算平均
                        if (Value_count != 0)
                        {
                            T60_value[column - 1] = Value_total / Value_count;
                        }
                        else
                        {
                            T60_value[column - 1] = "";
                        }
                    }
                }
                Command.Dispose();
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-計算RawData_T60 失敗", 99);
            }

            //===============================================
            //算T60的熱值
            object[] T60_value_heat = new object[8];

            try
            {
                //Value1 ~ Value26 and Status1 ~ Status26 60分鐘內之數值
                string CommandString = @"SELECT isnull(SUM(Value27),0) , isnull(SUM(Value28),0) , isnull(SUM(Value29),0) , isnull(SUM(Value30),0) " +
                                    @"FROM [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[AVG_T15] 
                                     WHERE RecDateTime between @start_time and @end_time";

                SqlCommand Command = new SqlCommand(CommandString, conn);

                //Command.Parameters.AddWithValue("@start_time", dateTime.AddSeconds(-1 * dateTime.Second).AddHours(-1));
                Command.Parameters.AddWithValue("@start_time", dateTime.AddHours(-1).ToString("yyyy-MM-dd HH:00"));
                Command.Parameters.AddWithValue("@end_time", dateTime);

                Command.CommandTimeout = 5;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(Command);//從Command取得資料存入dataAdapter
                DataTable dt = new DataTable("TableName");//TableName可以隨便取
                dataAdapter.Fill(dt);


                //將SQL結果放進二維陣列
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        T60_value_heat[j*2] = dt.Rows[i][j].ToString();
                        T60_value_heat[j*2 + 1] = "NA10";
                    }
                }
                
                Command.Dispose();
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-計算RawData_T60 失敗", 99);
            }

            //合併成完整T60 所有測項的數值
            object[] Value_Array = { InitialFileControl.FileRead("Declare", "DeclareFlareNo", ""), InitialFileControl.FileRead("Declare", "DeclarePolNo", ""), dateTime.AddHours(-1).ToString("yyyy-MM-dd HH:mm") };
            Value_Array = Value_Array.Concat(T60_value).ToArray();
            Value_Array = Value_Array.Concat(T60_value_heat).ToArray();

            NewInsert_AVG("T60", Value_Array);
            Temporary_T60();
        }

        public void Cal_AVG(DateTime dateTime, string table)
        {
            //最後的陣列!!!!!!!!!
            object[] T15_value = new object[28];

            try
            {
                //Value1,Status1 ... Value26,Status26
                string ColumnString = "";
                for (int i = 0; i < GlobalVariable.SQL.InsertToAll.GetLength(0); i++)
                {
                    ColumnString += "Value" + GlobalVariable.SQL.InsertToAll[i, 0] + ",";

                    //20210817 加入status
                    ColumnString += "Status" + GlobalVariable.SQL.InsertToAll[i, 0] + ",";
                }
                ColumnString = ColumnString.Substring(0, ColumnString.Length - 1);


                //看計算T60 or T1440 決定去哪張表找資料
                string CommandString = "";
                if (table == "T60")
                {
                    //從AVG_T15 select Value1 ~ Value26 and Status1 ~ Status26 1小時內之數值
                    CommandString = @"SELECT " + ColumnString +
                                        @" FROM [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[AVG_T15] 
                                     WHERE RecDateTime between @start_time and @end_time order by RecDateTime desc";

                }
                else if(table == "T1440")
                {
                    //從AVG_T60 select Value1 ~ Value26 and Status1 ~ Status26 24小時內之數值
                    CommandString = @"SELECT " + ColumnString +
                                        @" FROM [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[AVG_T60] 
                                     WHERE RecDateTime between @start_time and @end_time order by RecDateTime desc";
                }

                SqlCommand Command = new SqlCommand(CommandString, conn);

                //設置起點 and 終點時間
                if(table == "T60")
                {
                    Command.Parameters.AddWithValue("@start_time", dateTime.AddHours(-1));
                    Command.Parameters.AddWithValue("@end_time", dateTime);
                }
                else if (table == "T1440")
                {
                    Command.Parameters.AddWithValue("@start_time", dateTime.AddHours(-24));
                    Command.Parameters.AddWithValue("@end_time", dateTime);
                }

                Command.CommandTimeout = 5;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(Command);//從Command取得資料存入dataAdapter
                DataTable dt = new DataTable("TableName");//TableName可以隨便取
                dataAdapter.Fill(dt);

                //將SQL結果放進二維陣列
                object[][] LoadData = new object[dt.Rows.Count][];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    LoadData[i] = new object[dt.Columns.Count];

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        LoadData[i][j] = dt.Rows[i][j].ToString();
                    }
                }

                //Value13 ~ Value26
                for (int column = 1; column < LoadData[0].GetLength(0); column = column + 2)
                {
                    int Useless_data = 0;
                    string[] Status = new string[LoadData.GetLength(0)];

                    //每個測項的15筆狀態碼
                    for (int Row = 0; Row < LoadData.GetLength(0); Row++)
                    {
                        if (LoadData[Row][column].ToString().Substring(2, 2) == "40")
                        {
                            Useless_data += 1;
                        }
                        string status = LoadData[Row][column].ToString();
                        Status[Row] = status; // 後面用來檢查Statue的眾數用
                    }

                    //該15分鐘 缺值太多
                    //Value = 全部平均 , Status = XX40
                    if (Useless_data >= 5)
                    {
                        T15_value[column] = InitialFileControl.FileRead("CPDC", "Status", "").Substring(0, 2) + "40";

                        float useless_Value_total = 0;
                        float useless_Value_count = 0;

                        //把所有非0數值加起來 and 計算筆數
                        for (int Row = 0; Row < LoadData.GetLength(0); Row++)
                        {
                            if (LoadData[Row][column - 1].ToString() != "")
                            {
                                useless_Value_total += (float)LoadData[Row][column - 1];
                                useless_Value_count += 1;
                            }
                        }
                        //檢查是否15筆都是null
                        if (useless_Value_total > 0 && useless_Value_count > 0)
                        {
                            T15_value[column - 1] = useless_Value_total / useless_Value_count;
                        }
                        else
                        {
                            T15_value[column - 1] = "";
                        }
                    }
                    //合格筆數夠 , 找Status眾數 and 該眾數之Value平均
                    else
                    {
                        //找出出現最多次的狀態碼
                        var temp = from p in Status
                                   group p by p.ToString() into g
                                   select new
                                   {
                                       g.Key,
                                       NumProducts = g.Count()
                                   };
                        string Most_status = "";
                        foreach (var x in temp)
                        {
                            Most_status = x.Key.ToString();
                            break;
                        }

                        //Status = 出現最多次狀態碼
                        T15_value[column] = Most_status;

                        float Value_total = 0;
                        float Value_count = 0;

                        //把所有 眾數之狀態碼 的 value 平均
                        for (int Row = 0; Row < LoadData.GetLength(0); Row++)
                        {
                            //該分鐘狀態碼為出現最多次之狀態碼 and 數值不為空
                            if (LoadData[Row][column].ToString() == Most_status && LoadData[Row][column - 1].ToString() != "")
                            {
                                Value_total += Convert.ToSingle(LoadData[Row][column - 1]);
                                Value_count += 1;
                            }
                        }

                        //假如眾數是使用者設定 NA20 but NA20 數值皆是null 無法算平均
                        if (Value_count != 0)
                        {
                            T15_value[column - 1] = Value_total / Value_count;
                        }
                        else
                        {
                            T15_value[column - 1] = "";
                        }
                    }
                }
                Command.Dispose();
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-計算RawData_T15 失敗", 99);
            }

            //最後的GC數值!!!!!!!!!!!!!!!
            object[] T15_value_GC = new object[24];
            try
            {
                string CommandString = @"SELECT TOP 1 Value1,Status1,Value2,Status2,Value3,Status3,Value4,Status4,Value5,Status5,Value6,Status6,Value7,Status7,Value8,Status8,Value9,Status9,Value10,Status10,Value11,Status11,Value12,Status12 " +
                                    @"FROM [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[RawData_GC] 
                                     WHERE RecDateTime between @start_time and @end_time order by RecDateTime desc";

                SqlCommand Command = new SqlCommand(CommandString, conn);
                Command.CommandTimeout = 5;

                Command.Parameters.AddWithValue("@start_time", dateTime.AddMinutes(-15));
                Command.Parameters.AddWithValue("@end_time", dateTime);

                SqlDataAdapter dataAdapter = new SqlDataAdapter(Command);//從Command取得資料存入dataAdapter
                DataTable dt = new DataTable("TableName");//TableName可以隨便取
                dataAdapter.Fill(dt);

                object[][] LoadData = new object[dt.Rows.Count][];
                //將SQL結果放進二維陣列
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    LoadData[i] = new object[dt.Columns.Count];

                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        T15_value_GC[j] = dt.Rows[i][j].ToString();
                    }
                }
                Command.Dispose();
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-計算RawData_GC_T15 失敗", 99);
            }

            //合併成完整T15 所有測項的數值
            object[] Value_Array = { InitialFileControl.FileRead("Declare", "DeclareFlareNo", ""), InitialFileControl.FileRead("Declare", "DeclarePolNo", ""), dateTime.ToString("yyyy-MM-dd HH:mm") };
            Value_Array = Value_Array.Concat(T15_value_GC).ToArray();
            Value_Array = Value_Array.Concat(T15_value).ToArray();

            NewInsert_AVG("T15", Value_Array);
        }

        //20210826 將計算T15 T60 T1440 移至程式內
        //傳入表的名稱,所有資料
        public void NewInsert_AVG( string Table_name , object[] Value)
        {
            try
            {
                string InsertSQLTable = "", SQLDataValueString = "", ColumnString = "";//1.寫入表格  2.Parameters 3.SQL欄位
                int count = 0;//計數

                //依類型取得對應欄位數值
                switch (Table_name)
                {
                    case "T15":
                        InsertSQLTable = "AVG_T15";
                        break;
                    case "T60":
                        InsertSQLTable = "AVG_T60";
                        break;
                }

                //@CNo,@PolNo ...
                SQLDataValueString = GlobalVariable.CPDCInfo.SQLDataValueString_All;

                //組合sql語句 Value1,Status1 ... Value26,Status26
                for (int i = 0; i < GlobalVariable.SQL.InsertToAll.GetLength(0); i++)
                {
                    ColumnString += "Value" + GlobalVariable.SQL.InsertToAll[i, 0] + ",";

                    //20210817 加入status
                    ColumnString += "Status" + GlobalVariable.SQL.InsertToAll[i, 0] + ",";
                }

                string CommandString = @"Insert Into " + GlobalVariable.CPDCInfo.SQLDataCatelog + ".dbo. " + InsertSQLTable + " (CNo,PolNo,RecDateTime," + ColumnString;
                CommandString = CommandString.Substring(0, CommandString.Length - 1) + ")Values(" + SQLDataValueString + ")";

                SqlCommand Command = new SqlCommand(CommandString, conn);
                Command.CommandTimeout = 5;

                //

                //SET VALUE by 傳入的陣列
                string[] ColumnArray = SQLDataValueString.Split(',');
                foreach (string Key in ColumnArray)
                {
                    // 20210817 新增判斷 @StatusXXXXX
                    if (Key == "@CNo" || Key == "@PolNo" || Key == "@RecDateTime" || (Key.Length >= 7 && Key.Substring(0, 7) == "@Status"))
                    {
                        Command.Parameters.AddWithValue(Key, Value[count]);
                    }
                    else
                    {
                        //Set Value 若為空 or 0.0 set Null (For SQL)
                        if (Value[count].ToString() == null || Value[count].ToString() == "" || Value[count].ToString() == "0" || Value[count].ToString() == "0.0" || Value[count].ToString() == "0.00")
                            Command.Parameters.AddWithValue(Key, DBNull.Value);
                        else
                            Command.Parameters.AddWithValue(Key, decimal.Parse(Value[count].ToString()));
                    }
                    count++;
                }

                try
                {
                    if (conn != null && conn.State != ConnectionState.Closed)
                    {
                        Command.ExecuteNonQuery();
                    }
                    else
                    {
                        GlobalVariable.SQL.Status = SQLConnect(1);
                        Command.ExecuteNonQuery();
                    }

                    Command.Dispose();
                    DataColletionSystem.ListBoxAQILogWrite("Process CPDC " + Table_name + "insert.");
                }
                catch (Exception e)
                {
                    Command.Parameters.Clear();
                    if (!e.Message.Contains("重複的索引鍵"))
                    {
                        Log.LogWrite(e.ToString(), 98);
                        Log.LogWrite("SQL-新增資料 失敗 重複的索引鍵", 99);
                        Log.LogWrite(Table_name + "----" + CommandString, 99);
                    }
                    else if (e.Message.Contains("重複的索引鍵"))
                    {
                        Log.LogWrite(e.ToString(), 98);
                        Log.LogWrite("SQL-新增資料 失敗 其他", 99);
                        Log.LogWrite(Table_name + "----" + CommandString, 99);
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogWrite(e.ToString(), 98);
                Log.LogWrite("SQL-新增資料 失敗", 99);
                Log.LogWrite(Table_name + "----" , 99);
            }
        }
        public void InsertRawData_Temporary(decimal C1, decimal C2, decimal C3, decimal C4, decimal C5, decimal HCN, decimal C3H6, decimal ACN, decimal AN, decimal GHV, decimal TMP)
        {
            DateTime RecDateTime = DateTime.Now;
            RecDateTime = new DateTime(RecDateTime.Year, RecDateTime.Month, RecDateTime.Day, RecDateTime.Hour, RecDateTime.Minute, 0);

            string CommandString = @"INSERT INTO [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[RawData_Temporary] 
                         ([RecDateTime], [C1], [C2], [C3], [C4], [C5], [HCN], [C3H6], [ACN], [AN], [GHV], [TMP])
                         VALUES (@RecDateTime, @C1, @C2, @C3, @C4, @C5, @HCN, @C3H6, @ACN, @AN, @GHV, @TMP)";
            


            try
            {
                using (SqlCommand Command = new SqlCommand(CommandString, conn))
                {
                    Command.CommandTimeout = 5; 

                    Command.Parameters.AddWithValue("@RecDateTime", RecDateTime);
                    Command.Parameters.AddWithValue("@C1", C1);
                    Command.Parameters.AddWithValue("@C2", C2);
                    Command.Parameters.AddWithValue("@C3", C3);
                    Command.Parameters.AddWithValue("@C4", C4);
                    Command.Parameters.AddWithValue("@C5", C5);
                    Command.Parameters.AddWithValue("@HCN", HCN);
                    Command.Parameters.AddWithValue("@C3H6", C3H6);
                    Command.Parameters.AddWithValue("@ACN", ACN);
                    Command.Parameters.AddWithValue("@AN", AN);
                    Command.Parameters.AddWithValue("@GHV", GHV);
                    Command.Parameters.AddWithValue("@TMP", TMP);

                    Command.ExecuteNonQuery();
                }
            }
            catch (SqlException sqlEx)
            {
                Log.LogWrite(sqlEx.ToString(), 98);
                Log.LogWrite("SQL-插入臨時原始數據錯誤", 99);
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("SQL-插入臨時原始數據時發生未預期的錯誤", 99);
            }
        }

        public void Temporary_T15()
        {
            string selectCommandString = @"SELECT 
                                   AVG(C1) AS AvgC1,
                                   AVG(C2) AS AvgC2,
                                   AVG(C3) AS AvgC3,
                                   AVG(C4) AS AvgC4,
                                   AVG(C5) AS AvgC5,
                                   AVG(HCN) AS AvgHCN,
                                   AVG(C3H6) AS AvgC3H6,
                                   AVG(ACN) AS AvgACN,
                                   AVG(AN) AS AvgAN,
                                   AVG(GHV) AS AvgGHV,
                                   AVG(TMP) AS AvgTMP
                                FROM [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[RawData_Temporary] 
                                WHERE RecDateTime BETWEEN @start_time AND @end_time";

            DateTime dateTime = DateTime.Now;
            dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
            DateTime endTime = dateTime; // 當前時間
            DateTime startTime = endTime.AddMinutes(-15); // 15分鐘前的時間

            try
            {
                using (SqlCommand selectCommand = new SqlCommand(selectCommandString, conn))
                {
                    selectCommand.Parameters.AddWithValue("@start_time", startTime);
                    selectCommand.Parameters.AddWithValue("@end_time", endTime);

                    
                    decimal AvgC1 = 0, AvgC2 = 0, AvgC3 = 0, AvgC4 = 0, AvgC5 = 0, AvgHCN = 0, AvgC3H6 = 0, AvgACN = 0, AvgAN = 0, AvgGHV = 0, AvgTMP = 0;

                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            AvgC1 = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            AvgC2 = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                            AvgC3 = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                            AvgC4 = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                            AvgC5 = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                            AvgHCN = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                            AvgC3H6 = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                            AvgACN = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);
                            AvgAN = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
                            AvgGHV = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                            AvgTMP = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);
                        }
                    }

                    
                    string insertCommandString = @"INSERT INTO [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[Temporary_T15]
                                         (CNo, PolNo, RecDateTime, C1, C2, C3, C4, C5, HCN, C3H6, ACN, AN, GHV, TMP)
                                         VALUES (@CNo, @PolNo, @RecDateTime, @C1, @C2, @C3, @C4, @C5, @HCN, @C3H6, @ACN, @AN, @GHV, @TMP)";

                    using (SqlCommand insertCommand = new SqlCommand(insertCommandString, conn))
                    {
                        insertCommand.Parameters.AddWithValue("@CNo", "S2300410");
                        insertCommand.Parameters.AddWithValue("@PolNo", "A002");
                        insertCommand.Parameters.AddWithValue("@RecDateTime", endTime);
                        insertCommand.Parameters.AddWithValue("@C1", AvgC1);
                        insertCommand.Parameters.AddWithValue("@C2", AvgC2);
                        insertCommand.Parameters.AddWithValue("@C3", AvgC3);
                        insertCommand.Parameters.AddWithValue("@C4", AvgC4);
                        insertCommand.Parameters.AddWithValue("@C5", AvgC5);
                        insertCommand.Parameters.AddWithValue("@HCN", AvgHCN);
                        insertCommand.Parameters.AddWithValue("@C3H6", AvgC3H6);
                        insertCommand.Parameters.AddWithValue("@ACN", AvgACN);
                        insertCommand.Parameters.AddWithValue("@AN", AvgAN);
                        insertCommand.Parameters.AddWithValue("@GHV", AvgGHV);
                        insertCommand.Parameters.AddWithValue("@TMP", AvgTMP);

                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("SQL-插入臨時原始數據T15錯誤", 99);
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("SQL-插入臨時原始數據T15錯誤", 99);
            }
        }
        public void Temporary_T60()
        {
            string selectCommandString = @"SELECT 
                                   AVG(C1) AS AvgC1,
                                   AVG(C2) AS AvgC2,
                                   AVG(C3) AS AvgC3,
                                   AVG(C4) AS AvgC4,
                                   AVG(C5) AS AvgC5,
                                   AVG(HCN) AS AvgHCN,
                                   AVG(C3H6) AS AvgC3H6,
                                   AVG(ACN) AS AvgACN,
                                   AVG(AN) AS AvgAN,
                                   AVG(GHV) AS AvgGHV,
                                   AVG(TMP) AS AvgTMP
                                FROM [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[RawData_Temporary] 
                                WHERE RecDateTime BETWEEN @start_time AND @end_time";

            DateTime dateTime = DateTime.Now;
            dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
            DateTime endTime = dateTime; // 當前時間
            DateTime startTime = endTime.AddMinutes(-60); // 60分鐘前的時間

            try
            {
                using (SqlCommand selectCommand = new SqlCommand(selectCommandString, conn))
                {
                    selectCommand.Parameters.AddWithValue("@start_time", startTime);
                    selectCommand.Parameters.AddWithValue("@end_time", endTime);


                    decimal AvgC1 = 0, AvgC2 = 0, AvgC3 = 0, AvgC4 = 0, AvgC5 = 0, AvgHCN = 0, AvgC3H6 = 0, AvgACN = 0, AvgAN = 0, AvgGHV = 0, AvgTMP = 0;

                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            AvgC1 = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            AvgC2 = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                            AvgC3 = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                            AvgC4 = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                            AvgC5 = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                            AvgHCN = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                            AvgC3H6 = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                            AvgACN = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);
                            AvgAN = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8);
                            AvgGHV = reader.IsDBNull(9) ? 0 : reader.GetDecimal(9);
                            AvgTMP = reader.IsDBNull(10) ? 0 : reader.GetDecimal(10);
                        }
                    }


                    string insertCommandString = @"INSERT INTO [" + GlobalVariable.CPDCInfo.SQLDataCatelog + @"].[dbo].[Temporary_T60]
                                         (CNo, PolNo, RecDateTime, C1, C2, C3, C4, C5, HCN, C3H6, ACN, AN, GHV, TMP)
                                         VALUES (@CNo, @PolNo, @RecDateTime, @C1, @C2, @C3, @C4, @C5, @HCN, @C3H6, @ACN, @AN, @GHV, @TMP)";

                    using (SqlCommand insertCommand = new SqlCommand(insertCommandString, conn))
                    {
                        insertCommand.Parameters.AddWithValue("@CNo", "S2300410");
                        insertCommand.Parameters.AddWithValue("@PolNo", "A002");
                        insertCommand.Parameters.AddWithValue("@RecDateTime", endTime);
                        insertCommand.Parameters.AddWithValue("@C1", AvgC1);
                        insertCommand.Parameters.AddWithValue("@C2", AvgC2);
                        insertCommand.Parameters.AddWithValue("@C3", AvgC3);
                        insertCommand.Parameters.AddWithValue("@C4", AvgC4);
                        insertCommand.Parameters.AddWithValue("@C5", AvgC5);
                        insertCommand.Parameters.AddWithValue("@HCN", AvgHCN);
                        insertCommand.Parameters.AddWithValue("@C3H6", AvgC3H6);
                        insertCommand.Parameters.AddWithValue("@ACN", AvgACN);
                        insertCommand.Parameters.AddWithValue("@AN", AvgAN);
                        insertCommand.Parameters.AddWithValue("@GHV", AvgGHV);
                        insertCommand.Parameters.AddWithValue("@TMP", AvgTMP);

                        insertCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("SQL-插入臨時原始數據T60錯誤", 99);
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("SQL-插入臨時原始數據T60錯誤", 99);
            }
        }


    }
}