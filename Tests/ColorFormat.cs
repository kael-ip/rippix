using NUnit.Framework;
using System;

namespace Rippix.Tests {
    public class ColorFormatTests {
        [Test]
        public void Test8888() {
            unchecked {
                var cf = new ColorFormat(0, 8, 8, 8, 16, 8, 24, 8);
                var result = cf.Decode((int)0x87654321);
                Assert.AreEqual((int)0x87654321, result);
                Assert.AreEqual((int)0x87, ((result >> 24) & 0xff));
            }
        }
        [Test]
        public void Test444() {
            var cf = new ColorFormat(0, 4, 4, 4, 8, 4, 0, 0);
            var result = cf.Decode(0x0352);
            Assert.AreEqual((uint)0xff335522, (uint)result);
        }
    }
}
