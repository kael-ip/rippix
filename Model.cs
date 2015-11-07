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
        private byte[] data;
        public byte[] Data { get { return data; } }
        private Resource currentResource;
        public Resource CurrentResource { get { return currentResource; } }
        private List<Resource> resources;
        public List<Resource> Resources { get { return resources; } }
        public Document() {
            this.resources = new List<Resource>();
        }
    }

    public class Resource {
        public string Name { get; set; }
        public int Offset { get; set; }
        public Format Format { get; set; }
    }

    public class Format {
        public string Code { get; set; }
        private List<Parameter> parameters;
        public List<Parameter> Parameters { get { return parameters; } }
        public Format() {
            this.parameters = new List<Parameter>();
        }
    }

    public class Parameter {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Value { get; set; }
    }

    public class Helper {
        public static T Load<T>(string fileName) {
            var s = new XmlSerializer(typeof(T));
            using (var fs = File.OpenRead(fileName)) {
                return (T)s.Deserialize(fs);
            }
        }
        public static void Save<T>(T obj, string fileName) {
            var s = new XmlSerializer(typeof(T));
            using (var fs = File.Create(fileName)) {
                s.Serialize(fs, obj);
            }
        }
    }
}
