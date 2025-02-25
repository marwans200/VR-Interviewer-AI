using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;
using System.Text;

public class MobileNetworkManager : MonoBehaviour
{
    public RawImage displayImage;
    private Texture2D texture;
    private UdpClient udpClient;
    private UdpClient udpReceiver;
    private int receivePort = 5000; // Receiving video from PC
    private int sendPort = 5001; // Sending input to PC
    public string pcIP = "192.168.1.72"; // Replace with your PC's local IP

    private void Start()
    {
        udpClient = new UdpClient();
        udpReceiver = new UdpClient(receivePort);
        texture = new Texture2D(256, 256, TextureFormat.RGB24, false);

        // Start listening for incoming frames
        Thread receiveThread = new Thread(ReceiveFrame);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        // Start sending input
        InvokeRepeating(nameof(SendInput), 1, 0.2f); // Send data every 0.2s
    }

    private void ReceiveFrame()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, receivePort);

        while (true)
        {
            byte[] imageData = udpReceiver.Receive(ref remoteEP);

            if (imageData.Length > 0)
            {
                texture.LoadImage(imageData);
                texture.Apply();
            }
        }
    }

    private void SendInput()
    {
        string inputData = "Touch: " + Input.touchCount;
        byte[] sendBytes = Encoding.UTF8.GetBytes(inputData);

        IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(pcIP), sendPort);
        udpClient.Send(sendBytes, sendBytes.Length, endpoint);
    }

    private void Update()
    {
        if (texture != null)
        {
            displayImage.texture = texture;
        }
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
        udpReceiver.Close();
    }
}
