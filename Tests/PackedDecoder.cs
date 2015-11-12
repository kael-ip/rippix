using NUnit.Framework;
using Rippix.Decoders;
using Rippix.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if DEBUG
namespace Rippix.Tests {
    public class PackedDecoderTests {
        [Test]
        public void TestBPP() {
            var decoder = new PackedDecoder();
            decoder.ColorBPP = 8;
            Assert.AreEqual(8, decoder.ColorBPP);
            decoder.ColorBPP = 4;
            Assert.AreEqual(4, decoder.ColorBPP);
            decoder.ColorBPP = 2;
            Assert.AreEqual(2, decoder.ColorBPP);
            decoder.ColorBPP = 1;
            Assert.AreEqual(1, decoder.ColorBPP);
            decoder.ColorBPP = 9;
            Assert.AreEqual(8, decoder.ColorBPP);
            decoder.ColorBPP = 0;
            Assert.AreEqual(8, decoder.ColorBPP);
            decoder.ColorBPP = -1;
            Assert.AreEqual(8, decoder.ColorBPP);
            decoder.ColorBPP = 15;
            Assert.AreEqual(8, decoder.ColorBPP);
            decoder.ColorBPP = 4;
            Assert.AreEqual(4, decoder.ColorBPP);
            decoder.ColorBPP = 6;
            Assert.AreEqual(8, decoder.ColorBPP);
            decoder.ColorBPP = 456;
            Assert.AreEqual(8, decoder.ColorBPP);
        }
        [Test]
        public void TestParameterSerialization() {
            var decoder = new PackedDecoder();
            var p = new List<Parameter>();
            decoder.WriteParameters(p);
            Assert.AreNotEqual(0, p.Count);
            var c1 = p.Count;
            decoder.WriteParameters(p);
            Assert.AreEqual(c1, p.Count);
            decoder.ColorBPP = 4;
            var cf = new ColorFormat(12, 5, 6, 4, 1, 3, 18, 1);
            decoder.ColorFormat = cf;
            ((IPictureController)decoder).Width = 13;
            ((IPictureController)decoder).Height = 21;
            decoder.WriteParameters(p);
            Assert.AreEqual(1.ToString(), p.First(z => z.Name == "ppbyp").Value);
            Assert.AreEqual(cf.ToString(), p.First(z => z.Name == "ColorFormat").Value);
            Assert.AreEqual("13", p.First(z => z.Name == "Width").Value);
            Assert.AreEqual("21", p.First(z => z.Name == "Height").Value);
            var d2 = new PackedDecoder();
            d2.ReadParameters(p);
            Assert.AreEqual(4, d2.ColorBPP);
            Assert.AreEqual(cf, d2.ColorFormat);
            Assert.AreEqual(13, ((IPictureController)decoder).Width);
            Assert.AreEqual(21, ((IPictureController)decoder).Height);
        }

    }
}
#endif
