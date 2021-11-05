using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum TurnPhase
{
    idle,
    pre,
    waiting,
    post,
    gameOver
}

public class Bartok : MonoBehaviour
{
    public static Bartok S;
    public static Player CURRENT_PLAYER;
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
    public List<CardBartok> drawPile;

    [HideInInspector]
    public List<CardBartok> discardPile;

    public List<Player> players;
    BartokLayout layout;
    public Transform layoutAnchor = null;
    CardBartok targetCard;

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

    /*
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
    }*/

    List<CardBartok> UpgradeCardList(List<Card> lCD)
    {
        List<CardBartok> lCB = new List<CardBartok>();
        foreach (Card tCD in lCD)
            lCB.Add(tCD as CardBartok);
        return lCB;
    }

    public void ArrangeDrawPile()
    {
        CardBartok tCB;

        for (int i = 0; i < drawPile.Count; i++)
        {
            tCB = drawPile[i];
            tCB.transform.SetParent(layoutAnchor);
            tCB.transform.localPosition = layout.drawPile.pos;
            tCB.faceUp = false;
            tCB.SetSortingLayerName(layout.drawPile.layerName);
            tCB.SetSortOrder(-i * 4);
            tCB.state = CBState.drawpile;
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
        Player p1;

        players = new List<Player>();
        foreach (darcproducts.SlotDef tSD in layout.slotDefs)
        {
            p1 = new Player();
            p1.handSlotDef = tSD;
            players.Add(p1);
            p1.playerNumber = tSD.player;
        }

        players[0].type = PlayerType.human;

        CardBartok tCB;
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
        CardBartok tCB = MoveToTarget(Draw());
        tCB.reportFinishTo = this.gameObject;
    }

    public CardBartok MoveToTarget(CardBartok tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.discardPile.pos + Vector3.back);
        tCB.state = CBState.toTarget;
        tCB.faceUp = true;
        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target.layerName;
        if (targetCard != null)
            MoveToDiscard(targetCard);
        targetCard = tCB;
        return tCB;
    }

    public CardBartok MoveToDiscard(CardBartok tCB)
    {
        tCB.state = CBState.discard;
        discardPile.Add(tCB);
        tCB.SetSortingLayerName(layout.discardPile.layerName);
        tCB.SetSortOrder(discardPile.Count * 4);
        tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;
        return tCB;
    }

    public CardBartok Draw()
    {
        CardBartok cB = drawPile[0];
        if (drawPile.Count == 0)
        {
            int ndx;
            while (discardPile.Count > 0)
            {
                ndx = Random.Range(0, discardPile.Count);
                drawPile.Add(discardPile[ndx]);
                discardPile.RemoveAt(ndx);
            }
            ArrangeDrawPile();
            float t = Time.time;
            foreach (CardBartok tCB in drawPile)
            {
                tCB.transform.localPosition = layout.discardPile.pos;
                tCB.callbackPlayer = null;
                tCB.MoveTo(layout.drawPile.pos);
                tCB.timeStart = t;
                t += 0.2f;
                tCB.state = CBState.toDrawpile;
                tCB.eventualSortLayer = "0";
            }
        }
        drawPile.RemoveAt(0);
        return cB;
    }

    public void CBCallback(CardBartok cb)
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
        {
            lastPlayerNum = CURRENT_PLAYER.playerNumber;
            if (CheckGameOver()) return;
        }
        CURRENT_PLAYER = players[num];
        phase = TurnPhase.pre;

        CURRENT_PLAYER.TakeTurn();

        Utils.tr("Bartok:PassTurn()", "Old: " + lastPlayerNum, "New: " + CURRENT_PLAYER.playerNumber);
    }

    public bool ValidPlay(CardBartok cb)
    {
        if (cb.rank == targetCard.rank) return true;
        if (cb.suit == targetCard.suit) return true;
        return false;
    }

    public void CardClicked(CardBartok tCB)
    {
        if (CURRENT_PLAYER.type != PlayerType.human) return;
        if (phase == TurnPhase.waiting) return;

        switch (tCB.state)
        {
            case CBState.drawpile:
                CardBartok cb = CURRENT_PLAYER.AddCard(Draw());
                cb.callbackPlayer = CURRENT_PLAYER;
                Utils.tr("Bartok:CardClicked()", "Draw", cb.name);
                phase = TurnPhase.waiting;
                break;
            case CBState.hand:
                if (ValidPlay(tCB))
                {
                    CURRENT_PLAYER.RemoveCard(tCB);
                    MoveToTarget(tCB);
                    tCB.callbackPlayer = CURRENT_PLAYER;
                    Utils.tr("Bartok:CardClicked()", "Play", tCB.name, $"{targetCard.name} is target");
                    phase = TurnPhase.waiting;
                }
                else
                    Utils.tr("Bartok:CardClicked()", "Attempted to play", tCB.name, $"{targetCard.name} is target");
                break;
        }
    }

    bool CheckGameOver()
    {
        if (drawPile.Count == 0)
        {
            List<Card> cards = new List<Card>();
            foreach (CardBartok cb in discardPile)
                cards.Add(cb);
            discardPile.Clear();
            Deck.Shuffle(ref cards);
            drawPile = UpgradeCardList(cards);
            ArrangeDrawPile();
        }
        if (CURRENT_PLAYER.hand.Count == 0)
        {
            phase = TurnPhase.gameOver;
            Invoke("RestartGame", 3);
            return true;
        }
        return false;
    }

    void RestartGame()
    {
        CURRENT_PLAYER = null;
        SceneManager.LoadScene("_Bartok_Scene_0");
    }
}