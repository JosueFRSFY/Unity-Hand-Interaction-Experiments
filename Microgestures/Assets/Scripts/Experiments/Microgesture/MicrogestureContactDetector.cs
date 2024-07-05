using Leap;
using Leap.Unity;
using System;
using UnityEngine;

public class MicrogestureContactDetector : MonoBehaviour
{
    public Action<bool> OnContactStateChange;

    public float thumbRotEntry = 10f;
    public float thumbRotExit = 12f;

    public Chirality chirality;
    public bool processing;

    public LeapProvider leapProvider;

    public float DistanceToAngleBoundary { get; private set; }
    public bool Contacting { get; private set; }


    private void Start()
    {
        if (leapProvider == null)
        {
            leapProvider = Hands.Provider;
        }
    }

    private void Update()
    {
        Hand hand = leapProvider.GetHand(chirality);

        if (processing)
        {
            // Handle hand dropout
            if (hand == null)
            {
                if (Contacting)
                {
                    Contacting = false;
                    OnContactStateChange?.Invoke(Contacting);
                }
            }
            else
            {
                bool prevContactState = Contacting;
                CalculateContacting(leapProvider.GetHand(chirality));

                if (Contacting != prevContactState)
                {
                    OnContactStateChange?.Invoke(Contacting);
                }
            }
        }
    }

    protected void CalculateContacting(Hand hand)
    {
        // Get the rotation of the index based on the knuckle and index tip
        Quaternion indexRotation = Quaternion.LookRotation
            (
                hand.GetIndex().Bone(Bone.BoneType.TYPE_METACARPAL).NextJoint -
                hand.GetIndex().TipPosition,
                 hand.Rotation * (hand.IsLeft ? Vector3.right : Vector3.left)
            );

        //Orient the rotation so that the up is facing the opposite way to the middle finger
        indexRotation *= Quaternion.Euler(Vector3.up * (hand.IsLeft ? 90 : -90));


        // Get the thumb rotation as the following
        // forward: from the centre of the proximal, to the tip of the thumb. This means that the rotation is fairly stable no matter the bend of the thumb
        Vector3 tip = hand.GetThumb().TipPosition;
        Vector3 proximal = hand.GetThumb().Bone(Bone.BoneType.TYPE_PROXIMAL).Center;
        Vector3 thumbForward = tip - proximal;

        // up: the same up vector as the index rotation
        Vector3 indexUp = indexRotation * Vector3.up;

        Quaternion knuckleTipRotation = Quaternion.LookRotation(thumbForward, indexUp);

        // Get the thumb rotation in the index axis's local space
        Quaternion thumbRotationInLocalIndexAxis = Quaternion.Inverse(indexRotation) * knuckleTipRotation;


        // Change the angles from a range of 0->360, to a range of -180->180
        Vector3 simplifiedEuler = thumbRotationInLocalIndexAxis.eulerAngles;
        simplifiedEuler = new Vector3(SimplifyAngle(simplifiedEuler.x) * -1, SimplifyAngle(simplifiedEuler.y) * -1, SimplifyAngle(simplifiedEuler.z));

        float rotationValue = simplifiedEuler.x;

        float angleBoundary = Contacting ? thumbRotExit : thumbRotEntry;

        DistanceToAngleBoundary = Mathf.InverseLerp(angleBoundary, 35, rotationValue);

        // If the x value of our thumb rotation is close to the "angle boundary", then it is fairly aligned with our constructed index "up", and therefore contacting
        Contacting = (rotationValue < angleBoundary);
    }

    /// <summary>
    /// Change the range of an angle from 0->360 to -180->180
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    private float SimplifyAngle(float angle)
    {
        angle = angle % 360;

        if (angle > 180)
        {
            angle -= 360;
        }
        else if (angle < -180)
        {
            angle += 360;
        }
        return angle;
    }
}
