using System;
using System.IO.Ports;
using System.Windows.Forms;
using CPDC_Flare_DASH.Models;

namespace CPDC_Flare_DASH
{
    public partial class Setting : Form
    {
        public Setting()
        {
            InitializeComponent();
        }


        private void Setting_Load(object sender, EventArgs e)
        {
            string[] ports2 = SerialPort.GetPortNames();
            string[] BaudArrary = { "300", "1200", "2400", "9600", "19200", "38400", "115200" };

            foreach (string tmp in ports2)
            {
                comboComport1.Items.Add(tmp);
                comboComport2.Items.Add(tmp);
                comboComport3.Items.Add(tmp);
                comboComport4.Items.Add(tmp);
            }

            foreach (string tmp in BaudArrary)
            {
                comboBaud1.Items.Add(tmp);
                comboBaud2.Items.Add(tmp);
                comboBaud3.Items.Add(tmp);
                comboBaud4.Items.Add(tmp);
            }
            

            //透過ini,取得設定
            comboComport1.SelectedIndex = comboComport1.FindStringExact(InitialFileControl.FileRead("COMWaste", "Comport", "null"));
            comboBaud1.SelectedIndex = comboBaud1.FindStringExact(InitialFileControl.FileRead("COMWaste", "Baud", "9600"));
            comboComport2.SelectedIndex = comboComport1.FindStringExact(InitialFileControl.FileRead("COMGas", "Comport", "null"));
            comboBaud2.SelectedIndex = comboBaud1.FindStringExact(InitialFileControl.FileRead("COMGas", "Baud", "9600"));
            comboComport3.SelectedIndex = comboComport1.FindStringExact(InitialFileControl.FileRead("COMDCS", "Comport", "null"));
            comboBaud3.SelectedIndex = comboBaud1.FindStringExact(InitialFileControl.FileRead("COMDCS", "Baud", "9600"));
            comboComport4.SelectedIndex = comboComport1.FindStringExact(InitialFileControl.FileRead("COMRegulate", "Comport", "null"));
            comboBaud4.SelectedIndex = comboBaud1.FindStringExact(InitialFileControl.FileRead("COMRegulate", "Baud", "9600"));
            comboStatus.SelectedIndex = comboStatus.FindString(InitialFileControl.FileRead("CPDC", "Status", "null")); 
            txtPath.Text = InitialFileControl.FileRead("File", "FileAddress", @"C:\");
            string chkVOC = InitialFileControl.FileRead("File", "VOCItem", "null");
            string chkTHC = InitialFileControl.FileRead("File", "THCItem", "null");
            if (chkVOC.IndexOf("C3H6") > -1)
                chkBoxC3H6.CheckState = CheckState.Checked;
            if (chkVOC.IndexOf("HCN") > -1)
                chkBoxHCN.CheckState = CheckState.Checked;
            if (chkVOC.IndexOf("ACN") > -1)
                chkBoxACN.CheckState = CheckState.Checked;
            if (chkVOC.IndexOf("AN") > -1)
                chkBoxAN.CheckState = CheckState.Checked;

            if (chkTHC.IndexOf("THC") > -1)
                chkBoxTHC.CheckState = CheckState.Checked;
            if (chkTHC.IndexOf("C1") > -1)
                chkBoxC1.CheckState = CheckState.Checked;
            if (chkTHC.IndexOf("C2") > -1)
                chkBoxC2.CheckState = CheckState.Checked;
            if (chkTHC.IndexOf("C3") > -1)
                chkBoxC3.CheckState = CheckState.Checked;
            if (chkTHC.IndexOf("C4") > -1)
                chkBoxC4.CheckState = CheckState.Checked;
            if (chkTHC.IndexOf("C5") > -1)
                chkBoxC5.CheckState = CheckState.Checked;

        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            this.txtPath.Text = path.SelectedPath;
        }

        private void Form1_Closing(object sender, EventArgs e)
        {
            string chkVOC = "", chkTHC = "";
            if (chkBoxC3H6.CheckState == CheckState.Checked)
                chkVOC += "C3H6,";
            if (chkBoxHCN.CheckState == CheckState.Checked)
                chkVOC += "HCN,";
            if (chkBoxACN.CheckState == CheckState.Checked)
                chkVOC += "ACN,";
            if (chkBoxAN.CheckState == CheckState.Checked)
                chkVOC += "AN,";
            if (chkVOC.Length > 0)
                chkVOC = chkVOC.Substring(0, chkVOC.Length - 1);


            if (chkBoxTHC.CheckState == CheckState.Checked)
                chkTHC += "THC,";
            if (chkBoxC1.CheckState == CheckState.Checked)
                chkTHC += "C1,";
            if (chkBoxC2.CheckState == CheckState.Checked)
                chkTHC += "C2,";
            if (chkBoxC3.CheckState == CheckState.Checked)
                chkTHC += "C3,";
            if (chkBoxC4.CheckState == CheckState.Checked)
                chkTHC += "C4,";
            if (chkBoxC5.CheckState == CheckState.Checked)
                chkTHC += "C5,";
            if (chkTHC.Length > 0)
                chkTHC = chkTHC.Substring(0, chkTHC.Length - 1);

            if (txtPath.Text == "")
                txtPath.Text = @"C:\";
            if (comboComport1.SelectedItem != null)
                InitialFileControl.FileWrite("COMWaste", "Comport", comboComport1.SelectedItem);
            InitialFileControl.FileWrite("COMWaste", "Baud", comboBaud1.SelectedItem);
            if (comboComport2.SelectedItem != null)
                InitialFileControl.FileWrite("COMGas", "Comport", comboComport2.SelectedItem);
            InitialFileControl.FileWrite("COMGas", "Baud", comboBaud2.SelectedItem);
            if (comboComport3.SelectedItem != null)
                InitialFileControl.FileWrite("COMDCS", "Comport", comboComport3.SelectedItem);
            InitialFileControl.FileWrite("COMDCS", "Baud", comboBaud3.SelectedItem);
            if (comboComport4.SelectedItem != null)
                InitialFileControl.FileWrite("COMRegulate", "Comport", comboComport4.SelectedItem);
            InitialFileControl.FileWrite("COMRegulate", "Baud", comboBaud4.SelectedItem);
            InitialFileControl.FileWrite("File", "FileAddress", txtPath.Text);
            InitialFileControl.FileWrite("File", "VOCItem", chkVOC);
            InitialFileControl.FileWrite("File", "THCItem", chkTHC); 
        }

    }
}
