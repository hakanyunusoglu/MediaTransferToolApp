namespace MediaTransferToolApp.UI.Controls.TabControls
{
    partial class TransferTab
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblFailedItems = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblSuccessfulItems = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblProcessedItems = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblProgressPercent = new System.Windows.Forms.Label();
            this.progressBarTotal = new System.Windows.Forms.ProgressBar();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblDuration = new System.Windows.Forms.Label();
            this.lblDurationLabel = new System.Windows.Forms.Label();
            this.lblProgressMediaCount = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lblCurrentCategoryId = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.lblCurrentFolder = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnViewLogs = new System.Windows.Forms.Button();
            this.pnlControls.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlControls
            // 
            this.pnlControls.Controls.Add(this.groupBox3);
            this.pnlControls.Controls.Add(this.groupBox2);
            this.pnlControls.Controls.Add(this.groupBox1);
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlControls.Location = new System.Drawing.Point(0, 0);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Padding = new System.Windows.Forms.Padding(10);
            this.pnlControls.Size = new System.Drawing.Size(600, 400);
            this.pnlControls.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(10, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(580, 80);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Transfer Kontrolü";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnViewLogs);
            this.panel2.Controls.Add(this.btnStop);
            this.panel2.Controls.Add(this.btnStart);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 19);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(574, 58);
            this.panel2.TabIndex = 0;
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnStop.ForeColor = System.Drawing.Color.Red;
            this.btnStop.Location = new System.Drawing.Point(230, 3);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(180, 50);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "DURDUR";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnStart.ForeColor = System.Drawing.Color.Green;
            this.btnStart.Location = new System.Drawing.Point(10, 3);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(180, 50);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "BAŞLAT";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panel1);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(10, 90);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(580, 100);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "İlerleme";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblFailedItems);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.lblSuccessfulItems);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.lblProcessedItems);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.lblProgressPercent);
            this.panel1.Controls.Add(this.progressBarTotal);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(574, 78);
            this.panel1.TabIndex = 0;
            // 
            // lblFailedItems
            // 
            this.lblFailedItems.AutoSize = true;
            this.lblFailedItems.Location = new System.Drawing.Point(517, 53);
            this.lblFailedItems.Name = "lblFailedItems";
            this.lblFailedItems.Size = new System.Drawing.Size(13, 15);
            this.lblFailedItems.TabIndex = 7;
            this.lblFailedItems.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(457, 53);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(54, 15);
            this.label10.TabIndex = 6;
            this.label10.Text = "Başarısız:";
            // 
            // lblSuccessfulItems
            // 
            this.lblSuccessfulItems.AutoSize = true;
            this.lblSuccessfulItems.Location = new System.Drawing.Point(371, 53);
            this.lblSuccessfulItems.Name = "lblSuccessfulItems";
            this.lblSuccessfulItems.Size = new System.Drawing.Size(13, 15);
            this.lblSuccessfulItems.TabIndex = 5;
            this.lblSuccessfulItems.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(318, 53);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 15);
            this.label8.TabIndex = 4;
            this.label8.Text = "Başarılı:";
            // 
            // lblProcessedItems
            // 
            this.lblProcessedItems.AutoSize = true;
            this.lblProcessedItems.Location = new System.Drawing.Point(217, 53);
            this.lblProcessedItems.Name = "lblProcessedItems";
            this.lblProcessedItems.Size = new System.Drawing.Size(27, 15);
            this.lblProcessedItems.TabIndex = 3;
            this.lblProcessedItems.Text = "0 / 0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(163, 53);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 15);
            this.label6.TabIndex = 2;
            this.label6.Text = "İşlenen:";
            // 
            // lblProgressPercent
            // 
            this.lblProgressPercent.AutoSize = true;
            this.lblProgressPercent.Location = new System.Drawing.Point(69, 53);
            this.lblProgressPercent.Name = "lblProgressPercent";
            this.lblProgressPercent.Size = new System.Drawing.Size(20, 15);
            this.lblProgressPercent.TabIndex = 1;
            this.lblProgressPercent.Text = "0%";
            // 
            // progressBarTotal
            // 
            this.progressBarTotal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarTotal.Location = new System.Drawing.Point(7, 12);
            this.progressBarTotal.Name = "progressBarTotal";
            this.progressBarTotal.Size = new System.Drawing.Size(560, 30);
            this.progressBarTotal.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.panel3);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Location = new System.Drawing.Point(10, 190);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(580, 165);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Transfer Bilgileri";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.lblDuration);
            this.panel3.Controls.Add(this.lblDurationLabel);
            this.panel3.Controls.Add(this.lblProgressMediaCount);
            this.panel3.Controls.Add(this.label14);
            this.panel3.Controls.Add(this.lblCurrentCategoryId);
            this.panel3.Controls.Add(this.label12);
            this.panel3.Controls.Add(this.lblCurrentFolder);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.lblStatus);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 19);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(574, 143);
            this.panel3.TabIndex = 0;
            // 
            // lblDuration
            // 
            this.lblDuration.AutoSize = true;
            this.lblDuration.Location = new System.Drawing.Point(123, 115);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(13, 15);
            this.lblDuration.TabIndex = 9;
            this.lblDuration.Text = "0";
            this.lblDuration.Visible = false;
            // 
            // lblDurationLabel
            // 
            this.lblDurationLabel.AutoSize = true;
            this.lblDurationLabel.Location = new System.Drawing.Point(8, 115);
            this.lblDurationLabel.Name = "lblDurationLabel";
            this.lblDurationLabel.Size = new System.Drawing.Size(78, 15);
            this.lblDurationLabel.TabIndex = 8;
            this.lblDurationLabel.Text = "Toplam Süre:";
            this.lblDurationLabel.Visible = false;
            // 
            // lblProgressMediaCount
            // 
            this.lblProgressMediaCount.AutoSize = true;
            this.lblProgressMediaCount.Location = new System.Drawing.Point(123, 89);
            this.lblProgressMediaCount.Name = "lblProgressMediaCount";
            this.lblProgressMediaCount.Size = new System.Drawing.Size(13, 15);
            this.lblProgressMediaCount.TabIndex = 7;
            this.lblProgressMediaCount.Text = "0";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(8, 89);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(109, 15);
            this.label14.TabIndex = 6;
            this.label14.Text = "İşlenen Medya Sayısı:";
            // 
            // lblCurrentCategoryId
            // 
            this.lblCurrentCategoryId.AutoSize = true;
            this.lblCurrentCategoryId.Location = new System.Drawing.Point(123, 62);
            this.lblCurrentCategoryId.Name = "lblCurrentCategoryId";
            this.lblCurrentCategoryId.Size = new System.Drawing.Size(12, 15);
            this.lblCurrentCategoryId.TabIndex = 5;
            this.lblCurrentCategoryId.Text = "-";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(8, 62);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(48, 15);
            this.label12.TabIndex = 4;
            this.label12.Text = "Kategori ID:";
            // 
            // lblCurrentFolder
            // 
            this.lblCurrentFolder.AutoSize = true;
            this.lblCurrentFolder.Location = new System.Drawing.Point(123, 37);
            this.lblCurrentFolder.Name = "lblCurrentFolder";
            this.lblCurrentFolder.Size = new System.Drawing.Size(12, 15);
            this.lblCurrentFolder.TabIndex = 3;
            this.lblCurrentFolder.Text = "-";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(109, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "İşlenen Klasör Adı:";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(123, 12);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(35, 15);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "Hazır";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Durum:";
            // 
            // btnViewLogs
            // 
            this.btnViewLogs.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnViewLogs.Location = new System.Drawing.Point(450, 3);
            this.btnViewLogs.Name = "btnViewLogs";
            this.btnViewLogs.Size = new System.Drawing.Size(117, 50);
            this.btnViewLogs.TabIndex = 2;
            this.btnViewLogs.Text = "Log Ekranını Göster";
            this.btnViewLogs.UseVisualStyleBackColor = true;
            // 
            // TransferTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlControls);
            this.Name = "TransferTab";
            this.Size = new System.Drawing.Size(600, 400);
            this.pnlControls.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label lblProgressMediaCount;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblCurrentCategoryId;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label lblCurrentFolder;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblFailedItems;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lblSuccessfulItems;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblProcessedItems;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblProgressPercent;
        private System.Windows.Forms.ProgressBar progressBarTotal;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.Label lblDurationLabel;
        private System.Windows.Forms.Button btnViewLogs;
    }
}