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

        private ViewModel viewModel;
        private IPictureFormat picture;
        private PictureControllerHelper inputController;
        private IPalette palette;

        public Form1() {
            InitializeComponent();
            this.viewModel = new ViewModel();
            this.Icon = Rippix.Properties.Resources.MainIcon;
            this.Text = "Rippix";
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
                    ColorFormat cf = menuItem.Tag as ColorFormat;
                    if (cf != null) {
                        menuItem.Checked = (picture != null)
                        && cf.UsedBits == picture.ColorBPP
                        && Equals(cf, picture.ColorFormat);
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

        delegate void MenuEventHandler(ToolStripMenuItem item);

        private ToolStripItem CreateMenuItem(Preset preset, MenuEventHandler onExecute) {
            if (preset.Name == null) return new ToolStripSeparator();
            var item = new ToolStripMenuItem();
            item.Text = preset.Name;
            item.Tag = preset.Value;
            item.Click += (s, e) => {
                onExecute((ToolStripMenuItem)s);
            };
            return item;
        }

        private void CreateColorMenuItems() {
            colorToolStripMenuItem.DropDownItems.Clear();
            MenuEventHandler onExecute = delegate(ToolStripMenuItem item) {
                if (item.Tag is ColorFormat && picture != null) {
                    var cf = (ColorFormat)item.Tag;
                    picture.ColorBPP = cf.UsedBits;
                    picture.ColorFormat = new ColorFormat(cf);
                }
            };
            foreach (var preset in viewModel.GetAvailableColorFormats()) {
                var item = CreateMenuItem(preset, onExecute);
                colorToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void CreateFormatMenuItems() {
            formatToolStripMenuItem.DropDownItems.Clear();
            MenuEventHandler onExecute = delegate(ToolStripMenuItem item) {
                if (item.Tag is Type) {
                    var decoder = (IPictureDecoder)Activator.CreateInstance((Type)item.Tag);
                    setPictureDecoder(decoder);
                }
            };
            foreach (var preset in viewModel.GetAvailableDecoders()) {
                var item = CreateMenuItem(preset, onExecute);
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
