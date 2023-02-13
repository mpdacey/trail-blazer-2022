using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchUISprite : MonoBehaviour
{
    public Sprite[] sprites;
    private Image image;

    public void SetSprite(int index)
    {
        image = GetComponent<Image>();
        if (index < sprites.Length)
            image.sprite = sprites[index];
    }
}
