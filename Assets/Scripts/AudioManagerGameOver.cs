using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManagerGameOver : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip gameOverClip;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 0.25f;

        StartCoroutine(PlayGameOverLoop());
    }

    IEnumerator PlayGameOverLoop()
    {
        while (true)
        {
            audioSource.clip = gameOverClip;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(gameOverClip.length + 5f);
        }
    }
}
