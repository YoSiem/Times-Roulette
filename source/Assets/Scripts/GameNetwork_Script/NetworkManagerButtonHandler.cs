using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerButtonHandler : MonoBehaviour
{
    public void OnPlayButtonPressed()
    {
        NetworkManagerGame.instance.OnPlayButtonPressed();
    }

    public void OnBackToMainMenuButtonPressed()
    {
        NetworkManagerGame.instance.OnBackToMainMenuButtonPressedClient();
    }
}
