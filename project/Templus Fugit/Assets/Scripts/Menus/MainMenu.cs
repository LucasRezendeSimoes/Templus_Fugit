using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // public GUISkin layout; // Fonte do placar

    private AudioSource _audioSource;

    void Start()
    {
        // Pega (ou adiciona) o AudioSource e toca a música
        _audioSource = GetComponent<AudioSource>();

        // Se você não marcou "Play On Awake" no Inspector:
        if (!_audioSource.playOnAwake)
        {
            _audioSource.loop = true;   // repete a faixa
            _audioSource.Play();        // inicia a música
        }
    }

    // void OnGUI()
    // {
    //     GUI.skin = layout;
    // }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Instructions()
    {
        SceneManager.LoadScene("Instructions");
    }

    public void Volume()
    {
        SceneManager.LoadScene("VolumeGame");
    }

    public void Credits()
    {
        SceneManager.LoadScene("Credits");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
