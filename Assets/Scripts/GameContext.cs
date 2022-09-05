using System;
using System.Collections.Generic;
using System.ComponentModel;
using Client;
using Leopotam.Ecs;
using MyBox;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class GameContext
{
    [NonSerialized]
    public Card[] cardsPlayer;
    [NonSerialized]
    public Card[] cardsEnemy;
    
    [NonSerialized]
    public List<Card> cardsInDeck;
    
    [NonSerialized]
    public List<Card> cardsInHand;
    
    [NonSerialized]
    public int playerEnemyHpBalance = 5;

    [NonSerialized] 
    public int invasionLevel = 0;

    public CardUI cardChosenUI;
    public CardUI playerCardClickedUI;

    public bool isCardChoseLevel;
}

[Serializable]
public class Card
{
    public int hp;
    public int damage;
    public string name;
    
    [Newtonsoft.Json.JsonIgnore]
    public Sprite sprite;
    
    public Optional<SplitAttack> splitAttack;
    public Optional<Poison> poisonOther;
    public Optional<DeadlyPoison> deadlyPoison;
    public Optional<Buff> buff;
    public Optional<Transformation> transformation;
    public Optional<ArrowShot> arrowShot;
    public Optional<Steroids> steroids;
    public Optional<Summon> summon;
    public Optional<Shield> shield;
    public Optional<Quill> quill;
    public Optional<ReduceDamage> reduceDamage;
    public Optional<GyroAttack> gyroAttack;

    public Optional<Poisoned> poisoned;

    [NonSerialized]
    public bool IsDead;
    
    [NonSerialized, Newtonsoft.Json.JsonIgnore]
    public Side side;

    [NonSerialized, Newtonsoft.Json.JsonIgnore]
    public CardObject cardObject;

    public override string ToString()
    {
        return $"isDead: {IsDead} {name}\n hp:{hp}, damage:{damage}, {side}" +
               $" {splitAttack} {poisonOther} {deadlyPoison} {buff} " +
               $"{steroids} {arrowShot} {reduceDamage} {summon} {quill} {shield} " +
               $"{transformation} {gyroAttack}   ({poisoned} {poisoned?.Value?.level} {poisoned?.Value?.needToTick})";
    }
}

public class Poisoned
{
    public bool needToTick;
    public int level;
}

[Serializable]
public class GyroAttack
{
}

[Serializable]
public class ReduceDamage
{
}

[Serializable]
public class Steroids
{
}


[Serializable]
public class ArrowShot
{
}

[Serializable]
public class Summon
{
    public int turnsToSummon = 1;
    
    [JsonIgnore]
    public ScriptableObject cardToSummon;
}

[Serializable]
public class DeadlyPoison
{
}

[Serializable]
public class Buff
{
}


[Serializable]
public class Optional<T>
{
    public bool IsSet;

    [ConditionalField("IsSet", false, true)]
    public T Value;

    public override string ToString()
    {
        if (IsSet)
        {
            return Value.ToString();
        }
        else
        {
            return "";
        }
    }
}

[Serializable]
public class SplitAttack
{
}

[Serializable]
public class Shield
{
    public bool Alive = true;
}

[Serializable]
public class Quill
{
}

[Serializable]
public class Poison
{
    public int level;
}

[Serializable]
public class Transformation
{
    [NonSerialized]
    public int countTurnsToTransform = 1;
    
    [Newtonsoft.Json.JsonIgnore]
    public CardObject transformTo;
}

public enum Side
{
    player,
    enemy
}