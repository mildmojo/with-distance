using UnityEngine;
using System.Collections;

public class TimerController : MonoBehaviour {

  public GameObject TimerSprite;
  public float TimerDuration;

  private float startedAt;
  private Vector3 zeroPos;

  void Awake() {
    zeroPos = transform.position;
  }

  void Start () {
    startedAt = Time.time;
  }

  void Update () {
    var elapsed = Time.time - startedAt;

// TODO
//    TimerSprite.transform.position = getPosition(zeroPos, elapsed/TimerDuration);

    // tween ymin to ymax and xmin + 0.25 * xrange to xmin + 0.75 * xrange
    // tween ymax to zero and xmin + 0.75 * xrange to xmax
    // tween zero to ymin and xmax to xmin + 0.75 * xrange
    // tween ymin to ymax and xmin + 0.75 * xrange to xmin + 0.25 * xrange
    // tween ymax to zero and xmin + 0.25 * xrange to xmin
    // tween zero to ymin and xmin to xmin + 0.25 * xrange
  }

  // Bernoulli Lemniscate (http://gamedev.stackexchange.com/a/43704)
  Vector3 getPosition(Vector3 zeroPos, float percent) {
    var scale = 2 / (3 - Mathf.Cos(2*percent));
    var x = scale * Mathf.Cos(percent);
    var y = scale * Mathf.Sin(2*percent) / 2;
    return zeroPos + new Vector3(x, y, 0);
  }
}
