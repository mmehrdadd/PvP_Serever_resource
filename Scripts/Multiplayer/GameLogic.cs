using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GameLogic : MonoBehaviour
{
    
    private static GameLogic _instance = null;

    private GameLogic()
    {

    }

    public static GameLogic instance
    {

        get => _instance;
        set
        {
            if (_instance == null)
            {
                _instance = value;
            }
            else if (_instance != value)
            {
                Debug.Log($"{nameof(GameLogic)} has already a value, new value will be deleted");
                Destroy(value);
            }
        }
    }
    private void Awake()
    {
        instance = this;
    }

    public GameObject playerPrefab
    {
        get { return _playerPrefab; }
    }
    [Header("Prefabs")]
    [SerializeField] private GameObject _playerPrefab;

    
    
    
}
