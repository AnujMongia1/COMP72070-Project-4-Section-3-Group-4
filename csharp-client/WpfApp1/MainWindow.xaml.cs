using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using WebSocketSharp;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private WebSocket ws;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWebSocket();
        }

        private void InitializeWebSocket()
        {
            ws = new WebSocket("ws://127.0.0.1:7890/EchoAll");
            ws.OnOpen += (sender, e) =>
            {
                Console.WriteLine("WebSocket connection established");
            };
            ws.OnMessage += (sender, e) =>
            {
                Console.WriteLine("Message received: " + e.Data);
            };
            ws.Connect();
        }

        private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            var strokesData = new List<Point>();
            foreach (var stroke in inkCanvas.Strokes)
            {
                foreach (var stylusPoint in stroke.StylusPoints)
                {
                    strokesData.Add(new Point(stylusPoint.X, stylusPoint.Y));
                }
            }

            byte[] packetBytes = SerializePacket(strokesData);
            ws.Send(packetBytes);
        }

        private byte[] SerializePacket(List<Point> coordinates)
        {
            var packetTypeBytes = BitConverter.GetBytes((int)PacketType.DrawingData);
            var packetLengthBytes = BitConverter.GetBytes(coordinates.Count * 2 * sizeof(double));

            var packetDataBytes = new List<byte>();
            foreach (var point in coordinates)
            {
                packetDataBytes.AddRange(BitConverter.GetBytes(point.X));
                packetDataBytes.AddRange(BitConverter.GetBytes(point.Y));
            }

            var packetBytes = new List<byte>();
            packetBytes.AddRange(packetTypeBytes);
            packetBytes.AddRange(packetLengthBytes);
            packetBytes.AddRange(packetDataBytes);

            return packetBytes.ToArray();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ws.Close();
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
}
