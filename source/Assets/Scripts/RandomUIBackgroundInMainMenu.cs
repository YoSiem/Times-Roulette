using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomUIBackgroundInMainMenu : MonoBehaviour
{
 public Sprite[] sprites;
 public Image image;

    void OnEnable()
    {
    int randomIndex = Random.Range(0, sprites.Length);
    image.sprite = sprites[randomIndex];
    }


}
