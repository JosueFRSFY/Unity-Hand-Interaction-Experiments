using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapSwipeTween : MonoBehaviour
{

    public Microgesture thumbSlider;
    public TapDetector tapDetector;
    public SwipeDetector swipeDetector;
    public GameObject imageCanvas;

    private Vector3 _originalScale, _contactScale, _tappedScale;

    // Start is called before the first frame update
    void Start()
    {
        thumbSlider.OnContactStart.AddListener(OnContactStart);
        tapDetector.OnTap.AddListener(OnTapped);
        swipeDetector.OnSwiped.AddListener(OnSwiped);

        _originalScale = imageCanvas.transform.localScale;
        _contactScale = _originalScale * 0.75f;
        _tappedScale = _originalScale * 1.5f;
    }

    private void OnSwiped(SwipeDirection direction, float velocity)
    {
        if (direction == SwipeDirection.LEFT)
        {
            imageCanvas.transform.DOScale(_tappedScale, 0.2f).onComplete += ReturnToOriginalScale;
            imageCanvas.transform.DORotate(new Vector3(0, 45f, 0), 0.2f);
        }
        else if (direction == SwipeDirection.RIGHT)
        {
            imageCanvas.transform.DOScale(_tappedScale, 0.2f).onComplete += ReturnToOriginalScale;
            imageCanvas.transform.DORotate(new Vector3(0, -45f, 0), 0.2f);
        }
    }

    private void OnTapped()
    {
        imageCanvas.transform.DOScale(_tappedScale, 0.2f).onComplete += ReturnToOriginalScale;
    }

    private void OnContactStart(float arg0)
    {
        ReturnToOriginalRotation();
        imageCanvas.transform.DOScale(_contactScale, 0.2f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ReturnToOriginalScale()
    {
        imageCanvas.transform.DOScale(_originalScale, 0.5f);
    }

    private void ReturnToOriginalRotation()
    {
        imageCanvas.transform.DORotate(new Vector3(0, 0, 0), 0.2f);

    }
}
