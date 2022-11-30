using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class SampleGroupMovement : MonoBehaviour
{
    //The position calculated from the masterObject
    private Vector3 relativePosition;
    //What position do we want the slaveObject to have in local space?
    private Vector3 wantedPosition = new Vector3(0,0,-5);
    //The slave object(child)
    public Transform slaveObject;
    //The master object(parent)
    public Transform masterObject;

    //Call every frame
    void Update()
    {
        //Calculate the position
        //What this does:
        //Take the master object and use the local space "wantedPosition" to
        //calculate a world space position. Assign this to "relative position"

        relativePosition = masterObject.TransformPoint(wantedPosition);

        //Set the position of the slave object to this relative position
        slaveObject.position = relativePosition;

        //Make the slave objects look at the master object
        slaveObject.LookAt(masterObject);
    }
}
