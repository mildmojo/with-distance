using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
// using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

  public static GameManager Instance;

  public GameObject CardPrefab;
  public float XSpacing;
  public float ZSpacing;
  public List<TextAsset> StoryFiles;

  [HideInInspector] [System.NonSerialized]
  public int CardIdx;

  [HideInInspector] [System.NonSerialized]
  public List<int> StoryCounts;
  [HideInInspector] [System.NonSerialized]
  public List<GameObject> Cards;
  [HideInInspector] [System.NonSerialized]
  public int StoryIdx;

  private Text statusText;

  void Awake() {
    Instance = Instance ?? this;
    statusText = GetComponentInChildren<Text>();
  }

  // Use this for initialization
  void Start () {
    CardIdx = 0;
    StoryIdx = 0;
    statusText.enabled = false;
    LoadStoryFiles();
    SendMessage("CardChange", new int[]{0,0});
  }

  // Update is called once per frame
  void Update () {
    var oldCardIdx = CardIdx;

    if (Input.GetKeyDown(KeyCode.DownArrow)) {
      CardIdx++;
    } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
      CardIdx--;
    }

    CardIdx = Mathf.Clamp(CardIdx, 0, StoryCounts[StoryIdx] - 1);

    if (CardIdx != oldCardIdx) {
      SendMessage("CardChange", new int[]{oldCardIdx, CardIdx});
    }
  }

  void LoadStoryFiles() {
    StoryCounts = new List<int>();
    Cards = new List<GameObject>();
    var pos = Vector3.zero;

    foreach (var file in StoryFiles) {
      var lines = file.text.Split('\n').ToList();
      lines = lines.Select(line => line.Trim()).Where(line => line.Length > 0).ToList();
      StoryCounts.Add(lines.Count());

      for (var i = 0; i < lines.Count(); i++) {
        var line = lines[i];
        var card = Instantiate(CardPrefab) as GameObject;
        var textController = card.GetComponent<TextController>();
        textController.SetText(line);
        textController.SetIndex(i);
        textController.TweenOut(0.1f);
        card.transform.position = pos;
        Cards.Add(card);
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
