using UnityEngine;

public class Card
{
    public readonly string cardName;
    public readonly byte cost;
    public readonly string description;
    private readonly string type;
    private readonly string rarity;
    private readonly float modHealth;
    private readonly float modAttack;
    private readonly float modVelocity;

    public Card(string cardName, byte cost, string description, string type, string rarity, float modHealth, float modAttack, float modVelocity){
        this.cardName = cardName;
        this.cost = cost;
        this.description = description;
        this.type = type;
        this.rarity = rarity;
        this.modHealth = modHealth;
        this.modAttack = modAttack;
        this.modVelocity = modVelocity;
    }

}
