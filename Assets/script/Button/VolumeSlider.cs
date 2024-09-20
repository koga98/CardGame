using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public Slider m_Slider;
    void Start(){
        m_Slider.value = 0.5f;
    }
    public void SetBGMVolume(float volume)
    {
       AudioManager.Instance.audioSource.volume = m_Slider.value;
    }

    public void SetSEVolume(float volume)
    {
       AudioManager.Instance.SEaudioSource.volume = m_Slider.value;
    }

    public void SetVoiceVolume(float volume)
    {
       AudioManager.Instance.voiceSource.volume = m_Slider.value;
    }
}
