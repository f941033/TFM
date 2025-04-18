using UnityEngine;
using TMPro;

public class CardUI : MonoBehaviour
{
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textDescription;
    [HideInInspector] public CardData data;

    public void setCardUI(CardData cardData){
        data = cardData;
        textName.text = data.cardName;
        textDescription.text = data.description;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
