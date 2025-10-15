using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MulliganSelectable : MonoBehaviour, IPointerClickHandler
{
    private CardManager manager;
    private CardUI cardUI;
    private Image haloImg;
    [SerializeField] private Graphic clickTarget;
    public bool Selected { get; private set; }

    public void Init(CardManager mgr, CardUI ui)
    {
        manager = mgr;
        cardUI = ui;
        haloImg = cardUI?.GetMulliganOverlay();
        EnsureClickableTarget();

        // Por si el halo está desactivado por defecto
        Selected = false;
        ApplyVisual();
        if (clickTarget) clickTarget.raycastTarget = true;
    }
    private void EnsureClickableTarget()
    {
        if (!clickTarget)
        {
            // intenta usar un Graphic ya existente en la raíz
            clickTarget = GetComponent<Graphic>();
            if (!clickTarget) clickTarget = GetComponentInChildren<Graphic>(true);
        }
        if (!clickTarget)
        {
            // último recurso: crea una Image transparente
            var img = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            img.color = new Color(0,0,0,0);
            clickTarget = img;
        }

        // que cubra toda la carta
        var rt = (clickTarget as Image)?.rectTransform ?? GetComponent<RectTransform>();
        if (rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    public void SetSelected(bool value, bool applyVisual)
    {
        Selected = value;
        if (applyVisual) ApplyVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager == null || !manager.inMulligan) return;

        bool wantSelect = !Selected;
        if (!manager.TryToggleSelect(this, wantSelect)) return;

        Selected = wantSelect;
        ApplyVisual();
    }

    private void ApplyVisual()
    {
        if (!haloImg) return;

        // Opción A: si el GO del halo está desactivado por defecto:
        haloImg.gameObject.SetActive(Selected);

        // Opción B (si el GO está activo y solo ocultas mostrador):
        // haloImg.enabled = Selected;

        // Asegura que NO haga raycast:
        haloImg.raycastTarget = false;

        // Opcional: forzar que quede detrás si te hace falta
        if (Selected) haloImg.transform.SetAsFirstSibling();
    }

    private void OnDisable()
    {
        // Limpieza por si sales de mulligan o destruyes la carta
        if (haloImg) haloImg.gameObject.SetActive(false);
        Selected = false;
    }
    public void ForceDisable()
    {
        SetSelected(false, applyVisual: true);
        if (clickTarget) clickTarget.raycastTarget = false;
        enabled = false;
    }
}
