using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace USG.Character
{
    public enum Attribute {
        Health,
        Mana,
        Attack,
        Defense,
        CritChance,
        CritDamage
    }

    public class CharacterStats : MonoBehaviour
    {
        [SerializeField] protected float maxHealth;
        protected float currentHealth;
        [SerializeField] protected float maxMana;
        protected float currentMana;
        [SerializeField] protected float attackPower;
        [SerializeField] protected float defense;
        [SerializeField] protected float criticalChance;
        [SerializeField] protected float criticalDamage;

        public float MaxHealth() { return maxHealth; }
        public float CurrentHealth() { return currentHealth; }
        public float MaxMana() { return maxMana; }
        public float CurrentMana() { return currentMana; }
        public float AttackPower() { return attackPower; }
        public float Defense() { return defense; }
        public float GetCC() { return criticalChance; }
        public float GetCD() { return criticalDamage; }

        public AbilitySO[] abilities;



        [Header("Character Animation")]
        private Animator playAnim;
        private int attackID;

        private void Start()
        {
            currentHealth = maxHealth;
            currentMana = maxMana;

            playAnim = GetComponentInChildren<Animator>();
        }

		private void Update() {
            if (maxHealth >= 5000) {
                maxHealth = 5000;
            }
            if (maxMana >= 500) {
                maxMana = 500;
            }
            if (attackPower >= 1500) {
                attackPower = 1500;
            }
            if (defense >= 200) {
                defense = 200;
            }

            if (criticalChance >= 1f) {
                criticalChance = 1f;
            }
            if (criticalDamage >= 2f) {
                criticalDamage = 2f;
            }
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

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                StartCoroutine(CharacterDead());
            }
        }

        public void ModifyAttribute(float amount, Attribute attribute) {
            switch (attribute) {
                case Attribute.Health:
                    currentHealth += amount;
                    break;
                case Attribute.Mana:
                    float newMana = currentMana += amount;
                    SetCurrentMana(newMana);
                    break;
                case Attribute.Attack:
                    attackPower += amount;
                    break;
                case Attribute.Defense:
                    defense += amount;
                    break;
                case Attribute.CritChance:
                    criticalChance += amount;
                    break;
                case Attribute.CritDamage:
                    criticalDamage += amount;
                    break;
                default:
                    break;
            }
        }

        public void SetCurrentMana(float newMana) {
            currentMana = Mathf.Clamp(newMana, 0, maxMana); // Make sure currentMana doesn't exceed maxMana or go below 0
            Debug.Log("Current mana set to: " + currentMana);
        }

        public void PlayAttack() {
            playAnim.SetTrigger("Attack");
            attackID = Random.Range(1, 2);
            playAnim.SetInteger("AttackID", attackID);
        }

        public void PlayStagger() {
            playAnim.SetTrigger("Hit");
        }


        IEnumerator CharacterDead() {
            playAnim.SetBool("IsDead", true);
            yield return new WaitForSeconds(3f);
            Destroy(gameObject);
        }
    }
}

