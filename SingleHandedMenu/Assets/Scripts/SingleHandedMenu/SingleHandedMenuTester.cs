using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleHandedMenuTester : MonoBehaviour
{
    public SingleHandedMenu singleHandedMenu;
    public bool logEvents = false;
    // Start is called before the first frame update
    void Start()
    {
        singleHandedMenu.OnAttachStart.AddListener(OnAttachStart);
        singleHandedMenu.OnAttaching.AddListener(OnAttaching);
        singleHandedMenu.OnAttachEnd.AddListener(OnAttachEnd);

        singleHandedMenu.OnDetachStart.AddListener(OnDetachStart);
        singleHandedMenu.OnDetaching.AddListener(OnDetaching);
        singleHandedMenu.OnDetachEnd.AddListener(OnDetachEnd);
    }


    private void OnDetachStart()
    {
        LogEvent("OnDetachStart");
    }

    private void OnDetaching(float t)
    {
        LogEvent("OnDetaching: " + t);
    }

    private void OnDetachEnd(bool detached)
    {
        LogEvent("OnDetachEnd: " + detached);
    }

    private void OnAttachEnd(bool attached)
    {
        LogEvent("OnAttachEnd: " + attached);
    }

    private void OnAttaching(float t)
    {
        LogEvent("OnAttaching: " + t);
    }

    private void OnAttachStart()
    {
        LogEvent("OnAttachStart");
    }

    private void LogEvent(string message)
    {
        if (!logEvents) return;
        Debug.Log(message);
    }
}
