using UnityEngine;

public enum eScoreEvent { draw, mine, mineGold, gameWin, gameLose }

public class ScoreManager : MonoBehaviour
{
    static ScoreManager S;
    public static int SCORE_FROM_PREV_ROUND = 0;
    public static int HIGH_SCORE = 100;
    public static int GOLD_CARDS_ACTIVE = 0;
    public string highScoreKeyString = "ProspectorHighScore";

    int chain = 0;
    int scoreRun = 0;
    int score = 0;

    public void Awake()
    {
        if (S == null)
            S = this;
        else
            Debug.LogError($"ERROR: ScoreManager.Awake() : S is already set!");

        if (PlayerPrefs.HasKey(highScoreKeyString))
            HIGH_SCORE = PlayerPrefs.GetInt(highScoreKeyString);

        score += SCORE_FROM_PREV_ROUND;
        SCORE_FROM_PREV_ROUND = 0;
    }

    public static void EVENT(eScoreEvent evt)
    {
        try { S.Event(evt); }
        catch (System.NullReferenceException nre) { Debug.LogError($"ScoreManager : EVENT() called while S = null \n {nre}"); }
    }

    void Event(eScoreEvent evt)
    {
        switch (evt)
        {
            case eScoreEvent.draw:
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLose:
                chain = 0;
                score += scoreRun;
                scoreRun = 0;
                break;

            case eScoreEvent.mineGold:
            case eScoreEvent.mine:
                chain++;
                scoreRun += chain;
                break;
        }

        switch (evt)
        {
            case eScoreEvent.gameWin:
                SCORE_FROM_PREV_ROUND = score;
                print($"You won this round! Round Score: {score}");
                break;

            case eScoreEvent.gameLose:
                if (HIGH_SCORE <= score)
                {
                    print($"You got the high score: {score}");
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt(highScoreKeyString, score);
                }
                else print($"Your final score for the game was: {score}");

                break;

            default:
                print($"Score: {score} scoreRun: {scoreRun} chain: {chain} gold cards: {GOLD_CARDS_ACTIVE}");
                break;
        }
    }

    public static int CHAIN { get { return S.chain; } }
    public static int SCORE { get { return S.score; } }
    public static int SCORE_RUN { get { return S.scoreRun; } }
}