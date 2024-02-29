using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LimitStringLength : MonoBehaviour
{
    [SerializeField] byte m_maxStringLength;

    [SerializeField] TMP_InputField m_textToClamp;

    public void ClampText()
    {
        if(m_textToClamp.text.Length > m_maxStringLength)
        {
            m_textToClamp.text = m_textToClamp.text.Substring(0, m_maxStringLength);
        }
    }
}
