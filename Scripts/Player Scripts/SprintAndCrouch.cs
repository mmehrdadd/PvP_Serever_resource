using RiptideNetworking;
using UnityEngine;

public class SprintAndCrouch : MonoBehaviour
{
    public float move_Speed = 5f;
    public float sprint_Speed = 8f;
    public float crouch_Speed = 2f;
    public float stand_Height = 1f;
    public float crouch_Height = 0.8f;

    [SerializeField] private CharacterController _characterController;
    private PlayerAudio _playerAudio;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private Transform _lookRoot;
    private bool _isCrouched = false;
    private bool[] inputs;
    private float _sprintVolume = 1f,
                  _crouchVolume = 0.1f,
                  walk_Min = 0.3f, walk_Max = 0.7f;
    private float _walkStepDistance = 0.4f,
                  _sprintStepDistance = 0.25f,
                  _crouchStepDistance = 0.6f;
    private float _sprintValue = 100f;
    private float _sprintTreshold = 10f;
    
    [SerializeField] private PlayerNetwork _player;
    void Awake()
    {
        _playerAudio = transform.GetChild(1).GetComponent<PlayerAudio>();
    }
    void Start()
    {
        inputs = new bool[10];
        //_playerStats = GetComponent<PlayerUI>();
        _playerAudio.max_Sound = walk_Max;
        _playerAudio.min_Sound = walk_Min;
        _playerAudio.step_Distance = _walkStepDistance;
    }
    public void SetInputs(bool[] inputs, Vector3 vector3)
    {
        Debug.Log("we are setting inputs");
        //this.inputs = inputs;
    }
    void Update()
    {
        //Sprint();
        //Crouch();
        //SendValues();
    }
    
    public void Sprint()
    {
        if (_sprintValue > 0f)
        {
            if (inputs[4] & !_isCrouched)
            {
                Debug.Log("we are running");
                _playerMovement.speed = sprint_Speed;               
                _playerAudio.step_Distance = _sprintStepDistance;
                _playerAudio.min_Sound = _sprintVolume;
                _playerAudio.max_Sound = _sprintVolume;
            }
            if (!inputs[4] && !_isCrouched)
            {
                _playerMovement.speed = move_Speed;
                _playerAudio.step_Distance = _walkStepDistance;
                _playerAudio.min_Sound = walk_Min;
                _playerAudio.max_Sound = walk_Max;
            }
        }
        if (inputs[4] && !_isCrouched)
        {
            _sprintValue -= Time.deltaTime * _sprintTreshold;
            //_playerStats.UpdateStaminaBar(_sprintValue);
            if (_sprintValue <= 0f)
            {
                _sprintValue = 0f;
                _playerMovement.speed = move_Speed;
                _playerAudio.min_Sound = walk_Min;
                _playerAudio.max_Sound = walk_Max;
                _playerAudio.step_Distance = _walkStepDistance;
            }            
        }
        else
        {
            if (_sprintValue != 100f)
            {
                _sprintValue += Time.deltaTime * (_sprintTreshold / 2f);
                //_playerStats.UpdateStaminaBar(_sprintValue);
            }
            if (_sprintValue > 100f)
                _sprintValue = 100f;
        }

    }
    void Crouch()
    {
        if (inputs[9])
        {
            if (_isCrouched)
            {
                _lookRoot.localPosition = new Vector3(_lookRoot.localPosition.x,stand_Height, _lookRoot.localPosition.z);
                _characterController.height = stand_Height;
                _playerMovement.speed = move_Speed;
                _playerAudio.step_Distance = _walkStepDistance;
                _playerAudio.min_Sound = walk_Min;
                _playerAudio.max_Sound = walk_Max;   
                _isCrouched = false;
            }
            else
            {
                _lookRoot.localPosition = new Vector3(_lookRoot.localPosition.x, crouch_Height, _lookRoot.localPosition.z);
                _characterController.height = crouch_Height;
                _playerMovement.speed = crouch_Speed;
                _playerAudio.step_Distance = _crouchStepDistance;
                _playerAudio.min_Sound = _crouchVolume;
                _playerAudio.max_Sound = _crouchVolume;
                _isCrouched = true;
            }
        }       
    }
    public void SendValues()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClient.playerSpeedCrouch);
        message.AddUShort(_player.id);
        message.AddFloat(_playerMovement.speed);
        message.AddFloat(_characterController.height);
        message.AddFloat(_sprintValue);
        message.AddFloat(_playerAudio.step_Distance);
        message.AddFloat(_playerAudio.min_Sound);
        message.AddFloat(_playerAudio.max_Sound);
        NetworkManager.instance.server.SendToAll(message);
    }    
}
