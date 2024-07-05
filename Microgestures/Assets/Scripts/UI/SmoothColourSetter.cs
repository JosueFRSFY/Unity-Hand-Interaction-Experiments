using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmoothColourSetter : MonoBehaviour
{
    public Slider slider;
    public Image image;
    public Gradient gradient;
    public float lerpSpeed = 15f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        image.color = Color.Lerp(image.color, gradient.Evaluate(slider.value), Time.deltaTime * lerpSpeed);
    }
}
