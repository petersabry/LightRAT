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
            protocol.DataReceivedEvent += (received) => receivedData = received;

            protocol.Read(framedData);

            Assert.AreEqual(data, receivedData);
        }
        [Test]
        public void Read_PassingHighSizeData_ReturnsTheSameData()
        {
            var path = Path.GetFullPath(TestContext.CurrentContext.TestDirectory) + @"\SampleTextFile_1000kb.txt";
            byte[] data = File.ReadAllBytes(path);
            byte[] framedData = MessageFramingProtocol.Frame(data);
            byte[] receivedData = null;
            var protocol = new MessageFramingProtocol(20 * 1024 * 1024);
            protocol.DataReceivedEvent += (received) => receivedData = received;

            protocol.Read(framedData);

            Assert.AreEqual(data, receivedData);
        }
        [Test] 
        public void Read_PassingHighSizeDataWithNetworkSimulation_ReturnsTheSameData()
        {
            var path = Path.GetFullPath(TestContext.CurrentContext.TestDirectory) + @"\SampleTextFile_1000kb.txt";
            byte[] data = File.ReadAllBytes(path);
            byte[] framedData = MessageFramingProtocol.Frame(data);
            byte[] receivedData = null;
            var protocol = new MessageFramingProtocol(20 * 1024 * 1024);
            protocol.DataReceivedEvent += (received) => receivedData = received;

            using (var ms = new MemoryStream(framedData))
            {
                var tempBuffer = new byte[100];

                for (int i = 100; i <= framedData.Length; i += 100)
                {
                    ms.Read(tempBuffer, 0, tempBuffer.Length);
                    protocol.Read(tempBuffer);
                    ms.Seek(i, SeekOrigin.Begin);
                }
            }

            Assert.AreEqual(data, receivedData);
        }
        [Test]
        public void Read_DataBelongsToTheNextPacket_ReturnsTheSameDataOfTheFirstPacket_StartsNewSessionAndReturnsTheDataOfTheSecondPacket()
        {
            byte[] dataOne = { 0, 1, 2 };
            byte[] dataTwo = { 3, 4, 5 };

            byte[] finalBytes = new byte[dataOne.Length + dataTwo.Length];
            dataOne.CopyTo(finalBytes, 0);
            dataTwo.CopyTo(finalBytes, dataOne.Length);


            byte[] framedDataOne = MessageFramingProtocol.Frame(dataOne);
            byte[] framedDataTwo = MessageFramingProtocol.Frame(dataTwo);

            byte[] finalFramedBytes = new byte[framedDataOne.Length + framedDataTwo.Length];
            framedDataOne.CopyTo(finalFramedBytes, 0);
            framedDataTwo.CopyTo(finalFramedBytes, framedDataOne.Length);

            byte[] receivedData = null;
            var protocol = new MessageFramingProtocol(20);
            protocol.DataReceivedEvent += (received) =>
            {

               if (receivedData == null) receivedData = received;
                else received.FlexCopyTo<byte>(ref receivedData);
            };
            protocol.Read(finalFramedBytes);

            Assert.AreEqual(finalBytes, receivedData);
        }



    }
    public  static class ArrayExtensions
    {
        public static void FlexCopyTo<T>(this T[] array, ref T[] destArray)
        {
            int count = destArray.Count();
            Array.Resize(ref destArray, count + array.Length);
            array.CopyTo(destArray, count);
        }
    }
}
