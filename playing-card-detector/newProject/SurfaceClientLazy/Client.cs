using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace SurfaceClient {
    class Client {

        private UdpClient udpClient = new UdpClient();
        private ImageAnalyzer analyzer = new ImageAnalyzer();

        public Client() {
            udpClient.Connect("127.0.0.1", 50001);
        }

        public void Process(byte[] image) {
            string cardString = analyzer.AnalyzeImage(image);
            byte[] bytesToSend = Encoding.ASCII.GetBytes(cardString);
            udpClient.Send(bytesToSend, bytesToSend.Length);
        }

        ~Client() {
            udpClient.Close();
        }
    }
}
