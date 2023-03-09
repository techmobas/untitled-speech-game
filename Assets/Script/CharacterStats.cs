using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace USG.Character
{
    public class CharacterStats : MonoBehaviour
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
        public int Defense() { return defense; }
        public int MaxMana() { return maxMana; }
        public int CurrentMana() { return currentMana; }

        public AbilitySO[] abilities;

        private void Start()
        {
            currentHealth = maxHealth;
            currentMana = maxMana;
        }

        public string[] GetAbilityNames()
        {
            List<string> abilityKeyword = new List<string>();
            foreach (AbilitySO ability in abilities)
            {
                abilityKeyword.Add(ability.abilityKeyword);
            }
            return abilityKeyword.ToArray();
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                //Die function
            }
        }

        public void SetCurrentMana(int newMana) {
            currentMana = Mathf.Clamp(newMana, 0, maxMana); // Make sure currentMana doesn't exceed maxMana or go below 0
            Debug.Log("Current mana set to: " + currentMana);
        }
    }
}

