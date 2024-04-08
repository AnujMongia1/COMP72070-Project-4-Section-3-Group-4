using csharp_server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;
using System;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSocketSharp.Net.WebSockets;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private WebSocketServer _server;
        private WebSocket _client;

        private ActionState GetActionStateFromPacketType(PacketType type)
        {
            switch (type)
            {
                case PacketType.DrawingData:
                    return ActionState.Drawing;
                case PacketType.AddText:
                    return ActionState.Writing;
                case PacketType.AddImage:
                    return ActionState.AddingImage;
                case PacketType.EstablishConnection:
                    return ActionState.Connected;
                case PacketType.CloseConnection:
                    return ActionState.Disconnected;
                case PacketType.JoinSession:
                case PacketType.CreateSession:
                    return ActionState.Processing;
                default:
                    return ActionState.Idle;
            }
        }


        [TestInitialize]
        public void Initialize()
        {
            _server = new WebSocketServer("ws://127.0.0.1:7890");
            //_server.AddWebSocketService<Echo>("/Echo");
            _server.AddWebSocketService<EchoAll>("/EchoAll");
            _server.Start();

            _client = new WebSocket("ws://127.0.0.1:7890/EchoAll");
        }

        [TestMethod]
        public void TestOnOpen()
        {
            bool isConnected = false;

            _client.OnOpen += (sender, e) =>
            {
                isConnected = true;
            };

            _client.Connect();

            System.Threading.Thread.Sleep(1000);

            Assert.IsTrue(isConnected, "WebSocket connection should be open.");
        }

        [TestMethod]
        public void TestOnMessage()
        {
            //
        }


        [TestMethod]
        public async Task TestHandleCreateSessionPacket()
        {
            bool receivedResponse = false;
            bool received6DigitCode = false;

            _client.OnMessage += (sender, e) =>
            {
                receivedResponse = true;

                dynamic jsonPacketpenis = JsonConvert.DeserializeObject(e.Data);

                PacketType type = (PacketType)jsonPacketpenis.PacketHeader.Type;
                int sequenceNumber = jsonPacketpenis.PacketHeader.SequenceNumber;
                DateTime timeStamp = jsonPacketpenis.PacketHeader.TimeStamp;

                dynamic bodyData = jsonPacketpenis.PacketBody.Data;

                Packet receivedPacket = new Packet(PacketType.SessionDetails, sequenceNumber, timeStamp, bodyData);

                string receivedString = receivedPacket.PacketBody.Data.ToString();

                int number;
                if (receivedString.Length == 6 && int.TryParse(receivedString, out number))
                {
                    received6DigitCode = true;
                }
            };

            var createSessionPacket = new Packet(PacketType.CreateSession, 1, DateTime.Now, null);
            string jsonPacket = createSessionPacket.Serialize();
            _client.Connect();
            _client.Send(jsonPacket);

            await Task.Delay(1000);

            Assert.IsTrue(receivedResponse, "Server should respond to create session packet.");
            Assert.IsTrue(received6DigitCode, "Server should send a 6-digit code in response.");
        }

        [TestMethod]
        public void TestHandleJoinSessionPacket()
        {
            bool receivedResponse = false;
            bool sessionAdded = false;

            _client.OnMessage += (sender, e) =>
            {
                receivedResponse = true;
                dynamic jsonPacketpenis = JsonConvert.DeserializeObject(e.Data);
                PacketType type = (PacketType)jsonPacketpenis.PacketHeader.Type;
                if (type == PacketType.SessionDetails)
                {
                    sessionAdded = true;
                }
            };

            var joinSessionPacket = new Packet(PacketType.JoinSession, 1, DateTime.Now, "123456");
            string jsonPacket = joinSessionPacket.Serialize();
            _client.Connect();
            _client.Send(jsonPacket);

            Thread.Sleep(1000);

            Assert.IsTrue(receivedResponse, "Server should respond to join session packet.");
            Assert.IsTrue(sessionAdded, "Server should add the client to the session.");
        }


        [TestMethod]
        public void TestSerialization()
        {
            // Arrange
            Packet packet = new Packet(PacketType.EstablishConnection, 1, DateTime.Now, "Test message");

            // Act
            string serializedPacket = packet.Serialize();

            // Assert
            Assert.IsNotNull(serializedPacket, "Serialized packet should not be null.");
        }

        [TestMethod]
        public void TestDeserialization()
        {
            // Arrange
            string serializedPacket = "{\"PacketHeader\":{\"Type\":0,\"SequenceNumber\":1,\"TimeStamp\":\"2024-04-07T16:40:48.0320777-04:00\"},\"PacketBody\":{\"Data\":null}}";

            // Act
            Packet deserializedPacket = Packet.Deserialize(System.Text.Encoding.ASCII.GetBytes(serializedPacket));

            // Assert
            Assert.IsNotNull(deserializedPacket, "Deserialized packet should not be null.");
        }

        [TestMethod]
        public void TestPacketType()
        {
            // Arrange
            Packet packet = new Packet(PacketType.AddImage, 1, DateTime.Now, "Test message");

            // Act
            PacketType type = packet.PacketHeader.Type;

            // Assert
            Assert.AreEqual(PacketType.AddImage, type, "Packet type should be AddImage.");
        }

        [TestMethod]
        public void TestPacketTimestamp()
        {
            // Arrange
            DateTime currentTime = DateTime.Now;
            Packet packet = new Packet(PacketType.AddText, 1, currentTime, "Test message");

            // Act
            DateTime timestamp = packet.PacketHeader.TimeStamp;

            // Assert
            Assert.AreEqual(currentTime, timestamp, "Packet timestamp should be set correctly.");
        }

        [TestMethod]
        public void TestStateIdle()
        {
            // Arrange
            ActionState state = default(ActionState);

            // Assert
            Assert.AreEqual(ActionState.Idle, state, "State should be idle by default.");
        }


        [TestMethod]
        public void TestStateTransition()
        {
            // Arrange
            ActionState state = ActionState.Idle;

            // Simulate receiving a drawing packet
            var packet = new Packet(PacketType.DrawingData, 1, DateTime.Now, null);
            state = GetActionStateFromPacketType(packet.PacketHeader.Type);

            // Assert
            Assert.AreEqual(ActionState.Drawing, state, "State should transition to Drawing.");
        }

        [TestMethod]
        public void TestTextPacketSerialization()
        {
            var packet = new Packet(PacketType.AddText, 1, DateTime.Now, "Some text to add");

            string serializedPacket = packet.Serialize();
            Packet deserializedPacket = Packet.Deserialize(Encoding.ASCII.GetBytes(serializedPacket));

            Assert.AreEqual(PacketType.AddText, deserializedPacket.PacketHeader.Type);
            Assert.AreEqual(1, deserializedPacket.PacketHeader.SequenceNumber);
            Assert.AreEqual(packet.PacketHeader.TimeStamp, deserializedPacket.PacketHeader.TimeStamp);
            Assert.AreEqual(packet.PacketBody.Data, deserializedPacket.PacketBody.Data);
        }

        public void TestDrawingPacketSerialization()
        {

            var packet = new Packet(PacketType.DrawingData, 1, DateTime.Now, new List<string> { "point1", "point2", "point3" });


            string serializedPacket = packet.Serialize();


            Packet deserializedPacket = Packet.Deserialize(Encoding.ASCII.GetBytes(serializedPacket));

            Assert.AreEqual(PacketType.DrawingData, deserializedPacket.PacketHeader.Type);
            Assert.AreEqual(1, deserializedPacket.PacketHeader.SequenceNumber);
            Assert.AreEqual(packet.PacketHeader.TimeStamp, deserializedPacket.PacketHeader.TimeStamp);

            var data = deserializedPacket.PacketBody.Data as IEnumerable<string>;

            var orderedData = data.OrderBy(x => x).ToList();

            var expected = new List<string> { "point1", "point2", "point3" };

            Assert.IsTrue(expected.SequenceEqual(orderedData));
        }
        [TestMethod]
        public void TestImagePacketSerialization()
        {
            // Arrange
            var packet = new Packet(PacketType.AddImage, 1, DateTime.Now, new
            {
                Position = new { X = 214.40000000000009, Y = 133.6 },
                ImageData = "/9j/4AAQSkZJRgAdGckDH4DP4muiv7lLcq45G4rgc4Fec3d60EqzDkxsHAPOcHNXpvHUEtuZhE0kqghYSCBn3PpXn1o6Ky1R6ypyklbY6C9vvOLRAkvL8zHphew/Ej8hWHqyESRDPO3pj1P/6qs2PmPullbfNJhnb1PsOw7Cq9+we+C8EKOfw6183WhKNZpnbSaWiP/9k="
            });

            // Act
            string serializedPacket = packet.Serialize();
            Packet deserializedPacket = Packet.Deserialize(Encoding.ASCII.GetBytes(serializedPacket));

            // Assert
            var data = deserializedPacket.PacketBody.Data as JObject;
            var position = data["Position"].ToObject<JObject>();
            var imageData = data["ImageData"].ToString();

            Assert.AreEqual(PacketType.AddImage, deserializedPacket.PacketHeader.Type);
            Assert.AreEqual(1, deserializedPacket.PacketHeader.SequenceNumber);
            Assert.AreEqual(packet.PacketHeader.TimeStamp, deserializedPacket.PacketHeader.TimeStamp);

            Assert.AreEqual(214.40000000000009, position["X"].Value<double>());
            Assert.AreEqual(133.6, position["Y"].Value<double>());
            Assert.AreEqual("/9j/4AAQSkZJRgAdGckDH4DP4muiv7lLcq45G4rgc4Fec3d60EqzDkxsHAPOcHNXpvHUEtuZhE0kqghYSCBn3PpXn1o6Ky1R6ypyklbY6C9vvOLRAkvL8zHphew/Ej8hWHqyESRDPO3pj1P/6qs2PmPullbfNJhnb1PsOw7Cq9+we+C8EKOfw6183WhKNZpnbSaWiP/9k=", imageData);
        }


        [TestMethod]
        public void TestStateBasedOnPacketType()
        {
            ActionState actionState;

            var packet1 = new Packet(PacketType.DrawingData, 1, DateTime.Now, null);
            actionState = GetActionStateFromPacketType(packet1.PacketHeader.Type);
            Assert.AreEqual(ActionState.Drawing, actionState);

            var packet2 = new Packet(PacketType.AddText, 1, DateTime.Now, null);
            actionState = GetActionStateFromPacketType(packet2.PacketHeader.Type);
            Assert.AreEqual(ActionState.Writing, actionState);

            var packet3 = new Packet(PacketType.AddImage, 1, DateTime.Now, null);
            actionState = GetActionStateFromPacketType(packet3.PacketHeader.Type);
            Assert.AreEqual(ActionState.AddingImage, actionState);

            var packet4 = new Packet(PacketType.EstablishConnection, 1, DateTime.Now, null);
            actionState = GetActionStateFromPacketType(packet4.PacketHeader.Type);
            Assert.AreEqual(ActionState.Connected, actionState);

            var packet5 = new Packet(PacketType.CloseConnection, 1, DateTime.Now, null);
            actionState = GetActionStateFromPacketType(packet5.PacketHeader.Type);
            Assert.AreEqual(ActionState.Disconnected, actionState);

            var packet6 = new Packet(PacketType.JoinSession, 1, DateTime.Now, null);
            actionState = GetActionStateFromPacketType(packet6.PacketHeader.Type);
            Assert.AreEqual(ActionState.Processing, actionState);

            var packet7 = new Packet(PacketType.CreateSession, 1, DateTime.Now, null);
            actionState = GetActionStateFromPacketType(packet7.PacketHeader.Type);
            Assert.AreEqual(ActionState.Processing, actionState);

            var packet8 = new Packet((PacketType)100, 1, DateTime.Now, null);
            actionState = GetActionStateFromPacketType(packet8.PacketHeader.Type);
            Assert.AreEqual(ActionState.Idle, actionState);
        }
        [TestMethod]
        public void TN_TestOnOpen()
        {
            bool isConnected = false;

            _client.OnOpen += (sender, e) =>
            {
                isConnected = true;
            };
            Assert.IsFalse(isConnected, "WebSocket connection should not be open.");
        }

        [TestMethod]
        public void FP_TestOnOpen()
        {
            bool isConnected = true;
            _client.OnOpen += (sender, e) =>
            {
                isConnected = true;
            };
            Assert.IsFalse(isConnected, "WebSocket connection should not be open."); // Change to Assert.IsFalse
        }

        [TestMethod]
        public void FN_TestOnMessage()
        {
            bool receivedMessage = false;

            _client.OnMessage += (sender, e) =>
            {
                receivedMessage = true;
            };
            Assert.IsTrue(receivedMessage, "Did not receive a message."); // Change to Assert.IsTrue
        }
        [TestMethod]
        public void TN_TestOnMessage()
        {
            bool receivedMessage = false;
            Assert.IsFalse(receivedMessage, "Did not receive a message.");
        }
        [TestMethod]
        public void FN_TestOnOpen()
        {
            bool isConnected = false;

            _client.OnOpen += (sender, e) =>
            {
                isConnected = true;
            };
            // Client is not connected, so isConnected should remain false
            Assert.IsFalse(isConnected, "WebSocket connection should not be open.");
        }

        [TestMethod]
        public void FP_TestOnMessage()
        {
            bool receivedMessage = false;
            _client.OnMessage += (sender, e) =>
            {
                receivedMessage = true;
            };
            // No message is received, so receivedMessage should remain false
            Assert.IsFalse(receivedMessage, "Did not receive a message.");
        }


        [TestCleanup]
        public void Cleanup()
        {
            _server.Stop();
        }
    }
}