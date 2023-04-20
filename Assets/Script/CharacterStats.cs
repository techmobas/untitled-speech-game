using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;


namespace USG.Character
{

    public class CharacterStats : MonoBehaviour
    {
        [Header("Stats Attributes")]
        [SerializeField] protected float maxHealth;
        protected float currentHealth;
        [SerializeField] protected float maxMana;
        protected float currentMana;
        [SerializeField] protected float attackPower;
        [SerializeField] protected float defense;
        [SerializeField] protected float criticalChance;
        [SerializeField] protected float criticalDamage;
        public bool isCritical;

        public float MaxHealth() { return maxHealth; }
        public float CurrentHealth() { return currentHealth; }
        public float MaxMana() { return maxMana; }
        public float CurrentMana() { return currentMana; }
        public float AttackPower() { return attackPower; }
        public float Defense() { return defense; }
        public float GetCC() { return criticalChance; }
        public float GetCD() { return criticalDamage; }

        [Header("Buff Attributes")]
        public AbilitySO[] abilities;
        [SerializeField][ReadOnly] AbilitySO[] activeBuffs = new AbilitySO[4];

        [Header("Character Animation")]
        private Animator playAnim;

        [Header("Canvas Control")]
        [SerializeField] RectTransform statusCanvas;
        [SerializeField] RectTransform iconGroup;

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
            if (isCritical) {
                StatsUIManager.Instance.GenerateText(statusCanvas, transform.position, damage.ToString(), Color.yellow, isCritical);
            }
			else {
                StatsUIManager.Instance.GenerateText(statusCanvas, transform.position, damage.ToString(), Color.white, isCritical);
            }
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                StartCoroutine(CharacterDead());
            }
        }

        public void Regenerate(float regenValue, Color color) {
            StatsUIManager.Instance.GenerateText(statusCanvas, transform.position, regenValue.ToString(), color, false);
        }

        public void SetCurrentMana(float newMana) {
            currentMana = Mathf.Clamp(newMana, 0, maxMana); 
            Debug.Log("Current mana set to: " + currentMana);
        }

        public void SetCurrentHealth(float newHP) {
            currentHealth = Mathf.Clamp(newHP, 0, maxHealth); 
            Debug.Log("Current HP set to: " + currentHealth);
        }

        public void ApplyBuff(AbilitySO buff) {
            // Create a copy of the buff to avoid modifying the asset in the project
            AbilitySO buffCopy = Instantiate(buff);

            // Check if the buff is already active
            for (int i = 0; i < activeBuffs.Length; i++) {
                AbilitySO activeBuff = activeBuffs[i];
                if (activeBuff != null && activeBuff.name == buffCopy.name) {
                    // If the buff is already active, refresh the duration
                    activeBuff.duration = buffCopy.duration;
                    Debug.Log("Buff refreshed: " + activeBuff.name);
                    return;
                }
            }

            // Find an empty slot for the new buff
            for (int i = 0; i < activeBuffs.Length; i++) {
                if (activeBuffs[i] == null) {
                    activeBuffs[i] = buffCopy;
                    Debug.Log("Buff applied: " + activeBuffs[i].name);
                    
                    ApplyBuffEffects();
                    StatsUIManager.Instance.AddBuffIcon(iconGroup, transform.position, buffCopy);
                    return;
                }
            }

            // If all slots are full, override the oldest buff
            int oldestBuffIndex = 0;
            int oldestBuffDuration = activeBuffs[0].duration;
            for (int i = 1; i < activeBuffs.Length; i++) {
                if (activeBuffs[i] != null && activeBuffs[i].duration < oldestBuffDuration) {
                    oldestBuffIndex = i;
                    oldestBuffDuration = activeBuffs[i].duration;
                }
            }
            activeBuffs[oldestBuffIndex] = buffCopy;
            Debug.Log("Buff applied: " + activeBuffs[oldestBuffIndex].name);

            StatsUIManager.Instance.AddBuffIcon(iconGroup, transform.position, buffCopy);
            ApplyBuffEffects();
        }

        public void UpdateBuffs() {
            for (int i = 0; i < activeBuffs.Length; i++) {
                AbilitySO activeBuff = activeBuffs[i];
                if (activeBuff != null) {
                    // Update the duration
                    activeBuff.duration -= 1;
                    // Check if the buff has expired
                    if (activeBuff.duration < 0) {
                        // Remove the buff
                        activeBuffs[i] = null;

                        // Reverse the effect of the buff
                        switch (activeBuff.buffType) {
                            case AbilitySO.BuffType.Attack:
                                attackPower -= activeBuff.damage;
                                break;
                            case AbilitySO.BuffType.Defense:
                                defense -= activeBuff.damage;
                                break;
                            case AbilitySO.BuffType.CriticalChance:
                                criticalChance -= activeBuff.damage;
                                break;
                            case AbilitySO.BuffType.CriticalDamage:
                                criticalDamage -= activeBuff.damage;
                                break;
                        }
                        StatsUIManager.Instance.RemoveBuffIcon(activeBuff);
                    }
                }
            }
        }

        public void ApplyBuffEffects() {
            for (int i = 0; i < activeBuffs.Length; i++) {
                AbilitySO activeBuff = activeBuffs[i];
                if (activeBuff != null) {
                    switch (activeBuff.buffType) {
                        case AbilitySO.BuffType.Attack:
                            attackPower += activeBuff.damage;
                            StatsUIManager.Instance.GenerateText(statusCanvas, transform.position, "ATK UP", Color.red, false);
                            break;
                        case AbilitySO.BuffType.Defense:
                            defense += activeBuff.damage;
                            StatsUIManager.Instance.GenerateText(statusCanvas, transform.position, "DEF Up", Color.magenta, false);
                            break;
                        case AbilitySO.BuffType.CriticalChance:
                            criticalChance += activeBuff.damage;
                            StatsUIManager.Instance.GenerateText(statusCanvas, transform.position, "CC% UP", Color.yellow, false);
                            break;
                        case AbilitySO.BuffType.CriticalDamage:
                            criticalDamage += activeBuff.damage;
                            StatsUIManager.Instance.GenerateText(statusCanvas, transform.position, "CDMG% Up", Color.yellow, false);
                            break;
                    }
                }
            }
        }

        public void PlayAttack(int attackID) {
            playAnim.SetTrigger("Attack");
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

