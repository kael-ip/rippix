using Rippix.Decoders;
using Rippix.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rippix {

    class ViewModel {
        private Document document;
        private bool isPictureChanged;
        private bool isPaletteChanged;

        public IPicture Picture { get { return null; } }
        public IPalette Palette { get { return null; } }

        public IList<Preset> GetAvailableDecoders() {
            var list = new List<Preset>();
            list.Add(new Preset("Direct", typeof(DirectDecoder)));
            list.Add(new Preset("Packed", typeof(PackedDecoder)));
            list.Add(new Preset("Planar (test)", typeof(TestPictureDecoder)));
            return list;
        }
        public IList<Preset> GetAvailableColorFormats() {
            var list = new List<Preset>();
            list.Add(new Preset("R8G8B8A8", new ColorFormat(24, 8, 16, 8, 8, 8, 0, 8)));
            list.Add(new Preset("B8G8R8A8", new ColorFormat(8, 8, 16, 8, 24, 8, 0, 8)));
            list.Add(new Preset("A8R8G8B8", new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8)));
            list.Add(new Preset("A8B8G8R8", new ColorFormat(0, 8, 8, 8, 16, 8, 24, 8)));
            list.Add(new Preset(null, null));
            list.Add(new Preset("A1R5G5B5", new ColorFormat(10, 5, 5, 5, 0, 5, 15, 1)));
            list.Add(new Preset("R5G6B5", new ColorFormat(11, 5, 5, 6, 0, 5, 0, 0)));
            list.Add(new Preset(null, null));
            list.Add(new Preset("R3G3B2", new ColorFormat(5, 3, 2, 3, 0, 2, 0, 0)));
            list.Add(new Preset(null, null));
            list.Add(new Preset("R8G8B8", new ColorFormat(16, 8, 8, 8, 0, 8, 24, 0)));
            list.Add(new Preset("B8G8R8", new ColorFormat(0, 8, 8, 8, 16, 8, 24, 0)));
            return list;
        }
        public void OpenDataFile(string fileName) {

        }
        public void SaveModel() {

        }
    }

    class Preset {
        private string name;
        private object value;
        public Preset(string name, object value){
            this.name = name;
            this.value = value;
        }
        public string Name { get { return name; } }
        public object Value { get { return value; } }
    }
}
