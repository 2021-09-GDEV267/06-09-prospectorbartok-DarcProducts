using System.Collections.Generic;
using UnityEngine;

public enum CBState
{
    toDrawpile,
    drawpile,
    toHand,
    hand,
    toTarget,
    target,
    discard,
    to,
    idle
}

public class CardBartok : Card
{
    public static float MOVE_DURATION = 0.5f;
    public AnimationCurve easingCurve;
    public static float CARD_HEIGHT = 3.5f;
    public static float CARD_WIDTH = 2f;
    [HideInInspector]
    public int eventualSortOrder;

    [HideInInspector]
    public string eventualSortLayer;

    [HideInInspector]
    public CBState state = CBState.toDrawpile;

    [HideInInspector]
    public List<Vector3> bezierPts;

    [HideInInspector]
    public List<Quaternion> bezierRots;

    [HideInInspector]
    public float timeStart, timeDuration;

    [HideInInspector]
    public GameObject reportFinishTo = null;
    [System.NonSerialized]
    public Player callbackPlayer = null;

    void Update()
    {
        switch (state)
        {
            case CBState.toHand:
            case CBState.toTarget:
            case CBState.toDrawpile:
            case CBState.to:
                float u = (Time.time - timeStart) / timeDuration;
                float uC = Mathf.Clamp01(easingCurve.Evaluate(u));

                if (u < 0)
                {
                    transform.localPosition = bezierPts[0];
                    transform.rotation = bezierRots[0];
                    return;
                }
                else if (u >= 1)
                {
                    uC = 1;

                    if (state == CBState.toHand) state = CBState.hand;
                    if (state == CBState.toTarget) state = CBState.target;
                    if (state == CBState.toDrawpile) state = CBState.drawpile;
                    if (state == CBState.to) state = CBState.idle;

                    transform.localPosition = bezierPts[bezierPts.Count - 1];
                    transform.rotation = bezierRots[bezierPts.Count - 1];

                    timeStart = 0;

                    if (reportFinishTo != null)
                    {
                        reportFinishTo.SendMessage("CBCallback", this);
                        reportFinishTo = null;
                    }
                    else if (callbackPlayer != null)
                    {
                        callbackPlayer.CBCallback(this);
                        callbackPlayer = null;
                    }
                    else
                    {
                        //just let stay still
                    }
                }
                else
                {
                    Vector3 pos = Utils.Bezier(uC, bezierPts);
                    transform.localPosition = pos;
                        
                    Quaternion rotQ = Utils.Bezier(uC, bezierRots);
                    transform.rotation = rotQ;

                    if (u > 0.5f)
                    {
                        SpriteRenderer sRend = spriteRenderers[0];
                        if (sRend.sortingOrder != eventualSortOrder)
                            SetSortOrder(eventualSortOrder);
                        if (sRend.sortingLayerName != eventualSortLayer)
                            SetSortingLayerName(eventualSortLayer);
                    }
                }
                break;
        }
    }

    public void MoveTo(Vector3 ePos, Quaternion eRot)
    {
        bezierPts = new List<Vector3>();
        bezierPts.Add(transform.localPosition);
        bezierPts.Add(ePos);

        bezierRots = new List<Quaternion>();
        bezierRots.Add(transform.rotation);
        bezierRots.Add(eRot);

        if (timeStart == 0)
            timeStart = Time.time;

        timeDuration = MOVE_DURATION;

        state = CBState.to;
    }

    public void MoveTo(Vector3 ePos) => MoveTo(ePos, Quaternion.identity);

    public override void OnMouseUpAsButton()
    {
        Bartok.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}