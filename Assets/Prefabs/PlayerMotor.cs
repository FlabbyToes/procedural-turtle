 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class PlayerMotor : MonoBehaviour
{
    public enum State
    {
        STATE_STANDING,
        STATE_WALKING,
        STATE_CROCHING,
        STATE_CRAWLING,
        STATE_SPRINTING
    };
    private State playerState = State.STATE_STANDING;
    public State PlayerState { get { return playerState; } }
/*
    public delegate void PlayerStateChanged();
    public static event PlayerStateChanged OnStateChanged;*/

    private CharacterController controller;
    private PlayerLook looker;
    private PlayerStamina stamina;
    private Vector3 playerVelocity;
    public Vector3 PlayerVelocity
    {
        get { return playerVelocity; }
        private set { playerVelocity = value; }
    }
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;

    private bool isCrawling;
    private float normalHeight;
    private float normalStepOffeset;

    private bool isAutoCrouched;

    [SerializeField] private float gravity = -19.6f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float airMultiplier = .25f; 
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpDrain = 10f;
    [SerializeField] private float jumpSpeedBoost = 2f;
    [SerializeField] private float sprintMult = 1.6f;
    [SerializeField] private float sprintDrain = 10f;
    [SerializeField] private float crouchMult = .4f;
    [SerializeField] private float crouchHeight = 1.2f;
    [SerializeField] private float crouchSpeed = 10f;
    [SerializeField] private float deceleration = 1f;

    private Vector3 preJumpMomentum;
    private bool justJumped;
    /*public delegate void JumpLanded();
    public static event JumpLanded OnJumpLanded;*/

    [SerializeField] private float crawlMult = .4f;
    [SerializeField] float crawlHeight = .4f;



    [SerializeField] private int playerLayer = 6;
    [SerializeField] private int ignoreRaycastLayer = 2;
    [SerializeField] private int playerSoundLayer = 8;
    private int layerMask;


    // Start is called before the first frame update
    void Awake()
    {
        justJumped = false;
        controller = GetComponent<CharacterController>();
        normalHeight = controller.height;
        normalStepOffeset = controller.stepOffset;
        looker = GetComponent<PlayerLook>();

        stamina = GetComponent<PlayerStamina>();
        layerMask = ~((1 << playerLayer) | (1 << playerSoundLayer)); //Exclude layers
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded)
        {
            if (justJumped)
            {
                justJumped = false;
                /*OnJumpLanded();*/
            }
            Ground();
        }
        else
        {
            Air();
        }
    }

    //recieve the inputs for our InputManager.cs and apply them to our Character Controller.
    public void ProcessMove(Vector2 input){


        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;

        float newHeight = normalHeight;
        //determine movement
        Vector3 movement = Vector3.zero;
        float newSpeed = speed;
        if (!isGrounded)
        {
            newSpeed = speed * airMultiplier;
        }
        else
        {
            preJumpMomentum = Vector3.zero;
        }
        if (input.x == 0 && input.y == 0)
        {
            if (playerState != State.STATE_STANDING)
            {
                playerState = State.STATE_STANDING;
                /*OnStateChanged();*/
            }
            stamina.staminaRegening();
            //not walking
        }
        else if (isCrawling || isBlockedCrawl())
        {
            if (playerState != State.STATE_CRAWLING)
            {
                playerState = State.STATE_CRAWLING;
                /*OnStateChanged();*/
            }
            stamina.staminaRegening();
            newHeight = crawlHeight;
            movement = transform.TransformDirection(moveDirection) * newSpeed * crawlMult;
            
        }
        else if (isCrouching || isBlockedCrouch())
        {
            if (playerState != State.STATE_CROCHING)
            {
                playerState = State.STATE_CROCHING;
                /*OnStateChanged();*/
            }
            stamina.staminaRegening();
            newHeight = crouchHeight;
            movement = transform.TransformDirection(moveDirection) * newSpeed * crouchMult;
        }
        else if (isSprinting && !stamina.IsTired)
        {
            if (playerState != State.STATE_SPRINTING)
            {
                playerState = State.STATE_SPRINTING;

                /*OnStateChanged();*/
            }
            movement = transform.TransformDirection(moveDirection) * newSpeed * sprintMult;
            stamina.staminaDrainingFactor(sprintDrain);
            
        }
        else
        {
            if (playerState != State.STATE_WALKING)
            {
                playerState = State.STATE_WALKING;
                /*OnStateChanged();*/
            }
            stamina.staminaRegening();
            movement = transform.TransformDirection(moveDirection) * newSpeed;
        }
        playerVelocity.x = movement.x + preJumpMomentum.x;
        playerVelocity.z = movement.z + preJumpMomentum.z;


        //play walk animation if walking
        
        





        //change cam height
        float camX = looker.cam.transform.position.x;
        float camZ = looker.cam.transform.position.z;
        float newCamHeight = Mathf.Lerp(looker.cam.transform.position.y, looker.cam.transform.position.y - (controller.height - newHeight), crouchSpeed * Time.deltaTime);
        looker.cam.transform.position = new Vector3(camX, newCamHeight, camZ);

        //change player height
        //hitbox
        controller.height = Mathf.Lerp(controller.height, newHeight, crouchSpeed * Time.deltaTime);
        controller.center = Vector3.up * controller.height / 2.0f;

        //headhit
        if (Physics.CheckCapsule(transform.position + new Vector3(0, controller.radius * .8f, 0),
            transform.position + new Vector3(0f, controller.height * 1.1f - controller.radius * .8f, 0f), controller.radius * .8f,
            layerMask))
        {
            playerVelocity.y = -2f;
        }
        

        //stick to the ground
        playerVelocity.y += gravity * Time.deltaTime;
        if(isGrounded && playerVelocity.y < 0){
            playerVelocity.y = -2f;
        }
        controller.Move(playerVelocity * Time.deltaTime);
        if (isGrounded)
        {
            preJumpMomentum = playerVelocity;
        }
    }
    public void Jump(){
        if(isGrounded && !stamina.IsTired)
        {
            stamina.staminaDrainingFlat(jumpDrain);
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
            justJumped = true;
        }
    }
    public void Sprint()
    {
        isSprinting = true;
    }
    public void Unsprint()
    {
        isSprinting = false;
    }

    public void Crouch()
    {
        isCrouching = true;
    }
    public void Uncrouch()
    {
        isCrouching = false;
    }

    public void Air()
    {
    }
    public void Ground()
    {
    }

    public void Crawl()
    {
        isCrawling = true;
        //animator.CrouchAnimation(true);
    }
    public void Uncrawl()
    {
        isCrawling = false;
        //animator.CrouchAnimation(false);
    }

    private bool isBlockedCrouch()
    {
        return Physics.CheckCapsule(transform.position + new Vector3(0, controller.radius * .8f, 0),
            transform.position + new Vector3(0f, normalHeight - controller.radius * .8f, 0f), controller.radius * .5f,
            layerMask, QueryTriggerInteraction.Ignore);
    }
    private bool isBlockedCrawl()
    {
        return Physics.CheckCapsule(transform.position + new Vector3(0, controller.radius * .8f, 0),
            transform.position + new Vector3(0f, crouchHeight - controller.radius * .8f, 0f), controller.radius * .5f,
            layerMask, QueryTriggerInteraction.Ignore);
    }

}
