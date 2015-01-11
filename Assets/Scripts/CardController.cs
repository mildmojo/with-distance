using UnityEngine;
using System.Linq;

public class CardController : MonoBehaviour {

  GameManager gameManager;

  void Start () {
    gameManager = GameManager.Instance;
  }

  void Update () {

  }

  void CardChange(int[] indices) {
    gameManager = gameManager ?? GameManager.Instance;
    var oldCard = gameManager.Cards[indices.First()];
    var newCard = gameManager.Cards[indices.Last()];
    var oldCardText = oldCard.GetComponent<TextController>();
    var newCardText = newCard.GetComponent<TextController>();
    Debug.Log("Blending!");

    oldCardText.TweenOut();
    newCardText.TweenIn();
  }
}
