using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
   public Slider m_Slider;
    public void SetBGMVolume()
    {
      AudioManager.Instance.audioSource.volume = m_Slider.value;
    }

    public void SetSEVolume()
    {
       AudioManager.Instance.SEaudioSource.volume = m_Slider.value;
    }

    public void SetVoiceVolume()
    {
       AudioManager.Instance.voiceSource.volume = m_Slider.value;
    }
}
