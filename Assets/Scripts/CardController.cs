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
Debug.Log(indices[0]);
Debug.Log(indices[1]);
Debug.Log(gameManager.Cards.Count());
    var oldCard = gameManager.Cards[indices.First()];
    var newCard = gameManager.Cards[indices.Last()];
    var oldCardText = oldCard.GetComponent<TextController>();
    var newCardText = newCard.GetComponent<TextController>();
    Debug.Log("Blending!");

    oldCardText.TweenOut();
    newCardText.TweenIn();
  }
}
