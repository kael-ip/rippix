using Rippix.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Rippix.Decoders {

    public class PackedDecoder : IPictureDecoder, INeedsPalette, INotifyPropertyChanged {
        readonly DecoderProperties props;
        private int width;
        private int height;
        private int ppbyp;//pixels per byte power
        private IPalette palette;
        public PackedDecoder() {
            this.width = 16;
            this.height = 16;
            this.ppbyp = 0;
            this.props = new DecoderProperties(this);
        }
        void palette_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnChanged("Palette." + e.PropertyName);
        }
        public IPalette Palette {
            get { return palette; }
            set {
                if (Equals(palette, value)) return;
                if ((palette as INotifyPropertyChanged) != null) {
                    ((INotifyPropertyChanged)palette).PropertyChanged -= palette_PropertyChanged;
                }
                palette = value;
                if ((palette as INotifyPropertyChanged) != null) {
                    ((INotifyPropertyChanged)palette).PropertyChanged += palette_PropertyChanged;
                }
                OnChanged("Palette");
            }
        }
        public int ColorCount { get { return 1 << ColorBPP; } }
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
            //v = ColorFormat.Decode(v);
            if (Palette != null) {
                v = Palette.GetARGB(v);
            }
            return v;
        }
        private int GetValue(byte[] data, int loffset, int poffset) {
            int offset = loffset + (poffset >> ppbyp);
            if (data == null || offset < 0 || offset >= data.Length) return 0;
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
            parameters.Peek("Width", out width, 1);
            parameters.Peek("Height", out height, 8);
        }
        public void WriteParameters(IList<Parameter> parameters) {
            parameters.Poke("Width", width);
            parameters.Poke("Height", height);
            parameters.Poke("ppbyp", ppbyp);
        }
        protected void OnChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public object Properties { get { return props; } }

        public class DecoderProperties : ISizeController {
            readonly PackedDecoder self;
            public DecoderProperties(PackedDecoder self) {
                this.self = self;
            }
            public int Width {
                get { return self.width; }
                set { self.width = value; }
            }
            public int Height {
                get { return self.height; }
                set { self.height = value; }
            }
            public int ColorBPP {
                get { return self.ColorBPP; }
                set { self.ColorBPP = value; }
            }
        }
    }
}
