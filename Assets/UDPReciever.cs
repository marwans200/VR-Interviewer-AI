using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System;

public class UdpReceiver : MonoBehaviour
{
    public int port = 5002;
    public RawImage displayImage;
    public RawImage RdisplayImage;
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool dataReceived = false;
    private byte[] receivedData;

    void Start()
    {
        udpClient = new UdpClient(port);
        receiveThread = new Thread(ReceiveData) { IsBackground = true };
        receiveThread.Start();
        Debug.Log($"[Receiver] Listening on port {port}...");
    }

    void ReceiveData()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
        while (true)
        {
            try
            {
                byte[] data = udpClient.Receive(ref endPoint);
                Debug.Log($"[Receiver] Received {data.Length} bytes");

                int length = BitConverter.ToInt32(data, 0);
                receivedData = new byte[length];
                System.Array.Copy(data, 4, receivedData, 0, length);
                dataReceived = true;
            }
            catch (SocketException ex)
            {
                Debug.LogError("[Receiver] UDP Receive Error: " + ex.Message);
            }
        }
    }

    void Update()
    {
        if (dataReceived)
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(receivedData);
            displayImage.texture = tex;
            RdisplayImage.texture = tex;
            dataReceived = false;
        }
    }

    void OnApplicationQuit()
    {
        receiveThread.Abort();
        udpClient.Close();
    }
}
