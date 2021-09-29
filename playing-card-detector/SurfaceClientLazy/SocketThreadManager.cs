using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

/*
 *  If Servers/Clients are to be used in the project, this file must be attached
 *  to one of the game objects. Otherwise OnApplicationQuit() will never be called
 *  by the UnityEngine and the game will NOT shutdown properly (thread/socket safety).
 */
public class SocketThreadManager
{

    //fuckit
    //static IVideoSource VS;
    


    // This disgusting variable is to tell the Server and it's threads
    // That the program is alive or dead. This is needed so that each 
    // thread separately shutsdown and that each socket is closed so that 
    // we don't have to restart the debugger every time.
    // If you want a more detailed explanation on why it has to be this way
    // and want to hear a big rant, contact Victor. 



    // THREAD/NETWORK SAFETY
    public static volatile bool Alive;

    // Not currently used but may be good to have in the future
    public static volatile int ActiveConnections;
    public static volatile int ActiveThreads;
    public static volatile bool FrameRecieved;

    static SocketThreadManager()
    {
        Alive = true;
        ActiveConnections = 0;
        ActiveThreads = 0;
    }


    /* Used to create a server on specified IP and Port. 
     * Will notify when it dies too.
     * Future/TODO: Specify name of what thread dies.
     */
    public static void CreateServer(string ip, int port)
    {
        new Thread(() =>
        {
            Interlocked.Increment(ref ActiveThreads);
            Server s = new Server(IPAddress.Parse(ip), port);
            s.InitializeServer();
            RawImageVisualizer.Program.sw.WriteLine("ServerListener Thread is Dead");
            //print("ServerListener Thread is Dead");
            Interlocked.Decrement(ref ActiveThreads);
        }).Start();

        Thread.Sleep(500);
    }

    // Double ensures thread safety.
    // On QUIT we need to set SocketThreadManager.Alive = false;
    // And let the main Thread sleep for a bit so that everything 
    // Has time to shutdown and close properly.
    void OnApplicationQuit()
    {
        SocketThreadManager.Alive = false;
        // Arbitrary time to ensure that sockets close properly.
        int times = 0;
        do
        {
            Thread.Sleep(1500);
        } while (times++ < 6 && ActiveThreads > 0 && ActiveConnections > 0);
        //if (ActiveThreads > 0 && ActiveConnections > 0)
            //print("Quit unsuccessful!");
        //else
            //print("Quit successful!");
    }

}