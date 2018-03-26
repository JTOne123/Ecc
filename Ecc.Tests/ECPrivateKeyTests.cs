﻿using NUnit.Framework;
using System;
using System.Numerics;

namespace Ecc.Tests {
    [TestFixture]
    [TestOf(typeof(ECPrivateKey))]
    public class ECPrivateKeyTests {

        [Test]
        public void CreateTest() {
            var curve = new ECCurve {
                A = 2,
                B = 3,
                Modulus = 97,
                Order = 100
            };
            var privateKey = ECPrivateKey.Create(curve);
            Assert.IsNotNull(privateKey);
        }

        [Test]
        public void SignIntTest() {
            var curve = ECCurve.Secp256k1;
            var privateKey = curve.CreatePrivateKey("8ce00ada2dffcfe03bd4775e90588f3f039bd83d56026fafd6f33080ebff72e8");
            var msg = BigIntegerExt.ParseHexUnsigned("7846e3be8abd2e089ed812475be9b51c3cfcc1a04fafa2ddb6ca6869bf272715");
            var random = BigIntegerExt.ParseHexUnsigned("cd6f06360fa5af8415f7a678ab45d8c1d435f8cf054b0f5902237e8cb9ee5fe5");
            var signature = privateKey.Sign(msg, random);
            var rhex = signature.R.ToHexUnsigned(32);
            var shex = signature.S.ToHexUnsigned(32);
            Assert.AreEqual("2794dd08b1dfa958552bc37916515a3accb0527e40f9291d62cc4316047d24dd", rhex);
            Assert.AreEqual("5dd1f95f962bb6871967dc17b22217100daa00a3756feb1e16be3e6936fd8594", shex);
        }

        [Test]
        public void SignBytesTest() {
            var curve = ECCurve.Secp256k1;
            var privateKey = curve.CreatePrivateKey("8ce00ada2dffcfe03bd4775e90588f3f039bd83d56026fafd6f33080ebff72e8");
            var msgHex = "7846e3be8abd2e089ed812475be9b51c3cfcc1a04fafa2ddb6ca6869bf272715";
            var msg = GetBytes(msgHex);
            var random = BigIntegerExt.ParseHexUnsigned("cd6f06360fa5af8415f7a678ab45d8c1d435f8cf054b0f5902237e8cb9ee5fe5");
            var signature = privateKey.Sign(msg, random);
            var rhex = signature.R.ToHexUnsigned(32);
            var shex = signature.S.ToHexUnsigned(32);
            Assert.AreEqual("2794dd08b1dfa958552bc37916515a3accb0527e40f9291d62cc4316047d24dd", rhex);
            Assert.AreEqual("5dd1f95f962bb6871967dc17b22217100daa00a3756feb1e16be3e6936fd8594", shex);
        }

        [Test]
        public void SignVerifyTest() {
            foreach (var curve in ECCurve.GetNamedCurves()) {
                var privateKey = curve.CreateKeyPair();
                var msg = BigIntegerExt.ParseHexUnsigned("7846e3be8abd2e089ed812475be9b51c3cfcc1a04fafa2ddb6ca6869bf272715");
                var signature = privateKey.Sign(msg);
                var valid = privateKey.PublicKey.VerifySignature(msg, signature);
                Assert.IsTrue(valid, $"curve {curve.Name}, r and s are valid");
                signature.R += 5;
                valid = privateKey.PublicKey.VerifySignature(msg, signature);
                Assert.IsFalse(valid, $"curve {curve.Name}, r is invalid");
                signature.R -= 5;
                signature.S += 5;
                valid = privateKey.PublicKey.VerifySignature(msg, signature);
                Assert.IsFalse(valid, $"curve {curve.Name}, s is invalid");
            }
        }


        [TestCase("42c6d7b9570c75e3778b3ac7bfd851198816da1dd39ce420f16378c9418b8a58", "0320319526ae7bb161eb650486c9c4ff80ab4f9e18d04d9da3651000d0ac335f16")]
        [TestCase("754119b222e208b4c24936bd6aae22b3f760956f905103270705e5257e467344", "02334180cfb8553b774d871c36be174171003cd13cb8325ad091b4f4b10934662f")]
        [TestCase("9ada5ef64bff005b95afb0acb5f7d51df1f42ed5435c09ab7eb4ed9b6eb08572", "02861529d088817d897efcc7233ff344a85c905bd2ae524b359eacd39537b5cb8e")]
        public void PublicKeyTest(string privateKeyHex, string publicKeyHex) {
            var curve = ECCurve.Secp256k1;
            var privateKey = curve.CreatePrivateKey(privateKeyHex);
            var publicKey = privateKey.PublicKey;
            Assert.AreEqual(publicKeyHex, publicKey.ToString());
        }

        private static byte[] GetBytes(string hex) {
            var res = new byte[hex.Length / 2];

            for (var i = 0; i < hex.Length; i++) {
                var ch = hex[i];
                res[i >> 1] <<= 4;
                res[i >> 1] |= GetHexDigit(ch);
            }

            return res;
        }

        private static byte GetHexDigit(char ch) {
            if (ch >= '0' && ch <= '9') return (byte)(ch - '0');
            if (ch >= 'A' && ch <= 'F') return (byte)(ch - 'A' + 10);
            if (ch >= 'a' && ch <= 'f') return (byte)(ch - 'a' + 10);
            throw new ArgumentOutOfRangeException(nameof(ch));
        }

    }
}
