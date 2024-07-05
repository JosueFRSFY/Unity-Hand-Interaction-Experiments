using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquishTap : MonoBehaviour
{
    public Microgesture thumbSlider;
    public DoubleTapDetector tapDetector;
    public GameObject imageCanvas;

    private Vector3 _originalScale, _contactScale, _tappedScale;

    public bool useReactivity = true;

    // Start is called before the first frame update
    void Start()
    {
        thumbSlider.OnContactStart.AddListener(OnContactStart);
        tapDetector.OnDoubleTap.AddListener(OnTapped);

        _originalScale = imageCanvas.transform.localScale;
        _contactScale = _originalScale * 0.75f;
        _tappedScale = _originalScale * 1.5f;
    }

    private void OnTapped()
    {
        imageCanvas.transform.DOScale(_tappedScale, 0.2f).onComplete += () => { imageCanvas.transform.DOScale(_originalScale, 0.5f); };
    }

    private void OnContactStart(float arg0)
    {
        if (!useReactivity) return;
        imageCanvas.transform.DOScale(_contactScale, 0.2f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
