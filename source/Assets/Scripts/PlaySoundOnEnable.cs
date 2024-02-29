using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnEnable : MonoBehaviour
{
    [SerializeField] AudioSource m_audioSource;
    [SerializeField] AudioClip m_soundToBePlayed;

    private void Start()
    {
        m_audioSource.clip = m_soundToBePlayed;
        m_audioSource.pitch = Random.Range(.7f, 1.3f);
        m_audioSource.Play();
    }
}
