using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAimScript : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject headTarget;
    [SerializeField] private GameObject headTargetBase;
    [SerializeField] private AnimationCurve headIdleCurve;
    [SerializeField] private float idleLookDistance = 10;
    [SerializeField] private float activateDistance;
    [SerializeField] private float lookSpeed = 1f;
    [SerializeField] private float headGroundOffset = 2f;
    private bool isIdling;
    private int flip = 1;
    public bool activated;
    void Start()
    {
        if (player==null)
        {
            player = GameObject.FindWithTag("Player");
        }
        isIdling = false;
        activated = false;
    }
    void checkDistance()
    {
        if (Vector3.Distance(player.transform.position, headTargetBase.transform.position)>activateDistance)
        {
            activated = false;
            if (!isIdling)
            {
                StartCoroutine(movingIK(flip* idleLookDistance, flip * (headTarget.transform.position - headTargetBase.transform.position).magnitude, lookSpeed));
                
            }
        }
        else{
            if (!isIdling)
            {
                activated = true;
                lookAtTarget();

            }
        }

    }
    IEnumerator movingIK(float position, float startPos, float updateTime)
    {
        isIdling = true;
        float elapsedTime = 0f;
        if (Math.Abs(startPos)> Math.Abs(idleLookDistance))
        {
            startPos = 0;
            while ((headTarget.transform.position - (headTargetBase.transform.position - headTarget.transform.right * startPos)).magnitude > .5f)
            {
                headTarget.transform.position = Vector3.MoveTowards(headTarget.transform.position, headTargetBase.transform.position - headTarget.transform.right * startPos, lookSpeed * Time.deltaTime);
                yield return new WaitForFixedUpdate();
            }
        }
        
        while (elapsedTime < updateTime)
        {
            headTarget.transform.position = Vector3.Lerp(headTargetBase.transform.position - headTarget.transform.right * startPos,
                headTargetBase.transform.position + headTarget.transform.right * position, headIdleCurve.Evaluate(elapsedTime / updateTime));
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        headTarget.transform.position = headTargetBase.transform.position + headTarget.transform.right * position;
        isIdling = false;
        flip = -flip;
        yield return new WaitForFixedUpdate();
    }
    private void lookAtTarget()
    {
        headTarget.transform.position = Vector3.MoveTowards(headTarget.transform.position, player.transform.position, lookSpeed * Time.deltaTime);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        checkDistance();
    }
    
}
