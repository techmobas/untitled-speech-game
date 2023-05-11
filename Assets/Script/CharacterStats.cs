using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using USG.UI;


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
        [ReadOnly] public List<AbilitySO> activeBuffs = new List<AbilitySO>();

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

        public void TakeDamage(float damage, Color color, bool isCrit)
        {
            StatsUIManager.Instance.GenerateText(statusCanvas, transform.position, damage.ToString(), color, isCrit);
            currentHealth -= damage;
            PlayStagger();
            if (currentHealth <= 0)
            {
                StartCoroutine(CharacterDead());
            }
        }

        public void StatusUIText(string text, Color color) {
            StatsUIManager.Instance.GenerateText(statusCanvas, transform.position, text, color, false);
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

        public void SpawnEffect(GameObject effect) {
            GameObject fx = Instantiate(effect, transform.position, Quaternion.identity);
            Destroy(fx.gameObject, fx.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        }

        #region Buff Logic
        public void ApplyBuff(AbilitySO buff) {
            // Create a copy of the buff to avoid modifying the asset in the project
            AbilitySO buffCopy = Instantiate(buff);

            // Check if the buff is already active
            for (int i = 0; i < activeBuffs.Count; i++) {
                AbilitySO activeBuff = activeBuffs[i];
                if (activeBuff != null && activeBuff.name == buffCopy.name) {
                    // If the buff is already active, refresh the duration
                    activeBuff.duration = buffCopy.duration;
                    Debug.Log("Buff refreshed: " + activeBuff.name);
                    return;
                }
            }

            activeBuffs.Add(buffCopy);
            Debug.Log("Buff applied: " + buffCopy.name);

            ApplyBuffEffects();
            StatsUIManager.Instance.AddBuffIcon(iconGroup, transform.position, buffCopy);

            // Determine the type of buff and set the string accordingly
            string buffString = "";
            switch (buffCopy.buffType) {
                case AbilitySO.BuffType.Attack:
                    buffString = "ATK UP";
                    break;
                case AbilitySO.BuffType.Defense:
                    buffString = "DEF UP";
                    break;
                case AbilitySO.BuffType.CriticalChance:
                    buffString = "CC% UP";
                    break;
                case AbilitySO.BuffType.CriticalDamage:
                    buffString = "CDMG% UP";
                    break;
                default:
                    break;
            }

            StatsUIManager.Instance.GenerateText(statusCanvas, transform.position, buffString, Color.yellow, false);
        }

        public void UpdateBuffs() {
            for (int i = activeBuffs.Count - 1; i >= 0; i--) {
                AbilitySO activeBuff = activeBuffs[i];
                if (activeBuff != null) {
                    activeBuff.duration -= 1;
                    // Check if the buff has expired
                    if (activeBuff.duration <= 0) {
                        // Remove the buff
                        activeBuffs.RemoveAt(i);

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
            for (int i = 0; i < activeBuffs.Count; i++) {
                AbilitySO activeBuff = activeBuffs[i];
                if (activeBuff != null) {
                    switch (activeBuff.buffType) {
                        case AbilitySO.BuffType.Attack:
                            attackPower += activeBuff.damage;
                            break;
                        case AbilitySO.BuffType.Defense:
                            defense += activeBuff.damage;
                            break;
                        case AbilitySO.BuffType.CriticalChance:
                            criticalChance += activeBuff.damage;
                            break;
                        case AbilitySO.BuffType.CriticalDamage:
                            criticalDamage += activeBuff.damage;
                            break;
                    }
                }
            }
        }

        #endregion

        #region Debuff Logic
        public void ApplyDebuff(AbilitySO debuff) {
            // Create a copy of the buff to avoid modifying the asset in the project
            AbilitySO debuffCopy = Instantiate(debuff);

            // Check if the buff is already active
            for (int i = 0; i < activeBuffs.Count; i++) {
                AbilitySO activeBuff = activeBuffs[i];
                if (activeBuff != null && activeBuff.name == debuffCopy.name) {
                    // If the buff is already active, refresh the duration
                    activeBuff.duration = debuffCopy.duration;
                    Debug.Log("Buff refreshed: " + activeBuff.name);
                    return;
                }
            }

            activeBuffs.Add(debuffCopy);
            Debug.Log("Buff applied: " + debuffCopy.name);

            ApplyDebuffEffects();
            StatsUIManager.Instance.AddBuffIcon(iconGroup, transform.position, debuffCopy);

            // Determine the type of buff and set the string accordingly
            string buffString = "";
            switch (debuffCopy.debuffType) {
                case AbilitySO.DebuffType.Attack:
                    buffString = "ATK DOWN";
                    break;
                case AbilitySO.DebuffType.Defense:
                    buffString = "DEF DOWN";
                    break;
                default:
                    break;
            }

            StatsUIManager.Instance.GenerateText(statusCanvas, transform.position, buffString, Color.yellow, false);
        }

        public void UpdateDebuffs() {
            for (int i = activeBuffs.Count - 1; i >= 0; i--) {
                AbilitySO activeBuff = activeBuffs[i];
                if (activeBuff != null) {
                    // Update the duration
                    activeBuff.duration -= 1;
                    // Check if the buff has expired
                    if (activeBuff.duration < 0) {
                        // Remove the buff
                        activeBuffs.RemoveAt(i);

                        // Reverse the effect of the buff
                        switch (activeBuff.debuffType) {
                            case AbilitySO.DebuffType.Attack:
                                attackPower += activeBuff.damage;
                                break;
                            case AbilitySO.DebuffType.Defense:
                                defense += activeBuff.damage;
                                break;
                        }
                        StatsUIManager.Instance.RemoveBuffIcon(activeBuff);
                    }
                }
            }
        }

        public void ApplyDebuffEffects() {
            for (int i = 0; i < activeBuffs.Count; i++) {
                AbilitySO activeBuff = activeBuffs[i];
                if (activeBuff != null) {
                    switch (activeBuff.debuffType) {
                        case AbilitySO.DebuffType.Attack:
                            attackPower -= activeBuff.damage;
                            break;
                        case AbilitySO.DebuffType.Defense:
                            defense -= activeBuff.damage;
                            break;
                    }
                }
            }
        }
        #endregion

        #region Animation Shenaigans
        public void PlayAttack(int attackID) {
            playAnim.SetTrigger("Attack");
            playAnim.SetInteger("AttackID", attackID);
        }

        void PlayStagger() {
            playAnim.SetTrigger("Hit");
        }


        IEnumerator CharacterDead() {
            playAnim.SetBool("IsDead", true);
            yield return new WaitForSeconds(3f);
            //Destroy(gameObject);
        }
		#endregion
	}
}