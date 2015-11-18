using Rippix.Decoders;
using Rippix.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rippix {

    class ViewModel {
        private Document document;
        private bool isDocumentChanged;
        private bool isPictureChanged;
        private bool isPaletteChanged;
        private IPictureFormat picture;
        private IPalette palette;

        public IPicture Picture { get { return picture; } }
        public IPalette Palette { get { return palette; } }
        public ColorFormat ColorFormat { get { return picture == null ? null : picture.ColorFormat; } }
        //public IPictureDecoder Decoder { get { return picture == null ? null : picture.Decoder; } }
        public IPictureAdapter PictureAdapter { get { return picture; } }
        public bool IsDocumentChanged { get { return document == null ? false : isDocumentChanged; } }
        public bool IsPictureChanged { get { return isPictureChanged; } }
        public bool IsPaletteChanged { get { return isPaletteChanged; } }

        public event EventHandler Changed;

        public ViewModel() {
            palette = new GrayscalePalette();
        }
        private void OnChanged() {
            if (Changed != null) {
                Changed(this, EventArgs.Empty);
            }
        }
        public void SetDecoder(Type decoderType) {
            if (picture != null && decoderType.IsInstanceOfType(picture.Decoder)) return;
            IPictureDecoder decoder = (IPictureDecoder)Activator.CreateInstance(decoderType);
            if (picture != null) {
                picture.Changed -= new EventHandler(Format_Changed);
            }
            var old = picture;
            picture = new PictureAdapter(decoder);
            if (old != null) {
                picture.Data = old.Data;
                picture.PicOffset = old.PicOffset;
                picture.Width = old.Width;
                picture.Height = old.Height;
                picture.ColorBPP = old.ColorBPP;
                picture.Zoom = old.Zoom;
            };
            if (decoder is INeedsPalette) {
                ((INeedsPalette)decoder).Palette = palette;
            }
            picture.Changed += new EventHandler(Format_Changed);
            Format_Changed(picture, EventArgs.Empty);
        }
        public void SetColorFormat(ColorFormat colorFormat) {
            if (picture == null) return;
            picture.ColorBPP = colorFormat.UsedBits;
            picture.ColorFormat = new ColorFormat(colorFormat);
            OnChanged();
        }
        void Format_Changed(object sender, EventArgs e) {
            ((GrayscalePalette)palette).Length = 1 << picture.ColorBPP;
            OnChanged();
        }
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
        public bool IsCurrentPreset(object value) {
            if (picture == null)return false;
            if (value is Type) {
                if (((Type)value).IsInstanceOfType(picture.Decoder))
                    return true;
            }
            if (value is ColorFormat) {
                return Equals(picture.ColorFormat, value);
            }
            return false;
        }
        public void OpenDataFile(string fileName) {
            document = Helper.LoadOrNew(fileName);
            byte[] data = document.Data;
            if (picture == null) {
                SetDecoder(typeof(DirectDecoder));
            }
            picture.PicOffset = 0;
            picture.Width = 8;
            picture.Height = 8;
            picture.Data = data;
        }
        public void SaveModel() {
            if (document == null) return;
            Helper.Save(document);
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
