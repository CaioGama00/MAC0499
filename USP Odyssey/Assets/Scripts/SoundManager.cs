using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;

    void Start()
    {
        // Load saved volume or default to 1 if no value exists
        float savedVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
        volumeSlider.value = savedVolume;

        // Add listener to save volume whenever the slider value changes
        volumeSlider.onValueChanged.AddListener(ChangeVolume);
    }

    public void ChangeVolume(float newVolume)
    {
        AudioListener.volume = newVolume;
        SaveVolume(newVolume);
    }

    private void SaveVolume(float volume)
    {
        PlayerPrefs.SetFloat("musicVolume", volume);
        PlayerPrefs.Save(); // Ensure changes are saved immediately
    }
}
