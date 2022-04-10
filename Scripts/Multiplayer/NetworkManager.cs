using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;
using System.Linq;

public enum ClientToServerId : ushort
{
    name = 1,
    input,
    playerRotation,
    selectedWeapon,
    animations,
    hitRegister,
} 
public enum ServerToClient : ushort
{
   playerSpawned = 1,
   playerMovement,
   selectedWeapon,
   playerSpeedCrouch,
   animation,
   hitRegister,
   playerRespawn,
}


public sealed class NetworkManager : MonoBehaviour
{
    
    
    private static NetworkManager _instance = null;
    
    private NetworkManager()
    {
        
    }

    public static NetworkManager instance
    {

        get => _instance;
        set
        {
            if(_instance == null)
            {
                _instance = value;
            }
            else if(_instance != value)
            {
                Debug.Log($"{nameof(NetworkManager)} has already a value, new value will be deleted");
                Destroy(value);               
            }
        }
        
    }
    public Server server { get; private set; }
    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClient;

    private void Awake()
    {
        _instance = this;
    }
    private void Start()
    {
        Application.targetFrameRate = 60;
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
        server = new Server();
        server.Start(port, maxClient);
        server.ClientDisconnected += PlayerLeft;
    }
    private void FixedUpdate()
    {
        server.Tick();
    }
    private void OnApplicationQuit()
    {
        server.Stop();
            
    }
    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e) 
    {
        Destroy(PlayerNetwork.players[e.Id].gameObject);
    }
    
}
