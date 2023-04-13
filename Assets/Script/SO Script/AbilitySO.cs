using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Ability", menuName = "Ability")]
public class AbilitySO : ScriptableObject
{
    [Header("Text Data")]
    public string abilityName;
    public string abilityKeyword;
    [Header("Stats Number")]
    public float damage;
    public float manaCost;
    public AbilityType abilityType;

    public enum AbilityType
	{
        Damage,
        Heal,
        Recharge,
        ATKBuff,
        DEFBuff,
        CCBuff,
        CDBuff,
    }
}
