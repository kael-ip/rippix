using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

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

    public class PictureFormat : INotifyPropertyChanged {
        private byte[] data;
        private int picOffset;
        private int picStride;
        private int picWidth;
        private int picHeight;
        private int picBPP;
        private int palOffset;
        private int palBPP;
        private bool palRelative;
        private bool indexed;
        private int shiftR, shiftG, shiftB, shiftA;
        private int bitsR, bitsG, bitsB, bitsA;
        private int[] palCache;
        private bool palCacheDirty;

        public PictureFormat() {
            palCache = new int[256];
            Reset();
        }
        public void Reset() {
            this.data = null;
            this.picOffset = 0;
            this.picWidth = 16;
            this.picHeight = 16;
            this.picBPP = 32;
            this.palOffset = 0;
            this.palBPP = 32;
            this.palRelative = true;
            this.indexed = false;
            this.palCacheDirty = true;
            SetPacking(16, 8, 8, 8, 0, 8, 24, 8);
        }
        //public void Assign(PictureFormat source) {
        //    this.data = source.data;
        //    this.picOffset = source.picOffset;
        //    this.PicStride = source.picStride;
        //    this.PicHeight = source.picHeight;
        //    //this.picPack = source.picPack;
        //    this.palOffset = source.palOffset;
        //    this.palBPP = source.palBPP;
        //    //this.palPack = source.palPack;
        //    this.palRelative = source.palRelative;
        //    this.indexed = source.indexed;
        //    this.palCacheDirty = true;
        //    OnChanged(null);
        //}
        [Browsable(false)]
        public byte[] Data { get { return data; } set { data = value; palCacheDirty = true; OnChanged("Data"); } }
        public int PicOffset { get { return picOffset; } set { SetIntPropertyValue("PicOffset", ref picOffset, value, 0, int.MaxValue, PalRelative); } }
        public int PicStride { get { return picStride; } private set { picStride = Math.Max(1, value); OnChanged("PicStride"); } }
        public int PicWidth {
            get { return picWidth; }
            set {
                if (CalcStride(Math.Max(1, value), picBPP)) { SetIntPropertyValue("PicWidth", ref picWidth, value, 1, int.MaxValue, PalRelative); }
            }
        }
        public int PicHeight { get { return picHeight; } set { SetIntPropertyValue("PicHeight", ref picHeight, value, 1, int.MaxValue, PalRelative); } }
        public int PalOffset { get { return palOffset; } set { SetIntPropertyValue("PalOffset", ref palOffset, value, 0, int.MaxValue, true); } }
        public bool Indexed { get { return indexed; } set { indexed = value; palCacheDirty = true; OnChanged("Indexed"); } }
        public bool PalRelative { get { return palRelative; } set { palRelative = value; palCacheDirty = true; OnChanged("PalRelative"); } }
        public int PicBPP {
            get { return picBPP; }
            set {
                if (CalcStride(PicWidth, value)) { SetIntPropertyValue("PicBPP", ref  picBPP, value, 1, 32, PalRelative); }
            }
        }
        public int PalBPP { get { return palBPP; } set { SetIntPropertyValue("PalBPP", ref palBPP, value, 1, 32, true); } }
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
            if (makeDirty) palCacheDirty = true;
            OnChanged(name);
        }

        public void SetPacking(int shiftR, int bitsR, int shiftG, int bitsG, int shiftB, int bitsB, int shiftA, int bitsA) {
            this.ShiftR = shiftR;
            this.BitsR = bitsR;
            this.ShiftG = shiftG;
            this.BitsG = bitsG;
            this.ShiftB = shiftB;
            this.BitsB = bitsB;
            this.ShiftA = shiftA;
            this.BitsA = bitsA;
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
            return GetColor(loffset, x, picBPP, indexed);
        }
        private int Pack(int c0, int c1, int c2, int c3) {
            return c0 | (c1 << 8) | (c2 << 16) | (c3 << 24);
        }
        private int GetColor(int loffset, int poffset, int bpp, bool indexed) {
            int v = GetValue(loffset, poffset, bpp);
            if (indexed) {
                EnsurePalette();
                if (v < 0 || v > 255) {
                    v = 0;
                } else {
                    v = palCache[v];
                }
            } else {
                int cr = ((v >> shiftR) & ((1 << BitsR) - 1)) * 255 / ((1 << BitsR) - 1);
                int cg = ((v >> shiftG) & ((1 << BitsG) - 1)) * 255 / ((1 << BitsG) - 1);
                int cb = ((v >> shiftB) & ((1 << BitsB) - 1)) * 255 / ((1 << BitsB) - 1);
                if (BitsA == 0) {
                    v = Pack(cr, cg, cb, 255);
                } else {
                    int ca = ((v >> shiftA) & ((1 << BitsA) - 1)) * 255 / ((1 << BitsA) - 1);
                    v = Pack(cr, cg, cb, ca);
                }
            }
            return v;
        }
        private void EnsurePalette() {
            if (palCacheDirty) {
                for (int i = 0; i <= 255; i++) {
                    palCache[i] = GetColor(GetPalOffset(), i, palBPP, false);
                }
                palCacheDirty = false;
            }
        }
        private int GetValue(int loffset, int poffset, int bpp) {
            int offset = loffset + ((poffset * bpp) >> 3);
            if (offset < 0 || offset + (bpp>>3) >= data.Length) return 0;
            int v = 0;
            switch (bpp) {
                case 32:
                    v = Pack(data[offset], data[offset + 1], data[offset + 2], data[offset + 3] << 24);
                    break;
                case 24:
                    v = Pack(data[offset], data[offset + 1], data[offset + 2], 0);
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
            }else{
                return PalOffset;
            }
        }
        private void OnChanged(string propertyName) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class PixelView : Control{
        private Bitmap bitmap;
        private PictureFormat format = new PictureFormat();
        public Bitmap Bitmap { get { return bitmap; } }
        public PictureFormat Format { get { return format; } }
        private int zoom;
        public int Zoom { get { return zoom; } set { zoom = Math.Max(0, value); Invalidate(); } }
        private bool isDirty;

        public PixelView() {
            this.SetStyle(ControlStyles.Selectable, true);
            format.PropertyChanged += new PropertyChangedEventHandler(format_PropertyChanged);
        }
        void format_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            //UpdateBitmap();
            isDirty = true;
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e) {
            //base.OnPaint(e);
            EnsureBitmap();
            if (isDirty) UpdateBitmap();
            e.Graphics.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(Point.Empty, this.Size));
            //e.Graphics.DrawImageUnscaledAndClipped(bitmap, new Rectangle(Point.Empty, this.Size));
            //e.Graphics.DrawImageUnscaled(bitmap, 0, 0);
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(bitmap, 0, 0, bitmap.Width * (Zoom + 1), bitmap.Height * (Zoom + 1));
            SolidBrush brush = new SolidBrush(this.BackColor);
            //if (bitmap.Width < this.Width) {
            //    e.Graphics.FillRectangle(brush, bitmap.Width, 0, this.Width - bitmap.Width, bitmap.Height);
            //}
            //if (bitmap.Height < this.Height) {
            //    e.Graphics.FillRectangle(brush, 0, bitmap.Height, this.Width, this.Height - bitmap.Height);
            //}
        }
        protected override void OnPaintBackground(PaintEventArgs pevent) {
            //base.OnPaintBackground(pevent);
        }
        //protected override void OnLayout(LayoutEventArgs levent) {
        //    base.OnLayout(levent);
        //    EnsureBitmap();
        //}
        private void EnsureBitmap() {
            Size sz = Format.GetBitmapSize();
            if (bitmap == null || bitmap.Size != sz) {
                bitmap = new Bitmap(sz.Width, sz.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
        }
        int[] lineBuffer;
        long[] timings = new long[100];
        private void UpdateBitmap() {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            //timings[0] = sw.ElapsedTicks;
            EnsureBitmap();
            //timings[1] = sw.ElapsedTicks;
            if (Format.Data == null) return;
            System.Drawing.Imaging.BitmapData bits = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
            //timings[2] = sw.ElapsedTicks;
            EnsureLineBuffer(bits.Width);
            //timings[3] = sw.ElapsedTicks;
            //for (int y = 0; y < bits.Height; y++) {
                int scanPosition = bits.Scan0.ToInt32();
                int scanline = 0;
                while (scanline < bits.Height) {
                    //System.Diagnostics.Stopwatch swy = new System.Diagnostics.Stopwatch();
                    //swy.Start();
                    //timings[6] = swy.ElapsedTicks;
                    RenderScanLine(lineBuffer, scanline);
                    //timings[7] = swy.ElapsedTicks;
                    System.Runtime.InteropServices.Marshal.Copy(lineBuffer, 0, (IntPtr)scanPosition, bits.Width);
                    //timings[8] = swy.ElapsedTicks;
                    scanPosition += bits.Stride;
                    scanline++;
                }
            //}
            //timings[4] = sw.ElapsedTicks;
            bitmap.UnlockBits(bits);
            //timings[5] = sw.ElapsedTicks;
            //Invalidate();
            isDirty = false;
            //ReportTimings();
        }
        private void ReportTimings() {
            System.Diagnostics.Debug.WriteLine(string.Format("FullUpdateBitmap={0}, EnsureBitmap={1}, LockBits={2}, EnsureLineBuffer={3}, Render={4}, UnlockBits={5}",
                timings[5] - timings[0], timings[1] - timings[0], timings[2] - timings[1], timings[3] - timings[2], timings[4] - timings[3], timings[5] - timings[4]));
            System.Diagnostics.Debug.WriteLine(string.Format("-- FullLine={0}, RenderScanLine={1}, Copy={2}",
                timings[8] - timings[6], timings[7] - timings[6], timings[8] - timings[7]));
        }
        private void EnsureLineBuffer(int length) {
            if (lineBuffer == null || lineBuffer.Length != length) {
                lineBuffer = new int[length];
            }
        }
        private void RenderScanLine(int[] lineBuffer, int scanline) {
            for (int i = 0; i < lineBuffer.Length; i++) {
                lineBuffer[i] = Format.GetRGBAColor(scanline, i);
            }
        }
        protected override bool IsInputKey(Keys keyData) {
            keyData = keyData & ~Keys.Shift;
            keyData = keyData & ~Keys.Control;
            if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right
                || keyData == Keys.PageDown||keyData==Keys.PageUp|| keyData==Keys.Add|| keyData == Keys.Subtract) return true;
            return base.IsInputKey(keyData);
        }
        protected override void OnKeyDown(KeyEventArgs e) {
            int step = e.Control ? 8 : 1;
            base.OnKeyDown(e);
            if (e.Shift) {
                switch (e.KeyCode) {
                    case Keys.Up:
                        format.PicHeight -= step;
                        break;
                    case Keys.Down:
                        format.PicHeight += step;
                        break;
                    case Keys.Left:
                        format.PicWidth -= step;
                        break;
                    case Keys.Right:
                        format.PicWidth += step;
                        break;
                    case Keys.PageUp:
                        break;
                    case Keys.PageDown:
                        break;
                    case Keys.Add:
                    case Keys.Subtract:
                    case Keys.OemOpenBrackets:
                    case Keys.OemCloseBrackets:
                        break;
                    case Keys.Z:
                        this.Zoom--;
                        break;
                }
            } else {
                switch (e.KeyCode) {
                    case Keys.Up:
                        format.PicOffset -= format.PicStride*step;
                        break;
                    case Keys.Down:
                        format.PicOffset += format.PicStride*step;
                        break;
                    case Keys.Left:
                        format.PicOffset -= step;
                        break;
                    case Keys.Right:
                        format.PicOffset += step;
                        break;
                    case Keys.PageUp:
                        format.PicOffset -= format.PicStride * format.PicHeight;
                        break;
                    case Keys.PageDown:
                        format.PicOffset += format.PicStride * format.PicHeight;
                        break;
                    case Keys.Add:
                    case Keys.Subtract:
                    case Keys.OemOpenBrackets:
                    case Keys.OemCloseBrackets:
                        break;
                    case Keys.Z:
                        this.Zoom++;
                        break;
                }
            }
        }
    }
}
