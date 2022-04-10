using RiptideNetworking;
using UnityEngine;
using System.Collections.Generic;
public class PlayerNetwork : MonoBehaviour
{
    public static Dictionary<ushort, PlayerNetwork> players = new Dictionary<ushort, PlayerNetwork>();

    public ushort id { get; private set; }
    public string username { get; private set; }

    public PlayerMovement playerMovement
    {
        get
        {
            return _PlayerMovement;
        }
    }
    

    [SerializeField] private Transform _lookRoot, _playerRoot;
    [SerializeField] private PlayerMovement _PlayerMovement;
    [SerializeField] private WeaponManager _weaponManager;
    [SerializeField] private PlayerAttack _playerAttack;
    [SerializeField] private HealthScript _healthScript;
    private void OnDestroy()
    {
        players.Remove(id);
    }
   
    public void SetRotation(Quaternion camRotation, Quaternion playerRotation)
    {
        _playerRoot.localRotation = playerRotation;
        _lookRoot.localRotation = camRotation;
         
    }
    public static void Spawn(ushort id, string username)
    {
        foreach (PlayerNetwork otherPlayers in players.Values)
        {
            otherPlayers.SendSpawned(id);
        }
        PlayerNetwork player = Instantiate(GameLogic.instance.playerPrefab, new Vector3(0.289326906f, 5.079f, -1.49843836f), Quaternion.identity).GetComponent<PlayerNetwork>();
        player.gameObject.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.id = id;
        player.username = string.IsNullOrEmpty(username) ? $"Guest{id}" : username;
        player.SendSpawned();
        players.Add(id, player);
    }


    [MessageHandler((ushort)ClientToServerId.name)]
    private static void name(ushort clientId, Message message)
    {
        Spawn(clientId, message.GetString());
    }
    private void SendSpawned()
    {   
        NetworkManager.instance.server.SendToAll(AppSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.playerSpawned))); 
    }
    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.instance.server.Send(AppSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClient.playerSpawned)) , toClientId);
    }
    private Message AppSpawnData(Message message)
    {
        message.AddUShort(id);
        message.AddString(username);
        message.AddVector3(transform.position);
        return message;
    }
    [MessageHandler((ushort)ClientToServerId.input)]
    private static void Input(ushort fromClientId, Message message)
    {
        if(players.TryGetValue(fromClientId, out PlayerNetwork player))
        {         
            player.playerMovement.SetInput(message.GetBools(10), message.GetVector3());                       
        }
    }
   
    [MessageHandler((ushort)ClientToServerId.playerRotation)]
    private static void SetRotation(ushort fromClientId, Message message)
    {
        if (players.TryGetValue(fromClientId, out PlayerNetwork player))            
        {
            player.SetRotation(message.GetQuaternion(), message.GetQuaternion());
            
        }
        
    }
    [MessageHandler((ushort)ClientToServerId.selectedWeapon)]
    private static void SelectWeapon(ushort fromClientID, Message message)
    {
        if(players.TryGetValue(fromClientID, out PlayerNetwork player))
        {
            player._weaponManager.TurnOnSelectedWeapon(message.GetInt());
            
        }
        
    }
    [MessageHandler((ushort)ClientToServerId.animations)]
    private static void Animation(ushort fromClientID, Message message)
    {
        if(players.TryGetValue(fromClientID, out PlayerNetwork player))
        {
            player._playerAttack.PlayShootAnimation();
            //message.AddUShort(player.id);
            //NetworkManager.instance.server.SendToAll(message);
        }
    }
    [MessageHandler((ushort)ClientToServerId.hitRegister)]
    private static void DealDamage(ushort fromClientID,Message message)
    {
        
        if(players.TryGetValue(message.GetUShort(), out PlayerNetwork player))
        {
            Debug.Log($"player {player.id} got shot");
            player._healthScript.ApplyDamage(message.GetFloat());
            
        }
    }
}
