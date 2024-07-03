using DG.Tweening;
using Leap.Unity.PhysicalHands;
using Leap.Unity.Preview.HandRays;
using System.Collections.Generic;
using UnityEngine;

public class SquishableObject : FarFieldObject
{
    public enum SquishState { IDLE, HOVERED, SQUISHING, SQUISHED, UNSQUISHING, SUMMONED }

    public bool Locked
    {
        get
        {
            return squishState == SquishState.SQUISHING
                || squishState == SquishState.UNSQUISHING
                || squishState == SquishState.SQUISHED;
        }
    }

    private Color _hoveredColor;
    public SquishState squishState;
    public Vector3 originalScale;
    public Vector3 originalPos;
    public Quaternion originalRot;

    private Color _color;
    private Rigidbody _rigidbody;
    private Collider _collider;
    private Vector3 _localScale;
    private bool _setLocalScale;
    public SquishSummon owner;
    public HashSet<SquishSummon> interestedOwners = new HashSet<SquishSummon>();

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _localScale = transform.localScale;

        squishState = SquishState.IDLE;
        originalScale = transform.localScale;
        _color = GetComponentInChildren<Renderer>().material.color;
        _hoveredColor = Color.Lerp(_color, Color.white, .75f); // lighten by 50 %
    }

    private void FixedUpdate()
    {
        if (_setLocalScale)
        {
            transform.localScale = _localScale;
            _setLocalScale = false;
        }

        if (squishState == SquishState.SQUISHED || squishState == SquishState.UNSQUISHING)
        {
            TrackThrowingVelocities();
            PhysicsMovement(_trackedTransform.position, _trackedTransform.rotation);
        }
    }

    public void SetState(SquishState newState, SquishSummon summoner)
    {
        if (summoner != owner && Locked)
        {
            return;
        }

        switch (newState)
        {
            case SquishState.IDLE:
                owner = null;
                interestedOwners.Remove(summoner);
                SetScale(originalScale);
                if (interestedOwners.Count == 0)
                {
                    GetComponentInChildren<Renderer>().material.DOColor(_color, 0.2f);
                    squishState = newState;
                }
                break;
            case SquishState.HOVERED:
                owner = null;
                interestedOwners.Add(summoner);
                SetScale(originalScale);
                if (interestedOwners.Count == 1)
                {
                    GetComponentInChildren<Renderer>().material.DOColor(_hoveredColor, 0.2f);
                    squishState = newState;
                }
                break;
            case SquishState.SQUISHING:
                if (owner == null)
                {
                    owner = summoner;
                }

                if (owner != summoner)
                {
                    return;
                }
                squishState = newState;
                originalPos = transform.position;
                originalRot = transform.rotation;
                originalScale = transform.localScale;
                break;
            case SquishState.SQUISHED:
                squishState = newState;

                _trackedTransform = summoner.activeObjectTransformHelper;
                _collider.enabled = false;
                _rigidbody.position = _trackedTransform.position;
                _rigidbody.rotation = _trackedTransform.rotation;
                SetScale(Vector3.zero);

                break;
            case SquishState.UNSQUISHING:
                squishState = newState;
                break;
            case SquishState.SUMMONED:
                squishState = newState;
                Summon();
                break;
        }
    }
    private Transform _trackedTransform;

    private void Summon()
    {
        squishState = SquishState.SUMMONED;
        SetScale(originalScale);
        _collider.enabled = true;
        ThrowingOnRelease();

        SetState(SquishState.IDLE, null);
    }



    public void SetScale(Vector3 scale)
    {
        _localScale = scale;
        _setLocalScale = true;
    }

    private const float VELOCITY_HISTORY_LENGTH = 0.045f;

    private Queue<VelocitySample> _velocityQueue = new Queue<VelocitySample>();

    private struct VelocitySample
    {
        public float removeTime;
        public Vector3 velocity;

        public VelocitySample(Vector3 velocity, float time)
        {
            this.velocity = velocity;
            this.removeTime = time;
        }
    }

    private void TrackThrowingVelocities()
    {
        _velocityQueue.Enqueue(new VelocitySample(_rigidbody.velocity,
                                                  Time.time + VELOCITY_HISTORY_LENGTH));

        while (true)
        {
            VelocitySample oldestVelocity = _velocityQueue.Peek();

            // Dequeue conservatively
            if (Time.time > oldestVelocity.removeTime)
            {
                _velocityQueue.Dequeue();
            }
            else
            {
                break;
            }
        }
    }

    private void ThrowingOnRelease()
    {
        Vector3 averageVelocity = Vector3.zero;

        int velocityCount = 0;

        while (_velocityQueue.Count > 0)
        {
            VelocitySample oldestVelocity = _velocityQueue.Dequeue();

            averageVelocity += oldestVelocity.velocity;
            velocityCount++;
        }

        // average the frames to get the average positional change
        averageVelocity /= velocityCount;

        if (averageVelocity.magnitude > 0.5f)
        {
            // Set the new velocty. Allow physics to solve for rotational change
            _rigidbody.velocity = averageVelocity;
        }
        else
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }
    private const float MAX_VELOCITY_SQUARED = 100;


    private void PhysicsMovement(Vector3 solvedPosition, Quaternion solvedRotation)
    {
        Vector3 solvedCenterOfMass = solvedRotation * _rigidbody.centerOfMass + solvedPosition;
        Vector3 currCenterOfMass = _rigidbody.rotation * _rigidbody.centerOfMass + _rigidbody.position;

        Vector3 targetVelocity = ToLinearVelocity(currCenterOfMass, solvedCenterOfMass, Time.fixedDeltaTime);
        Vector3 targetAngularVelocity = ToAngularVelocity(_rigidbody.rotation, solvedRotation, Time.fixedDeltaTime);

        // Clamp targetVelocity by MAX_VELOCITY_SQUARED.
        float targetSpeedSqrd = targetVelocity.sqrMagnitude;
        if (targetSpeedSqrd > MAX_VELOCITY_SQUARED)
        {
            float targetPercent = MAX_VELOCITY_SQUARED / targetSpeedSqrd;
            targetVelocity *= targetPercent;
        }

        _rigidbody.velocity = targetVelocity;
        if (IsValid(targetAngularVelocity))
        {
            _rigidbody.angularVelocity = targetAngularVelocity;
        }
    }

    public Vector3 ToLinearVelocity(Vector3 deltaPosition, float deltaTime)
    {
        return deltaPosition / deltaTime;
    }

    public Vector3 ToLinearVelocity(Vector3 startPosition, Vector3 destinationPosition, float deltaTime)
    {
        return ToLinearVelocity(destinationPosition - startPosition, deltaTime);
    }

    public Vector3 ToAngularVelocity(Quaternion deltaRotation, float deltaTime)
    {
        Vector3 deltaAxis;
        float deltaAngle;
        deltaRotation.ToAngleAxis(out deltaAngle, out deltaAxis);

        if (float.IsInfinity(deltaAxis.x))
        {
            deltaAxis = Vector3.zero;
            deltaAngle = 0;
        }

        if (deltaAngle > 180)
        {
            deltaAngle -= 360.0f;
        }

        return deltaAxis * deltaAngle * Mathf.Deg2Rad / deltaTime;
    }

    public Vector3 ToAngularVelocity(Quaternion startRotation, Quaternion destinationRotation, float deltaTime)
    {
        return ToAngularVelocity(destinationRotation * Quaternion.Inverse(startRotation), deltaTime);
    }

    public bool IsValid(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z)) && !(float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
}
