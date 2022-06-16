using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{

    public GameObject ON;
    public GameObject OFF;
    private bool isON;

    // Start is called before the first frame update
    void Start()
    {

        ON.SetActive(false);
        OFF.SetActive(true);
        isON = false;

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.F))
        {

            if (isON)
            {
                ON.SetActive(false);
                OFF.SetActive(true);
            }

            if (!isON)
            {
                ON.SetActive(true);
                OFF.SetActive(false);
            }

            isON = !isON;
        }

    }
}