using Modbus.Data;
using Modbus.Device;
using System;
using System.IO.Ports;
using System.Threading;
using System.Timers;

namespace CPDC_Flare_DASH.Models
{
    class ModbusSLV
    {
        ModbusSerialSlave slave;
        SerialPort comPort = new SerialPort();

        public event EventHandler DataUpdated;


        public bool Consolo_ModbusSlave(string comport, int baud)
        {
            try
            {
                if(comPort.IsOpen)
                {
                    comPort.Close();
                    comPort.Dispose();
                }

                comPort = new SerialPort(comport, baud, Parity.None, 8, StopBits.One);

                comPort.Open();
                slave = ModbusSerialSlave.CreateRtu(1, comPort);
                slave.DataStore = Modbus.Data.DataStoreFactory.CreateDefaultDataStore();
                slave.ModbusSlaveRequestReceived += this.OnModbusSlaveRX;
                slave.Listen();

                Log.LogWrite("ModBus_S-連接ModBus成功", 1);
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 98);
                Log.LogWrite("ModBus_S-連接ModBus錯誤 失敗", 99);
                return false;
            }
        } 

        private void OnModbusSlaveRX(object sender, ModbusSlaveRequestEventArgs e)
        {
            try
            {
                this.OnDataUpdated(new EventArgs());
            }
            catch (Exception ex)
            {
                Log.LogWrite("Modbus-OnModbusSlaveRX錯誤", 99);
                Log.LogWrite("Modbus-OnModbusSlaveRX錯誤" + ex.ToString(), 98);
            }
        }

        protected virtual void OnDataUpdated(EventArgs e)
        {
            try
            {
                if (this.DataUpdated != null)
                {
                    this.DataUpdated(this, e);
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite("Modbus-OnDataUpdated錯誤", 99);
                Log.LogWrite("Modbus-OnDataUpdated錯誤" + ex.ToString(), 98);
            }
        }

        public ushort getHoldingRegister(int index)
        {
            try
            {
                if (this.slave != null && this.slave.DataStore != null)
                {
                    return this.slave.DataStore.HoldingRegisters[index];
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite("Modbus-slave_getHold錯誤", 99);
                Log.LogWrite("Modbus-slave_getHold錯誤" + ex.ToString(), 98);
            }
            return 0;
        }

        public void setHoldingRegister(int index, ushort value)
        {
            try
            {
                if (this.slave != null && value < 65535)
                {
                    this.slave.DataStore.HoldingRegisters[index] = value;
                }
                //Log.LogWrite($"{index.ToString()} : {value.ToString()} send successful", 1);
            }
            catch (Exception ex)
            {
                Log.LogWrite("Modbus-slave_setHoldingRegister錯誤", 99);
                Log.LogWrite("Modbus-slave_setHoldingRegister錯誤" + ex.ToString(), 98);
            } 
        }

        public void Close_Modbus()
        {
            try
            {
                slave.Stop();
                slave.Dispose();
                slave = null;
                if (comPort != null)
                {
                    if (comPort.IsOpen)
                    {
                        comPort.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogWrite("Modbus-slave_Close_Modbus錯誤", 99);
                Log.LogWrite("Modbus-slave_Close_Modbus錯誤" + ex.ToString(), 98);
            } 
        }
    }
}
