using Rippix.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rippix {

    public interface IPaletteAdapter {
        byte[] Data { get; set; }
        int Offset { get; set; }
        int Length { get; }
        int GetARGBColor(int i);
    }

    public class PaletteAdapter : IPaletteAdapter {
        private IPaletteDecoder decoder;
        private byte[] data;
        private int offset;
        private int[] palCache;
        private bool isCacheDirty;
        private bool IsFixedPalette = true;
        public PaletteAdapter(IPaletteDecoder decoder) {
            this.decoder = decoder;
            this.isCacheDirty = true;
        }
        private int bpp = 8;
        public int BPP {
            get { return bpp; }
            set { bpp = (value < 0) ? 0 : (value > 8) ? 8 : value; }
        }
        public byte[] Data {
            get { return data; }
            set { }
        }
        public int Offset {
            get { return offset; }
            set { }
        }
        public int Length {
            get { return (1 << bpp); }
        }
        private int GetGrayscale(int i) {
            int v = 0;
            if (BPP > 0 && BPP <= 8 && i < (1 << BPP)) {
                v = 255 * i / ((1 << BPP) - 1);
                v = ColorFormat.Pack(v, v, v, 255);
            }
            return v;
        }
        public int GetARGBColor(int c) {
            if (c < 0 || c > Length) return 0;
            if (isCacheDirty) {
                for (int i = 0; i <= 255; i++) {
                    int v = 0;
                    if (IsFixedPalette) {
                        v = GetGrayscale(i);
                    } else {
                        v = decoder.GetARGB(Data, Offset, i);
                    }
                    palCache[i] = v;
                }
                isCacheDirty = false;
            }
            return palCache[c];
        }
    }
}
