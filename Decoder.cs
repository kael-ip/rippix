using System;
using System.ComponentModel;
using System.Drawing;

namespace Rippix {

    /*
     * Modes:
     * - Raw RGBA (32/24/16/8 bpp, different orders)
     * - Indexed with palette
     * -- packed (8/4/2/1 bpp)
     * -- planes (1..8)
     * 
     * 
     * Direct/Indexed
     * bpp (32/24/16/8/4/2/1)
     * planes
     */

    //TODO: fixed palettes
    //+TODO: optimize / native impl
    //TODO: bitplanes
    //TODO: exotic modes

    public class ColorFormat : INotifyPropertyChanged {
        private int shiftR, shiftG, shiftB, shiftA;
        private int bitsR, bitsG, bitsB, bitsA;

        public ColorFormat() {
        }
        public ColorFormat(int shiftR, int bitsR, int shiftG, int bitsG, int shiftB, int bitsB, int shiftA, int bitsA) {
            this.ShiftR = shiftR;
            this.BitsR = bitsR;
            this.ShiftG = shiftG;
            this.BitsG = bitsG;
            this.ShiftB = shiftB;
            this.BitsB = bitsB;
            this.ShiftA = shiftA;
            this.BitsA = bitsA;
        }

        public int ShiftR { get { return shiftR; } set { SetIntPropertyValue("ShiftR", ref shiftR, value, 0, 32, true); } }
        public int ShiftG { get { return shiftG; } set { SetIntPropertyValue("ShiftG", ref shiftG, value, 0, 32, true); } }
        public int ShiftB { get { return shiftB; } set { SetIntPropertyValue("ShiftB", ref shiftB, value, 0, 32, true); } }
        public int ShiftA { get { return shiftA; } set { SetIntPropertyValue("ShiftA", ref shiftA, value, 0, 32, true); } }
        public int BitsR { get { return bitsR; } set { SetIntPropertyValue("BitsR", ref bitsR, value, 0, 12, true); } }
        public int BitsG { get { return bitsG; } set { SetIntPropertyValue("BitsG", ref bitsG, value, 0, 12, true); } }
        public int BitsB { get { return bitsB; } set { SetIntPropertyValue("BitsB", ref bitsB, value, 0, 12, true); } }
        public int BitsA { get { return bitsA; } set { SetIntPropertyValue("BitsA", ref bitsA, value, 0, 12, true); } }

        private void SetIntPropertyValue(string name, ref int store, int value, int min, int max, bool makeDirty) {
            if (value == store) return;
            store = Math.Max(min, Math.Min(max, value));
            OnChanged(name);
        }
        public static int Pack(int c0, int c1, int c2, int c3) {
            return c0 | (c1 << 8) | (c2 << 16) | (c3 << 24);
        }
        public int Decode(int v) {
            int cr = ((v >> ShiftR) & ((1 << BitsR) - 1)) * 255 / ((1 << BitsR) - 1);
            int cg = ((v >> ShiftG) & ((1 << BitsG) - 1)) * 255 / ((1 << BitsG) - 1);
            int cb = ((v >> ShiftB) & ((1 << BitsB) - 1)) * 255 / ((1 << BitsB) - 1);
            if (BitsA == 0) {
                v = Pack(cr, cg, cb, 255);
            } else {
                int ca = ((v >> ShiftA) & ((1 << BitsA) - 1)) * 255 / ((1 << BitsA) - 1);
                v = Pack(cr, cg, cb, ca);
            }
            return v;
        }
        private void OnChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public override bool Equals(object obj) {
            if (obj is ColorFormat) {
                ColorFormat other = (ColorFormat)obj;
                return (ShiftA == other.ShiftA) && (ShiftR == other.ShiftR) && (ShiftG == other.ShiftG) && (ShiftB == other.ShiftB)
                    && (BitsA == other.BitsA) && (BitsR == other.BitsR) && (BitsG == other.BitsG) && (BitsB == other.BitsB);
            }
            return base.Equals(obj);
        }
        public override int GetHashCode() {
            return (ShiftA + ShiftR + ShiftG + ShiftB) ^ (BitsA + BitsR + BitsG + BitsB);
        }
    }

    public class PictureFormat : INotifyPropertyChanged {
        private byte[] data;
        private int picOffset;
        private int picStride;
        private int picWidth;
        private int picHeight;
        private int colorBPP;
        private int indexBPP;
        private int palOffset;
        private bool palRelative;
        private bool indexed;
        private int[] palCache;
        private bool palCacheDirty;
        private ColorFormat colorFormat;
        private bool isFixedPalette;

        public PictureFormat() {
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
            this.data = null;
            this.picOffset = 0;
            this.picWidth = 16;
            this.picHeight = 16;
            this.colorBPP = 32;
            this.indexBPP = 8;
            this.palOffset = 0;
            this.palRelative = true;
            this.indexed = false;
            this.palCacheDirty = true;
            this.isFixedPalette = true;
            SetPacking(32, 16, 8, 8, 8, 0, 8, 24, 8);
        }
        [Browsable(false)]
        public byte[] Data { get { return data; } set { data = value; palCacheDirty = true; OnChanged("Data"); } }
        public int PicOffset { get { return picOffset; } set { SetIntPropertyValue("PicOffset", ref picOffset, value, 0, int.MaxValue, PalRelative); } }
        public int PicStride { get { return picStride; } private set { picStride = Math.Max(1, value); OnChanged("PicStride"); } }
        public int PicWidth {
            get { return picWidth; }
            set {
                if (CalcStride(Math.Max(1, value))) { SetIntPropertyValue("PicWidth", ref picWidth, value, 1, int.MaxValue, PalRelative); }
            }
        }
        public int PicHeight { get { return picHeight; } set { SetIntPropertyValue("PicHeight", ref picHeight, value, 1, int.MaxValue, PalRelative); } }
        public int ColorBPP { get { return colorBPP; } set { SetIntPropertyValue("BPP", ref colorBPP, value, 1, 32, true); } }
        public int PalOffset { get { return palOffset; } set { SetIntPropertyValue("PalOffset", ref palOffset, value, 0, int.MaxValue, true); } }
        public bool Indexed { get { return indexed; } set { indexed = value; palCacheDirty = true; OnChanged("Indexed"); } }
        public bool PalRelative { get { return palRelative; } set { palRelative = value; palCacheDirty = true; OnChanged("PalRelative"); } }
        public int IndexBPP {
            get { return indexBPP; }
            set {
                if (CalcStride(PicWidth, value)) { SetIntPropertyValue("PicBPP", ref  indexBPP, value, 1, 32, true); }
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
            return CalcStride(width, Indexed ? IndexBPP : ColorBPP);
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
        public System.Drawing.Size GetBitmapSize() {
            return new Size(PicWidth, picHeight);
        }
        private int GetPictureLength() {
            return PicHeight * PicStride;
        }
        public int GetRGBAColor(int y, int x) {
            int loffset = PicOffset + (y * PicStride);
            int v;
            if (Indexed) {
                v = GetValue(loffset, x, IndexBPP);
                v = LookupPalette(v);
            } else {
                v = GetValue(loffset, x, ColorBPP);
                v = colorFormat.Decode(v);
            }
            return v;
        }
        private int LookupPalette(int c) {
            if (palCacheDirty) {
                for (int i = 0; i <= 255; i++) {
                    int v = 0;
                    if (IsFixedPalette) {
                        if (IndexBPP > 0 && IndexBPP <= 8 && i < (1 << IndexBPP)) {
                            v = 255 * i / ((1 << IndexBPP) - 1);
                            v = ColorFormat.Pack(v, v, v, 255);
                        }
                    } else {
                        v = GetValue(GetPalOffset(), i, ColorBPP);
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
        private int GetValue(int loffset, int poffset, int bpp) {
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
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
