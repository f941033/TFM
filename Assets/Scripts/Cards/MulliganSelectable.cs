using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MulliganSelectable : MonoBehaviour, IPointerClickHandler
{
    private CardManager manager;
    private CardUI cardUI;
    public bool Selected { get; private set; } = false;
    private Image highlight;
    private CanvasGroup cg;

    public void Init(CardManager mgr, CardUI card)
    {
        manager = mgr;
        cardUI = card;
        if (!cg) cg = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (!highlight)
        {
            // Busca una Image de fondo del prefab de carta para tintarla sutilmente (opcional)
            highlight = GetComponentInChildren<Image>();
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

        string cardName = cardUI ? cardUI.data?.cardName : "(sin CardUI)";
        Debug.Log("[Mulligan] " + (Selected ? "Seleccionada" : "Deseleccionada")
                  + " | total=" + manager.mulliganSelectedCount
                  + " | max=" + manager.drawPile.Count
                  + " | carta=" + cardName);
    }

    private void ApplyVisual()
    {
        if (cg) cg.alpha = Selected ? 0.6f : 1f;
        // Si tienes un borde/outline propio, actívalo aquí.
        // Ej.: if (outline) outline.SetActive(Selected);
    }
}
