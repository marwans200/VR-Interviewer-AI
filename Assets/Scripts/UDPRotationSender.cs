using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UdpRotationSender : MonoBehaviour
{
    public string receiverIP = "192.168.1.100"; // Change this to the receiver's IP
    public int port = 5001;
    private UdpClient udpClient;
    private Quaternion calibrationOffset = Quaternion.identity;
    private bool isCalibrated = false;

    void Start()
    {
        udpClient = new UdpClient();
        Input.gyro.enabled = true;
        Debug.Log("[Sender] Gyroscope enabled.");
        InvokeRepeating(nameof(SendRotation), 0, 0.05f); // Send data every 50ms
    }

    void SendRotation()
    {
        Quaternion rawRotation = Input.gyro.attitude;

        // Adjust for Unity's coordinate system
        rawRotation = new Quaternion(rawRotation.x, rawRotation.y, -rawRotation.z, -rawRotation.w);

        rawRotation = Quaternion.Euler(0, 0, -90) * rawRotation;

        // Apply calibration offset
        if (isCalibrated)
        {
            rawRotation = Quaternion.Inverse(calibrationOffset) * rawRotation;
        }

        string message = $"{rawRotation.x},{rawRotation.y},{rawRotation.z},{rawRotation.w}";
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, receiverIP, port);
        Debug.Log($"[Sender] Sent rotation: {message}");
    }

    public void Calibrate()
    {
        calibrationOffset = Input.gyro.attitude;
        calibrationOffset = new Quaternion(calibrationOffset.x, calibrationOffset.y, -calibrationOffset.z, -calibrationOffset.w);
        isCalibrated = true;
        Debug.Log("[Sender] Calibration set!");
    }

    void OnApplicationQuit()
    {
        udpClient.Close();
    }
}
