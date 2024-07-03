/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2024.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using System;
using UnityEngine;

namespace Leap.Unity.Preview.Locomotion
{
    public class LightweightGrabPoseDetector : MonoBehaviour
    {
        public enum GrabType
        {
            GRAB, FIST
        }

        public LeapProvider leapProvider;
        public Chirality chirality;

        public GrabType grabType = GrabType.FIST;

        [Header("Grab Activation Settings")]
        public float activate = 0.85f;
        public float deactivate = 0.6f;

        public Action<Hand> OnGrab, OnUngrab, OnGrabbing;

        public bool IsGrabbing { get; private set; }

        public float SquishPercent { get; private set; }

        private Chirality _chiralityLastFrame;

        private void Start()
        {
            _chiralityLastFrame = chirality;
            if (leapProvider == null)
            {
                leapProvider = FindAnyObjectByType<LeapProvider>();
            }
        }

        private void Update()
        {
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

        public float grabAmount = 0;

        private void UpdateGrabStatus(Hand hand)
        {
            if (hand == null)
            {
                return;
            }

            grabAmount = 0;
            if (grabType == GrabType.FIST)
            {
                grabAmount = hand.GetFistStrength();
            }
            else if (grabType == GrabType.GRAB)
            {

                grabAmount = hand.GrabStrength;
            }

            if (grabAmount > activate)
            {
                if (!IsGrabbing)
                {
                    OnGrab?.Invoke(hand);
                }
                IsGrabbing = true;
            }
            else if (grabAmount < deactivate)
            {
                if (IsGrabbing)
                {
                    OnUngrab?.Invoke(hand);
                }
                IsGrabbing = false;
            }

            if (IsGrabbing)
            {
                OnGrabbing?.Invoke(hand);
                SquishPercent = Mathf.InverseLerp(activate, 1, grabAmount);
            }
            else
            {
                SquishPercent = 0;
            }
        }
    }
}
