using UnityEngine;
using System.Linq;

public class CameraController : MonoBehaviour {

  public float CameraSnappiness;

  private Vector3 cameraPos;
  private Rangefinder rangefinder;
  private GameManager gameManager;

  void Start () {
    cameraPos = Camera.main.transform.position;
    rangefinder = Rangefinder.Instance;
    gameManager = GameManager.Instance;
  }

  void Update () {
    MoveCamera();
  }

  void MoveCamera() {
    var card = gameManager.Cards[gameManager.CardIdx];

    var colliderSize = card.collider ? card.collider.bounds.size.z : 0;
    var cardGap = gameManager.ZSpacing;

    // Size of gap between cards in sensor units
    var sensorChunk = gameManager.SensorRange / gameManager.Cards.Count();
    var distSensed = Mathf.Min(rangefinder.distance_cm, gameManager.SensorMaxDistance);
    // Query the rangefinder. If in attract mode, invert the reading so the closer
    // you get, the farther you recede from the attract mode message.
    if (gameManager.AttractMode) {
      // Just invert the reading so moving closer becomes moving away.
      distSensed = gameManager.SensorMaxDistance - distSensed;
    } else {
      // Rangefinder reading relative to current card
      // (raw reading minus all gaps for previous cards minus minimum range)
      distSensed -= sensorChunk * gameManager.CardIdx + gameManager.SensorMinDistance;
    }
    // Start at outward edge of card's collider and count outward by sensor
    // reading converted to world units.
    var worldZ = card.transform.position.z - colliderSize;
    worldZ -= distSensed * ((cardGap - colliderSize) / sensorChunk);

    // Move that camera!
    cameraPos = (Vector3.forward * worldZ) + (Vector3.right * gameManager.XSpacing * gameManager.StoryIdx);
    transform.position = Vector3.Lerp(transform.position, cameraPos, Time.deltaTime * CameraSnappiness);

// Debug.Log("cardIdx: " + gameManager.CardIdx + ", storyIdx: " + gameManager.StoryIdx);
// Debug.Log("rangefinder: " + rangefinder.distance_cm + ", range subtraction: " + (sensorChunk * gameManager.CardIdx + gameManager.SensorMinDistance) +
//           ", distSensed: " + distSensed + ", sensorChunk: " + sensorChunk +
//           ", ratio: " + (sensorChunk / (cardGap - colliderSize)));
  }

  void OnTriggerEnter(Collider c) {
    // Don't trigger old story card colliders on the way to a new story.
    if (IsOutsideStoryBounds()) return;
    if (c.tag == "AttractCard") return;

    var card = c.gameObject;
    var cardIdx = card.GetComponent<TextController>().index;

    if (IsInFrontOfCamera(c)) {
      // Card is in front of camera, select next-lower index.
      Debug.Log("goto card " + (cardIdx - 1));
      gameManager.SelectCard(cardIdx - 1);
    } else {
      // Card is behind camera, select it.
      Debug.Log("goto card " + cardIdx);
      gameManager.SelectCard(cardIdx);
    }
  }

  void OnTriggerExit(Collider c) {
    // Don't trigger old story card colliders on the way to a new story.
    if (IsOutsideStoryBounds()) return;
    if (c.tag == "AttractCard") return;

    var card = c.gameObject;
    var cardIdx = card.GetComponent<TextController>().index;

    if (IsInFrontOfCamera(c)) {
      // Card is in front of camera, select it.
      Debug.Log("goto card " + cardIdx);
      gameManager.SelectCard(cardIdx);
    } else {
      // Card is behind camera, select next-lower index.
      Debug.Log("goto card " + (cardIdx - 1));
      gameManager.SelectCard(cardIdx - 1);
    }
  }

  bool IsOutsideStoryBounds() {
    var acceptableDiff = gameManager.XSpacing / 2f;
    var cameraX = transform.position.x;
    var storyX = gameManager.XSpacing * gameManager.StoryIdx;
    return Mathf.Abs(cameraX - storyX) > acceptableDiff;
  }

  bool IsInFrontOfCamera(Collider c) {
    return c.bounds.center.z > collider.bounds.center.z;
  }
}
