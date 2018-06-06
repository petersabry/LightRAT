using NUnit.Framework;
using LightRAT.Core.Network.Engine;

namespace LightRAT.Tests.EngineTests
{
    [TestFixture]
    class CryptEngineTest
    {
        [Test]
        public void Compress_Decompress__ReturnTheSameInput()
        {
            var sampleString = "Hello World";

            var compressed = CryptEngine.Compress(sampleString);
            var decompressed = CryptEngine.Decompress(compressed);

            Assert.AreEqual(sampleString, decompressed);
        }
    }
}
