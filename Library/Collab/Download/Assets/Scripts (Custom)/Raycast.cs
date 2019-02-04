using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycast : MonoBehaviour {

    private float rayLength;
    private Vector3 LeftAngVec;
    private Vector3 RightAngVec;

    // Use this for initialization
    void Start () {
        rayLength = 7f;
        LeftAngVec = new Vector3(-1, 0, 1);
        RightAngVec = new Vector3(1, 0, 1);
    }
	
	// Update is called once per frame
	void Update () {
        RaycastHit forwardHit, rightHit, leftHit, leftAngleHit, rightAngleHit;
        float forwardDistance, rightDistance, leftDistance, leftAngleDistance, rightAngleDistance;

        Vector3 forwardVector = transform.TransformDirection(Vector3.forward) * rayLength;
        Vector3 rightVector = transform.TransformDirection(Vector3.right) * rayLength;
        Vector3 leftVector = transform.TransformDirection(Vector3.left) * rayLength;
        Vector3 leftAngleVector = transform.TransformDirection(LeftAngVec) * rayLength;
        Vector3 rightAngleVector = transform.TransformDirection(RightAngVec) * rayLength;

        Debug.DrawRay(transform.position, forwardVector, Color.green);
        Debug.DrawRay(transform.position, rightVector, Color.green);
        Debug.DrawRay(transform.position, leftVector, Color.green);
        Debug.DrawRay(transform.position, leftAngleVector, Color.green);
        Debug.DrawRay(transform.position, rightAngleVector, Color.green);

        if (Physics.Raycast(transform.position, (forwardVector), out forwardHit))
        {
            forwardDistance = forwardHit.distance;         
        //    print("Front ray hit. Distance " + forwardDistance + " to " + forwardHit.collider.gameObject.name);
        }

        if (Physics.Raycast(transform.position, (rightVector), out rightHit))
        {
            rightDistance = rightHit.distance;
            //    print("Right ray hit. Distance " + rightDistance + " to " + rightHit.collider.gameObject.name);
        }

        if (Physics.Raycast(transform.position, (leftVector), out leftHit))
        {
            leftDistance = leftHit.distance;
            //        print("Left ray hit. Distance " + leftDistance + " to " + leftHit.collider.gameObject.name);
        }

        if (Physics.Raycast(transform.position, (leftAngleVector), out leftAngleHit))
        {
            leftAngleDistance = leftAngleHit.distance;
            //        print("Left angled ray hit. Distance " + leftAngleDistance + " to " + leftAngleHit.collider.gameObject.name);
        }

        if (Physics.Raycast(transform.position, (leftAngleVector), out rightAngleHit))
        {
            rightAngleDistance = rightAngleHit.distance;
            //        print("Right angled ray hit. Distance " + rightAngleDistance + " to " + rightAngleHit.collider.gameObject.name);
        }
    }
}
