using UnityEngine;
using DeckboundDungeon.Cards; 

public class CardSelectionController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Aquí arrastra el GameObject que tiene el GridLayoutGroup")]
    public Transform gridContainer;    

    [Tooltip("Prefab que contiene el script CardDeck y sus referencias (borde, textos…)")]
    public GameObject cardDeckPrefab; 
    public CardSummaryUI summaryPanel;

    void Start()
    {
        // Carga todos los CardData de Resources/Cards
        var allCards = Resources.LoadAll<CardData>("Cards");
        foreach (var card in allCards)
        {
            // Instancia un prefab por cada carta
            var go = Instantiate(cardDeckPrefab, gridContainer);
            // Le asignas el ScriptableObject concreto
            var cd = go.GetComponent<CardDeck>();
            cd.cardData = card;
            cd.summaryPanel = summaryPanel;
            // (El Start del CardDeck se encargará de rellenar nombres, coste, etc.)
        }
    }
}