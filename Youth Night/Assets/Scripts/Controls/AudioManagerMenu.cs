using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerMenu : MonoBehaviour
{
    [HideInInspector] public AudioSource waterAmbience;
    [HideInInspector] public AudioSource bogAmbience;
    [HideInInspector] public AudioSource flashlightButtonSFX;

    void Start()
    {

        waterAmbience = gameObject.transform.Find("waterAmbience").gameObject.GetComponent<AudioSource>();
        bogAmbience = gameObject.transform.Find("bogAmbience").gameObject.GetComponent<AudioSource>();
        flashlightButtonSFX = gameObject.transform.Find("flashlightButtonSFX").gameObject.GetComponent<AudioSource>();
    }

}
