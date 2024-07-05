using Leap;
using Leap.Unity;
using Leap.Unity.PhysicalHands;
using UnityEngine;

public class ThumbPosCloseToIndexGesture : MonoBehaviour
{
    private Bone _thumbBone, _indexBone;
    private Vector3 _posA, _posB, _midPoint;

    public float entryDistance = 0.02f, exitDistance = 0.03f;
    private bool _contacting = false;

    public bool useBoneWidth = true;

    public float DistanceToContact { get { return _distanceToContact; } }
    private float _distanceToContact;

    public Chirality chirality;
    public bool processing;

    public LeapProvider leapProvider;
    private void Update()
    {
        Hand hand = leapProvider.GetHand(chirality);

        if (processing)
        {
            // Handle hand dropout
            if (hand != null)
            {
                CalculateContact(leapProvider.GetHand(chirality));
            }
        }
    }

    protected void CalculateContact(Hand hand)
    {
        _thumbBone = hand.Fingers[0].Bone(Bone.BoneType.TYPE_DISTAL);
        float tempDist;
        _distanceToContact = 1;
        for (int i = 1; i < hand.Fingers[1].bones.Length; i++)
        {
            _indexBone = hand.Fingers[1].bones[i];
            _posA = GetClosestPointOnFiniteLine(_thumbBone.NextJoint, _indexBone.NextJoint, _indexBone.PrevJoint);
            _posB = GetClosestPointOnFiniteLine(_indexBone.Center, _thumbBone.NextJoint, _thumbBone.Center);

            _midPoint = _posA + (_posB - _posA) / 2f;

            _posA = GetClosestPointOnFiniteLine(_midPoint, _indexBone.NextJoint, _indexBone.PrevJoint);
            _posB = GetClosestPointOnFiniteLine(_midPoint, _thumbBone.NextJoint, _thumbBone.Center);

            tempDist = Vector3.Distance(_posA, _posB);
            if (useBoneWidth) tempDist -= _indexBone.Width;

            if (tempDist < _distanceToContact)
            {
                _distanceToContact = tempDist;
            }
        }

        if (_contacting)
        {
            if (_distanceToContact > exitDistance)
            {
                _contacting = false;
            }
        }
        else
        {
            if (_distanceToContact < entryDistance)
            {
                _contacting = true;
            }
        }

        // TODO expose this somewhere? maybe allow people to use this instead of angle detector? 
        float contactValue = Mathf.InverseLerp(0.5f, entryDistance, _distanceToContact);
    }

    public Vector3 GetClosestPointOnFiniteLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDir = lineEnd - lineStart;
        float lineLength = lineDir.magnitude;
        lineDir.Normalize();
        float projectLength = Mathf.Clamp(Vector3.Dot(point - lineStart, lineDir), 0f, lineLength);
        return lineStart + lineDir * projectLength;
    }
}
