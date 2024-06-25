/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2023.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using System;
using System.Collections;
using Ultraleap;
using UnityEngine;


/// <summary>
/// A lightweight Grab Detector.
/// Utilises hysteresis in order to have different grab and ungrab thresholds.
/// </summary>
public class LightweightGrabDetector : MonoBehaviour
{
    public LeapProvider leapProvider;
    public Chirality chirality;

    [Header("Grab Activation Settings")]
    [Tooltip("The grab strength at which a grab is activated.")]
    public float activateStrength = 1;
    [Tooltip("The grab strength at which a grab is deactivated.")]
    public float deactivateStrength = 0.9f;

    public Action<Hand> OnGrab, OnUngrab, OnGrabbing;

    public bool IsGrabbing { get; private set; }

    private Chirality _chiralityLastFrame;

    private bool init = false;

    private IEnumerator Start()
    {
        _chiralityLastFrame = chirality;
        if (leapProvider == null)
        {
            leapProvider = FindObjectOfType<LeapProvider>();
        }

        //fix false data on start
        int i = 0;
        while(i< 90)
        {
            i++;
            yield return new WaitForEndOfFrame();
        }

        init = true;
    }

    private void Update()
    {
        if (!init)
        {
            return;
        }

        if (_chiralityLastFrame != chirality)
        {
            IsGrabbing = false;
        }
        if (leapProvider != null && leapProvider.CurrentFrame != null)
        {
            UpdateGrabStatus(leapProvider.CurrentFrame.GetHand(chirality));
        }
        _chiralityLastFrame = chirality;
    }

    private void UpdateGrabStatus(Hand hand)
    {
        if (hand == null)
        {
            return;
        }

        if (hand.GrabStrength >= activateStrength)
        {
            if (!IsGrabbing)
            {
                OnGrab?.Invoke(hand);
            }
            else
            {
                OnGrabbing?.Invoke(hand);
            }

            IsGrabbing = true;
        }
        else if (hand.GrabStrength <= deactivateStrength)
        {
            if (IsGrabbing)
            {
                OnUngrab?.Invoke(hand);
            }
            IsGrabbing = false;
        }
    }
}
