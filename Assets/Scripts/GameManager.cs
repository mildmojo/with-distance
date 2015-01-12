using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

  public static GameManager Instance;

  public float IdleTimeout;

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
    SensorMinDistance = PlayerPrefs.GetFloat("SensorMinDistance", 80);
    SensorMaxDistance = PlayerPrefs.GetFloat("SensorMaxDistance", 270);

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
    if (rangefinder.idleTime > IdleTimeout) {
      SelectStory(0);
      var camPos = Camera.main.transform.position;
      Camera.main.transform.position = new Vector3(camPos.x, camPos.y, -ZSpacing / 2f);
      // LeanTween.moveZ(Camera.main.gameObject, -ZSpacing / 2f, 1f)
      //   .setEase(LeanTweenType.easeInQuad);
      rangefinder.Reset();
      Debug.Log("idle; going into attract mode");
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
    CardIdx = 0;

    Debug.Log("New story: " + StoryIdx);

    foreach (var card in Stories[oldStoryIdx]) {
      var text = card.GetComponent<TextController>();
      text.TweenOut(0.5f);
    }

    LeanTween.moveX(Camera.main.gameObject, (float) XSpacing * StoryIdx, 1f)
      .setEase(LeanTweenType.easeInOutCirc)
      .setOnComplete(() => {
        SendMessage("CardChange", new int[]{0,0});
      });
  }

  public void NextCard() {
Debug.Log("next");
    SelectCard(CardIdx + 1);
  }

  public void PrevCard() {
Debug.Log("prev");
    SelectCard(CardIdx - 1);
  }

  public void SelectCard(int newCardIdx) {
    var oldCardIdx = CardIdx;
    CardIdx = newCardIdx;
    CardIdx = Mathf.Clamp(CardIdx, 0, Stories[StoryIdx].Count() - 1);
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

    StoryDeck.Reshuffle();
    Cards = Stories.First();
  }
}
