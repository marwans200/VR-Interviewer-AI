using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UdpRotationReceiver : MonoBehaviour
{
    public int port = 5001; // Must match sender
    private UdpClient udpClient;
    private Thread receiveThread;
    private Quaternion receivedRotation = Quaternion.identity;
    private bool dataReceived = false;
    public Transform RotApplyTransform;
    public Vector3 OffsetRot;

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
                string message = Encoding.UTF8.GetString(data);
                Debug.Log($"[Receiver] Received: {message}");

                string[] values = message.Split(',');
                if (values.Length == 4)
                {
                    float x = float.Parse(values[0]);
                    float y = float.Parse(values[1]);
                    float z = float.Parse(values[2]);
                    float w = float.Parse(values[3]);

                    receivedRotation = new Quaternion(x, y, z, w);
                    dataReceived = true;
                }
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
            receivedRotation *= Quaternion.Euler(OffsetRot);
            RotApplyTransform.rotation = receivedRotation; // Apply rotation
            dataReceived = false;
        }
    }

    void OnApplicationQuit()
    {
        receiveThread.Abort();
        udpClient.Close();
    }
}
