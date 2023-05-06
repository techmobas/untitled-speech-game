using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using USG.Character;
using UnityEngine.UI;
using TMPro;

namespace USG.UI {
    public class StatusEffectUI : MonoBehaviour {

        [Header("Character Stats")]
        public CharacterStats chara;
        [SerializeField] TextMeshProUGUI charaName;

        [Header("Container")]
        [SerializeField] RectTransform columnGroup;
        [SerializeField] GameObject columnPrefab;

        [Space]
        [SerializeField] Color buffColor;
        [SerializeField] Color debuffColor;

        private List<AbilitySO> activeBuffs = new List<AbilitySO>();

		private void Start() {
            charaName.text = chara.name;
		}

		private void Update() {
            UpdateUI();
            // Check for changes in active buffs and update UI accordingly
            if (!activeBuffs.SequenceEqual(chara.activeBuffs)) {
                UpdateUI();
            }
        }

        private void UpdateUI() {
            // Clear previous columns
            foreach (Transform child in columnGroup.transform) {
                Destroy(child.gameObject);
            }

            // Add columns for each active buff
            foreach (AbilitySO activeBuff in chara.activeBuffs) {
                GameObject column = Instantiate(columnPrefab, columnGroup);
                column.transform.localScale = Vector3.one;

                Image buffIcon = column.transform.Find("Buff Icon").GetComponent<Image>();
                buffIcon.sprite = activeBuff.icon;

                TextMeshProUGUI abilityName = column.transform.Find("Ability Name").GetComponent<TextMeshProUGUI>();
                abilityName.text = activeBuff.abilityName.ToUpper();

                TextMeshProUGUI abilityDesc = column.transform.Find("Ability Description").GetComponent<TextMeshProUGUI>();
                abilityDesc.text = activeBuff.abilityDescription;

                TextMeshProUGUI duration = column.transform.Find("Duration").GetComponent<TextMeshProUGUI>();
                duration.text = activeBuff.duration.ToString();

                if (activeBuff.abilityType == AbilitySO.AbilityType.Buff) {
                    abilityName.color = buffColor;
                    abilityDesc.color = buffColor;
                    duration.color = buffColor;
                }
                else if (activeBuff.abilityType == AbilitySO.AbilityType.Debuff) {
                    abilityName.color = debuffColor;
                    abilityDesc.color = debuffColor;
                    duration.color = debuffColor;
                }
            }

            // Update active buffs list
            activeBuffs = new List<AbilitySO>(chara.activeBuffs);
        }
    }
}