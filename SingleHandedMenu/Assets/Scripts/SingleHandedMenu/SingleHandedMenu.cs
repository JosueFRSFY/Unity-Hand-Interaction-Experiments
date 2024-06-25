
using Leap.Unity.Interaction.Storage;
using System.Collections.Generic;
using System.Linq;
using Ultraleap;
using Ultraleap.Attachments;
using Ultraleap.PhysicalHands;
using UnityEngine;
using UnityEngine.Events;


public class SingleHandedMenu : MonoBehaviour
{
    /*
     * TODO:
     * - Set button correct chiralities
     * - Fix bug with tension line & hand showing/ not showing
     */

    [Header("Setup")]
    public GrabBall grabBall;
    public GameObject _uiParent;

    [SerializeField]
    private LeapProvider _leapProvider;
    
    [SerializeField]
    private LightweightGrabDetector _lightweightGrabDetector;
    
    [SerializeField]
    private SimpleFacingCameraCallbacks _palmFacingCamera;
    
    [SerializeField]
    private TappablePhysicalObject _tappableObject;

    [Header("Config")]
    public float detachDistanceConstraint = 0.17f;

    [HideInInspector] public bool attachedToHand = true;
    public bool isShowing { get { return _isShowing; } }
    private bool _isShowing = true;

    [Header("Events")]
    public UnityEvent OnAttachStart, OnDetachStart;
    public UnityEvent<float> OnAttaching, OnDetaching;
    public UnityEvent<bool> OnAttachEnd, OnDetachEnd;
    public UnityEvent OnMenuHide, OnMenuShow;

    private Transform _targetPosition;
    private Chirality _chirality;
    private Pose _objectLocalPose, _grabBallLocalPose;
    private bool _ungrabbedAfterStateChange = true;
    private bool _attachedOnGrab = false;
    private bool _lerpBackToTarget = false;
    private List<PhysicalHandsButton> _buttons;
    private float _attachedDistance = 0;
    private float _lerpSpeed = 10f;

    private void Awake()
    {
        if (grabBall == null)
        {
            grabBall = GetComponentInChildren<GrabBall>(true);
        }

        grabBall.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        _chirality = GetComponentInParent<AttachmentHand>().chirality;
        _buttons = GetComponentsInChildren<PhysicalHandsButton>(true).ToList();

        foreach(PhysicalHandsButton button in _buttons)
        {
            button.SetWhichHandCanActivateButtonPresses(_chirality == Chirality.Left ? ChiralitySelection.RIGHT : ChiralitySelection.LEFT);
        }

        _targetPosition = new GameObject("SingleHandedMenu_Target").transform;
        _targetPosition.parent = transform.parent;
        _targetPosition.position = transform.position;
        _targetPosition.rotation = transform.rotation;

        transform.parent = null;
        attachedToHand = true;

        _objectLocalPose.position = grabBall.attachedObject.transform.localPosition;
        _objectLocalPose.rotation = grabBall.attachedObject.transform.localRotation;

        _grabBallLocalPose.position = grabBall.transform.localPosition;
        _grabBallLocalPose.rotation = grabBall.transform.localRotation;

    }

    private void OnEnable()
    {
        if (_lightweightGrabDetector == null)
        {
            _lightweightGrabDetector = GetComponent<LightweightGrabDetector>();
        }
        _lightweightGrabDetector.OnGrab += OnGrab;
        _lightweightGrabDetector.OnUngrab += OnUngrab;

        if (_palmFacingCamera == null)
        {
            _palmFacingCamera = gameObject.GetComponentInParent<SimpleFacingCameraCallbacks>(true);
        }

        _palmFacingCamera.OnBeginFacingCamera.AddListener(OnStartFacingCamera);
        _palmFacingCamera.OnEndFacingCamera.AddListener(OnEndFacingCamera);

        if (_tappableObject == null)
        {
            _tappableObject = gameObject.GetComponentInChildren<TappablePhysicalObject>(true);
        }

        _tappableObject.OnTap.AddListener(OnTap);
    }

    private void OnDisable()
    {
        _lightweightGrabDetector.OnGrab -= OnGrab;
        _lightweightGrabDetector.OnUngrab -= OnUngrab;

        _palmFacingCamera.OnBeginFacingCamera.RemoveListener(OnStartFacingCamera);
        _palmFacingCamera.OnEndFacingCamera.RemoveListener(OnEndFacingCamera);
        _tappableObject.OnTap.RemoveListener(OnTap);
    }

    // Update is called once per frame
    void Update()
    {
        if (_leapProvider == null)
        {
            return;
        }

        Hand hand = _leapProvider.CurrentFrame.GetHand(_chirality);

        if (hand == null)
        {
            return;
        }

        // If the UI isn't shown
        if (!isShowing)
        {
            return;
        }

        if (attachedToHand)
        {
            AttachedStateLogic(hand);
        }
        else
        {
            DetachedStateLogic(hand);
        }
    }

    private void OnStartFacingCamera()
    {
        if (!attachedToHand) return; 

        if (!_isShowing)
        {
            ShowMenu();
        }
    }

    private void OnEndFacingCamera()
    {
        if (!attachedToHand)  return;

        if (_lightweightGrabDetector.IsGrabbing) return;

        if (_isShowing)
        {
            HideMenu();
        }
    }

    private void OnTap()
    {
        if (attachedToHand) return; 

        if (_isShowing)
        {
            HideMenu();
        }
        else
        {
            ShowMenu();
        }
    }

    private void OnGrab(Hand hand)
    {
        if (!_palmFacingCamera.IsFacingCamera) return;

        _attachedOnGrab = attachedToHand;

        if (attachedToHand)
        {
            _attachedDistance = Vector3.Distance(hand.PalmPosition, _uiParent.transform.position);

            OnDetachStart?.Invoke();
        }
        else
        {
            OnAttachStart?.Invoke();
        }
    }

    private void OnUngrab(Hand hand)
    {
        if (attachedToHand)
        {
            if (_attachedOnGrab && _ungrabbedAfterStateChange)
            {
                _lerpBackToTarget = true;
                OnDetachEnd?.Invoke(false);
            }

        }
        else
        {
            if (!_attachedOnGrab && _ungrabbedAfterStateChange)
            {
                OnAttachEnd?.Invoke(false);
            }
        }

        if (!_ungrabbedAfterStateChange)
        {
            _ungrabbedAfterStateChange = true;
        }
    }

    private void ShowMenu()
    {
        _isShowing = true;

        if (!attachedToHand)
        {
            grabBall.attachedObject.gameObject.SetActive(true);
        }

        _uiParent.gameObject.SetActive(true);
        OnMenuShow?.Invoke();
    }

    private void HideMenu()
    {
        _isShowing = false;

        if (!attachedToHand)
        {
            grabBall.attachedObject.gameObject.SetActive(false);
        }
        _uiParent.gameObject.SetActive(false);
        OnMenuHide?.Invoke();
    }

    private void AttachedStateLogic(Hand hand)
    {
        if (!_lightweightGrabDetector.IsGrabbing || !_ungrabbedAfterStateChange)
        {
            if (_lerpBackToTarget)
            {
                transform.position = Vector3.Lerp(transform.position, _targetPosition.position, _lerpSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, _targetPosition.rotation, _lerpSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, _targetPosition.position) < 0.005f)
                {
                    _lerpBackToTarget = false;
                }
            }
            else
            {
                transform.position = _targetPosition.position;
                transform.rotation = _targetPosition.rotation;
            }



            if (!_ungrabbedAfterStateChange) return;
        }

        if (_lightweightGrabDetector.IsGrabbing)
        {
            float distanceDelta = Vector3.Distance(hand.PalmPosition, _uiParent.transform.position) - _attachedDistance;
            if (distanceDelta > detachDistanceConstraint)
            {
                //DETACH
                attachedToHand = false;
                grabBall.gameObject.SetActive(true);
                _ungrabbedAfterStateChange = false;

                foreach (PhysicalHandsButton button in _buttons)
                {
                    button.SetWhichHandCanActivateButtonPresses(ChiralitySelection.BOTH);
                }
                OnDetachEnd?.Invoke(true);
            }
            else
            {
                float detachT = distanceDelta / detachDistanceConstraint;
                OnDetaching?.Invoke(detachT);
            }
        }
    }

    private void DetachedStateLogic(Hand hand)
    {
        if (!_lightweightGrabDetector.IsGrabbing || !_ungrabbedAfterStateChange) return;

        float distanceDelta = Vector3.Distance(hand.PalmPosition, _uiParent.transform.position);
        float attachT = Unity.Mathematics.math.remap(_attachedDistance, _attachedDistance + detachDistanceConstraint, 1, 0, distanceDelta);

        if (attachT >= 0.95f)
        {
            //ATTACH

            attachedToHand = true;
            grabBall.attachedObject.transform.localPosition = _objectLocalPose.position;
            grabBall.attachedObject.transform.localRotation = _objectLocalPose.rotation;

            grabBall.transform.localPosition = _grabBallLocalPose.position;
            grabBall.transform.localRotation = _grabBallLocalPose.rotation;

            grabBall.gameObject.SetActive(false);

            _ungrabbedAfterStateChange = false;

            foreach (PhysicalHandsButton button in _buttons)
            {
                button.SetWhichHandCanActivateButtonPresses(_chirality == Chirality.Left ? ChiralitySelection.RIGHT : ChiralitySelection.LEFT);
            }

            OnAttachEnd?.Invoke(true);
        }
        else
        {
            OnAttaching?.Invoke(attachT);
        }
    }
}
