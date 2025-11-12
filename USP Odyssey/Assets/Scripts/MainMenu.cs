using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioSource buttonClickSound; // Reference to the AudioSource component

    public void Start(){
        AudioListener.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
    }
    public void playGame()
    {
        PlaySound();  // Play the sound when the button is pressed
        SceneManager.LoadSceneAsync(1);
    }
    public void goMenu()
    {
        PlaySound();  // Play the sound when the button is pressed
        SceneManager.LoadSceneAsync(0);
    }
    public void quitGame()
    {
        PlaySound();  // Play the sound when the button is pressed
        Application.Quit();
    }

    public void PlaySound()
    {
        if (buttonClickSound != null) // Check if AudioSource is assigned
        {
            buttonClickSound.Play();
        }
    }
}
