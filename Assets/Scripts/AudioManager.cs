using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip clipBeep, clipBackgroundSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayBackgroundSound()
    {
        audioSource.Play();
    }

    public void PauseBackgroundSound()
    {
        audioSource.Pause();
    }

    public void UnPauseBackgroundSound()
    {
        audioSource.UnPause();
    }


    public void SoundBeep()
    {
        if (clipBeep != null)
            audioSource.PlayOneShot(clipBeep);
    }
}
