using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip[] audioclips;

    // ==============   LISTA DE CLIPS  ==============
    // 0: MUSICA MENU PPAL
    // 1: FASE DE PREPARACIÓN
    // 2: FASE DE ACCIÓN

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
