using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rippix.Model;

namespace Rippix.Decoders {

    public class PlanarDecoder : IPictureDecoder, INeedsPalette, INotifyPropertyChanged {
        readonly DecoderProperties props;
        private int width;
        private int height;
        private int planesCount;
        private bool lineInterleave;
        private IPalette palette;
        public PlanarDecoder() {
            this.width = 16;
            this.height = 16;
            this.planesCount = 1;
            this.props = new DecoderProperties(this);
        }
        void palette_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnChanged("Palette." + e.PropertyName);
        }
        public IPalette Palette {
            get { return palette; }
            set {
                if(Equals(palette, value))
                    return;
                if((palette as INotifyPropertyChanged) != null) {
                    ((INotifyPropertyChanged)palette).PropertyChanged -= palette_PropertyChanged;
                }
                palette = value;
                if((palette as INotifyPropertyChanged) != null) {
                    ((INotifyPropertyChanged)palette).PropertyChanged += palette_PropertyChanged;
                }
                OnChanged("Palette");
            }
        }
        public int ColorCount { get { return 1 << planesCount; } }
        public int PlanesCount {
            get { return planesCount; }
            set { SetIntPropertyValue(nameof(PlanesCount), ref planesCount, value, 1, 8); }
        }
        public bool LineInterleave {
            get { return lineInterleave; }
            set {
                if(lineInterleave == value)
                    return;
                lineInterleave = value;
                OnChanged(nameof(LineInterleave));
            }
        }
        protected void SetIntPropertyValue(string name, ref int store, int value, int min, int max) {
            if(value == store)
                return;
            store = Math.Max(min, Math.Min(max, value));
            OnChanged(name);
        }
        public int ImageWidth { get { return (width << 3); } }
        public int ImageHeight { get { return height; } }
        public int LineStride { get { return lineInterleave ? (width * planesCount) : width; } }
        public int FrameStride { get { return width * height * planesCount; } }
        public int GetARGB(byte[] data, int offset, int x, int y) {
            int loffset = offset + y * LineStride;
            int xby = x >> 3;
            int xbi = 7 - (x & 7);
            int v = 0;
            loffset = loffset + xby;
            for(int i = 0; i < planesCount; i++) {
                int p = ((GetValue(data, loffset) >> xbi) & 1) << i;
                v = v | p;
                if(lineInterleave) {
                    loffset = loffset + width;
                } else {
                    loffset = loffset + width * height;
                }
            }
            if(Palette != null) {
                v = Palette.GetARGB(v);
            }
            return v;
        }
        private byte GetValue(byte[] data, int offset) {
            if(data == null || offset < 0 || offset >= data.Length)
                return 0;
            return data[offset];
        }
        public void ReadParameters(IList<Parameter> parameters) {
            parameters.Peek("PlanesCount", out planesCount, 1);
            parameters.Peek("Width", out width, 1);
            parameters.Peek("Height", out height, 8);
        }
        public void WriteParameters(IList<Parameter> parameters) {
            parameters.Poke("PlanesCount", planesCount);
            parameters.Poke("Width", width);
            parameters.Poke("Height", height);
        }
        protected void OnChanged(string propertyName) {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public object Properties { get { return props; } }

        public class DecoderProperties : ISizeController {
            readonly PlanarDecoder self;
            public DecoderProperties(PlanarDecoder self) {
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
            public int NumPlanes {
                get { return self.PlanesCount; }
                set { self.PlanesCount = value; }
            }
            public bool LineInterleave {
                get { return self.lineInterleave; }
                set { self.LineInterleave = value; }
            }
        }
    }
}
