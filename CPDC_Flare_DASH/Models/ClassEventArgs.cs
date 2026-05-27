using System; 

namespace CPDC_Flare_DASH.Models
{ 
    internal class ClassEventArgs : EventArgs
    { 
        public ClassEventArgs(string Msg)
        {
            this.Message = Msg;
        }
         
        public readonly string Message;
    }
}
