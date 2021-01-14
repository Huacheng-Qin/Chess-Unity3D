using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript
{
    public Text GameOverText;

    public void PrintGameOver(bool isCheckmate, GamePiece.Team winner) {
        GameOverText = GameObject.Find("GameOverText").GetComponent<Text>();
        string text;

        if (isCheckmate) {
            text = "Checkmate! ";
        } else {
            text = "Stalemate! ";
        }

        if (winner == GamePiece.Team.white) {
            text += "White team wins!";
        } else if (winner == GamePiece.Team.black) {
            text += "Black team wins!";
        } else {
            text += "This game is a tie!";
        }

        GameOverText.text = text;
    }
}
