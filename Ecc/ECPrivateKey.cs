﻿using System;
using System.Numerics;

namespace Ecc {
    public class ECPrivateKey {

        public readonly ECCurve Curve;

        public readonly BigInteger D;

        public ECPublicKey PublicKey => new ECPublicKey(Curve.G * D);

        public ECPrivateKey(BigInteger d, ECCurve curve) {
            if (d.Sign < 0) throw new ArgumentOutOfRangeException(nameof(d), "d must be positive");
            D = d;
            Curve = curve;
        }

        public ECSignature Sign(byte[] hash) {
            var num = BigIntegerExt.FromBigEndianBytes(hash);
            return Sign(num);
        }

        public ECSignature? Sign(byte[] hash, BigInteger random) {
            var num = BigIntegerExt.FromBigEndianBytes(hash);
            return Sign(num, random);
        }

        public ECSignature Sign(BigInteger message) {
            var truncated = Curve.TruncateHash(message);
            ECSignature? signature;
            do {
                var random = BigIntegerExt.ModRandom(Curve.Order);
                signature = SignTruncated(truncated, random);
            } while (signature == null);
            return signature.Value;
        }

        public ECSignature? Sign(BigInteger message, BigInteger random) {
            var truncated = Curve.TruncateHash(message);
            return SignTruncated(truncated, random);
        }

        private ECSignature? SignTruncated(BigInteger message, BigInteger random) {
            var p = Curve.G * random;
            var r = p.X % Curve.Order;
            if (r == 0) return null;
            var s = ((message + r * D) * BigIntegerExt.ModInverse(random, Curve.Order)) % Curve.Order;
            if (s == 0) return null;
            return new ECSignature(r, s, Curve);
        }

        public static ECPrivateKey Create(ECCurve curve) {
            var priv = BigIntegerExt.ModRandom(curve.Order);
            //todo: check not zero
            return new ECPrivateKey(priv, curve);
        }

        public static ECPrivateKey ParseHex(string hex, ECCurve curve) {
            return new ECPrivateKey(BigIntegerExt.ParseHexUnsigned(hex), curve);
        }

    }
}
