using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDetailUI : MonoBehaviour
{
    public static CardDetailUI Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject panel;

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
        panel.SetActive(false);
    }

    public void Show(CardData data)
    {
        // limpia contenedor
        //panel.SetActive(true);
        foreach (Transform c in cardContainer) Destroy(c.gameObject);
        // instancia la carta (sin lógica de drag/drop)
        var go = Instantiate(cardPrefab, cardContainer);
        var ui = go.GetComponent<CardUI>();
        ui.setCardUI(data);

        Destroy(go.GetComponent<CardDragDrop>());
        Destroy(go.GetComponent<CardSelector>());

        descriptionText.text = data.description;

        panel.SetActive(true);
    }

    public void Hide()
    {
        foreach (Transform c in cardContainer)
        {
            Destroy(c.gameObject);
        }
        panel.SetActive(false);
    }
}