using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI.Pagination
{
    /// <summary>
    /// With thanks to "chikku techie" @ http://chikkooos.blogspot.jp/2015/02/detecting-swipe-from-unity3d-c-scripts.html
    /// </summary>
    public class MobileInput : MonoBehaviour
    {                
        private readonly Vector2 mXAxis = new Vector2(1, 0);
        private readonly Vector2 mYAxis = new Vector2(0, 1);

        private const float mAngleRange = 30;
        public float mMinSwipeDist = 15f;

        private const float mMinVelocity = 600f;

        private Vector2 mStartPosition;
        private float mSwipeStartTime;

        public Action OnSwipeRight = null;
        public Action OnSwipeLeft = null;
        public Action OnSwipeUp = null;
        public Action OnSwipeDown = null;
        
        void Update()
        {
            // Mouse button down, possible chance for a swipe
            if (Input.GetMouseButtonDown(0))
            {
                // Record start time and position
                mStartPosition = new Vector2(Input.mousePosition.x,
                                             Input.mousePosition.y);
                mSwipeStartTime = Time.time;
            }

            // Mouse button up, possible chance for a swipe
            if (Input.GetMouseButtonUp(0))
            {
                float deltaTime = Time.time - mSwipeStartTime;

                Vector2 endPosition = new Vector2(Input.mousePosition.x,
                                                   Input.mousePosition.y);
                Vector2 swipeVector = endPosition - mStartPosition;

                float velocity = swipeVector.magnitude / deltaTime;

                if (velocity > mMinVelocity &&
                    swipeVector.magnitude > mMinSwipeDist)
                {
                    // if the swipe has enough velocity and enough distance

                    swipeVector.Normalize();

                    float angleOfSwipe = Vector2.Dot(swipeVector, mXAxis);
                    angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;

                    // Detect left and right swipe
                    if (angleOfSwipe < mAngleRange)
                    {
                        if (OnSwipeRight != null)
                        {
                            OnSwipeRight();
                        }
                    }
                    else if ((180.0f - angleOfSwipe) < mAngleRange)
                    {
                        if (OnSwipeLeft != null)
                        {
                            OnSwipeLeft();
                        }
                    }
                    else
                    {
                        // Detect top and bottom swipe
                        angleOfSwipe = Vector2.Dot(swipeVector, mYAxis);
                        angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
                        if (angleOfSwipe < mAngleRange)
                        {
                            if (OnSwipeUp != null)
                            {
                                OnSwipeUp();
                            }
                        }
                        else if ((180.0f - angleOfSwipe) < mAngleRange)
                        {
                            if (OnSwipeDown != null)
                            {
                                OnSwipeDown();
                            }
                        }                        
                    }
                }
            }
        }
    }
}
