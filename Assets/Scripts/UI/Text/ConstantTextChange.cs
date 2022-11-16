using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConstantTextChange : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float interval;
    public List<string> textStrings;

    private int currentIndex;

    private Coroutine ctc;

    private void Awake()
    {
        if (!text)
            text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        ctc = StartCoroutine(TextChange());
    }

    private void OnDisable()
    {
        if (ctc != null)
            StopCoroutine(ctc);
    }

    private IEnumerator TextChange()
    {
        currentIndex = 0;

        if (textStrings != null && textStrings.Count > 1)
        {
            for (; ; )
            {
                text.text = textStrings[currentIndex];

                ++currentIndex;
                if (currentIndex >= textStrings.Count)
                    currentIndex = 0;

                yield return new WaitForSecondsRealtime(interval);
            }
        }

        yield return null;
    }
}
