using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace AttemptAtNetcode
{
    public class StartGame: MonoBehaviour
    {
        public GameLift GameLift { get; private set; }

        public void Awake()
        {
            var gameliftObj = GameObject.Find("/GameLiftStatic");
            Debug.Assert(gameliftObj != null);
            GameLift = gameliftObj.GetComponent<GameLift>();
        }
        
        public async void Start()
        {
#if !UNITY_SERVER
            (bool success, ConnectionInfo connectionInfo) = await GameLift.GetConnectionInfo();
            var port = connectionInfo.Port;
            var ip = connectionInfo.IpAddress;
            Debug.Log($"ip: {ip} port: {port}");
            if (ip != null)
            {
                Debug.Log($"Connecting to {ip}:{port}");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, (ushort)port);
            }
            else
            {
                Debug.Log($"Connecting to localhost:{port}");
            }
            
            NetworkManager.Singleton.StartClient();
#else
            var port = GameLift.ServerPort;
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", (ushort)port, "0.0.0.0");
            Debug.Log($"Starting server on port {port}");
            NetworkManager.Singleton.StartServer();
#endif
        }
    }
}