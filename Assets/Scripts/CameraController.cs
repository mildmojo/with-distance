using UnityEngine;
using System.Linq;

public class CameraController : MonoBehaviour {

  public float CameraSnappiness;

  private Vector3 cameraPos;
  private Rangefinder rangefinder;
  private GameManager gameManager;
  private float finalCardTime;

  private float RANGE_MIN;
  private float RANGE_MAX;

  void Start () {
    cameraPos = Camera.main.transform.position;
    rangefinder = Rangefinder.Instance;
    gameManager = GameManager.Instance;
    RANGE_MIN = gameManager.SensorMinDistance;
    RANGE_MAX = gameManager.SensorMaxDistance;
  }

  void Update () {
    MoveCamera();
  }

  void MoveCamera() {
    var card = gameManager.Cards[gameManager.CardIdx];

    var colliderSize = card.collider ? card.collider.bounds.size.z : 0;
    var cardGap = gameManager.ZSpacing;

    // Size of gap between cards in sensor units
    var sensorChunk = (RANGE_MAX - RANGE_MIN) / gameManager.Cards.Count();
    var distSensed = Mathf.Min(rangefinder.distance_cm, RANGE_MAX);
    // Query the rangefinder. If in attract mode, invert the reading so the closer
    // you get, the farther you recede from the attract mode message.
    if (gameManager.AttractMode) {
      // Just invert the reading so moving closer becomes moving away.
      distSensed = RANGE_MAX - distSensed;
    } else {
      // Rangefinder reading relative to current card
      // (raw reading minus all gaps for previous cards minus minimum range)
      distSensed -= sensorChunk * gameManager.CardIdx + RANGE_MIN;
    }
    // Start at outward edge of card's collider and count outward by sensor
    // reading converted to world units.
    var worldZ = card.transform.position.z - colliderSize;
    worldZ -= distSensed * ((cardGap - colliderSize) / sensorChunk);

    // Move that camera!
    cameraPos = (Vector3.forward * worldZ) + (Vector3.right * gameManager.XSpacing * gameManager.StoryIdx);
    transform.position = Vector3.Lerp(transform.position, cameraPos, Time.deltaTime * CameraSnappiness);

Debug.Log("cardIdx: " + gameManager.CardIdx + ", storyIdx: " + gameManager.StoryIdx);
// Debug.Log("rangefinder: " + rangefinder.distance_cm + ", range subtraction: " + (sensorChunk * gameManager.CardIdx + RANGE_MIN) +
//           ", distSensed: " + distSensed + ", sensorChunk: " + sensorChunk +
//           ", ratio: " + (sensorChunk / (cardGap - colliderSize)));
  }

  void OnTriggerEnter(Collider c) {
    if (c.tag == "AttractCard") return;
    if (c.tag == "FinalCard") return;

    // Don't trigger if we're moving toward a new story.
    var cameraX = transform.position.x;
    var storyX = gameManager.XSpacing * gameManager.StoryIdx;
    var acceptableDiff = gameManager.XSpacing / 2f;
    if (Mathf.Abs(cameraX - storyX) > acceptableDiff) return;

    var card = c.gameObject;
    var cardIdx = card.GetComponent<TextController>().index;
    if (c.bounds.center.z < collider.bounds.center.z) {
      // Front edge of collider, move to the back (-z).
      gameManager.SelectCard(cardIdx);
    } else {
      // Back edge of the collider, move to the front (+z).
      gameManager.SelectCard(cardIdx - 1);
    }
  }

  void OnTriggerStay(Collider c) {
    // Accumulate time spent in final card's trigger zone.
    if (c.tag == "FinalCard") finalCardTime += Time.deltaTime;
  }

  void OnTriggerExit(Collider c) {
    if (c.tag == "FinalCard") finalCardTime = 0f;
  }
}
