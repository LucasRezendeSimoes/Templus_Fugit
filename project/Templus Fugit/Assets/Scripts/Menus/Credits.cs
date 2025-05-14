using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Credits : MonoBehaviour
{
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadMainMenu(){
        SceneManager.LoadScene("MainMenu");
    }
}
