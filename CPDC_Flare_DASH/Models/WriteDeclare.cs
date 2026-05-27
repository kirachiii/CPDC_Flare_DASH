using System;
using System.Data;
using System.Globalization;
using System.IO;

namespace CPDC_Flare_DASH.Models
{
    class WriteDeclare
    {
        byte[] Byte = { 0x04 };
        public bool FileCreate(string Path, bool ForceCreate)
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

        //20211025 產生舊檔
        public void Create_old_file(string text, DateTime dateTime)
        {
            string FileName = @"D:\ReportOutput\FL" + dateTime.ToString("MMddHHmm") + @".S40";
            FileCreate(FileName, false);

            //
            string[] tmp = text.Split('\n');


            tmp[0] = tmp[0] + '\n';
            tmp[0] = tmp[0].Replace("V109", "");
            tmp[0] = tmp[0].Replace(",", "");
            //for (int i = 1; i < tmp.Length - 1; i++)
            //{
            //    tmp[i] = tmp[i] + '\n';
            //    tmp[i] = tmp[i].Replace("V109", "");
            //    tmp[i] = tmp[i].Replace(",", "");

            //    string tmp1 = "";
            //    int start = tmp[i].IndexOf("N");//找a的位置
            //    int end = tmp[i].IndexOf("\n");//找b的位置
            //    tmp1 = (tmp[i].Substring(start)).Substring(0, end - start);//找出a和b之間的字串
            //    tmp[i] = tmp[i].Replace(tmp1, "    10");
            //}
            //2024-10-30 暫時使用
            for (int i = 1; i < tmp.Length - 1; i++)
            {
                tmp[i] = tmp[i] + '\n';
                tmp[i] = tmp[i].Replace("V109", "");
                tmp[i] = tmp[i].Replace(",", "");

                string tmp1 = "";
                int start = tmp[i].IndexOf("N");
                int end = tmp[i].IndexOf("\n");

                // 判斷 start 和 end 是否有效
                if (start != -1 && end != -1 && end > start)
                {
                    tmp1 = tmp[i].Substring(start, end - start);
                    tmp[i] = tmp[i].Replace(tmp1, "    10");
                }
            }



            string final_string = "";
            for (int i = 0; i < tmp.Length - 1; i++)
            {
                final_string += tmp[i];
            }

            


            text = text.Replace("V109", "");
            text = text.Replace(",", "");
            text = text.Replace("NA40", "    10");
            text = text.Replace("NA", "    ");

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(FileName, true))
            {
                file.Write(final_string + System.Text.Encoding.ASCII.GetString(Byte));
                file.Dispose();
                file.Close();
            }
        }



        public void WriteFile_T15(DateTime dateTime, SQLControl SQLControl)
        {
            try
            {
                string FlareNo = InitialFileControl.FileRead("Declare", "DeclareFlareNo", "");
                string PolNo = InitialFileControl.FileRead("Declare", "DeclarePolNo", "");
                string CNo = InitialFileControl.FileRead("Declare", "DeclareCNo", "");
                string Version = InitialFileControl.FileRead("Declare", "DeclareVer", "");
                string Status = InitialFileControl.FileRead("CPDC", "Status", "00");
                TaiwanCalendar taiwanCalendar = new TaiwanCalendar();
                string FileName = "";

                if (PolNo == "" || CNo == "")
                    Log.LogWrite("尚未設定燃燒塔編號或公私場所碼", 99);
                else
                {
                    FileName = InitialFileControl.FileRead("Declare", "DeclareFile", "") + @"\FL" + (dateTime.Year - 1911).ToString() + dateTime.ToString("MMddHHmm") + "." + CNo;

                    FileCreate(FileName, false);
                    string writetitle = "1000," + FlareNo + ",FLR," + Version + "\n";
                    string writestring = "";
                    string SQLString = "";

                    //20210903 新增select 狀態碼
                    string SQLString_test = "";

                    using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(FileName, true))
                    {
                        for (int i = 0; i < GlobalVariable.Declare.DeclareItem.GetLength(0); i++)
                        {
                            if (GlobalVariable.Declare.DeclareItem[i, 1] == "Checked")
                            {
                                for (int j = 0; j < GlobalVariable.SQL.InsertToRawGC.GetLength(0); j++)
                                {
                                    if (GlobalVariable.SQL.InsertToRawGC[j, 1].Equals(GlobalVariable.Declare.DeclareItem[i, 0]))
                                    {
                                        SQLString += "cast(round(isnull(AVG(Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + "),0),2) as numeric(15,2)) AS Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",Status" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",";

                                        //20210903 T15就一筆幹麻用AVG?
                                        SQLString_test += "cast(round(isnull(Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",0),2) as numeric(15,2)) AS Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",Status" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",";
                                    }
                                }
                                for (int j = 0; j < GlobalVariable.SQL.InsertToRaw.GetLength(0); j++)
                                {
                                    if (GlobalVariable.SQL.InsertToRaw[j, 1].Equals(GlobalVariable.Declare.DeclareItem[i, 0]))
                                    {
                                        SQLString += "cast(round(isnull(AVG(Value" + GlobalVariable.SQL.InsertToRaw[j, 0] + "),0),2) as numeric(15,2)) AS Value" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",Status" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",";

                                        //20210903 T15就一筆幹麻用AVG?
                                        SQLString_test += "cast(round(isnull(Value" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",0),2) as numeric(15,2)) AS Value" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",Status" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",";
                                    }
                                }
                            }
                        }
                        //20210903 testing
                        SQLString_test = SQLString_test.Substring(0, SQLString_test.Length - 1);

                        if (SQLString != "")
                            SQLString = SQLString.Substring(0, SQLString.Length - 1);
                        DataSet ds_avg15 = SQLControl.GetDataAVG(SQLString_test, "AVG_T15", dateTime.AddMinutes(-16).ToString("yyyy-MM-dd HH:mm:59"), dateTime.AddMinutes(+1).ToString("yyyy-MM-dd HH:mm:59"));//example 2019-11-22 17:12

                        if (ds_avg15.Tables[0].Rows.Count == 0)
                        {
                            Log.LogWrite("該時段無資料", 99);
                        }
                        else
                        {
                            for (int i = 0; i < GlobalVariable.Declare.DeclareItem.GetLength(0); i++)
                            {
                                if (GlobalVariable.Declare.DeclareItem[i, 1] == "Checked")
                                {
                                    for (int j = 0; j < GlobalVariable.SQL.InsertToRaw.GetLength(0) - 2; j++)
                                    {
                                        if (GlobalVariable.SQL.InsertToRawGC[j, 1].Equals(GlobalVariable.Declare.DeclareItem[i, 0]))
                                        {
                                            //數值
                                            string DataValue_avg15 = ds_avg15.Tables[0].Rows[0]["Value" + GlobalVariable.SQL.InsertToRawGC[j, 0]].ToString();//get dateset avg(value)

                                            //20210903 Status
                                            Status = ds_avg15.Tables[0].Rows[0]["Status" + GlobalVariable.SQL.InsertToRawGC[j, 0]].ToString();
                                            if (DataValue_avg15 != "")
                                            {

                                                switch (GlobalVariable.SQL.InsertToRawGC[j, 2])
                                                {
                                                    case "AX90":
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "9") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + DataValue_avg15 + "," + Status + "\n";
                                                        break;
                                                    case "AX80":
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "9") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + DataValue_avg15 + "," + Status + "\n";
                                                        break;
                                                    case "":
                                                        break;
                                                    default:
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "9") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + DataValue_avg15 + "," + Status + "\n";
                                                        break;
                                                }
                                            }
                                        }
                                        if (GlobalVariable.SQL.InsertToRaw[j, 1].Equals(GlobalVariable.Declare.DeclareItem[i, 0]))
                                        {
                                            //數值
                                            string DataValue_avg15 = ds_avg15.Tables[0].Rows[0]["Value" + GlobalVariable.SQL.InsertToRaw[j, 0]].ToString();//get dateset avg(value) 

                                            //20210903 Status
                                            Status = ds_avg15.Tables[0].Rows[0]["Status" + GlobalVariable.SQL.InsertToRaw[j, 0]].ToString();

                                            if (DataValue_avg15 != "")
                                            {
                                                switch (GlobalVariable.SQL.InsertToRaw[j, 2])
                                                {
                                                    //20210909 在母火1 2 3 中選出最高值&該狀態碼 寫入報表
                                                    case "AX90":
                                                        string[] PL_value_status = Highest_PilotLight(ds_avg15);

                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "9") + "A," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + PL_value_status[0] + "," + PL_value_status[1] + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "9") + "B," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + PL_value_status[2] + "," + PL_value_status[3] + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "9") + "C," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + PL_value_status[4] + "," + PL_value_status[5] + "\n";


                                                        break;
                                                    case "AX80":
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "9") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + DataValue_avg15 + "," + Status + "\n";
                                                        break;
                                                    case "":
                                                        break;
                                                    default:
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "9") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + DataValue_avg15 + "," + Status + "\n";
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            file.Write(writetitle + writestring + System.Text.Encoding.ASCII.GetString(Byte));
                            Log.LogWrite("Create Declare File Successful", 1);

                            //20211026 測試 新格式換成舊格式
                            Create_old_file(writetitle + writestring, dateTime);
                        }
                        file.Dispose();
                        file.Close();
                    }

                    if (writestring == "")
                    {
                        File.Delete(FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite("Write-T15 錯誤", 99);
                Log.LogWrite("Write-T15 錯誤" + ex.ToString(), 98);
            }

        }

        public void WriteFile_T60(DateTime dateTime, SQLControl SQLControl)
        {
            try
            {
                string FlareNo = InitialFileControl.FileRead("Declare", "DeclareFlareNo", "");
                string PolNo = InitialFileControl.FileRead("Declare", "DeclarePolNo", "");
                string CNo = InitialFileControl.FileRead("Declare", "DeclareCNo", "");
                string Version = InitialFileControl.FileRead("Declare", "DeclareVer", "");

                string Status = InitialFileControl.FileRead("CPDC", "Status", "00");
                TaiwanCalendar taiwanCalendar = new TaiwanCalendar();
                string FileName = "";

                if (PolNo == "" || CNo == "")
                    Log.LogWrite("尚未設定燃燒塔編號或公私場所碼", 99);
                else
                {
                    FileName = InitialFileControl.FileRead("Declare", "DeclareFile", "") + @"\FL" + (dateTime.Year - 1911).ToString() + dateTime.ToString("MMddHHmm") + "." + CNo;

                    FileCreate(FileName, false);
                    string writetitle = "1000," + FlareNo + ",FLR," + Version + "\n";
                    string writestring = "";
                    string SQLString = "";

                    //20210903 新增select 狀態碼
                    string SQLString_test = "";
                    using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(FileName, true))
                    {
                        for (int i = 0; i < GlobalVariable.Declare.DeclareItem.GetLength(0); i++)
                        {
                            if (GlobalVariable.Declare.DeclareItem[i, 1] == "Checked")
                            {
                                for (int j = 0; j < GlobalVariable.SQL.InsertToRawGC.GetLength(0); j++)
                                {
                                    if (GlobalVariable.SQL.InsertToRawGC[j, 1].Equals(GlobalVariable.Declare.DeclareItem[i, 0]))
                                    {
                                        SQLString += "cast(round(isnull(AVG(Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + "),0),2) as numeric(15,2)) AS Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",";

                                        //20210903 T15就一筆幹麻用AVG?
                                        SQLString_test += "cast(round(isnull(Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",0),2) as numeric(15,2)) AS Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",Status" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",";
                                    }
                                }
                                for (int j = 0; j < GlobalVariable.SQL.InsertToRaw.GetLength(0); j++)
                                {
                                    if (GlobalVariable.SQL.InsertToRaw[j, 1].Equals(GlobalVariable.Declare.DeclareItem[i, 0]))
                                    {
                                        SQLString += "cast(round(isnull(AVG(Value" + GlobalVariable.SQL.InsertToRaw[j, 0] + "),0),2) as numeric(15,2)) AS Value" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",";

                                        //20210903 T15就一筆幹麻用AVG?
                                        SQLString_test += "cast(round(isnull(Value" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",0),2) as numeric(15,2)) AS Value" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",Status" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",";
                                    }
                                }
                            }
                        }
                        //20210903
                        if (SQLString_test != "")
                        {
                            SQLString = SQLString.Substring(0, SQLString.Length - 1);
                            SQLString_test = SQLString_test.Substring(0, SQLString_test.Length - 1);
                        }

                        DataSet ds_avg15 = SQLControl.GetDataAVG(SQLString_test, "AVG_T15", dateTime.AddMinutes(-16).ToString("yyyy-MM-dd HH:mm:59"), dateTime.AddMinutes(+1).ToString("yyyy-MM-dd HH:mm:59"));//example 2019-11-22 17:12
                        DataSet ds_avg60 = SQLControl.GetDataAVG(SQLString_test, "AVG_T60", dateTime.AddHours(-2).ToString("yyyy-MM-dd HH:mm:59"), dateTime.AddMinutes(+1).ToString("yyyy-MM-dd HH:mm:59"));

                        if (ds_avg15.Tables[0].Rows.Count == 0 || ds_avg60.Tables[0].Rows.Count == 0)
                        {
                            Log.LogWrite("該時段無資料", 99);
                        }
                        else
                        {
                            for (int i = 0; i < GlobalVariable.Declare.DeclareItem.GetLength(0); i++)
                            {
                                if (GlobalVariable.Declare.DeclareItem[i, 1] == "Checked")
                                {
                                    for (int j = 0; j < GlobalVariable.SQL.InsertToRaw.GetLength(0) - 2; j++)
                                    {
                                        if (GlobalVariable.SQL.InsertToRawGC[j, 1].Equals(GlobalVariable.Declare.DeclareItem[i, 0]))
                                        {
                                            string DataValue_avg15 = ds_avg15.Tables[0].Rows[0]["Value" + GlobalVariable.SQL.InsertToRawGC[j, 0]].ToString();//get dateset avg(value)
                                            string DataValue_avg60 = ds_avg60.Tables[0].Rows[0]["Value" + GlobalVariable.SQL.InsertToRawGC[j, 0]].ToString();//get dateset avg(value)

                                            //20210903 拿T15 and T60 狀態碼
                                            string Status_avg15 = ds_avg15.Tables[0].Rows[0]["Status" + GlobalVariable.SQL.InsertToRawGC[j, 0]].ToString();
                                            string Status_avg60 = ds_avg60.Tables[0].Rows[0]["Status" + GlobalVariable.SQL.InsertToRawGC[j, 0]].ToString();

                                            if (DataValue_avg15 != "" && DataValue_avg60 != "")
                                            {
                                                switch (GlobalVariable.SQL.InsertToRawGC[j, 2])
                                                {
                                                    case "AX90":
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "9") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + DataValue_avg15 + "," + Status_avg15 + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddHours(-1).ToString("MMdd,HHmm") + "," + DataValue_avg60 + "," + Status_avg60 + "\n";
                                                        break;
                                                    case "AX80":
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "9") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + DataValue_avg15 + "," + Status_avg15 + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddHours(-1).ToString("MMdd,HHmm") + "," + DataValue_avg60 + "," + Status_avg60 + "\n";
                                                        break;
                                                    case "":
                                                        break;
                                                    default:
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "9") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + DataValue_avg15 + "," + Status_avg15 + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddHours(-1).ToString("MMdd,HHmm") + "," + DataValue_avg60 + "," + Status_avg60 + "\n";
                                                        break;
                                                }
                                            }
                                        }
                                        if (GlobalVariable.SQL.InsertToRaw[j, 1].Equals(GlobalVariable.Declare.DeclareItem[i, 0]))
                                        {
                                            string DataValue_avg15 = ds_avg15.Tables[0].Rows[0]["Value" + GlobalVariable.SQL.InsertToRaw[j, 0]].ToString();//get dateset avg(value) 
                                            string DataValue_avg60 = ds_avg60.Tables[0].Rows[0]["Value" + GlobalVariable.SQL.InsertToRaw[j, 0]].ToString();//get dateset avg(value) 

                                            //20210903 拿T15 and T60 狀態碼
                                            string Status_avg15 = ds_avg15.Tables[0].Rows[0]["Status" + GlobalVariable.SQL.InsertToRaw[j, 0]].ToString();
                                            string Status_avg60 = ds_avg60.Tables[0].Rows[0]["Status" + GlobalVariable.SQL.InsertToRaw[j, 0]].ToString();

                                            if (DataValue_avg15 != "" && DataValue_avg60 != "")
                                            {
                                                switch (GlobalVariable.SQL.InsertToRaw[j, 2])
                                                {
                                                    case "AX90":
                                                        //取母火溫度1 2 3 最高值
                                                        string[] PL_value_status_avg15 = Highest_PilotLight(ds_avg15);
                                                        string[] PL_value_status_avg60 = Highest_PilotLight(ds_avg60);

                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "9") + "A," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + PL_value_status_avg15[0] + "," + PL_value_status_avg15[1] + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "9") + "B," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + PL_value_status_avg15[2] + "," + PL_value_status_avg15[3] + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "9") + "C," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + PL_value_status_avg15[4] + "," + PL_value_status_avg15[5] + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "2") + "A," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddHours(-1).ToString("MMdd,HHmm") + "," + PL_value_status_avg60[0] + "," + PL_value_status_avg60[1] + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "2") + "B," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddHours(-1).ToString("MMdd,HHmm") + "," + PL_value_status_avg60[2] + "," + PL_value_status_avg60[3] + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "2") + "C," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddHours(-1).ToString("MMdd,HHmm") + "," + PL_value_status_avg60[4] + "," + PL_value_status_avg60[5] + "\n";
                                                        break;
                                                    case "AX80":
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "9") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + DataValue_avg15 + "," + Status_avg15 + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "2") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddHours(-1).ToString("MMdd,HHmm") + "," + DataValue_avg60 + "," + Status_avg60 + "\n";
                                                        break;
                                                    case "":
                                                        break;
                                                    default:
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "9") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddMinutes(-15).ToString("MMdd,HHmm") + "," + DataValue_avg15 + "," + Status_avg15 + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "2") + "," + PolNo + "," + taiwanCalendar.GetYear(dateTime) + dateTime.AddHours(-1).ToString("MMdd,HHmm") + "," + DataValue_avg60 + "," + Status_avg60 + "\n";
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            file.Write(writetitle + writestring + System.Text.Encoding.ASCII.GetString(Byte));
                            Log.LogWrite("Create Declare File Successful", 1);

                            //20211026 測試 新格式換成舊格式
                            Create_old_file(writetitle + writestring, dateTime);
                        }
                        file.Dispose();
                        file.Close();
                    }

                    if (writestring == "")
                    {
                        File.Delete(FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite("Write-T60 錯誤", 99);
                Log.LogWrite("Write-T60 錯誤" + ex.ToString(), 98);
            }

        }

        public void WriteFile_T1440(DateTime dateTime, SQLControl SQLControl)
        {
            try
            {
                string FlareNo = InitialFileControl.FileRead("Declare", "DeclareFlareNo", "");
                string PolNo = InitialFileControl.FileRead("Declare", "DeclarePolNo", "");
                string CNo = InitialFileControl.FileRead("Declare", "DeclareCNo", "");
                string Version = InitialFileControl.FileRead("Declare", "DeclareVer", "");
                string Status = InitialFileControl.FileRead("CPDC", "Status", "00");
                TaiwanCalendar taiwanCalendar = new TaiwanCalendar();
                string FileName = "";

                if (PolNo == "" || CNo == "")
                    Log.LogWrite("尚未設定燃燒塔編號或公私場所碼", 99);
                else
                {
                    FileName = InitialFileControl.FileRead("Declare", "DeclareFile", "") + @"\FL" + (dateTime.Year - 1911).ToString() + dateTime.ToString("MMdd") + "." + CNo;

                    FileCreate(FileName, false);
                    string writetitle = "1000," + FlareNo + ",FLL," + Version + "\n";
                    string writestring = "";
                    string SQLString = "CONVERT(VARCHAR(3),CONVERT(VARCHAR(4),DATEADD(HOUR, 0, [RecDateTime]),20) - 1911) + ";
                    SQLString += "SUBSTRING(CONVERT(VARCHAR(10), DATEADD(HOUR, 0, [RecDateTime]), 112), 5, 4) +";
                    SQLString += "SUBSTRING(CONVERT(VARCHAR(10), DATEADD(HOUR, 0, [RecDateTime]), 108), 0, 3) + ";
                    SQLString += "SUBSTRING(CONVERT(VARCHAR(10), DATEADD(HOUR, 0, [RecDateTime]), 108), 4, 2)  as RecDateTime ,";
                    using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(FileName, true))
                    {
                        for (int i = 0; i < GlobalVariable.Declare.DeclareItem.GetLength(0); i++)
                        {
                            if (GlobalVariable.Declare.DeclareItem[i, 1] == "Checked")
                            {
                                for (int j = 0; j < GlobalVariable.SQL.InsertToRawGC.GetLength(0); j++)
                                {
                                    if (GlobalVariable.SQL.InsertToRawGC[j, 1].Equals(GlobalVariable.Declare.DeclareItem[i, 0]))
                                    {
                                        //SQLString += "isnull(cast(round(AVG(Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + "),2) as numeric(15,2)),0) AS Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",";

                                        //熱值欄位 取到小數點後4位
                                        if (GlobalVariable.SQL.InsertToRawGC[j, 0] == "27" || GlobalVariable.SQL.InsertToRawGC[j, 0] == "28" || GlobalVariable.SQL.InsertToRawGC[j, 0] == "29" || GlobalVariable.SQL.InsertToRawGC[j, 0] == "30")
                                        {
                                            SQLString += "isnull(cast(round(Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",4) as numeric(15,4)),0) AS Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",Status" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",";
                                        }
                                        else
                                        {
                                            SQLString += "isnull(cast(round(Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",2) as numeric(15,2)),0) AS Value" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",Status" + GlobalVariable.SQL.InsertToRawGC[j, 0] + ",";
                                        }
                                    }
                                }
                                for (int j = 0; j < GlobalVariable.SQL.InsertToRaw.GetLength(0); j++)
                                {
                                    if (GlobalVariable.SQL.InsertToRaw[j, 1].Equals(GlobalVariable.Declare.DeclareItem[i, 0]))
                                    {
                                        //SQLString += "isnull(cast(round(AVG(Value" + GlobalVariable.SQL.InsertToRaw[j, 0] + "),2) as numeric(15,2)),0) AS Value" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",";

                                        SQLString += "isnull(cast(round(Value" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",2) as numeric(15,2)),0) AS Value" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",Status" + GlobalVariable.SQL.InsertToRaw[j, 0] + ",";
                                    }
                                }
                            }
                        }
                        if (SQLString != "")
                            SQLString = SQLString.Substring(0, SQLString.Length - 1);
                        for (int x = 0; x < 24; x++)
                        {
                            //DataSet ds_avg60 = SQLControl.GetDataAVG(SQLString, "AVG_T60", dateTime.AddHours(-x - 1).ToString("yyyy-MM-dd HH:mm:59"), dateTime.AddHours(-x).ToString("yyyy-MM-dd HH:01:59"));
                            DataSet ds_avg60 = SQLControl.GetDataAVG(SQLString, "AVG_T60", dateTime.AddHours(-x - 2).ToString("yyyy-MM-dd HH:00:59"), dateTime.AddHours(-x - 1).ToString("yyyy-MM-dd HH:01:59"));

                            if (ds_avg60.Tables[0].Rows.Count == 0)
                            {
                                Log.LogWrite("該時段無資料", 99);
                            }
                            else
                            {
                                for (int i = 0; i < GlobalVariable.Declare.DeclareItem.GetLength(0); i++)
                                {
                                    if (GlobalVariable.Declare.DeclareItem[i, 1] == "Checked")
                                    {
                                        for (int j = 0; j < GlobalVariable.SQL.InsertToRaw.GetLength(0) - 2; j++)
                                        {
                                            if (GlobalVariable.SQL.InsertToRawGC[j, 1].Equals(GlobalVariable.Declare.DeclareItem[i, 0]))
                                            {
                                                string DataValue_avg60 = ds_avg60.Tables[0].Rows[0]["Value" + GlobalVariable.SQL.InsertToRawGC[j, 0]].ToString();//get dateset avg(value)
                                                string DataValue_avg60_time = ds_avg60.Tables[0].Rows[0]["RecDateTime"].ToString();//get dateset avg(value)

                                                //日期跟時間隔開
                                                DataValue_avg60_time = DataValue_avg60_time.Insert(7, ",");

                                                //20211001 拿狀態碼
                                                string Status_avg60 = ds_avg60.Tables[0].Rows[0]["Status" + GlobalVariable.SQL.InsertToRawGC[j, 0]].ToString();

                                                //算小時排放量
                                                double A280 = Convert.ToDouble(ds_avg60.Tables[0].Rows[0]["Value14"]);
                                                string release = (Convert.ToDouble(DataValue_avg60) * A280 * 0.71 * 0.000001).ToString("#0.00");

                                                switch (GlobalVariable.SQL.InsertToRawGC[j, 2])
                                                {
                                                    //20211004 VOC 項目後面加熱值
                                                    case "X999":
                                                        string HCN_heat_value = Convert.ToDouble(ds_avg60.Tables[0].Rows[0]["Value27"]).ToString("#0.00");
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,," + HCN_heat_value + "," + release + "\n";
                                                        //writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + "\n";
                                                        break;
                                                    case "X258":
                                                        string C6H6_heat_value = Convert.ToDouble(ds_avg60.Tables[0].Rows[0]["Value28"]).ToString("#0.00");
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,," + C6H6_heat_value + "," + release + "\n";
                                                        //writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + "\n";
                                                        break;
                                                    case "X601":
                                                        string ACN_heat_value = Convert.ToDouble(ds_avg60.Tables[0].Rows[0]["Value29"]).ToString("#0.00");
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,," + ACN_heat_value + "," + release + "\n";
                                                        //writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + "\n";
                                                        break;
                                                    case "X602":
                                                        string AN_heat_value = Convert.ToDouble(ds_avg60.Tables[0].Rows[0]["Value30"]).ToString("#0.00");
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,," + AN_heat_value + "," + release + "\n";
                                                        //writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + "\n";
                                                        break;

                                                    //==============================================

                                                    case "AX90":
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,,," + "\n";
                                                        break;
                                                    case "AX78":
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,,," + "\n";
                                                        break;
                                                    case "AX79":
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,,," + "\n";
                                                        break;
                                                    case "AX80":
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,,," + "\n";
                                                        break;
                                                    case "AX81":
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,,," + "\n";
                                                        break;
                                                    case "":
                                                        break;
                                                    default:
                                                        writestring += GlobalVariable.SQL.InsertToRawGC[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,,," + release + "\n";
                                                        break;
                                                }
                                            }
                                            if (GlobalVariable.SQL.InsertToRaw[j, 1].Equals(GlobalVariable.Declare.DeclareItem[i, 0]))
                                            {
                                                string DataValue_avg60 = ds_avg60.Tables[0].Rows[0]["Value" + GlobalVariable.SQL.InsertToRaw[j, 0]].ToString();//get dateset avg(value) 
                                                string DataValue_avg60_time = ds_avg60.Tables[0].Rows[0]["RecDateTime"].ToString();//get dateset avg(value)
                                                DataValue_avg60_time = DataValue_avg60_time.Insert(7, ",");

                                                //20211001 拿狀態碼
                                                string Status_avg60 = ds_avg60.Tables[0].Rows[0]["Status" + GlobalVariable.SQL.InsertToRaw[j, 0]].ToString();

                                                switch (GlobalVariable.SQL.InsertToRaw[j, 2])
                                                {
                                                    case "AX90":
                                                        string[] PL_value_status_avg60 = Highest_PilotLight(ds_avg60);

                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "2") + "A," + PolNo + "," + DataValue_avg60_time + "," + PL_value_status_avg60[0] + "," + PL_value_status_avg60[1] + ",,,," + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "2") + "B," + PolNo + "," + DataValue_avg60_time + "," + PL_value_status_avg60[2] + "," + PL_value_status_avg60[3] + ",,,," + "\n";
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "2") + "C," + PolNo + "," + DataValue_avg60_time + "," + PL_value_status_avg60[4] + "," + PL_value_status_avg60[5] + ",,,," + "\n";
                                                        break;
                                                    case "AX78":
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,,," + "\n";
                                                        break;
                                                    case "AX79":
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,,," + "\n";
                                                        break;
                                                    case "AX80":
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,,," + "\n";
                                                        break;
                                                    case "":
                                                        break;
                                                    default:
                                                        writestring += GlobalVariable.SQL.InsertToRaw[j, 2].Replace("X", "2") + "," + PolNo + "," + DataValue_avg60_time + "," + DataValue_avg60 + "," + Status_avg60 + ",,,," + "\n";
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //20211026 測試 新格式換成舊格式
                        //先產舊的 再把日平均欄位加上去後產新的
                        Create_old_file(writetitle + writestring, dateTime);

                        file.Write(writetitle + writestring + SQLControl.Cal_daily_value(dateTime) + System.Text.Encoding.ASCII.GetString(Byte));
                        Log.LogWrite("Create Declare File Successful", 1);

                        file.Dispose();
                        file.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite("Write-T1440 錯誤", 99);
                Log.LogWrite("Write-T1440 錯誤" + ex.ToString(), 98);
            }

        }


        //回傳最高的母火溫度&他的狀態碼
        //2024-08-14 三個都回傳，不只傳最高的
        public string[] Highest_PilotLight(DataSet ds_avg)
        {
            string[] PL = { ds_avg.Tables[0].Rows[0]["Value16"].ToString(), ds_avg.Tables[0].Rows[0]["Value17"].ToString(), ds_avg.Tables[0].Rows[0]["Value18"].ToString() };
            string[] PL_status = { ds_avg.Tables[0].Rows[0]["Status16"].ToString(), ds_avg.Tables[0].Rows[0]["Status17"].ToString(), ds_avg.Tables[0].Rows[0]["Status18"].ToString() };

            //string[] arr = new string[2];
            string[] arr = new string[6];
            Double PilotLight_1 = Convert.ToDouble(PL[0]);
            Double PilotLight_2 = Convert.ToDouble(PL[1]);
            Double PilotLight_3 = Convert.ToDouble(PL[2]);

            arr[0] = PL[0];
            arr[1] = PL_status[0];
            arr[2] = PL[1];
            arr[3] = PL_status[1];
            arr[4] = PL[2];
            arr[5] = PL_status[2];
            return arr;
            //if (PilotLight_1 > PilotLight_2 && PilotLight_1 > PilotLight_3)
            //{
            //    arr[0] = PL[0];
            //    arr[1] = PL_status[0];
            //    return arr;
            //}
            //else if (PilotLight_2 > PilotLight_1 && PilotLight_2 > PilotLight_3)
            //{
            //    arr[0] = PL[1];
            //    arr[1] = PL_status[1];
            //    return arr;
            //}
            //else if (PilotLight_3 > PilotLight_1 && PilotLight_3 > PilotLight_2)
            //{
            //    arr[0] = PL[2];
            //    arr[1] = PL_status[2];
            //    return arr;
            //}
            //else
            //{
            //    arr[0] = PL[0];
            //    arr[1] = PL_status[0];
            //    return arr;
            //}
        }

        //20210910 計算日平均值!!?? (目前先寫死固定欄位)
        public string Cal_Daily_AVG(SQLControl SQLControl)
        {
            string FlareNo = InitialFileControl.FileRead("Declare", "DeclareFlareNo", "");
            string PolNo = InitialFileControl.FileRead("Declare", "DeclarePolNo", "");
            string CNo = InitialFileControl.FileRead("Declare", "DeclareCNo", "");
            string Status = InitialFileControl.FileRead("CPDC", "Status", "00");
            TaiwanCalendar taiwanCalendar = new TaiwanCalendar();

            if (PolNo == "" || CNo == "")
                Log.LogWrite("尚未設定燃燒塔編號或公私場所碼", 99);
            else
            {
                //select 幾個要算日平均的欄位
                string SQLstring = "";

            }
            return "test";
        }
    }
}
