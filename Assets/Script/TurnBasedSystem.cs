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

            // Start recording audio from the microphone
            AudioClip audioClip = Microphone.Start(null, false, 10, 44100);
            float startTime = Time.time;

            keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            keywordRecognizer.Start();

            yield return new WaitUntil(() => playerActionSuccess);

            Microphone.End(null);

            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            keywordRecognizer.Stop();

            if (audioClip != null) {
                // Save the audio clip if there was a successful action
                if (playerActionSuccess) {
                    float endTime = Time.time;
                    float recordingLength = endTime - startTime;

                    // Save the audio clip to a WAV file in the project's Assets folder
                    SavWav.Save("Assets/player_action_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".wav", audioClip);
                }
                // If there was no successful action, discard the audio clip
                else {
                    Destroy(audioClip);
                }
            }
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
                            playerStats.PlayAttack();

                            float realDamage;
                            
                            float rawOutcome = selectedAbility.damage + (playerStats.AttackPower() * .5f) - enemyStats.Defense();

                            if (UnityEngine.Random.Range(0f, 1f) >= playerStats.GetCC()) {
                                
                                float critOutcome = rawOutcome * (1 + playerStats.GetCD());
                                realDamage = critOutcome;
                                Debug.Log("Woooo Yeah Baby! That's what i'm waiting for, that what is all about");
                            }
							else {
                                realDamage = rawOutcome;
							}
                            enemyStats.TakeDamage(realDamage);
                            enemyStats.PlayStagger();
                            Debug.Log("Damage Dealt by " + selectedAbility.abilityName + " for " + realDamage);
                            break;
                        case AbilitySO.AbilityType.Heal:
                            playerStats.PlayAttack();

                            float heal = selectedAbility.damage;
                            playerStats.ModifyAttribute(heal, Character.Attribute.Attack);
                            break;
                        case AbilitySO.AbilityType.Recharge:
                            playerStats.PlayAttack();

                            float mana_charge = selectedAbility.damage;
                            playerStats.ModifyAttribute(mana_charge, Character.Attribute.Attack);
                           
                            break;
                        case AbilitySO.AbilityType.ATKBuff:
                            playerStats.PlayAttack();

                            float atk_buff = selectedAbility.damage;
                            playerStats.ModifyAttribute(atk_buff, Character.Attribute.Attack);

                            break;
                        case AbilitySO.AbilityType.DEFBuff:
                            playerStats.PlayAttack();

                            float def_buff = selectedAbility.damage;
                            playerStats.ModifyAttribute(def_buff, Character.Attribute.Defense);

                            break;
                        case AbilitySO.AbilityType.CCBuff:
                            playerStats.PlayAttack();

                            float cc_buff = selectedAbility.damage;
                            playerStats.ModifyAttribute(cc_buff, Character.Attribute.CritChance);

                            break;
                        case AbilitySO.AbilityType.CDBuff:
                            playerStats.PlayAttack();

                            float cd_buff = selectedAbility.damage;
                            playerStats.ModifyAttribute(cd_buff, Character.Attribute.CritDamage);

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
            float enemyCurrentMana = enemyStats.CurrentMana();

            if (enemyCurrentMana > 0) {

                int abilityIndex = UnityEngine.Random.Range(0, enemyStats.abilities.Length);
                AbilitySO selectedAbility = enemyStats.abilities[abilityIndex];
                switch (selectedAbility.abilityType) {
                    case AbilitySO.AbilityType.Damage:
                        float realDamage;
                        float randomValue = UnityEngine.Random.Range(0f, 1f);

                        float rawOutcome = enemyStats.abilities[abilityIndex].damage + enemyStats.AttackPower() - playerStats.Defense();

                        if (randomValue > enemyStats.GetCC()) {

                            float critOutcome = rawOutcome * (1 + enemyStats.GetCD());
                            realDamage = critOutcome;
                            Debug.Log("Woooo Yeah Baby! That's what i'm waiting for, that what is all about");
                        }
                        else {
                            realDamage = rawOutcome;
                        }

                        // Play attack animation
                        enemyStats.PlayAttack();

                        playerStats.TakeDamage(realDamage);
                        playerStats.PlayStagger();
                        Debug.Log("Damage Dealt by " + enemyStats.abilities[abilityIndex].abilityName + " for " + realDamage);
                        break;
                    case AbilitySO.AbilityType.Heal:
                        enemyStats.PlayAttack();
                        float heal = enemyStats.abilities[abilityIndex].damage;
                        enemyStats.ModifyAttribute(heal, Character.Attribute.Health);

                        Debug.Log("Buff Type " + enemyStats.abilities[abilityIndex].abilityName);
                        break;
                    case AbilitySO.AbilityType.Recharge:
                        enemyStats.PlayAttack();
                        float manaCharge = enemyStats.abilities[abilityIndex].damage;
                        enemyStats.ModifyAttribute(manaCharge, Character.Attribute.Mana);

                        Debug.Log("Buff Type " + enemyStats.abilities[abilityIndex].abilityName);
                        break;
                    case AbilitySO.AbilityType.ATKBuff:
                        enemyStats.PlayAttack();
                        float atk_buff = enemyStats.abilities[abilityIndex].damage;
                        enemyStats.ModifyAttribute(atk_buff, Character.Attribute.Attack);
                        break;
                    case AbilitySO.AbilityType.DEFBuff:
                        enemyStats.PlayAttack();
                        float def_buff = enemyStats.abilities[abilityIndex].damage;
                        enemyStats.ModifyAttribute(def_buff, Character.Attribute.Defense);
                        break;
                    case AbilitySO.AbilityType.CCBuff:
                        enemyStats.PlayAttack();
                        float cc_buff = enemyStats.abilities[abilityIndex].damage;
                        enemyStats.ModifyAttribute(cc_buff, Character.Attribute.CritChance);
                        break;
                    case AbilitySO.AbilityType.CDBuff:
                        enemyStats.PlayAttack();
                        float cd_buff = enemyStats.abilities[abilityIndex].damage;
                        enemyStats.ModifyAttribute(cd_buff, Character.Attribute.CritDamage);
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