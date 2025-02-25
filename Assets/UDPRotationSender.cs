using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UDPRotationSender : MonoBehaviour
{
    public string receiverIP = "192.168.1.100"; // Change this to your receiver's IP
    public int port = 5001; // Match receiver port
    private UdpClient udpClient;

    void Start()
    {
        udpClient = new UdpClient();
        Input.gyro.enabled = true; // Enable gyroscope
        Debug.Log("[Sender] Gyroscope enabled.");
        InvokeRepeating(nameof(SendRotation), 0, 0.05f); // Send data every 50ms (20 FPS)
    }

    void SendRotation()
    {
        Quaternion rotation = Input.gyro.attitude; // Get phone rotation
        rotation = new Quaternion(rotation.x, rotation.y, -rotation.z, -rotation.w); // Adjust for Unity's coordinate system

        string message = $"{rotation.x},{rotation.y},{rotation.z},{rotation.w}";
        byte[] data = Encoding.UTF8.GetBytes(message);

        udpClient.Send(data, data.Length, receiverIP, port);
        Debug.Log($"[Sender] Sent rotation: {message}");
    }

    void OnApplicationQuit()
    {
        udpClient.Close();
    }
}
