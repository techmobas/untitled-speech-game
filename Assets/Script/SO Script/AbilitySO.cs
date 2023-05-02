using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;


[CreateAssetMenu(fileName = "New Ability", menuName = "Ability")]
public class AbilitySO : ScriptableObject
{
    [Header("Text Data")]
    public string abilityName;
    public string abilityKeyword;
    [TextArea] public string abilityDescription;

    [Header("Stats Number")]
    public float damage;
    public float manaCost;

    [Header("Ability Type")]
    public AbilityType abilityType;

    [ConditionalField(nameof(abilityType), false, AbilityType.Charge)]
    public ChargeType chargeType;

    [ConditionalField(nameof(abilityType), false, AbilityType.Buff)]
    public BuffType buffType;

    [ConditionalField(nameof(abilityType), false, AbilityType.Debuff)]
    public DebuffType debuffType;

    [ConditionalField(nameof(abilityType), false, AbilityType.Buff, AbilityType.Debuff)]
    public int duration;
    [ConditionalField(nameof(abilityType), false, AbilityType.Buff, AbilityType.Debuff)]
    public Sprite icon;

    [Header("Ability Type")]
    public GameObject effect;

    public enum AbilityType
	{
        Damage,
        Charge,
        Buff,
        Debuff
    }

    public enum ChargeType{
        Heal,
        Mana
    }

    public enum BuffType {
       Attack,
       Defense,
       CriticalChance,
       CriticalDamage
    }

    public enum DebuffType {
        Attack,
        Defense,
    }
}
