using UnityEngine;
using Unity.WebRTC;
using System.Collections;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebRTCServer : MonoBehaviour
{
    private RTCPeerConnection peerConnection;
    private RTCDataChannel dataChannel;
    private RenderTexture videoTexture;

    private async void Start()
    {
        Debug.Log("[Server] Initializing WebRTC");
        WebRTC.Initialize();

        peerConnection = new RTCPeerConnection();
        peerConnection.OnIceCandidate = OnIceCandidate;
        peerConnection.OnIceConnectionChange = OnIceConnectionChange;

        dataChannel = peerConnection.CreateDataChannel("data");
        dataChannel.OnMessage = OnDataReceived;

        RTCSessionDescriptionAsyncOperation offerOp = peerConnection.CreateOffer();
        offerOp.OnSuccess += async offer =>
        {
            Debug.Log("[Server] Offer created successfully.");
            RTCSetSessionDescriptionAsyncOperation setLocalOp = peerConnection.SetLocalDescription(ref offer);
            setLocalOp.OnSuccess += () => Debug.Log("[Server] Set local description successfully.");
            setLocalOp.OnFailure += error => Debug.LogError("[Server] Failed to set local description: " + error);
        };
        offerOp.OnFailure += error => Debug.LogError("[Server] Failed to create offer: " + error);
    }

    private void OnIceCandidate(RTCIceCandidate candidate)
    {
        Debug.Log("[Server] New ICE Candidate: " + candidate.Candidate);
    }

    private void OnIceConnectionChange(RTCIceConnectionState state)
    {
        Debug.Log("[Server] ICE Connection State Changed: " + state);
    }

    private void OnDataReceived(byte[] data)
    {
        string message = Encoding.UTF8.GetString(data);
        Debug.Log("[Server] Received Data: " + message);
    }

    private void OnDestroy()
    {
        Debug.Log("[Server] Cleaning up WebRTC");
        peerConnection.Close();
        WebRTC.Dispose();
    }
}
