using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;

    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;

    [HideInInspector]
    public Vector2 fsPosMid = new Vector2(.5f, .9f);

    [HideInInspector]
    public Vector2 fsPosRun = new Vector2(.5f, .75f);

    [HideInInspector]
    public Vector2 fsPosMid2 = new Vector2(.4f, 1f);

    [HideInInspector]
    public Vector2 fsPosEnd = new Vector2(.5f, .95f);

    public float reloadDelay = 2f;

    public TMP_Text gameOverText, roundResultText, highScoreText, currentGoldText;

    public string prospectorSceneName;

    [Range(0f, 1f)] public float goldSpawnChance = .1f;

    [HideInInspector]
    public Deck deck;

    [HideInInspector]
    public Layout layout;

    [HideInInspector]
    public List<CardProspector> drawPile;

    [HideInInspector]
    public Transform layoutAnchor;

    [HideInInspector]
    public CardProspector target;

    [HideInInspector]
    public List<CardProspector> tableau;

    [HideInInspector]
    public List<CardProspector> discardPile;

    FloatingScore fsRun;

    void Awake()
    {
        if (S == null)
            S = this;
        else
            Debug.LogError($"ERROR: Prospector.Awake() : S is already set!");
        SetUpUITexts();
    }

    void Start()
    {
        ScoreBoard.S.score = ScoreManager.SCORE;
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        Card c;
        for (int cNum = 0; cNum < deck.cards.Count; cNum++)
        {
            c = deck.cards[cNum];
            c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        }

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);

        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }

    void SetUpUITexts()
    {
        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = $"High Score: {Utils.AddCommasToNumber(highScore)}";
        if (highScoreText != null)
            highScoreText.text = hScore;

        ShowResultsUI(false);
    }

    void ShowResultsUI(bool show)
    {
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(show);
        if (roundResultText != null)
            roundResultText.gameObject.SetActive(show);
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;

        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }
        return lCP;
    }

    CardProspector Draw()
    {
        CardProspector cd = drawPile[0];
        drawPile.RemoveAt(0);
        return cd;
    }

    void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cp;
        foreach (SlotDef tSD in layout.slotDefs)
        {
            cp = Draw();
            cp.faceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;
            cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID);
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(tSD.layerName);
            tableau.Add(cp);
        }

        foreach (CardProspector tCP in tableau)
        {
            // GOLD CARDS ------------->
            if (Random.value <= goldSpawnChance)
            {
                print($"Turning {tCP.name} to gold!");
                tCP.isGold = true;
                tCP.back.GetComponent<SpriteRenderer>().sprite = deck.cardBackGold;
                tCP.GetComponent<SpriteRenderer>().sprite = deck.cardFrontGold;
            }
            // <----------- GOLD CARDS

            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        MoveToTarget(Draw());
        UpdateDrawPile();
    }

    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (CardProspector tCP in tableau)
        {
            if (tCP.layoutID == layoutID)
                return tCP;
        }
        return null;
    }

    void SetTableauFaces()
    {
        foreach (CardProspector cd in tableau)
        {
            bool faceUp = true;
            foreach (CardProspector cover in cd.hiddenBy)
            {
                if (cover.state == eCardState.tableau)
                    faceUp = false;
            }
            cd.faceUp = faceUp;
        }
    }

    void MoveToDiscard(CardProspector cd)
    {
        cd.state = eCardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;

        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + .5f);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    void MoveToTarget(CardProspector cd)
    {
        if (target != null) MoveToDiscard(target);
        target = cd;
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    void UpdateDrawPile()
    {
        CardProspector cd;
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x), layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y), -layout.drawPile.layerID + .1f * i);
            cd.faceUp = false;
            cd.state = eCardState.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public void CardClicked(CardProspector cd)
    {
        switch (cd.state)
        {
            case eCardState.target:
                break;

            case eCardState.drawpile:
                MoveToDiscard(target);
                MoveToTarget(Draw());
                UpdateDrawPile();
                ScoreManager.EVENT(eScoreEvent.draw);
                FloatingScoreHandler(eScoreEvent.draw);
                break;

            case eCardState.tableau:
                bool validMatch = true;
                if (!cd.faceUp)
                    validMatch = false;
                if (!AdjacentRank(cd, target))
                    validMatch = false;
                if (!validMatch) return;
                tableau.Remove(cd);
                MoveToTarget(cd);
                SetTableauFaces();

                // <-------- checked card if it was a gold one, added to current run count
                if (cd.isGold)
                {
                    ScoreManager.GOLD_CARDS_ACTIVE++;
                    currentGoldText.text = $"Current Gold: {ScoreManager.GOLD_CARDS_ACTIVE}";
                }
                // <--------- check card if it was a gold one, added to current run count

                ScoreManager.EVENT(eScoreEvent.mine);
                FloatingScoreHandler(eScoreEvent.mine);
                break;
        }
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (tableau.Count == 0)
        {
            GameOver(true);
            return;
        }
        if (drawPile.Count > 0)
            return;

        foreach (CardProspector cd in tableau)
        {
            if (AdjacentRank(cd, target))
                return;
        }

        GameOver(false);
    }

    void GameOver(bool won)
    {
        int score = ScoreManager.SCORE;
        if (fsRun != null) score += fsRun.score;
        if (won)
        {
            gameOverText.text = "Round Over";
            roundResultText.text = $"You won this round:\nRound Score: {score}";
            ShowResultsUI(true);
            print($"Game Over! You Won!");
            ScoreManager.EVENT(eScoreEvent.gameWin);
            FloatingScoreHandler(eScoreEvent.gameWin);
        }
        else
        {
            gameOverText.text = "Game Over";
            if (ScoreManager.HIGH_SCORE <= score)
            {
                string str = $"You got the high score:\nHigh Score: {score}";
                roundResultText.text = str;
            }
            else
                roundResultText.text = $"Your final score was: {score}";
            ShowResultsUI(true);
            print($"Game Over! You Lost!");
            ScoreManager.EVENT(eScoreEvent.gameLose);
            FloatingScoreHandler(eScoreEvent.gameLose);
        }
        Invoke("ReloadLevel", reloadDelay);
    }

    void ReloadLevel() => SceneManager.LoadScene(0);

    void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch (evt)
        {
            case eScoreEvent.draw:
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLose:
                if (fsRun != null)
                {
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);

                    // >-------- fsRun from mining previously -------->
                    int modifiedTotal = fsRun.score * Mathf.CeilToInt(Mathf.Pow(2, ScoreManager.GOLD_CARDS_ACTIVE));
                    fsRun.score = modifiedTotal;
                    print($"Total Score: {modifiedTotal} from score: {ScoreManager.SCORE} + gold cards: {ScoreManager.GOLD_CARDS_ACTIVE} ");
                    // <--------- fsRun modified score added ----------<

                    fsRun.reportFinishTo = ScoreBoard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null;
                    ScoreManager.GOLD_CARDS_ACTIVE = 0;
                    currentGoldText.text = $"Current Gold: {ScoreManager.GOLD_CARDS_ACTIVE}";
                }
                break;

            case eScoreEvent.mineGold:
            case eScoreEvent.mine:
                FloatingScore fs;
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = ScoreBoard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                    fs.reportFinishTo = fsRun.gameObject;
                break;
        }
    }

    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        if (!c0.faceUp || !c1.faceUp) return false;
        if (Mathf.Abs(c0.rank - c1.rank) == 1)
            return true;
        if (c0.rank == 1 && c1.rank == 1) return true;
        if (c0.rank == 13 && c1.rank == 1) return true;
        return false;
    }
}