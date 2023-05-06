using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using USG.Character;

namespace USG.UI {

    public class StatusUI : MonoBehaviour {
        CharacterStats chara;

        [SerializeField] Slider hpSlider;
        [SerializeField] Slider manaSlider;


        void Start() {
            chara = GetComponentInParent<CharacterStats>();

            hpSlider.maxValue = chara.MaxHealth();
            manaSlider.maxValue = chara.MaxMana();
        }



        // Update is called once per frame
        void Update() {
            hpSlider.value = chara.CurrentHealth();
            manaSlider.value = chara.CurrentMana();
        }
    }
}
