using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class PCNetworkManager : MonoBehaviour
{
    public Camera cam; // Assign the camera
    private UdpClient udpClient;
    private UdpClient udpReceiver;
    public int sendPort = 5000; // Sending video
    public int receivePort = 5001; // Receiving input
    public string mobileIP = "192.168.1.71"; // Replace with your mobile's local IP

    private void Start()
    {
        udpClient = new UdpClient();
        udpReceiver = new UdpClient(receivePort);

        // Start listening for mobile data
        Thread receiveThread = new Thread(ReceiveMobileData);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        // Start sending frames
        InvokeRepeating(nameof(SendFrame), 0, 0.05f); // 20 FPS
    }

    private void SendFrame()
    {
        // Capture the camera view
        RenderTexture renderTexture = new RenderTexture(256, 256, 16);
        cam.targetTexture = renderTexture;
        cam.Render();

        RenderTexture.active = renderTexture;
        Texture2D image = new Texture2D(256, 256, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        image.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);

        // Convert image to JPEG
        byte[] imageData = image.EncodeToJPG();

        // Send the image data via UDP
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(mobileIP), sendPort);
        udpClient.Send(imageData, imageData.Length, endpoint);
    }

    private void ReceiveMobileData()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, receivePort);

        while (true)
        {
            byte[] receivedData = udpReceiver.Receive(ref remoteEP);
            string receivedMessage = Encoding.UTF8.GetString(receivedData);

            Debug.Log("Received from Mobile: " + receivedMessage);
        }
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
        udpReceiver.Close();
    }
}
