using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Variables")]
    public float stamina;
    [SerializeField] private float maxStamina = 100f;

    [Header("Stamina Regen Variables")]
    [SerializeField] private float staminaRegen = 5f;
    [SerializeField] private float drainDelay = 3f;
    [SerializeField] private bool isTired;
    public bool IsTired {  get { return isTired; } }
    [SerializeField] private bool depletedDelayBool;

    [Header("Stamina UI")]
    [SerializeField] private Image staminaSliderFill;
    [SerializeField] private CanvasGroup staminaSlider;

    // Start is called before the first frame update
    void Awake()
    {
        stamina = maxStamina;
        isTired = false;
        depletedDelayBool = true;
    }

    public void staminaDrainingFactor(float factor)
    {
        stamina -= factor * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        if (stamina == 0)
        {
            isTired = true;
            if (depletedDelayBool)
            {
                depletedDelayBool = false;
                Invoke("depletedDelay", drainDelay);
            }
        }
        updateStaminaUI(stamina);
    }
    public void staminaDrainingFlat(float flat)
    {
        stamina -= flat;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        if (stamina == 0)
        {
            isTired = true;
            if (depletedDelayBool)
            {
                depletedDelayBool = false;
                Invoke("depletedDelay", drainDelay);

            }
        }
        updateStaminaUI(stamina);
    }
    public void staminaRegening()
    {
        stamina += staminaRegen * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        updateStaminaUI(stamina);
    }
    private void depletedDelay()
    {
        isTired = false;
        depletedDelayBool = true;
    }
    private void updateStaminaUI(float v)
    {
        if (staminaSliderFill==null|| staminaSlider ==null)
        {
            return;
        }
        staminaSliderFill.fillAmount = stamina / maxStamina;
        if (v == maxStamina)
        {
            staminaSlider.alpha = 0;
        }
        else
        {
            staminaSlider.alpha = 1;
        }
    }

}
