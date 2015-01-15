using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BackgroundManager : MonoBehaviour {
  public static BackgroundManager Instance;

  public List<Material> Backgrounds;

  private Dictionary<string, Material> bgMap;

  void Awake() {
    Instance = this;
    bgMap = new Dictionary<string, Material>();
    foreach (var bg in Backgrounds) {
      bgMap.Add(bg.name, bg);
    }
  }

  public Material GetBackground(string name) {
    return bgMap[name];
  }
}
