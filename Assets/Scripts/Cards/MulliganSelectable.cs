using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MulliganSelectable : MonoBehaviour, IPointerClickHandler
{
    private CardManager manager;
    private CardUI cardUI;
    public bool Selected { get; private set; } = false;
    private Image overlayImg;

    public void Init(CardManager mgr, CardUI card)
    {
        manager = mgr;
        cardUI = card;
        overlayImg = GetComponent<Image>(); 
        if (overlayImg)
            overlayImg.raycastTarget = true;
        Selected = false;
        ApplyVisual();
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
        if (!overlayImg) return;
        overlayImg.color = Selected ? new Color(1f, 1f, 0f, 0.18f) : new Color(0f, 0f, 0f, 0f);
    }
}
