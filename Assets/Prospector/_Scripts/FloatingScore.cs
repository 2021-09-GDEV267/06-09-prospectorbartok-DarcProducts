using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum eFSState { idle, pre, active, post }

public class FloatingScore : MonoBehaviour
{
    eFSState state = eFSState.idle;
    protected int _score = 0;
    string scoreString;

    List<Vector2> bezierPts;

    [HideInInspector]
    public List<float> fontSizes;

    float timeStart = -1f;
    float timeDuration = 1f;

    public AnimationCurve easingCurve = new AnimationCurve();

    [HideInInspector]
    public GameObject reportFinishTo = null;

    RectTransform recTrans;
    TMP_Text txt;

    void Update()
    {
        if (state == eFSState.idle) return;
        float u = (Time.time - timeStart) / timeDuration;
        
        float uC = Mathf.Clamp01(easingCurve.Evaluate(u));

        if (u < 0)
        {
            state = eFSState.pre;
            txt.enabled = false;
        }
        else
        {
            if (u >= 1)
            {
                uC = 1;
                state = eFSState.post;
                if (reportFinishTo != null)
                {
                    reportFinishTo.SendMessage("FSCallBack", this);
                    Destroy(gameObject);
                }
                else
                    state = eFSState.idle;
            }
            else
            {
                state = eFSState.active;
                txt.enabled = true;
            }

            Vector2 pos = Utils.Bezier(uC, bezierPts);
            recTrans.anchorMin = recTrans.anchorMax = pos;

            if (fontSizes != null && fontSizes.Count > 0)
            {
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<TMP_Text>().fontSize = size;
            }
        }
    }

    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        recTrans = GetComponent<RectTransform>();
        recTrans.anchoredPosition = Vector2.zero;

        txt = GetComponent<TMP_Text>();
        bezierPts = new List<Vector2>(ePts);

        if (ePts.Count == 1)
        {
            transform.position = ePts[0];
            return;
        }

        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;

        state = eFSState.pre;
    }

    public int score
    {
        get { return _score; }
        set
        {
            _score = value;
            scoreString = _score.ToString();
            GetComponent<TMP_Text>().text = scoreString;
        }
    }

    public void FSCallBack(FloatingScore fs) => score += fs.score;
}