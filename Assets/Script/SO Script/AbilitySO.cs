using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Ability", menuName = "Ability")]
public class AbilitySO : ScriptableObject
{
    public string abilityName;
    public string abilityKeyword;
    public int manaCost;
    public int damage;
    public AbilityType abilityType;

    public enum AbilityType
	{
        Damage,
        Defense,
        Heal
	}
}
