using Rippix.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Rippix.Decoders {

    public class PackedDecoder : IPictureDecoder, IPictureController, INotifyPropertyChanged {
        private int width;
        private int height;
        private int ppbyp;//pixels per byte power
        private ColorFormat colorFormat;
        public PackedDecoder() {
            this.width = 16;
            this.height = 16;
            this.ppbyp = 0;
            this.ColorFormat = new Rippix.ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
        }
        void colorFormat_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnChanged("ColorFormat." + e.PropertyName);
        }
        public int ColorBPP {
            get { return (1<<(3-ppbyp)); }//(3)8->1/(2)4->2/(1)2->4/(0)1->8
            set { SetIntPropertyValue("ColorBPP", ref ppbyp, fromBPP(value), 0, 3); }//{1/2/4/8}
        }
        private static int fromBPP(int v) {
            switch (v) {
                case 1: return 3;
                case 2: return 2;
                case 4: return 1;
                case 8: return 0;
                default: return 0; //throw new NotSupportedException();
            }
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
        public int ImageWidth { get { return (width << ppbyp); } }
        public int ImageHeight { get { return height; } }
        public int LineStride { get { return width; } }
        public int FrameStride { get { return LineStride * height; } }
        public int GetARGB(byte[] data, int offset, int x, int y) {
            int loffset = offset + (y * LineStride);
            int v;
            v = GetValue(data, loffset, x);
            v = ColorFormat.Decode(v);
            return v;
        }
        private int GetValue(byte[] data, int loffset, int poffset) {
            int offset = loffset + (poffset >> ppbyp);
            if (offset < 0 || offset >= data.Length) return 0;
            int v = 0;
            switch (ppbyp) {
                case 0:
                    v = data[offset];
                    break;
                case 1:
                    v = (data[offset] >> ((1 - (poffset & 1)) << 2)) & 15;
                    break;
                case 2:
                    v = (data[offset] >> ((3 - (poffset & 3)) << 1)) & 3;
                    break;
                case 3:
                    v = (data[offset] >> ((7 - (poffset & 7)) << 0)) & 1;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return v;
        }
        public void ReadParameters(IList<Parameter> parameters) {
            parameters.Peek("ppbyp", out ppbyp, 0);
            ColorFormat = new Rippix.ColorFormat(parameters.Peek("ColorFormat", ColorFormat.A8R8G8B8));
            parameters.Peek("Width", out width, 1);
            parameters.Peek("Height", out height, 8);
        }
        public void WriteParameters(IList<Parameter> parameters) {
            parameters.Poke("Width", width);
            parameters.Poke("Height", height);
            if (ColorFormat != null) {
                parameters.Poke("ColorFormat", ColorFormat);
            }
            parameters.Poke("ppbyp", ppbyp);
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
