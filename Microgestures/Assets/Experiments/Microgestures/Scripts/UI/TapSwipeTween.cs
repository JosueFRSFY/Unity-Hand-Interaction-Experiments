using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapSwipeTween : MonoBehaviour
{
    [SerializeField] private Microgesture _thumbSlider;
    [SerializeField] private TapDetector _tapDetector;
    [SerializeField] private SwipeDetector _swipeDetector;

    [SerializeField] private GameObject _imageCanvas;

    private Vector3 _originalScale, _contactScale, _tappedScale;

    void Start()
    {
        _thumbSlider.OnContactStart.AddListener(OnContactStart);
        _tapDetector.OnTap.AddListener(OnTapped);
        _swipeDetector.OnSwiped.AddListener(OnSwiped);

        _originalScale = _imageCanvas.transform.localScale;
        _contactScale = _originalScale * 0.75f;
        _tappedScale = _originalScale * 1.5f;
    }

    private void OnSwiped(SwipeDirection direction, float velocity)
    {
        if (direction == SwipeDirection.LEFT)
        {
            _imageCanvas.transform.DOScale(_tappedScale, 0.2f).onComplete += ReturnToOriginalScale;
            _imageCanvas.transform.DORotate(new Vector3(0, 45f, 0), 0.2f);
        }
        else if (direction == SwipeDirection.RIGHT)
        {
            _imageCanvas.transform.DOScale(_tappedScale, 0.2f).onComplete += ReturnToOriginalScale;
            _imageCanvas.transform.DORotate(new Vector3(0, -45f, 0), 0.2f);
        }
    }

    private void OnTapped()
    {
        _imageCanvas.transform.DOScale(_tappedScale, 0.2f).onComplete += ReturnToOriginalScale;
    }

    private void OnContactStart(float arg0)
    {
        ReturnToOriginalRotation();
        _imageCanvas.transform.DOScale(_contactScale, 0.2f);
    }

    private void ReturnToOriginalScale()
    {
        _imageCanvas.transform.DOScale(_originalScale, 0.5f);
    }

    private void ReturnToOriginalRotation()
    {
        _imageCanvas.transform.DORotate(new Vector3(0, 0, 0), 0.2f);

    }
}
