using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour {

  public static GameManager Instance;

  public float IdleTimeout;

  public GameObject AttractPrefab;
  public GameObject CardPrefab;
  public GameObject FinalCardPrefab;
  public Font DefaultCardFont;
  public int DefaultCardFontSize;
  public float XSpacing;
  public float ZSpacing;
  public List<GameObject> TimerDots;
  public List<TextAsset> StoryFiles;

  [HideInInspector] [System.NonSerialized]
  public int CardIdx;

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

  public float SensorRange {
    get {
      return SensorMaxDistance - SensorMinDistance;
    }
  }

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
    CheckDebugMode();
  }

  void CheckAttractModeTimeout() {
    var idleTime = rangefinder.idleTime;
    // Always reset rangefinder idleTime so we don't continuously trigger timeout.
    if (idleTime > IdleTimeout) {
      rangefinder.Reset();
    };

    if (AttractMode) return;

    // Show timeout dots for inactivity.
    var dotTime = idleTime - IdleTimeout * 0.4;
    var dotTotalTime = IdleTimeout * 0.5;
    for (var i = 0; i < TimerDots.Count(); i++) {
      // If time has passed this dot's interval, activate it.
      if (dotTime > (dotTotalTime / TimerDots.Count()) * i) {
        TimerDots[i].active = true;
      }
    }

    if (idleTime > IdleTimeout || Input.GetKeyDown(KeyCode.R)) {
      foreach (var dot in TimerDots) {
        dot.active = false;
      }
      SelectStory(0);
      var camPos = Camera.main.transform.position;
      Camera.main.transform.position = new Vector3(camPos.x, camPos.y, -ZSpacing / 2f);
      Debug.Log("idle; going into attract mode");
    }
  }

  void CheckDebugMode() {
    if (Input.GetKeyDown(KeyCode.D)) {
      statusText.enabled = !statusText.enabled;
    }

    if (!statusText.enabled) return;

    statusText.text = "Range (cm): " + SensorMinDistance + " min, "
      + SensorMaxDistance + " max";
    statusText.text += "\n(dist_cm " + Mathf.Round(rangefinder.distance_cm) + ")";

    // Adjust settings, save to disk.
    if (Input.GetKeyDown(KeyCode.LeftBracket)) {
      SensorMinDistance -= 5;
      PlayerPrefs.SetFloat("SensorMinDistance", SensorMinDistance);
      PlayerPrefs.Save();
    } else if (Input.GetKeyDown(KeyCode.RightBracket)) {
      SensorMinDistance += 5;
      PlayerPrefs.SetFloat("SensorMinDistance", SensorMinDistance);
      PlayerPrefs.Save();
    } else if (Input.GetKeyDown(KeyCode.Comma)) {
      SensorMaxDistance -= 5;
      PlayerPrefs.SetFloat("SensorMaxDistance", SensorMaxDistance);
      PlayerPrefs.Save();
    } else if (Input.GetKeyDown(KeyCode.Period)) {
      SensorMaxDistance += 5;
      PlayerPrefs.SetFloat("SensorMaxDistance", SensorMaxDistance);
      PlayerPrefs.Save();
    }

    if (rangefinder.last_raw_cm > 0) {
      statusText.text += "\n(hit " + Mathf.Round(rangefinder.last_raw_cm) + ")";
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

    rangefinder.Reset();
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
    StoryDeck = new ShuffleDeck();
    var pos = new Vector3(XSpacing * Stories.Count(), 0, 0);
    var fontPattern = new Regex(@"^<font (.+?) ?(\d+)?>");

    foreach (var file in StoryFiles) {
      var currentFont = DefaultCardFont;
      var currentFontSize = DefaultCardFontSize;
      var cards = new List<GameObject>();
      var lines = file.text.Split('\n').ToList();
      lines = lines.Select(line => line.Trim()).Where(line => line.Length > 0).ToList();

      for (var i = 0; i < lines.Count(); i++) {
        var line = lines[i];

        // Look for font changes.
        var fontMatch = fontPattern.Match(line);
        if (fontMatch.Success) {
          var fontName = fontMatch.Groups[1].Value;
          var fontSize = fontMatch.Groups[2].Value;
          if (fontName == "default") {
            currentFont = DefaultCardFont;
          } else {
            currentFont = FontManager.Instance.GetFont(fontName);
          }
          try {
            currentFontSize = Convert.ToInt32(fontSize);
          } catch {}
          continue;
        }

        var card = Instantiate(CardPrefab) as GameObject;
        var textController = card.GetComponent<TextController>();
        textController.SetFont(currentFont);
        textController.SetFontSize(currentFontSize);
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
