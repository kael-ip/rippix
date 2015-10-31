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
        private PictureFormat format = new PictureFormat();
        public Bitmap Bitmap { get { return bitmap; } }
        public PictureFormat Format { get { return format; } }
        private int zoom;
        public int Zoom { get { return zoom; } set { zoom = Math.Max(0, value); Invalidate(); } }
        private bool isDirty;

        public PixelView() {
            this.SetStyle(ControlStyles.Selectable, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
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
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            this.Focus();
        }
    }
}
