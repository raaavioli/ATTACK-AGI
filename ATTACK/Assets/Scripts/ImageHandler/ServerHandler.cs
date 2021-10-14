using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine.Events;
using static GameManager;

public class ServerHandler : MonoBehaviour {
    public static string mostRecentCardInfo { get; private set; }

    private byte[] data;
    private EndPoint senderRemote;
    private ServerHandler instance;
    private Socket socket;

    void Awake() {
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
        socket.Blocking = false;
    }

    void Update() {
        if (socket.Available > 0) {
            socket.ReceiveFrom(data, ref senderRemote);
            string cards = Encoding.ASCII.GetString(data);
            Debug.Log(cards);
            mostRecentCardInfo = cards;
        }
    }

    private void OnDestroy() {
        socket.Close();
    }
}