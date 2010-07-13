namespace HSpellCoverageTester
{
    partial class MainForm
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
            System.Windows.Forms.Label label4;
            System.Windows.Forms.Label label2;
            System.Windows.Forms.Label label1;
            this.btnExecute = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txbReportPath = new System.Windows.Forms.TextBox();
            this.btnSelectReportPath = new System.Windows.Forms.Button();
            this.txbCorpusPath = new System.Windows.Forms.TextBox();
            this.txbHSpellPath = new System.Windows.Forms.TextBox();
            this.btnSelectCorpusWikiDump = new System.Windows.Forms.Button();
            this.btnSelectCorpusPath = new System.Windows.Forms.Button();
            this.btnSelectHSpellPath = new System.Windows.Forms.Button();
            this.chbComputeCoverage = new System.Windows.Forms.CheckBox();
            label4 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(1, 104);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(77, 13);
            label4.TabIndex = 19;
            label4.Text = "Save report to:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(1, 56);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(67, 13);
            label2.TabIndex = 15;
            label2.Text = "Corpus path:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(1, 4);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(110, 13);
            label1.TabIndex = 16;
            label1.Text = "HSpell data files path:";
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExecute.Location = new System.Drawing.Point(438, 213);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(66, 26);
            this.btnExecute.TabIndex = 4;
            this.btnExecute.Text = "Start";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 213);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(420, 26);
            this.progressBar1.TabIndex = 5;
            this.progressBar1.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(9, 197);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(37, 13);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Status";
            this.lblStatus.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.chbComputeCoverage);
            this.panel1.Controls.Add(label4);
            this.panel1.Controls.Add(this.txbReportPath);
            this.panel1.Controls.Add(this.btnSelectReportPath);
            this.panel1.Controls.Add(label2);
            this.panel1.Controls.Add(label1);
            this.panel1.Controls.Add(this.txbCorpusPath);
            this.panel1.Controls.Add(this.txbHSpellPath);
            this.panel1.Controls.Add(this.btnSelectCorpusWikiDump);
            this.panel1.Controls.Add(this.btnSelectCorpusPath);
            this.panel1.Controls.Add(this.btnSelectHSpellPath);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(492, 182);
            this.panel1.TabIndex = 11;
            // 
            // txbReportPath
            // 
            this.txbReportPath.Location = new System.Drawing.Point(0, 123);
            this.txbReportPath.Name = "txbReportPath";
            this.txbReportPath.Size = new System.Drawing.Size(396, 20);
            this.txbReportPath.TabIndex = 18;
            // 
            // btnSelectReportPath
            // 
            this.btnSelectReportPath.Location = new System.Drawing.Point(402, 121);
            this.btnSelectReportPath.Name = "btnSelectReportPath";
            this.btnSelectReportPath.Size = new System.Drawing.Size(90, 23);
            this.btnSelectReportPath.TabIndex = 17;
            this.btnSelectReportPath.Text = "Browse...";
            this.btnSelectReportPath.UseVisualStyleBackColor = true;
            this.btnSelectReportPath.Click += new System.EventHandler(this.btnSelectReportPath_Click);
            // 
            // txbCorpusPath
            // 
            this.txbCorpusPath.Location = new System.Drawing.Point(0, 72);
            this.txbCorpusPath.Name = "txbCorpusPath";
            this.txbCorpusPath.Size = new System.Drawing.Size(300, 20);
            this.txbCorpusPath.TabIndex = 14;
            // 
            // txbHSpellPath
            // 
            this.txbHSpellPath.Location = new System.Drawing.Point(0, 23);
            this.txbHSpellPath.Name = "txbHSpellPath";
            this.txbHSpellPath.Size = new System.Drawing.Size(396, 20);
            this.txbHSpellPath.TabIndex = 13;
            // 
            // btnSelectCorpusWikiDump
            // 
            this.btnSelectCorpusWikiDump.Location = new System.Drawing.Point(306, 69);
            this.btnSelectCorpusWikiDump.Name = "btnSelectCorpusWikiDump";
            this.btnSelectCorpusWikiDump.Size = new System.Drawing.Size(90, 23);
            this.btnSelectCorpusWikiDump.TabIndex = 10;
            this.btnSelectCorpusWikiDump.Text = "Wiki-dump...";
            this.btnSelectCorpusWikiDump.UseVisualStyleBackColor = true;
            this.btnSelectCorpusWikiDump.Click += new System.EventHandler(this.btnSelectCorpusWikiDump_Click);
            // 
            // btnSelectCorpusPath
            // 
            this.btnSelectCorpusPath.Enabled = false;
            this.btnSelectCorpusPath.Location = new System.Drawing.Point(402, 69);
            this.btnSelectCorpusPath.Name = "btnSelectCorpusPath";
            this.btnSelectCorpusPath.Size = new System.Drawing.Size(90, 23);
            this.btnSelectCorpusPath.TabIndex = 11;
            this.btnSelectCorpusPath.Text = "Select folder...";
            this.btnSelectCorpusPath.UseVisualStyleBackColor = true;
            // 
            // btnSelectHSpellPath
            // 
            this.btnSelectHSpellPath.Location = new System.Drawing.Point(402, 21);
            this.btnSelectHSpellPath.Name = "btnSelectHSpellPath";
            this.btnSelectHSpellPath.Size = new System.Drawing.Size(90, 23);
            this.btnSelectHSpellPath.TabIndex = 12;
            this.btnSelectHSpellPath.Text = "Select folder...";
            this.btnSelectHSpellPath.UseVisualStyleBackColor = true;
            this.btnSelectHSpellPath.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // chbComputeCoverage
            // 
            this.chbComputeCoverage.AutoSize = true;
            this.chbComputeCoverage.Checked = true;
            this.chbComputeCoverage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbComputeCoverage.Location = new System.Drawing.Point(4, 149);
            this.chbComputeCoverage.Name = "chbComputeCoverage";
            this.chbComputeCoverage.Size = new System.Drawing.Size(172, 17);
            this.chbComputeCoverage.TabIndex = 20;
            this.chbComputeCoverage.Text = "Compute coverage (longer run)";
            this.chbComputeCoverage.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 251);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "HSpell Coverage Tester";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txbReportPath;
        private System.Windows.Forms.Button btnSelectReportPath;
        private System.Windows.Forms.TextBox txbCorpusPath;
        private System.Windows.Forms.TextBox txbHSpellPath;
        private System.Windows.Forms.Button btnSelectCorpusWikiDump;
        private System.Windows.Forms.Button btnSelectCorpusPath;
        private System.Windows.Forms.Button btnSelectHSpellPath;
        private System.Windows.Forms.CheckBox chbComputeCoverage;
    }
}

