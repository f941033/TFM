using UnityEngine;

public class CardSelectionManager : MonoBehaviour
{
    private CardSelector selectedCard = null;

    public void SelectCard(CardSelector newSelection)
    {
        if (selectedCard != null && selectedCard != newSelection)
        {
            selectedCard.Deselect();
        }
        selectedCard = newSelection;
        selectedCard.Select();
    }

    // Devuelve el CardData de la carta seleccionada
    public CardData GetSelectedCardData()
    {
        return selectedCard != null ? selectedCard.GetCardData() : null;
    }

    public string GetSelectedCardName()
    {
        return selectedCard != null ? selectedCard.GetCardName() : null;
    }
}
