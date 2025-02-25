using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using UnityEngine.UI;

public class UdpReceiver : MonoBehaviour
{
    public int port = 5000; // Match with sender
    private UdpClient udpClient;
    private Texture2D receivedTexture;
    private Thread receiveThread;
    public RawImage displayImage;
    public RawImage displayImageR;
    private byte[] receivedData; // Store received data

    void Start()
    {
        Debug.Log("[Receiver] Initializing UDP Receiver...");

        receivedTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        if (displayImage == null)
        {
            Debug.LogError("[Receiver] RawImage not found! Make sure it's named 'ReceivedImage'.");
            return;
        }

        udpClient = new UdpClient(port);
        Debug.Log($"[Receiver] Listening on port {port}...");

        receiveThread = new Thread(ReceiveData) { IsBackground = true };
        receiveThread.Start();

        // Start coroutine to update UI
        StartCoroutine(UpdateImage());
    }

    void ReceiveData()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
        while (true)
        {
            try
            {
                Debug.Log("[Receiver] Waiting for data...");
                byte[] data = udpClient.Receive(ref endPoint);
                Debug.Log($"[Receiver] Received {data.Length} bytes from {endPoint.Address}");

                receivedData = data; // Store the latest data
            }
            catch (SocketException ex)
            {
                Debug.LogError("[Receiver] UDP Receive Error: " + ex.Message);
            }
        }
    }

    IEnumerator UpdateImage()
    {
        while (true)
        {
            if (receivedData != null && receivedData.Length > 0)
            {
                receivedTexture.LoadImage(receivedData);
                displayImage.texture = displayImageR.texture = receivedTexture;
                Debug.Log("[Receiver] Image updated on UI.");
                receivedData = null; // Clear the buffer
            }
            yield return new WaitForSeconds(0.05f); // 20 FPS update
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("[Receiver] Closing UDP connection...");
        receiveThread.Abort();
        udpClient.Close();
    }
}
