using UnityEngine;
using TMPro;

public class CardUI : MonoBehaviour
{
    public TextMeshProUGUI textName;
    public TextMeshProUGUI textDescription;

    public void setCardUI(Card card){
        textName.text = card.cardName;
        textDescription.text = card.description;
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
