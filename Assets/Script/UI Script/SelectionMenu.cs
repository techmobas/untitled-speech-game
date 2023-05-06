using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Malee.List;
using DG.Tweening;

public class SelectionMenu : MonoBehaviour {
	[SerializeField] protected Button buttonPrefab;
	[SerializeField] protected float fadeDuration = .5f;
	[SerializeField] protected Transform buttonContainer;

	[SerializeField, Reorderable] protected MenuButtonList buttonList;

	public float FadeDuration {
		get => fadeDuration;
	}

	protected MenuController mc;
	protected Dictionary<Button, MenuButton> buttons;
	protected CanvasGroup canvasGroup;

	protected virtual void Awake() {
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;

		buttons = new Dictionary<Button, MenuButton>();

		InitializeButtons();

		mc = MenuController.Instance;
	}

	protected virtual void Start() {
		mc = MenuController.Instance;
	}

	public virtual void Show() {
		canvasGroup.DOFade(1f, fadeDuration);
	}

	public virtual void Hide() {
		canvasGroup.DOFade(0f, fadeDuration);
	}

	protected virtual void InitializeButtons() {
		if (buttonList != null && buttonList.Count > 0) {
			foreach (MenuButton mb in buttonList) {
				Button butt = Instantiate(buttonPrefab, buttonContainer);
				List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>(butt.GetComponentsInChildren<TextMeshProUGUI>());
				foreach (TextMeshProUGUI text in texts) {
					text.text = mb.name;
				}

				MenuButton temp = mb;
				butt.onClick.AddListener(delegate {
					ToggleButtons(false);
					if (temp.onExecute != null) {
						temp.onExecute.Invoke();
					}
				});

				butt.gameObject.SetActive(!mb.hidden);
				buttons.Add(butt, mb);
			}
		}
	}

	public virtual void ToggleButtons(bool set) {
		foreach (Button button in buttons.Keys) {
			MenuButton mb = buttons[button];

			button.interactable = set && mb.enabled;
			button.gameObject.SetActive(!mb.hidden);
		}
		if (set) SelectFirst();
	}

	public virtual void HighlightButton(Button button) {
		if (!mc) mc = MenuController.Instance;
		//if (buttons.ContainsKey(button))
		//	tmc.ShowDescription(buttons[button].description);
	}

	void SelectFirst() {
		foreach (Button button in buttons.Keys) {
			if (button.interactable) {
				button.Select();
				break;
			}
		}
	}

	[System.Serializable]
	public class MenuButtonList : ReorderableArray<MenuButton> {
	}

	[System.Serializable]
	public class MenuButton {
		public string name = "Menu Button";
		public bool enabled = true;
		public bool hidden = false;
		public UnityEvent onExecute;
	}
}