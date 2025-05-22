namespace MediaTransferToolApp.UI.Controls.TabControls
{
    partial class DestinationConfigTab
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlControls = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBaseUrl = new System.Windows.Forms.TextBox();
            this.txtEndpoint = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.cmbTokenType = new System.Windows.Forms.ComboBox();
            this.lblToken = new System.Windows.Forms.Label();
            this.pnlPassword = new System.Windows.Forms.Panel();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.pnlToken = new System.Windows.Forms.Panel();
            this.txtToken = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.btnSaveConfig = new System.Windows.Forms.Button();
            this.btnLoadFromFile = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlControls.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlPassword.SuspendLayout();
            this.pnlToken.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlControls
            // 
            this.pnlControls.Controls.Add(this.groupBox1);
            this.pnlControls.Controls.Add(this.panel1);
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlControls.Location = new System.Drawing.Point(0, 0);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Padding = new System.Windows.Forms.Padding(11);
            this.pnlControls.Size = new System.Drawing.Size(686, 427);
            this.pnlControls.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(11, 11);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(664, 352);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Hedef Sunucu Bilgileri";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblUsername, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblPassword, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.txtBaseUrl, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtEndpoint, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtUsername, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.cmbTokenType, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblToken, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.pnlPassword, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.pnlToken, 1, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 18);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(658, 331);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(9, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(187, 43);
            this.label1.TabIndex = 0;
            this.label1.Text = "Base URL:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(9, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(187, 43);
            this.label2.TabIndex = 1;
            this.label2.Text = "Endpoint:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblUsername.Location = new System.Drawing.Point(9, 134);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(187, 43);
            this.lblUsername.TabIndex = 2;
            this.lblUsername.Text = "Kullanıcı Adı:";
            this.lblUsername.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPassword.Location = new System.Drawing.Point(9, 177);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(187, 43);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "Şifre:";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(9, 91);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(187, 43);
            this.label5.TabIndex = 4;
            this.label5.Text = "Token Türü:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtBaseUrl
            // 
            this.txtBaseUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBaseUrl.Location = new System.Drawing.Point(202, 8);
            this.txtBaseUrl.Name = "txtBaseUrl";
            this.txtBaseUrl.Size = new System.Drawing.Size(447, 22);
            this.txtBaseUrl.TabIndex = 5;
            // 
            // txtEndpoint
            // 
            this.txtEndpoint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEndpoint.Location = new System.Drawing.Point(202, 51);
            this.txtEndpoint.Name = "txtEndpoint";
            this.txtEndpoint.Size = new System.Drawing.Size(447, 22);
            this.txtEndpoint.TabIndex = 6;
            // 
            // txtUsername
            // 
            this.txtUsername.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtUsername.Location = new System.Drawing.Point(202, 137);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(447, 22);
            this.txtUsername.TabIndex = 8;
            // 
            // cmbTokenType
            // 
            this.cmbTokenType.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmbTokenType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTokenType.FormattingEnabled = true;
            this.cmbTokenType.Location = new System.Drawing.Point(202, 94);
            this.cmbTokenType.Name = "cmbTokenType";
            this.cmbTokenType.Size = new System.Drawing.Size(447, 24);
            this.cmbTokenType.TabIndex = 7;
            // 
            // lblToken
            // 
            this.lblToken.AutoSize = true;
            this.lblToken.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblToken.Location = new System.Drawing.Point(9, 220);
            this.lblToken.Name = "lblToken";
            this.lblToken.Size = new System.Drawing.Size(187, 43);
            this.lblToken.TabIndex = 10;
            this.lblToken.Text = "Token:";
            this.lblToken.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlPassword
            // 
            this.pnlPassword.Controls.Add(this.txtPassword);
            this.pnlPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPassword.Location = new System.Drawing.Point(202, 180);
            this.pnlPassword.Name = "pnlPassword";
            this.pnlPassword.Size = new System.Drawing.Size(447, 37);
            this.pnlPassword.TabIndex = 11;
            // 
            // txtPassword
            // 
            this.txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPassword.Location = new System.Drawing.Point(0, 0);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(447, 22);
            this.txtPassword.TabIndex = 0;
            // 
            // pnlToken
            // 
            this.pnlToken.Controls.Add(this.txtToken);
            this.pnlToken.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlToken.Location = new System.Drawing.Point(202, 223);
            this.pnlToken.Name = "pnlToken";
            this.pnlToken.Size = new System.Drawing.Size(447, 37);
            this.pnlToken.TabIndex = 12;
            // 
            // txtToken
            // 
            this.txtToken.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtToken.Location = new System.Drawing.Point(0, 0);
            this.txtToken.Name = "txtToken";
            this.txtToken.Size = new System.Drawing.Size(447, 22);
            this.txtToken.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnNext);
            this.panel1.Controls.Add(this.btnTestConnection);
            this.panel1.Controls.Add(this.btnSaveConfig);
            this.panel1.Controls.Add(this.btnLoadFromFile);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(11, 363);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(664, 53);
            this.panel1.TabIndex = 1;
            // 
            // btnNext
            // 
            this.btnNext.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnNext.Enabled = false;
            this.btnNext.Location = new System.Drawing.Point(578, 0);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(86, 53);
            this.btnNext.TabIndex = 4;
            this.btnNext.Text = "İleri";
            this.btnNext.UseVisualStyleBackColor = true;
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.Location = new System.Drawing.Point(299, 0);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(141, 53);
            this.btnTestConnection.TabIndex = 3;
            this.btnTestConnection.Text = "Bağlantıyı Test Et";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            // 
            // btnSaveConfig
            // 
            this.btnSaveConfig.Location = new System.Drawing.Point(152, 0);
            this.btnSaveConfig.Name = "btnSaveConfig";
            this.btnSaveConfig.Size = new System.Drawing.Size(141, 53);
            this.btnSaveConfig.TabIndex = 2;
            this.btnSaveConfig.Text = "Yapılandırmayı Kaydet";
            this.btnSaveConfig.UseVisualStyleBackColor = true;
            // 
            // btnLoadFromFile
            // 
            this.btnLoadFromFile.Location = new System.Drawing.Point(5, 0);
            this.btnLoadFromFile.Name = "btnLoadFromFile";
            this.btnLoadFromFile.Size = new System.Drawing.Size(141, 53);
            this.btnLoadFromFile.TabIndex = 1;
            this.btnLoadFromFile.Text = "Dosyadan Yükle";
            this.btnLoadFromFile.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.progressBar);
            this.panel2.Controls.Add(this.lblStatus);
            this.panel2.Location = new System.Drawing.Point(447, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(123, 53);
            this.panel2.TabIndex = 0;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(3, 28);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(114, 21);
            this.progressBar.TabIndex = 1;
            this.progressBar.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(3, 9);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(46, 16);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Durum";
            this.lblStatus.Visible = false;
            // 
            // DestinationConfigTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlControls);
            this.Name = "DestinationConfigTab";
            this.Size = new System.Drawing.Size(686, 427);
            this.pnlControls.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.pnlPassword.ResumeLayout(false);
            this.pnlPassword.PerformLayout();
            this.pnlToken.ResumeLayout(false);
            this.pnlToken.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBaseUrl;
        private System.Windows.Forms.TextBox txtEndpoint;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.ComboBox cmbTokenType;
        private System.Windows.Forms.Label lblToken;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnTestConnection;
        private System.Windows.Forms.Button btnSaveConfig;
        private System.Windows.Forms.Button btnLoadFromFile;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel pnlPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Panel pnlToken;
        private System.Windows.Forms.TextBox txtToken;
    }
}