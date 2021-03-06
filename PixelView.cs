﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.Serialization;
using Rippix.Model;

namespace Rippix {

    public class PixelView : Control{
        private IPicture format;
        public IPicture Format {
            get { return format; }
            set {
                if (Equals(format, value)) return;
                format = value;
                Refresh();
            }
        }
        private int zoom;
        public int Zoom { get { return zoom; } set { zoom = Math.Max(0, value); Invalidate(); } }
        private Bitmap bitmap;
        public Bitmap Bitmap { get { return bitmap; } }
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
        public override void Refresh() {
            isDirty = true;
            base.Refresh();
        }
        protected override void OnPaint(PaintEventArgs e) {
            //base.OnPaint(e);
            SolidBrush brush = new SolidBrush(this.BackColor);
            Rectangle rect = Rectangle.Empty;
            if (Format != null && Format.ImageWidth > 0 && Format.ImageHeight > 0) {
                EnsureBitmap();
                if (isDirty) UpdateBitmap();
                SolidBrush tbrush = new SolidBrush(this.BoxBackColor);
                rect = new Rectangle(0, 0, bitmap.Width * (Zoom + 1), bitmap.Height * (Zoom + 1));
                e.Graphics.FillRectangle(tbrush, rect);
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
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
            Size sz = new Size(Format.ImageWidth, Format.ImageHeight);
            if (bitmap == null || bitmap.Size != sz) {
                bitmap = new Bitmap(sz.Width, sz.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
        }
        int[] lineBuffer;
        long[] timings = new long[100];
        private void UpdateBitmap() {
            if (Format == null) return;
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
                lineBuffer[i] = Format.GetARGB(i, scanline);
            }
        }
        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);
            this.Focus();
        }
    }
}
