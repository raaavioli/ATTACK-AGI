using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using System.Threading;

public class Server
{

    int port;
    IPAddress ipAddress;
    TcpListener server;
    TcpClient client;
    bool alive;
    public Server(IPAddress ip, int portNumber)
    {
        
        port = portNumber;
        ipAddress = ip;
        alive = true;
        Thread.CurrentThread.IsBackground = true;

        //InitializeServer();
    }


    public void InitializeServer()
    {
        try
        {
            //Debug.Log("1");
            RawImageVisualizer.Program.sw.WriteLine("1");
            server = new TcpListener(ipAddress, port);
            //Debug.Log("2");
            RawImageVisualizer.Program.sw.WriteLine("2");
            server.Start();
            //Debug.Log("ServerListener Started!");
            RawImageVisualizer.Program.sw.WriteLine("ServerListener Started!");
            while (SocketThreadManager.Alive)
            {
                while (!server.Pending())
                {
                    if (!SocketThreadManager.Alive)
                    {
                        break;
                    }
                    Thread.Sleep(10);
                }
                if (server.Pending())
                {
                    new Thread(() =>
                    {
                        Interlocked.Increment(ref SocketThreadManager.ActiveConnections);
                        //Debug.Log("ServerThread created for new client.");
                        RawImageVisualizer.Program.sw.WriteLine("ServerThread created for new client.");
                        Thread.CurrentThread.IsBackground = true;
                        ServerThread t = new ServerThread(server.AcceptTcpClient());
                        t.server_loop();//enter main server loop
                        // print("ServerThread for client is dead");
                        RawImageVisualizer.Program.sw.WriteLine("ServerThread for client is dead");
                        Interlocked.Decrement(ref SocketThreadManager.ActiveConnections);

                    }).Start();
                    Thread.Sleep(100);
                }

            }
            CloseServer();
        }
        catch (SocketException e)
        {
            RawImageVisualizer.Program.sw.WriteLine("Cannot start server on IPAddress: " + ipAddress.ToString() + " on port " + port);
            RawImageVisualizer.Program.sw.WriteLine(e);
            //Debug.Log("Cannot start server on IPAddress: " + ipAddress.ToString() + " on port " + port);
            //Debug.Log(e.ToString());
        }
    }

    void CloseServer()
    {
        server.Stop();
        Thread.Sleep(1000);
        //Debug.Log("ServerListener on IPAddress: " + ipAddress.ToString() + " on port " + port + " shut down successfully.");
        RawImageVisualizer.Program.sw.WriteLine("ServerListener on IPAddress: " + ipAddress.ToString() + " on port " + port + " shut down successfully.");
    }


}