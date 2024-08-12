using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class ProceduralSpider : MonoBehaviour
{
    [SerializeField] Transform armature;
    [SerializeField] private List<Transform> ikTargets;
    private List<Transform> normalGroundTargets;
    private List<Transform> forwardGroundTargets;
    private List<bool> isMoving;
    //private List<bool> isGrounded; may implement later
    private Quaternion ikRotationOffset ;
    private Quaternion armatureRotationOffset;

    [SerializeField] private Vector3 fGTBoxcastHalfExtents;
    [SerializeField] private float forwardGroundTargetOffset = 1f;

    [SerializeField] private float stepDistanceThreshold = 1f;
    [SerializeField] private float legRotateSpeed;
    [SerializeField] private float legUpdateTime = 1f;
    [SerializeField] private AnimationCurve legHeightCurve;
    [SerializeField] private AnimationCurve legInterpolationCurve;
    [SerializeField] private float legHeightMax;
    [SerializeField] private float stepHeightFactor = 1f;
    [SerializeField] private float bodyHeight = 1f;
    [SerializeField] private float forwardTargetSmoothSpeed = 1f;
    [SerializeField] private float legFixSpeed = 3f;

    private float stepHeight = 1f;
    private float legLength = 1f;
    [SerializeField] private List<int> groundCheckLayers;
    private Transform ikCenter;

    private int groundCheckMask = 0;
    private List<Vector3> currentPosition;
    private List<Vector3> previousPosition;
    private List<Vector3> velocity;

    /*public bool anyFeetMoving { get {
            if (isMoving == null)
            {
                return false;
            }
            for (int i =0; i < isMoving.Count; ++i)
            {
                if (isMoving[i])
                {
                    return true;
                }
            }
            return false; }
        private set {}
    }*/
    
    // Start is called before the first frame update
    void Start()
    {
        velocity = new List<Vector3>();
        previousPosition = new List<Vector3>();
        currentPosition = new List<Vector3>();

        normalGroundTargets = new List<Transform>();
        forwardGroundTargets = new List<Transform>();
        isMoving = new List<bool>();

        ikCenter = new GameObject("Center").transform;
        ikCenter.SetParent(transform);
        

        for (int i = 0; i < groundCheckLayers.Count; ++i)
        {
            groundCheckMask = groundCheckMask | (1 << groundCheckLayers[i]);
        }
        //Create ground targets, and set position, and parent
        for (int i = 0; i < ikTargets.Count; ++i)
        {
            //Normal
            normalGroundTargets.Add(new GameObject("ng" + i.ToString()).transform);
            normalGroundTargets[i].SetParent(transform);
            normalGroundTargets[i].position = ikTargets[i].position;
            //Forward
            forwardGroundTargets.Add(new GameObject("fg" + i.ToString()).transform);
            forwardGroundTargets[i].SetParent(normalGroundTargets[i]);
            forwardGroundTargets[i].position = normalGroundTargets[i].position;
            //isMoving
            isMoving.Add(false);
            velocity.Add(Vector3.zero);
            currentPosition.Add(normalGroundTargets[i].transform.position);
            previousPosition.Add(normalGroundTargets[i].transform.position);

            ikCenter.position += ikTargets[i].position;
        }
        legLength = ikTargets[0].position.y;
        stepHeight = legLength * stepHeightFactor;

        ikRotationOffset = ikTargets[0].rotation;
        armatureRotationOffset = armature.rotation;

        ikCenter.position /= ikTargets.Count;
    }
    RaycastHit forwardGroundTargetUpdate(int i)
    {
        Vector3 vel = Vector3.zero;
        
        Vector3 offset = velocity[i].normalized * forwardGroundTargetOffset;
        forwardGroundTargets[i].position = Vector3.SmoothDamp(forwardGroundTargets[i].position, normalGroundTargets[i].position + offset,
            ref vel, forwardTargetSmoothSpeed);
        RaycastHit hit;
        Vector3 origin = forwardGroundTargets[i].position + forwardGroundTargets[i].up * stepHeight;
        if (Physics.BoxCast(origin, fGTBoxcastHalfExtents, - forwardGroundTargets[i].up, out hit, forwardGroundTargets[i].rotation, legLength + (2 * stepHeight), groundCheckMask))
        {
            float dist = hit.distance - stepHeight - legLength;
            Vector3 heightOffset = -forwardGroundTargets[i].up * dist;
            forwardGroundTargets[i].position += heightOffset;
            if (isMoving[i])
            {

                Quaternion to = Quaternion.FromToRotation(forwardGroundTargets[i].up, hit.normal) * forwardGroundTargets[i].rotation;
                forwardGroundTargets[i].rotation = Quaternion.RotateTowards(forwardGroundTargets[i].rotation, to, legRotateSpeed * Time.deltaTime);
            }
            
        }

        Debug.DrawRay(origin, -forwardGroundTargets[i].up * (legLength + (2 * stepHeight)), Color.green);
        return hit;
        
    }
    void groundTargetUpdate(int i)
    {
        
        RaycastHit hit;
        Vector3 origin = normalGroundTargets[i].position + normalGroundTargets[i].up * stepHeight;
        Debug.DrawRay(origin, -normalGroundTargets[i].up * (legLength + (2 * stepHeight)), Color.green);
        if (Physics.Raycast(origin, -normalGroundTargets[i].up, out hit, legLength + (2*stepHeight), groundCheckMask))
        {
            float dist = hit.distance - stepHeight - legLength;
            Vector3 offset = -normalGroundTargets[i].up * dist;
            normalGroundTargets[i].position += offset;
        }
        
    }
    void ikTargetCheck(int i, RaycastHit hit)
    {
        /*Quaternion to = Quaternion.FromToRotation(forwardGroundTargets[i].up, hit.normal) * forwardGroundTargets[i].rotation;
        forwardGroundTargets[i].rotation = Quaternion.RotateTowards(forwardGroundTargets[i].rotation, to, legRotateSpeed * Time.deltaTime);*/
        ikTargets[i].rotation = forwardGroundTargets[i].rotation * ikRotationOffset;
        float dist = Vector3.Distance(ikTargets[i].position, forwardGroundTargets[i].position);
        if (dist >= stepDistanceThreshold)
        {
            if (!isMoving[i] && !isMoving[(i+1)%2])
            {
                StartCoroutine(movingIK(i));
            }
        }
    }
    IEnumerator movingIK(int ik)
    {
        List<Vector3> startPos = new List<Vector3>();
        for (int i = ik % 2; i < ikTargets.Count; i = i + 2)
        {
            isMoving[i] = true;
            startPos.Add(forwardGroundTargets[i].position - ikTargets[i].position);
        }
        float elapsedTime = 0f;
        float curvePrevious = 0;
        while (elapsedTime < legUpdateTime)
        {
            bool pause = false;
            for (int i = ik % 2; i < ikTargets.Count; i = i + 2)
            {
                float newX = Mathf.Lerp(forwardGroundTargets[i].position.x - startPos[i/2].x, forwardGroundTargets[i].position.x, elapsedTime / legUpdateTime);
                float newY = Mathf.Lerp(forwardGroundTargets[i].position.y - startPos[i/2].y, forwardGroundTargets[i].position.y, elapsedTime / legUpdateTime)
                    - ikTargets[i].up.y * legHeightCurve.Evaluate(elapsedTime / legUpdateTime) * legHeightMax;
                float newZ = Mathf.Lerp(forwardGroundTargets[i].position.z - startPos[i/2].z, forwardGroundTargets[i].position.z, elapsedTime / legUpdateTime);
                //gotta change the lerp... maybe... idk how to fix
                //problem: when y changes suddenly, if the lerp is close to 1, will jump. if lerp is at or close to 0 its fine. dunno what the intended behaviour should be like.
                //solution, pause time and movetowards until close enough then unpause
                Vector3 diff = new Vector3(newX, newY, newZ) - ikTargets[i].position;
                if (diff.magnitude > 1)
                {
                    pause = true;
                    ikTargets[i].position = Vector3.MoveTowards(ikTargets[i].position, new Vector3(newX, newY, newZ), legFixSpeed* Time.deltaTime);
                }
                else
                {
                    ikTargets[i].position += diff;
                }
            }
            curvePrevious = legHeightCurve.Evaluate(elapsedTime / legUpdateTime);
            if (!pause)
            {
                elapsedTime += Time.deltaTime;
            }

            // Yield here
            yield return new WaitForFixedUpdate();
        }
        for (int i = ik % 2; i < ikTargets.Count; i = i + 2)
        {
            ikTargets[i].position = forwardGroundTargets[i].position;
        }
        for (int i = ik % 2; i < ikTargets.Count; i = i + 2)
        {
            isMoving[i] = false;
        }
        yield return new WaitForFixedUpdate();
    }
    void updateBodyPosition()
    {
        Vector3 avgLegPosition = Vector3.zero;
        Vector3 upSum = Vector3.zero;
        //position
        for (int i = 0; i < ikTargets.Count; ++i)
        {
            avgLegPosition += ikTargets[i].position;
        }
        avgLegPosition /= ikTargets.Count;

        transform.position = Vector3.MoveTowards(transform.position,  new Vector3(transform.position.x, avgLegPosition.y+ bodyHeight, transform.position.z), 1*Time.deltaTime);//add speed variable
        //rotation
        for (int i = 0; i < ikTargets.Count; ++i)
        {
            Plane plane = new Plane(ikTargets[i].position, ikTargets[(i+1)%ikTargets.Count].position, ikCenter.position);
            if ((Vector3.Dot(transform.up, plane.normal) < 0))
            {
                plane.Flip();
            }
            upSum += plane.normal;
            Debug.DrawRay(ikCenter.position, plane.normal*100, Color.gray);
        }
        upSum = upSum.normalized;
        armature.rotation = Quaternion.RotateTowards(armature.rotation, Quaternion.LookRotation(upSum, -transform.forward), 1 * Time.deltaTime); //add speed variable
        Debug.DrawRay(ikCenter.position, Vector3.up, Color.yellow, 100);
    }
    void velocityUpdate(int i)
    {
            currentPosition[i] = normalGroundTargets[i].position;
            if (currentPosition[i] - previousPosition[i] != Vector3.zero)
            {
                velocity[i] = currentPosition[i] - previousPosition[i];
            }
            previousPosition[i] = currentPosition[i];
    }
    void OnDrawGizmos()
    {
        if (normalGroundTargets == null)
        {
            return;
        }
        for (int i=0; i < normalGroundTargets.Count; ++i)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(normalGroundTargets[i].position, 2);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(forwardGroundTargets[i].position, 2);
        }
    }
        // Update is called once per frame
        void Update()
    {
        for(int i = 0; i < ikTargets.Count; ++i)
        {
            groundTargetUpdate(i);
            RaycastHit hit = forwardGroundTargetUpdate(i);
            ikTargetCheck(i, hit);
            velocityUpdate(i);
        }
        updateBodyPosition();
    }
}
