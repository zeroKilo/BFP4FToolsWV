namespace BFP4FLauncherWV
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.rtb3 = new System.Windows.Forms.RichTextBox();
            this.menuStrip3 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.rtb1 = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.launchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.rtb2 = new System.Windows.Forms.RichTextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.launchStandaloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.rtb4 = new System.Windows.Forms.RichTextBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.rtb5 = new System.Windows.Forms.RichTextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.menuStrip3.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(637, 371);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.rtb3);
            this.tabPage3.Controls.Add(this.menuStrip3);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(629, 345);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Launcher Server";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // rtb3
            // 
            this.rtb3.DetectUrls = false;
            this.rtb3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb3.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.rtb3.Location = new System.Drawing.Point(3, 27);
            this.rtb3.Name = "rtb3";
            this.rtb3.Size = new System.Drawing.Size(623, 315);
            this.rtb3.TabIndex = 5;
            this.rtb3.Text = "+key \"eakey\" \n+useServerMonitorTool 0\n+soldierName \"test-server\"\n+sessionId 1234 " +
    "\n+magmaProtocol http\n+magmaHost \"127.0.0.1\"\n+magma 1\n+guid \"5678\"\n+secret \"secre" +
    "t\"";
            // 
            // menuStrip3
            // 
            this.menuStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2});
            this.menuStrip3.Location = new System.Drawing.Point(3, 3);
            this.menuStrip3.Name = "menuStrip3";
            this.menuStrip3.Size = new System.Drawing.Size(623, 24);
            this.menuStrip3.TabIndex = 4;
            this.menuStrip3.Text = "menuStrip3";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(53, 20);
            this.toolStripMenuItem2.Text = "Launch";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.rtb1);
            this.tabPage1.Controls.Add(this.menuStrip1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(629, 345);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Launcher Game";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // rtb1
            // 
            this.rtb1.DetectUrls = false;
            this.rtb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb1.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.rtb1.Location = new System.Drawing.Point(3, 27);
            this.rtb1.Name = "rtb1";
            this.rtb1.Size = new System.Drawing.Size(623, 315);
            this.rtb1.TabIndex = 3;
            this.rtb1.Text = resources.GetString("rtb1.Text");
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.launchToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(3, 3);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(623, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // launchToolStripMenuItem
            // 
            this.launchToolStripMenuItem.Name = "launchToolStripMenuItem";
            this.launchToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.launchToolStripMenuItem.Text = "Launch";
            this.launchToolStripMenuItem.Click += new System.EventHandler(this.launchToolStripMenuItem_Click_1);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.rtb2);
            this.tabPage2.Controls.Add(this.checkBox1);
            this.tabPage2.Controls.Add(this.menuStrip2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(629, 345);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Blaze";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // rtb2
            // 
            this.rtb2.DetectUrls = false;
            this.rtb2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb2.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.rtb2.Location = new System.Drawing.Point(3, 44);
            this.rtb2.Name = "rtb2";
            this.rtb2.Size = new System.Drawing.Size(623, 298);
            this.rtb2.TabIndex = 5;
            this.rtb2.Text = "";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.checkBox1.Location = new System.Drawing.Point(3, 27);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(623, 17);
            this.checkBox1.TabIndex = 6;
            this.checkBox1.Text = "Redirector uses SSL";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // menuStrip2
            // 
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.launchStandaloneToolStripMenuItem});
            this.menuStrip2.Location = new System.Drawing.Point(3, 3);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(623, 24);
            this.menuStrip2.TabIndex = 4;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(167, 20);
            this.toolStripMenuItem1.Text = "Launch for use with Blazeshark";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // launchStandaloneToolStripMenuItem
            // 
            this.launchStandaloneToolStripMenuItem.Name = "launchStandaloneToolStripMenuItem";
            this.launchStandaloneToolStripMenuItem.Size = new System.Drawing.Size(110, 20);
            this.launchStandaloneToolStripMenuItem.Text = "Launch Standalone";
            this.launchStandaloneToolStripMenuItem.Click += new System.EventHandler(this.launchStandaloneToolStripMenuItem_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.rtb4);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(629, 345);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Magma Log";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // rtb4
            // 
            this.rtb4.DetectUrls = false;
            this.rtb4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb4.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.rtb4.Location = new System.Drawing.Point(3, 3);
            this.rtb4.Name = "rtb4";
            this.rtb4.Size = new System.Drawing.Size(623, 339);
            this.rtb4.TabIndex = 7;
            this.rtb4.Text = "";
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.rtb5);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(629, 345);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "WebServer Log";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // rtb5
            // 
            this.rtb5.DetectUrls = false;
            this.rtb5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb5.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.rtb5.Location = new System.Drawing.Point(3, 3);
            this.rtb5.Name = "rtb5";
            this.rtb5.Size = new System.Drawing.Size(623, 339);
            this.rtb5.TabIndex = 8;
            this.rtb5.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(637, 371);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Battlefield Play 4 Free Launcher by Warranty Voider";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.menuStrip3.ResumeLayout(false);
            this.menuStrip3.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox rtb1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem launchToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox rtb2;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem launchStandaloneToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.RichTextBox rtb3;
        private System.Windows.Forms.MenuStrip menuStrip3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.RichTextBox rtb4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.RichTextBox rtb5;
        private System.Windows.Forms.CheckBox checkBox1;

    }
}

