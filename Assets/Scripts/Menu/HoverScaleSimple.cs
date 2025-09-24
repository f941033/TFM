using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverScaleSimple : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public AudioSource audioSource;
    public AudioClip clipHover, clipPressed;

    public float hoverScale = 1.3f;   // cuánto crece al pasar el ratón
    private Vector3 originalScale;
    private float animationDuration = 0.2f;
    private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private Coroutine currentAnimation;
    Vector3 finalScale;

    void Awake()
    {
        originalScale = transform.localScale;
        finalScale = originalScale * hoverScale;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        //transform.localScale = originalScale * hoverScale;
        
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(Animate(originalScale, finalScale));
        if(clipHover != null) audioSource.PlayOneShot(clipHover);
    }

    private IEnumerator Animate(Vector3 fromScale, Vector3 toScale)
    {
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = easeCurve.Evaluate(elapsed / animationDuration);
            transform.localScale = Vector3.Lerp(fromScale, toScale, t);
            yield return null;
        }

        // Asegurar valores finales exactos
        transform.localScale = toScale;
        currentAnimation = null;
    }

    public void OnPointerExit(PointerEventData e)
    {
        //transform.localScale = originalScale;
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(Animate(finalScale, originalScale));
        if (clipHover != null) audioSource.PlayOneShot(clipHover);
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
