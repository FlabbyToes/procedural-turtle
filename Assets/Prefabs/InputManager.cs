using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.OnFootActions onFoot;
    private PlayerInput.UIActions UI;

    private PlayerMotor motor;
    private PlayerLook look;


    void Awake()
    {
        //Movement and look
        
        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;
        UI = playerInput.UI;
        //inventory = InvUI.GetComponent<PlayerInventory>();
        motor = GetComponent<PlayerMotor>();
        onFoot.Jump.performed += ctx => motor.Jump();
        onFoot.Sprint.performed += ctx => motor.Sprint();
        onFoot.Sprint.canceled += ctx => motor.Unsprint();

        onFoot.Crouch.performed += ctx => motor.Crouch();
        onFoot.Crouch.canceled += ctx => motor.Uncrouch();

        onFoot.Crawl.performed += ctx => motor.Crawl();
        onFoot.Crawl.canceled += ctx => motor.Uncrawl();

        look = GetComponent<PlayerLook>();
        onFoot.Interact.performed += ctx => look.InteractRaycast(true);

        //UI
        //UI.Inventory.performed += ctx => inventory.openInventory();


    }

    // Update is called once per frame
    void Update()
    {

        
        //tell the player mototr to move from our value action
        motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
    }
    void LateUpdate(){
        look.ProcessLook(onFoot.Look.ReadValue<Vector2>());
    }
    private void OnEnable(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        onFoot.Enable();
        UI.Enable();
    }
    private void OnDisable(){
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        onFoot.Disable();
        UI.Disable();
    }
}
