#if DEBUG
using NUnit.Framework;
using System;
using System.Drawing;

namespace Rippix.Tests {
    public class ColorFormatTests {
        [Test]
        public void TestStandardConformity() {
            var color = Color.FromArgb(0x12, 0x34, 0x56, 0x78);//ARGB
            var cf = new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
            Assert.AreEqual(color.ToArgb(), cf.Decode(color.ToArgb()));
            Assert.AreEqual(0x34, (color.ToArgb() >> 16) & 0xff);
            Assert.AreEqual(0x12, (color.ToArgb() >> 24) & 0xff);
        }
        [Test]
        public void TestDecode() {
            unchecked {
                var cf = new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
                var result = cf.Decode((int)0x87654321);
                Assert.AreEqual((int)0x87654321, result);
                Assert.AreEqual((int)0x87, ((result >> 24) & 0xff));
            }
            {
                var cf = new ColorFormat(8, 4, 4, 4, 0, 4, 0, 0);
                var result = cf.Decode(0x0352);
                Assert.AreEqual((uint)0xff335522, (uint)result);
            }
        }
        [Test]
        public void TestSerialize() {
            {
                var cf = new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
                Assert.AreEqual("S24:8,16:8,8:8,0:8", cf.ToString());
            }
            {
                var cf = new ColorFormat(0, 4, 4, 4, 8, 4, 0, 0);
                Assert.AreEqual("S0:0,0:4,4:4,8:4", cf.ToString());
            }
            {
                var cf = new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
                Assert.AreEqual("S24:8,16:8,8:8,0:8", cf.ToString());
            }
            {
                var cf = new ColorFormat(11, 5, 5, 6, 0, 5, 0, 0);
                Assert.AreEqual("S0:0,11:5,5:6,0:5", cf.ToString());
            }
            {
                var cf = new ColorFormat(10, 5, 5, 5, 0, 5, 15, 1);
                Assert.AreEqual("S15:1,10:5,5:5,0:5", cf.ToString());
            }
        }
        [Test]
        public void TestDeserialize() {
            {
                var cf = new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
                Assert.AreEqual(cf, new ColorFormat("S24:8,16:8,8:8,0:8"));
            }
            {
                var cf = new ColorFormat(0, 4, 4, 4, 8, 4, 0, 0);
                Assert.AreEqual(cf, new ColorFormat("S0:0,0:4,4:4,8:4"));
            }
            {
                var cf = new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
                Assert.AreEqual(cf, new ColorFormat("S24:8,16:8,8:8,0:8"));
            }
            {
                var cf = new ColorFormat(11, 5, 5, 6, 0, 5, 0, 0);
                Assert.AreEqual(cf, new ColorFormat("S0:0,11:5,5:6,0:5"));
            }
            {
                var cf = new ColorFormat(10, 5, 5, 5, 0, 5, 15, 1);
                Assert.AreEqual(cf, new ColorFormat("S15:1,10:5,5:5,0:5"));
            }
        }
        [Test]
        public void TestCalculatedBPP() {
            Assert.AreEqual(32, new ColorFormat(24, 8, 16, 8, 8, 8, 0, 8).UsedBits);
            Assert.AreEqual(32, new ColorFormat(8, 8, 16, 8, 24, 8, 0, 8).UsedBits);
            Assert.AreEqual(32, new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8).UsedBits);
            Assert.AreEqual(32, new ColorFormat(0, 8, 8, 8, 16, 8, 24, 8).UsedBits);
            Assert.AreEqual(16, new ColorFormat(10, 5, 5, 5, 0, 5, 15, 1).UsedBits);
            Assert.AreEqual(16, new ColorFormat(11, 5, 5, 6, 0, 5, 0, 0).UsedBits);
            Assert.AreEqual(8, new ColorFormat(5, 3, 2, 3, 0, 2, 0, 0).UsedBits);
            Assert.AreEqual(24, new ColorFormat(16, 8, 8, 8, 0, 8, 24, 0).UsedBits);
            Assert.AreEqual(24, new ColorFormat(0, 8, 8, 8, 16, 8, 24, 0).UsedBits);

        }
    }
}
#endif
