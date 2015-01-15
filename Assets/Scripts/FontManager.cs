using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FontManager : MonoBehaviour {
  public static FontManager Instance;

  public List<Font> Fonts;

  private Dictionary<string, Font> fontMap;

  void Awake() {
    Instance = this;
    fontMap = new Dictionary<string, Font>();
    foreach (var font in Fonts) {
      fontMap.Add(font.name, font);
    }
  }

  public Font GetFont(string name) {
    return fontMap[name];
  }
}
