using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundControl : MonoBehaviour
{
    [SerializeField] string _volumeName;
    [SerializeField] AudioMixer _mixer;
    [SerializeField] Slider _slider;
    [SerializeField] float _multiplier = 30f;

    private void Awake()
    {
        _slider.onValueChanged.AddListener(HandleSliderValueChanged);
    }
    private void HandleSliderValueChanged(float value)
    {
        _mixer.SetFloat(_volumeName, Mathf.Log10(value) * _multiplier);
    }
}
