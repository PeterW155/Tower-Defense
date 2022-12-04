using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopChange : MonoBehaviour
{
    public float x;
    public float y;
    public float z;
    public float yRot;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.localPosition = new Vector3(0, -1, 0);
        this.gameObject.transform.localEulerAngles = new Vector3(0, 0, 0);
    }
}
