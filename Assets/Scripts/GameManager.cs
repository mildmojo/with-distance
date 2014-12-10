using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
// using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

  public static GameManager Instance;

  public GameObject BillboardPrefab;
  public float XSpacing;
  public float ZSpacing;
  public List<TextAsset> StoryFiles;

  [HideInInspector] [System.NonSerialized]
  public int BillboardIdx;

  [HideInInspector] [System.NonSerialized]
  public List<int> StoryCounts;
  [HideInInspector] [System.NonSerialized]
  public List<GameObject> Billboards;
  [HideInInspector] [System.NonSerialized]
  public int StoryIdx;

  void Awake() {
    Instance = Instance ?? this;
  }

  // Use this for initialization
  void Start () {
    BillboardIdx = 0;
    StoryIdx = 0;
    LoadStoryFiles();
  }

  // Update is called once per frame
  void Update () {
    if (Input.GetKeyDown(KeyCode.DownArrow)) {
      BillboardIdx++;
    } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
      BillboardIdx--;
    }

    BillboardIdx = Mathf.Clamp(BillboardIdx, 0, StoryCounts[StoryIdx] - 1);
  }

  void LoadStoryFiles() {
    StoryCounts = new List<int>();
    Billboards = new List<GameObject>();
    var pos = Vector3.zero;

    foreach (var file in StoryFiles) {
      var lines = file.text.Split('\n').ToList();
      lines = lines.Select(line => line.Trim()).Where(line => line.Length > 0).ToList();
      StoryCounts.Add(lines.Count());

      foreach (var line in lines) {
        var billboard = Instantiate(BillboardPrefab) as GameObject;
        var textComponent = billboard.GetComponentInChildren<Text>();
        textComponent.text = line;
        billboard.transform.position = pos;
        Billboards.Add(billboard);
        pos += new Vector3(0, 0, -ZSpacing);
      }
      pos += new Vector3(XSpacing, 0, 0);
    }
  }
}

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
