using UnityEngine;
using UnityEngine.UI;

public class TextController : MonoBehaviour {

  [System.NonSerialized]
  public int index;

  private Text textComponent;
  private Color textColor = Color.white;

  void Awake () {
    textComponent = GetComponentInChildren<Text>();
    textColor = textComponent.color;
  }

  void Update () {

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

  public LTDescr TweenIn(float time = 1.8f) {
    LeanTween.cancel(gameObject);
    var tween = LeanTween.value(gameObject,
                                c => textComponent.color = c,
                                textComponent.color,
                                textColor,
                                time)
                         .setEase(LeanTweenType.easeOutQuad);
    return tween;
  }

  public LTDescr TweenOut(float time = 1.8f) {
    LeanTween.cancel(gameObject);
    var targetColor = textColor;
    var darkenBy = Mathf.Min(targetColor.r, targetColor.g, targetColor.b) * 0.9f;
    targetColor.r -= darkenBy;
    targetColor.g -= darkenBy;
    targetColor.b -= darkenBy;
    targetColor.a = 0;
    var tween = LeanTween.value(gameObject,
                                c => textComponent.color = c,
                                textComponent.color,
                                targetColor,
                                time)
                         .setEase(LeanTweenType.easeOutQuad);
    return tween;
  }

  Color HexToColor(string hex) {
    byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
    byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
    byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
    return new Color32(r,g,b, 255);
  }
}
