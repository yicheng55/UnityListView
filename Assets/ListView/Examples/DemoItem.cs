﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class DemoItem : MonoBehaviour
{
    [SerializeField]
    private Image _image;
    [SerializeField]
    private Text _text;

    private void Awake()
    {
        if (_image == null)
        {
            _image = GetComponentInChildren<Image>();
        }

        if (_text == null)
        {
            _text = GetComponentInChildren<Text>();
        }

        _text.text = "-";
    }

    public void SetContent(string text, Color bgColor)
    {
        _text.text = text;
        _image.color = bgColor;
    }
    public void SetText(string text)
    {
        _text.text = text;
    }



    public void listViewOnClick(Text msg)
    {
        string tag_id;
        string log_Status;
        Debug.Log("listViewOnClick msg: " + msg.text);
        log_Status = msg.text;
        if (DemoMainCanvas1.instance.isShow)
        {
            char[] separators = new char[] { ' ', '|' };
            string[] subs = log_Status.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    //foreach (var sub in subs)
                    //{
                    //	Debug.Log($"Substring: " + sub);
                    //}

            tag_id = subs[1].Substring(0, 10);
            Debug.Log("tag_id: " + tag_id);
            DemoMainCanvas1.instance.UIlog_Status.text = tag_id;
        }
        else
        {
            Debug.Log("Msg isn't show!");
        }

    }

}
