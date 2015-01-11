using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
// using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

  public static GameManager Instance;

  private const float IDLE_TIMEOUT = 20f;

  public GameObject AttractPrefab;
  public GameObject CardPrefab;
  public GameObject FinalCardPrefab;
  public float XSpacing;
  public float ZSpacing;
  public List<TextAsset> StoryFiles;

  [HideInInspector] [System.NonSerialized]
  public int CardIdx;

  [HideInInspector] [System.NonSerialized]
  public List<int> StoryCounts;
  [HideInInspector] [System.NonSerialized]
  public List<List<GameObject>> Stories;
  [HideInInspector] [System.NonSerialized]
  public List<GameObject> Cards;
  [HideInInspector] [System.NonSerialized]
  public int StoryIdx;

  [HideInInspector] [System.NonSerialized]
  public float SensorMinDistance;
  [HideInInspector] [System.NonSerialized]
  public float SensorMaxDistance;

  private ShuffleDeck StoryDeck;
  private Text statusText;
  private Rangefinder rangefinder;

  public bool AttractMode {
    get {
      return StoryIdx == 0;
    }
  }

  void Awake() {
    Instance = Instance ?? this;
    statusText = GetComponentInChildren<Text>();
    SensorMinDistance = PlayerPrefs.GetFloat("SensorMinDistance", 70);
    SensorMaxDistance = PlayerPrefs.GetFloat("SensorMaxDistance", 250);

    // Let's play hide-the-arrow. I'll go first.
    Screen.showCursor = false;
  }

  // Use this for initialization
  void Start () {
    rangefinder = Rangefinder.Instance;
    CardIdx = 0;
    StoryIdx = 0;
    statusText.enabled = false;
    Stories = new List<List<GameObject>>();
    SetupAttractMode();
    LoadStoryFiles();
    Debug.Log("cardchange!");
    SendMessage("CardChange", new int[]{0,0});
  }

  // Update is called once per frame
  void Update () {
    CheckAttractModeTimeout();
  }

  void CheckAttractModeTimeout() {
    if (AttractMode) return;
    if (rangefinder.idleTime > IDLE_TIMEOUT) {
      Cards[CardIdx].GetComponent<TextController>().TweenOut();
      StoryIdx = 0;
      Debug.Log("idle; going into attract mode");
      SendMessage("CardChange", new int[]{0,0});
    }
  }

  public void NextStory() {
    var newIdx = (int) StoryDeck.Draw();
    SelectStory(newIdx);
  }

  public void SelectStory(int newStoryIdx) {
    var oldStoryIdx = StoryIdx;
    StoryIdx = newStoryIdx;
    Cards = Stories[StoryIdx];

    foreach (var card in Stories[oldStoryIdx]) {
      var text = card.GetComponent<TextController>();
      text.TweenOut();
    }

    Debug.Log("New story: " + StoryIdx);
    SendMessage("CardChange", new int[]{0,0});
  }

  public void NextCard() {
    var oldCardIdx = CardIdx;
    CardIdx++;
    CardIdx = Mathf.Clamp(CardIdx, 0, StoryCounts[StoryIdx] - 1);
Debug.Log("next");

    if (CardIdx != oldCardIdx) {
      Debug.Log("cardchange next!");
      SendMessage("CardChange", new int[]{oldCardIdx, CardIdx});
    }
  }

  public void PrevCard() {
    var oldCardIdx = CardIdx;
    CardIdx--;
    CardIdx = Mathf.Clamp(CardIdx, 0, StoryCounts[StoryIdx] - 1);
Debug.Log("prev");
    if (CardIdx != oldCardIdx) {
      Debug.Log("cardchange prev!");
      SendMessage("CardChange", new int[]{oldCardIdx, CardIdx});
    }
  }

  void SetupAttractMode() {
    var cards = new List<GameObject>();
    var card = Instantiate(AttractPrefab) as GameObject;
    cards.Add(card);
    Stories.Add(cards);
  }

  void LoadStoryFiles() {
    StoryCounts = new List<int>();
    StoryDeck = new ShuffleDeck();
    var pos = new Vector3(XSpacing * Stories.Count(), 0, 0);

    foreach (var file in StoryFiles) {
      var cards = new List<GameObject>();
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
        cards.Add(card);
        Debug.Log("card added");
        pos += new Vector3(0, 0, -ZSpacing);
      }

      // Don't add card to story stack; it'll affect sensor distances.
      var finalCard = Instantiate(FinalCardPrefab) as GameObject;
      finalCard.transform.position = pos;
      // cards.Add(finalCard);

      Stories.Add(cards);
      StoryDeck.Add(Stories.Count() - 1);
      pos.z = 0;
      pos += new Vector3(XSpacing, 0, 0);
    }
    Cards = Stories.First();
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
