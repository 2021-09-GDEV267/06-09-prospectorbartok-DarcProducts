using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guillotine : MonoBehaviour
{
    public static Guillotine S;
    public static GuillotinePlayer CURRENT_PLAYER;
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public Vector3 layoutCenter = Vector3.zero;
    public float handFanDegrees = 10f;
    public int numStartingCards = 7;
    public float drawTimeStagger = 0.1f;
    public TurnPhase phase = TurnPhase.idle;

    [HideInInspector]
    public Deck deck;

    [HideInInspector]
    public List<CardGuillotine> drawPile;

    [HideInInspector]
    public List<CardGuillotine> discardPile;

    public List<GuillotinePlayer> players;
    BartokLayout layout;
    public Transform layoutAnchor = null;
    CardGuillotine targetCard;

    void Awake() => S = this;

    void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<BartokLayout>();
        layout.ReadLayout(layoutXML.text);
        drawPile = UpgradeCardList(deck.cards);

        LayoutGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            if (drawPile.Count > 0)
                players[0].AddCard(Draw());
        if (Input.GetKeyDown(KeyCode.Alpha2))
            if (drawPile.Count > 0)
                players[1].AddCard(Draw());
        if (Input.GetKeyDown(KeyCode.Alpha3))
            if (drawPile.Count > 0)
                players[2].AddCard(Draw());
        if (Input.GetKeyDown(KeyCode.Alpha4))
            if (drawPile.Count > 0)
                players[3].AddCard(Draw());
    }

    List<CardGuillotine> UpgradeCardList(List<Card> lCD)
    {
        List<CardGuillotine> lCB = new List<CardGuillotine>();
        foreach (Card tCD in lCD)
            lCB.Add(tCD as CardGuillotine);
        return lCB;
    }

    public void ArrangeDrawPile()
    {
        CardGuillotine tCB;

        for (int i = 0; i < drawPile.Count; i++)
        {
            tCB = drawPile[i];
            tCB.transform.SetParent(layoutAnchor);
            tCB.transform.localPosition = layout.drawPile.pos;
            tCB.faceUp = false;
            tCB.SetSortingLayerName(layout.drawPile.layerName);
            tCB.SetSortOrder(-i * 4);
            tCB.state = CGState.toTarget; //------
        }
    }

    void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGo = new GameObject("_LayoutAnchor");
            layoutAnchor = tGo.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        ArrangeDrawPile();
        GuillotinePlayer p1;

        players = new List<GuillotinePlayer>();
        foreach (darcproducts.SlotDef tSD in layout.slotDefs)
        {
            p1 = new GuillotinePlayer();
            p1.handSlotDef = tSD;
            players.Add(p1);
            p1.playerNumber = tSD.player;
        }

        players[0].type = GuillotinePlayerType.human;

        CardGuillotine tCB;
        for (int i = 0; i < numStartingCards; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                tCB = Draw();
                tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);
                players[(j + 1) % 4].AddCard(tCB);
            }
        }
        Invoke("DrawFirstTarget", drawTimeStagger * (numStartingCards * 4 + 4));
    }

    public void DrawFirstTarget()
    {
        CardGuillotine tCB = MoveToTarget(Draw());
        tCB.reportFinishTo = this.gameObject;
    }

    public CardGuillotine MoveToTarget(CardGuillotine tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.discardPile.pos + Vector3.back);
        tCB.state = CGState.toTarget;
        tCB.faceUp = true;
        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target.layerName;
        if (targetCard != null)
            MoveToDiscard(targetCard);
        targetCard = tCB;
        return tCB;
    }

    public CardGuillotine MoveToDiscard(CardGuillotine tCB)
    {
        tCB.state = CGState.discard;
        discardPile.Add(tCB);
        tCB.SetSortingLayerName(layout.discardPile.layerName);
        tCB.SetSortOrder(discardPile.Count * 4);
        tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;
        return tCB;
    }

    public CardGuillotine Draw()
    {
        CardGuillotine cB = drawPile[0];
        drawPile.RemoveAt(0);
        return cB;
    }

    public void CBCallback(CardGuillotine cb)
    {
        Utils.tr("Bartok:CBCallback()", cb.name);
        StartGame();
    }

    public void StartGame() => PassTurn();

    public void PassTurn(int num = -1)
    {
        if (num == -1)
        {
            int ndx = players.IndexOf(CURRENT_PLAYER);
            num = (ndx + 1) % 4;
        }
        int lastPlayerNum = -1;
        if (CURRENT_PLAYER != null)
            lastPlayerNum = CURRENT_PLAYER.playerNumber;
        CURRENT_PLAYER = players[num];
        phase = TurnPhase.pre;

        Utils.tr("Bartok:PassTurn()", "Old: " + lastPlayerNum, "New: " + CURRENT_PLAYER.playerNumber);
    }

    public bool ValidPlay(CardGuillotine cb)
    {
        if (cb.rank == targetCard.rank) return true;
        if (cb.suit == targetCard.suit) return true;
        return false;
    }
}
