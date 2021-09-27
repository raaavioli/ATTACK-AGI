using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

public class ServerHandler : MonoBehaviour
{
    public static ServerHandler instance;
    private byte[] data;
    public bool rdyToFetchData { get; private set; } = false;

    Socket socket;
    EndPoint senderRemote;

    void Awake()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(this);
        }

        data = new byte[128];

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50002);

        socket = new Socket(endPoint.Address.AddressFamily,
            SocketType.Dgram,
            ProtocolType.Udp);

        // Creates an IPEndPoint to capture the identity of the sending host.
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        senderRemote = sender;

        // Binding is required with ReceiveFrom calls.
        socket.Bind(endPoint);
        Console.WriteLine("Waiting to receive datagrams from client...");
        socket.Blocking = false;
    }

    void Update()
    {
        if (socket.Available > 0) {
            socket.ReceiveFrom(data, ref senderRemote);
            rdyToFetchData = true;
            string str = "";
            foreach (byte piece in data) {
                str += piece.ToString();
			}
            Debug.Log(socket.Available);
            Debug.Log(str);
            Debug.Log(Encoding.ASCII.GetString(data));
		}
    }

    public byte[] fetchData() {
        rdyToFetchData = false;
        return data;
	}

	private void OnDestroy() {
        socket.Close();
	}
}