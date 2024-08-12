using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;


interface IInteractable
{
    public void Interact();
    public void InteractHover();
}
public class PlayerLook : MonoBehaviour
{
    public Camera cam;

    private float xRotation = 0f;

    [SerializeField] private float xSensitivity = 30;
    [SerializeField] private float ySensitivity = 30;

    [SerializeField] private float interactRaycastLength = 3;
    [SerializeField] private int playerLayer;

    private float regularFOV;
    [SerializeField] private float chaseFOV;
    [SerializeField] private float changeSpeedFOV;
    private float FOV;

    private int layerMask;

    private void Awake()
    {
        FOV = cam.fieldOfView;
        regularFOV = FOV;
    layerMask = ~((1 << playerLayer));
    }
    public void ProcessLook(Vector2 input){
        float mouseX = input.x;
        float mouseY = input.y;
        //calculate camera rotation for lookin up/down
        xRotation -= (mouseY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        //apply this to camera transform
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        //rotate player to look left right
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xSensitivity);
        InteractRaycast(false);
    }
    public void InteractRaycast(bool pressedInteractButton)
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, interactRaycastLength, layerMask))
        {
            if (hit.collider.gameObject.TryGetComponent(out IInteractable interactObj))
            {
                if (pressedInteractButton)
                {
                    interactObj.Interact();
                }
                else
                {
                    interactObj.InteractHover();
                }
            }
        }
    }
    void updateSensitivity()
    {
        xSensitivity = PlayerPrefs.GetFloat("xSens");
        ySensitivity = PlayerPrefs.GetFloat("ySens");
    }
    void ChaseFOV()
    {
        FOV = chaseFOV;
    }
    void RegularFOV()
    {
        FOV = regularFOV;
    }
    void Update()
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, FOV, changeSpeedFOV * Time.deltaTime);
    }

}
