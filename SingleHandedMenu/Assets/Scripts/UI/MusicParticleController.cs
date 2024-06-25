using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicParticleController : MonoBehaviour
{
    public MusicPlayerController musicPlayer;
    public SingleHandedMenu _handMenu;

    public ParticleSystem _attachedSystem, _detachedSystem;

    private void OnEnable()
    {
        _handMenu.OnMenuShow.AddListener(OnMenuShow);
        _handMenu.OnMenuHide.AddListener(OnMenuHide);
    }

    private void OnDisable()
    {
        _handMenu.OnMenuShow.RemoveListener(OnMenuShow);
        _handMenu.OnMenuHide.RemoveListener(OnMenuHide);
    }

    private void OnMenuHide()
    {
        if (musicPlayer.isPlaying)
        {
            if (_handMenu.attachedToHand)
            {
                _attachedSystem.Play();
            }
            else
            {
                _detachedSystem.Play();
            }
        }
    }

    private void OnMenuShow()
    {
        if (_attachedSystem.isPlaying)
        {
            _attachedSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (_detachedSystem.isPlaying)
        {
            _detachedSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
