using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    public static ScoreBoard S;
    [SerializeField] GameObject prefabFloatingScore;

    int _score = 0;
    string _scoreString;
    Transform canvasTrans;

    void Awake()
    {
        if (S == null)
            S = this;
        else
            Debug.LogError($"ERROR: ScoreBoard.Awake() : S is already set!");
        canvasTrans = transform.parent;
    }

    public void FSCallBack(FloatingScore fs) => score += fs.score;

    public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts)
    {
        GameObject go = Instantiate<GameObject>(prefabFloatingScore);
        go.transform.SetParent(canvasTrans);
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject;
        fs.Init(pts);
        return fs;
    }

    public int score
    {
        get { return _score; }
        set
        {
            _score = value;
            scoreString = _score.ToString();
        }
    }

    public string scoreString
    {
        get { return _scoreString; }
        set
        {
            _scoreString = value;
            GetComponent<TMP_Text>().text = _scoreString;
        }
    }
}