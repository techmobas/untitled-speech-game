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
        public CharacterStats playerStats;
        public CharacterStats enemyStats;
        private string[] keywords;

        private KeywordRecognizer keywordRecognizer;


        private GameState gameState;
        public bool isPlayerTurn;
        [SerializeField] float timeBetweenTurns;

        void Start() {
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
                        gameState = GameState.EnemyTurn;
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

            while (keywordRecognizer.IsRunning) {
                yield return null;
            }
            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            keywordRecognizer.Stop();
            //yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            //PlayerUseAbility("Attack");
        }

        void PlayerUseAbility(string abilityName) {
            AbilitySO selectedAbility = null;
            foreach (AbilitySO ability in playerStats.abilities) {
                if (ability.abilityName == abilityName) {
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
                            int damage = selectedAbility.damage + playerStats.AttackPower() - enemyStats.Defense();
                            enemyStats.TakeDamage(damage);
                            Debug.Log("Damage Dealt by " + selectedAbility.abilityName + " for " + damage);
                            break;
                        case AbilitySO.AbilityType.Defense:
                            //Trigger defense function
                            break;
                        case AbilitySO.AbilityType.Heal:
                            //Trigger heal function
                            break;
                    }
                    enemyStats.SetCurrentMana(playerCurrentMana);
                }
                else {
                    Debug.Log("Not enough mana to use " + abilityName);
                }
            }
            else {
                Debug.Log(abilityName + " not found");
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
                        playerStats.TakeDamage(damage);
                        Debug.Log("Damage Dealt by " + enemyStats.abilities[abilityIndex].abilityName + " for " + damage);
                        break;
                    case AbilitySO.AbilityType.Defense:
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
            yield return null;
        }
    }
}



