using Leap;
using Leap.Unity;
using Leap.Unity.PhysicalHands;
using UnityEngine;

public class ThumbPosCloseToIndexGesture : MonoBehaviour
{
    [SerializeField] private float _entryDistance = 0.02f;
    [SerializeField] private float _exitDistance = 0.03f;

    [SerializeField] private bool _useBoneWidth = true;

    [SerializeField] private Chirality _chirality;
    [SerializeField] private bool _processing;

    [SerializeField] private LeapProvider _leapProvider;

    public float DistanceToContact { get { return _distanceToContact; } }
    private float _distanceToContact;

    private Bone _thumbBone, _indexBone;
    private Vector3 _posA, _posB, _midPoint;

    private bool _contacting = false;

    private void Update()
    {
        Hand hand = _leapProvider.GetHand(_chirality);

        // Handle hand dropout
        if (_processing && hand != null)
            CalculateContact(_leapProvider.GetHand(_chirality));
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
            if (_useBoneWidth) tempDist -= _indexBone.Width;

            if (tempDist < _distanceToContact)
            {
                _distanceToContact = tempDist;
            }
        }

        if (_contacting)
        {
            if (_distanceToContact > _exitDistance)
            {
                _contacting = false;
            }
        }
        else
        {
            if (_distanceToContact < _entryDistance)
            {
                _contacting = true;
            }
        }
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
