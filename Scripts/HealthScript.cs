using System;
using System.Collections;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.AI;

public class HealthScript : MonoBehaviour
{

    public bool isPlayer,
                isCannibal,
                isBoar;
    public float health;
    private EnemyAnimationScript enemyAnim;
    [SerializeField] PlayerNetwork player;
    private NavMeshAgent navAgent;
    private EnemyScript enemy;
    private bool isDead;
    private EnemyState enemyState;
    private EnemyAudio enemyAudio;
    
    void Awake()
    {
               
        if(isBoar || isCannibal)
        {
            enemyAudio = GetComponentInChildren<EnemyAudio>();
            enemy = GetComponent<EnemyScript>();
            enemyAnim = GetComponent<EnemyAnimationScript>();
            navAgent = GetComponent<NavMeshAgent>();
        }
        if (isPlayer)
        {
            
        }
    }
    public void ApplyDamage(float damage)
    {
        if (isDead)
        {
            return;
        }
        health -= damage;
        if (isPlayer)
        {           
            SendHealth();
        }
        if(isBoar || isCannibal)
        {
            if(enemyState == EnemyState.Idle)
            {
                enemy.chaseDistance = 50f;
            }
        }
        if (health <= 0)
        {
            Death();
        }
        
    }
   
   public void Death()
    {
        isDead = true;
        if (isCannibal)
        {
            gameObject.GetComponent<BoxCollider>().enabled = false;
            StartCoroutine(DieSound());
            enemyAnim.Dead();
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
            enemy.enabled = false;
        }
        if (isBoar)
        {
            StartCoroutine(DieSound());
            enemyAnim.Dead();
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
            enemy.enabled = false;
            
        }
        if (isPlayer)
        {
            player.transform.position = new Vector3(0.289326906f, 5.079f, -1.49843836f);
            RespawnPlayer();
        }
        
        else
        {
            Invoke("DestroyGameObject", 3f);
        }
    }

    private void RespawnPlayer()
    {      
        Message message = Message.Create(MessageSendMode.reliable, ServerToClient.playerRespawn);
        message.AddUShort(player.id);
        NetworkManager.instance.server.SendToAll(message);
    }

    private void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    private void DestroyGameObject()
    {
        gameObject.SetActive(false);
    }
    
    IEnumerator DieSound()
    {
        yield return new WaitForSeconds(0.3f);
        enemyAudio.DieSound();
    }
    private void SendHealth()
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClient.hitRegister);
        message.AddUShort(player.id);
        message.AddFloat(health);
        NetworkManager.instance.server.SendToAll(message);
    }
}
