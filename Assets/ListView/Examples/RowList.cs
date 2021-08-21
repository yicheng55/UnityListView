using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RowList : MonoBehaviour
{
    public Text text1;
    public Text text2;
    public Text text3;

    public string value1;
    public string value2;
    public string value3;


    public void UpdateUI()
    {
        text1.text = value1;
        text2.text = value2;
        text3.text = value3;
    }


}
