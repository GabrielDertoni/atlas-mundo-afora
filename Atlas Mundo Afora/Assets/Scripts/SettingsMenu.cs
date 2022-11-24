using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer m_AudioMixer;

    public void OnMasterVolumeChange(float volume)
    {
        m_AudioMixer.SetFloat("volume", volume);
    }
}
