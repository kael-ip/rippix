using System;
using System.ComponentModel;

namespace Rippix {

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

        public int ShiftR { get { return shiftR; } set { SetIntPropertyValue("ShiftR", ref shiftR, value, 0, 31, true); } }
        public int ShiftG { get { return shiftG; } set { SetIntPropertyValue("ShiftG", ref shiftG, value, 0, 31, true); } }
        public int ShiftB { get { return shiftB; } set { SetIntPropertyValue("ShiftB", ref shiftB, value, 0, 31, true); } }
        public int ShiftA { get { return shiftA; } set { SetIntPropertyValue("ShiftA", ref shiftA, value, 0, 31, true); } }
        public int BitsR { get { return bitsR; } set { SetIntPropertyValue("BitsR", ref bitsR, value, 0, 16, true); } }
        public int BitsG { get { return bitsG; } set { SetIntPropertyValue("BitsG", ref bitsG, value, 0, 16, true); } }
        public int BitsB { get { return bitsB; } set { SetIntPropertyValue("BitsB", ref bitsB, value, 0, 16, true); } }
        public int BitsA { get { return bitsA; } set { SetIntPropertyValue("BitsA", ref bitsA, value, 0, 16, true); } }

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
        public override string ToString() {
            return string.Concat("S", string.Join(",", ComponentToString(ShiftR, BitsR), ComponentToString(ShiftG, BitsG), ComponentToString(ShiftB, BitsB), ComponentToString(ShiftA, BitsA)));
        }
        private static string ComponentToString(int shift, int bits) {
            return string.Format("{0}:{1}", bits, shift);
        }
        public ColorFormat(string code) {
            if (code == null) throw new ArgumentNullException();
            if (!code.StartsWith("S")) throw new ArgumentException();
            var components = code.Substring(1).Split(',');
            if (components == null || components.Length != 4) throw new ArgumentException();
            Decode(components[0], out shiftR, out bitsR);
            Decode(components[1], out shiftG, out bitsG);
            Decode(components[2], out shiftB, out bitsB);
            Decode(components[3], out shiftA, out bitsA);
        }
        private void Decode(string code, out int shift, out int bits) {
            if (string.IsNullOrEmpty(code)) throw new ArgumentException();
            var pair = code.Split(':');
            if (pair == null || pair.Length != 2) throw new ArgumentException();
            bits = Int32.Parse(pair[0]);
            shift = Int32.Parse(pair[1]);
            if (shift < 0 || shift >= 32 || bits < 0 || bits > 16) throw new ArgumentOutOfRangeException();
        }
    }

}
