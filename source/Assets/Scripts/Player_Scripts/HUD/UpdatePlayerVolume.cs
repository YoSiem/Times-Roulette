using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UpdatePlayerVolume : MonoBehaviour
{
    public void SetFilmgrainIntensity(float intensity)
    {
        FilmGrain filmGrain;
        gameObject.GetComponent<Volume>().profile.TryGet<FilmGrain>(out filmGrain);
        filmGrain.intensity.Override(intensity);
    }
}
