using Rippix.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Rippix.Decoders {

    public class DirectDecoder : IPictureDecoder, IPictureController, INotifyPropertyChanged {
        private int width;
        private int height;
        private int bypp;//bytes ber pixel
        private ColorFormat colorFormat;
        public DirectDecoder() {
            this.width = 16;
            this.height = 16;
            this.bypp = 4;
            this.ColorFormat = new Rippix.ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
        }
        void colorFormat_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnChanged("ColorFormat." + e.PropertyName);
        }
        public int ColorBPP {
            get { return bypp * 8; }
            set { SetIntPropertyValue("ColorBPP", ref bypp, ((value + 7) & 0xf8), 1, 4); }//{8/16/24/32}
        }
        public ColorFormat ColorFormat {
            get { return colorFormat; }
            set {
                if (Equals(colorFormat, value)) return;
                if (colorFormat != null) {
                    colorFormat.PropertyChanged -= colorFormat_PropertyChanged;
                }
                colorFormat = value;
                if (colorFormat != null) {
                    colorFormat.PropertyChanged += colorFormat_PropertyChanged;
                }
                OnChanged("ColorFormat");
            }
        }
        protected void SetIntPropertyValue(string name, ref int store, int value, int min, int max) {
            if (value == store) return;
            store = Math.Max(min, Math.Min(max, value));
            OnChanged(name);
        }
        public int ImageWidth { get { return width; } }
        public int ImageHeight { get { return height; } }
        public int LineStride { get { return width * bypp; } }
        public int FrameStride { get { return LineStride * height; } }
        public int GetARGB(byte[] data, int offset, int x, int y) {
            int loffset = offset + (y * LineStride);
            int v;
            v = GetValue(data, loffset, x);
            v = ColorFormat.Decode(v);
            return v;
        }
        private int GetValue(byte[] data, int loffset, int poffset) {
            int offset = loffset + (poffset * bypp);
            if (offset < 0 || offset + bypp >= data.Length) return 0;
            int v = 0;
            switch (bypp) {
                case 4:
                    v = ColorFormat.Pack(data[offset], data[offset + 1], data[offset + 2], data[offset + 3] << 24);
                    break;
                case 3:
                    v = ColorFormat.Pack(data[offset], data[offset + 1], data[offset + 2], 0);
                    break;
                case 2:
                    v = data[offset] | ((int)data[offset + 1] << 8);
                    break;
                case 1:
                    v = data[offset];
                    break;
                default:
                    throw new NotSupportedException();
            }
            return v;
        }
        public void ReadParameters(IList<Parameter> parameters) {
            throw new NotImplementedException();
        }
        public void WriteParameters(IList<Parameter> parameters) {
            throw new NotImplementedException();
        }
        protected void OnChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            if (Changed != null) Changed(this, EventArgs.Empty);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Changed;
        #region IPictureController
        int IPictureController.Width {
            get { return this.width; }
            set { this.width = value; }
        }
        int IPictureController.Height {
            get { return this.height; }
            set { this.height = value; }
        }
        int IPictureController.ColorBPP {
            get { return this.ColorBPP; }
            set { this.ColorBPP = value; }
        }
        ColorFormat IPictureController.ColorFormat {
            get { return this.ColorFormat; }
            set { this.ColorFormat = value; }
        }
        #endregion
    }
}
