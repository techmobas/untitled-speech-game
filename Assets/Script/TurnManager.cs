using System.Collections;
using System;
using System.Collections.Generic;
using USG.Character;
using UnityEngine;
using UnityEngine.Windows.Speech;
using MyBox;

namespace USG.Mechanics {
    public enum GameState {
        PlayerTurn,
        EnemyTurn,
        GameOver
    }

    public class TurnManager : MonoBehaviour {
        public GameObject player;
        public GameObject enemy;

        CharacterStats playerStats;
        CharacterStats enemyStats;

        private string[] keywords;

        private KeywordRecognizer keywordRecognizer;

        private GameState gameState;
        
        [SerializeField] float timeBetweenTurns;
        bool playerActionSuccess;
        float realDamage;

        [SerializeField][ReadOnly]private int turnCounter;

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
                        turnCounter += 1;

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
                        turnCounter += 1;
                        

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
            playerStats.UpdateBuffs();

            // Start recording audio from the microphone
            AudioClip audioClip = Microphone.Start(null, false, 10, 44100);
            float startTime = Time.time;

            keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            keywordRecognizer.Start();

            yield return new WaitUntil(() => playerActionSuccess);
            
            Microphone.End(null);

            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            keywordRecognizer.Stop();

            //if (audioClip != null) {
            //    // Save the audio clip if there was a successful action
            //    if (playerActionSuccess) {
            //        float endTime = Time.time;
            //        float recordingLength = endTime - startTime;

            //        // Save the audio clip to a WAV file in the project's Assets folder
            //        SavWav.Save("Assets/player_action_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".wav", audioClip);
            //    }
            //    // If there was no successful action, discard the audio clip
            //    else {
            //        Destroy(audioClip);
            //    }
            //}
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
                float playerCurrentMana = playerStats.CurrentMana();
                if (playerCurrentMana >= selectedAbility.manaCost) {

                    playerCurrentMana -= selectedAbility.manaCost;
                    switch (selectedAbility.abilityType) {
                        case AbilitySO.AbilityType.Damage:
                            playerStats.PlayAttack(1);

                            float rawOutcome = selectedAbility.damage + (playerStats.AttackPower() * .5f) - enemyStats.Defense();

                            if (UnityEngine.Random.Range(0f, 1f) >= playerStats.GetCC()) {
                                playerStats.isCritical = true;
                                float critOutcome = rawOutcome * (1 + playerStats.GetCD());
                                realDamage = critOutcome;

                                Debug.Log("Woooo Yeah Baby! That's what i'm waiting for, that what is all about");
                            }
							else {
                                playerStats.isCritical = false;
                                realDamage = rawOutcome;
							}
                            enemyStats.TakeDamage(realDamage);
                            enemyStats.PlayStagger();
                            Debug.Log("Damage Dealt by " + selectedAbility.abilityName + " for " + realDamage);
                            playerStats.SetCurrentMana(playerCurrentMana);
                            break;

                        case AbilitySO.AbilityType.Charge:
                            playerStats.PlayAttack(0);

							if (selectedAbility.chargeType == AbilitySO.ChargeType.Heal) {
                                float playerHP = playerStats.CurrentHealth();
                                float regenValue = selectedAbility.damage;
                                playerHP += regenValue;

                                playerStats.Regenerate(regenValue, Color.green);

                                playerStats.SetCurrentHealth(playerHP);
                                playerStats.SetCurrentMana(playerCurrentMana);
                                

                                Debug.Log("Using " + selectedAbility.abilityName);
                            }
                            else if (selectedAbility.chargeType == AbilitySO.ChargeType.Mana) {
                                float regenValue = selectedAbility.damage;
                                playerCurrentMana += regenValue;

                                playerStats.Regenerate(regenValue, Color.cyan);

                                playerStats.SetCurrentMana(playerCurrentMana);
                                Debug.Log("Using " + selectedAbility.abilityName);
                            }
                            break;
                        case AbilitySO.AbilityType.Buff:
                            playerStats.PlayAttack(0);

                            playerStats.ApplyBuff(selectedAbility);
                        
                            playerStats.SetCurrentMana(playerCurrentMana);
                            Debug.Log("Added " + selectedAbility.abilityName);
                            break;
                    }
                   
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
            enemyStats.UpdateBuffs();
            float enemyCurrentMana = enemyStats.CurrentMana();

            if (enemyCurrentMana > 0) {

                int abilityIndex = UnityEngine.Random.Range(0, enemyStats.abilities.Length);
                AbilitySO selectedAbility = enemyStats.abilities[abilityIndex];
                enemyCurrentMana -= selectedAbility.manaCost;
                switch (selectedAbility.abilityType) {
                    case AbilitySO.AbilityType.Damage:
                        float rawOutcome = selectedAbility.damage + enemyStats.AttackPower() - playerStats.Defense();

                        if (UnityEngine.Random.Range(0f, 1f) > enemyStats.GetCC()) {
                            enemyStats.isCritical = true;
                            float critOutcome = rawOutcome * (1 + enemyStats.GetCD());
                            realDamage = critOutcome;
                            Debug.Log("Woooo Yeah Baby! That's what i'm waiting for, that what is all about");
                        }
                        else {
                            enemyStats.isCritical = false;
                            realDamage = rawOutcome;
                        }

                        // Play attack animation
                        enemyStats.PlayAttack(1);

                        playerStats.TakeDamage(realDamage);
                        playerStats.PlayStagger();
                        Debug.Log("Damage Dealt by " + selectedAbility.abilityName + " for " + realDamage);
                        break;
                    case AbilitySO.AbilityType.Charge:
                        enemyStats.PlayAttack(0);

                        if (selectedAbility.chargeType == AbilitySO.ChargeType.Heal) {
                            float enemyHP = enemyStats.CurrentHealth();
                            float regenValue = selectedAbility.damage;
                            enemyHP += regenValue;

                            enemyStats.Regenerate(regenValue, Color.green);

                            enemyStats.SetCurrentHealth(enemyHP);
                            enemyStats.SetCurrentMana(enemyCurrentMana);
                            Debug.Log("Using " + selectedAbility.abilityName);
                        }
                        else if (selectedAbility.chargeType == AbilitySO.ChargeType.Mana) {
                            float regenValue = selectedAbility.damage;
                            enemyCurrentMana += regenValue;

                            enemyStats.Regenerate(regenValue, Color.cyan);

                            enemyStats.SetCurrentMana(enemyCurrentMana);
                            Debug.Log("Using " + selectedAbility.abilityName);
                        }
                        break;
                    case AbilitySO.AbilityType.Buff:
                        enemyStats.PlayAttack(0);

                        enemyStats.ApplyBuff(selectedAbility);
                        playerStats.SetCurrentMana(enemyCurrentMana);
                        Debug.Log("Added " + selectedAbility.abilityName);
                        break;
                }
            }
            else {
                Debug.Log("Enemy run out of mana");
            }
            playerActionSuccess = false;
            yield return null;
        }
    }
}