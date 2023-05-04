/**Class for singleton monobehaviours such as managers or controllers
 * Ref: VF.GameControllerSingleton with slight modification
 * Stolen From StellaGale : The Trials of Faith by Extra Life Enterteinment (18/4/2023)
 */

using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
	static readonly object instanceLock = new object();

	static T _instance = null;
	public static T Instance {
		get {
			if (_instance != null) {
				return _instance;
			}
			else {
				Debug.LogWarning("Warning: Attempted to call instance of " + typeof(T) + ", but no instance has been initialized.");
				return null;
			}
		}
		private set {
			lock (instanceLock) {
				if (_instance != null) {
					Debug.LogWarning("Warning: Singleton " + typeof(T) + " is not singular, existing instance is deleted.", value.gameObject);
					Destroy(_instance.gameObject);
				}
				_instance = value;
			}
		}
	}

	public static bool IsInstantiated {
		get {
			return _instance != null ? true : false;
		}
	}

	protected virtual void Awake() {
		Instance = GetComponent<T>();
	}
} 

