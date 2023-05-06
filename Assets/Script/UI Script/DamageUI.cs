using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace USG.UI {
	public class DamageUI : MonoBehaviour {
		private float speed;
		private Vector3 direction;
		private float fadeTime;

		[Header("Critical Hit")]
		public AnimationClip critAnimation;
		private bool isCrit;

		private void Update() {
			if (!isCrit) {
				float translation = speed * Time.deltaTime;
				transform.Translate(direction * translation);
			}
		}

		public void InitializeText(float speed, Vector3 direction, float fadeTime, bool isCrit) {
			this.speed = speed;
			this.direction = direction;
			this.fadeTime = fadeTime;
			this.isCrit = isCrit;

			if (isCrit) {
				GetComponent<Animator>().SetTrigger("Critical");
				StartCoroutine(Critical());
			}
			else {
				StartCoroutine(FadeOut());
			}
		}

		IEnumerator Critical() {
			yield return new WaitForSeconds(critAnimation.length);
			isCrit = false;
			StartCoroutine(FadeOut());
		}

		IEnumerator FadeOut() {
			float startAlpha = GetComponent<TextMeshProUGUI>().color.a;

			float rate = 1.0f / fadeTime;
			float progress = 0.0f;

			while (progress < 1.0) {
				Color tmpColor = GetComponent<TextMeshProUGUI>().color;
				GetComponent<TextMeshProUGUI>().color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, Mathf.Lerp(startAlpha, 0, progress));

				progress += rate * Time.deltaTime;
				yield return null;
			}
			Destroy(gameObject);
		}
	}
}