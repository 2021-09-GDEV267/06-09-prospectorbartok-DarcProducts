using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoundResultUI : MonoBehaviour
{
    TMP_Text txt;

    void Awake()
    {
        txt = GetComponent<TMP_Text>();
        txt.text = "";
    }

    void Update()
    {
        if (Bartok.S.phase != TurnPhase.gameOver)
        {
            txt.text = "";
            return;
        }

        Player cP = Bartok.CURRENT_PLAYER;
        if (cP == null || cP.type == PlayerType.human)
            txt.text = "";
        else
            txt.text = $"Player {cP.playerNumber} won";
    }
}
