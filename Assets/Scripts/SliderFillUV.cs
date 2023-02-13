using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderFillUV : MonoBehaviour
{
    private RectTransform rectTransform;
    private RawImage image;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<RawImage>();
    }

    public void UpdateSliderUV()
    {
        image.uvRect = new Rect(0,0, rectTransform.anchorMax.x, 1);
    }
}
