using UnityEngine;
using System.Collections;

public class Fading : MonoBehaviour {

    public float fadeTime = 1.0f;
    
    private float fadeSpeed;

    private int drawDepth = -1000;
    private float alpha = 0.0f;
    private int fadeDir = -1;

    private Texture2D fadeOutTexture;
    private Color color;
    private Rect screenRect;

    private bool isBeginFade = false;


    void Start() {
        fadeOutTexture = new Texture2D(1, 1);
        fadeOutTexture.SetPixel(0, 0, Color.black);
        fadeOutTexture.Apply();

        screenRect = new Rect(0, 0, Screen.width, Screen.height);

        fadeSpeed = 1.0f / fadeTime;

        BeginFade(-1);
    }

    void OnGUI() {
        if (isBeginFade) {
            alpha += fadeDir * fadeSpeed * Time.deltaTime;
            alpha = Mathf.Clamp01(alpha);

            color = GUI.color;
            color.a = alpha;

            GUI.color = color;
            GUI.depth = drawDepth;
            GUI.DrawTexture(screenRect, fadeOutTexture);
        }
    }

    // void OnLevelWasLoaded(int level) {
    //     BeginFade(-1);
    // }

    public float BeginFade(int direction) {
        fadeDir = direction;
        alpha = direction > 0 ? 0.0f : 1.0f;
        isBeginFade = true;
        return fadeTime;
    }
}
