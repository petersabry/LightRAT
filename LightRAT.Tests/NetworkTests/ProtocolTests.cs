using System;
using System.Linq;
using NUnit.Framework;
using LightRAT.Core.Network.Protocol;
using System.Threading;
using System.IO;

namespace LightRAT.Tests.NetworkTests
{
    [TestFixture]
    public class ProtocolTests
    {
        [Test]
        public void Frame__ReturnsDataWithHeaderThatContainsItsLength()
        {
            byte[] data = { 0, 1, 2 };
            var result = MessageFramingProtocol.Frame(data);
            var dataLengthFromTheHeader = BitConverter.ToInt32(result.Take(4).ToArray(), 0);
        
            Assert.Greater(result.Length, data.Length);
            Assert.AreEqual(dataLengthFromTheHeader, data.Length);
        }
        [Test]
        public void Frame_PassingNullData_ThrowsNullException()
        {
            Assert.Throws<ArgumentNullException>(() => MessageFramingProtocol.Frame(null));
        }
        [Test]
        public void Read_PassingLowSizeData_ReturnsTheSameData()
        {
            byte[] data = { 0, 1, 2 };
            byte[] framedData = MessageFramingProtocol.Frame(data);
            byte[] receivedData = null;
            var protocol = new MessageFramingProtocol(20);
            protocol.DataReceivedEvent += (recevied) => receivedData = recevied;

            protocol.Read(framedData);

            Assert.AreEqual(receivedData, data);
        }
        [Test]
        public void Read_PassingHighSizeData_ReturnsTheSameData()
        {
            var path = Path.GetFullPath(TestContext.CurrentContext.TestDirectory) + @"\SampleTextFile_1000kb.txt";
            byte[] data = File.ReadAllBytes(path);
            byte[] framedData = MessageFramingProtocol.Frame(data);
            byte[] receivedData = null;
            var protocol = new MessageFramingProtocol(20 * 1024 * 1024);
            protocol.DataReceivedEvent += (recevied) => receivedData = recevied;

            protocol.Read(framedData);

            Assert.AreEqual(receivedData, data);
        }
        [Test] //TODO: Implement this test.
        public void Read_PassingHighSizeDataWithNetworkSimulation_ReturnsTheSameData()
        {
            //var path = Path.GetFullPath(TestContext.CurrentContext.TestDirectory) + @"\SampleTextFile_1000kb.txt";
            //byte[] data = File.ReadAllBytes(path);
            //byte[] framedData = MessageFramingProtocol.Frame(data);
            //byte[] receivedData = null;
            //var protocol = new MessageFramingProtocol(20 * 1024 * 1024);
            //protocol.DataReceivedEvent += (recevied) => receivedData = recevied;

            //protocol.Read(framedData);

            //Assert.AreEqual(receivedData, data);
        }
    }
}
