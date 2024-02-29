using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDColorChanger : MonoBehaviour
{
    [SerializeField] List<Image> healthRelatedHUD;

    [SerializeField] Color fullHpColor;

    [SerializeField] Color lowHpColor;

    // Start is called before the first frame update
    void Start()
    {
        foreach(Image hudElement in healthRelatedHUD)
        {
            hudElement.color = fullHpColor;
        }
    }

    public void UpdateHUDColor(float healthRatio)
    {
        foreach(Image hudElement in healthRelatedHUD)
        {
            hudElement.color = Color.Lerp(lowHpColor, fullHpColor, healthRatio);
        }
    }
}
