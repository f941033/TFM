using UnityEngine;
using TMPro;

public class CardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textName;
    [SerializeField] private TextMeshProUGUI textDescription;
    [SerializeField] private TextMeshProUGUI textCost;
    [SerializeField] private TextMeshProUGUI textDamage;
    [HideInInspector] public CardData data;

    public void setCardUI(CardData cardData){
        data = cardData;
        textName.text = data.cardName;
        textDescription.text = data.description;
        textCost.text = cardData.cost.ToString();
        if(data.cardType == CardType.Trap){
            var trapData = cardData as TrapCardData;
            if(trapData!=null)
                textDamage.text = trapData.damage.ToString();
        }
        else{
            textDamage.gameObject.SetActive(false);
        }
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
