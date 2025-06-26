using UnityEngine;

public enum DebuffType
{
    Speed,
    AttackRate,
    AttackRange,
    Damage
    // futuros: Defense, Stun, Poison...
}

[System.Serializable]
public class DebuffEffect
{
    public DebuffType type;
    [Tooltip("Valor multiplicador, e.g. 0.5 reduce a la mitad")]
    public float multiplier = 1f;
}