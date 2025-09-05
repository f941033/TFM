using System;
using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip[] audioclips;

    // ==============   LISTA DE CLIPS  ==============
    // 0: MUSICA MENU PPAL
    // 1: FASE DE PREPARACIÓN
    // 2: FASE DE ACCIÓN
    //------------------------------------------------
    // 0: GAME OVER

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "GameOver")
        {
            audioSource.Stop();
            audioSource.volume = 0.25f;
            StartCoroutine("PlayGameOver");
        }
    }

    IEnumerator PlayGameOver()
    {
        while (true)
        {
            audioSource.PlayOneShot(audioclips[0]);
            yield return new WaitForSeconds(5);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayBackgroundSound(int index)
    {
        audioSource.Stop();
        if(index != 2) audioSource.volume = 0.2f;
        else audioSource.volume = 0.04f;
        audioSource.PlayOneShot(audioclips[index]);
    }


}
