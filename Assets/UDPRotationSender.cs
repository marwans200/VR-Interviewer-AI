using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UdpRotationSender : MonoBehaviour
{
    public string receiverIP = "192.168.1.100"; // Change to receiver's IP
    public int port = 5001;
    private UdpClient udpClient;

    void Start()
    {
        udpClient = new UdpClient();
        Input.gyro.enabled = true;
        Debug.Log("[Sender] Gyroscope enabled.");
        InvokeRepeating(nameof(SendRotation), 0, 0.05f); // Send data every 50ms
    }

    void SendRotation()
    {
        Quaternion rotation = Input.gyro.attitude;

        // Adjust for Unity's coordinate system
        rotation = new Quaternion(rotation.x, rotation.y, -rotation.z, -rotation.w);

        // Correct rotation based on screen orientation
        switch (Screen.orientation)
        {
            case ScreenOrientation.LandscapeLeft:
                rotation = Quaternion.Euler(0, 0, -90) * rotation;
                break;
            case ScreenOrientation.LandscapeRight:
                rotation = Quaternion.Euler(0, 0, 90) * rotation;
                break;
            case ScreenOrientation.PortraitUpsideDown:
                rotation = Quaternion.Euler(0, 0, 180) * rotation;
                break;
            default: // Portrait (default)
                break;
        }

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
