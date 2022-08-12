using System;
using System.Collections.Generic;
using System.ComponentModel;
using Rippix.Model;

namespace Rippix.Decoders {

    public class TestPictureDecoder : INotifyPropertyChanged, IPictureDecoder, IPictureDecoderController {
        private int planesCount = 4;
        private int width = 40;
        private int height = 200;
        private byte GetData(byte[] data, int offset) {
            if(data == null || offset < 0 || offset >= data.Length)
                return 0;
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
            for(int i = 0; i < planesCount; i++) {
                int p = ((GetData(data, o) >> xbi) & 1) << i;
                v = v | p;
                o = o + width;
            }
            v = v * 255 / ((1 << planesCount) - 1);
            return ColorFormat.Pack(v, v, v, 255);
        }
        public void ReadParameters(IList<Parameter> parameters) {
            //throw new NotImplementedException();
        }
        public void WriteParameters(IList<Parameter> parameters) {
            //throw new NotImplementedException();
        }
        protected void SetIntPropertyValue(string name, ref int store, int value, int min, int max) {
            if(value == store)
                return;
            store = Math.Max(min, Math.Min(max, value));
            OnChanged(name);
        }
        private void OnChanged(string propertyName) {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            if(Changed != null)
                Changed(this, EventArgs.Empty);
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
