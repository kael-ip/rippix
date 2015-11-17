using Rippix.Decoders;
using Rippix.Model;
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
        private PictureControllerHelper inputController;
        private IPalette palette;

        public Form1() {
            InitializeComponent();
            this.Icon = Rippix.Properties.Resources.MainIcon;
            this.ClientSize = new Size(800, 600);
            propertyGrid1.Height = 300;
            toolTip1.SetToolTip(pixelView1, helpText);
            CreateFormatMenuItems();
            CreateColorMenuItems();
            inputController = new PictureControllerHelper(pixelView1);
            palette = new GrayscalePalette();
        }

        void setPictureDecoder(IPictureDecoder decoder) {
            if (picture != null) {
                picture.Changed -= new EventHandler(Format_Changed);
            }
            var old = picture;
            picture = new PictureAdapter(decoder);
            if (picture != null) {
                if (old != null) {
                    picture.Data = old.Data;
                    picture.PicOffset = old.PicOffset;
                    picture.Width = old.Width;
                    picture.Height = old.Height;
                    picture.ColorBPP = old.ColorBPP;
                    picture.Zoom = old.Zoom;
                };
                if (decoder is INeedsPalette) {
                    ((INeedsPalette)decoder).Palette = palette;
                }
                picture.Changed += new EventHandler(Format_Changed);
                propertyGrid1.SelectedObject = picture;
                propertyGrid2.SelectedObject = picture.ColorFormat;
                Format_Changed(picture, EventArgs.Empty);
            }
            pixelView1.Format = picture;
            inputController.Format = picture;
        }

        void Format_Changed(object sender, EventArgs e) {
            if (inputController.Format != null) {
                pixelView1.Zoom = inputController.Format.Zoom;
                inputController.Format.Zoom = pixelView1.Zoom;
            }
            ((GrayscalePalette)palette).Length = 1 << picture.ColorBPP;
            propertyGrid1.Refresh();
            propertyGrid2.SelectedObject = picture.ColorFormat;
            propertyGrid2.Refresh();
            highlightColorItem();
            highlightFormatItem();
            pixelView1.Refresh();
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

        private void highlightFormatItem() {
            foreach (var item in formatToolStripMenuItem.DropDownItems) {
                ToolStripMenuItem menuItem = item as ToolStripMenuItem;
                if (menuItem != null) {
                    Type decoderType = menuItem.Tag as Type;
                    if (decoderType != null) {
                        menuItem.Checked = (picture != null)
                        && decoderType.IsInstanceOfType(picture.Decoder);
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
                    setPictureDecoder(new DirectDecoder());
                }
                picture.PicOffset = 0;
                picture.Width = 8;
                picture.Height = 8;
                picture.Data = data;
                highlightColorItem();
                highlightFormatItem();
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

        private ToolStripItem CreateColorMenuItem(string name, int bpp, ColorFormat colorFormat) {
            var item = new ToolStripMenuItem();
            item.Text = name;
            item.Tag = new FormatPreset(bpp, colorFormat);
            item.Click += (s, e) => {
                var item2 = (ToolStripItem)s;
                var preset = (FormatPreset)item2.Tag;
                if (picture == null) return;
                picture.ColorBPP = preset.ColorBPP;
                picture.ColorFormat = new ColorFormat(preset.ColorFormat);
            };
            return item;
        }

        private void CreateColorMenuItems() {
            var list = new List<ToolStripItem>();
            list.Add(CreateColorMenuItem("R8G8B8A8", 32, new ColorFormat(24, 8, 16, 8, 8, 8, 0, 8)));
            list.Add(CreateColorMenuItem("B8G8R8A8", 32, new ColorFormat(8, 8, 16, 8, 24, 8, 0, 8)));
            list.Add(CreateColorMenuItem("A8R8G8B8", 32, new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8)));
            list.Add(CreateColorMenuItem("A8B8G8R8", 32, new ColorFormat(0, 8, 8, 8, 16, 8, 24, 8)));
            list.Add(new ToolStripSeparator());
            list.Add(CreateColorMenuItem("A1R5G5B5", 16, new ColorFormat(10, 5, 5, 5, 0, 5, 15, 1)));
            list.Add(CreateColorMenuItem("R5G6B5", 16, new ColorFormat(11, 5, 5, 6, 0, 5, 0, 0)));
            list.Add(new ToolStripSeparator());
            list.Add(CreateColorMenuItem("R3G3B2", 8, new ColorFormat(5, 3, 2, 3, 0, 2, 0, 0)));
            list.Add(new ToolStripSeparator());
            list.Add(CreateColorMenuItem("R8G8B8", 24, new ColorFormat(16, 8, 8, 8, 0, 8, 24, 0)));
            list.Add(CreateColorMenuItem("B8G8R8", 24, new ColorFormat(0, 8, 8, 8, 16, 8, 24, 0)));
            colorToolStripMenuItem.DropDownItems.AddRange(list.ToArray());
        }

        private ToolStripItem CreateFormatMenuItem(string name, Type decoderType) {
            var item = new ToolStripMenuItem();
            item.Text = name;
            item.Tag = decoderType;
            item.Click += (s, e) => {
                var item2 = (ToolStripItem)s;
                if (item2.Tag is Type) {
                    var decoder = (IPictureDecoder)Activator.CreateInstance((Type)item2.Tag);
                    setPictureDecoder(decoder);
                }
            };
            return item;
        }

        private void CreateFormatMenuItems() {
            var list = new List<ToolStripItem>();
            list.Add(CreateFormatMenuItem("Direct", typeof(DirectDecoder)));
            list.Add(CreateFormatMenuItem("Packed", typeof(PackedDecoder)));
            list.Add(CreateFormatMenuItem("Planar (test)", typeof(TestPictureDecoder)));
            formatToolStripMenuItem.DropDownItems.AddRange(list.ToArray());
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
