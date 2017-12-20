using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Language;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour {

	// Use this for initialization

	void Awake () {
		LanguageService.Instance.Language = new LanguageInfo ("English");
	}

	void Start()
	{
		GameObject textPrefab = (GameObject)Resources.Load ("Text");
		GameObject textObj = (GameObject)Instantiate (textPrefab);
		textObj.transform.SetParent (this.transform);
		textObj.transform.localPosition = Vector3.zero;

        transform.Find("Button").GetComponent<Button>().onClick.AddListener(delegate() {
            LanguageService.Instance.Language = new LanguageInfo ("Chinese");
			LanguageText[] texts = FindObjectsOfType(typeof(LanguageText)) as LanguageText[];
			foreach (var text in texts) {
				text.Reload();
			}
        });
	}		
}
