using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorChooser : MonoBehaviour
{
    [SerializeField] List<Transform> m_armorList;

    public void ChangeArmorTo(int armorIndex)
    {
        foreach(Transform armor in m_armorList)
        {
            if(armor != m_armorList[armorIndex])
            {
                armor.gameObject.SetActive(false);
                continue;
            }

            armor.gameObject.SetActive(true);
        }
    }
}
