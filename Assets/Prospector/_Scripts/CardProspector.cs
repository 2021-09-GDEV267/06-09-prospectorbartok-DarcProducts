using System.Collections.Generic;
using UnityEngine;

public class CardProspector : Card
{
    [HideInInspector]
    public eCardState state = eCardState.drawpile;
    [HideInInspector]
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    [HideInInspector]
    public int layoutID;
    [HideInInspector]
    public SlotDef slotDef;
    [HideInInspector]
    public bool isGold = false;

    public override void OnMouseUpAsButton()
    {
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}