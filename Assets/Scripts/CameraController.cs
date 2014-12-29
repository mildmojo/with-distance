using UnityEngine;
using System.Linq;

public class CameraController : MonoBehaviour {

  public float CameraSnappiness;

  private Vector3 cameraPos;
  private Rangefinder rangefinder;
  private GameManager gameManager;
  private float finalCardTime;

  private const int RANGE_MIN = 50;
  private const int RANGE_MAX = 300;

  void Start () {
    cameraPos = Camera.main.transform.position;
    rangefinder = Rangefinder.Instance;
    gameManager = GameManager.Instance;
  }

  void Update () {
    // float distance;

    // distance = rangefinder.distance_mm * -gameManager.ZSpacing / 50f;

    var card = gameManager.Cards[gameManager.CardIdx];

    /* One card has:
     * - Card 0-0
     * - Collider 0-350
     * - Deadzone 350-400
     *
     */
     var colliderSize = card.collider.bounds.size.z;
     var cardGap = gameManager.ZSpacing;

     var sensorChunk = (RANGE_MAX - RANGE_MIN) / gameManager.Cards.Count();
     var distSensed = rangefinder.distance_mm;
     distSensed -= sensorChunk * gameManager.CardIdx + RANGE_MIN;
     var worldZ = card.transform.position.z - colliderSize;
     worldZ -= distSensed * ((cardGap - colliderSize) / sensorChunk);

// Debug.Log("distSensed (" + distSensed + "): " + rangefinder.distance_mm + " - (" +
//           sensorChunk + " * " + gameManager.CardIdx + " + " + RANGE_MIN + ")");
// Debug.Log("worldZ (" + worldZ + "): " + card.transform.position.z + " - " + colliderSize +
//           " - " + distSensed + " * ((" + cardGap + " - " + colliderSize + ") / " + sensorChunk + ")");
// // Debug.Log("cardPos: " + card.transform.position.z + ", cardIdx: " + gameManager.CardIdx +
// //           ", colliderSize: " + colliderSize + ", worldZ: " + worldZ);
Debug.Log("rangefinder: " + rangefinder.distance_mm + ", range subtraction: " + (sensorChunk * gameManager.CardIdx + RANGE_MIN) +
          ", distSensed: " + distSensed + ", sensorChunk: " + sensorChunk +
          ", ratio: " + (sensorChunk / (cardGap - colliderSize)));

//     // Player moves through deadzone between colliders.
//     // Depth of one deadzone is total sensing range divided by card count.
//     var deadzoneSensorDepth = (RANGE_MAX - RANGE_MIN) / gameManager.Cards.Count();
//     // Depth of deadzone in world coords is gap between cards minus collider depth.
//     var deadzoneWorldDepth = gameManager.ZSpacing - card.collider.bounds.size.z;
//     // Conversion factor for converting sensed range to world Z.
//     var deadzoneDepthToWorld = deadzoneSensorDepth / deadzoneWorldDepth;
//     var distSensed = rangefinder.distance_mm;
//     // Current world Z is sensor reading minus minimum range minus all deadzones
//     //   that came before, converted to world coords, plus current card Z plus
//     //   card collider depth.
//     var worldZ = (distSensed - RANGE_MIN - deadzoneSensorDepth * gameManager.CardIdx);
//     worldZ *= deadzoneDepthToWorld;
// Debug.Log("cardPos: " + card.transform.position.z + ", cardIdx: " + gameManager.CardIdx +
//           ", colliderDepth: " + card.collider.bounds.size.z + ", worldZ: " + worldZ);
// Debug.Log("distSensed: " + distSensed + ", RANGE_MIN: " + RANGE_MIN +
//           ", deadzoneSensorDepth * gameManager.CardIdx: " + deadzoneSensorDepth * gameManager.CardIdx);
//     worldZ = card.transform.position.z - card.collider.bounds.size.z - worldZ;
    cameraPos = Vector3.forward * worldZ;

    // var cardIdx = c.gameObject.GetComponent<
    // Space goes from 500 to 10000us
    // 5 cards, placed at 0 - 400 * N intervals
    // 5 camera positions, at 0 - 400 * (N + 1) + 50
    // Camera can waver +/- 50
    // Cards at 2000us intervals
    // 500: -350
    // 2500: -750
    // 4500: -1150
    // 6500: -1550
    // 8500: -1950
    // rangefinder-controlled traversal space is N * 100, gaps between colliders
    // On collider contact, snap to other side of collider

    // rangefinder totalRangeR = maxR - minR
    // deadzoneDepth = totalRangeR / cardCount
    // rangeToWorld = deadzoneDepth / (ZSpacing - card.collider.bounds.size.z)
    // worldDistR = (dist - minR - deadzoneDepth * cardNum) * rangeToWorld
    // camerapos = cardpos - card.collider.bounds.size.z - worldDistR


    // cameraPos = Vector3.forward * distance;
    transform.position = Vector3.Lerp(transform.position, cameraPos, Time.deltaTime * CameraSnappiness);
  }

  void OnTriggerEnter(Collider c) {
    if (c.tag == "FinalCard") return;
    // if (!rangefinder.IsActive) return;

    if (c.bounds.center.z < collider.bounds.center.z) {
      // Front edge of collider, move to the back (-z).
      gameManager.NextCard();
    } else {
      // Back edge of the collider, move to the front (+z).
      gameManager.PrevCard();
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
