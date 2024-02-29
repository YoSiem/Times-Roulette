using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorChooserFirstPerson : MonoBehaviour
{
    [SerializeField] GameObject m_fpsTop;
    [SerializeField] GameObject m_fpsBottom;

    public void ChangeArmorTo(int armorIndex)
    {
        m_fpsTop.GetComponent<ArmorChooser>().ChangeArmorTo(armorIndex);
        m_fpsBottom.GetComponent<ArmorChooser>().ChangeArmorTo(armorIndex);
    }
}
