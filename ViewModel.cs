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
        private IPicture palettePicture;

        public IPicture Picture { get { return picture; } }
        public IPalette Palette { get { return palette; } }
        public ColorFormat ColorFormat { get { return picture == null ? null : picture.ColorFormat; } }
        //public IPictureDecoder Decoder { get { return picture == null ? null : picture.Decoder; } }
        public IPictureAdapter PictureAdapter { get { return picture; } }
        public bool IsDocumentChanged { get { return document == null ? false : isDocumentChanged; } }
        public bool IsPictureChanged { get { return isPictureChanged; } }
        public bool IsPaletteChanged { get { return isPaletteChanged; } }
        public IPicture PalettePicture { get { return palettePicture; } }
        public IList<Resource> Resources { get { return document == null ? null : document.Resources; } }
        public event EventHandler Changed;

        static ViewModel() {
            DecoderFactory.Instance.Register<DirectDecoder>("Direct");
            DecoderFactory.Instance.Register<PackedDecoder>("Packed");
            DecoderFactory.Instance.Register<TestPictureDecoder>("$Planar$0.1");
        }
        public ViewModel() {
            palette = new GrayscalePalette();
            palettePicture = new PalettePictureAdapter(Palette) { Length = 256 };
        }
        private void OnChanged() {
            if (Changed != null) {
                Changed(this, EventArgs.Empty);
            }
        }
        private void RefreshAdapter() {
            IPictureDecoder decoder = document.CurrentResource.Format.Decoder as IPictureDecoder;
            if (decoder == null) {
                document.CurrentResource.Format.Code = "Direct";
                decoder = document.CurrentResource.Format.Decoder as IPictureDecoder;
            }
            if (picture != null) {
                picture.Changed -= new EventHandler(Format_Changed);
            }
            var old = picture;
            picture = new PictureAdapter(decoder);
            picture.Data = document.Data;
            picture.PicOffset = document.CurrentResource.Offset;
            if (old != null) {
                picture.Zoom = old.Zoom;
            }
            if (decoder is INeedsPalette) {
                ((INeedsPalette)decoder).Palette = palette;
            }
            picture.Changed += new EventHandler(Format_Changed);
            Format_Changed(picture, EventArgs.Empty);
        }
        public void SetDecoder(string decoderCode) {
            if (Equals(decoderCode, document.CurrentResource.Format.Code)) return;
            document.CurrentResource.Format.Code = decoderCode;
            RefreshAdapter();
        }
        public void SetColorFormat(ColorFormat colorFormat) {
            if (picture == null) return;
            picture.ColorBPP = colorFormat.UsedBits;
            picture.ColorFormat = new ColorFormat(colorFormat);
            OnChanged();
        }
        void Format_Changed(object sender, EventArgs e) {
            document.CurrentResource.Offset = picture.PicOffset;
            int plength = 1;
            if (picture.Decoder is INeedsPalette) {
                plength = 1 << picture.ColorBPP;
            }
            ((GrayscalePalette)palette).Length = plength;
            ((PalettePictureAdapter)PalettePicture).Length = ((GrayscalePalette)palette).Length;
            OnChanged();
        }
        public IList<Preset> GetAvailableDecoders() {
            var list = new List<Preset>();
            list.Add(new Preset("Direct", "Direct"));
            list.Add(new Preset("Packed", "Packed"));
            list.Add(new Preset("Planar (test)", "$Planar$0.1"));
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
            if (picture == null) return false;
            if (value is string) {
                return Equals(document.CurrentResource.Format.Code, value);
            }
            if (value is ColorFormat) {
                return Equals(picture.ColorFormat, value);
            }
            return false;
        }
        public void OpenDataFile(string fileName) {
            document = Helper.LoadOrNew(fileName);
            byte[] data = document.Data;
            RefreshAdapter();
            if (document.Resources.Count == 0) {
                picture.PicOffset = 0;
                picture.Width = 8;
                picture.Height = 8;
            }
        }
        public void SaveModel() {
            if (document == null) return;
            Helper.Save(document);
        }
        public void BookmarkStore() {
            if (document == null) return;
            document.StoreResource();
            //SaveModel();
            OnChanged();
        }
        public void BookmarkLoad(int index) {
            if (document == null) return;
            document.LoadResource(index);
            RefreshAdapter();
            OnChanged();
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
