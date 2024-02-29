using UnityEngine;
using TMPro;

public class DeathCountdown : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float startTimerValue;
    private float newTimer;

    void Start()
    {
        newTimer = startTimerValue;
    }

    private void OnEnable()
    {
        newTimer = startTimerValue;
    }

    void Update()
    {
        newTimer -= Time.deltaTime;

        if (newTimer <= 1f)
        {
            newTimer = startTimerValue;
        }

        timerText.text = ((int)newTimer).ToString();
    }
}
