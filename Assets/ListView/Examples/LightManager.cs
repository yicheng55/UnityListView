using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public GameObject[] lightings;

    public static LightManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Light Up RedLight!");
            TurnOn(0);
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Turn off RedLight!");
            TurnOff(0);
        }
    }

    public void TurnOn(int index)
    {
        lightings[index].SetActive(true);
    }

    public void TurnOff(int index)
    {
        lightings[index].SetActive(false);
    }
}
