using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
	[SerializeField] float delayTime;

	public void LoadNextScene() {
		Delay(delayTime);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		SceneManager.LoadScene("Persistent", LoadSceneMode.Additive);
	}

	IEnumerator Delay(float delay) {
		yield return new WaitForSeconds(delay);
	}

}
