using Rippix.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rippix {

    public interface IPalette {
        int GetARGB(int index);
    }

    public interface INeedsPalette {
        IPalette Palette { get; set; }
    }

    public class GrayscalePalette : IPalette {
        private int length;
        public int Length {
            get { return length; }
            set { length = value; }
        }
        public int GetARGB(int index) {
            if (index < 0 || index >= Length) return 0;
            var v = 255 * index / (Length - 1);
            v = ColorFormat.Pack(v, v, v, 255);
            return v;
        }
    }

    public class CachedPalette : IPalette {
        private int[] palCache;
        private bool isCacheDirty;
        private IPalette nested;
        private int length;
        public CachedPalette(IPalette nested, int length) {
            this.nested = nested;
            this.length = length;
        }
        public int Length { get { return length; } }
        public int GetARGB(int index) {
            if (index < 0 || index > Length) return 0;
            if (isCacheDirty) {
                if (palCache == null || palCache.Length != Length) {
                    palCache = new int[Length];
                }
                for (int i = 0; i < Length; i++) {
                    palCache[i] = nested.GetARGB(i);
                }
                isCacheDirty = false;
            }
            return palCache[index];
        }
        public void Invalidate() {
            isCacheDirty = true;
        }
    }

    public class CodedPalette : IPalette {
        private IPaletteDecoder decoder;
        private byte[] data;
        private int offset;
        public CodedPalette(IPaletteDecoder decoder) {
            this.decoder = decoder;
        }
        public byte[] Data {
            get { return data; }
            set { data = value; }
        }
        public int Offset {
            get { return offset; }
            set { offset = value; }
        }
        public int GetARGB(int index) {
            if (index < 0 || index >= decoder.Length) return 0;
            return decoder.GetARGB(Data, Offset, index);
        }
    }

}
