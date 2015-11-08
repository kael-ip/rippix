using NUnit.Framework;
using System;

namespace Rippix.Tests {
    public class ColorFormatTests {
        [Test]
        public void TestDecode8888() {
            unchecked {
                var cf = new ColorFormat(0, 8, 8, 8, 16, 8, 24, 8);
                var result = cf.Decode((int)0x87654321);
                Assert.AreEqual((int)0x87654321, result);
                Assert.AreEqual((int)0x87, ((result >> 24) & 0xff));
            }
            {
                var cf = new ColorFormat(0, 4, 4, 4, 8, 4, 0, 0);
                var result = cf.Decode(0x0352);
                Assert.AreEqual((uint)0xff335522, (uint)result);
            }
        }
        [Test]
        public void TestSerialize() {
            {
                var cf = new ColorFormat(0, 8, 8, 8, 16, 8, 24, 8);
                Assert.AreEqual("S8:0,8:8,8:16,8:24", cf.ToString());
            }
            {
                var cf = new ColorFormat(0, 4, 4, 4, 8, 4, 0, 0);
                Assert.AreEqual("S4:0,4:4,4:8,0:0", cf.ToString());
            }
            {
                var cf = new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
                Assert.AreEqual("S8:16,8:8,8:0,8:24", cf.ToString());
            }
            {
                var cf = new ColorFormat(0, 5, 5, 6, 11, 5, 0, 0);
                Assert.AreEqual("S5:0,6:5,5:11,0:0", cf.ToString());
            }
            {
                var cf = new ColorFormat(0, 5, 5, 5, 10, 5, 15, 1);
                Assert.AreEqual("S5:0,5:5,5:10,1:15", cf.ToString());
            }
        }
        [Test]
        public void TestDeserialize() {
            {
                var cf = new ColorFormat(0, 8, 8, 8, 16, 8, 24, 8);
                Assert.AreEqual(cf, new ColorFormat("S8:0,8:8,8:16,8:24"));
            }
            {
                var cf = new ColorFormat(0, 4, 4, 4, 8, 4, 0, 0);
                Assert.AreEqual(cf, new ColorFormat("S4:0,4:4,4:8,0:0"));
            }
            {
                var cf = new ColorFormat(16, 8, 8, 8, 0, 8, 24, 8);
                Assert.AreEqual(cf, new ColorFormat("S8:16,8:8,8:0,8:24"));
            }
            {
                var cf = new ColorFormat(0, 5, 5, 6, 11, 5, 0, 0);
                Assert.AreEqual(cf, new ColorFormat("S5:0,6:5,5:11,0:0"));
            }
            {
                var cf = new ColorFormat(0, 5, 5, 5, 10, 5, 15, 1);
                Assert.AreEqual(cf, new ColorFormat("S5:0,5:5,5:10,1:15"));
            }
        }
    }
}
