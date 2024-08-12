using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class TurtleWalkScript : MonoBehaviour
{
    [SerializeField] Transform sourceTarget;

    [SerializeField] float maxWalkVelocity;
    [SerializeField] float maxTurnVelocity;
    [SerializeField] private float moveOffsetRadius;
    [SerializeField] private float activateDistance;
    private Vector3 targetLevel;
    HeadAimScript headAim;
    // Start is called before the first frame update

    private void Start()
    {
        headAim = GetComponent<HeadAimScript>();
    }
    void turnToTarget()
    {
        Quaternion to = Quaternion.FromToRotation(transform.forward, targetLevel - transform.position) * transform.rotation;
        Debug.DrawLine(transform.position, targetLevel,Color.magenta);
        Debug.DrawRay(transform.position, targetLevel - transform.position, Color.red);
        //to = Quaternion.Euler(0, to.eulerAngles.y, 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, to, maxTurnVelocity * Time.deltaTime);
    }
    void moveToTarget()
    {
        targetLevel = new Vector3(sourceTarget.position.x, transform.position.y, sourceTarget.position.z);
        Vector3 moveOffset = (transform.position - targetLevel).normalized * moveOffsetRadius;
        moveOffset += targetLevel;
        transform.position = Vector3.MoveTowards(transform.position, moveOffset, maxWalkVelocity * Time.deltaTime);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetLevel, 2);
    }

    void FixedUpdate()
    {
        if (headAim.activated)
        {
            moveToTarget();
            turnToTarget();
        }
    }
}
