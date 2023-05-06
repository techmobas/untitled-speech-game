using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using USG.Character;
using UnityEngine.UI;
using TMPro;

namespace USG.UI {
	public class PlayerAbilityUI : MonoBehaviour {
		private CharacterStats player;

		[Header("Container")]
		[SerializeField] RectTransform columnGroup;
		[SerializeField] GameObject columnPrefab;

		private List<AbilitySO> playerAbility = new List<AbilitySO>();

		private void Start() {
			GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
			if (playerObj != null) {
				player = playerObj.GetComponent<CharacterStats>();
			}
			else {
				Debug.LogError("Could not find Player object.");
			}
		}

		private void Update() {
			UpdateAbility();

			if (!playerAbility.SequenceEqual(player.abilities)) {
				UpdateAbility();
			}
		}

		private void UpdateAbility() {
			foreach (Transform child in columnGroup.transform) {
				Destroy(child.gameObject);
			}

			foreach (AbilitySO abilities in player.abilities) {
				GameObject column = Instantiate(columnPrefab, columnGroup);
				column.transform.localScale = Vector3.one;

				TextMeshProUGUI abilityName = column.transform.Find("Ability Name").GetComponent<TextMeshProUGUI>();
				abilityName.text = abilities.abilityName.ToUpper();

				Image logo = column.transform.Find("Logo").GetComponent<Image>();
				logo.sprite = abilities.icon;

				TextMeshProUGUI abilityKeyword = column.transform.Find("Ability Keyword").GetComponent<TextMeshProUGUI>();
				abilityKeyword.text = abilities.abilityKeyword.ToLower();

				TextMeshProUGUI abilityDesc = column.transform.Find("Ability Description").GetComponent<TextMeshProUGUI>();
				abilityDesc.text = abilities.abilityDescription;

				TextMeshProUGUI manaCost = column.transform.Find("Mana Cost").GetComponent<TextMeshProUGUI>();
				manaCost.text = abilities.manaCost.ToString();
			}

			// Update active buffs list
			playerAbility = new List<AbilitySO>(player.abilities);
		}
	}
}