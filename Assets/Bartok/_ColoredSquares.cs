using TMPro;
using UnityEngine;

public class _ColoredSquares : MonoBehaviour
{
    [SerializeField] GameObject objectPrefab;
    GameObject currentObject;
    [SerializeField] SpriteRenderer[] targets;
    [SerializeField] TMP_Text colorChangeCounter;
    [SerializeField] float moveSpeed;
    Color[] colors;
    SpriteRenderer target;
    Color currentColor;
    int counter = 0;

    void Start()
    {
        colors = new Color[targets.Length];
        for (int i = 0; i < targets.Length; i++)
            colors[i] = targets[i].color;
        InitializeObject();
    }

    void InitializeObject()
    {
        currentObject = Instantiate(objectPrefab, Vector3.zero, Quaternion.identity);
        SpriteRenderer mSRend = currentObject.GetComponent<SpriteRenderer>();
        if (mSRend != null)
            currentColor = mSRend.color = colors[Random.Range(0, colors.Length)];
        SetNewTarget();
    }


    void FixedUpdate()
    {
        if (!CheckForAllColored())
        {
            if (currentObject != null)
            {
                if (currentObject.transform.position != target.transform.position)
                    currentObject.transform.position = Vector3.MoveTowards(currentObject.transform.position, target.transform.position, moveSpeed * Time.fixedDeltaTime);
                else if (currentObject.transform.position == target.transform.position)
                {
                    SetSpriteColor();
                    Destroy(currentObject);
                    InitializeObject();
                }
            }
        }
    }

    public bool CheckForAllColored()
    {
        bool isAllColored = true;
        foreach (var t in targets)
        {
            if (!t.color.Equals(currentColor))
                isAllColored = false;
        }
        return isAllColored;
    }

    public void SetNewTarget() => target = targets[Random.Range(0, targets.Length)];

    public void SetSpriteColor()
    {
        if (!target.color.Equals(currentColor))
        {
            target.color = currentColor;
            counter++;
            colorChangeCounter.text = $"{counter}";
        }
    }
}