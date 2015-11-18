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
        private PictureControllerHelper inputController;

        public Form1() {
            InitializeComponent();
            this.viewModel = new ViewModel();
            this.viewModel.Changed += viewModel_Changed;
            this.Icon = Rippix.Properties.Resources.MainIcon;
            this.Text = "Rippix";
            this.ClientSize = new Size(800, 600);
            propertyGrid1.Height = 300;
            toolTip1.SetToolTip(pixelView1, helpText);
            CreateFormatMenuItems();
            CreateColorMenuItems();
            inputController = new PictureControllerHelper(pixelView1);
        }
        void viewModel_Changed(object sender, EventArgs e) {
            if (viewModel.Picture == null) return;
            propertyGrid1.SelectedObject = viewModel.Picture;
            propertyGrid2.SelectedObject = viewModel.ColorFormat;
            inputController.Format = viewModel.PictureAdapter;
            if (inputController.Format != null) {
                pixelView1.Zoom = inputController.Format.Zoom;
                inputController.Format.Zoom = pixelView1.Zoom;
            }
            propertyGrid1.Refresh();
            propertyGrid2.Refresh();
            highlightColorItem();
            highlightFormatItem();
            pixelView1.Format = viewModel.Picture;
            pixelView1.Refresh();
        }
        private void highlightColorItem() {
            foreach (var item in colorToolStripMenuItem.DropDownItems) {
                ToolStripMenuItem menuItem = item as ToolStripMenuItem;
                if (menuItem != null) {
                    ColorFormat cf = menuItem.Tag as ColorFormat;
                    if (cf != null) {
                        menuItem.Checked = (viewModel.Picture != null)
                        && Equals(cf, viewModel.ColorFormat);
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
                        menuItem.Checked = (viewModel.Picture != null)
                        && decoderType.IsInstanceOfType(viewModel.Decoder);
                    }
                }
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try {
                viewModel.OpenDataFile(dlg.FileName);
            } catch { }
        }
        private void savePictureToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try {
                pixelView1.Bitmap.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
            } catch { }
        }
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
                if (item.Tag is ColorFormat) {
                    viewModel.SetColorFormat((ColorFormat)item.Tag);
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
                    viewModel.SetDecoder((Type)item.Tag);
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

    delegate void MenuEventHandler(ToolStripMenuItem item);

}
