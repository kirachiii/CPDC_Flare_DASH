using System;
using System.Windows.Forms;
using CPDC_Flare_DASH.Models;

namespace CPDC_Flare_DASH
{
    public partial class DeclareInput : Form
    {
        public DeclareInput()
        {
            InitializeComponent();
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            GlobalVariable.Declare.Password = InitialFileControl.FileRead("Declare", "DeclarePWD", "null");
            //密碼解密
            if (GlobalVariable.Declare.Password.EndsWith("="))
            {
                char[] KeyArray = GlobalVariable.Key.ToCharArray();
                Array.Reverse(KeyArray);
                string ReverseKey = new string(KeyArray);
                GlobalVariable.Declare.Password = AESEncryption.AESDecryptBase64(GlobalVariable.Declare.Password, ReverseKey);
                if (txtBox_Code.Text == GlobalVariable.Declare.Password)
                {
                    DeclareSetting declareSetting = new DeclareSetting();//產生Setting的物件
                    declareSetting.ShowDialog(this);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("密碼錯誤,請聯絡管理員!!");
                }
            }

        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
