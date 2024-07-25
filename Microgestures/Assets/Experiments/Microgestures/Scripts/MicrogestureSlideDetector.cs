using Leap;
using Leap.Unity;
using System;
using UnityEngine;

public class MicrogestureSlideDetector : MonoBehaviour
{
    [SerializeField] private LeapProvider _leapProvider;
    [SerializeField] private float _minAngle = -5f;
    [SerializeField] private float _maxAngle = 50f;

    public float SlideValue { get; private set; }

    public Action<float> OnSlide;

    public Chirality Chirality;
    public bool Processing;

    private void Update()
    {
        Hand hand = _leapProvider.GetHand(Chirality);

        // Handle hand dropout
        if (Processing && hand != null)
        {
            CalculateSlide(_leapProvider.GetHand(Chirality));
            OnSlide?.Invoke(SlideValue);
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
        return InverseLerpUnclamped(isLeft ? _minAngle : _maxAngle, isLeft ? _maxAngle : _minAngle, sliderValueNew);
    }
    protected float InverseLerpUnclamped(float a, float b, float value)
    {
        return (value - a) / (b - a);
    }
}
