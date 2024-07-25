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

    [SerializeField] private bool _usePoseToActivate = true;

    public Chirality Chirality;

    [SerializeField] private MicrogestureContactDetector _contactDetector;
    [SerializeField] private MicrogestureSlideDetector _slideDetector;

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
        if (_usePoseToActivate)
        {
            State = MicrogestureState.IDLE;
        }
        else
        {
            State = MicrogestureState.READY;
        }

        UpdateSystemChirality(Chirality);

        _contactDetector.Processing = !_usePoseToActivate;
        _slideDetector.Processing = !_usePoseToActivate;
    }

    private void OnEnable()
    {
        _contactDetector.OnContactStateChange += ContactStateChange;
    }

    private void OnDisable()
    {
        _contactDetector.OnContactStateChange -= ContactStateChange;
    }

    private void PoseStateChange(bool result, float val)
    {
        if (!_usePoseToActivate)
        {
            result = true;
        }

        if (result)
        {
            SetState(MicrogestureState.READY);
            _contactDetector.Processing = true;
            _slideDetector.Processing = true;
        }
        else
        {
            SetState(MicrogestureState.IDLE);
            _contactDetector.Processing = false;
            _slideDetector.Processing = false;
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
        _contactDetector.Chirality = chirality;
        _slideDetector.Chirality = chirality;
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
