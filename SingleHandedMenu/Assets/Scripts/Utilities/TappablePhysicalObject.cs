using System;
using Ultraleap.PhysicalHands;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Get tap events from objects being interacted with PhysicalHands
/// </summary>
/// 
[RequireComponent(typeof(PhysicalHandEvents))]
public class TappablePhysicalObject : MonoBehaviour
{
    /// <summary>
    /// Called when a TappableInteractionBehaviour is tapped
    /// Useful for activation/deactivation.
    /// </summary>
    public UnityEvent OnTap;

    [SerializeField]
    [Tooltip("When ticked, the object will only register taps when it is primary hovered. Only one object can be primary hovered at a time by either hand.")]
    private bool _usePrimaryHover;

    // We use a timeout after grasping rather than simply tapping when contact exits after no grabs, as accidental contact can happen after ungrasping an object.
    protected float _timeoutAfterGrasp = 0.25f;
    protected float _timeLastGrasped = -1;

    /// <summary>
    /// The position of the storage when it was grasped
    /// </summary>
    protected Vector3 _positionOnContact;

    protected PhysicalHandEvents handEvents;
    protected PhysicalHandsManager physicalHandManager;

    // Start is called before the first frame update
    protected void OnEnable()
    {
        if (handEvents == null)
        {
            handEvents = GetComponent<PhysicalHandEvents>();
            if (handEvents == null)
            {
                handEvents = gameObject.AddComponent<PhysicalHandEvents>();
            }

            handEvents.usePrimaryHover = _usePrimaryHover;
        }

        handEvents.onContactExit.AddListener(CheckForTap);
        handEvents.onGrabExit.AddListener(SetTimeLastGrasped);
    }

    protected void OnDisable()
    {
        handEvents.onContactExit.RemoveListener(CheckForTap);
        handEvents.onGrabExit.RemoveListener(SetTimeLastGrasped);
    }

    protected void CheckForTap(ContactHand hand)
    {

        // If other controllers are still in contact, return
        if (handEvents.anyHandGrabbing || handEvents.anyHandContacting)
        {
            return;
        }


        if (Time.time - _timeLastGrasped > _timeoutAfterGrasp)
        {
            OnTap?.Invoke();
        }
    }

    protected virtual void SetTimeLastGrasped(ContactHand hand)
    {
        _timeLastGrasped = Time.time;
    }
}
