using UnityEngine;

[CreateAssetMenu]
public class CardObject : ScriptableObject
{
    public Card card;
    public CardObject nextGrade;
    public string description;
}