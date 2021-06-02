using System.Text;
using CovidPassQr.Lib;
using NUnit.Framework;

namespace CovidPassQr.Tests
{
    public class Base45Test
    {
        [Test]
        public void Encode()
        {
            // see https://datatracker.ietf.org/doc/draft-faltstrom-base45/ section 4.3
            Assert.AreEqual("BB8", Base45.Encode(Encoding.ASCII.GetBytes("AB")));
            Assert.AreEqual("%69 VD92EX0", Base45.Encode(Encoding.ASCII.GetBytes("Hello!!")));
            Assert.AreEqual("UJCLQE7W581", Base45.Encode(Encoding.ASCII.GetBytes("base-45")));
            
            // +inverse of section 4.4
            Assert.AreEqual("QED8WEX0", Base45.Encode(Encoding.ASCII.GetBytes("ietf!")));
        }

        [Test]
        public void Decode()
        {
            // see https://datatracker.ietf.org/doc/draft-faltstrom-base45/ section 4.4
            Assert.AreEqual("ietf!", Encoding.ASCII.GetString(Base45.Decode("QED8WEX0")));

            // +inverse of section 4.3
            Assert.AreEqual("AB", Encoding.ASCII.GetString(Base45.Decode("BB8")));
            Assert.AreEqual("Hello!!", Encoding.ASCII.GetString(Base45.Decode("%69 VD92EX0")));
            Assert.AreEqual("base-45", Encoding.ASCII.GetString(Base45.Decode("UJCLQE7W581")));
        }
    }
}
