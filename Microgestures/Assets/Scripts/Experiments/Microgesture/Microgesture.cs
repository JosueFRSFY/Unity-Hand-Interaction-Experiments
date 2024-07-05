using Leap.Unity;
using System;
using UnityEngine;
using UnityEngine.Events;

public class Microgesture : MonoBehaviour
{
    public enum MicrogestureState { IDLE, READY, CONTACTING }

    /// <summary>
    /// Current state of the Microgesture System
    /// IDLE = hand not in correct microgesture pose
    /// READY = hand in correct pose, but not contacting
    /// CONTACTING = hand in correct pose, and contacting
    /// </summary>
    public MicrogestureState State { get; private set; }


    public bool usePoseToActivate = true;

    public Chirality chirality;

    [SerializeField]
    private MicrogestureContactDetector _contactDetector;
    [SerializeField]
    private MicrogestureSlideDetector _slideDetector;


    [Tooltip("Called when the thumb touches the index finger, with the hand in a valid pose")]
    public UnityEvent<float> OnContactStart;

    [Tooltip("Called when the thumb stops touching the index finger, or if the hand leaves a valid pose")]
    public UnityEvent<float> OnContactEnd;

    [Tooltip("Called as the thumb slides along the index finger")]
    public UnityEvent<float> OnSlide;

    public float SlideValue { get { return Mathf.Clamp01(_slideDetector.SlideValue); } }

    private bool _contact;

    public Action<MicrogestureState> OnStateChanged;

    private void Awake()
    {
        if (usePoseToActivate)
        {
            State = MicrogestureState.IDLE;
        }
        else
        {
            State = MicrogestureState.READY;
        }

        UpdateSystemChirality(chirality);

        //_poseGesture.processing = usePoseToActivate;
        _contactDetector.processing = !usePoseToActivate;
        _slideDetector.processing = !usePoseToActivate;
    }

    private void OnEnable()
    {
        //_poseGesture.OnStateChange += PoseStateChange;
        _contactDetector.OnContactStateChange += ContactStateChange;
    }

    private void OnDisable()
    {
        //_poseGesture.OnStateChange -= PoseStateChange;
        _contactDetector.OnContactStateChange -= ContactStateChange;
    }

    private void PoseStateChange(bool result, float val)
    {
        if (!usePoseToActivate)
        {
            result = true;
        }

        if (result)
        {
            SetState(MicrogestureState.READY);
            _contactDetector.processing = true;
            _slideDetector.processing = true;
        }
        else
        {
            SetState(MicrogestureState.IDLE);
            _contactDetector.processing = false;
            _slideDetector.processing = false;
        }
    }

    private void ContactStateChange(bool result)
    {
        _contact = result;
    }

    private void Update()
    {
        if (State == MicrogestureState.READY)
        {
            // On Contact Start
            if (_contact && _slideDetector.SlideValue > 0 && _slideDetector.SlideValue < 1)
            {
                SetState(MicrogestureState.CONTACTING);
                OnContactStart.Invoke(SlideValue);
            }
        }
        else if (State == MicrogestureState.CONTACTING)
        {
            // On Contact End
            if (!_contact || _slideDetector.SlideValue < 0 || _slideDetector.SlideValue > 1)
            {
                SetState(MicrogestureState.READY);
                OnContactEnd.Invoke(SlideValue);
            }
            else
            {
                OnSlide.Invoke(SlideValue);
            }
        }
    }

    public void UpdateSystemChirality(Chirality chirality)
    {
        //_poseGesture.chirality = chirality;
        _contactDetector.chirality = chirality;
        _slideDetector.chirality = chirality;
    }

    private void SetState(MicrogestureState newState)
    {
        if (newState != State)
        {
            OnStateChanged?.Invoke(newState);

            if (State == MicrogestureState.CONTACTING && newState == MicrogestureState.IDLE)
            {
                OnContactEnd.Invoke(SlideValue);
            }
        }

        State = newState;
    }
}
