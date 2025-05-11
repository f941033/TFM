using UnityEngine;

[CreateAssetMenu(menuName="Config/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Tooltip("Número de cartas iniciales en el mazo")]
    public int initialDeckSize = 10;
}
