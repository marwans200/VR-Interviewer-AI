using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.IO;

public class UdpSender : MonoBehaviour
{
    public Camera cam;
    public string receiverIP = "192.168.1.100"; // Replace with receiver's actual IP
    public int port = 5000; // Must match receiver's port
    private UdpClient udpClient;
    private RenderTexture renderTexture;
    private Texture2D tex2D;

    void Start()
    {
        Debug.Log("[Sender] Initializing UDP Sender...");

        udpClient = new UdpClient();
        renderTexture = new RenderTexture(Screen.width, Screen.height, 16);
        tex2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        Debug.Log($"[Sender] Target Receiver: {receiverIP}:{port}");
        InvokeRepeating(nameof(CaptureAndSend), 1f, 0.05f); // 20 FPS (Adjustable)
    }

    void CaptureAndSend()
    {
        Debug.Log("[Sender] Capturing frame...");

        // Capture frame from camera
        cam.targetTexture = renderTexture;
        cam.Render();
        RenderTexture.active = renderTexture;
        tex2D.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex2D.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;

        // Convert to JPG
        byte[] imageBytes = tex2D.EncodeToJPG(50); // Compression to reduce size
        Debug.Log($"[Sender] Captured frame size: {imageBytes.Length / 1024} KB");

        // Send via UDP
        SendData(imageBytes);
    }

    void SendData(byte[] data)
    {
        try
        {
            Debug.Log($"[Sender] Sending {data.Length} bytes to {receiverIP}:{port}...");
            udpClient.Send(data, data.Length, receiverIP, port);
            Debug.Log("[Sender] Data sent successfully.");
        }
        catch (SocketException ex)
        {
            Debug.LogError("[Sender] UDP Send Error: " + ex.Message);
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("[Sender] Closing UDP connection...");
        udpClient.Close();
    }
}
