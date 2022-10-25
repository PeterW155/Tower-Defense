using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PopupHandler : MonoBehaviour
{
    public enum Direction { up, down, left, right }
    public List<RectTransform> popups;
    public List<RectTransform> offsets;
    [Space(25)]
    public Direction direction;
    public float animationTime = 0.5f;
    public AnimationCurve animationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private int currentActive;
    private bool animating;
    private bool activating;

    private void Start()
    {
        currentActive = -1;
        animating = false;
        activating = false;
    }

    public void ActivatePopup(int index)
    {
        if (!activating)
            StartCoroutine(AwaitAnimation(index));
    }

    private IEnumerator AwaitAnimation(int index)
    {
        activating = true;
        if (index == currentActive) //if selected active needs to be deactivated
        {
            //deactive current popup
            RectTransform deactivePopup = popups.ElementAtOrDefault(index);
            StartCoroutine(Animation(deactivePopup, 0));
            //wait for deactivate animation
            while (animating)
                yield return null;
            currentActive = -1;
        }
        else if (currentActive != -1) //if selected active is different from an already current active
        {
            //deactive current popup
            RectTransform deactivePopup = popups.ElementAtOrDefault(currentActive);
            StartCoroutine(Animation(deactivePopup, 0));
            //wait for deactivate animation
            while (animating)
                yield return null;
            //activate new popup
            RectTransform activePopup = popups.ElementAtOrDefault(index);
            StartCoroutine(Animation(activePopup, GetTarget(index)));
            //wait for activate animation
            while (animating)
                yield return null;
            currentActive = index;


        }
        else //nothing is active so activate index
        {
            //activate new popup
            RectTransform activePopup = popups.ElementAtOrDefault(index);
            StartCoroutine(Animation(activePopup, GetTarget(index)));
            //wait for activate animation
            while (animating)
                yield return null;
            currentActive = index;
        }

        float GetTarget(int index)
        {
            float popupMovementTarget = 0;
            if (popups != null)
            {
                RectTransform popup = popups.ElementAtOrDefault(index);

                switch (direction)
                {
                    case Direction.up:
                        popupMovementTarget = popup.rect.height;
                        break;
                    case Direction.down:
                        popupMovementTarget = -popup.rect.height;
                        break;
                    case Direction.right:
                        popupMovementTarget = popup.rect.width;
                        break;
                    case Direction.left:
                        popupMovementTarget = -popup.rect.width;
                        break;
                }
            }
            return popupMovementTarget;
        }

        activating = false;

        yield return null;
    }

    private IEnumerator Animation(RectTransform rt, float movementTarget)
    {
        animating = true;
        Vector2 init = rt.anchoredPosition;

        float currentTime = 0;
        while (currentTime < animationTime)
        {
            if (direction == Direction.up || direction == Direction.down)
            {
                rt.anchoredPosition = new Vector2(init.x, Mathf.LerpUnclamped(init.y, movementTarget, animationCurve.Evaluate(currentTime / animationTime)));
                if (offsets != null)
                    foreach (RectTransform offset in offsets)
                        offset.anchoredPosition = new Vector2(init.x, Mathf.LerpUnclamped(init.y, movementTarget, animationCurve.Evaluate(currentTime / animationTime)));
            }
            else
            {
                rt.anchoredPosition = new Vector2(Mathf.LerpUnclamped(init.x, movementTarget, animationCurve.Evaluate(currentTime / animationTime)), init.y);
                if (offsets != null)
                    foreach (RectTransform offset in offsets)
                        offset.anchoredPosition = new Vector2(Mathf.LerpUnclamped(init.x, movementTarget, animationCurve.Evaluate(currentTime / animationTime)), init.y);
            }

            currentTime += Time.unscaledDeltaTime;

            yield return null;        
        }
        //set final position
        if (direction == Direction.up || direction == Direction.down)
            rt.anchoredPosition = new Vector2(init.x, movementTarget);
        else
            rt.anchoredPosition = new Vector2(movementTarget, init.y);

        animating = false;
    }
}
