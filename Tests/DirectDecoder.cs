using NUnit.Framework;
using Rippix.Decoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if DEBUG
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
    }
}
#endif
