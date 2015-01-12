using UnityEngine;
using UnityEditor;
using System.Linq;

// Add a button to the editor to sort the river file list by name.
[CustomEditor(typeof(GameManager))]
public class GameManagerFileSorter : Editor {
  public override void OnInspectorGUI () {
    DrawDefaultInspector();
    if (GUILayout.Button("Sort by name")) {
      var currentTarget = (GameManager) target;
      currentTarget.StoryFiles = currentTarget.StoryFiles.OrderBy(x => x.name).ToList();
    }
  }

  int compareNames(TextAsset a, TextAsset b) {
    return a.name.CompareTo(b.name);
  }
}
