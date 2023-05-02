using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : Singleton<MenuController>
{
	[SerializeField] SelectionMenu selectionMenu;

	Stack<SelectionMenu> panelStack;

	protected override void Awake() {
		base.Awake();
		panelStack = new Stack<SelectionMenu>();
	}
	void Start() {
		List<SelectionMenu> panels = new List<SelectionMenu>(FindObjectsOfType<SelectionMenu>());
		foreach (SelectionMenu panel in panels) {
			if (panel == selectionMenu) {
				panel.Show();
				panel.ToggleButtons(true);
				panelStack.Push(panel);
			}
			else {
				panel.Hide();
				panel.ToggleButtons(false);
				panel.gameObject.SetActive(false);
			}
		}
	}

	public void OpenPanel(SelectionMenu panel) {
		StartCoroutine(SetActivePanelRoutine(panel));
	}

	public void CloseCurrentPanel() {
		if (panelStack.Count > 1) {
			StartCoroutine(SetActivePanelRoutine());
		}
		else {
			Debug.Log("Top of panel stack reached");
		}
	}

	IEnumerator SetActivePanelRoutine(SelectionMenu panel = null) {
		if (panelStack.Count > 0 && panelStack.Peek() != null) {
			SelectionMenu peek = panelStack.Peek();
			peek.Hide();
			yield return new WaitForSeconds(peek.FadeDuration);
			peek.gameObject.SetActive(false);
		}

		if (panel) {
			panelStack.Push(panel);
			Debug.Log($"PUSH {panelStack.Peek()}");
		}
		else {
			Debug.Log($"POP {panelStack.Peek()}");
			panelStack.Pop();
		}

		SelectionMenu top = panelStack.Peek();
		top.gameObject.SetActive(true);
		top.Show();
		top.ToggleButtons(true);
	}
}
