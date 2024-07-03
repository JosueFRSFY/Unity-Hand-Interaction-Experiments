using Leap;
using Leap.Unity;
using Leap.Unity.PhysicalHands;
using Leap.Unity.Preview.HandRays;
using Leap.Unity.Preview.Locomotion;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using static SquishableObject;

public class SquishSummon : MonoBehaviour
{
    public HandRayInteractor rayInteractor;
    public LightweightGrabPoseDetector grabDetector;
    public Chirality chirality = Chirality.Right;

    private SquishableObject _activeObject;
    public float summonDistance = 0.1f;
    public float squishActivate = 0.85f;
    public Transform activeObjectTransformHelper;
    private Transform _palmTransformHelper;
    public LeapProvider _leapProvider;

    public GameObject _particlePrefab;
    public PhysicalHandsManager physicalHandsManager;
    private Hand _hand;

    // Start is called before the first frame update
    void Start()
    {
        if (_leapProvider == null)
        {
            _leapProvider = Hands.Provider;
        }

        if (physicalHandsManager == null)
        {
            physicalHandsManager = FindAnyObjectByType<PhysicalHandsManager>();
        }

        grabDetector.chirality = chirality;
        GetComponentInChildren<HandRay>().chirality = chirality;

        rayInteractor.OnRaycastUpdate += OnRaycastUpdate;
        grabDetector.OnGrab += OnGrab;
        grabDetector.OnGrabbing += OnGrabbing;
        grabDetector.OnUngrab += OnUngrab;
        _palmTransformHelper = new GameObject("SquishSummon - Palm TransformHelper").transform;
        activeObjectTransformHelper = new GameObject("SquishSummon - Active Object TransformHelper").transform;
        activeObjectTransformHelper.transform.parent = _palmTransformHelper.transform;
    }

    private void Update()
    {
        if (_leapProvider != null && _activeObject != null && _activeObject.Locked)
        {
            _hand = _leapProvider.GetHand(chirality);
            if (_hand != null)
            {
                _palmTransformHelper.position = _hand.PalmPosition;
                _palmTransformHelper.rotation = _hand.Rotation;
            }
        }
    }

    private void OnGrabbing(Hand hand)
    {
        if (hand.GetChirality() != rayInteractor.handRay.chirality) return;
        if (_activeObject == null) return;
        if (!_activeObject.Locked) return;
        if (_activeObject.owner != this) return;

        Vector3 scale = _activeObject.transform.localScale;
        if (_activeObject.squishState == SquishState.SQUISHING)
        {
            float squishAmount = Mathf.InverseLerp(grabDetector.activate, squishActivate, grabDetector.grabAmount);
            if (squishAmount >= 1)
            {
                activeObjectTransformHelper.position = hand.PalmPosition + (hand.PalmNormal * summonDistance);
                activeObjectTransformHelper.rotation = _activeObject.transform.rotation;
                Instantiate(_particlePrefab, _activeObject.originalPos, _activeObject.originalRot);
                _activeObject.SetState(SquishState.SQUISHED, this);
                return;
            }
            scale = Vector3.Lerp(_activeObject.originalScale, Vector3.zero, squishAmount);
        }
        else if (_activeObject.squishState == SquishState.SQUISHED)
        {
            float squishAmount = Mathf.InverseLerp(squishActivate, grabDetector.deactivate, grabDetector.grabAmount);
            if (squishAmount < 1)
            {
                _activeObject.SetState(SquishState.UNSQUISHING, this);
                return;
            }
        }
        else if (_activeObject.squishState == SquishState.UNSQUISHING)
        {
            float squishAmount = Mathf.InverseLerp(squishActivate, grabDetector.deactivate, grabDetector.grabAmount);
            scale = Vector3.Lerp(Vector3.zero, _activeObject.originalScale, squishAmount);
        }
        _activeObject.SetScale(scale);
    }

    private void OnGrab(Hand hand)
    {
        if (hand.GetChirality() != rayInteractor.handRay.chirality) return;

        if (_activeObject == null) return;
        if (_activeObject.owner != null) return;
        if (_activeObject.squishState != SquishState.HOVERED) return;


        _activeObject.SetState(SquishState.SQUISHING, this);
    }

    private void OnUngrab(Hand hand)
    {
        if (hand.GetChirality() != rayInteractor.handRay.chirality) return;
        if (_activeObject == null) return;
        if (_activeObject.owner != this) return;
        if (!_activeObject.Locked) return;

        if (_activeObject.squishState == SquishState.SQUISHING)
        {
            _activeObject.SetState(SquishState.HOVERED, this);
        }
        else if (_activeObject.squishState == SquishState.UNSQUISHING || _activeObject.squishState == SquishState.SQUISHED)
        {
            Instantiate(_particlePrefab, _activeObject.transform.position, _activeObject.transform.rotation);
            _activeObject.SetState(SquishState.SUMMONED, this);
        }
    }

    private bool IsContactHandInteracting()
    {
        ContactHand contactHand = chirality == Chirality.Left ? physicalHandsManager.ContactParent.LeftHand : physicalHandsManager.ContactParent.RightHand;
        return (contactHand.IsHovering || contactHand.IsGrabbing || contactHand.IsContacting);
    }

    private void OnRaycastUpdate(RaycastHit[] hits, RaycastHit primaryHit)
    {
        if (_activeObject != null && _activeObject.Locked && _activeObject.owner == this)
        {
            return;
        }

        bool contactHandInteracting = IsContactHandInteracting();
        if (contactHandInteracting)
        {
            if (_activeObject != null && !_activeObject.Locked && _activeObject.squishState != SquishState.IDLE)
            {
                _activeObject.SetState(SquishState.IDLE, this);
            }
            return;
        }

        if (hits != null && hits.Length != 0 && primaryHit.transform.gameObject.GetComponent<SquishableObject>() != null)
        {
            SquishableObject interactedObject = primaryHit.transform.gameObject.GetComponent<SquishableObject>();

            if (_activeObject == null)
            {
                _activeObject = interactedObject;
                _activeObject.SetState(SquishState.IDLE, this);
            }
            else
            {
                if (_activeObject != interactedObject)
                {
                    _activeObject.SetState(SquishState.IDLE, this);
                    _activeObject = interactedObject;
                }
                if (_activeObject.squishState != SquishState.HOVERED)
                {
                    _activeObject.SetState(SquishState.HOVERED, this);
                }
            }
        }
        else if (_activeObject != null)
        {
            _activeObject.SetState(SquishState.IDLE, this);
            _activeObject = null;
        }
    }

    public static float Vector3InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }
}


