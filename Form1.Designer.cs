namespace Rippix {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.savePictureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.formatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.r8G8B8A8ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.b8G8R8A8ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.a8R8G8B8ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.a8B8G8R8ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.a1R5G5B5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.r8G8B8ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.b8G8R8ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.r5G6B5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.r3G3B2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.alpha8ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alpha0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.propertyGrid2 = new System.Windows.Forms.PropertyGrid();
            this.pixelView1 = new Rippix.PixelView();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.formatToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(597, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.savePictureToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // savePictureToolStripMenuItem
            // 
            this.savePictureToolStripMenuItem.Name = "savePictureToolStripMenuItem";
            this.savePictureToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.savePictureToolStripMenuItem.Text = "Save Picture";
            this.savePictureToolStripMenuItem.Click += new System.EventHandler(this.savePictureToolStripMenuItem_Click);
            // 
            // formatToolStripMenuItem
            // 
            this.formatToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.r8G8B8A8ToolStripMenuItem,
            this.b8G8R8A8ToolStripMenuItem,
            this.a8R8G8B8ToolStripMenuItem,
            this.a8B8G8R8ToolStripMenuItem,
            this.a1R5G5B5ToolStripMenuItem,
            this.r8G8B8ToolStripMenuItem,
            this.b8G8R8ToolStripMenuItem,
            this.r5G6B5ToolStripMenuItem,
            this.r3G3B2ToolStripMenuItem,
            this.toolStripSeparator1,
            this.alpha8ToolStripMenuItem,
            this.alpha0ToolStripMenuItem,
            this.toolStripSeparator2});
            this.formatToolStripMenuItem.Name = "formatToolStripMenuItem";
            this.formatToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.formatToolStripMenuItem.Text = "Format";
            // 
            // r8G8B8A8ToolStripMenuItem
            // 
            this.r8G8B8A8ToolStripMenuItem.Name = "r8G8B8A8ToolStripMenuItem";
            this.r8G8B8A8ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.r8G8B8A8ToolStripMenuItem.Text = "R8G8B8A8";
            this.r8G8B8A8ToolStripMenuItem.Click += new System.EventHandler(this.r8G8B8A8ToolStripMenuItem_Click);
            // 
            // b8G8R8A8ToolStripMenuItem
            // 
            this.b8G8R8A8ToolStripMenuItem.Name = "b8G8R8A8ToolStripMenuItem";
            this.b8G8R8A8ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.b8G8R8A8ToolStripMenuItem.Text = "B8G8R8A8";
            this.b8G8R8A8ToolStripMenuItem.Click += new System.EventHandler(this.b8G8R8A8ToolStripMenuItem_Click);
            // 
            // a8R8G8B8ToolStripMenuItem
            // 
            this.a8R8G8B8ToolStripMenuItem.Name = "a8R8G8B8ToolStripMenuItem";
            this.a8R8G8B8ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.a8R8G8B8ToolStripMenuItem.Text = "A8R8G8B8";
            this.a8R8G8B8ToolStripMenuItem.Click += new System.EventHandler(this.a8R8G8B8ToolStripMenuItem_Click);
            // 
            // a8B8G8R8ToolStripMenuItem
            // 
            this.a8B8G8R8ToolStripMenuItem.Name = "a8B8G8R8ToolStripMenuItem";
            this.a8B8G8R8ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.a8B8G8R8ToolStripMenuItem.Text = "A8B8G8R8";
            this.a8B8G8R8ToolStripMenuItem.Click += new System.EventHandler(this.a8B8G8R8ToolStripMenuItem_Click);
            // 
            // a1R5G5B5ToolStripMenuItem
            // 
            this.a1R5G5B5ToolStripMenuItem.Name = "a1R5G5B5ToolStripMenuItem";
            this.a1R5G5B5ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.a1R5G5B5ToolStripMenuItem.Text = "A1R5G5B5";
            this.a1R5G5B5ToolStripMenuItem.Click += new System.EventHandler(this.a1R5G5B5ToolStripMenuItem_Click);
            // 
            // r8G8B8ToolStripMenuItem
            // 
            this.r8G8B8ToolStripMenuItem.Name = "r8G8B8ToolStripMenuItem";
            this.r8G8B8ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.r8G8B8ToolStripMenuItem.Text = "R8G8B8";
            this.r8G8B8ToolStripMenuItem.Click += new System.EventHandler(this.r8G8B8ToolStripMenuItem_Click);
            // 
            // b8G8R8ToolStripMenuItem
            // 
            this.b8G8R8ToolStripMenuItem.Name = "b8G8R8ToolStripMenuItem";
            this.b8G8R8ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.b8G8R8ToolStripMenuItem.Text = "B8G8R8";
            this.b8G8R8ToolStripMenuItem.Click += new System.EventHandler(this.b8G8R8ToolStripMenuItem_Click);
            // 
            // r5G6B5ToolStripMenuItem
            // 
            this.r5G6B5ToolStripMenuItem.Name = "r5G6B5ToolStripMenuItem";
            this.r5G6B5ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.r5G6B5ToolStripMenuItem.Text = "R5G6B5";
            this.r5G6B5ToolStripMenuItem.Click += new System.EventHandler(this.r5G6B5ToolStripMenuItem_Click);
            // 
            // r3G3B2ToolStripMenuItem
            // 
            this.r3G3B2ToolStripMenuItem.Name = "r3G3B2ToolStripMenuItem";
            this.r3G3B2ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.r3G3B2ToolStripMenuItem.Text = "R3G3B2";
            this.r3G3B2ToolStripMenuItem.Click += new System.EventHandler(this.r3G3B2ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(125, 6);
            // 
            // alpha8ToolStripMenuItem
            // 
            this.alpha8ToolStripMenuItem.Name = "alpha8ToolStripMenuItem";
            this.alpha8ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.alpha8ToolStripMenuItem.Text = "Alpha8";
            this.alpha8ToolStripMenuItem.Click += new System.EventHandler(this.alpha8ToolStripMenuItem_Click);
            // 
            // alpha0ToolStripMenuItem
            // 
            this.alpha0ToolStripMenuItem.Name = "alpha0ToolStripMenuItem";
            this.alpha0ToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.alpha0ToolStripMenuItem.Text = "Alpha0";
            this.alpha0ToolStripMenuItem.Click += new System.EventHandler(this.alpha0ToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(125, 6);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Top;
            this.propertyGrid1.HelpVisible = false;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(240, 118);
            this.propertyGrid1.TabIndex = 1;
            this.propertyGrid1.ToolbarVisible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.propertyGrid2);
            this.panel1.Controls.Add(this.propertyGrid1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(357, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(240, 238);
            this.panel1.TabIndex = 3;
            // 
            // propertyGrid2
            // 
            this.propertyGrid2.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.propertyGrid2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid2.Location = new System.Drawing.Point(0, 118);
            this.propertyGrid2.Name = "propertyGrid2";
            this.propertyGrid2.Size = new System.Drawing.Size(240, 120);
            this.propertyGrid2.TabIndex = 2;
            this.propertyGrid2.ToolbarVisible = false;
            // 
            // pixelView1
            // 
            this.pixelView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pixelView1.Location = new System.Drawing.Point(0, 24);
            this.pixelView1.Name = "pixelView1";
            this.pixelView1.Size = new System.Drawing.Size(357, 238);
            this.pixelView1.TabIndex = 2;
            this.pixelView1.Text = "pixelView1";
            this.pixelView1.Zoom = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 262);
            this.Controls.Add(this.pixelView1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem savePictureToolStripMenuItem;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private PixelView pixelView1;
        private System.Windows.Forms.ToolStripMenuItem formatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem r8G8B8A8ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem b8G8R8A8ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem a8R8G8B8ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem a8B8G8R8ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem a1R5G5B5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem r5G6B5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem r3G3B2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem alpha8ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alpha0ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem r8G8B8ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem b8G8R8ToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PropertyGrid propertyGrid2;
    }
}

