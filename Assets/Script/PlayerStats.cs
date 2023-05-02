using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USG.Character {
    public class PlayerStats : CharacterStats
    {
        public void InitializeStats(float maxHealth, float maxMana, float attackPower, float defense, float criticalChance, float criticalDamage) {
            this.maxHealth = maxHealth;
            currentHealth = maxHealth;
            this.maxMana = maxMana;
            currentMana = maxMana;
            this.attackPower = attackPower;
            this.defense = defense;
            this.criticalChance = criticalChance;
            this.criticalDamage = criticalDamage;
        }
    }
}