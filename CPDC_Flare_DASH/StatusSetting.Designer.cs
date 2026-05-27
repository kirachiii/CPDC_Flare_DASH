
namespace CPDC_Flare_DASH
{
    partial class StatusSetting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.comboBox3 = new System.Windows.Forms.ComboBox();
            this.StatusSetting_yes = new System.Windows.Forms.Button();
            this.StatusSetting_no = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.comboBox4 = new System.Windows.Forms.ComboBox();
            this.comboBox5 = new System.Windows.Forms.ComboBox();
            this.comboBox6 = new System.Windows.Forms.ComboBox();
            this.comboBox7 = new System.Windows.Forms.ComboBox();
            this.comboBox8 = new System.Windows.Forms.ComboBox();
            this.comboBox9 = new System.Windows.Forms.ComboBox();
            this.comboBox10 = new System.Windows.Forms.ComboBox();
            this.comboBox11 = new System.Windows.Forms.ComboBox();
            this.comboBox12 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "A（固定汙染源正常運轉，防制設施故障、維修期間）",
            "N（固定汙染源正常運轉，防制設施正常運轉期間）",
            "B（固定汙染源起火（爐）期間，防制設施故障、維修期間）",
            "C（固定汙染源起火（爐）期間，防制設施正常運轉期間）",
            "D（固定汙染源停車（爐）期間，防制設施故障、維修期間）",
            "E（固定汙染源停車（爐）期間，防制設施正常運轉期間）",
            "F（固定汙染源暫停運轉）",
            "G（固定汙染源歲（檢）修期間）",
            "P（固定汙染源停工期間）"});
            this.comboBox1.Location = new System.Drawing.Point(15, 59);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(485, 23);
            this.comboBox1.TabIndex = 0;
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Items.AddRange(new object[] {
            "B（備用檢測設施）",
            "A（施核定使用監測設施）"});
            this.comboBox2.Location = new System.Drawing.Point(15, 118);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(485, 23);
            this.comboBox2.TabIndex = 1;
            // 
            // comboBox3
            // 
            this.comboBox3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox3.FormattingEnabled = true;
            this.comboBox3.Items.AddRange(new object[] {
            "21（主管機關稽查或查核）",
            "20（監測設施之例行校正測試、檢查或查核）",
            "31（監測設施修復性維修）",
            "32（監測設施預防性保養）",
            "01（監測設施汰換或量測位置變更）",
            "02（監測設施拆除）",
            "03（監測設施停電）",
            "10（依數據狀況判斷）",
            "30（無效數據）",
            "40（遺失數據）",
            "11（依數據狀況判斷）",
            "93（依過去資料之替代值）"});
            this.comboBox3.Location = new System.Drawing.Point(15, 177);
            this.comboBox3.Name = "comboBox3";
            this.comboBox3.Size = new System.Drawing.Size(485, 23);
            this.comboBox3.TabIndex = 2;
            this.comboBox3.SelectedIndexChanged += new System.EventHandler(this.comboBox3_SelectedIndexChanged);
            // 
            // StatusSetting_yes
            // 
            this.StatusSetting_yes.Location = new System.Drawing.Point(889, 416);
            this.StatusSetting_yes.Name = "StatusSetting_yes";
            this.StatusSetting_yes.Size = new System.Drawing.Size(121, 51);
            this.StatusSetting_yes.TabIndex = 3;
            this.StatusSetting_yes.Text = "確定";
            this.StatusSetting_yes.UseVisualStyleBackColor = true;
            this.StatusSetting_yes.Click += new System.EventHandler(this.StatusSetting_yes_Click);
            // 
            // StatusSetting_no
            // 
            this.StatusSetting_no.Location = new System.Drawing.Point(749, 416);
            this.StatusSetting_no.Name = "StatusSetting_no";
            this.StatusSetting_no.Size = new System.Drawing.Size(121, 51);
            this.StatusSetting_no.TabIndex = 4;
            this.StatusSetting_no.Text = "取消";
            this.StatusSetting_no.UseVisualStyleBackColor = true;
            this.StatusSetting_no.Click += new System.EventHandler(this.StatusSetting_no_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(10, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(139, 23);
            this.label1.TabIndex = 5;
            this.label1.Text = "第一碼(母火)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(11, 91);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(139, 23);
            this.label2.TabIndex = 6;
            this.label2.Text = "第二碼(母火)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(11, 150);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 23);
            this.label3.TabIndex = 7;
            this.label3.Text = "第三碼(母火)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(522, 150);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(184, 23);
            this.label4.TabIndex = 13;
            this.label4.Text = "第三碼(C1～THC)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(522, 91);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(184, 23);
            this.label5.TabIndex = 12;
            this.label5.Text = "第二碼(C1～THC)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label6.Location = new System.Drawing.Point(521, 32);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(184, 23);
            this.label6.TabIndex = 11;
            this.label6.Text = "第一碼(C1～THC)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label7.Location = new System.Drawing.Point(11, 344);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(208, 23);
            this.label7.TabIndex = 19;
            this.label7.Text = "第三碼(流速、流量)";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label8.Location = new System.Drawing.Point(11, 285);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(208, 23);
            this.label8.TabIndex = 18;
            this.label8.Text = "第二碼(流速、流量)";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label9.Location = new System.Drawing.Point(10, 226);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(208, 23);
            this.label9.TabIndex = 17;
            this.label9.Text = "第一碼(流速、流量)";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label10.Location = new System.Drawing.Point(521, 343);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(194, 23);
            this.label10.TabIndex = 25;
            this.label10.Text = "第三碼(HCN～AN)";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label11.Location = new System.Drawing.Point(521, 284);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(194, 23);
            this.label11.TabIndex = 24;
            this.label11.Text = "第二碼(HCN～AN)";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("新細明體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label12.Location = new System.Drawing.Point(520, 225);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(194, 23);
            this.label12.TabIndex = 23;
            this.label12.Text = "第一碼(HCN～AN)";
            // 
            // comboBox4
            // 
            this.comboBox4.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox4.FormattingEnabled = true;
            this.comboBox4.Items.AddRange(new object[] {
            "A（固定汙染源正常運轉，防制設施故障、維修期間）",
            "N（固定汙染源正常運轉，防制設施正常運轉期間）",
            "B（固定汙染源起火（爐）期間，防制設施故障、維修期間）",
            "C（固定汙染源起火（爐）期間，防制設施正常運轉期間）",
            "D（固定汙染源停車（爐）期間，防制設施故障、維修期間）",
            "E（固定汙染源停車（爐）期間，防制設施正常運轉期間）",
            "F（固定汙染源暫停運轉）",
            "G（固定汙染源歲（檢）修期間）",
            "P（固定汙染源停工期間）"});
            this.comboBox4.Location = new System.Drawing.Point(15, 253);
            this.comboBox4.Name = "comboBox4";
            this.comboBox4.Size = new System.Drawing.Size(485, 23);
            this.comboBox4.TabIndex = 26;
            // 
            // comboBox5
            // 
            this.comboBox5.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox5.FormattingEnabled = true;
            this.comboBox5.Items.AddRange(new object[] {
            "B（備用檢測設施）",
            "A（施核定使用監測設施）"});
            this.comboBox5.Location = new System.Drawing.Point(15, 312);
            this.comboBox5.Name = "comboBox5";
            this.comboBox5.Size = new System.Drawing.Size(485, 23);
            this.comboBox5.TabIndex = 27;
            // 
            // comboBox6
            // 
            this.comboBox6.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox6.FormattingEnabled = true;
            this.comboBox6.Items.AddRange(new object[] {
            "21（主管機關稽查或查核）",
            "20（監測設施之例行校正測試、檢查或查核）",
            "31（監測設施修復性維修）",
            "32（監測設施預防性保養）",
            "01（監測設施汰換或量測位置變更）",
            "02（監測設施拆除）",
            "03（監測設施停電）",
            "10（依數據狀況判斷）",
            "30（無效數據）",
            "40（遺失數據）",
            "11（依數據狀況判斷）",
            "93（依過去資料之替代值）"});
            this.comboBox6.Location = new System.Drawing.Point(15, 371);
            this.comboBox6.Name = "comboBox6";
            this.comboBox6.Size = new System.Drawing.Size(485, 23);
            this.comboBox6.TabIndex = 28;
            // 
            // comboBox7
            // 
            this.comboBox7.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox7.FormattingEnabled = true;
            this.comboBox7.Items.AddRange(new object[] {
            "A（固定汙染源正常運轉，防制設施故障、維修期間）",
            "N（固定汙染源正常運轉，防制設施正常運轉期間）",
            "B（固定汙染源起火（爐）期間，防制設施故障、維修期間）",
            "C（固定汙染源起火（爐）期間，防制設施正常運轉期間）",
            "D（固定汙染源停車（爐）期間，防制設施故障、維修期間）",
            "E（固定汙染源停車（爐）期間，防制設施正常運轉期間）",
            "F（固定汙染源暫停運轉）",
            "G（固定汙染源歲（檢）修期間）",
            "P（固定汙染源停工期間）"});
            this.comboBox7.Location = new System.Drawing.Point(525, 59);
            this.comboBox7.Name = "comboBox7";
            this.comboBox7.Size = new System.Drawing.Size(485, 23);
            this.comboBox7.TabIndex = 29;
            // 
            // comboBox8
            // 
            this.comboBox8.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox8.FormattingEnabled = true;
            this.comboBox8.Items.AddRange(new object[] {
            "B（備用檢測設施）",
            "A（施核定使用監測設施）"});
            this.comboBox8.Location = new System.Drawing.Point(526, 118);
            this.comboBox8.Name = "comboBox8";
            this.comboBox8.Size = new System.Drawing.Size(485, 23);
            this.comboBox8.TabIndex = 30;
            // 
            // comboBox9
            // 
            this.comboBox9.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox9.FormattingEnabled = true;
            this.comboBox9.Items.AddRange(new object[] {
            "21（主管機關稽查或查核）",
            "20（監測設施之例行校正測試、檢查或查核）",
            "31（監測設施修復性維修）",
            "32（監測設施預防性保養）",
            "01（監測設施汰換或量測位置變更）",
            "02（監測設施拆除）",
            "03（監測設施停電）",
            "10（依數據狀況判斷）",
            "30（無效數據）",
            "40（遺失數據）",
            "11（依數據狀況判斷）",
            "93（依過去資料之替代值）"});
            this.comboBox9.Location = new System.Drawing.Point(524, 177);
            this.comboBox9.Name = "comboBox9";
            this.comboBox9.Size = new System.Drawing.Size(485, 23);
            this.comboBox9.TabIndex = 31;
            // 
            // comboBox10
            // 
            this.comboBox10.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox10.FormattingEnabled = true;
            this.comboBox10.Items.AddRange(new object[] {
            "A（固定汙染源正常運轉，防制設施故障、維修期間）",
            "N（固定汙染源正常運轉，防制設施正常運轉期間）",
            "B（固定汙染源起火（爐）期間，防制設施故障、維修期間）",
            "C（固定汙染源起火（爐）期間，防制設施正常運轉期間）",
            "D（固定汙染源停車（爐）期間，防制設施故障、維修期間）",
            "E（固定汙染源停車（爐）期間，防制設施正常運轉期間）",
            "F（固定汙染源暫停運轉）",
            "G（固定汙染源歲（檢）修期間）",
            "P（固定汙染源停工期間）"});
            this.comboBox10.Location = new System.Drawing.Point(524, 253);
            this.comboBox10.Name = "comboBox10";
            this.comboBox10.Size = new System.Drawing.Size(485, 23);
            this.comboBox10.TabIndex = 32;
            // 
            // comboBox11
            // 
            this.comboBox11.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox11.FormattingEnabled = true;
            this.comboBox11.Items.AddRange(new object[] {
            "B（備用檢測設施）",
            "A（施核定使用監測設施）"});
            this.comboBox11.Location = new System.Drawing.Point(524, 312);
            this.comboBox11.Name = "comboBox11";
            this.comboBox11.Size = new System.Drawing.Size(485, 23);
            this.comboBox11.TabIndex = 33;
            // 
            // comboBox12
            // 
            this.comboBox12.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox12.FormattingEnabled = true;
            this.comboBox12.Items.AddRange(new object[] {
            "21（主管機關稽查或查核）",
            "20（監測設施之例行校正測試、檢查或查核）",
            "31（監測設施修復性維修）",
            "32（監測設施預防性保養）",
            "01（監測設施汰換或量測位置變更）",
            "02（監測設施拆除）",
            "03（監測設施停電）",
            "10（依數據狀況判斷）",
            "30（無效數據）",
            "40（遺失數據）",
            "11（依數據狀況判斷）",
            "93（依過去資料之替代值）"});
            this.comboBox12.Location = new System.Drawing.Point(524, 371);
            this.comboBox12.Name = "comboBox12";
            this.comboBox12.Size = new System.Drawing.Size(485, 23);
            this.comboBox12.TabIndex = 34;
            // 
            // StatusSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1027, 483);
            this.Controls.Add(this.comboBox12);
            this.Controls.Add(this.comboBox11);
            this.Controls.Add(this.comboBox10);
            this.Controls.Add(this.comboBox9);
            this.Controls.Add(this.comboBox8);
            this.Controls.Add(this.comboBox7);
            this.Controls.Add(this.comboBox6);
            this.Controls.Add(this.comboBox5);
            this.Controls.Add(this.comboBox4);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.StatusSetting_no);
            this.Controls.Add(this.StatusSetting_yes);
            this.Controls.Add(this.comboBox3);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.comboBox1);
            this.Name = "StatusSetting";
            this.Text = "StatusSetting";
            this.Load += new System.EventHandler(this.StatusSetting_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Button StatusSetting_yes;
        private System.Windows.Forms.Button StatusSetting_no;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox comboBox4;
        private System.Windows.Forms.ComboBox comboBox5;
        private System.Windows.Forms.ComboBox comboBox6;
        private System.Windows.Forms.ComboBox comboBox7;
        private System.Windows.Forms.ComboBox comboBox8;
        private System.Windows.Forms.ComboBox comboBox9;
        private System.Windows.Forms.ComboBox comboBox10;
        private System.Windows.Forms.ComboBox comboBox11;
        private System.Windows.Forms.ComboBox comboBox12;
    }
}