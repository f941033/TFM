using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private AudioSettings audioSettingsPrefab;
    private AudioSource audioSource;
    public AudioClip[] audioclips;


    // ==============   LISTA DE CLIPS  ==============
    // 0: MUSICA MENU PPAL
    // 1: FASE DE PREPARACIÓN
    // 2: FASE DE ACCIÓN



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
    }

    private void Start()
    {
        audioSettingsPrefab = FindAnyObjectByType<AudioSettings>();
    }
    public void PlayBackgroundSound(int index)
    {
        //audioSource.Stop();
        ////audioSource.volume = (index != 2) ? 0.2f : 0.04f;
        //audioSource.volume = AudioSettings.MusicVolume;
        //audioSource.clip = audioclips[index];
        //audioSource.Play();

        AudioSettings.Instance.PlayMusic(audioclips[index],true);
    }


}
