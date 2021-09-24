using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace SurfaceClient {
    class SurfaceClient {

        private static UdpClient udpClient = new UdpClient();

        static void Main(string[] args) {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(InterruptReceived);

            ImageRetriever imageRetriever = new ImageRetriever();

            udpClient.Connect("127.0.0.1", 50001);

            int i = 0;

            while (true) {
                Console.WriteLine("hej" + ++i);
                byte[] image = imageRetriever.GetImage();
                if (image != null) {
                    foreach (byte piece in image) {
                        Console.WriteLine(piece);
                    }
                } else {
                    Console.WriteLine("image was null");
                }
                string cardString = ImageAnalyzer.AnalyzeImage(image);
                byte[] bytesToSend = Encoding.ASCII.GetBytes(cardString);
                udpClient.Send(bytesToSend, bytesToSend.Length);
            }
        }

        private static void InterruptReceived(object sender, ConsoleCancelEventArgs e) {
            Console.WriteLine("Interrup signal received, shutting down client.");
            udpClient.Close();
            Environment.Exit(0);
        }
    }
}
