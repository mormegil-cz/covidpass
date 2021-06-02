using System;
using System.Diagnostics;

namespace CovidPassQr.Lib
{
    public static class Base45
    {
        private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";
        private const char AlphabetMin = ' ';
        private const char AlphabetMax = 'Z';
        private static readonly int[] alphabetInverse = BuildAlphabetInverse();

        public static string Encode(byte[] data)
        {
            var lastPiece = data.Length % 2;
            var result = new char[(data.Length / 2) * 3 + lastPiece * 2];
            var pos = 0;
            for (int i = 0; i < data.Length - 1; i += 2)
            {
                var a = data[i];
                var b = data[i + 1];
                var val = a * 256 + b;

                var e = Math.DivRem(val, 45 * 45, out var rem);
                var d = Math.DivRem(rem, 45, out var c);

                result[pos++] = Alphabet[c];
                result[pos++] = Alphabet[d];
                result[pos++] = Alphabet[e];
            }

            if (lastPiece > 0)
            {
                var d = Math.DivRem(data[^1], 45, out var c);
                result[pos++] = Alphabet[c];
                result[pos++] = Alphabet[d];
            }

            Debug.Assert(pos == result.Length);

            return new string(result);
        }

        public static byte[] Decode(string encoded)
        {
            var fullPieces = encoded.Length / 3;
            var remainingChars = encoded.Length % 3;
            if (remainingChars == 1) throw new FormatException("Invalid Base45 data length");
            var result = new byte[fullPieces * 2 + remainingChars / 2];

            var chars = encoded.ToCharArray();
            var pos = 0;
            for (var i = 0; i < encoded.Length - 2; i += 3)
            {
                var c = GetAlphabetInverse(chars[i]);
                var d = GetAlphabetInverse(chars[i + 1]);
                var e = GetAlphabetInverse(chars[i + 2]);

                var val = e * 45 * 45 + d * 45 + c;

                result[pos++] = (byte) (val >> 8);
                result[pos++] = (byte) (val & 0xFF);
            }

            if (remainingChars > 0)
            {
                var c = GetAlphabetInverse(chars[^2]);
                var d = GetAlphabetInverse(chars[^1]);

                var val = d * 45 + c;

                result[pos++] = (byte) val;
            }

            Debug.Assert(pos == result.Length);

            return result;
        }

        private static int GetAlphabetInverse(char c)
        {
            if (c is < AlphabetMin or > AlphabetMax) throw new FormatException($"Invalid Base45 character U+{(int) c:X4}");
            var result = alphabetInverse[c - AlphabetMin];
            if (result < 0) throw new FormatException($"Invalid Base45 character U+{(int) c:X4}");
            return result;
        }

        private static int[] BuildAlphabetInverse()
        {
            var result = new int[AlphabetMax - AlphabetMin + 1];
            Array.Fill(result, -1);
            for (var i = 0; i < Alphabet.Length; ++i)
            {
                result[Alphabet[i] - AlphabetMin] = (byte) i;
            }
            return result;
        }
    }
}