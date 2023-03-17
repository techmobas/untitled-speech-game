using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace USG.Character {

    public class StatusUI : MonoBehaviour {
        CharacterStats chara;
        [SerializeField] TextMeshProUGUI hpText;
        [SerializeField] TextMeshProUGUI manaText;

        float uiHP;
        float uiMana;


        void Start() {
            chara = GetComponentInParent<CharacterStats>();

        }

        // Update is called once per frame
        void Update() {
            uiHP = Mathf.Max(chara.CurrentHealth());
            uiMana = Mathf.Max(chara.CurrentMana());

            hpText.text = $"HP: {Mathf.RoundToInt(uiHP)}";
            manaText.text = $"SP: {Mathf.RoundToInt(uiMana)}";
        }
    }
}
