using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquishTap : MonoBehaviour
{
    [SerializeField] private Microgesture _thumbSlider;
    [SerializeField] private DoubleTapDetector _tapDetector;
    [SerializeField] private GameObject _imageCanvas;

    [SerializeField] private bool _useReactivity = true;

    private Vector3 _originalScale, _contactScale, _tappedScale;

    void Start()
    {
        _thumbSlider.OnContactStart.AddListener(OnContactStart);
        _tapDetector.OnDoubleTap.AddListener(OnTapped);

        _originalScale = _imageCanvas.transform.localScale;
        _contactScale = _originalScale * 0.75f;
        _tappedScale = _originalScale * 1.5f;
    }

    private void OnTapped()
    {
        _imageCanvas.transform.DOScale(_tappedScale, 0.2f).onComplete += () => { _imageCanvas.transform.DOScale(_originalScale, 0.5f); };
    }

    private void OnContactStart(float arg0)
    {
        if (!_useReactivity) 
            return;

        _imageCanvas.transform.DOScale(_contactScale, 0.2f);
    }
}
