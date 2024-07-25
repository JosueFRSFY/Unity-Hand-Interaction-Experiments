using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmoothColourSetter : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private Image _image;
    [SerializeField] private Gradient _gradient;
    [SerializeField] private float _lerpSpeed = 15f;

    void Update()
    {
        _image.color = Color.Lerp(_image.color, _gradient.Evaluate(_slider.value), Time.deltaTime * _lerpSpeed);
    }
}
