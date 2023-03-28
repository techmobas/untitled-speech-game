using System.Collections;
using System;
using System.Collections.Generic;
using USG.Character;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace USG.Mechanics {
    public enum GameState {
        PlayerTurn,
        EnemyTurn,
        GameOver
    }

    public class TurnBasedSystem : MonoBehaviour {
        public GameObject player;
        public GameObject enemy;

        CharacterStats playerStats;
        CharacterStats enemyStats;

        private string[] keywords;

        private KeywordRecognizer keywordRecognizer;

        private GameState gameState;
        
        [SerializeField] float timeBetweenTurns;
        bool playerActionSuccess;

        void Start() {
            playerStats = player.GetComponent<CharacterStats>();
            enemyStats = enemy.GetComponent<CharacterStats>();

            // Add the player's abilities as keywords for the PhraseRecognizer
            keywords = playerStats.GetAbilityNames();
            for (int i = 0; i < keywords.Length; i++) {
                keywords[i] = keywords[i].ToLower();
            }

            // initialize the keyword recognizer with the keywords array
            keywordRecognizer = new KeywordRecognizer(keywords);
           
            StartCoroutine(TakeTurn());
        }

        private void OnPhraseRecognized(PhraseRecognizedEventArgs args) {
            if (string.IsNullOrEmpty(args.text)) {
                Debug.LogWarning("No phrase was recognized");
                return;
            }

            string recognizedText = args.text.ToLower();
            string[] abilityKeyword = playerStats.GetAbilityNames();
            for (int i = 0; i < abilityKeyword.Length; i++) {
                abilityKeyword[i] = abilityKeyword[i].ToLower();
            }

            if (Array.IndexOf(abilityKeyword, recognizedText) < 0) {
                Debug.LogWarning("Phrase not recognized as a valid keyword: " + recognizedText);
                return;
            }

            Debug.Log("Phrase recognized: " + recognizedText);
            playerActionSuccess = true;
            keywordRecognizer.Stop();

            if (Array.IndexOf(abilityKeyword, recognizedText) >= 0) {
                if (args.confidence == ConfidenceLevel.High || args.confidence == ConfidenceLevel.Medium || args.confidence == ConfidenceLevel.Low) {
                    switch (args.confidence) {
                        case ConfidenceLevel.High:
                            Debug.Log("High confidence");
                            PlayerUseAbility(recognizedText);
                            break;
                        case ConfidenceLevel.Medium:
                            Debug.Log("Medium confidence");
                            PlayerUseAbility(recognizedText);
                            break;
                        case ConfidenceLevel.Low:
                            Debug.LogWarning("Low confidence");
                            PlayerUseAbility(recognizedText);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        IEnumerator TakeTurn() {
            while (gameState != GameState.GameOver) {
                switch (gameState) {
                    case GameState.PlayerTurn:
                        Debug.Log("Player's turn");

                        yield return StartCoroutine(PlayerTurn());
						
                        if (WinCondition()) {
                            gameState = GameState.GameOver;
                        }
                        else {
                            if(playerActionSuccess) {
                                gameState = GameState.EnemyTurn;
                            }
                        }

                        break;
                    case GameState.EnemyTurn:
                        Debug.Log("Enemy turn");

                        yield return StartCoroutine(EnemyTurn());
                        if (WinCondition()) {
                            gameState = GameState.GameOver;
                        }
                        else {
                            gameState = GameState.PlayerTurn;
                        }
                        break;
                }
                yield return new WaitForSeconds(timeBetweenTurns);
            }
        }

        bool WinCondition() {
            if (playerStats.CurrentHealth() <= 0) {
                Debug.Log("Enemy wins!");
                return true;
            }
            if (enemyStats.CurrentHealth() <= 0) {
                Debug.Log("Player wins!");
                return true;
            }
            return false;
        }

        IEnumerator PlayerTurn() {
            Debug.Log("Waiting for player input...");
            keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            keywordRecognizer.Start();

            yield return new WaitUntil(() => playerActionSuccess);

            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            keywordRecognizer.Stop();

        }

        void PlayerUseAbility(string abilityName) {
            AbilitySO selectedAbility = null;
            foreach (AbilitySO ability in playerStats.abilities) {
                if (ability.abilityKeyword == abilityName) {
                    selectedAbility = ability;
                    break;
                }
            }

            // Check if an ability was found
            if (selectedAbility != null) {
                int playerCurrentMana = playerStats.CurrentMana();
                if (playerCurrentMana >= selectedAbility.manaCost) {

                    playerCurrentMana -= selectedAbility.manaCost;
                    switch (selectedAbility.abilityType) {
                        case AbilitySO.AbilityType.Damage:
                            playerStats.PlayAttack();
                            int damage = selectedAbility.damage + playerStats.AttackPower() - enemyStats.Defense();
                            enemyStats.TakeDamage(damage);
                            enemyStats.PlayStagger();
                            Debug.Log("Damage Dealt by " + selectedAbility.abilityName + " for " + damage);
                            break;
                        case AbilitySO.AbilityType.Buff:
                            //Trigger defense function
                            break;
                        case AbilitySO.AbilityType.Heal:
                            //Trigger heal function
                            break;
                        case AbilitySO.AbilityType.Recharge:
                            //Trigger heal function
                            break;
                    }
                    playerStats.SetCurrentMana(playerCurrentMana);
                }
                else {
                    Debug.Log("Not enough mana to use " + selectedAbility.abilityName);
                }
            }
            else {
                Debug.Log(selectedAbility.abilityName + " not found");
            }
        }

        IEnumerator EnemyTurn() {
            Debug.Log("Waiting for enemy input...");
            int enemyCurrentMana = enemyStats.CurrentMana();

            if (enemyCurrentMana > 0) {

                int abilityIndex = UnityEngine.Random.Range(0, enemyStats.abilities.Length);
                AbilitySO selectedAbility = enemyStats.abilities[abilityIndex];
                switch (selectedAbility.abilityType) {
                    case AbilitySO.AbilityType.Damage:
                        int damage = enemyStats.abilities[abilityIndex].damage + enemyStats.AttackPower() - playerStats.Defense();
                        // Play attack animation
                        enemyStats.PlayAttack();

                        playerStats.TakeDamage(damage);
                        playerStats.PlayStagger();
                        Debug.Log("Damage Dealt by " + enemyStats.abilities[abilityIndex].abilityName + " for " + damage);
                        break;
                    case AbilitySO.AbilityType.Buff:
                        //Trigger defense function
                        break;
                    case AbilitySO.AbilityType.Heal:
                        //Trigger heal function
                        break;
                }
                enemyCurrentMana -= selectedAbility.manaCost;
                enemyStats.SetCurrentMana(enemyCurrentMana);
            }
            else {
                Debug.Log("Enemy run out of mana");
            }
            playerActionSuccess = false;
            yield return null;
        }
    }
}