using UnityEngine;
using UnityEngine.EventSystems;

public class HoverScaleSimple : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.3f;   // cuánto crece al pasar el ratón
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        transform.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData e)
    {
        transform.localScale = originalScale;
    }
}
