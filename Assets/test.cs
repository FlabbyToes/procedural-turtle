using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject g = new GameObject();
        GameObject s = new GameObject();
        GameObject d = new GameObject();
        Transform i = new GameObject("hi").transform;
        i.position = new Vector3(0, 10, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
