using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDoorController : MonoBehaviour
{
    [SerializeField] private Door myDoor = null;
    [SerializeField] private bool isReversed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (myDoor.OpenState != 0)
        {
            myDoor.Close();
        } else if (isReversed)
        {
            myDoor.OpenReverse();
        } else
        {
            myDoor.Open();
        }
    }
}
