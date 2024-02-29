using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] TMP_Dropdown m_resolutionDropdown;

    [SerializeField] AudioMixer m_audioMixer;
    [SerializeField] float m_startingVolume;

    Resolution[] resolutions;

    static bool instanceExists = false;

     void Start ()
     {
        if(instanceExists)
        {
            Destroy(gameObject);
        }

        instanceExists = true;
        DontDestroyOnLoad(gameObject);

         resolutions = Screen.resolutions;

         m_resolutionDropdown.ClearOptions();

         List<string> options = new List<string> ();

         int currentResolutionIndex = 0;
         for (int i=0; i < resolutions.Length; i++)
         {
             string option = resolutions[i].width + " x " + resolutions[i].height;
             options.Add(option);

             if (resolutions[i].width == Screen.currentResolution.width &&
                 resolutions[i].height == Screen.currentResolution.height)
             {
                 currentResolutionIndex = i;
             }
           }

         m_resolutionDropdown.AddOptions(options);
         m_resolutionDropdown.value = currentResolutionIndex;
         m_resolutionDropdown.RefreshShownValue();

        SetSoundVolumeDefault();

     }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            HideMenu();
        }
    }

    public void HideMenu()
    {
        GetComponent<Canvas>().enabled = false;

        var selectables = GetComponentsInChildren<Selectable>();
        foreach (var selectable in selectables) 
        {
            selectable.interactable = false;
        }

        enabled = false;
    }

    public void ShowMenu()
    {
        GetComponent<Canvas>().enabled = true;

        var selectables = GetComponentsInChildren<Selectable>();
        foreach (var selectable in selectables)
        {
            selectable.interactable = true;
        }

        enabled = true;
    }

    #region Video-Settings
    public void SetResolution (int resolutionIndex)
      { 
         Resolution resolution = resolutions [resolutionIndex];
         Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
       }

    public void SetQuality (int qualityIndex)
     {
         QualitySettings.SetQualityLevel(qualityIndex);
      }

    public void SetFullscreen (bool isFullscreen)
     {
         Screen.fullScreen = isFullscreen;
      }
    #endregion

    #region Sound-Settings

    public void AdjustMasterVolume(float masterLvl)
    {
        m_audioMixer.SetFloat("masterVolume", masterLvl);
    }

    public void AdjustSfxVolume(float sfxLvl)
    {
        m_audioMixer.SetFloat("sfxVolume", sfxLvl);
    }

    public void AdjustMusicVolume(float musicLvl)
    {
        m_audioMixer.SetFloat("musicVolume", musicLvl);
    }

    void SetSoundVolumeDefault()
    {
        AdjustMasterVolume(m_startingVolume);
        AdjustSfxVolume(m_startingVolume);
        AdjustMusicVolume(m_startingVolume);
    }

    #endregion

}
