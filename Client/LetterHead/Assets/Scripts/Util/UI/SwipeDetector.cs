using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwipeDetector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public float minSwipeLength = 100;

    private Vector2 dragOffset;
    private float canvasScale;



    [Serializable]
    public class SwipeEvent : UnityEvent<bool> { };
    public SwipeEvent OnHorizontalSwipe;
    public SwipeEvent OnVerticalSwipe;

    public ScrollRect scrollRect;

    public void OnDrag(PointerEventData eventData)
    {
        dragOffset += eventData.delta;
        eventData.eligibleForClick = false;
        scrollRect.OnDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        scrollRect.OnBeginDrag(eventData);

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        canvasScale = canvas.scaleFactor;

        dragOffset = Vector2.zero;
        eventData.eligibleForClick = false;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        scrollRect.OnEndDrag(eventData);

        var offset = dragOffset / canvasScale;

        if (Mathf.Abs(offset.x) >= minSwipeLength)
        {
            OnHorizontalSwipe.Invoke(offset.x > 0);
        }

        if (Mathf.Abs(offset.y) >= minSwipeLength)
        {
            OnVerticalSwipe.Invoke(offset.y > 0);
        }

        eventData.eligibleForClick = false;
    }
}
