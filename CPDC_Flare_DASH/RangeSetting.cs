using CPDC_Flare_DASH.Models;
using System; 
using System.Linq; 
using System.Windows.Forms;

namespace CPDC_Flare_DASH
{
    public partial class RangeSetting : Form
    {
        string[] GasArray = { "HCN", "O2", "H2", "Temp", "LNGA", "LNGB" };
        public RangeSetting()
        {
            InitializeComponent();
        }

        private void RangeSetting_Load(object sender, EventArgs e)
        {
            foreach (var GasValue in GasArray)
            {
                string[] tmp = InitialFileControl.FileRead("Range", GasValue, "null").Split('~');

                var control = Controls.OfType<NumericUpDown>().FirstOrDefault(c => c.Name == "num_" + GasValue + "_Min");
                control.Invoke((MethodInvoker)(() => control.Value = Convert.ToDecimal(tmp[0])));
                control = Controls.OfType<NumericUpDown>().FirstOrDefault(c => c.Name == "num_" + GasValue + "_Max");
                control.Invoke((MethodInvoker)(() => control.Value = Convert.ToDecimal(tmp[1])));
            }
        }

        private void btn_confirm_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var GasValue in GasArray)
                {
                    string[] tmp = InitialFileControl.FileRead("Range", GasValue, "null").Split('~');

                    Decimal getMinValue = 0, getMaxValue = 0;
                    var control = Controls.OfType<NumericUpDown>().FirstOrDefault(c => c.Name == "num_" + GasValue + "_Min");
                    control.Invoke((MethodInvoker)(() => getMinValue = control.Value));
                    control = Controls.OfType<NumericUpDown>().FirstOrDefault(c => c.Name == "num_" + GasValue + "_Max");
                    control.Invoke((MethodInvoker)(() => getMaxValue = control.Value));

                    InitialFileControl.FileWrite("Range", GasValue, getMinValue.ToString() + "~" + getMaxValue.ToString());
                }
                Log.LogWrite("數值範圍更新成功", 1);
                this.Close();
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.ToString(), 99);
                this.Close();
            }

        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        } 
    }
}
