using Leap;
using Leap.Unity;
using System;
using UnityEngine;

public class MicrogestureSlideDetector : MonoBehaviour
{
    public Action<float> OnSlide;

    public bool processing;
    public Chirality chirality;

    public float SlideValue { get; private set; }

    public LeapProvider leapProvider;
    public float minAngle = -5f;
    public float maxAngle = 50f;

    private void Update()
    {
        Hand hand = leapProvider.GetHand(chirality);

        if (processing)
        {
            // Handle hand dropout
            if (hand != null)
            {
                CalculateSlide(leapProvider.GetHand(chirality));
                OnSlide?.Invoke(SlideValue);
            }
            else
            {

            }
        }
    }

    protected void CalculateSlide(Hand hand)
    {
        Quaternion thumbInterRotationLocal = Quaternion.Inverse(hand.Rotation) * hand.GetThumb().bones[2].Rotation;
        SlideValue = RotationalSlide(thumbInterRotationLocal, hand.IsLeft);
    }

    protected float RotationalSlide(Quaternion thumbInterRotationLocal, bool isLeft)
    {
        var sliderValueNew = thumbInterRotationLocal.eulerAngles.x;
        if (sliderValueNew > 180)
            sliderValueNew -= 360;

        //L: -5 to 50, R:50 to -5
        return InverseLerpUnclamped(isLeft ? minAngle : maxAngle, isLeft ? maxAngle : minAngle, sliderValueNew);
    }
    protected float InverseLerpUnclamped(float a, float b, float value)
    {
        return (value - a) / (b - a);
    }
}
