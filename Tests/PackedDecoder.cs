using NUnit.Framework;
using Rippix.Decoders;
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
    }
}
#endif
