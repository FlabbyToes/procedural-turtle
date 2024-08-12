using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailForce : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Rigidbody body;
    [SerializeField] float force;
    [SerializeField] float maxSpeed;
    [SerializeField] float oscilationTime;
    float timePassed;
    void Start()
    {

    }
    void swingTail()
    {
        Vector3 Direction = transform.right;
        if (timePassed >= oscilationTime)
        {
            timePassed = 0;
            //left
            if (Vector3.Dot(transform.right, body.transform.position) < 0)
            {
                body.AddForce(-transform.forward * force);
                body.AddForce(Direction * force);
            }
            //right
            else
            {
                body.AddForce(-transform.forward * force);
                body.AddForce(-Direction * force);
            }
            //clamp speed
            if (body.velocity.magnitude > maxSpeed)
            {
                body.velocity = body.velocity.normalized * maxSpeed;
            }
        }

        if (body.velocity.magnitude > maxSpeed)
        {
            body.velocity = body.velocity.normalized * maxSpeed;
        }
        timePassed += Time.fixedDeltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        swingTail();
        if (Input.GetKeyDown(KeyCode.D))
        {
            body.AddForce(transform.right * force);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            body.AddForce(-transform.right * force);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            body.AddForce(-transform.forward * force);
        }
    }
}
