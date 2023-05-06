using System.Collections;	
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using USG.Character;


namespace USG.UI {
	public class CharacterStatsUI : MonoBehaviour {
		[Header("Character Stats")]
		public CharacterStats chara;
		[SerializeField] TextMeshProUGUI charaName;

		[Header("UI Parts")]
		[SerializeField] TextMeshProUGUI hpInfo;
		[SerializeField] TextMeshProUGUI manaInfo;
		[SerializeField] TextMeshProUGUI atkInfo;
		[SerializeField] TextMeshProUGUI defInfo;
		[SerializeField] TextMeshProUGUI ccInfo;
		[SerializeField] TextMeshProUGUI cdmgInfo;


		private void Start() {
			charaName.text = chara.name;
		}
		private void Update() {
			hpInfo.text = "HP : " + chara.MaxHealth();
			manaInfo.text = "MANA : " + chara.MaxMana();
			atkInfo.text = "ATK : " + chara.AttackPower();
			defInfo.text = "DEF : " + chara.Defense();
			ccInfo.text = "CC : " + 100 * (chara.GetCC()) + "%";
			cdmgInfo.text = "CDMG : " + 100 * (chara.GetCD()) + "%";
		}


	}
}