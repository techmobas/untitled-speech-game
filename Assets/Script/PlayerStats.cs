using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] protected int maxHealth;
    protected int currentHealth;
    [SerializeField] protected int attackPower;
    [SerializeField] protected int defense;
    [SerializeField] protected int maxMana;
    protected int currentMana;

    public int MaxHealth() { return maxHealth; }
    public int CurrentHealth() { return currentHealth; }
    public int AttackPower() { return attackPower; }
    public int Defense() { return defense;  }
    public int MaxMana() { return maxMana;  }
    public int CurrentMana() { return currentMana;  }


    public AbilitySO[] abilities;

	private void Start()
	{
        currentHealth = maxHealth;
        currentMana = maxMana;
	}

	public string[] GetAbilityNames()
    {
        List<string> abilityNames = new List<string>();
        foreach (AbilitySO ability in abilities)
        {
            abilityNames.Add(ability.abilityName);
        }
        return abilityNames.ToArray();
    }

    public void TakeDamage(int damage)
	{
        currentHealth -= damage;
        if(currentHealth <= 0)
		{
            //Die function
		}
	}
}
