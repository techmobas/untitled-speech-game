using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class TurnManager : MonoBehaviour
{
    public PlayerStats playerStats;
    public EnemyStats enemyStats;

    private KeywordRecognizer keywordRecognizer;
    private PhraseRecognizer phraseRecognizer;

    public bool isPlayerTurn = true;
    public bool waitingForInput = false;

    private void Start()
    {
        // Add the player's abilities as keywords for the PhraseRecognizer
        string[] keywords = playerStats.GetAbilityNames();

        // initialize the keyword recognizer with the keywords array
        keywordRecognizer = new KeywordRecognizer(keywords);

        // register the OnPhraseRecognized event and start the keyword recognizer
       
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        switch (args.text)
        {
            case "attack":
                PlayerAttack();
                break;
            default:
                PlayerUseAbility(args.text);
                break;
        }
    }

    private void PlayerAttack()
    {
        // perform the player attack on the enemy
        enemyStats.TakeDamage(playerStats.AttackPower());
    }

    public void PlayerUseAbility(string abilityName)
    {
        AbilitySO selectedAbility = null;

        // Find the ability with the specified name
        foreach (AbilitySO ability in playerStats.abilities)
        {
            if (ability.name == abilityName)
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

    //private IEnumerator PlayerTurn()
    //{
    //    // Wait for player input
    //    phraseRecognizer.OnPhraseRecognized += HandlePhraseRecognized;
    //    keywordRecognizer.Start();

    //    while (waitingForInput)
    //    {
    //        yield return null;
    //    }
            
    //    keywordRecognizer.Stop();
    //    phraseRecognizer.OnPhraseRecognized -= HandlePhraseRecognized;
    //}

    //private void HandlePhraseRecognized(PhraseRecognizedEventArgs args)
    //{
    //    waitingForInput = false;
    //    keywordRecognizer.Stop();
    //    phraseRecognizer.OnPhraseRecognized -= HandlePhraseRecognized;
    //    PlayerUseAbility(args.text);
    //}

    private void PlayerTurn()
	{
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();
    }

	private void EnemyTurn()
	{
        //Check if the enemy can use an ability

        int enemyCurrentMana = enemyStats.CurrentMana();

        if (enemyCurrentMana > 0)
		{
			int abilityIndex = Random.Range(0, enemyStats.abilities.Length);
			AbilitySO ability = enemyStats.abilities[abilityIndex];
			switch (ability.abilityType)
			{
				case AbilitySO.AbilityType.Damage:
					int damage = enemyStats.AttackPower() - playerStats.Defense();
					playerStats.TakeDamage(damage);
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
			//Enemy skips turn

		}

		EndTurn();
	}

    public void EndTurn()
	{
        isPlayerTurn = !isPlayerTurn;
		if (isPlayerTurn)
		{
            PlayerTurn();
		}
		else
		{
            EnemyTurn();
            keywordRecognizer.Stop();
            keywordRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
        }

        CheckVictory();
	}

	private void CheckVictory()
	{
		if (playerStats.CurrentHealth() <= 0)
		{
			//Trigger player defeat function

		}
		else if (enemyStats.CurrentHealth() <= 0)
		{
			//Trigger enemy defeat function

		}
	}
}
