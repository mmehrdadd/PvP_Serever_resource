using RiptideNetworking;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    
    [SerializeField] private PlayerNetwork player;
    [SerializeField] private Transform camProxy;
    [SerializeField] private Transform _lookRoot;
    private CharacterController characterController;
    private PlayerAudio _playerAudio;
    private Vector3 moveDirection;
    private float moveH, moveV;
    public float speed;
    public float jumpForce = 10f;
    public float gravity = 25f;
    private float verticalVelocity;
    private bool[] inputs;

    public float move_Speed = 5f;
    public float sprint_Speed = 8f;
    public float crouch_Speed = 2f;
    public float stand_Height = 1f;
    public float crouch_Height = 0.8f;

    private bool _isCrouched;
    private float _sprintVolume = 1f,
                  _crouchVolume = 0.1f,
                  walk_Min = 0.3f, walk_Max = 0.7f;
    private float _walkStepDistance = 0.4f,
                  _sprintStepDistance = 0.25f,
                  _crouchStepDistance = 0.6f;
    private float _sprintValue = 100f;
    private float _sprintTreshold = 10f;
    void Awake()
    {        
        characterController = GetComponent<CharacterController>();
        player = GetComponent<PlayerNetwork>();
        _playerAudio = transform.Find("FootStep_Sound").GetComponent<PlayerAudio>();
    }
    
    private void Start()
    {
        _isCrouched = false;
        speed = move_Speed;
        inputs = new bool[10];
    }
    public void SetInput(bool[] inputs, Vector3 forward)
    {     
        this.inputs = inputs;
        camProxy.forward = forward;
    }
    void Update()
    {
        Vector3 inputValue = Vector3.zero;
        
        if (inputs[0])
        {            
            inputValue.z += 1f;           
        }
        if (inputs[1])
        {            
            inputValue.z -= 1f;
        }
        if(inputs[2])
        {            
            inputValue.x -= 1f;
        }
        if (inputs[3])
        {           
            inputValue.x += 1f;
        }
        
        PlayerMove(inputValue);
    }

    public void PlayerMove(Vector3 inputValues)
    {
        moveDirection = Vector3.Normalize(camProxy.right * inputValues.x + camProxy.forward * inputValues.z);
        if (_sprintValue > 0f)
        {
            if (inputs[4] & !_isCrouched)
            {               
                speed = sprint_Speed;
                _playerAudio.step_Distance = _sprintStepDistance;
                _playerAudio.min_Sound = _sprintVolume;
                _playerAudio.max_Sound = _sprintVolume;
            }
            if (!inputs[4] && !_isCrouched)
            {
                speed = move_Speed;
                _playerAudio.step_Distance = _walkStepDistance;
                _playerAudio.min_Sound = walk_Min;
                _playerAudio.max_Sound = walk_Max;
            }
        }
        if (inputs[4] && !_isCrouched)
        {
            _sprintValue -= Time.deltaTime * _sprintTreshold;
            
            if (_sprintValue <= 0f)
            {
                _sprintValue = 0f;
                speed = move_Speed;
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
            }
            if (_sprintValue > 100f)
                _sprintValue = 100f;
        }

        moveDirection *= speed * Time.deltaTime;        
        ApplyGravity();
        Crouch();
        characterController.Move(moveDirection);
        SendMovement();
    }

    public void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
            PlayerJump();
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        moveDirection.y = verticalVelocity*Time.deltaTime;
    }

    public void PlayerJump()
    {
        if (inputs[8])
        {
            verticalVelocity = jumpForce;
        }
    }

    void Crouch()
    {
        if (inputs[9])
        {
            if (_isCrouched)
            {
                _lookRoot.localPosition = new Vector3(_lookRoot.localPosition.x, stand_Height, _lookRoot.localPosition.z);
                characterController.height = stand_Height;
                speed = move_Speed;
                _playerAudio.step_Distance = _walkStepDistance;
                _playerAudio.min_Sound = walk_Min;
                _playerAudio.max_Sound = walk_Max;
                _isCrouched = false;
            }
            else
            {
                _lookRoot.localPosition = new Vector3(_lookRoot.localPosition.x, crouch_Height, _lookRoot.localPosition.z);
                characterController.height = crouch_Height;
                speed = crouch_Speed;
                _playerAudio.step_Distance = _crouchStepDistance;
                _playerAudio.min_Sound = _crouchVolume;
                _playerAudio.max_Sound = _crouchVolume;
                _isCrouched = true;
            }
        }
    }

    private void SendMovement()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClient.playerMovement);
        message.AddUShort(player.id);
        message.AddVector3(transform.position);
        message.AddVector3(camProxy.forward);
        message.AddVector3(_lookRoot.localPosition);
        message.AddFloat(characterController.height);
        NetworkManager.instance.server.SendToAll(message);
    }

}
