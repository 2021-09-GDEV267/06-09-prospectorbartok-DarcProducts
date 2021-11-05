using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum GuillotinePlayerType { human, ai }

[System.Serializable]
public class GuillotinePlayer
{
    public GuillotinePlayerType type = GuillotinePlayerType.ai;
    public int playerNumber;
    public darcproducts.SlotDef handSlotDef;
    public List<CardGuillotine> hand;

    public CardGuillotine AddCard(CardGuillotine eCB)
    {
        if (hand == null) hand = new List<CardGuillotine>();
        hand.Add(eCB);
        if (type == GuillotinePlayerType.human)
        {
            CardGuillotine[] cards = hand.ToArray();
            cards = cards.OrderBy(cd => cd.rank).ToArray();
            hand = new List<CardGuillotine>(cards);
        }
        eCB.SetSortingLayerName("10");
        eCB.eventualSortLayer = handSlotDef.layerName;
        FanHand();
        return eCB;
    }

    public CardGuillotine RemoveCard(CardGuillotine eCB)
    {
        if (hand == null || !hand.Contains(eCB)) return null;
        hand.Remove(eCB);
        FanHand();
        return eCB;
    }

    public void FanHand()
    {
        float startRot = 0;
        startRot = handSlotDef.rot;
        if (hand.Count > 1)
            startRot += Guillotine.S.handFanDegrees * (hand.Count - 1) / 2;

        Vector3 pos;
        float rot;
        Quaternion rotQ;
        for (int i = 0; i < hand.Count; i++)
        {
            rot = startRot - Guillotine.S.handFanDegrees * i;
            rotQ = Quaternion.Euler(0, 0, rot);
            pos = Vector3.up * CardGuillotine.CARD_HEIGHT / 2f;
            pos = rotQ * pos;
            pos += handSlotDef.pos;
            pos.z = -0.5f * i;
            //hand[i].transform.localPosition = pos;
            //hand[i].transform.rotation = rotQ;
            hand[i].MoveTo(pos, rotQ);
            //hand[i].state = CGState.hand;
            hand[i].state = CGState.toHand;
            hand[i].faceUp = (type == GuillotinePlayerType.human);
            hand[i].eventualSortOrder = i * 4;
            //hand[i].SetSortOrder(i * 4);
        }
    }
}
