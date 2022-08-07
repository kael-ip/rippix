#if DEBUG
using NUnit.Framework;
using Rippix.Decoders;
using Rippix.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rippix.Tests {
    public class DirectDecoderTests {
        [Test]
        public void TestBPP() {
            var decoder = new DirectDecoder();
            decoder.ColorBPP = 32;
            Assert.AreEqual(32, decoder.ColorBPP);
            decoder.ColorBPP = 24;
            Assert.AreEqual(24, decoder.ColorBPP);
            decoder.ColorBPP = 16;
            Assert.AreEqual(16, decoder.ColorBPP);
            decoder.ColorBPP = 8;
            Assert.AreEqual(8, decoder.ColorBPP);
            decoder.ColorBPP = 1;
            Assert.AreEqual(8, decoder.ColorBPP);
            decoder.ColorBPP = 0;
            Assert.AreEqual(8, decoder.ColorBPP);
            decoder.ColorBPP = -1;
            Assert.AreEqual(8, decoder.ColorBPP);
            decoder.ColorBPP = 15;
            Assert.AreEqual(16, decoder.ColorBPP);
            decoder.ColorBPP = 33;
            Assert.AreEqual(32, decoder.ColorBPP);
            decoder.ColorBPP = 456;
            Assert.AreEqual(32, decoder.ColorBPP);
        }
        [Test]
        public void TestParameterSerialization() {
            var decoder = new DirectDecoder();
            var p = new List<Parameter>();
            decoder.WriteParameters(p);
            Assert.AreNotEqual(0, p.Count);
            var c1 = p.Count;
            decoder.WriteParameters(p);
            Assert.AreEqual(c1, p.Count);
            decoder.ColorBPP = 24;
            var cf = new ColorFormat(12, 5, 6, 4, 1, 3, 18, 1);
            decoder.ColorFormat = cf;
            ((IPictureDecoderController)decoder).Width = 13;
            ((IPictureDecoderController)decoder).Height = 21;
            decoder.WriteParameters(p);
            Assert.AreEqual(3.ToString(), p.First(z => z.Name == "bypp").Value);
            Assert.AreEqual(cf.ToString(), p.First(z => z.Name == "ColorFormat").Value);
            Assert.AreEqual("13", p.First(z => z.Name == "Width").Value);
            Assert.AreEqual("21", p.First(z => z.Name == "Height").Value);
            var d2 = new DirectDecoder();
            d2.ReadParameters(p);
            Assert.AreEqual(24, d2.ColorBPP);
            Assert.AreEqual(cf, d2.ColorFormat);
            Assert.AreEqual(13, ((IPictureDecoderController)decoder).Width);
            Assert.AreEqual(21, ((IPictureDecoderController)decoder).Height);
        }
    }
}
#endif
