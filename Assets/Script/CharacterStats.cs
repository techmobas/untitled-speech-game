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
        [SerializeField] protected int criticalChance;
        [SerializeField] protected int criticalDamage;

        public int MaxHealth() { return maxHealth; }
        public int CurrentHealth() { return currentHealth; }
        public int AttackPower() { return attackPower; }
        public int Defense() { return defense; }
        public int MaxMana() { return maxMana; }
        public int CurrentMana() { return currentMana; }
        public int GetCC() { return criticalChance; }
        public int GetCD() { return criticalChance; }

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
                StartCoroutine(CharacterDead());
            }
        }

        public void SetCurrentMana(int newMana) {
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

