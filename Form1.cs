using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rippix {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            this.ClientSize = new Size(800, 600);
            propertyGrid1.SelectedObject = pixelView1.Format;
            pixelView1.Format.PropertyChanged += new PropertyChangedEventHandler(Format_PropertyChanged);
            toolTip1.SetToolTip(pixelView1, helpText);
        }

        void Format_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            propertyGrid1.Refresh();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try {
                byte[] data = System.IO.File.ReadAllBytes(dlg.FileName);
                pixelView1.Format.Reset();
                pixelView1.Format.Data = data;
            } catch { }
        }

        private void savePictureToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try {
                pixelView1.Bitmap.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
            } catch { }
        }

        private void r8G8B8A8ToolStripMenuItem_Click(object sender, EventArgs e) {
            pixelView1.Format.SetPacking(24, 8, 16, 8, 8, 8, 0, 8);
        }

        private void b8G8R8A8ToolStripMenuItem_Click(object sender, EventArgs e) {
            pixelView1.Format.SetPacking(8, 8, 16, 8, 24, 8, 0, 8);
        }

        private void a8R8G8B8ToolStripMenuItem_Click(object sender, EventArgs e) {
            pixelView1.Format.SetPacking(16, 8, 8, 8, 0, 8, 24, 8);
        }

        private void a8B8G8R8ToolStripMenuItem_Click(object sender, EventArgs e) {
            pixelView1.Format.SetPacking(0, 8, 8, 8, 16, 8, 24, 8);
        }

        private void a1R5G5B5ToolStripMenuItem_Click(object sender, EventArgs e) {
            pixelView1.Format.SetPacking(10, 5, 5, 5, 0, 5, 15, 1);
        }

        private void r5G6B5ToolStripMenuItem_Click(object sender, EventArgs e) {
            pixelView1.Format.SetPacking(11, 5, 5, 6, 0, 5, 0, 0);
        }

        private void r3G3B2ToolStripMenuItem_Click(object sender, EventArgs e) {
            pixelView1.Format.SetPacking(5, 3, 2, 3, 0, 2, 0, 0);
        }

        private void alpha8ToolStripMenuItem_Click(object sender, EventArgs e) {
            pixelView1.Format.BitsA = 8;
        }

        private void alpha0ToolStripMenuItem_Click(object sender, EventArgs e) {
            pixelView1.Format.BitsA = 0;
        }

        private void r8G8B8ToolStripMenuItem_Click(object sender, EventArgs e) {
            pixelView1.Format.SetPacking(16, 8, 8, 8, 0, 8, 24, 0);
        }

        private void b8G8R8ToolStripMenuItem_Click(object sender, EventArgs e) {
            pixelView1.Format.SetPacking(0, 8, 8, 8, 16, 8, 24, 0);
        }

        #region
        const string helpText = @"
Key controls:
Up, Down - Pan vertically
Left, Right - Pan horizontally
PageUp, PageDown - Pan vertically by page
Z - Zoom in

Shift mode:
Up, Down - Change picture height
Left, Right - Change picture width
Z - Zoom out

Control mode - Step by 8 instead of 1
";
        #endregion
    }
}
