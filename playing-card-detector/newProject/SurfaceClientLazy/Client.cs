using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace SurfaceClient {
    class Client {

        private TcpClient tcpClient = new TcpClient();
        private DateTime lastTime = DateTime.Now;

        private const float TIME_THRESHOLD = 100;

        public Client() {
            tcpClient.Connect("127.0.0.1", 50001);
        }

        public void Process(byte[] image) {
            TimeSpan timeDifference = DateTime.Now - lastTime;
            if (timeDifference.TotalMilliseconds > TIME_THRESHOLD) {
                lastTime = DateTime.Now;
                try {
                    tcpClient.GetStream().Write(image, 0, image.Length);
                } catch (IOException e) {
                    tcpClient.Close();
                }
            }
        }

        ~Client() {
            tcpClient.Close();
        }
    }
}
