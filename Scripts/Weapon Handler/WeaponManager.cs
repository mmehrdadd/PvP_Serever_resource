using RiptideNetworking;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private PlayerNetwork player;
    [SerializeField] private  WeaponHandler[] weapons;
    public int currentWeapon;
    
    void Start()
    {        
        currentWeapon = 0;
        weapons[currentWeapon].gameObject.SetActive(true);
        
    }

    private void Awake()
    {
        player = this.GetComponent<PlayerNetwork>();
    }

    public void TurnOnSelectedWeapon(int weaponindex)
    {
        if(currentWeapon != weaponindex)
        {
            weapons[currentWeapon].gameObject.SetActive(false);
            weapons[weaponindex].gameObject.SetActive(true);
            currentWeapon = weaponindex;
            SendSelectedWeaponToOtherClients();
            
        }        
    }
    public WeaponHandler GetCurrentWeapon()
    {
        return weapons[currentWeapon];
    }
    private void SendSelectedWeaponToOtherClients()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClient.selectedWeapon);
        message.AddUShort(player.id);        
        message.AddInt(currentWeapon);
        NetworkManager.instance.server.SendToAll(message);
    }
}
