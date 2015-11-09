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

        private IPictureFormat picture;

        public Form1() {
            InitializeComponent();
            this.Icon = Rippix.Properties.Resources.MainIcon;
            this.ClientSize = new Size(800, 600);
            propertyGrid1.Height = 300;
            toolTip1.SetToolTip(pixelView1, helpText);
            CreateFormatMenuItems();
            CreateColorMenuItems();
            //setPictureDecoder(new PictureFormat());
        }

        void setPictureDecoder(IPictureFormat decoder) {
            if (picture != null) {
                picture.Changed -= new EventHandler(Format_Changed);
            }
            picture = decoder;
            pixelView1.Format = decoder;
            if (picture != null) {
                picture.Changed += new EventHandler(Format_Changed);
                propertyGrid1.SelectedObject = picture;
                propertyGrid2.SelectedObject = picture.ColorFormat;
                Format_Changed(picture, EventArgs.Empty);
            }
        }

        void Format_Changed(object sender, EventArgs e) {
            propertyGrid1.Refresh();
            propertyGrid2.Refresh();
            highlightColorItem();
        }

        private void highlightColorItem() {
            foreach (var item in colorToolStripMenuItem.DropDownItems) {
                ToolStripMenuItem menuItem = item as ToolStripMenuItem;
                if (menuItem != null) {
                    FormatPreset preset = menuItem.Tag as FormatPreset;
                    if (preset != null) {
                        menuItem.Checked = (picture != null)
                        && preset.ColorBPP == picture.ColorBPP
                        && Equals(preset.ColorFormat, picture.ColorFormat);
                    }
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try {
                byte[] data = System.IO.File.ReadAllBytes(dlg.FileName);
                if (picture == null) {
                    setPictureDecoder(new DirectPictureFormat());
                }
                pixelView1.Data = data;
                picture.PicOffset = 0;
                picture.PicWidth = 8;
                picture.PicHeight = 8;
                highlightColorItem();
            } catch { }
        }

        private void savePictureToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try {
                pixelView1.Bitmap.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
            } catch { }
        }

        class FormatPreset {
            public int ColorBPP { get; private set; }
            public ColorFormat ColorFormat { get; private set; }
            public FormatPreset(int bpp, ColorFormat colorFormat) {
                this.ColorBPP = bpp;
                this.ColorFormat = colorFormat;
            }
        }

        private ToolStripItem CreateFormatMenuItem(string name, int bpp, ColorFormat colorFormat) {
            var item = new ToolStripMenuItem();
            item.Text = name;
            item.Tag = new FormatPreset(bpp, colorFormat);
            item.Click += item_Click;
            return item;
        }

        void item_Click(object sender, EventArgs e) {
            var item = (ToolStripItem)sender;
            var preset = (FormatPreset)item.Tag;
            if (picture == null) return;
            picture.SetPacking(preset.ColorBPP, preset.ColorFormat);
        }

        private void CreateColorMenuItems() {
            var list = new List<ToolStripItem>();
            list.Add(CreateFormatMenuItem("R8G8B8A8", 32, new ColorFormat(24, 8, 16, 8, 8, 8, 0, 8)));
            list.Add(CreateFormatMenuItem("B8G8R8A8", 32, new ColorFormat(8, 8, 16, 8, 24, 8, 0, 8)));
            list.Add(CreateFormatMenuItem("A8R8G8B8", 32, new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8)));
            list.Add(CreateFormatMenuItem("A8B8G8R8", 32, new ColorFormat(0, 8, 8, 8, 16, 8, 24, 8)));
            list.Add(new ToolStripSeparator());
            list.Add(CreateFormatMenuItem("A1R5G5B5", 16, new ColorFormat(10, 5, 5, 5, 0, 5, 15, 1)));
            list.Add(CreateFormatMenuItem("R5G6B5", 16, new ColorFormat(11, 5, 5, 6, 0, 5, 0, 0)));
            list.Add(new ToolStripSeparator());
            list.Add(CreateFormatMenuItem("R3G3B2", 8, new ColorFormat(5, 3, 2, 3, 0, 2, 0, 0)));
            list.Add(new ToolStripSeparator());
            list.Add(CreateFormatMenuItem("R8G8B8", 24, new ColorFormat(16, 8, 8, 8, 0, 8, 24, 0)));
            list.Add(CreateFormatMenuItem("B8G8R8", 24, new ColorFormat(0, 8, 8, 8, 16, 8, 24, 0)));
            colorToolStripMenuItem.DropDownItems.AddRange(list.ToArray());
        }

        private void CreateFormatMenuItems() {
            {
                var item = new ToolStripMenuItem();
                item.Text = "Direct";
                item.Click += delegate {
                    var old = picture;
                    IPictureFormat format = new DirectPictureFormat();
                    if (old != null) {
                        format.PicOffset = old.PicOffset;
                        format.PicWidth = old.PicWidth;
                        format.PicHeight = old.PicHeight;
                        format.ColorBPP = old.ColorBPP;
                    };
                    setPictureDecoder(format);
                };
                formatToolStripMenuItem.DropDownItems.Add(item);
            }
            {
                var item = new ToolStripMenuItem();
                item.Text = "Indexed";
                item.Click += delegate {
                    var old = picture;
                    IPictureFormat format = new IndexedPictureFormat();
                    if (old != null) {
                        format.PicOffset = old.PicOffset;
                        format.PicWidth = old.PicWidth;
                        format.PicHeight = old.PicHeight;
                        format.ColorBPP = old.ColorBPP;
                    };
                    setPictureDecoder(format);
                };
                formatToolStripMenuItem.DropDownItems.Add(item);
            }
            {
                var item = new ToolStripMenuItem();
                item.Text = "Amiga4";
                item.Click += delegate {
                    var old = picture;
                    IPictureFormat format = new TestPictureDecoder();
                    if (old != null) {
                        format.PicOffset = old.PicOffset;
                        format.PicWidth = old.PicWidth;
                        format.PicHeight = old.PicHeight;
                        format.ColorBPP = old.ColorBPP;
                    };
                    setPictureDecoder(format);
                };
                formatToolStripMenuItem.DropDownItems.Add(item);
            }
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
