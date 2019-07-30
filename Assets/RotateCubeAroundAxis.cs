using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCubeAroundAxis : MonoBehaviour
{

    private bool rotate_toggle = false;

    void Update()
    {
        if(rotate_toggle)
        {
            // Spin the object around the world origin at 20 degrees/second.
            //transform.RotateAround(Vector3.zero, Vector3.up, 20 * Time.deltaTime);
            transform.Rotate(Vector3.up, 20 * Time.deltaTime);
        }

    }

    public void RotateCubeAction()
    {
        Debug.Log("Rotating around axis.");
        rotate_toggle = !rotate_toggle;

    }

}
