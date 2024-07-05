using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColourPickerSlider : MonoBehaviour
{
    [Header("Config")]
    public float lerpTime = 0.2f;

    [Header("Setup")]
    [SerializeField] private Image[] _images = null;
    [SerializeField] private Slider _slider;
    [SerializeField] private Transform _handle;
    [SerializeField] private SlideAccelerationGenerator _accelerationGenerator;

    private Microgesture _thumbSlider;
    private Image _selectedImage;
    private float _targetValue;
    private bool _contact = false;
    private Vector3 handleScale;

    private bool _interactionEnabled = false;
    

    private void Start()
    {
        _thumbSlider = FindFirstObjectByType<Microgesture>();

        _thumbSlider.OnContactStart.AddListener(StartContact);
        _thumbSlider.OnContactEnd.AddListener(StopContact);

        _accelerationGenerator.OnAcceleration.AddListener(OnAcceleration);
        _targetValue = _accelerationGenerator.NormalizedValue;
        handleScale = _handle.localScale;
        _interactionEnabled = true;
    }

    private void StartContact(float dist)
    {
        if (!_interactionEnabled) return;
        _contact = true;
        _handle.localScale = handleScale * 1.25f;
    }

    private void StopContact(float dist)
    {
        if (!_interactionEnabled) return;
        if (_contact)
        {
            _targetValue = Mathf.Round(_targetValue * (_images.Length - 1)) / (_images.Length - 1);
            _accelerationGenerator.SetValue(_targetValue);
        }
        _contact = false;
        _handle.localScale = handleScale;
    }

    private void OnAcceleration(float normalizedValue01)
    {
        if (!_interactionEnabled) return;

        if (!_contact)
            return;
        _targetValue = normalizedValue01;
    }

    private void Update()
    {
        _slider.value = Mathf.Lerp(_slider.value, _targetValue, Time.deltaTime * (1f / lerpTime));
        UpdateUI();
    }

    private void UpdateUI()
    {
        float dist = float.MaxValue, tempdist;
        Image tempImage = null;

        foreach (var image in _images)
        {
            tempdist = Vector3.Distance(image.transform.position, _handle.transform.position);
            if (tempdist < dist)
            {
                dist = tempdist;
                tempImage = image;
            }
        }

        if (tempImage != _selectedImage)
        {
            _selectedImage = tempImage;
            foreach (var image in _images)
            {
                if (image == _selectedImage)
                {
                    image.transform.localScale = Vector3.one * 1.375f;
                }
                else
                {
                    image.transform.localScale = Vector3.one;
                }
            }

        }
    }


}
