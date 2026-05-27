using System;
using System.Data;
using System.Linq;
using System.Text;

//指示詞
using System.IO.Ports;
using System.Threading;

namespace CPDC_Flare_DASH.Models
{
    class SerialPortConnect
    {
        //console 參數
        public SerialPort My_SerialPort;
        delegate void Display(string buffer);

        public bool Console_Connect(string COM, Int32 baud)
        {
            try
            {
                My_SerialPort = new SerialPort();

                if (My_SerialPort.IsOpen)
                {
                    My_SerialPort.Close();
                }

                //設定 Serial Port 參數
                My_SerialPort.PortName = COM;
                My_SerialPort.BaudRate = baud;
                My_SerialPort.DataBits = 8;
                My_SerialPort.StopBits = StopBits.One;

                if (!My_SerialPort.IsOpen)
                {
                    //開啟 Serial Port
                    My_SerialPort.Open();

                }
                Log.LogWrite("Serailport-開啟ComPort [" + COM + "] 成功", 1);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("Serailport-開啟ComPort [" + COM + "] 失敗", 99);
                return false;
            }
        }

        //關閉 Console
        public void CloseComport()
        {
            try
            {
                if (My_SerialPort.IsOpen)
                {
                    My_SerialPort.Close();
                    Log.LogWrite("Serailport-關閉ComPort 成功", 1);
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("Serailport-開啟ComPort 失敗", 99);
            }
        }

        //Console 接收資料
        public void DoReceive(string[] SensorArray)
        {
            Byte[] buffer = new Byte[1024];

            try
            {
                if (My_SerialPort.IsOpen)
                {
                    if (My_SerialPort.BytesToRead > 0)
                    {
                        

                        Int32 length = My_SerialPort.Read(buffer, 0, buffer.Length);

                        string[] buf = Encoding.ASCII.GetString(buffer).Replace(">", "").Split('+');
                        buf = buf.Where(s => !string.IsNullOrEmpty(s)).ToArray();

                        int count = 0;
                        foreach (string SensorName in SensorArray)
                        {
                            string[] range = InitialFileControl.FileRead("Range", SensorName, "null").Split('~');
                            double persent = (Convert.ToDouble(range[1]) - Convert.ToDouble(range[0])) / 16;
                            if (SensorName == "Temp")
                            {
                                DataColletionSystem.SetControlInfo("labelSite" + SensorName + "_Value", Convert.ToDouble(buf[count]).ToString("#0.0"));
                            }
                            else if (Convert.ToDouble(buf[count]) > 20 || Convert.ToDouble(buf[count]) < 4)
                            {
                                //母火溫度
                                if (SensorName == "Light1" || SensorName == "Light2" || SensorName == "Light3")
                                {
                                    DataColletionSystem.SetControlInfo("labelPilot" + SensorName + "_Value", "0.00");
                                }
                                else
                                {
                                    DataColletionSystem.SetControlInfo("labelSite" + SensorName + "_Value", "0.0");
                                }
                            }
                            else if (Convert.ToDouble(range[1]) == Convert.ToDouble(range[0]))
                            {
                                DataColletionSystem.SetControlInfo("labelSite" + SensorName + "_Value", Convert.ToDouble(range[1]).ToString("#0.0"));
                            }
                            else
                            {
                                //20210706 測試用
                                //Log.LogWrite("(TEST) Sendor = " + SensorName + " Value = " + buf[count] , 1);



                                //母火溫度
                                if (SensorName == "Light1" || SensorName == "Light2" || SensorName == "Light3")
                                {
                                    DataColletionSystem.SetControlInfo("labelPilot" + SensorName + "_Value", ((Convert.ToDouble(buf[count]) - 4) * persent).ToString("#0.000"));
                                }
                                else
                                {
                                    DataColletionSystem.SetControlInfo("labelSite" + SensorName + "_Value", ((Convert.ToDouble(buf[count]) - 4) * persent).ToString("#0.0"));
                                }
                            }
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("Serailport-接收偵測訊息 失敗", 99);
            }
        }

        //Console 接收資料
        public void DoReceive_Regulate()
        {
            Byte[] buffer = new Byte[1024];

            try
            {
                if (My_SerialPort.IsOpen)
                {
                    if (My_SerialPort.BytesToRead > 0)
                    {
                        Int32 length = My_SerialPort.Read(buffer, 0, buffer.Length);

                        string buf = Encoding.ASCII.GetString(buffer);

                        Log.LogWrite(buf, 1);
                        GlobalVariable.CPDCInfo.Regulate_Status = false;
                        string date = "";
                        string zero1 = "", zero2 = "";
                        string full1 = "", full2 = "";
                        string range = "";


                        try
                        {
                            String[] bufArray = buf.Replace("\r\n", "").Split(' ');//將多餘字元去除,依空白切割
                            bufArray = bufArray.Where(s => !string.IsNullOrEmpty(s)).ToArray();//將空白陣列去除
                            for (int i = 0; i < bufArray.Length; i++)
                            {
                                if (bufArray[i].IndexOf("Channel") != -1)
                                {
                                    DateTime dateTime = DateTime.Now;
                                    date = Convert.ToDateTime(bufArray[i + 4] + " " + bufArray[i + 3] + dateTime.ToString(" yyyy")).ToString("yyyy/MM/dd");

                                    if (bufArray[i + 6].IndexOf("AM") != -1)
                                        date += " " + bufArray[i + 5];
                                    else if (bufArray[i + 6].IndexOf("PM") != -1)
                                        date += " " + (Convert.ToInt32(bufArray[i + 5].Substring(0, 2)) + 12) + bufArray[i + 5].Substring(2, 3);

                                    GlobalVariable.CPDCInfo.Regulate_Range = date;
                                    Log.LogWrite(date, 99);
                                }
                                else if (bufArray[i].IndexOf("Zero") != -1)
                                {
                                    if (range == "Range1")
                                        zero1 = bufArray[i + 2];//20230522
                                    else
                                        zero2 = bufArray[i + 2];//20230522
                                }
                                else if (bufArray[i].IndexOf("Full") != -1)
                                {
                                    if (range == "Range1")
                                        full1 = bufArray[i + 2];//20230522
                                    else
                                        full2 = bufArray[i + 2];//20230522
                                }

                                if (bufArray[i].IndexOf("Range1") != -1)
                                    range = "Range1";
                                else if (bufArray[i].IndexOf("Range2") != -1)
                                    range = "Range2";

                                if (date != "" && zero1 != "" && zero2 != "" && full1 != "" && full2 != "" && range == "Range2")
                                {
                                    DataColletionSystem.SetControlInfo("Range1Zero_Value", ((Convert.ToDouble(zero1) - 0.03) / 0.03).ToString("#0.0000"));
                                    DataColletionSystem.SetControlInfo("Range1Full_Value", ((Convert.ToDouble(full1) - 0.3) / 0.3).ToString("#0.0000"));
                                    DataColletionSystem.SetControlInfo("Range2Zero_Value", ((Convert.ToDouble(zero2) - 0.3) / 0.3).ToString("#0.0000"));
                                    DataColletionSystem.SetControlInfo("Range2Full_Value", ((Convert.ToDouble(full2) - 50) / 50).ToString("#0.0000"));

                                    /*
                                    DataColletionSystem.SetControlInfo("Range1Zero_Value", Convert.ToDouble(zero1).ToString("#0.00"));
                                    DataColletionSystem.SetControlInfo("Range1Full_Value", Convert.ToDouble(full1).ToString("#0.00"));
                                    DataColletionSystem.SetControlInfo("Range2Zero_Value", Convert.ToDouble(zero2).ToString("#0.00"));
                                    DataColletionSystem.SetControlInfo("Range2Full_Value", Convert.ToDouble(full2).ToString("#0.00"));
                                    */

                                    DataColletionSystem.Regulate_Insert();
                                    date = "";
                                    zero1 = "";
                                    zero2 = "";
                                    full1 = "";
                                    full2 = "";
                                    range = "";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("Serailport-接收校正訊息 失敗", 99);
            }
        }

        //Console 發送資料
        public void SendData(string[] SensorArray, string Type)
        {
            try
            {
                if (Type == "Recive")
                {
                    byte[] buffer = { 0x23, 0x30, 0x31, 0x0d };// 送 # 0 1 CR
                    My_SerialPort.Write(buffer, 0, buffer.Length);
                    DoReceive(SensorArray);

                    Log.LogWrite("Recive GAS",1);
                }
                //母火
                else if (Type == "FireRecive")
                {
                    byte[] buffer = { 0x23, 0x30, 0x32, 0x0d };// 送 # 0 2 CR
                    My_SerialPort.Write(buffer, 0, buffer.Length);
                    DoReceive(SensorArray);

                    Log.LogWrite("Recive 母火", 1);
                }
                else if (Type == "Regulate")
                {
                    byte[] buffer = { 0x30 };
                    My_SerialPort.Write(buffer, 0, buffer.Length);
                    GlobalVariable.CPDCInfo.Regulate_Status = true;
                }

            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("Serailport-傳送失敗訊息失敗", 99);
            }
        }

        public void Close_SerialPortConnect()
        {
            CloseComport();
        }
    }
}
