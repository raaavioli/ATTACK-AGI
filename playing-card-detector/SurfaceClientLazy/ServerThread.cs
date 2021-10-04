using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;
using System.Threading;


/* Each Client is assigned it's own designated Server Thread, as to create
 * the possibility of having multiple clients connected at the same time.
 * This may not be implemented in our project, but we have proper network code
 * now. 
 * 
 * The server will timeout after X amount of time. This variable can be adjusted
 * at will. 
 * 
 */

// covnevrt and reserver process request for -1 and 0 specifically
// Then 
public class ServerThread
{
    TcpClient client;
    NetworkStream stream;
    bool alive;
    private static volatile int _ID = 0;
    int id;

    //IVideoSource VS;

    public ServerThread(TcpClient t)
    {

        id = Interlocked.Increment(ref _ID);

        client = t;
        client.ReceiveTimeout = 10000;
        stream = client.GetStream();
        stream.ReadTimeout = 3000;
        stream.WriteTimeout = 3000;

        alive = true;
    }

    public void server_loop()
    {
        try
        {
            while (SocketThreadManager.Alive && alive)
            {
                int req = GetRequest();
                ////Debug.Log("Req: " + req);
                RawImageVisualizer.Program.sw.WriteLine("Req: " + req);
                if (ValidateRequest(req))
                {
                    // DO RESERVED COMMANDS
                    if (req == -1)
                    {
                        alive = false;
                        break;
                    }
                    else if (req == 0)
                    {
                        // this is just here to test the connection
                        // print("0");
                        RawImageVisualizer.Program.sw.WriteLine("Client pinged us. Hi, clint!");
                    }
                    else
                    {
                        ProcessRequests(req);
                    }
                }
                else
                {
                    RawImageVisualizer.Program.sw.WriteLine("invalid request recieved from client: " + req);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            RawImageVisualizer.Program.sw.WriteLine("Error: ServerThread ecoundered an unexpected error and had to close:");
            RawImageVisualizer.Program.sw.WriteLine(e);
            //Debug.Log("Error: ServerThread ecoundered an unexpected error and had to close:");
            //Debug.Log(e.Message);
        }
        CloseConnection();
    }



    int GetRequest()
    {
        Byte[] data = new Byte[4];
        int byte_count = stream.Read(data, 0, 4);
        if (byte_count == 0)
        {
            RawImageVisualizer.Program.sw.WriteLine("Read 0 Bytes from Client");
            //Debug.Log("Read 0 Bytes from Client");
            return Int32.MinValue;
        }
        int req = BitConverter.ToInt32(data, 0);
        return req;
    }

    /* Recieves a 4 byte int - this being the request number. 
     * Intuitive Switch/Case depending on the request. 
     * USER IS RESPONSIBLE FOR SENDING AFFIRMATION THAT THE REQUEST WAS VALID. 
     * 
     */
    protected virtual bool ProcessRequests(int req)
    {
        //Byte[] data = new Byte[4];
        //int read = stream.Read(data, 0, 4);
        //int req = BitConverter.ToInt32(data, 0);

        //if(read == 0) { return false; }

        switch (req)
        {
            case 1:
                SendVideoRect();
                ////Debug.Log("1");
                break;
            case 2:
                ////Debug.Log("2");
                break;
            case 3:
                ////Debug.Log("3");
                break;
            default:
                //Debug.Log("Invalid Request: " + req);
                return false;
        }
        return true;
    }

    protected virtual bool ValidateRequest(int req)
    {
        if ((req >= -1) && (req < 4))
        {
            return true;
        }
        return false;
    }

    void CloseConnection()
    {
        stream.Close();
        client.Close();
        //Debug.Log("ServerThread with ID=" + id + " closed on Client");
        RawImageVisualizer.Program.sw.WriteLine("ServerThread with ID=" + id + " closed on Client");
    }


    // SERVER SPECIFIC REQUESTS

    void SendVideoRect()
    {
        // This would be the actual FRAME sent. 
        //stream.Write(FRAME, 0, FRAME.Length);
        
        //byte[] data = VS.GetVideoFrame();
        //compress data
        ////Debug.Log("I will compress");
        byte[] data2 = RawImageVisualizer.App1.normalizedImage_compressed == null ? StaticNetworkUtilities.compressFrame_assumeNoZero(RawImageVisualizer.App1.normalizedImage) : RawImageVisualizer.App1.normalizedImage_compressed;
        ////Debug.Log("compression complete");
        /*byte[] tmp = StaticNetworkUtilities.decompressFrame(data2);
        if (tmp.Length != data.Length)
        {
            //Debug.Log("Length Missmatch!");
        } else {
            for (int i = 0; i < tmp.Length; ++i)
            {
                if(tmp[i] != data[i])
                {
                        
                    //Debug.Log("Data missmatch! at: " + i + " expected " + tmp[i] + " got " + data[i]);
                    //Debug.Log("hi");
                    break;
                }
            }
        }*/
        //data = data;
        ////Debug.Log(data);
        //Debug.Log("sending " + data2.Length + " bytes of video data");
        RawImageVisualizer.Program.sw.WriteLine("sending " + data2.Length + " bytes of video data");
        stream.Write(BitConverter.GetBytes(data2.Length), 0, 4);
        stream.Write(data2, 0, data2.Length);
        //Debug.Log("sent");
        RawImageVisualizer.Program.sw.WriteLine("sent");
        

    }

}