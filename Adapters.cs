using Rippix.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Rippix {

    public interface IPicture {
        int ImageWidth { get; }
        int ImageHeight { get; }
        int GetARGB(int x, int y);
    }

    public interface IPictureAdapter {
        int PicOffset { get; set; }
        int LineStride { get; }
        int FrameStride { get; }
        int Width { get; set; }
        int Height { get; set; }
        int MaxOffset { get; }
        int Zoom { get; set; }
        int TileColumns { get; set; }
        int TileRows { get; set; }
    }

    public interface ISizeController {
        int Width { get; set; }
        int Height { get; set; }
    }

    public interface IColorFormatController {
        ColorFormat ColorFormat { get; set; }
    }

    public class PictureAdapter : IPicture, IPictureAdapter, INotifyPropertyChanged {
        private IPictureDecoder decoder;
        private byte[] data;
        private int picOffset;
        private int zoom;
        private int tileColumns = 1;
        private int tileRows = 1;
        private int tileCount = 1;
        public PictureAdapter(IPictureDecoder decoder) {
            this.decoder = decoder;
            var pictureControl = decoder as INotifyPropertyChanged;
            if(pictureControl != null) {
                pictureControl.PropertyChanged += decoder_Changed;
            }
        }
        void decoder_Changed(object sender, EventArgs e) {
            OnChanged(null);
        }
        public IPictureDecoder Decoder { get { return decoder; } }
        [Browsable(false)]
        public byte[] Data {
            get { return data; }
            set {
                if (data == value) return;
                data = value;
                OnChanged("Data");
            }
        }
        public ColorFormat ColorFormat {
            get { return (decoder.Properties as IColorFormatController)?.ColorFormat; }
            set { if(decoder.Properties is IColorFormatController) { ((IColorFormatController)decoder.Properties).ColorFormat = value; OnChanged(""); } }
        }
        public int PicOffset {
            get { return picOffset; }
            set { SetIntPropertyValue("PicOffset", ref picOffset, value, 0, int.MaxValue); }
        }
        public int LineStride {
            get { return decoder.LineStride; }
        }
        public int FrameStride {
            get { return decoder.LineStride * decoder.ImageHeight; }
        }
        public int Width {
            get { return (decoder.Properties is ISizeController) ? ((ISizeController)decoder.Properties).Width : 0; }
            set { if(decoder.Properties is ISizeController) { ((ISizeController)decoder.Properties).Width = value; OnChanged(""); } }
        }
        public int Height {
            get { return (decoder.Properties is ISizeController) ? ((ISizeController)decoder.Properties).Height : 0; }
            set { if(decoder.Properties is ISizeController) { ((ISizeController)decoder.Properties).Height = value; OnChanged(""); } }
        }
        public int Zoom {
            get { return zoom; }
            set { SetIntPropertyValue(nameof(Zoom), ref zoom, value, 0, 256); }
        }
        public int TileColumns {
            get { return tileColumns; }
            set {
                if(SetIntPropertyValue(nameof(TileColumns), ref tileColumns, value, 1, 256)) {
                    UpdateTileCount();
                }
            }
        }
        public int TileRows {
            get { return tileRows; }
            set {
                if(SetIntPropertyValue(nameof(TileRows), ref tileRows, value, 1, 256)) {
                    UpdateTileCount();
                }
            }
        }
        private void UpdateTileCount() {
            tileCount = tileColumns * tileRows;
            OnChanged(nameof(TileCount));
        }
        [Browsable(false)]
        public int TileCount {
            get { return tileCount; }
            set { SetIntPropertyValue(nameof(TileCount), ref tileCount, value, 1, tileColumns * tileRows); }
        }
        public int MaxOffset {
            get { return (Data == null) ? 0 : Data.Length; }
        }
        public int ImageWidth { get { return decoder.ImageWidth * tileColumns; } }
        public int ImageHeight { get { return decoder.ImageHeight * tileRows; } }
        public int GetARGB(int x, int y) {
            int tx = x / decoder.ImageWidth;
            int ty = y / decoder.ImageHeight;
            int offset = PicOffset + decoder.FrameStride * (tx + ty * tileColumns);
            return decoder.GetARGB(Data, offset, x % decoder.ImageWidth, y % decoder.ImageHeight);
        }
        private bool SetIntPropertyValue(string name, ref int store, int value, int min, int max) {
            if (value == store) return false;
            store = Math.Max(min, Math.Min(max, value));
            OnChanged(name);
            return true;
        }
        private void OnChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            if (Changed != null) Changed(this, EventArgs.Empty);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Changed;
    }

    public class PalettePictureAdapter : IPicture {
        private IPalette palette;
        public PalettePictureAdapter(IPalette palette) {
            this.palette = palette;
        }
        public int Length { get; set; }
        public int ImageWidth {
            get { return Length / ImageHeight; }
        }
        public int ImageHeight {
            get { return (1 << (3 - Zoom)); }
        }
        public int Zoom {
            get {
                if (Length <= 16) return 3;// 16*1
                if (Length <= 64) return 2;// 32*2
                if (Length <= 256) return 1;// 64*4
                return 0;
            }
        }
        public int GetARGB(int x, int y) {
            if (x < 0 || y < 0) return 0;
            if (x >= ImageWidth || y >= ImageHeight) return 0;
            return palette.GetARGB(y * ImageWidth + x);
        }
    }

}
