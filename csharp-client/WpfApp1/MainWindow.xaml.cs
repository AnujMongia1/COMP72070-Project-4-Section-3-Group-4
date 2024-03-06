using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            ws.OnMessage += Ws_OnMessage;
            ws.Connect();
            ws.Send("connected");
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    List<List<Point>> strokesData = JsonConvert.DeserializeObject<List<List<Point>>>(e.Data);

                    foreach (List<Point> strokePoints in strokesData)
                    {
                        StylusPointCollection points = new StylusPointCollection();
                        foreach (Point point in strokePoints)
                        {
                            points.Add(new StylusPoint(point.X, point.Y));
                        }
                        Stroke newStroke = new Stroke(points);
                        inkCanvas.Strokes.Add(newStroke);
                    }
                }
                catch (Exception ex)
                {
                    // Handle parsing or drawing errors
                    Console.WriteLine("Error: " + ex.Message);
                }
            });
        }

        private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            var strokesData = new List<List<Point>>();
            foreach (var stroke in inkCanvas.Strokes)
            {
                var strokePoints = new List<Point>();
                foreach (var stylusPoint in stroke.StylusPoints)
                {
                    strokePoints.Add((Point)stylusPoint);
                }
                strokesData.Add(strokePoints);
            }

            string jsonStrokes = JsonConvert.SerializeObject(strokesData, Newtonsoft.Json.Formatting.Indented);
            ws.Send(jsonStrokes);
        }

        private void SendDrawingData()
        {
/*            var strokesData = new List<List<Point>>();
            foreach (var stroke in inkCanvas.Strokes)
            {
                var strokePoints = new List<Point>();
                foreach (var stylusPoint in stroke.StylusPoints)
                {
                    strokePoints.Add((Point)stylusPoint);
                }
                strokesData.Add(strokePoints);
            }

            string jsonStrokes = JsonConvert.SerializeObject(strokesData, Newtonsoft.Json.Formatting.Indented);
            ws.Send(jsonStrokes);*/
        }
    }
}
