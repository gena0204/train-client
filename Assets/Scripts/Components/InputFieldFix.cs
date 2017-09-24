// 修正Android部分手機輸入有時不會回傳文字
// http://forum.unity3d.com/threads/android-touchscreenkeyboard-not-always-returning-value-to-inputfield.317047/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// TODO: remove this component when unity bug will be resolved
[RequireComponent(typeof(InputField))]
public class InputFieldFix : MonoBehaviour {
    private string backUpString = "";
    private InputField input;
    private bool isFinishingEditing;

    void Start() {
        isFinishingEditing = true;
        OnChangeValue(" ");

        input = GetComponent<InputField>();
        input.onValueChanged.AddListener(OnChangeValue);
        input.onEndEdit.AddListener(OnEndEdit);
    }

    void OnDestroy() {
        input.onValueChanged.RemoveAllListeners();
        input.onEndEdit.RemoveAllListeners();
    }

    void OnChangeValue(string value) {
        if (string.IsNullOrEmpty(value) && isFinishingEditing == true) {
            input.text = backUpString;
            isFinishingEditing = false;
        }
        backUpString = value;
    }

    void OnEndEdit(string value) {
        isFinishingEditing = true;
    }

	public void setText(string str) {
		backUpString = str;
		input.text = backUpString;
	}
}
