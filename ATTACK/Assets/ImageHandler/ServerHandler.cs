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
    public byte[] data {
        get { rdyToFetchData = false; return data; }
        private set { data = value; }
    }
    public bool rdyToFetchData { get; private set; } = false;

    IPEndPoint ipep;
    UdpClient newSock;


    void Awake()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(this);
        }

        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50001);
        newSock = new UdpClient(ipep);
    }

    void Start()
    {
        newSock.BeginReceive(ASyncReceiver, null);
    }

    public void ASyncReceiver(IAsyncResult result)
    {
        data = newSock.EndReceive(result, ref ipep);
        rdyToFetchData = true;
        newSock.BeginReceive(ASyncReceiver, null);
    }
}