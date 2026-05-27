using System;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Threading;
using log4net;
using Modbus.IO;
using Modbus.Message;
using Modbus.Utility;

namespace Modbus.Device
{
    // Token: 0x0200002F RID: 47
    public class ModbusSerialSlave : ModbusSlave
    {
        // Token: 0x0600018C RID: 396 RVA: 0x00005B06 File Offset: 0x00003D06
        private ModbusSerialSlave(byte unitId, ModbusTransport transport) : base(unitId, transport)
        {
        }

        // Token: 0x17000065 RID: 101
        // (get) Token: 0x0600018D RID: 397 RVA: 0x00005B10 File Offset: 0x00003D10
        private ModbusSerialTransport SerialTransport
        {
            get
            {
                ModbusSerialTransport modbusSerialTransport = base.Transport as ModbusSerialTransport;
                if (modbusSerialTransport == null)
                {
                    throw new ObjectDisposedException("SerialTransport");
                }
                return modbusSerialTransport;
            }
        }

        // Token: 0x0600018E RID: 398 RVA: 0x00005B38 File Offset: 0x00003D38
        public static ModbusSerialSlave CreateAscii(byte unitId, SerialPort serialPort)
        {
            if (serialPort == null)
            {
                throw new ArgumentNullException("serialPort");
            }
            return ModbusSerialSlave.CreateAscii(unitId, new SerialPortAdapter(serialPort));
        }

        // Token: 0x0600018F RID: 399 RVA: 0x00005B54 File Offset: 0x00003D54
        public static ModbusSerialSlave CreateAscii(byte unitId, IStreamResource streamResource)
        {
            if (streamResource == null)
            {
                throw new ArgumentNullException("streamResource");
            }
            return new ModbusSerialSlave(unitId, new ModbusAsciiTransport(streamResource));
        }

        // Token: 0x06000190 RID: 400 RVA: 0x00005B70 File Offset: 0x00003D70
        public static ModbusSerialSlave CreateRtu(byte unitId, SerialPort serialPort)
        {
            if (serialPort == null)
            {
                throw new ArgumentNullException("serialPort");
            }
            return ModbusSerialSlave.CreateRtu(unitId, new SerialPortAdapter(serialPort));
        }

        // Token: 0x06000191 RID: 401 RVA: 0x00005B8C File Offset: 0x00003D8C
        public static ModbusSerialSlave CreateRtu(byte unitId, IStreamResource streamResource)
        {
            if (streamResource == null)
            {
                throw new ArgumentNullException("streamResource");
            }
            return new ModbusSerialSlave(unitId, new ModbusRtuTransport(streamResource));
        }

        // Token: 0x06000192 RID: 402 RVA: 0x00005BA8 File Offset: 0x00003DA8
        public override void Listen()
        {
            this.threadDelegate = new ThreadStart(this.ThreadListen);
            this.thredModbusSlaveListen = new Thread(this.threadDelegate);
            this.thredModbusSlaveListen.Start();
        }

        // Token: 0x06000193 RID: 403 RVA: 0x00005BD8 File Offset: 0x00003DD8
        public override void Stop()
        {
            this.thredModbusSlaveListen.Abort();
        }

        // Token: 0x06000194 RID: 404 RVA: 0x00005BE8 File Offset: 0x00003DE8
        public void ThreadListen()
        {
            try
            {
                for (; ; )
                {
                    try
                    {
                        byte[] array;
                        IModbusMessage modbusMessage;
                        for (; ; )
                        {
                        IL_00:
                            array = this.SerialTransport.ReadRequest();
                            modbusMessage = ModbusMessageFactory.CreateModbusRequest(array);
                            if (this.SerialTransport.CheckFrame && !this.SerialTransport.ChecksumsMatch(modbusMessage, array))
                            {
                                break;
                            }
                            if (modbusMessage.SlaveAddress == base.UnitId)
                            {
                                goto IL_86;
                            }
                        }
                        string message = string.Format(CultureInfo.InvariantCulture, "Checksums failed to match {0} != {1}", new object[]
                        {
                            StringUtility.Join<byte>(", ", modbusMessage.MessageFrame),
                            StringUtility.Join<byte>(", ", array)
                        });
                        throw new IOException(message);
                    IL_86:
                        IModbusMessage message2 = base.ApplyRequest(modbusMessage);
                        this.SerialTransport.Write(message2);
                        goto IL_E6;
                    }
                    catch (IOException ex)
                    {
                        ModbusSerialSlave._logger.ErrorFormat("IO Exception encountered while listening for requests - {0}", ex.Message);
                        this.SerialTransport.DiscardInBuffer();
                        goto IL_E6;
                    }
                    catch (TimeoutException ex2)
                    {
                        ModbusSerialSlave._logger.ErrorFormat("Timeout Exception encountered while listening for requests - {0}", ex2.Message);
                        this.SerialTransport.DiscardInBuffer();
                        goto IL_E6;
                    }
                IL_E6:
                    goto IL_00;
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        // Token: 0x04000052 RID: 82
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ModbusSerialSlave));

        // Token: 0x04000053 RID: 83
        private ThreadStart threadDelegate;

        // Token: 0x04000054 RID: 84
        private Thread thredModbusSlaveListen;
    }
}
