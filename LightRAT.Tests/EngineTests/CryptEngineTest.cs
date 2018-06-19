using NUnit.Framework;
using LightRAT.Engine;
using System.Text;

namespace LightRAT.Tests.EngineTests
{
    [TestFixture]
    class CryptEngineTest
    {
        [Test]
        public void Compress_Decompress__ReturnTheSameInput()
        {
            var sampleString = "Hello World";

            var compressed = CryptEngine.Compress(Encoding.ASCII.GetBytes(sampleString));
            var decompressed = Encoding.ASCII.GetString(CryptEngine.Decompress(compressed));

            Assert.AreEqual(sampleString, decompressed);
        }
    }
}
