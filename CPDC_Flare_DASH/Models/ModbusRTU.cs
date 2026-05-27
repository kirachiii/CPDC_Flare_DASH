//using HslCommunication.ModBus;
using Modbus.Device;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Net.Sockets;

namespace CPDC_Flare_DASH.Models
{
    class ModbusRTU
    {
        //宣告ModBus元件
        public SerialPort serialPort = new SerialPort(); //create a new SerialPort object with default settings.
        public ModbusSerialMaster master;


        public bool Console_ModbusRtu(string COM, Int32 baud)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    Close_Modbus();
                }

                serialPort = new SerialPort();

                serialPort.PortName = COM;
                serialPort.BaudRate = baud;
                serialPort.DataBits = 8;
                //get parity 
                serialPort.Parity = Parity.None;
                //get stopBit  
                serialPort.StopBits = StopBits.One; 
                serialPort.ReadTimeout = 10000;

                serialPort.Open();
                  
                master = ModbusSerialMaster.CreateRtu(serialPort); 
                master.Transport.Retries = 0;   //don't have to do retriesv
                master.Transport.ReadTimeout = 10000; //milliseconds
                Console.WriteLine(DateTime.Now.ToString() + " =>Open " + serialPort.PortName + " sucessfully!");
                Log.LogWrite("ModBus_M-連接ModBus成功", 1);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("ModBus_M-連接ModBus錯誤 失敗", 99);
                return false;
            }
        }

        //接取獲得字串
        public string ConsoleRead(string labelName)
        {
            try
            {
                string[] labelarray = labelName.Split(',');
                string returnValue = "正常";
                byte slaveID = 2;
                ushort startAddress = 0;
                ushort numofPoints = 50;
                  

                if (master != null )
                {

                    //read AO(4xxxx)  
                    ushort[] holdingregister = master.ReadHoldingRegisters(slaveID, startAddress, numofPoints); 
                    float[] floatData = new float[holdingregister.Length / 2]; 
                    Buffer.BlockCopy(holdingregister, 0, floatData, 0, holdingregister.Length * 2);
                    //Log.LogWrite("Main-Timer_Refresh3", 1);

                    //20220607 看一下裡面長怎樣
                    
                    string a = string.Join(" ", holdingregister);
                    string b = string.Join(" ", floatData);
                    Log.LogWrite("holdingregister = " + a, 99);
                    Log.LogWrite("floatData = " + b, 99);
                    
                    foreach (var item in labelarray)
                    {
                        try
                        {
                            float Waste1 = 0;
                            string ReadValue = "0.0";
                            switch (item)
                            {
                                case "labelWasteFlow_Value":
                                    //Waste1 = floatData[2];
                                    Waste1 = holdingregister[3];
                                    break;
                                case "labelWasteTemp_Value":
                                    Waste1 = holdingregister[32];
                                    break;
                                case "ErrorCode":
                                    returnValue = holdingregister[24].ToString();
                                    break;
                            }

                            if (item == "labelWasteTemp_Value")
                            {
                                ReadValue = (Convert.ToDouble(Waste1) / 100).ToString("#0.00");
                            } 
                            else
                            {
                                double Q = Convert.ToDouble(Waste1) * 0.433 / 100;
                                double P = Convert.ToDouble(holdingregister[34]) / 1.0135 / 1000;
                                double T = 273 / ((Convert.ToDouble(holdingregister[32]) / 100) + 273);

                                ReadValue = (Q*P*T*3600).ToString("#0.0");
                            }

                            if (item != "ErrorCode" && Waste1 < 12100) //20230526 (不知道怎樣算異常) 先設一個閥值頂住 
                                DataColletionSystem.SetControlInfo(item, ReadValue);
                        } 
                        catch (Exception exception)
                        {
                            Log.LogWrite(exception.ToString(), 98);
                            Log.LogWrite("ModBus_M-接收數值[" + item + "]錯誤 失敗", 99);
                            if (item != "ErrorCode")
                                DataColletionSystem.SetControlInfo(item, "0.0");

                            return "異常";
                        }
                    }

                    master.Dispose();
                    Close_Modbus();
                    return returnValue;
                }
                else
                    return returnValue;

            }
            //catch (ArgumentOutOfRangeException AO)
            //{
            //    //Log.LogWrite(AO.ToString(), 98);
            //    Log.LogWrite("ModBus_M-接收數值 錯誤 失敗", 99);

            //    return "異常";
            //} 
            //catch (NotImplementedException ni)
            //{
            //    //Log.LogWrite(ni.ToString(), 98);
            //    Log.LogWrite("ModBus_M-接收數值 錯誤 失敗", 99);

            //    return "異常";
            //}
            catch (Exception exception)
            {
                //Log.LogWrite(exception.ToString(), 98);
                Log.LogWrite("ModBus_M-接收數值 錯誤 失敗", 99);

                return "異常";
            }


        }

        public void Close_Modbus()
        {
            serialPort.Close();
            serialPort.Dispose();
        }
    }
}
