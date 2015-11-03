using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Rippix {

    public class PixelView : Control{
        private Bitmap bitmap;
        private IPictureDecoder format;
        public Bitmap Bitmap { get { return bitmap; } }
        public IPictureDecoder Format {
            get { return format; }
            set {
                if (Equals( format,value)) return;
                if (format != null) {
                    format.Changed -= format_Changed;
                }
                format = value;
                if (format != null) {
                    format.Changed += format_Changed;
                }
            }
        }
        private int zoom;
        public int Zoom { get { return zoom; } set { zoom = Math.Max(0, value); Invalidate(); } }
        private bool isDirty;
        private Color boxBackColor;
        public Color BoxBackColor {
            get { return boxBackColor; }
            set {
                if (boxBackColor == value) return;
                boxBackColor = value;
                Invalidate();
            }
        }
        public PixelView() {
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            BoxBackColor = Color.Violet;
        }
        void format_Changed(object sender, EventArgs e) {
            isDirty = true;
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e) {
            //base.OnPaint(e);
            SolidBrush brush = new SolidBrush(this.BackColor);
            Rectangle rect = Rectangle.Empty;
            if (Format != null) {
                EnsureBitmap();
                if (isDirty) UpdateBitmap();
                SolidBrush tbrush = new SolidBrush(this.BoxBackColor);
                rect = new Rectangle(0, 0, bitmap.Width * (Zoom + 1), bitmap.Height * (Zoom + 1));
                e.Graphics.FillRectangle(tbrush, rect);
                //e.Graphics.DrawImageUnscaledAndClipped(bitmap, new Rectangle(Point.Empty, this.Size));
                //e.Graphics.DrawImageUnscaled(bitmap, 0, 0);
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                e.Graphics.DrawImage(bitmap, rect);
            }
            if (rect.Width < this.Width) {
                e.Graphics.FillRectangle(brush, rect.Width, 0, this.Width - rect.Width, rect.Height);
            }
            if (rect.Height < this.Height) {
                e.Graphics.FillRectangle(brush, 0, rect.Height, this.Width, this.Height - rect.Height);
            }
        }
        protected override void OnPaintBackground(PaintEventArgs pevent) {
            //base.OnPaintBackground(pevent);
        }
        private void EnsureBitmap() {
            Size sz = new Size(Format.PicWidth, Format.PicHeight);
            if (bitmap == null || bitmap.Size != sz) {
                bitmap = new Bitmap(sz.Width, sz.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
        }
        int[] lineBuffer;
        long[] timings = new long[100];
        private void UpdateBitmap() {
            if (Format == null || Format.Data == null) return;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            timings[0] = sw.ElapsedTicks;
            EnsureBitmap();
            timings[1] = sw.ElapsedTicks;
            System.Drawing.Imaging.BitmapData bits = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
            timings[2] = sw.ElapsedTicks;
            EnsureLineBuffer(bits.Width);
            timings[3] = sw.ElapsedTicks;
            int scanPosition = bits.Scan0.ToInt32();
            int scanline = 0;
            while (scanline < bits.Height) {
                RenderScanLine(lineBuffer, scanline);
                System.Runtime.InteropServices.Marshal.Copy(lineBuffer, 0, (IntPtr)scanPosition, bits.Width);
                scanPosition += bits.Stride;
                scanline++;
            }
            timings[4] = sw.ElapsedTicks;
            bitmap.UnlockBits(bits);
            timings[5] = sw.ElapsedTicks;
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
            base.OnKeyDown(e);
            if (Format == null) return;
            int step = e.Control ? 8 : 1;
            CorrectOffset(0);
            int oldOffset = format.PicOffset;
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
            CorrectOffset(oldOffset);
        }
        private void CorrectOffset(int oldOffset) {
            if (format.PicOffset < 0 || format.PicOffset >= format.Data.Length) {
                format.PicOffset = oldOffset;
            }
        }
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            this.Focus();
        }
    }
}
