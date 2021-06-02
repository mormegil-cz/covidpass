using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using PeterO.Cbor;

namespace CovidPassQr.Lib
{
    public record CovidPass(string Version, string GivenName, string FamilyName, string GivenNameTranslit, string FamilyNameTranslit, DateTime DateOfBirth)
    {
        private static readonly Dictionary<int, string> cwtClaims = new()
        {
            // see https://datatracker.ietf.org/doc/html/rfc8392#section-4
            { 1, "iss" },
            { 2, "sub" },
            { 3, "aud" },
            { 4, "exp" },
            { 5, "nbf" },
            { 6, "iat" },
            { 7, "cti" }
        };

        private static readonly Dictionary<int, string> hCertClaims = new()
        {
            // see https://ec.europa.eu/health/sites/default/files/ehealth/docs/digital-green-certificates_v3_en.pdf
            { -260, "hcert" },
        };

        public static CovidPass ParseFromQr(string qrContent)
        {
            if (!qrContent.StartsWith("HC1:")) throw new FormatException("Invalid or unsupported data");
            var data = Base45.Decode(qrContent.Substring(4));

            CBORObject cbor;
            using (var stream = new InflaterInputStream(new MemoryStream(data))) cbor = CBORObject.Read(stream);

            if (!cbor.HasTag(18) || cbor.Type != CBORType.Array || cbor.Count != 4) throw new FormatException("Unexpected CBOR message structure");

            var protectedCbor = ReadCborFromByteString(cbor[0]);
            var unprotectedData = cbor[1];
            var payloadCbor = ReadCborFromByteString(cbor[2]);
            var signature = cbor[3];

            var payload = new Dictionary<string, CBORObject>();
            foreach (var key in payloadCbor.Keys)
            {
                var keyNum = key.AsInt32();
                var claim = keyNum < 0 ? hCertClaims[keyNum] : cwtClaims[keyNum];
                payload.Add(claim, payloadCbor[key]);
            }

            var hcert = payload["hcert"];
            var hcertContents = hcert[hcert.Keys.First()];

            var hcertVer = hcertContents["ver"].AsString();
            var hcertNam = hcertContents["nam"];
            var hcertDob = hcertContents["dob"].AsString();
            var hcertVaccination = hcertContents["v"];
            var hcertTesting = hcertContents["t"];

            if (hcertVer != "1.0.1") throw new FormatException("Unsupported hcert version " + hcertVer);

            return new CovidPass(
                hcertVer, hcertNam["gn"].AsString(), hcertNam["fn"].AsString(), hcertNam["gnt"].AsString(), hcertNam["fnt"].AsString(),
                DateTime.ParseExact(hcertDob, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None)
            );
        }

        private static CBORObject ReadCborFromByteString(CBORObject cbor)
        {
            if (cbor.Type != CBORType.ByteString) throw new FormatException("Unexpected CBOR message structure");
            using var stream = new MemoryStream(cbor.GetByteString());
            return CBORObject.Read(stream);
        }
    }
}