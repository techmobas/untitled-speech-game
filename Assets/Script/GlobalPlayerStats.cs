using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MyBox;

namespace USG.Character {

    public class GlobalPlayerStats : Singleton<GlobalPlayerStats> {
        public float maxHealth;
        public float maxMana;
        public float attackPower;
        public float defense;
        public float criticalChance;
        public float criticalDamage;

        private PlayerStats playerStats;

        private void Start() {
            playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
            if (playerStats == null) {
                Debug.LogError("Could not find PlayerStats object!");
            }
            else {
                playerStats.InitializeStats(maxHealth, maxMana, attackPower, defense, criticalChance, criticalDamage);
            }
        }
    }
}

