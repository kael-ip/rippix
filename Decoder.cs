using Rippix.Model;
using System;
using System.ComponentModel;
using System.Drawing;

namespace Rippix {

    public interface IPictureAdapter {
        event EventHandler Changed;
        //byte[] Data { get; set; }
        int PicOffset { get; set; }
        int PicStride { get; }
        int PicWidth { get; set; }
        int PicHeight { get; set; }
        int GetARGBColor(byte[] data, int y, int x);
    }

    public interface IPictureFormat : IPictureAdapter {
        void SetPacking(int bpp, ColorFormat colorFormat);
        int ColorBPP { get; set; }
        ColorFormat ColorFormat { get; }
    }

    public class DirectPictureFormat : INotifyPropertyChanged, IPictureFormat {
        private int picOffset;
        private int picStride;
        private int picWidth;
        private int picHeight;
        private int colorBPP;
        private ColorFormat colorFormat;

        public DirectPictureFormat() {
            colorFormat = new ColorFormat();
            colorFormat.PropertyChanged += colorFormat_PropertyChanged;
            Reset();
        }
        void colorFormat_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnChanged("ColorFormat." + e.PropertyName);
        }
        public void Reset() {
            this.picOffset = 0;
            this.picWidth = 16;
            this.picHeight = 16;
            this.colorBPP = 32;
            SetPacking(32, 16, 8, 8, 8, 0, 8, 24, 8);
        }
        public int PicOffset { get { return picOffset; } set { SetIntPropertyValue("PicOffset", ref picOffset, value, 0, int.MaxValue); } }
        public int PicStride { get { return picStride; } private set { picStride = Math.Max(1, value); OnChanged("PicStride"); } }
        public int PicWidth {
            get { return picWidth; }
            set {
                if (CalcStride(Math.Max(1, value))) { SetIntPropertyValue("PicWidth", ref picWidth, value, 1, int.MaxValue); }
            }
        }
        public int PicHeight { get { return picHeight; } set { SetIntPropertyValue("PicHeight", ref picHeight, value, 1, int.MaxValue); } }
        public int ColorBPP { get { return colorBPP; } set { SetIntPropertyValue("BPP", ref colorBPP, value, 1, 32); } }
        public ColorFormat ColorFormat { get { return colorFormat; } }

        private void SetIntPropertyValue(string name, ref int store, int value, int min, int max) {
            if (value == store) return;
            store = Math.Max(min, Math.Min(max, value));
            OnChanged(name);
        }
        public void SetPacking(int bpp, ColorFormat format) {
            SetPacking(bpp, format.ShiftR, format.BitsR, format.ShiftG, format.BitsG, format.ShiftB, format.BitsB, format.ShiftA, format.BitsA);
        }
        public void SetPacking(int bpp, int shiftR, int bitsR, int shiftG, int bitsG, int shiftB, int bitsB, int shiftA, int bitsA) {
            ColorBPP = bpp;
            colorFormat.ShiftR = shiftR;
            colorFormat.BitsR = bitsR;
            colorFormat.ShiftG = shiftG;
            colorFormat.BitsG = bitsG;
            colorFormat.ShiftB = shiftB;
            colorFormat.BitsB = bitsB;
            colorFormat.ShiftA = shiftA;
            colorFormat.BitsA = bitsA;
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
        public int GetARGBColor(byte[] data, int y, int x) {
            int loffset = PicOffset + (y * PicStride);
            int v;
            v = GetValue(data, loffset, x, ColorBPP);
            v = colorFormat.Decode(v);
            return v;
        }
        private int GetValue(byte[] data, int loffset, int poffset, int bpp) {
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
        private void OnChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            if (Changed != null) Changed(this, EventArgs.Empty);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Changed;
    }

    public class IndexedPictureFormat : INotifyPropertyChanged, IPictureFormat {
        private int picOffset;
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
            colorFormat = new ColorFormat();
            colorFormat.PropertyChanged += colorFormat_PropertyChanged;
            Reset();
        }
        void colorFormat_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            palCacheDirty = true;
            OnChanged("ColorFormat." + e.PropertyName);
        }
        public void Reset() {
            this.picOffset = 0;
            this.picWidth = 16;
            this.picHeight = 16;
            this.colorBPP = 32;
            this.indexBPP = 8;
            this.palOffset = 0;
            this.palRelative = true;
            this.palCacheDirty = true;
            this.isFixedPalette = true;
            SetPacking(32, 16, 8, 8, 8, 0, 8, 24, 8);
        }
        public int PicOffset { get { return picOffset; } set { SetIntPropertyValue("PicOffset", ref picOffset, value, 0, int.MaxValue, PalRelative); } }
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
        public ColorFormat ColorFormat { get { return colorFormat; } }

        private void SetIntPropertyValue(string name, ref int store, int value, int min, int max, bool makeDirty) {
            if (value == store) return;
            store = Math.Max(min, Math.Min(max, value));
            if (makeDirty) palCacheDirty = true;
            OnChanged(name);
        }
        public void SetPacking(int bpp, ColorFormat format) {
            SetPacking(bpp, format.ShiftR, format.BitsR, format.ShiftG, format.BitsG, format.ShiftB, format.BitsB, format.ShiftA, format.BitsA);
        }
        public void SetPacking(int bpp, int shiftR, int bitsR, int shiftG, int bitsG, int shiftB, int bitsB, int shiftA, int bitsA) {
            ColorBPP = bpp;
            colorFormat.ShiftR = shiftR;
            colorFormat.BitsR = bitsR;
            colorFormat.ShiftG = shiftG;
            colorFormat.BitsG = bitsG;
            colorFormat.ShiftB = shiftB;
            colorFormat.BitsB = bitsB;
            colorFormat.ShiftA = shiftA;
            colorFormat.BitsA = bitsA;
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
        public int GetARGBColor(byte[] data, int y, int x) {
            int loffset = PicOffset + (y * PicStride);
            int v;
            v = GetValue(data, loffset, x, IndexBPP);
            v = LookupPalette(data, v);
            return v;
        }
        private int LookupPalette(byte[] data, int c) {
            if (palCacheDirty) {
                for (int i = 0; i <= 255; i++) {
                    int v = 0;
                    if (IsFixedPalette) {
                        if (IndexBPP > 0 && IndexBPP <= 8 && i < (1 << IndexBPP)) {
                            v = 255 * i / ((1 << IndexBPP) - 1);
                            v = ColorFormat.Pack(v, v, v, 255);
                        }
                    } else {
                        v = GetValue(data, GetPalOffset(), i, ColorBPP);
                        v = colorFormat.Decode(v);
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
        private int GetValue(byte[] data, int loffset, int poffset, int bpp) {
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
        private int GetPalOffset() {
            if (palRelative) {
                return PicOffset + GetPictureLength() + PalOffset;
            } else {
                return PalOffset;
            }
        }
        private void OnChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            if (Changed != null) Changed(this, EventArgs.Empty);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Changed;
    }

    public class TestPictureDecoder : IPictureFormat, INotifyPropertyChanged {
        private int picOffset;
        //private int picStride;
        //private int picWidth;
        //private int picHeight;
        //private int colorBPP;
        //private int indexBPP;
        //private int palOffset;
        //private bool palRelative;
        //private bool indexed;
        private int[] palCache;
        //private bool palCacheDirty;
        private ColorFormat colorFormat;
        //private bool isFixedPalette;
        public int PicOffset { get { return picOffset; } set { SetIntPropertyValue("PicOffset", ref picOffset, value, 0, int.MaxValue, false); } }
        //public int PicStride { get { return picStride; } private set { picStride = Math.Max(1, value); OnChanged("PicStride"); } }
        //public int PicWidth {
        //    get { return picWidth; }
        //    set {
        //        if (CalcStride(Math.Max(1, value))) { SetIntPropertyValue("PicWidth", ref picWidth, value, 1, int.MaxValue, PalRelative); }
        //    }
        //}
        //public int PicHeight { get { return picHeight; } set { SetIntPropertyValue("PicHeight", ref picHeight, value, 1, int.MaxValue, PalRelative); } }
        //public int ColorBPP { get { return colorBPP; } set { SetIntPropertyValue("BPP", ref colorBPP, value, 1, 32, true); } }
        private int PlanesCount = 4;
        public int PicWidth { get { return 320; } set { } }
        public int PicHeight { get { return 200; } set { } }
        public int PicStride { get { return PicWidth / 8 * PlanesCount; } }
        public void SetPacking(int bpp, ColorFormat colorFormat) {
        }
        public int ColorBPP { get { return 24; } set { } }
        public ColorFormat ColorFormat { get { return colorFormat; } }
        public int GetARGBColor(byte[] data, int y, int x) {
            int o = PicOffset + y * PicStride;
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
        private void OnChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            if (Changed != null) Changed(this, EventArgs.Empty);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Changed;
    }
}
