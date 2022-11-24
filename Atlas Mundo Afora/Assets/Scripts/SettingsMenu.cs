using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer m_MasterAudioMixer;
    [SerializeField] private AudioMixer m_MusicAudioMixer;

    [SerializeField] private Slider m_MasterAudioSlider;
    [SerializeField] private Slider m_MusicAudioSlider;

    public void OnMasterVolumeChange()
    {
        m_MasterAudioMixer.SetFloat("volume", m_MasterAudioSlider.value);
    }

    public void OnMusicVolumeChange()
    {
        m_MusicAudioMixer.SetFloat("volume", m_MusicAudioSlider.value);
    }
}
