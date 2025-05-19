using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System;

public class UdpSender : MonoBehaviour
{
    public string receiverIP = "192.168.1.100"; // Set receiver's IP
    public int port = 5002; // UDP port
    public int width = 1280; // Adjust resolution (1920x1080 for Full HD)
    public int height = 720;
    public bool useJpeg = true; // Toggle between PNG and JPEG

    private UdpClient udpClient;
    private RenderTexture renderTexture;
    private Texture2D tex;

    void Start()
    {
        udpClient = new UdpClient();
        renderTexture = new RenderTexture(width, height, 16);
        tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        Camera.main.targetTexture = renderTexture;
        InvokeRepeating(nameof(SendImage), 0, 0.1f); // Send every 100ms (~10 FPS)
    }

    void SendImage()
    {
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        byte[] imageBytes;
        if (useJpeg)
            imageBytes = tex.EncodeToJPG(90); // JPEG compression (90% quality)
        else
            imageBytes = tex.EncodeToPNG(); // PNG (larger but lossless)

        byte[] lengthPrefix = BitConverter.GetBytes(imageBytes.Length);
        byte[] sendData = new byte[lengthPrefix.Length + imageBytes.Length];
        lengthPrefix.CopyTo(sendData, 0);
        imageBytes.CopyTo(sendData, lengthPrefix.Length);

        udpClient.Send(sendData, sendData.Length, receiverIP, port);
        Debug.Log($"[Sender] Sent image ({imageBytes.Length} bytes)");
    }

    void OnApplicationQuit()
    {
        udpClient.Close();
        Camera.main.targetTexture = null;
    }
}
