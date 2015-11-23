using Rippix.Decoders;
using Rippix.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rippix {
    public partial class Form1 : Form {

        private ViewModel viewModel;
        private KeyboardSeekControlHelper inputController;
        private ImageSeekController seekController;

        public Form1() {
            InitializeComponent();
            this.viewModel = new ViewModel();
            this.viewModel.Changed += viewModel_Changed;
            this.Icon = Rippix.Properties.Resources.MainIcon;
            this.Text = "Rippix";
            this.ClientSize = new Size(800, 600);
            pixelView2.Zoom = 16;
            propertyGrid1.Height = 300;
            toolTip1.SetToolTip(pixelView1, helpText);
            CreateFormatMenuItems();
            CreateColorMenuItems();
            CreateSeekItems();
            seekController = new ImageSeekController();
            inputController = new KeyboardSeekControlHelper(pixelView1);
            inputController.Controller = seekController;
        }
        void viewModel_Changed(object sender, EventArgs e) {
            if (viewModel.Picture == null) return;
            propertyGrid1.SelectedObject = viewModel.Picture;
            propertyGrid2.SelectedObject = viewModel.ColorFormat;
            seekController.Format = viewModel.PictureAdapter;
            if (seekController.Format != null) {
                pixelView1.Zoom = seekController.Format.Zoom;
                seekController.Format.Zoom = pixelView1.Zoom;
            }
            propertyGrid1.Refresh();
            propertyGrid2.Refresh();
            highlightColorItem();
            highlightFormatItem();
            pixelView1.Format = viewModel.Picture;
            pixelView1.Refresh();
            pixelView2.Format = viewModel.PalettePicture;
            pixelView2.Zoom = 8 - pixelView2.Format.ImageHeight;
            pixelView2.Height = pixelView2.Format.ImageHeight << pixelView2.Zoom;
            pixelView2.Refresh();
        }
        private void highlightColorItem() {
            foreach (var item in colorToolStripMenuItem.DropDownItems) {
                ToolStripMenuItem menuItem = item as ToolStripMenuItem;
                if (menuItem != null) {
                    menuItem.Checked = viewModel.IsCurrentPreset(menuItem.Tag);
                }
            }
        }
        private void highlightFormatItem() {
            foreach (var item in formatToolStripMenuItem.DropDownItems) {
                ToolStripMenuItem menuItem = item as ToolStripMenuItem;
                if (menuItem != null) {
                    menuItem.Checked = viewModel.IsCurrentPreset(menuItem.Tag);
                }
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try {
                viewModel.OpenDataFile(dlg.FileName);
            } catch(Exception ex) {
                ProcessError(ex);
            }
        }
        private void savePictureToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try {
                pixelView1.Bitmap.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
            } catch (Exception ex) {
                ProcessError(ex);
            }
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
        private void CreateSeekItems() {
            toolStrip1.Items.Clear();
            toolStrip1.Items.Add(CreateSeekCommandItem("<", ImageSeekCommand.ChangeOffsetByte, -1, true));
            toolStrip1.Items.Add(CreateSeekCommandItem(">", ImageSeekCommand.ChangeOffsetByte, 1, true));
            toolStrip1.Items.Add(CreateSeekCommandItem("<Line", ImageSeekCommand.ChangeOffsetLine, -1, true));
            toolStrip1.Items.Add(CreateSeekCommandItem("Line>", ImageSeekCommand.ChangeOffsetLine, 1, true));
            toolStrip1.Items.Add(CreateSeekCommandItem("<Frm", ImageSeekCommand.ChangeOffsetFrame, -1, false));
            toolStrip1.Items.Add(CreateSeekCommandItem("Frm>", ImageSeekCommand.ChangeOffsetFrame, 1, false));
            toolStrip1.Items.Add(CreateSeekCommandItem("W-", ImageSeekCommand.ChangeWidth, -1, true));
            toolStrip1.Items.Add(CreateSeekCommandItem("W+", ImageSeekCommand.ChangeWidth, 1, true));
            toolStrip1.Items.Add(CreateSeekCommandItem("H-", ImageSeekCommand.ChangeHeight, -1, true));
            toolStrip1.Items.Add(CreateSeekCommandItem("H+", ImageSeekCommand.ChangeHeight, 1, true));
            toolStrip1.Items.Add(CreateSeekCommandItem("Z-", ImageSeekCommand.ChangeZoom, -1, false));
            toolStrip1.Items.Add(CreateSeekCommandItem("Z+", ImageSeekCommand.ChangeZoom, 1, false));
        }
        private ToolStripItem CreateSeekCommandItem(string caption, ImageSeekCommand cmd, int step, bool useLeap) {
            var item = new ToolStripButton();
            item.Text = caption;
            item.Click += (s, e) => {
                var leap = ((Control.ModifierKeys & Keys.Shift) != Keys.None) ? 8 : 1;
                seekController.Execute(cmd, useLeap ? step * leap : step);
            };
            return item;
        }
        private void ProcessError(Exception ex) {
            System.Diagnostics.Trace.TraceError(ex.ToString());
            MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
