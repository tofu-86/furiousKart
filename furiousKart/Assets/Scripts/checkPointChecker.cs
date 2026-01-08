using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkPointChecker : MonoBehaviour
{
    public CarController carController; // Reference to script
    
    private void OnTriggerEnter(Collider other) // method called when "other" collider enters the trigger collider.
    {
        if (other.tag == "Checkpoint") // checking collider tag
        {
            //Debug.Log(other.GetComponent<checkpoints>().checkPointNum + "Checkpoint");

            carController.checkPointHit(other.GetComponent<checkpoints>().checkPointNum); // calls checkPointHit method on the carcontroller, and passes the checkpointNum.
        }
    }
}
