using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasicSlider : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float _lerpTime = 0.2f;

    [Header("Setup")]
    [SerializeField] private Slider _slider;
    [SerializeField] private Transform _handle;
    [SerializeField] private SlideAccelerationGenerator _accelerationGenerator;
    [SerializeField] private TextMeshProUGUI _text;

    private Microgesture _thumbSlider;
    private float _targetValue;
    private bool _contact = false;
    private Vector3 _handleScale;

    private void Start()
    {
        _thumbSlider = FindFirstObjectByType<Microgesture>();

        _thumbSlider.OnContactStart.AddListener(StartContact);
        _thumbSlider.OnContactEnd.AddListener(StopContact);

        _accelerationGenerator.OnAcceleration.AddListener(OnAcceleration);
        _targetValue = _accelerationGenerator.NormalizedValue;
        _handleScale = _handle.localScale;
    }

    private void StartContact(float dist)
    {
        _contact = true;
        _handle.localScale = _handleScale * 1.25f;
    }

    private void StopContact(float dist)
    {
        _contact = false;
        _handle.localScale = _handleScale;
    }

    private void OnAcceleration(float normalizedValue01)
    {
        if (!_contact)
            return;

        _targetValue = normalizedValue01;
    }

    private void Update()
    {
        _slider.value = Mathf.Lerp(_slider.value, _targetValue, Time.deltaTime * (1f / _lerpTime));

        if(_text != null)
            _text.text = _slider.value.ToString("0.00");
    }
}
