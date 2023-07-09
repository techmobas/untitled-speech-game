using System.Collections;
using System;
using System.Collections.Generic;
using USG.Character;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.Events;
using MyBox;
using TMPro;
using UnityEngine.SceneManagement;

namespace USG.Mechanics {
    public enum GameState {
        PlayerTurn,
        EnemyTurn,
        GameOver
    }

    public class TurnManager : MonoBehaviour {
        //public GameObject player;
        //public GameObject enemy;

        [Header("Character Stats")]
        private CharacterStats playerStats;
        private CharacterStats enemyStats;

        [Header("Speech Components")]
        private string[] keywords;
        private KeywordRecognizer keywordRecognizer;

        float realDamage;

        [Header("Turn Based Manager")]
        private GameState gameState;
        bool playerActionSuccess;
        [SerializeField] float timeBetweenTurns;
        [SerializeField][ReadOnly] private int turnCounter = 1;

        [Header("Game Condition")]
        public UnityEvent winCondition;
        public UnityEvent loseCondition;

        [Header("UI Shenanigans")]
        [SerializeField] TextMeshProUGUI subtitle;
        [SerializeField] TextMeshProUGUI levelConf;
        [SerializeField] TextMeshProUGUI turnSub;

        [SerializeField] GameObject playerLogo;
        [SerializeField] GameObject enemyLogo;

        void Start() {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) {
                playerStats = playerObj.GetComponent<CharacterStats>();
            }
            else {
                Debug.LogError("TurnManager: Could not find Player object.");
            }

            GameObject enemyObject = GameObject.FindGameObjectWithTag("Enemy");
            if (enemyObject != null) {
                enemyStats = enemyObject.GetComponent<CharacterStats>();
            }
            else {
                Debug.LogError("Could not find enemy with tag 'Enemy'");
            }


            // Add the player's abilities as keywords for the PhraseRecognizer
            keywords = playerStats.GetAbilityNames();
            for (int i = 0; i < keywords.Length; i++) {
                keywords[i] = keywords[i].ToLower();
            }

            // initialize the keyword recognizer with the keywords array
            keywordRecognizer = new KeywordRecognizer(keywords);

            turnSub.text = "Turn(s) : " + turnCounter.ToString();
            StartCoroutine(TakeTurn());
        }

		#region Speech System
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

            if (recognizedText == "exit") {
                Debug.Log("Player wants to exit the application");
                Application.Quit();
                return;
            }

            if (Array.IndexOf(abilityKeyword, recognizedText) < 0) {
                Debug.LogWarning("Phrase not recognized as a valid keyword: " + recognizedText);
                return;
            }

            Debug.Log("Phrase recognized: " + recognizedText);
            subtitle.text = "Player says : " + recognizedText.ToUpper(); 

            keywordRecognizer.Stop();

            if (Array.IndexOf(abilityKeyword, recognizedText) >= 0) {
                if (args.confidence == ConfidenceLevel.High || args.confidence == ConfidenceLevel.Medium || args.confidence == ConfidenceLevel.Low) {
                    switch (args.confidence) {
                        case ConfidenceLevel.High:
                            Debug.Log("High confidence");
                            PlayerUseAbility(recognizedText);

                            levelConf.text = "High confidence";
                            levelConf.color = Color.green;
                            break;
                        case ConfidenceLevel.Medium:
                            Debug.Log("Medium confidence");
                            PlayerUseAbility(recognizedText);

                            levelConf.text = "Medium confidence";
                            levelConf.color = Color.yellow;
                            break;
                        case ConfidenceLevel.Low:
                            Debug.LogWarning("Low confidence");
                            PlayerUseAbility(recognizedText);

                            levelConf.text = "Low confidence";
                            levelConf.color = Color.white;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
		#endregion

		#region Take Turns
		IEnumerator TakeTurn() {
            while (gameState != GameState.GameOver) {
                switch (gameState) {
                    case GameState.PlayerTurn:
                        Debug.Log("Player's turn");
                        playerLogo.SetActive(true);
                        enemyLogo.SetActive(false);

                        yield return StartCoroutine(PlayerTurn());
                        turnCounter += 1;
                        turnSub.text = "Turn(s) : " + turnCounter.ToString();

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
                        turnSub.text = "Turn(s) : " + turnCounter.ToString();

                        playerLogo.SetActive(false);
                        enemyLogo.SetActive(true);
                    
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
                loseCondition.Invoke();
                return true;
            }
            if (enemyStats.CurrentHealth() <= 0) {
                Debug.Log("Player wins!");
                winCondition.Invoke();
                return true;
            }
            return false;
        }
		#endregion

		#region Player Turn
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

                            if (UnityEngine.Random.Range(0f, 1f) <= playerStats.GetCC()) {
                                float critOutcome = rawOutcome * (1 + playerStats.GetCD());
                                realDamage = critOutcome;

                                Debug.Log("Woooo Yeah Baby! That's what i'm waiting for, that what is all about");
                                enemyStats.TakeDamage(realDamage, Color.yellow, true);
                            }
							else {
                                realDamage = rawOutcome;
                                enemyStats.TakeDamage(realDamage, Color.white, false);
                            }

                            Debug.Log("Damage Dealt by " + selectedAbility.abilityName + " for " + realDamage);
                            playerStats.SetCurrentMana(playerCurrentMana);

                            enemyStats.SpawnEffect(selectedAbility.effect);
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

                        case AbilitySO.AbilityType.Debuff:
                            playerStats.PlayAttack(0);

                            enemyStats.ApplyDebuff(selectedAbility);
                            playerStats.SetCurrentMana(playerCurrentMana);

                            Debug.Log("Added " + selectedAbility.abilityName);
                            break;
                    }
                   
                }
                else {
                    Debug.Log("Not enough mana to use " + selectedAbility.abilityName);
                    playerStats.StatusUIText("Not Enough Mana", Color.cyan);
                }
            }
            else {
                Debug.Log(selectedAbility.abilityName + " not found");
                playerStats.StatusUIText("Forbidden", Color.red);
            }
            playerActionSuccess = true;
        }

        #endregion

        #region Enemy Turn
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

                        if (UnityEngine.Random.Range(0f, 1f) <= enemyStats.GetCC()) {
                            float critOutcome = rawOutcome * (1 + enemyStats.GetCD());
                            realDamage = critOutcome;
                            Debug.Log("Woooo Yeah Baby! That's what i'm waiting for, that what is all about");

                            playerStats.TakeDamage(realDamage, Color.yellow, true);
                        }
                        else {
                            realDamage = rawOutcome;
                            playerStats.TakeDamage(realDamage, Color.white, false);
                        }

                        // Play attack animation
                        enemyStats.PlayAttack(1);

                        enemyStats.SetCurrentMana(enemyCurrentMana);
                        Debug.Log("Damage Dealt by " + selectedAbility.abilityName + " for " + realDamage);

                        playerStats.SpawnEffect(selectedAbility.effect);
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
                        enemyStats.SetCurrentMana(enemyCurrentMana);

                        
                        Debug.Log("Added " + selectedAbility.abilityName);
                        break;
                    case AbilitySO.AbilityType.Debuff:
                        enemyStats.PlayAttack(0);

                        playerStats.ApplyDebuff(selectedAbility);
                        enemyStats.SetCurrentMana(enemyCurrentMana);

                        Debug.Log("Added " + selectedAbility.abilityName);
                        break;
                }
            }
            else {
                Debug.Log("Enemy run out of mana");
                enemyStats.StatusUIText("Not Enough Mana", Color.cyan);
            }
            playerActionSuccess = false;
            yield return null;
        }
		#endregion
	}
}