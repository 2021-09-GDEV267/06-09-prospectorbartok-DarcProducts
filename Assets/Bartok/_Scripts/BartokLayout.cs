using System.Collections.Generic;
using UnityEngine;

namespace darcproducts
{
    [System.Serializable]
    public class SlotDef
    {
        public float x;
        public float y;
        public bool faceUp = false;
        public string layerName = "Default";
        public int layerID = 0;
        public int id;
        public List<int> hiddenBy = new List<int>();
        public float rot;
        public string type = "slot";
        public Vector2 stagger;
        public int player;
        public Vector3 pos;
    }
}

public class BartokLayout : MonoBehaviour
{
    [HideInInspector]
    public PT_XMLReader xmlr;

    [HideInInspector]
    public PT_XMLHashtable xml;

    [HideInInspector]
    public Vector2 multiplier;

    [HideInInspector]
    public List<darcproducts.SlotDef> slotDefs;

    [HideInInspector]
    public darcproducts.SlotDef drawPile;

    [HideInInspector]
    public darcproducts.SlotDef discardPile;

    [HideInInspector]
    public darcproducts.SlotDef target;

    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);
        xml = xmlr.xml["xml"][0];

        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        darcproducts.SlotDef tSD;

        PT_XMLHashList slotsX = xml["slot"];

        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new darcproducts.SlotDef();
            if (slotsX[i].HasAtt("type"))
                tSD.type = slotsX[i].att("type");
            else
                tSD.type = "slot";

            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            tSD.layerName = tSD.layerID.ToString();

            switch (tSD.type)
            {
                case "slot":
                    break;

                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;

                case "discardpile":
                    discardPile = tSD;
                    break;

                case "target":
                    target = tSD;
                    break;

                case "hand":
                    tSD.player = int.Parse(slotsX[i].att("player"));
                    tSD.rot = float.Parse(slotsX[i].att("rot"));
                    slotDefs.Add(tSD);
                    break;
            }
        }
    }
}