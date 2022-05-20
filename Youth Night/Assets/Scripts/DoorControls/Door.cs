using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Animator myDoor = null;
    [SerializeField] public int OpenState;

    // Start is called before the first frame update
    void Start()
    {
        myDoor = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Open()
    {
        switch (OpenState)
        {
            case -1:
            case 0:
                myDoor.Play("DoorOpen", 0, 0.0f);
                OpenState = 1;
                break;

        }
    }
    public void OpenReverse()
    {
        switch (OpenState)
        {
            case 1:
            case 0:
                myDoor.Play("DoorOpenReverse", 0, 0.0f);
                OpenState = -1;
                break;

        }
    }


    public void Close()
    {
        switch (OpenState)
        {
            case -1:
                myDoor.Play("DoorCloseReverse", 0, 0.0f);
                OpenState = 0;
                break;
            case 1:
                myDoor.Play("DoorClose", 0, 0.0f);
                OpenState = 0;
                break;

        }
    }
}
