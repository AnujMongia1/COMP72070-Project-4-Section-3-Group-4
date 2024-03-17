using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WpfApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var wssv = new WebSocketServer(IPAddress.Parse("127.0.0.1"), 7890);
            wssv.AddWebSocketService<Echo>("/EchoAll");
            wssv.Start();

            Console.WriteLine("WebSocket server started at ws://127.0.0.1:7890/EchoAll");

            Console.ReadKey();
            wssv.Stop();
        }

        public class Echo : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs e)
            {
                try
                {
                    var packet = DrawingPacket.Deserialize(e.RawData);
                    Console.WriteLine($"Received {packet.Type} packet with {packet.Coordinates.Count} coordinates");

                    // Broadcast the received drawing data to all clients
                    Sessions.Broadcast(e.RawData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling packet: {ex.Message}");
                }
            }
        }
    }

    public struct DrawingPacket
    {
        public PacketType Type { get; set; }
        public int Length { get; set; }
        public List<Point> Coordinates { get; set; }

        public static DrawingPacket Deserialize(byte[] rawData)
        {
            DrawingPacket packet = new DrawingPacket();
            packet.Type = (PacketType)BitConverter.ToInt32(rawData, 0);
            packet.Length = BitConverter.ToInt32(rawData, 4);

            packet.Coordinates = new List<Point>();
            for (int i = 8; i < rawData.Length; i += 16)
            {
                double x = BitConverter.ToDouble(rawData, i);
                double y = BitConverter.ToDouble(rawData, i + 8);
                packet.Coordinates.Add(new Point(x, y));
            }

            return packet;
        }
    }

    public enum PacketType
    {
        DrawingData
    }

    public struct Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
