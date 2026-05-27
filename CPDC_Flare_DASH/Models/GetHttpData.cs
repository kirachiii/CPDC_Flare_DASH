using System;
using System.Net;
using System.Text;
using System.Timers;

namespace CPDC_Flare_DASH.Models
{ 
	internal class GetHttpData : IDisposable
	{ 
        private Timer _tmrGetSignals;  
        private string _ipaddr;  
        private double _NMHCData;  
        private double _VOCData;
         
        public event EventHandler DataUpdated;
         
		public event EventHandler<ClassEventArgs> ReportMessage;
         
		public GetHttpData(string ipaddr)
		{
			this._ipaddr = ipaddr;
			this._tmrGetSignals = new Timer();
			this._tmrGetSignals.Interval = 1000.0;
			this._tmrGetSignals.Elapsed += new ElapsedEventHandler(this._tmrGetSignals_Tick);
			this._tmrGetSignals.Start();
		}
         
		private void _tmrGetSignals_Tick(object seender, EventArgs e)
		{
			try
			{
				WebClient webClient = new WebClient();
				byte[] bytes = webClient.DownloadData("http://" + this._ipaddr + ":8080/vocman");
				string @string = Encoding.Default.GetString(bytes);
				string[] array = @string.Split(new char[]
				{
					'\n'
				});
				this._NMHCData = Convert.ToDouble(array[0].Replace("FID_NMHC,", ""));
				this._VOCData = Convert.ToDouble(array[1].Replace("FID_VOC,", ""));
				this.OnDataUpdated(e);
			}
			catch (Exception ex)
			{
				this.OnReportMessage(new ClassEventArgs(string.Format("讀取熄火訊號錯誤!!({0})", ex.Message)));
                Console.WriteLine(string.Format("讀取熄火訊號錯誤!!({0})", ex.Message));
			}
		}
         
		public double getNMHCData()
		{
			return this._NMHCData;
		}
         
		public double getVOCData()
		{
			return this._VOCData;
		}
         
		protected virtual void OnReportMessage(ClassEventArgs e)
		{
			if (this.ReportMessage != null)
			{
				this.ReportMessage(this, e);
			}
		} 
		protected virtual void OnDataUpdated(EventArgs e)
		{
			if (this.DataUpdated != null)
			{
				this.DataUpdated(this, e);
			}
		} 

		public void Dispose()
		{
			this._tmrGetSignals.Stop();
		} 
	}
}
