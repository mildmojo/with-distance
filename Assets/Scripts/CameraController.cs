using UnityEngine;

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
    float distance;

    if (rangefinder.IsActive) {
      distance = rangefinder.distance_mm * -gameManager.ZSpacing / 50f;
    } else {
      var card = gameManager.Cards[gameManager.CardIdx];
      var z = card.transform.position.z;
      distance = z - gameManager.ZSpacing + gameManager.ZSpacing / 10f;
    }

    cameraPos = Vector3.forward * distance;
    transform.position = Vector3.Lerp(transform.position, cameraPos, Time.deltaTime * CameraSnappiness);
  }

  void OnTriggerEnter(Collider c) {
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
    // intervalR = totalRangeR / cardCount
    // rangeToWorld = intervalR / (ZSpacing - card.collider.bounds.size.z)
    // worldDistR = (dist - minR - intervalR * cardNum) * rangeToWorld
    // camerapos = cardpos - card.collider.bounds.size.z - worldDistR
  }
}
