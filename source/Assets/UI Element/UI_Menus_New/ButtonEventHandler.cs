using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEventHandler : MonoBehaviour
{
    [SerializeField] GameObject m_settingsMenu;

    public void OnSettingsButtonClicked()
    {
        if(m_settingsMenu == null)
        {
            m_settingsMenu = GameObject.FindGameObjectWithTag("OptionsMenu");
        }

        m_settingsMenu.GetComponent<SettingsMenu>().ShowMenu();
    }
}
