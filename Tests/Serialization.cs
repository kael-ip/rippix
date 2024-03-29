﻿#if DEBUG
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rippix.Tests {
    public class SerializationTests {
        [Test]
        public void TestSerializeDeserialize() {
            var m = new Model.Document();
            m.FileName = "xxx";
            m.SHA1 = "0123456789abcdef";
            var f1 = new Model.Format() {
                Code = "P4"
            };
            f1.Parameters.Add(new Model.Parameter() { Name = "a", Value = "1" });
            f1.Parameters.Add(new Model.Parameter() { Name = "b", Value = "2" });
            var r1 = new Model.Resource() {
                Name = "res1",
                Offset = 1000,
                Format = f1
            };
            m.Resources.Add(r1);
            Model.Helper.Save(m, "test.xml");
            var zm = Model.Helper.Load<Model.Document>("test.xml");
            Assert.IsNotNull(zm);
            Assert.AreEqual(m.FileName, zm.FileName);
            Assert.AreEqual(m.Resources.Count, zm.Resources.Count);
            for (int i = 0; i < m.Resources.Count; i++) {
                var mr = m.Resources[i];
                var zmr = zm.Resources[i];
                Assert.AreEqual(mr.Name, zmr.Name);
                Assert.AreEqual(mr.Offset, zmr.Offset);
                Assert.AreEqual(mr.Format.Code, zmr.Format.Code);
                for (int j = 0; j < mr.Format.Parameters.Count; j++) {
                    Assert.AreEqual(mr.Format.Parameters[j].Name, zmr.Format.Parameters[j].Name);
                    Assert.AreEqual(mr.Format.Parameters[j].Value, zmr.Format.Parameters[j].Value);
                }
            }
        }
        [Test]
        public void TestConvertToBase16() {
            Assert.AreEqual("123456789abcde", Rippix.Model.Helper.ConvertToBase16(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde }));
        }
    }
}
#endif