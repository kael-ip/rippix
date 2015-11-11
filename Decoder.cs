using Rippix.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Rippix {

    public interface IPictureAdapter {
        event EventHandler Changed;
        byte[] Data { get; set; }
        int PicOffset { get; set; }
        int PicStride { get; }
        int PicWidth { get; set; }
        int PicHeight { get; set; }
        int GetARGBColor(int y, int x);
    }

    public interface IPictureFormat : IPictureAdapter {
        int ColorBPP { get; set; }
        ColorFormat ColorFormat { get; set; }
    }

    public interface IPictureController {
        event EventHandler Changed;
        int Width { get; set; }
        int Height { get; set; }
        int ColorBPP { get; set; }
        ColorFormat ColorFormat { get; set; }
    }

    public class PictureAdapter : IPictureFormat, INotifyPropertyChanged {
        private IPictureDecoder decoder;
        private IPictureController pictureControl;
        private byte[] data;
        private int picOffset;
        public PictureAdapter(IPictureDecoder decoder) {
            this.decoder = decoder;
            this.pictureControl = decoder as IPictureController;
            if (this.pictureControl != null) {
                this.pictureControl.Changed += decoder_Changed;
            }
        }
        void decoder_Changed(object sender, EventArgs e) {
            OnChanged(null);
        }
        public IPictureDecoder Decoder { get { return decoder; } }
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
            set { if (pictureControl != null) { pictureControl.ColorBPP = value; } }
        }
        public ColorFormat ColorFormat {
            get { return (pictureControl != null) ? pictureControl.ColorFormat : null; }
            set { if (pictureControl != null) { pictureControl.ColorFormat = value; } }
        }
        public int PicOffset {
            get { return picOffset; }
            set { SetIntPropertyValue("PicOffset", ref picOffset, value, 0, int.MaxValue); }
        }
        public int PicStride {
            get { return decoder.LineStride; }
        }
        public int PicWidth {
            get { return (pictureControl != null) ? pictureControl.Width : 0; }
            set { if (pictureControl != null) { pictureControl.Width = value; } }
        }
        public int PicHeight {
            get { return (pictureControl != null) ? pictureControl.Height : 0; }
            set { if (pictureControl != null) { pictureControl.Height = value; } }
        }
        public int GetARGBColor(int y, int x) {
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

    public abstract class PictureFormatBase : INotifyPropertyChanged {
        protected int GetValue(byte[] data, int loffset, int poffset, int bpp) {
            int offset = loffset + ((poffset * bpp) >> 3);
            if (offset < 0 || offset + (bpp >> 3) >= data.Length) return 0;
            int v = 0;
            switch (bpp) {
                case 32:
                    v = ColorFormat.Pack(data[offset], data[offset + 1], data[offset + 2], data[offset + 3] << 24);
                    break;
                case 24:
                    v = ColorFormat.Pack(data[offset], data[offset + 1], data[offset + 2], 0);
                    break;
                case 16:
                    v = data[offset] | ((int)data[offset + 1] << 8);
                    break;
                case 8:
                    v = data[offset];
                    break;
                case 4:
                    v = (data[offset] >> ((1 - (poffset & 1)) << 2)) & 15;
                    break;
                case 2:
                    v = (data[offset] >> ((3 - (poffset & 3)) << 1)) & 3;
                    break;
                case 1:
                    v = (data[offset] >> ((7 - (poffset & 7)) << 0)) & 1;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return v;
        }
        protected void OnChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            if (Changed != null) Changed(this, EventArgs.Empty);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Changed;
    }

    public class DirectPictureFormat : PictureFormatBase, IPictureDecoder, IPictureController {
        private int picStride;
        private int picWidth;
        private int picHeight;
        private int colorBPP;
        private ColorFormat colorFormat;

        public DirectPictureFormat() {
            Reset();
        }
        void colorFormat_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnChanged("ColorFormat." + e.PropertyName);
        }
        public void Reset() {
            this.picWidth = 16;
            this.picHeight = 16;
            this.colorBPP = 32;
            this.ColorFormat = new Rippix.ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
        }
        public int PicStride { get { return picStride; } private set { picStride = Math.Max(1, value); OnChanged("PicStride"); } }
        public int PicWidth {
            get { return picWidth; }
            set {
                if (CalcStride(Math.Max(1, value))) { SetIntPropertyValue("PicWidth", ref picWidth, value, 1, int.MaxValue); }
            }
        }
        public int PicHeight { get { return picHeight; } set { SetIntPropertyValue("PicHeight", ref picHeight, value, 1, int.MaxValue); } }
        public int ColorBPP {
            get { return colorBPP; }
            set {
                if (CalcStride(PicWidth)) { SetIntPropertyValue("BPP", ref colorBPP, value, 1, 32); }
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
        private bool CalcStride(int width) {
            return CalcStride(width, ColorBPP);
        }
        private bool CalcStride(int width, int bpp) {
            if (bpp >= 8 || bpp == 4 || bpp == 2 || bpp == 1) {
                int w = width * bpp;
                if ((w & 7) != 0) return false;
                PicStride = w >> 3;
                return true;
            }
            return false;
        }
        private int GetPictureLength() {
            return PicHeight * PicStride;
        }
        public int GetARGBColor(byte[] data, int offset, int y, int x) {
            int loffset = offset + (y * PicStride);
            int v;
            v = GetValue(data, loffset, x, ColorBPP);
            v = ColorFormat.Decode(v);
            return v;
        }
        protected void SetIntPropertyValue(string name, ref int store, int value, int min, int max) {
            if (value == store) return;
            store = Math.Max(min, Math.Min(max, value));
            OnChanged(name);
        }
        public int ImageWidth { get { return PicWidth; } }
        public int ImageHeight { get { return PicHeight; } }
        public int LineStride { get { return PicStride; } }
        public int GetARGB(byte[] data, int offset, int x, int y) {
            return GetARGBColor(data, offset, y, x);
        }
        public void ReadParameters(IList<Parameter> parameters) {
            throw new NotImplementedException();
        }
        public void WriteParameters(IList<Parameter> parameters) {
            throw new NotImplementedException();
        }
        int IPictureController.Width {
            get { return this.PicWidth; }
            set { this.PicWidth = value; }
        }
        int IPictureController.Height {
            get { return this.PicHeight; }
            set { this.PicHeight = value; }
        }
        int IPictureController.ColorBPP {
            get { return this.ColorBPP; }
            set { this.ColorBPP = value; }
        }
        ColorFormat IPictureController.ColorFormat {
            get { return this.ColorFormat; }
            set { this.ColorFormat = value; }
        }
    }

    public class IndexedPictureFormat : PictureFormatBase, IPictureDecoder, IPictureController {
        private int picStride;
        private int picWidth;
        private int picHeight;
        private int colorBPP;
        private int indexBPP;
        private int palOffset;
        private bool palRelative;
        private int[] palCache;
        private bool palCacheDirty;
        private ColorFormat colorFormat;
        private bool isFixedPalette;

        public IndexedPictureFormat() {
            palCache = new int[256];
            Reset();
        }
        void colorFormat_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            palCacheDirty = true;
            OnChanged("ColorFormat." + e.PropertyName);
        }
        public void Reset() {
            this.picWidth = 16;
            this.picHeight = 16;
            this.colorBPP = 32;
            this.indexBPP = 8;
            this.palOffset = 0;
            this.palRelative = true;
            this.palCacheDirty = true;
            this.isFixedPalette = true;
            this.ColorFormat = new Rippix.ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
        }
        public int PicStride { get { return picStride; } private set { picStride = Math.Max(1, value); OnChanged("PicStride"); } }
        public int PicWidth {
            get { return picWidth; }
            set {
                if (CalcStride(Math.Max(1, value))) { SetIntPropertyValue("PicWidth", ref picWidth, value, 1, int.MaxValue, PalRelative); }
            }
        }
        public int PicHeight { get { return picHeight; } set { SetIntPropertyValue("PicHeight", ref picHeight, value, 1, int.MaxValue, PalRelative); } }
        public int ColorBPP { get { return colorBPP; } set { SetIntPropertyValue("ColorBPP", ref colorBPP, value, 1, 32, true); } }
        public int PalOffset { get { return palOffset; } set { SetIntPropertyValue("PalOffset", ref palOffset, value, 0, int.MaxValue, true); } }
        public bool PalRelative { get { return palRelative; } set { palRelative = value; palCacheDirty = true; OnChanged("PalRelative"); } }
        public int IndexBPP {
            get { return indexBPP; }
            set {
                if (CalcStride(PicWidth, value)) { SetIntPropertyValue("IndexBPP", ref  indexBPP, value, 1, 32, true); }
            }
        }
        public bool IsFixedPalette {
            get { return isFixedPalette; }
            set {
                if (isFixedPalette == value) return;
                isFixedPalette = value;
                palCacheDirty = true;
                OnChanged("IsFixedPalette");
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

        private void SetIntPropertyValue(string name, ref int store, int value, int min, int max, bool makeDirty) {
            if (value == store) return;
            store = Math.Max(min, Math.Min(max, value));
            if (makeDirty) palCacheDirty = true;
            OnChanged(name);
        }
        private bool CalcStride(int width) {
            return CalcStride(width, IndexBPP);
        }
        private bool CalcStride(int width, int bpp) {
            if (bpp >= 8 || bpp == 4 || bpp == 2 || bpp == 1) {
                int w = width * bpp;
                if ((w & 7) != 0) return false;
                PicStride = w >> 3;
                return true;
            }
            return false;
        }
        private int GetPictureLength() {
            return PicHeight * PicStride;
        }
        public int GetARGBColor(byte[] data, int offset, int y, int x) {
            int loffset = offset + (y * PicStride);
            int v;
            v = GetValue(data, loffset, x, IndexBPP);
            v = LookupPalette(data, offset, v);
            return v;
        }
        private int LookupPalette(byte[] data, int offset, int c) {
            if (palCacheDirty) {
                for (int i = 0; i <= 255; i++) {
                    int v = 0;
                    if (IsFixedPalette) {
                        if (IndexBPP > 0 && IndexBPP <= 8 && i < (1 << IndexBPP)) {
                            v = 255 * i / ((1 << IndexBPP) - 1);
                            v = ColorFormat.Pack(v, v, v, 255);
                        }
                    } else {
                        v = GetValue(data, GetPalOffset(offset), i, ColorBPP);
                        v = ColorFormat.Decode(v);
                    }
                    palCache[i] = v;
                }
                palCacheDirty = false;
            }
            if (c < 0 || c > 255) {
                return 0;
            }
            return palCache[c];
        }
        private int GetPalOffset(int offset) {
            if (palRelative) {
                return offset + GetPictureLength() + PalOffset;
            } else {
                return PalOffset;
            }
        }
        public int ImageWidth { get { return PicWidth; } }
        public int ImageHeight { get { return PicHeight; } }
        public int LineStride { get { return PicStride; } }
        public int GetARGB(byte[] data, int offset, int x, int y) {
            return GetARGBColor(data, offset, y, x);
        }
        public void ReadParameters(IList<Parameter> parameters) {
            throw new NotImplementedException();
        }
        public void WriteParameters(IList<Parameter> parameters) {
            throw new NotImplementedException();
        }
        int IPictureController.Width {
            get { return this.PicWidth; }
            set { this.PicWidth = value; }
        }
        int IPictureController.Height {
            get { return this.PicHeight; }
            set { this.PicHeight = value; }
        }
        int IPictureController.ColorBPP {
            get { return this.ColorBPP; }
            set { this.ColorBPP = value; }
        }
        ColorFormat IPictureController.ColorFormat {
            get { return this.ColorFormat; }
            set { this.ColorFormat = value; }
        }
    }

    public class TestPictureDecoder : INotifyPropertyChanged, IPictureDecoder, IPictureController {
        //private int[] palCache;
        //private ColorFormat colorFormat;
        private int PlanesCount = 4;
        public int PicWidth { get { return 320; } set { } }
        public int PicHeight { get { return 200; } set { } }
        public int PicStride { get { return PicWidth / 8 * PlanesCount; } }
        public void SetPacking(int bpp, ColorFormat colorFormat) {
        }
        public int ColorBPP { get { return 24; } set { } }
        public ColorFormat ColorFormat { get { return null; } set { } }
        public int GetARGBColor(byte[] data, int offset, int y, int x) {
            int o = offset + y * PicStride;
            int xby = x >> 3;
            int xbi = 7 - (x & 7);
            int v = 0;
            o = o + xby;
            for (int i = 0; i < PlanesCount; i++) {
                int p = ((GetData(data, o) >> xbi) & 1) << i;
                v = v | p;
                o = o + (PicWidth / 8);
            }
            v = v * 255 / ((1 << PlanesCount) - 1);
            return ColorFormat.Pack(v, v, v, 255);
        }
        private byte GetData(byte[] data, int offset) {
            if (offset < 0 || offset >= data.Length) return 0;
            return data[offset];
        }
        private void SetIntPropertyValue(string name, ref int store, int value, int min, int max, bool makeDirty) {
            if (value == store) return;
            store = Math.Max(min, Math.Min(max, value));
            //if (makeDirty) palCacheDirty = true;
            OnChanged(name);
        }
        public int ImageWidth { get { return PicWidth; } }
        public int ImageHeight { get { return PicHeight; } }
        public int LineStride { get { return PicStride; } }
        public int GetARGB(byte[] data, int offset, int x, int y) {
            return GetARGBColor(data, offset, y, x);
        }
        public void ReadParameters(IList<Parameter> parameters) {
            throw new NotImplementedException();
        }
        public void WriteParameters(IList<Parameter> parameters) {
            throw new NotImplementedException();
        }
        private void OnChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            if (Changed != null) Changed(this, EventArgs.Empty);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Changed;
        int IPictureController.Width {
            get { return 320; }
            set { }
        }
        int IPictureController.Height {
            get { return 200; }
            set { }
        }
        int IPictureController.ColorBPP {
            get { return 24; }
            set { }
        }
        ColorFormat IPictureController.ColorFormat {
            get { return null; }
            set { }
        }
    }
}
