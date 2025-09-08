using UnityEngine;
using UnityEngine.EventSystems;

public class HoverScaleSimple : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public AudioSource audioSource;
    public AudioClip clipHover, clipPressed;

    public float hoverScale = 1.3f;   // cuánto crece al pasar el ratón
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        transform.localScale = originalScale * hoverScale;
        if(clipHover != null) audioSource.PlayOneShot(clipHover);
    }

    public void OnPointerExit(PointerEventData e)
    {
        transform.localScale = originalScale;
    }

    //public void SoundOnPressed()
    //{
    //    audioSource.PlayOneShot(clipPressed);
    //}

    public void OnPointerClick(PointerEventData eventData)
    {
        if(gameObject.name == "PlayButton")
        {
            AudioClip clip = Resources.Load<AudioClip>("Audio/risa malefica");
            audioSource.PlayOneShot(clip);
        }else
            audioSource.PlayOneShot(clipPressed);
    }
}
