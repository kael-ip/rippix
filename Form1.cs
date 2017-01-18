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
        private int paletteViewHeightFactor = 5;

        private PropertyGrid propertyGrid1;
        private PropertyGrid propertyGrid2;
        private PixelView pixelViewPicture;
        private PixelView pixelViewPalette;
        private ToolStrip toolStrip1;
        private TabControl tabControl;
        private ToolStripMenuItem colorToolStripMenuItem;
        private ToolStripMenuItem formatToolStripMenuItem;

        public Form1() {
            InitializeComponent();
            CreateComponents();
            this.viewModel = new ViewModel();
            this.viewModel.Changed += viewModel_Changed;
            this.Icon = Rippix.Properties.Resources.MainIcon;
            this.Text = "Rippix";
            this.ClientSize = new Size(800, 600);
            pixelViewPalette.Height = (1 << paletteViewHeightFactor);
            propertyGrid1.Height = 300;
            CreateFileMenuItems();
            formatToolStripMenuItem = CreateFormatMenuItems();
            colorToolStripMenuItem = CreateColorMenuItems();
            CreateSeekItems();
            seekController = new ImageSeekController();
            inputController = new KeyboardSeekControlHelper(pixelViewPicture);
            inputController.Controller = seekController;
            toolTip1.SetToolTip(pixelViewPicture, inputController.HelpText);
        }
        void CreateComponents() {
            this.propertyGrid1 = new PropertyGrid() {
                HelpVisible = false,
                ToolbarVisible = false,
                Dock = DockStyle.Top
            };
            this.propertyGrid2 = new PropertyGrid() {
                ToolbarVisible = false,
                Dock = DockStyle.Fill
            };
            this.tabControl = new TabControl() { Dock = DockStyle.Fill };
            this.tabControl.TabPages.Add("Properties");
            this.tabControl.TabPages.Add("Bookmarks");
            var panelRight = new Panel(){
                Dock = DockStyle.Right
            };
            tabControl.TabPages[0].Controls.Add(this.propertyGrid2);
            tabControl.TabPages[0].Controls.Add(this.propertyGrid1);
            panelRight.Controls.Add(tabControl);
            this.pixelViewPicture = new PixelView() { Dock = DockStyle.Fill };
            this.pixelViewPalette = new PixelView() { Dock = DockStyle.Top };
            this.toolStrip1 = new ToolStrip();
            panelMain.Controls.Add(this.pixelViewPicture);
            panelMain.Controls.Add(this.toolStrip1);
            panelMain.Controls.Add(new Splitter() { Dock = DockStyle.Right });
            panelMain.Controls.Add(panelRight);
            panelMain.Controls.Add(this.pixelViewPalette);

        }
        void viewModel_Changed(object sender, EventArgs e) {
            if (viewModel.Picture == null) return;
            propertyGrid1.SelectedObject = viewModel.Picture;
            propertyGrid2.SelectedObject = viewModel.ColorFormat;
            seekController.Format = viewModel.PictureAdapter;
            if (seekController.Format != null) {
                pixelViewPicture.Zoom = seekController.Format.Zoom;
                seekController.Format.Zoom = pixelViewPicture.Zoom;
            }
            propertyGrid1.Refresh();
            propertyGrid2.Refresh();
            highlightColorItem();
            highlightFormatItem();
            pixelViewPicture.Format = viewModel.Picture;
            pixelViewPicture.Refresh();
            pixelViewPalette.Format = viewModel.PalettePicture;
            pixelViewPalette.Zoom = pixelViewPalette.Height / viewModel.PalettePicture.ImageHeight - 1;
            pixelViewPalette.Refresh();
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
                pixelViewPicture.Bitmap.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
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
        private void CreateFileMenuItems() {
            var menuItem = new ToolStripMenuItem("File");
            {
                var item = new ToolStripMenuItem("Open");
                item.Click += openToolStripMenuItem_Click;
                menuItem.DropDownItems.Add(item);
            }
            {
                var item = new ToolStripMenuItem("Save Picture");
                item.Click += savePictureToolStripMenuItem_Click;
                menuItem.DropDownItems.Add(item);
            }
            menuStrip.Items.Add(menuItem);
        }
        private ToolStripMenuItem CreateColorMenuItems() {
            var menuItem = new ToolStripMenuItem("Color");
            menuItem.DropDownItems.Clear();
            MenuEventHandler onExecute = delegate(ToolStripMenuItem item) {
                if (item.Tag is ColorFormat) {
                    viewModel.SetColorFormat((ColorFormat)item.Tag);
                }
            };
            foreach (var preset in viewModel.GetAvailableColorFormats()) {
                var item = CreateMenuItem(preset, onExecute);
                menuItem.DropDownItems.Add(item);
            }
            menuStrip.Items.Add(menuItem);
            return menuItem;
        }
        private ToolStripMenuItem CreateFormatMenuItems() {
            var menuItem = new ToolStripMenuItem("Format");
            menuItem.DropDownItems.Clear();
            MenuEventHandler onExecute = delegate(ToolStripMenuItem item) {
                if (item.Tag is Type) {
                    viewModel.SetDecoder((Type)item.Tag);
                }
            };
            foreach (var preset in viewModel.GetAvailableDecoders()) {
                var item = CreateMenuItem(preset, onExecute);
                menuItem.DropDownItems.Add(item);
            }
            menuStrip.Items.Add(menuItem);
            return menuItem;
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
    }

    delegate void MenuEventHandler(ToolStripMenuItem item);

}
