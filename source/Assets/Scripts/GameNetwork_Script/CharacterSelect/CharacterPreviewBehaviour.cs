using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPreviewBehaviour : MonoBehaviour
{ 
    [SerializeField] private List<Transform> m_armorList;

    private int m_armorIndex = 0;

    private void Start()
    {
        DisableAllArmors();
        EnableDefaultArmor();
    }

    void DisableAllArmors()
    {
        foreach(Transform armor in m_armorList)
        {
            armor.gameObject.SetActive(false);
        }
    }

    void EnableDefaultArmor()
    {

        m_armorList[m_armorIndex].gameObject.SetActive(true);
    }

    void DisableArmorAtIndex(int armorIndex)
    {
        m_armorList[armorIndex].gameObject.SetActive(false);
    }

    void EnableArmorAtIndex(int armorIndex)
    {
        m_armorList[armorIndex].gameObject.SetActive(true);
    }

    public void OnRightButtonClick() => ShowNextArmor();

    public void OnLeftButtonClick() => ShowPreviousArmor();

    public int OnSelectButtonClick() => GetCurrentArmorIndex();

    void ShowNextArmor()
    {
        DisableArmorAtIndex(m_armorIndex);

        m_armorIndex = (m_armorIndex + 1) % m_armorList.Count;

        EnableArmorAtIndex(m_armorIndex);
    }

    void ShowPreviousArmor()
    {
        DisableArmorAtIndex(m_armorIndex);

        m_armorIndex--;

        if(m_armorIndex < 0)
        {
            m_armorIndex = m_armorList.Count - 1;
        }

        EnableArmorAtIndex(m_armorIndex);
    }

    public int GetCurrentArmorIndex()
    {
        return m_armorIndex;
    }

    public string GetCurrentArmorName()
    {
        return m_armorList[m_armorIndex].gameObject.name;
    }
}
