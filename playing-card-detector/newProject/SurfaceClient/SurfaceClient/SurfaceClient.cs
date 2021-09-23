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

			while (true) {
				byte[] image = imageRetriever.GetImage();
				byte[] bytesToSend = Encoding.ASCII.GetBytes(ImageAnalyzer.AnalyzeImage(image));
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
