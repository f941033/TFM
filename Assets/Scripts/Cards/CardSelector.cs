using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardSelector : MonoBehaviour, IPointerClickHandler
{
    public Image haloImage;
    [HideInInspector] public bool isSelected = false;
    private CardSelectionManager selectionManager;
    private CardDragDrop dataHolder;
    public TextMeshProUGUI textName;
    void Awake()
    {
        selectionManager = FindAnyObjectByType<CardSelectionManager>();
        dataHolder = GetComponent<CardDragDrop>();
        haloImage.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        selectionManager.SelectCard(this);
    }

    public void Select()
    {
        isSelected = true;
        Color c = haloImage.color;
        c.a = 1f;
        haloImage.color = c;
        haloImage.gameObject.SetActive(true);
    }

    public void Deselect()
    {
        isSelected = false;
        haloImage.gameObject.SetActive(false);
    }

    public CardData GetCardData()
    {
        return dataHolder.cardData;
    }

    public string GetCardName()
    {
        return textName.text;
    }
}
