using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DeckHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform rectTransform;
    private Vector3 originalPosition;

    private Coroutine moveCoroutine;

    [Header("Animation Settings")]
    public float hoverMoveDistance = 200f;
    public float animationDuration = 0.3f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //rectTransform.localPosition = originalPosition + new Vector3(0,250,0);
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimatePosition(originalPosition + new Vector3(0, hoverMoveDistance, 0)
        ));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //rectTransform.localPosition = originalPosition;
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(AnimatePosition(originalPosition));
    }

    private IEnumerator AnimatePosition(Vector3 targetPos)
    {
        Vector3 startPos = rectTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            float t = easeCurve.Evaluate(elapsed / animationDuration);
            rectTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.localPosition = targetPos;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
