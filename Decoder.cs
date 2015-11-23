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
    }

    public interface IPictureFormat : IPicture, IPictureAdapter {
        byte[] Data { get; set; }
        int ColorBPP { get; set; }
        ColorFormat ColorFormat { get; set; }
        IPictureDecoder Decoder { get; }
        event EventHandler Changed;
    }

    public interface IPictureDecoderController {
        event EventHandler Changed;
        int Width { get; set; }
        int Height { get; set; }
        int ColorBPP { get; set; }
        ColorFormat ColorFormat { get; set; }
    }

    public class PictureAdapter : IPictureFormat, INotifyPropertyChanged {
        private IPictureDecoder decoder;
        private IPictureDecoderController pictureControl;
        private byte[] data;
        private int picOffset;
        private int zoom;
        public PictureAdapter(IPictureDecoder decoder) {
            this.decoder = decoder;
            this.pictureControl = decoder as IPictureDecoderController;
            if (this.pictureControl != null) {
                this.pictureControl.Changed += decoder_Changed;
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
        public int ColorBPP {
            get { return (pictureControl != null) ? pictureControl.ColorBPP : 0; }
            set { if (pictureControl != null) { pictureControl.ColorBPP = value; OnChanged(""); } }
        }
        public ColorFormat ColorFormat {
            get { return (pictureControl != null) ? pictureControl.ColorFormat : null; }
            set { if (pictureControl != null) { pictureControl.ColorFormat = value; OnChanged(""); } }
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
            get { return (pictureControl != null) ? pictureControl.Width : 0; }
            set { if (pictureControl != null) { pictureControl.Width = value; OnChanged(""); } }
        }
        public int Height {
            get { return (pictureControl != null) ? pictureControl.Height : 0; }
            set { if (pictureControl != null) { pictureControl.Height = value; OnChanged(""); } }
        }
        public int Zoom {
            get { return zoom; }
            set {
                if (zoom == value) return;
                zoom = value;
                OnChanged("Zoom");
            }
        }
        public int MaxOffset {
            get { return (Data == null) ? 0 : Data.Length; }
        }
        public int ImageWidth { get { return decoder.ImageWidth; } }
        public int ImageHeight { get { return decoder.ImageHeight; } }
        public int GetARGB(int x, int y) {
            return decoder.GetARGB(Data, PicOffset, x, y);
        }
        private void SetIntPropertyValue(string name, ref int store, int value, int min, int max) {
            if (value == store) return;
            store = Math.Max(min, Math.Min(max, value));
            OnChanged(name);
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
            get { return Length/ImageHeight; }
        }
        public int ImageHeight {
            get {
                if (Length <= 16) return 1;
                if (Length <= 32) return 2;
                if (Length <= 64) return 2;
                if (Length <= 128) return 4;
                if (Length <= 256) return 4;
                return 8;
            }
        }
        public int GetARGB(int x, int y) {
            if (x < 0 || y < 0) return 0;
            if (x >= ImageWidth || y >= ImageHeight) return 0;
            return palette.GetARGB(y * ImageWidth + x);
        }
    }

    public class TestPictureDecoder : INotifyPropertyChanged, IPictureDecoder, IPictureDecoderController {
        private int planesCount = 4;
        private int width = 40;
        private int height = 200;
        private byte GetData(byte[] data, int offset) {
            if (data == null || offset < 0 || offset >= data.Length) return 0;
            return data[offset];
        }
        public int ImageWidth { get { return width * 8; } }
        public int ImageHeight { get { return height; } }
        public int LineStride { get { return width * planesCount; } }
        public int FrameStride { get { return LineStride * height; } }
        public int GetARGB(byte[] data, int offset, int x, int y) {
            int o = offset + y * LineStride;
            int xby = x >> 3;
            int xbi = 7 - (x & 7);
            int v = 0;
            o = o + xby;
            for (int i = 0; i < planesCount; i++) {
                int p = ((GetData(data, o) >> xbi) & 1) << i;
                v = v | p;
                o = o + width;
            }
            v = v * 255 / ((1 << planesCount) - 1);
            return ColorFormat.Pack(v, v, v, 255);
        }
        public void ReadParameters(IList<Parameter> parameters) {
            throw new NotImplementedException();
        }
        public void WriteParameters(IList<Parameter> parameters) {
            throw new NotImplementedException();
        }
        protected void SetIntPropertyValue(string name, ref int store, int value, int min, int max) {
            if (value == store) return;
            store = Math.Max(min, Math.Min(max, value));
            OnChanged(name);
        }
        private void OnChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            if (Changed != null) Changed(this, EventArgs.Empty);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Changed;
        #region IPictureController
        int IPictureDecoderController.Width {
            get { return width; }
            set { SetIntPropertyValue("Width", ref width, value, 0, int.MaxValue); }
        }
        int IPictureDecoderController.Height {
            get { return height; }
            set { SetIntPropertyValue("Height", ref height, value, 0, int.MaxValue); }
        }
        int IPictureDecoderController.ColorBPP {
            get { return 24; }
            set { }
        }
        ColorFormat IPictureDecoderController.ColorFormat {
            get { return null; }
            set { }
        }
        #endregion
    }
}
