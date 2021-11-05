using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    TMP_Text txt;
    // Start is called before the first frame update
    void Awake()
    {
        txt = GetComponent<TMP_Text>();
        txt.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (Bartok.S.phase != TurnPhase.gameOver)
        {
            txt.text = "";
            return;
        }
        if (Bartok.CURRENT_PLAYER == null) return;
        if (Bartok.CURRENT_PLAYER.type == PlayerType.human)
            txt.text = "You Won!";
        else
            txt.text = "Game Over";
    }
}
