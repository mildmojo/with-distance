using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BackgroundManager : MonoBehaviour {
  public static BackgroundManager Instance;

  public List<Texture2D> Backgrounds;

  private Dictionary<string, Texture2D> bgMap;

  void Awake() {
    Instance = this;
    bgMap = new Dictionary<string, Texture2D>();
    foreach (var bg in Backgrounds) {
      bgMap.Add(bg.name, bg);
    }
  }

  public Texture2D GetBackground(string name) {
    return bgMap[name];
  }
}
