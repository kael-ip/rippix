using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Rippix.Model {

    public class Application {
        [XmlIgnore]
        public Document Document { get; set; }
    }

    public class Document {
        public string FileName { get; set; }
        [XmlAttribute]
        public string SHA1 { get; set; }
        private byte[] data;
        public byte[] Data { get { return data; } }
        private Resource currentResource;
        public Resource CurrentResource {
            get {
                if (currentResource == null) {
                    if (Resources.Count > 0) {
                        currentResource = Resources[0].Clone();
                    } else {
                        currentResource = new Resource();
                        currentResource.Offset = 0;
                        currentResource.Format = new Format();
                        currentResource.Format.Code = "sample";
                    }
                }
                return currentResource;
            }
        }
        private List<Resource> resources;
        public List<Resource> Resources { get { return resources; } }
        public Document() {
            this.resources = new List<Resource>();
        }
        internal void SetData(byte[] data, string sha1) {
            this.data = data;
            this.SHA1 = sha1;
        }
    }

    public class Resource {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public int Offset { get; set; }
        public Format Format { get; set; }
        public Resource Clone() {
            Resource clone = new Resource();
            clone.Offset = this.Offset;
            clone.Format = this.Format.Clone();
            return clone;
        }
    }

    public class Format {
        [XmlAttribute]
        public string Code { get; set; }
        private List<Parameter> parameters;
        public List<Parameter> Parameters {
            get {
                if (decoder != null) {
                    decoder.WriteParameters(parameters);
                }
                return parameters;
            }
        }
        private IDecoder decoder;
        public IDecoder Decoder {
            get {
                if (decoder == null) {
                    decoder = DecoderFactory.Instance.Create(Code);
                    if (decoder != null) {
                        decoder.ReadParameters(parameters);
                    }
                }
                return decoder;
            }
        }
        public Format() {
            this.parameters = new List<Parameter>();
        }
        public Format Clone() {
            Format clone = new Format();
            clone.Code = this.Code;
            foreach (var p in Parameters) {
                clone.Parameters.Add(new Parameter(p.Name, p.Value));
            }
            return clone;
        }
    }

    public class Parameter {
        public Parameter() { }
        public Parameter(string name, string value) {
            this.Name = name;
            this.Value = value;
        }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Value { get; set; }
    }

    public interface IDecoder {
        void ReadParameters(IList<Parameter> parameters);
        void WriteParameters(IList<Parameter> parameters);
    }

    public interface IPictureDecoder : IDecoder {
        int ImageWidth { get; }
        int ImageHeight { get; }
        int LineStride { get; }
        int FrameStride { get; }
        int GetARGB(byte[] data, int offset, int x, int y);
    }

    public interface IPaletteDecoder : IDecoder {
        int Length { get; }
        int GetARGB(byte[] data, int offset, int i);
    }

    public class DecoderFactory {
        private static DecoderFactory instance;
        public static DecoderFactory Instance {
            get {
                if (instance == null) {
                    instance = new DecoderFactory();
                }
                return instance;
            }
        }
        private Dictionary<string, Type> dict;
        public void Register<T>(string code) where T : IDecoder {
            dict[code] = typeof(T);
        }
        public IDecoder Create(string code) {
            Type type;
            if (dict.TryGetValue(code, out type)) {
                return (IDecoder)Activator.CreateInstance(type);
            }
            return null;
        }
        private DecoderFactory() {
            dict = new Dictionary<string, Type>();
        }
    }

    public class Helper {
        public static T Load<T>(string fileName) {
            var s = new XmlSerializer(typeof(T));
            using (var fs = File.OpenRead(fileName)) {
                return (T)s.Deserialize(fs);
            }
        }
        public static void Save<T>(T obj, string fileName) {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", ""); 
            var s = new XmlSerializer(typeof(T), string.Empty);            
            using (var fs = File.Create(fileName)) {
                s.Serialize(fs, obj, ns);
            }
        }
        public static Document Load(string dataFileName) {
            var data = File.ReadAllBytes(dataFileName);
            string sha1 = CalcHash(data);
            Document result = null;
            try {
                //if (File.Exists(GetProjectFileName(sha1, dataFileName))) {
                    result = Load<Document>(GetProjectFileName(sha1, dataFileName));
                //}
            } catch { }
            if (result == null) {
                result = new Document();
            }
            result.FileName = dataFileName;
            result.SetData(data, sha1);
            return result;
        }
        public static void Save(Document document) {
            string fileName = GetProjectFileName(document.SHA1, document.FileName);
            Backup(fileName);
            Save(document, fileName);
        }
        private static void Backup(string fileName) {
            int i = 1;
            while (true) {
                string bakFileName = string.Concat(fileName, string.Format(".bak{0:d}", i));
                if (!File.Exists(bakFileName)) {
                    File.Move(fileName, bakFileName);
                    return;
                }
                i++;
            }
        }
        private static string CalcHash(byte[] data) {
            var alg = System.Security.Cryptography.SHA1.Create();
            var hash = alg.ComputeHash(data);
            return ConvertToBase16(hash);
        }
        public static string ConvertToBase16(byte[] bytes) {
            if (bytes == null) return string.Empty;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++) {
                sb.Append(string.Format("{0:x2}", bytes[i]));
            }
            return sb.ToString();
        }
        private static string GetProjectFileName(string sha1, string dataFileName) {
            return Path.Combine("Projects", string.Concat(sha1, ".ripx"));
        }
    }
}
