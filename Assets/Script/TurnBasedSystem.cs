using System.Collections;
using System;
using System.Collections.Generic;
using USG.Character;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace USG.Mechanics
{
    public class TurnBasedSystem : MonoBehaviour
    {
        public CharacterStats playerStats;
        public CharacterStats enemyStats;

        private KeywordRecognizer keywordRecognizer;

        public bool isPlayerTurn = true;
        [SerializeField] float timeBetweenTurns = 2f;
        // Start is called before the first frame update
        void Start()
        {
            // Add the player's abilities as keywords for the PhraseRecognizer
            string[] keywords = playerStats.GetAbilityNames();
            for (int i = 0; i < keywords.Length; i++)
            {
                keywords[i] = keywords[i].ToLower();
            }

            // initialize the keyword recognizer with the keywords array
            keywordRecognizer = new KeywordRecognizer(keywords);

            StartCoroutine(TakeTurn());
        }

        private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            string recognizedText = args.text.ToLower();
            string[] abilityKeyword = playerStats.GetAbilityNames();
            for (int i = 0; i < abilityKeyword.Length; i++)
            {
                abilityKeyword[i] = abilityKeyword[i].ToLower();
            }

            if (isPlayerTurn)
            {
                if (Array.IndexOf(abilityKeyword, recognizedText) >= 0)
                {
                    if (args.confidence == ConfidenceLevel.High || args.confidence == ConfidenceLevel.Medium || args.confidence == ConfidenceLevel.Low)
                    {
                        switch (args.confidence)
                        {
                            case ConfidenceLevel.High:
                                Debug.Log("High confidence");
                                PlayerUseAbility(recognizedText);
                                break;
                            case ConfidenceLevel.Medium:
                                Debug.Log("Medium confidence");
                                PlayerUseAbility(recognizedText);
                                break;
                            case ConfidenceLevel.Low:
                                Debug.Log("Low confidence");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public IEnumerator TakeTurn()
        {
            Debug.Log("Player's turn");
            keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            keywordRecognizer.Start();

            while (keywordRecognizer.IsRunning)
            {
                yield return null;
            }

            // Player has made a move, stop the keyword recognizer
            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
            keywordRecognizer.Stop();

            // Check if the player has won
            //if (CheckWinCondition())
            //{
            //    Debug.Log("Player wins!");
            //    yield break;
            //}

            yield return new WaitForSeconds(1.0f);

            // Call the enemy's turn after a delay
            yield return StartCoroutine(EnemyTurn());
        }

        public void PlayerUseAbility(string abilityName)
        {
            AbilitySO selectedAbility = null;

            // Find the ability with the specified name
            foreach (AbilitySO ability in playerStats.abilities)
            {
                if (ability.abilityName == abilityName)
                {
                    selectedAbility = ability;
                    break;
                }
            }

            // Check if an ability was found
            if (selectedAbility != null)
            {
                int playerCurrentMana = playerStats.CurrentMana();

                // Check if the player has enough mana to use the ability
                if (playerCurrentMana >= selectedAbility.manaCost)
                {
                    // Deduct the mana cost from the player's mana pool
                    playerCurrentMana -= selectedAbility.manaCost;

                    // Apply the ability's effects
                    if (selectedAbility.abilityType == AbilitySO.AbilityType.Damage)
                    {
                        // Add logic for dealing damage here
                        int damage = selectedAbility.damage + playerStats.AttackPower() - enemyStats.Defense();
                        enemyStats.TakeDamage(damage);
                        Debug.Log("Damage Dealt by " + selectedAbility.abilityName + " for " + damage);
                    }
                    else if (selectedAbility.abilityType == AbilitySO.AbilityType.Defense)
                    {
                        // Add logic for increasing defense here
                    }
                    else if (selectedAbility.abilityType == AbilitySO.AbilityType.Heal)
                    {
                        // Add logic for healing here
                    }
                }
                else
                {
                    // The player doesn't have enough mana to use the ability
                    Debug.Log("Not enough mana to use " + abilityName);
                }
            }
            else
            {
                // The ability was not found
                Debug.Log(abilityName + " not found");
            }
        }

        IEnumerator EnemyTurn()
        {
            //Check if the enemy can use an ability
            Debug.Log("enemy input");
            int enemyCurrentMana = enemyStats.CurrentMana();

            if (enemyCurrentMana > 0)
            {
                int abilityIndex = UnityEngine.Random.Range(0, enemyStats.abilities.Length);
                AbilitySO ability = enemyStats.abilities[abilityIndex];
                switch (ability.abilityType)
                {
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
                enemyCurrentMana -= ability.manaCost;
            }
            else
            {
                Debug.Log("Enemy run out of mana");
            }

            // Wait for 1 second before ending the enemy turn
            yield return new WaitForSeconds(1f);
        }
    }
}

