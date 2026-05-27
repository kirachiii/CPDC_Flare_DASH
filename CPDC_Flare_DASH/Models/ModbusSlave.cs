using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using log4net;
using Modbus.Data;
using Modbus.IO;
using Modbus.Message;
using Modbus.Utility;

namespace Modbus.Device
{
    // Token: 0x02000013 RID: 19
    public abstract class ModbusSlave : ModbusDevice
    {
        // Token: 0x14000001 RID: 1
        // (add) Token: 0x0600009B RID: 155 RVA: 0x000036E9 File Offset: 0x000018E9
        // (remove) Token: 0x0600009C RID: 156 RVA: 0x00003702 File Offset: 0x00001902
        public event EventHandler<ModbusSlaveRequestEventArgs> ModbusSlaveRequestReceived;

        // Token: 0x0600009D RID: 157 RVA: 0x0000371B File Offset: 0x0000191B
        internal ModbusSlave(byte unitId, ModbusTransport transport) : base(transport)
        {
            this.DataStore = DataStoreFactory.CreateDefaultDataStore();
            this.UnitId = unitId;
        }

        // Token: 0x17000026 RID: 38
        // (get) Token: 0x0600009E RID: 158 RVA: 0x00003736 File Offset: 0x00001936
        // (set) Token: 0x0600009F RID: 159 RVA: 0x0000373E File Offset: 0x0000193E
        public DataStore DataStore { get; set; }

        // Token: 0x17000027 RID: 39
        // (get) Token: 0x060000A0 RID: 160 RVA: 0x00003747 File Offset: 0x00001947
        // (set) Token: 0x060000A1 RID: 161 RVA: 0x0000374F File Offset: 0x0000194F
        public byte UnitId { get; set; }

        // Token: 0x060000A2 RID: 162
        public abstract void Listen();

        // Token: 0x060000A3 RID: 163
        public abstract void Stop();

        // Token: 0x060000A4 RID: 164 RVA: 0x00003758 File Offset: 0x00001958
        internal static ReadCoilsInputsResponse ReadDiscretes(ReadCoilsInputsRequest request, DataStore dataStore, ModbusDataCollection<bool> dataSource)
        {
            DiscreteCollection discreteCollection = DataStore.ReadData<DiscreteCollection, bool>(dataStore, dataSource, request.StartAddress, request.NumberOfPoints, dataStore.SyncRoot);
            return new ReadCoilsInputsResponse(request.FunctionCode, request.SlaveAddress, discreteCollection.ByteCount, discreteCollection);
        }

        // Token: 0x060000A5 RID: 165 RVA: 0x0000379C File Offset: 0x0000199C
        internal static ReadHoldingInputRegistersResponse ReadRegisters(ReadHoldingInputRegistersRequest request, DataStore dataStore, ModbusDataCollection<ushort> dataSource)
        {
            RegisterCollection data = DataStore.ReadData<RegisterCollection, ushort>(dataStore, dataSource, request.StartAddress, request.NumberOfPoints, dataStore.SyncRoot);
            return new ReadHoldingInputRegistersResponse(request.FunctionCode, request.SlaveAddress, data);
        }

        // Token: 0x060000A6 RID: 166 RVA: 0x000037D8 File Offset: 0x000019D8
        internal static WriteSingleCoilRequestResponse WriteSingleCoil(WriteSingleCoilRequestResponse request, DataStore dataStore, ModbusDataCollection<bool> dataSource)
        {
            DataStore.WriteData<bool>(dataStore, new DiscreteCollection(new bool[]
            {
                request.Data[0] == 65280
            }), dataSource, request.StartAddress, dataStore.SyncRoot);
            return request;
        }

        // Token: 0x060000A7 RID: 167 RVA: 0x0000381C File Offset: 0x00001A1C
        internal static WriteMultipleCoilsResponse WriteMultipleCoils(WriteMultipleCoilsRequest request, DataStore dataStore, ModbusDataCollection<bool> dataSource)
        {
            DataStore.WriteData<bool>(dataStore, CollectionUtility.Slice<bool>(request.Data, 0, (int)request.NumberOfPoints), dataSource, request.StartAddress, dataStore.SyncRoot);
            return new WriteMultipleCoilsResponse(request.SlaveAddress, request.StartAddress, request.NumberOfPoints);
        }

        // Token: 0x060000A8 RID: 168 RVA: 0x00003867 File Offset: 0x00001A67
        internal static WriteSingleRegisterRequestResponse WriteSingleRegister(WriteSingleRegisterRequestResponse request, DataStore dataStore, ModbusDataCollection<ushort> dataSource)
        {
            DataStore.WriteData<ushort>(dataStore, request.Data, dataSource, request.StartAddress, dataStore.SyncRoot);
            return request;
        }

        // Token: 0x060000A9 RID: 169 RVA: 0x00003884 File Offset: 0x00001A84
        internal static WriteMultipleRegistersResponse WriteMultipleRegisters(WriteMultipleRegistersRequest request, DataStore dataStore, ModbusDataCollection<ushort> dataSource)
        {
            DataStore.WriteData<ushort>(dataStore, request.Data, dataSource, request.StartAddress, dataStore.SyncRoot);
            return new WriteMultipleRegistersResponse(request.SlaveAddress, request.StartAddress, request.NumberOfPoints);
        }

        // Token: 0x060000AA RID: 170 RVA: 0x000038C4 File Offset: 0x00001AC4
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Cast is not unneccessary.")]
        internal IModbusMessage ApplyRequest(IModbusMessage request)
        {
            ModbusSlave._logger.Info(request.ToString());
            EventHandler<ModbusSlaveRequestEventArgs> modbusSlaveRequestReceived = this.ModbusSlaveRequestReceived;
            if (modbusSlaveRequestReceived != null)
            {
                modbusSlaveRequestReceived(this, new ModbusSlaveRequestEventArgs(request));
            }
            byte functionCode = request.FunctionCode;
            switch (functionCode)
            {
                case 1:
                    return ModbusSlave.ReadDiscretes((ReadCoilsInputsRequest)request, this.DataStore, this.DataStore.CoilDiscretes);
                case 2:
                    return ModbusSlave.ReadDiscretes((ReadCoilsInputsRequest)request, this.DataStore, this.DataStore.InputDiscretes);
                case 3:
                    return ModbusSlave.ReadRegisters((ReadHoldingInputRegistersRequest)request, this.DataStore, this.DataStore.HoldingRegisters);
                case 4:
                    return ModbusSlave.ReadRegisters((ReadHoldingInputRegistersRequest)request, this.DataStore, this.DataStore.InputRegisters);
                case 5:
                    return ModbusSlave.WriteSingleCoil((WriteSingleCoilRequestResponse)request, this.DataStore, this.DataStore.CoilDiscretes);
                case 6:
                    return ModbusSlave.WriteSingleRegister((WriteSingleRegisterRequestResponse)request, this.DataStore, this.DataStore.HoldingRegisters);
                case 7:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                    break;
                case 8:
                    return request;
                case 15:
                    return ModbusSlave.WriteMultipleCoils((WriteMultipleCoilsRequest)request, this.DataStore, this.DataStore.CoilDiscretes);
                case 16:
                    return ModbusSlave.WriteMultipleRegisters((WriteMultipleRegistersRequest)request, this.DataStore, this.DataStore.HoldingRegisters);
                default:
                    if (functionCode == 23)
                    {
                        ReadWriteMultipleRegistersRequest readWriteMultipleRegistersRequest = (ReadWriteMultipleRegistersRequest)request;
                        IModbusMessage result = ModbusSlave.ReadRegisters(readWriteMultipleRegistersRequest.ReadRequest, this.DataStore, this.DataStore.HoldingRegisters);
                        ModbusSlave.WriteMultipleRegisters(readWriteMultipleRegistersRequest.WriteRequest, this.DataStore, this.DataStore.HoldingRegisters);
                        return result;
                    }
                    break;
            }
            string message = string.Format(CultureInfo.InvariantCulture, "Unsupported function code {0}", new object[]
            {
                request.FunctionCode
            });
            ModbusSlave._logger.Error(message);
            throw new ArgumentException(message, "request");
        }

        // Token: 0x0400001D RID: 29
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ModbusSlave));
    }
}
