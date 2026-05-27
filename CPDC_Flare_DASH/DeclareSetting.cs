using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CPDC_Flare_DASH.Models;

namespace CPDC_Flare_DASH
{
    public partial class DeclareSetting : Form
    {
        public DeclareSetting()
        {
            InitializeComponent();
        }

        private void DeclareSetting_Load(object sender, EventArgs e)
        {
            GlobalVariable.DeclareLoad();

            for (int i = 1; i <= 19; i++)
            {
                string CheckArray = GlobalVariable.Declare.DeclareItem[i - 1,1];
                var control = gBoxItem.Controls.OfType<CheckBox>().FirstOrDefault(c => c.Name == "chkBox" + i);
                if (CheckArray == "Checked")
                    control.Invoke((MethodInvoker)(() => control.CheckState = CheckState.Checked));
                else
                    control.Invoke((MethodInvoker)(() => control.CheckState = CheckState.Unchecked));
            }

            txt_FlareNo.Text = InitialFileControl.FileRead("Declare", "DeclareFlareNo", "");
            txt_CNo.Text = InitialFileControl.FileRead("Declare", "DeclareCNo", "");
            txtPath.Text = InitialFileControl.FileRead("Declare", "DeclareFile", "");
            txt_PolNo.Text = InitialFileControl.FileRead("Declare", "DeclarePolNo", "");
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            string DeclareArray = "";
            for (int i = 1; i <= 19; i++)
            {
                var control = gBoxItem.Controls.OfType<CheckBox>().FirstOrDefault(c => c.Name == "chkBox" + i);
                control.Invoke((MethodInvoker)(() => DeclareArray = control.AccessibleName + "," + control.CheckState.ToString()));
                InitialFileControl.FileWrite("Declare", "DeclareItem" + i, DeclareArray);
            }
            InitialFileControl.FileWrite("Declare", "DeclarePolNo", txt_PolNo.Text);
            InitialFileControl.FileWrite("Declare", "DeclareCNo", txt_CNo.Text);
            InitialFileControl.FileWrite("Declare", "DeclareFlareNo", txt_FlareNo.Text);
            InitialFileControl.FileWrite("Declare", "DeclareFile", txtPath.Text);

            this.Close();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            this.txtPath.Text = path.SelectedPath;
        }
         
    }
}
