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
    // var oldBoardIdx = gameManager.BillboardIdx;
    // var newBoardIdx = oldBoardIdx;

    // if (Input.GetKeyDown(KeyCode.DownArrow)) {
    //   newBoardIdx++;
    // } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
    //   newBoardIdx--;
    // }

    // newBoardIdx = Mathf.Clamp(newBoardIdx, 0, gameManager.StoryCounts[gameManager.StoryIdx] - 1);
    // if (newBoardIdx != oldBoardIdx) {
    //   var oldBoard = gameManager.Billboards[oldBoardIdx];
    //   LeanTween.move(oldBoard, oldBoard.transform.position - new Vector3(0, 900, 0), 0.75f);
    //   var newBoard = gameManager.Billboards[newBoardIdx];
    //   LeanTween.move(newBoard, new Vector3(0, 0, newBoard.transform.position.z), 0.5f);

    //   cameraPos = new Vector3(0, 0, newBoard.transform.position.z - 350);
    //   // var oldRects = gameManager.Billboards[oldBoardIdx].GetComponentsInChildren<Transform>().Select(tf => tf.gameObject).ToList();
    //   // foreach (var rect in oldRects) {
    //   //   LeanTween.alpha(rect, 0f, 1f);
    //   // };

    //   // var newRects = gameManager.Billboards[billboardIdx].GetComponentsInChildren<Transform>().Select(tf => tf.gameObject).ToList();
    //   // foreach (var rect in newRects) {
    //   //   LeanTween.alpha(rect, 1f, 1f);
    //   // };
    // }
    float distance;

    if (rangefinder.IsActive) {
      distance = rangefinder.distance_mm * -gameManager.ZSpacing / 50f;
    } else {
      var billboard = gameManager.Billboards[gameManager.BillboardIdx];
      var z = billboard.transform.position.z;
      distance = z - gameManager.ZSpacing + gameManager.ZSpacing / 10f;
    }
Debug.Log(distance);
    cameraPos = Vector3.forward * distance;
    transform.position = Vector3.Lerp(transform.position, cameraPos, Time.deltaTime * CameraSnappiness);
  }
}
