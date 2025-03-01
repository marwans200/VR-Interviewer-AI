using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class HandReceiver : MonoBehaviour
{
    TcpClient client;
    NetworkStream stream;

    public GameObject leftHandObject;  // Assign in Unity Inspector
    public Vector3 leftOffset;
    public GameObject rightHandObject; // Assign in Unity Inspector
    public Vector3 rightOffset;

    public float positionMultiplier = 0.01f;
    public float rotationMultiplier = 0.01f;  // Adjust if needed

    private Vector3 lastLeftPos = Vector3.zero;
    private Vector3 lastRightPos = Vector3.zero;
    private Quaternion lastLeftRot = Quaternion.identity;
    private Quaternion lastRightRot = Quaternion.identity;

    void Start()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 65432); // Connect to Python
            stream = client.GetStream();
            Debug.Log("Connected to Python!");
        }
        catch (Exception e)
        {
            Debug.LogError("Connection error: " + e.Message);
        }
    }

    void Update()
    {
        if (stream != null && stream.DataAvailable)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Debug.Log("Received: " + data);

            HandData handPositions = JsonUtility.FromJson<HandData>(data);

            // Update Left Hand
            if (handPositions.left != null)
            {
                Vector3 newPos = (new Vector3(handPositions.left.x, handPositions.left.y, 0) * positionMultiplier) + leftOffset;
                lastLeftPos = newPos;
                leftHandObject.transform.position = newPos;

                Quaternion newRot = Quaternion.Euler(handPositions.left.roll * rotationMultiplier,
                                                     handPositions.left.yaw * rotationMultiplier,
                                                     handPositions.left.pitch * rotationMultiplier);
                lastLeftRot = newRot;
                leftHandObject.transform.rotation = newRot;
            }
            else
            {
                // Keep last known position and rotation
                leftHandObject.transform.position = lastLeftPos;
                leftHandObject.transform.rotation = lastLeftRot;
            }

            // Update Right Hand
            if (handPositions.right != null)
            {
                Vector3 newPos = (new Vector3(handPositions.right.x, handPositions.right.y, 0) * positionMultiplier) + rightOffset;
                lastRightPos = newPos;
                rightHandObject.transform.position = newPos;

                Quaternion newRot = Quaternion.Euler(handPositions.right.roll * rotationMultiplier,
                                                     handPositions.right.yaw * rotationMultiplier,
                                                     handPositions.right.pitch * rotationMultiplier);
                lastRightRot = newRot;
                rightHandObject.transform.rotation = newRot;
            }
            else
            {
                // Keep last known position and rotation
                rightHandObject.transform.position = lastRightPos;
                rightHandObject.transform.rotation = lastRightRot;
            }
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }

    [Serializable]
    public class HandData
    {
        public Hand left;
        public Hand right;
    }

    [Serializable]
    public class Hand
    {
        public float x;
        public float y;
        public float yaw;
        public float pitch;
        public float roll;
    }
}
