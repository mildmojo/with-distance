using UnityEngine;
using UnityEngine.UI;

public class AttractController : MonoBehaviour {

  [System.NonSerialized]
  public int index;

  private Text textComponent;
  private Color textColor = Color.white;
  private GameManager gameManager;

  void Awake () {
    textComponent = GetComponentInChildren<Text>();
    textColor = textComponent.color;
  }

  void Start() {
    gameManager = GameManager.Instance;
  }

  void Update () {
    if (!gameManager.AttractMode) return;
    // Camera movement is inverted for attract mode; getting closer moves cam away.
    // At maximum range, change stories.
    var maxZ = -gameManager.ZSpacing * 0.9;
    if (Camera.main.transform.position.z < maxZ) {
      Debug.Log("Leaving attract mode, " + Camera.main.transform.position.z + " < " + maxZ);
      TweenOut();
      gameManager.NextStory();
    }
  }

  public void SetIndex(int idx) {
    index = idx;
  }

  public void SetText(string text) {
    textComponent.text = text;
  }

  public void SetColor(Color newColor) {
    textColor = newColor;
  }

  public void SetColor(string hexColor) {
    textColor = HexToColor(hexColor);
  }

  public void TweenIn(float time = 2.0f) {
    LeanTween.cancel(gameObject);
    LeanTween.value(gameObject,
                    c => textComponent.color = c,
                    textComponent.color,
                    textColor,
                    time)
             .setEase(LeanTweenType.easeInQuad);
  }

  public void TweenOut(float time = 2.0f) {
    LeanTween.cancel(gameObject);
    var targetColor = textColor;
    var darkenBy = Mathf.Min(targetColor.r, targetColor.g, targetColor.b) * 0.9f;
    targetColor.r -= darkenBy;
    targetColor.g -= darkenBy;
    targetColor.b -= darkenBy;
    targetColor.a = 0;
    LeanTween.value(gameObject,
                    c => textComponent.color = c,
                    textComponent.color,
                    targetColor,
                    time)
             .setEase(LeanTweenType.easeOutQuad);
  }

  Color HexToColor(string hex) {
    byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
    byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
    byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
    return new Color32(r,g,b, 255);
  }
}
