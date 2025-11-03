using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeManager : MonoBehaviour
{
    private static SwipeManager instance;
    
    [Header("Swipe Settings")]
    [SerializeField] private float minSwipeDistance = 50f;
    [SerializeField] private float maxSwipeTime = 0.5f;
    
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float startTime;
    
    private static bool swipeLeft;
    private static bool swipeRight;
    private static bool swipeUp;
    private static bool swipeDown;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        // Reset swipe flags
        swipeLeft = false;
        swipeRight = false;
        swipeUp = false;
        swipeDown = false;
        
        #if UNITY_EDITOR || UNITY_STANDALONE
        // Mouse input for testing in editor
        HandleMouseInput();
        #else
        // Touch input for mobile
        HandleTouchInput();
        #endif
    }
    
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
            startTime = Time.time;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            endTouchPosition = Input.mousePosition;
            DetectSwipe();
        }
    }
    
    void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startTouchPosition = touch.position;
                    startTime = Time.time;
                    break;
                    
                case TouchPhase.Ended:
                    endTouchPosition = touch.position;
                    DetectSwipe();
                    break;
            }
        }
    }
    
    void DetectSwipe()
    {
        float swipeTime = Time.time - startTime;
        
        if (swipeTime > maxSwipeTime)
            return;
            
        Vector2 swipeVector = endTouchPosition - startTouchPosition;
        float swipeDistance = swipeVector.magnitude;
        
        if (swipeDistance < minSwipeDistance)
            return;
            
        swipeVector.Normalize();
        
        // Determine swipe direction
        if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
        {
            // Horizontal swipe
            if (swipeVector.x > 0)
            {
                swipeRight = true;
                OnSwipeRight();
            }
            else
            {
                swipeLeft = true;
                OnSwipeLeft();
            }
        }
        else
        {
            // Vertical swipe
            if (swipeVector.y > 0)
            {
                swipeUp = true;
                OnSwipeUp();
            }
            else
            {
                swipeDown = true;
                OnSwipeDown();
            }
        }
    }
    
    // Virtual methods for swipe events
    void OnSwipeLeft()
    {
        Debug.Log("Swipe Left");
    }
    
    void OnSwipeRight()
    {
        Debug.Log("Swipe Right");
    }
    
    void OnSwipeUp()
    {
        Debug.Log("Swipe Up");
    }
    
    void OnSwipeDown()
    {
        Debug.Log("Swipe Down");
    }
    
    // Static methods to check swipe states
    public static bool IsSwipingLeft()
    {
        return swipeLeft;
    }
    
    public static bool IsSwipingRight()
    {
        return swipeRight;
    }
    
    public static bool IsSwipingUp()
    {
        return swipeUp;
    }
    
    public static bool IsSwipingDown()
    {
        return swipeDown;
    }
}
