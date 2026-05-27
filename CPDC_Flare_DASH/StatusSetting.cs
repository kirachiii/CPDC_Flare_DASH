using CPDC_Flare_DASH.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CPDC_Flare_DASH
{
    public partial class StatusSetting : Form
    {
        public StatusSetting()
        {
            InitializeComponent();
        }

        private void StatusSetting_Load(object sender, EventArgs e)
        {
            //讀設定檔當前設定狀態碼
            string status = InitialFileControl.FileRead("CPDC", "Status", "NA10");
            string Flow_status = InitialFileControl.FileRead("CPDC", "Flow_Status", "NA10");
            string GC1_status = InitialFileControl.FileRead("CPDC", "GC1_Status", "NA10");
            string GC2_status = InitialFileControl.FileRead("CPDC", "GC2_Status", "NA10");

            //以當前設定的狀態碼預設combobox內容
            switch (status.Substring(0, 1))
            {
                case "A":
                    comboBox1.SelectedIndex = 0;
                    break;
                case "N":
                    comboBox1.SelectedIndex = 1;
                    break;
                case "B":
                    comboBox1.SelectedIndex = 2;
                    break;
                case "C":
                    comboBox1.SelectedIndex = 3;
                    break;
                case "D":
                    comboBox1.SelectedIndex = 4;
                    break;
                case "E":
                    comboBox1.SelectedIndex = 5;
                    break;
                case "F":
                    comboBox1.SelectedIndex = 6;
                    break;
                case "G":
                    comboBox1.SelectedIndex = 7;
                    break;
                case "P":
                    comboBox1.SelectedIndex = 8;
                    break;
            }
            switch (status.Substring(1, 1))
            {
                case "B":
                    comboBox2.SelectedIndex = 0;
                    break;
                case "A":
                    comboBox2.SelectedIndex = 1;
                    break;
            }
            switch (status.Substring(2, 2))
            {
                case "21":
                    comboBox3.SelectedIndex = 0;
                    break;
                case "20":
                    comboBox3.SelectedIndex = 1;
                    break;
                case "31":
                    comboBox3.SelectedIndex = 2;
                    break;
                case "32":
                    comboBox3.SelectedIndex = 3;
                    break;
                case "01":
                    comboBox3.SelectedIndex = 4;
                    break;
                case "02":
                    comboBox3.SelectedIndex = 5;
                    break;
                case "03":
                    comboBox3.SelectedIndex = 6;
                    break;
                case "10":
                    comboBox3.SelectedIndex = 7;
                    break;
                case "30":
                    comboBox3.SelectedIndex = 8;
                    break;
                case "40":
                    comboBox3.SelectedIndex = 9;
                    break;
                case "11":
                    comboBox3.SelectedIndex = 10;
                    break;
                case "93":
                    comboBox3.SelectedIndex = 11;
                    break;
            }

            switch (Flow_status.Substring(0, 1))
            {
                case "A":
                    comboBox4.SelectedIndex = 0;
                    break;
                case "N":
                    comboBox4.SelectedIndex = 1;
                    break;
                case "B":
                    comboBox4.SelectedIndex = 2;
                    break;
                case "C":
                    comboBox4.SelectedIndex = 3;
                    break;
                case "D":
                    comboBox4.SelectedIndex = 4;
                    break;
                case "E":
                    comboBox4.SelectedIndex = 5;
                    break;
                case "F":
                    comboBox4.SelectedIndex = 6;
                    break;
                case "G":
                    comboBox4.SelectedIndex = 7;
                    break;
                case "P":
                    comboBox4.SelectedIndex = 8;
                    break;
            }
            switch (Flow_status.Substring(1, 1))
            {
                case "B":
                    comboBox5.SelectedIndex = 0;
                    break;
                case "A":
                    comboBox5.SelectedIndex = 1;
                    break;
            }
            switch (Flow_status.Substring(2, 2))
            {
                case "21":
                    comboBox6.SelectedIndex = 0;
                    break;
                case "20":
                    comboBox6.SelectedIndex = 1;
                    break;
                case "31":
                    comboBox6.SelectedIndex = 2;
                    break;
                case "32":
                    comboBox6.SelectedIndex = 3;
                    break;
                case "01":
                    comboBox6.SelectedIndex = 4;
                    break;
                case "02":
                    comboBox6.SelectedIndex = 5;
                    break;
                case "03":
                    comboBox6.SelectedIndex = 6;
                    break;
                case "10":
                    comboBox6.SelectedIndex = 7;
                    break;
                case "30":
                    comboBox3.SelectedIndex = 8;
                    break;
                case "40":
                    comboBox3.SelectedIndex = 9;
                    break;
                case "11":
                    comboBox3.SelectedIndex = 10;
                    break;
                case "93":
                    comboBox3.SelectedIndex = 11;
                    break;
            }
            switch (GC1_status.Substring(0, 1))
            {
                case "A":
                    comboBox7.SelectedIndex = 0;
                    break;
                case "N":
                    comboBox7.SelectedIndex = 1;
                    break;
                case "B":
                    comboBox7.SelectedIndex = 2;
                    break;
                case "C":
                    comboBox7.SelectedIndex = 3;
                    break;
                case "D":
                    comboBox7.SelectedIndex = 4;
                    break;
                case "E":
                    comboBox7.SelectedIndex = 5;
                    break;
                case "F":
                    comboBox7.SelectedIndex = 6;
                    break;
                case "G":
                    comboBox7.SelectedIndex = 7;
                    break;
                case "P":
                    comboBox7.SelectedIndex = 8;
                    break;
            }
            switch (GC1_status.Substring(1, 1))
            {
                case "B":
                    comboBox8.SelectedIndex = 0;
                    break;
                case "A":
                    comboBox8.SelectedIndex = 1;
                    break;
            }
            switch (GC1_status.Substring(2, 2))
            {
                case "21":
                    comboBox9.SelectedIndex = 0;
                    break;
                case "20":
                    comboBox9.SelectedIndex = 1;
                    break;
                case "31":
                    comboBox9.SelectedIndex = 2;
                    break;
                case "32":
                    comboBox9.SelectedIndex = 3;
                    break;
                case "01":
                    comboBox9.SelectedIndex = 4;
                    break;
                case "02":
                    comboBox9.SelectedIndex = 5;
                    break;
                case "03":
                    comboBox9.SelectedIndex = 6;
                    break;
                case "10":
                    comboBox9.SelectedIndex = 7;
                    break;
                case "30":
                    comboBox3.SelectedIndex = 8;
                    break;
                case "40":
                    comboBox3.SelectedIndex = 9;
                    break;
                case "11":
                    comboBox3.SelectedIndex = 10;
                    break;
                case "93":
                    comboBox3.SelectedIndex = 11;
                    break;
            }
            switch (GC2_status.Substring(0, 1))
            {
                case "A":
                    comboBox10.SelectedIndex = 0;
                    break;
                case "N":
                    comboBox10.SelectedIndex = 1;
                    break;
                case "B":
                    comboBox10.SelectedIndex = 2;
                    break;
                case "C":
                    comboBox10.SelectedIndex = 3;
                    break;
                case "D":
                    comboBox10.SelectedIndex = 4;
                    break;
                case "E":
                    comboBox10.SelectedIndex = 5;
                    break;
                case "F":
                    comboBox10.SelectedIndex = 6;
                    break;
                case "G":
                    comboBox10.SelectedIndex = 7;
                    break;
                case "P":
                    comboBox10.SelectedIndex = 8;
                    break;
            }
            switch (GC2_status.Substring(1, 1))
            {
                case "B":
                    comboBox11.SelectedIndex = 0;
                    break;
                case "A":
                    comboBox11.SelectedIndex = 1;
                    break;
            }
            switch (GC2_status.Substring(2, 2))
            {
                case "21":
                    comboBox12.SelectedIndex = 0;
                    break;
                case "20":
                    comboBox12.SelectedIndex = 1;
                    break;
                case "31":
                    comboBox12.SelectedIndex = 2;
                    break;
                case "32":
                    comboBox12.SelectedIndex = 3;
                    break;
                case "01":
                    comboBox12.SelectedIndex = 4;
                    break;
                case "02":
                    comboBox12.SelectedIndex = 5;
                    break;
                case "03":
                    comboBox12.SelectedIndex = 6;
                    break;
                case "10":
                    comboBox12.SelectedIndex = 7;
                    break;
                case "30":
                    comboBox3.SelectedIndex = 8;
                    break;
                case "40":
                    comboBox3.SelectedIndex = 9;
                    break;
                case "11":
                    comboBox3.SelectedIndex = 10;
                    break;
                case "93":
                    comboBox3.SelectedIndex = 11;
                    break;
            }
        }

        private void StatusSetting_yes_Click(object sender, EventArgs e)
        {
            //按下確定 將combobox內容寫進檔案
            string Write_Status = "";

            Write_Status += comboBox1.Text.Substring(0, 1);
            Write_Status += comboBox2.Text.Substring(0, 1);
            if (comboBox3.SelectedItem != null)
            {
                Write_Status += comboBox3.Text.Substring(0, 2);
            }
            else
            {
                Write_Status += "10";
            }
            InitialFileControl.FileWrite("CPDC", "Status", Write_Status);

            Write_Status = "";

            Write_Status += comboBox4.Text.Substring(0, 1);
            Write_Status += comboBox5.Text.Substring(0, 1);
            if (comboBox6.SelectedItem != null)
            {
                Write_Status += comboBox6.Text.Substring(0, 2);
            }
            else
            {
                Write_Status += "10";
            }

            InitialFileControl.FileWrite("CPDC", "Flow_Status", Write_Status);
            Write_Status = "";

            Write_Status += comboBox7.Text.Substring(0, 1);
            Write_Status += comboBox8.Text.Substring(0, 1);
            if (comboBox9.SelectedItem != null)
            {
                Write_Status += comboBox9.Text.Substring(0, 2);
            }
            else
            {
                Write_Status += "10";
            }

            InitialFileControl.FileWrite("CPDC", "GC1_Status", Write_Status);
            Write_Status = "";

            Write_Status += comboBox10.Text.Substring(0, 1);
            Write_Status += comboBox11.Text.Substring(0, 1);
            if (comboBox12.SelectedItem != null)
            {
                Write_Status += comboBox12.Text.Substring(0, 2);
            }
            else
            {
                Write_Status += "10";
            }

            InitialFileControl.FileWrite("CPDC", "GC2_Status", Write_Status);
            this.Close();
        }

        private void StatusSetting_no_Click(object sender, EventArgs e)
        {
            //按下取消 直接關閉 不寫檔
            this.Close();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
