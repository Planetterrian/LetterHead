using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Pagination
{
    /// <summary>
    /// A collection of pages, with optional pagination, animation, etc.
    /// </summary>
    [ExecuteInEditMode]
    public class PagedRect : MonoBehaviour, IEndDragHandler
    {        
        public int CurrentPage { get; protected set; }
        public int DefaultPage;        

        public int NumberOfPages { get { return Pages.Count; } }        

        [Tooltip("If this is true, then the pages property will automatically be populated from any child GameObjects with the 'Page' component.")]
        public bool AutoDiscoverPages = true;

        [Header("Pagination")]
        [Tooltip("If this is set to false, the page buttons will not be shown.")]
        public bool ShowPagination = true;

        public bool ShowFirstAndLastButtons = true;
        public bool ShowPreviousAndNextButtons = true;

        [Tooltip("If there are too many page buttons to show at once, use this field to limit the number of visible buttons. 0 == No Limit")]
        public int MaximumNumberOfButtonsToShow = 15;

        [Tooltip("Set this to false to hide the button templates in edit mode")]        
        public bool ShowButtonTemplatesInEditor = true;

        public bool ShowPageButtons = true;

        public bool ShowNumbersOnButtons = true;
        public bool ShowPageTitlesOnButtons = false;        

        [Header("Animation")]        
        public eAnimationType AnimationType = eAnimationType.SlideHorizontal;
        protected eAnimationType previousAnimationTypeValue;
        [Range(0.1f, 5f)]
        public float AnimationSpeed = 1.0f;

        [Header("Automation")]
        public bool AutomaticallyMoveToNextPage = false;
        public float DelayBetweenPages = 5f;      
        public bool LoopEndlessly = true;
        protected float _timeSinceLastPage = 0f;

        [Header("New Page Template")]
        [Tooltip("Optional Template for adding new pages dynamically at runtime.")]
        public Page NewPageTemplate;

        [Header("Keyboard Input")]
        public bool UseKeyboardInput = false;
        public KeyCode PreviousPageKey = KeyCode.LeftArrow;
        public KeyCode NextPageKey = KeyCode.RightArrow;
        public KeyCode FirstPageKey = KeyCode.Home;
        public KeyCode LastPageKey = KeyCode.End;
                
        [Header("Mobile Input")]        
        public bool UseSwipeInput = true;

        [Header("ScrollRect - Continuous Scrolling"), Tooltip("How large a swipe is needed in order to move to the next/previous page?")]
        public float SwipeDeltaThreshold = .1f;

        [Header("Scroll Wheel Input")]
        public bool UseScrollWheelInput = false;
        public bool OnlyUseScrollWheelInputWhenMouseIsOver = true;

        [Header("Highlight")]
        public bool HighlightWhenMouseIsOver = false;
        public Color NormalColor = new Color(1f, 1f, 1f);
        public Color HighlightColor = new Color(0.9f, 0.9f, 0.9f);
        
        protected bool mouseIsOverPagedRect = false;
 
        [Header("Events")]        
        public PageChangedEventType PageChangedEvent = new PageChangedEventType();

        [Serializable]
        public class PageChangedEventType : UnityEngine.Events.UnityEvent<Page, Page> { }

        [Header("References")]
        public UnityEngine.UI.ScrollRect ScrollRect;
        public GameObject ScrollRectViewport;
        public GameObject Viewport;
        public GameObject Pagination;
        public PaginationButton ButtonTemplate_CurrentPage;
        public PaginationButton ButtonTemplate_OtherPages;
        public PaginationButton ButtonTemplate_DisabledPage;

        public PaginationButton Button_PreviousPage;
        public PaginationButton Button_NextPage;
        public PaginationButton Button_FirstPage;
        public PaginationButton Button_LastPage;

        public RuntimeAnimatorController AnimationControllerTemplate;

        public List<Page> Pages = new List<Page>();

        public int editorSelectedPage = 1;

        public RectTransform sizingTransform;

        /// <summary>
        /// This is used to check for changes to the Page collection and avoid updating except where necessary
        /// If we do update unnecessarily, the scene gets marked as dirty without it actually needing to be
        /// </summary>
        protected List<Page> _pages = new List<Page>();
        public bool isDirty { get; set; }

        private MobileInput _MobileInput = null;
        public MobileInput MobileInput
        {
            get
            {
                if (_MobileInput == null)
                {
                    _MobileInput = this.GetComponent<MobileInput>();

                    if (_MobileInput == null && Application.isPlaying)
                    {
                        _MobileInput = this.gameObject.AddComponent<MobileInput>();

                        _MobileInput.OnSwipeLeft = _MobileInput.OnSwipeUp = () => this.NextPage();
                        _MobileInput.OnSwipeRight = _MobileInput.OnSwipeDown = () => this.PreviousPage();
                    }                    
                }

                return _MobileInput;
            }
        }

        private ScrollWheelInput _ScrollWheelInput = null;
        public ScrollWheelInput ScrollWheelInput
        {
            get
            {
                if (_ScrollWheelInput == null)
                {
                    _ScrollWheelInput = this.GetComponent<ScrollWheelInput>();

                    if (_ScrollWheelInput == null && Application.isPlaying)
                    {
                        _ScrollWheelInput = this.gameObject.AddComponent<ScrollWheelInput>();

                        _ScrollWheelInput.OnScrollUp = () => this.NextPage();
                        _ScrollWheelInput.OnScrollDown = () => this.PreviousPage();
                    }
                }

                return _ScrollWheelInput;
            }
        }

        private UnityEngine.UI.Image _imageComponent;
        protected UnityEngine.UI.Image imageComponent
        {
            get
            {
                if (_imageComponent == null)
                {
                    _imageComponent = this.GetComponent<UnityEngine.UI.Image>();
                }

                return this._imageComponent;
            }
        }


        private bool? _UsingScrollRect;
        /// <summary>
        /// If this is true, this PagedRect has detected that it is using a ScrollRect, and some behaviour will work differently (e.g. MobileInput)
        /// </summary>
        public bool UsingScrollRect
        {
            get
            {
                if (!_UsingScrollRect.HasValue)
                {
                    _UsingScrollRect = ScrollRect != null;
                }

                return _UsingScrollRect.Value;
            }
        }

        protected Vector2 _ScrollRectPosition = new Vector2();
        
        //protected Vector2 _lerp_ScrollRectInitialPosition = new Vector2();
        //protected Vector2 _lerp_DesiredScrollRectPosition = new Vector2();                
        protected float _lerp_ScrollRectInitialPosition = 0f;
        protected float _lerp_DesiredScrollRectPosition = 0f;
        protected float _lerp_timeStartedMoving = 0f;
        [NonSerialized]
        protected bool firstPageSet = false;

        protected List<KeyValuePair<double, Action>> delayedEditorActions = new List<KeyValuePair<double, Action>>();

        #region Enumerators
        public enum eAnimationType
        {
            None,
            SlideHorizontal,
            SlideVertical,
            Fade
        }

        public enum eDirection
        {
            Left,
            Right
        }
        #endregion

        #region Unity Functions

        void Awake()
        {
            CurrentPage = DefaultPage;
            if (UsingScrollRect)
            {
                ScrollRect.horizontalNormalizedPosition = 0;
                ScrollRect.verticalNormalizedPosition = 0;

                ScrollRect.content.anchoredPosition = Vector2.zero;

                CenterScrollRectOnCurrentPage(true);
            }
        }
        
        void LateUpdate()
        {            
            if (!firstPageSet)
            {                
                if (UsingScrollRect)
                {                    
                    CenterScrollRectOnCurrentPage(true);

                    StartCoroutine(DelayedCall(0.01f, () => CenterScrollRectOnCurrentPage(true)));
                }
                
                UpdatePagination();

                firstPageSet = true;
            }
        }

        void EditorUpdate()
        {
            if (Application.isPlaying) return;

#if UNITY_EDITOR                        
            var actionsToExecute = delayedEditorActions.Where(kvp => UnityEditor.EditorApplication.timeSinceStartup >= kvp.Key).ToList();                        
            foreach (var actionToExecute in actionsToExecute)
            {
                try
                {
                    actionToExecute.Value.Invoke();
                }
                finally
                {
                    delayedEditorActions.Remove(actionToExecute);
                }
            }
#endif
        }

        void Start()
        {            
            this.GetComponentInChildren<Viewport>().Initialise(this);

            if (UsingScrollRect)
            {                
                ScrollRect.onValueChanged.AddListener(ScrollRectValueChanged);                  
            }

            UpdateDisplay();

            if (Application.isPlaying)
            {
                if (UseSwipeInput && !UsingScrollRect)
                {
                    MobileInput.enabled = true;
                }

                if (UseScrollWheelInput)
                {
                    ScrollWheelInput.enabled = true;
                }                
            }

            if (Application.isPlaying)
            {
                SetupMouseEvents();
            }

            // Show the default first page            
            if (Application.isPlaying)
            {
                SetCurrentPage(DefaultPage, true);
            }
            else
            {
#if UNITY_EDITOR
                if (CurrentPage == 0)
                {
                    if (editorSelectedPage > 0)
                    {
                        var pageIsVisible = Pages[editorSelectedPage - 1].gameObject.activeSelf;
                        if (pageIsVisible && !UsingScrollRect)
                        {
                            CurrentPage = editorSelectedPage;
                        }
                        else
                        {
                            SetCurrentPage(editorSelectedPage, true);
                        }
                    }
                    else
                    {
                        SetCurrentPage(DefaultPage, true);
                    }
                }
                else if(UsingScrollRect)
                {
                    SetCurrentPage(editorSelectedPage, true);                    
                }

                UnityEditor.EditorApplication.update += EditorUpdate;
#endif
            }
        }

        void Update()
        {
            if (!Application.isPlaying) return;            

            // Check for dynamic changes to animation type
            if (previousAnimationTypeValue != AnimationType)
            {
                // if we don't do this, some of the pages may end up off-screen due to previous animations
                Pages.ForEach(p => p.ResetPositionAndAlpha());
            }
            previousAnimationTypeValue = AnimationType;

            // Handle Keyboard Input
            if (UseKeyboardInput)
            {
                HandleKeyboardInput();
            }

            _timeSinceLastPage += Time.deltaTime;

            // Handle moving to the next page if need be
            if (AutomaticallyMoveToNextPage)
            {                
                if (_timeSinceLastPage >= DelayBetweenPages)
                {
                    if (CurrentPage >= NumberOfPages)
                    {
                        if (LoopEndlessly)
                        {
                            SetCurrentPage(1);
                        }
                    }
                    else
                    {
                        SetCurrentPage(CurrentPage + 1);
                    }
                }
            }

            if (!mouseIsOverPagedRect && OnlyUseScrollWheelInputWhenMouseIsOver)
            {
                ScrollWheelInput.enabled = false;
            }
            else
            {
                ScrollWheelInput.enabled = UseScrollWheelInput;
            }
        }

        void OnValidate()
        {
            if (Application.isPlaying)
            {
                UpdateDisplay();
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (!Application.isPlaying && this.isDirty)
                    {                                            
                        UpdateDisplay();
                    }
                };
#endif
            }
        }
        #endregion

        void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(NextPageKey))
            {
                NextPage();
            }
            else if (Input.GetKeyDown(PreviousPageKey))
            {
                PreviousPage();
            }
            else if (Input.GetKeyDown(FirstPageKey))
            {
                ShowFirstPage();
            }
            else if (Input.GetKeyDown(LastPageKey))
            {
                ShowLastPage();
            }
        }

        void SetupMouseEvents()
        {
            var eventTrigger = this.gameObject.AddComponent<EventTrigger>();

            var pointerEnter = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerEnter,
                callback = new EventTrigger.TriggerEvent()
            };
            pointerEnter.callback.AddListener((eventData) => { mouseIsOverPagedRect = true; OnMouseOver(); });

            var pointerExit = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerExit,
                callback = new EventTrigger.TriggerEvent()
            };
            pointerExit.callback.AddListener((eventData) => { mouseIsOverPagedRect = false; OnMouseExit(); });

            eventTrigger.triggers.Add(pointerEnter);
            eventTrigger.triggers.Add(pointerExit);
        }

        /// <summary>
        /// Shows/Hides pagination and buttons as required
        /// </summary>
        public void UpdateDisplay()
        {
            if (!ShowPagination)
            {
                ToggleTemplateButtons(false);
                ToggleFirstAndLastButtons(false);
                TogglePreviousAndNextButtons(false);
            }
            else
            {
                // hide our templates if we're in game
                if (Application.isPlaying || !ShowButtonTemplatesInEditor)
                {
                    ToggleTemplateButtons(false);
                }
                else if (!Application.isPlaying && ShowButtonTemplatesInEditor)
                {
                    ToggleTemplateButtons(true);
                }

                ToggleFirstAndLastButtons(ShowFirstAndLastButtons);
                TogglePreviousAndNextButtons(ShowPreviousAndNextButtons);
            }

            // Initialise page counts and pagination
            UpdatePages();
        }

        void ToggleTemplateButtons(bool show)
        {
            if (ButtonTemplate_CurrentPage != null) ButtonTemplate_CurrentPage.gameObject.SetActive(show);            
            if (ButtonTemplate_OtherPages != null) ButtonTemplate_OtherPages.gameObject.SetActive(show);
            if (ButtonTemplate_DisabledPage != null) ButtonTemplate_DisabledPage.gameObject.SetActive(show);
        }

        void ToggleFirstAndLastButtons(bool show)
        {
            if (Button_FirstPage != null) Button_FirstPage.gameObject.SetActive(show);
            if (Button_LastPage != null) Button_LastPage.gameObject.SetActive(show);
        }

        void TogglePreviousAndNextButtons(bool show)
        {
            if (Button_NextPage != null) Button_NextPage.gameObject.SetActive(show);
            if (Button_PreviousPage != null) Button_PreviousPage.gameObject.SetActive(show);
        }

        /// <summary>
        /// Set the Current Page of this PagedRect.
        /// </summary>
        /// <param name="newPage"></param>
        /// <param name="initial"></param>
        public virtual void SetCurrentPage(Page newPage, bool initial = false)
        {
            var index = Pages.IndexOf(newPage);
            if (index == -1)
            {
                throw new UnityException("PagedRect.SetCurrentPag(Page newPage) :: The value provided for 'newPage' is not in the collection of pages!");
            }

            SetCurrentPage(index + 1, initial);
        }

        public virtual void SetCurrentPageProxy(int newPage)
        {
            SetCurrentPage(newPage, false);
        }

        /// <summary>
        /// Set the Current Page of this PagedRect (by its position in the hierarchy)
        /// </summary>
        /// <param name="newPage"></param>
        /// <param name="initial"></param>
        public virtual void SetCurrentPage(int newPage, bool initial = false)
        {            
            if (newPage > NumberOfPages)
            {
                throw new UnityException("PagedRect.SetCurrentPage(int newPage) :: The value provided for 'newPage' is greater than the number of pages.");
            }
            else if (newPage <= 0)
            {
                throw new UnityException("PagedRect.SetCurrentPage(int newPage) :: The value provided for 'newPage' is less than zero.");
            }

            if (CurrentPage == newPage && !UsingScrollRect)
            {
                Pages[CurrentPage-1].OnShow();
                return;
            }

            var previousPage = CurrentPage;

            _timeSinceLastPage = 0f;

            CurrentPage = newPage;

            // 1 == 0, 2 == 1, and so forth
            var newPageIndex = newPage - 1;

            if (!UsingScrollRect)
            {
                //Debug.Log("Current Page " + CurrentPage + " New Page " + newPage);
                var direction = CurrentPage < previousPage ? eDirection.Left : eDirection.Right;                

                for (var i = 0; i < NumberOfPages; i++)
                {
                    var page = Pages[i];
                    if (i == newPageIndex)
                    {
                        PageEnterAnimation(page, direction, initial);
                        if (Application.isPlaying) page.OnShow();
                    }
                    else
                    {
                        if (page.gameObject.activeSelf)
                        {
                            if (Application.isPlaying) page.OnHide();
                            PageExitAnimation(page, direction == eDirection.Left ? eDirection.Right : eDirection.Left);
                        }
                    }
                }
            }
            else
            {                
                if (Application.isPlaying)
                {
                    // Using a Scroll Rect means that the ScrollRect itself will handle animation, we just need to trigger OnShow and OnHide events here
                    for (var i = 0; i < NumberOfPages; i++)
                    {
                        var page = Pages[i];
                        if (i == newPageIndex)
                        {
                            page.OnShow();
                        }
                        else
                        {
                            if (page.Visible)
                            {
                                page.OnHide();
                            }
                        }
                    }                    
                }                

                CenterScrollRectOnCurrentPage(initial);  
            }

            UpdatePagination();

            if (PageChangedEvent != null)
            {
                PageChangedEvent.Invoke(GetPageByNumber(CurrentPage), GetPageByNumber(previousPage));
            }
        }

        public void CenterScrollRectOnCurrentPage(bool initial = false)
        {            
            if (NumberOfPages == 0) return;

            //var currentPageTransform = GetCurrentPage().transform;

            /*_lerp_ScrollRectInitialPosition = (Vector2)ScrollRect.content.anchoredPosition;
            _lerp_DesiredScrollRectPosition =  (Vector2)ScrollRect.transform.InverseTransformPoint(ScrollRect.content.position) 
                                             - (Vector2)ScrollRect.transform.InverseTransformPoint(currentPageTransform.position);            */


            if (ScrollRect.horizontal)
            {                
                ScrollRect.content.anchoredPosition = new Vector2(ScrollRect.content.anchoredPosition.x, -ScrollRect.content.sizeDelta.y / 2f);
                _lerp_ScrollRectInitialPosition = ScrollRect.horizontalNormalizedPosition;

                _lerp_DesiredScrollRectPosition = (float)(CurrentPage - 1f) / (float)(NumberOfPages - 1f);
            }
            else
            {
                ScrollRect.content.anchoredPosition = new Vector2(ScrollRect.content.sizeDelta.x / 2f, ScrollRect.content.anchoredPosition.y);
                _lerp_ScrollRectInitialPosition = ScrollRect.verticalNormalizedPosition;

                _lerp_DesiredScrollRectPosition = 1 - (float)(CurrentPage - 1f) / (float)(NumberOfPages - 1f);
            }            

            if (Application.isPlaying && !initial)
            {                
                StartCoroutine(ScrollToDesiredPosition());
            }
            else
            {                
                SetScrollRectNormalizedPosition();
            }
        }

        protected void SetScrollRectNormalizedPosition()
        {
            float calculatedPosition = (float)(CurrentPage - 1f) / (float)(NumberOfPages - 1f);
            if (ScrollRect.horizontal)
            {                
                ScrollRect.horizontalNormalizedPosition = calculatedPosition;
                if (Application.isPlaying)
                {
                    ScrollRect.horizontalNormalizedPosition = calculatedPosition;
                    //DelayedCall(0.01f, () => { ScrollRect.horizontalNormalizedPosition = calculatedPosition; });
                } 
                else 
                {
#if UNITY_EDITOR
                    DelayedEditorAction(0.01, () => { ScrollRect.horizontalNormalizedPosition = calculatedPosition; });
                    DelayedEditorAction(0.02, () => { ScrollRect.horizontalNormalizedPosition = calculatedPosition; });
#endif
                }

            }
            else
            {                
                ScrollRect.verticalNormalizedPosition = 1 - calculatedPosition;
                if (Application.isPlaying)
                {
                    DelayedCall(0.01f, () => { ScrollRect.verticalNormalizedPosition = calculatedPosition; });
                }
                else
                {
#if UNITY_EDITOR
                    DelayedEditorAction(0.01, () => { ScrollRect.verticalNormalizedPosition = calculatedPosition; });
                    DelayedEditorAction(0.02, () => { ScrollRect.verticalNormalizedPosition = calculatedPosition; });
#endif
                }
            }            
        }

        protected IEnumerator ScrollToDesiredPosition()
        {            
            _lerp_timeStartedMoving = Time.time;

            float percentageComplete = 0f;

            while (percentageComplete < 1f)
            {                
                float timeSinceStarted = Time.time - _lerp_timeStartedMoving;
                percentageComplete = timeSinceStarted / (0.25f / AnimationSpeed);
                
                //ScrollRect.content.anchoredPosition = Vector2.Lerp(_lerp_ScrollRectInitialPosition, _lerp_DesiredScrollRectPosition, percentageComplete);

                if (ScrollRect.horizontal)
                {
                    ScrollRect.horizontalNormalizedPosition = Mathf.Lerp(_lerp_ScrollRectInitialPosition, _lerp_DesiredScrollRectPosition, percentageComplete);
                }
                else
                {
                    ScrollRect.verticalNormalizedPosition = Mathf.Lerp(_lerp_ScrollRectInitialPosition, _lerp_DesiredScrollRectPosition, percentageComplete);
                }
                
                yield return new WaitForEndOfFrame();
            }
            
            if (ScrollRect.horizontal)
            {
                ScrollRect.horizontalNormalizedPosition = _lerp_DesiredScrollRectPosition;
            }
            else
            {
                ScrollRect.verticalNormalizedPosition = _lerp_DesiredScrollRectPosition;
            }

            SetScrollRectNormalizedPosition();            
        }

        /// <summary>
        /// Trigger a Page Show animation
        /// </summary>
        /// <param name="page"></param>
        /// <param name="direction"></param>
        /// <param name="initial"></param>
        protected void PageEnterAnimation(Page page, eDirection direction, bool initial = false)
        {
            if (!Application.isPlaying || AnimationType == eAnimationType.None || initial)
            {
                page.gameObject.SetActive(true);
            }
            else
            {
                var animationType = page.UsePageAnimationType ? page.AnimationType : AnimationType;
                switch (animationType)
                {                    
                    case eAnimationType.Fade:
                        {
                            //StartCoroutine(DelayedCall(0.5f, () => page.FadeIn()));
                            page.FadeIn();
                        }
                        break;

                    case eAnimationType.SlideHorizontal:
                    case eAnimationType.SlideVertical:
                        {
                            if (page.FlipAnimationDirection)
                            {
                                direction = (direction == eDirection.Left) ? eDirection.Right : eDirection.Left;
                            }

                            page.SlideIn(direction, animationType == eAnimationType.SlideVertical);
                        }
                        break;
                }                
            }
        }

        /// <summary>
        /// Trigger a Page Exit animation
        /// </summary>
        /// <param name="page"></param>
        /// <param name="direction"></param>
        protected void PageExitAnimation(Page page, eDirection direction)
        {
            if (!Application.isPlaying || AnimationType == eAnimationType.None)
            {
                page.gameObject.SetActive(false);
            }
            else
            {
                var animationType = page.UsePageAnimationType ? page.AnimationType : AnimationType;
                switch (animationType)
                {                    
                    case eAnimationType.Fade:
                        {
                            page.FadeOut();                            
                        }
                        break;

                    case eAnimationType.SlideHorizontal:
                    case eAnimationType.SlideVertical:
                        {
                            if (page.FlipAnimationDirection)
                            {
                                direction = (direction == eDirection.Left) ? eDirection.Right : eDirection.Left;
                            }

                            page.SlideOut(direction, animationType == eAnimationType.SlideVertical);
                        }
                        break;
                }                
            }
        }

        /// <summary>
        /// Show the next enabled Page (if there is one)
        /// </summary>
        public virtual void NextPage()
        {
            if (CurrentPage == NumberOfPages)
            {
                if (LoopEndlessly) ShowFirstPage();
                return;
            }

            var nextEnabledPage = Pages.Where(p => (Pages.IndexOf(p) + 1) > CurrentPage)
                                       .FirstOrDefault(p => p.PageEnabled);

            if (nextEnabledPage != null)
            {
                SetCurrentPage(nextEnabledPage);
            }            
        }

        /// <summary>
        /// Show the previous enabled Page (if there is one)
        /// </summary>
        public virtual void PreviousPage()
        {
            if (CurrentPage == 1)
            {
                if (LoopEndlessly) ShowLastPage();
                return;
            }

            var prevEnablePage = Pages.Where(p => (Pages.IndexOf(p) + 1) < CurrentPage)
                                      .Last(p => p.PageEnabled);

            if (prevEnablePage != null)
            {
                SetCurrentPage(prevEnablePage);
            }                                      
        }

        /// <summary>
        /// Show the first enabled page
        /// </summary>
        public virtual void ShowFirstPage()
        {
            var firstEnabledPage = Pages.FirstOrDefault(p => p.PageEnabled);

            if (firstEnabledPage != null)
            {
                SetCurrentPage(firstEnabledPage);
            }
        }

        /// <summary>
        /// Show the last enabled page
        /// </summary>
        public virtual void ShowLastPage()
        {
            var lastEnabledPage = Pages.LastOrDefault(p => p.PageEnabled);

            if (lastEnabledPage != null)
            {
                SetCurrentPage(lastEnabledPage);
            }
        }

        /// <summary>
        /// Call this function at any time to update the page collection.
        /// </summary>
        public void UpdatePages(bool force = false)
        {
            if (!AutoDiscoverPages) return;

            if (this == null) return;

            if (force) this.isDirty = true;

            var tempPages = Viewport.GetComponentsInChildren<Page>(true).Where(p => p.ShowOnPagination && p.transform.parent == Viewport.transform).ToList();
            
            // avoid an unnecessary initial update (_pages is empty when we initialise)
            if (!_pages.Any())
            {
                _pages = tempPages;
            }
            else
            {
                this.isDirty = this.isDirty || !tempPages.SequenceEqual<Page>(_pages);
            }
            
            Pages = _pages = tempPages;            
            Pages.ForEach(p => { if (!p.Initialised) p.Initialise(this); });

            // see if there are any changes
            if (!this.isDirty)
            {
                return;
            }            

            if (CurrentPage > NumberOfPages && NumberOfPages > 0)
            {
                SetCurrentPage(Math.Max(NumberOfPages - 1, 0));
            }
            
            UpdatePagination();            
        }

        public Page GetPageByNumber(int pageNumber)
        {
            if (pageNumber <= 0 || pageNumber > NumberOfPages) return null;

            return Pages[pageNumber - 1];
        }

        public Page GetCurrentPage()
        {
            return GetPageByNumber(CurrentPage);
        }

        public int GetPageNumber(Page page)
        {
            return Pages.IndexOf(page) + 1;
        }

        /// <summary>
        /// Updates the page buttons
        /// </summary>
        public void UpdatePagination()
        {            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {                
                // If we're not active and in the game, don't do this
                if (!this.gameObject.activeInHierarchy)
                {
                    return;
                }

                // If we're not dirty, then don't do this
                if (!this.isDirty)
                {
                    return;
                }
            }            
#endif
            var paginationButtons = GetComponentsInChildren<PaginationButton>(true)
                                    .Where(pb => !pb.DontUpdate)    // don't select buttons that we shouldn't be adjusting (e.g. templates, first/last, next/previous)
                                    .Where(pb => pb != ButtonTemplate_CurrentPage && pb != ButtonTemplate_OtherPages) // just in case we accidentally clear the DontUpdate field on the templates
                                    .ToList();

            paginationButtons.ForEach(pb => 
                {
                    if (Application.isPlaying)
                    {                        
                        Destroy(pb.gameObject);
                    }
                    else 
                    { 
                        // Destroy doesn't work in edit mode
                        DestroyImmediate(pb.Text.gameObject);
                        DestroyImmediate(pb.gameObject);
                    }
                });
            
            if (!ShowPagination || !ShowPageButtons)
            {                
                return;
            }
            
            var startingPoint = 0;
            var endingPoint = NumberOfPages - 1;

            if (MaximumNumberOfButtonsToShow != 0)
            {
                if (MaximumNumberOfButtonsToShow == 1)
                {
                    startingPoint = CurrentPage - 1;
                    endingPoint = CurrentPage - 1;
                }
                else if (NumberOfPages > MaximumNumberOfButtonsToShow)
                {
                    var halfOfMax = (int)Math.Floor(MaximumNumberOfButtonsToShow / 2f);

                    var distanceFromStart = CurrentPage - startingPoint;
                    var distanceFromEnd = endingPoint - CurrentPage;

                    if (distanceFromStart <= distanceFromEnd)
                    {
                        if (CurrentPage > halfOfMax)
                        {
                            startingPoint = CurrentPage - halfOfMax;
                        }

                        endingPoint = Math.Min(endingPoint, startingPoint + MaximumNumberOfButtonsToShow - 1);
                    }
                    else
                    {
                        if (distanceFromEnd > halfOfMax)
                        {
                            endingPoint = CurrentPage + halfOfMax;
                        }

                        startingPoint = Math.Max(startingPoint, endingPoint - MaximumNumberOfButtonsToShow + 1);
                    }
                }
            }

            for (int i = startingPoint; i <= endingPoint; i++)
            {
                var pageNumber = i + 1;
                var page = Pages[i];
                
                var template = (pageNumber == CurrentPage) ? ButtonTemplate_CurrentPage : ButtonTemplate_OtherPages;
                if (!page.PageEnabled && ButtonTemplate_DisabledPage != null)
                {
                    template = ButtonTemplate_DisabledPage;
                }

                var button = Instantiate(template) as PaginationButton;

                if (page.PageEnabled)
                {                                        
                    // Add the onClick listener
                    button.Button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => { this.SetCurrentPage((pageNumber)); }));
                }

                // Position the button
                button.gameObject.transform.SetParent(Pagination.transform, false);

                var buttonText = "";
                if (ShowNumbersOnButtons)
                {
                    buttonText = pageNumber.ToString();
                    if (ShowPageTitlesOnButtons)
                    {
                        buttonText += ". ";
                    }
                }

                if (ShowPageTitlesOnButtons)
                {
                    buttonText += page.PageTitle;
                }

                button.SetText(buttonText);

                button.gameObject.name = String.Format("Button - Page {0} {1}", pageNumber, pageNumber == CurrentPage ? "(Current Page)" : "");

                // DO update this button
                button.DontUpdate = false;

                // Activate the button if need be (the template is usually disabled at this point)
                button.gameObject.SetActive(true);
            }       
            
            // ensure our other buttons are in the right places
            Button_PreviousPage.gameObject.transform.SetAsFirstSibling();
            Button_FirstPage.gameObject.transform.SetAsFirstSibling();
            Button_NextPage.gameObject.transform.SetAsLastSibling();
            Button_LastPage.gameObject.transform.SetAsLastSibling();
        }

        /// <summary>
        /// Add a new page to this PagedRect - this Page should already have been instantiated.
        /// </summary>
        /// <param name="page"></param>
        public void AddPage(Page page)
        {
            page.transform.SetParent(Viewport.transform);
            page.transform.localPosition = Vector3.zero;
            
            // If we don't set these values, Unity seems to set them to non-zero values and our new page shows up off-screen
            var rectTransform = (RectTransform)page.transform;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            page.ShowOnPagination = true;

            this.isDirty = true;

            UpdateDisplay();

            if (UsingScrollRect)
            {
                StartCoroutine(DelayedCall(0.1f, () => CenterScrollRectOnCurrentPage(true)));
            }
        }

        /// <summary>
        /// Add a new page to this PagedRect - this Page will be instantiated and returned by this function. You can then customize it as required.
        /// </summary>
        public Page AddPageUsingTemplate()
        {
            if (NewPageTemplate == null)
            {
                throw new UnityException("Attempted to use PagedRect.AddPageUsingTemplate(), but this PagedRect instance has no NewPageTemplate set!");
            }

            var page = Instantiate(NewPageTemplate) as Page;
            this.AddPage(page);

            // this will always be the last page
            page.name = "Page " + this.NumberOfPages;            

            return page;
        }

        /// <summary>
        /// Remove a Page from this PagedRect, and optionally destroy it
        /// </summary>
        /// <param name="page"></param>
        /// <param name="destroyPageObject"></param>
        public void RemovePage(Page page, bool destroyPageObject = false)
        {
            if (Pages.Contains(page))
            {
                page.ShowOnPagination = false;
                Pages.Remove(page);

                if (destroyPageObject)
                {
                    Destroy(page.gameObject);
                }

                this.isDirty = true;
                this.UpdatePages();
            }
        }

        public void RemoveAllPages(bool destroyPageObjects = false)
        {
            var pages = this.Pages.ToList();
            foreach (var page in pages)
            {
                this.RemovePage(page, destroyPageObjects);
            }
        }

        /// <summary>
        /// Used by the Editor to keep track of the selected page in edit mode
        /// </summary>
        /// <param name="page"></param>
        public void SetEditorSelectedPage(int page)
        {
            editorSelectedPage = page;
        }       

        /// <summary>
        /// Call a function after the specified delay.
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="call"></param>
        /// <returns></returns>
        public System.Collections.IEnumerator DelayedCall(float delay, Action call)
        {
            yield return new WaitForSeconds(delay);

            call.Invoke();
        }

        protected void OnMouseOver()
        {
            if (HighlightWhenMouseIsOver)
            {
                ShowHighlight();
            }
        }

        protected void OnMouseExit()
        {
            if (HighlightWhenMouseIsOver)
            {
                ClearHighlight();
            }
        }

        protected void ShowHighlight()
        {
            imageComponent.color = HighlightColor;
        }

        protected void ClearHighlight()
        {
            imageComponent.color = NormalColor;             
        }

        protected void ScrollRectValueChanged(Vector2 newPosition)
        {
            _ScrollRectPosition = newPosition;
        }

        public void OnEndDrag(PointerEventData data)
        {
            if (!UsingScrollRect) return;
            
            bool goToNextPage = false;
            bool goToPreviousPage = false;
            if (ScrollRect.horizontal)
            {                
                if (data.delta.x > SwipeDeltaThreshold)
                {
                    goToPreviousPage = true;
                }
                else if (data.delta.x < -SwipeDeltaThreshold)
                {
                    goToNextPage = true;
                }
                else
                {
                    goToPreviousPage = _ScrollRectPosition.x < 0f;
                    goToNextPage = _ScrollRectPosition.x > 1f;
                }                
            }
            else if (ScrollRect.vertical)
            {
                if (data.delta.y > SwipeDeltaThreshold)
                {
                    goToPreviousPage = true;
                }
                else if (data.delta.y < -SwipeDeltaThreshold)
                {
                    goToNextPage = true;
                }
                else
                {
                    goToPreviousPage = _ScrollRectPosition.y < 0f;
                    goToNextPage = _ScrollRectPosition.y > 1f;
                }
            }

            if (goToNextPage) NextPage();
            if (goToPreviousPage) PreviousPage();

            if (goToNextPage || goToPreviousPage) return;

            // If we dragged less than the delta threshold, we may still be between pages - the following code will either return us to our previous page,
            // or take us to the next/previous if they are closer
            var centerOfScrollRect = ((RectTransform)ScrollRect.transform).position;

            var x = 1;
            Dictionary<int, float> pageDistances = new Dictionary<int, float>();
            foreach (var page in Pages)
            {
                if (!page.enabled) continue;

                var distanceToCenterOfScrollRect = Vector2.Distance((Vector2)centerOfScrollRect, (Vector2)((RectTransform)page.transform).position);
                pageDistances.Add(x++, distanceToCenterOfScrollRect);
            }

            if (pageDistances.Any())
            {                
                var closestPage = pageDistances.OrderBy(p => p.Value).First().Key;

                SetCurrentPage(closestPage);
            }
        }

        public void ViewportDimensionsChanged()
        {
            Pages.ForEach(p => p.UpdateDimensions());

            if (UsingScrollRect)
            {
                if (Application.isPlaying)
                {
                    StartCoroutine(DelayedCall(0.05f, () => { CenterScrollRectOnCurrentPage(true); }));
                }
                else
                {
                    SetScrollRectNormalizedPosition();
                }
            }
        }

#if UNITY_EDITOR
        protected void DelayedEditorAction(double delay, Action action)
        {
            delayedEditorActions.Add(new KeyValuePair<double, Action>(UnityEditor.EditorApplication.timeSinceStartup + delay, action));
        }
#endif

        #region Exposed Variables For Events
        /// <summary>
        /// Show or hide the First and Last Buttons
        /// </summary>
        /// <param name="show"></param>
        public void SetShowFirstAndLastButtons(bool show)
        {
            ShowFirstAndLastButtons = show;
            ToggleFirstAndLastButtons(show);
        }

        /// <summary>
        /// Show or Hide the Previous and Next Buttons
        /// </summary>
        /// <param name="show"></param>
        public void SetShowPreviousAndNextButtons(bool show)
        {
            ShowPreviousAndNextButtons = show;
            TogglePreviousAndNextButtons(show);
        }

        /// <summary>
        /// Set the animation speed for this PagedRect
        /// </summary>
        /// <param name="animationSpeed"></param>
        public void SetAnimationSpeed(float animationSpeed)
        {
            AnimationSpeed = animationSpeed;
        }

        /// <summary>
        /// Set the Animation type of this PagedRect
        /// </summary>
        /// <param name="animationType"></param>
        public void SetAnimationType(string animationType)
        {
            AnimationType = (eAnimationType)Enum.Parse(typeof(eAnimationType), animationType);
            Pages.ForEach(p => p.ResetPositionAndAlpha());
        }

        /// <summary>
        /// Set the delay between pages (when automatically scrolling through pages)
        /// </summary>
        /// <param name="delay"></param>
        public void SetDelayBetweenPages(float delay)
        {
            DelayBetweenPages = delay;
        }

        /// <summary>
        /// Enable or Disable Endless Looping
        /// </summary>
        /// <param name="loop"></param>
        public void SetLoopEndlessly(bool loop)
        {
            LoopEndlessly = loop;
        }

        /// <summary>
        /// Enable or Disable automatically moving to the next page
        /// </summary>
        /// <param name="move"></param>
        public void SetAutomaticallyMoveToNextPage(bool move)
        {
            AutomaticallyMoveToNextPage = move;
            _timeSinceLastPage = 0f;
        }

        /// <summary>
        /// Show or Hide page numbers on Buttons
        /// </summary>
        /// <param name="show"></param>
        public void SetShowPageNumbersOnButtons(bool show)
        {
            ShowNumbersOnButtons = show;
            UpdatePagination();
        }

        /// <summary>
        /// Show or Hide page titles on Buttons (page titles are sourced from Page.PageTitle)
        /// </summary>
        /// <param name="show"></param>
        public void SetShowPageTitlesOnButtons(bool show)
        {
            ShowPageTitlesOnButtons = show;
            UpdatePagination();
        }

        /// <summary>
        /// Set the maximum number of buttons to show at once
        /// </summary>
        /// <param name="maxNumber"></param>
        public void SetMaximumNumberOfButtonsToShow(int maxNumber)
        {
            MaximumNumberOfButtonsToShow = maxNumber;
            UpdatePagination();
        }

        /// <summary>
        /// Float version of SetMaximumNumberOfButtonsToShow so we can accept input from a UI slider.
        /// </summary>
        /// <param name="maxNumber"></param>
        public void SetMaximumNumberOfButtonsToShow(float maxNumber)
        {
            SetMaximumNumberOfButtonsToShow((int)maxNumber);
        }

        /// <summary>
        /// Enable or disable keyboard input
        /// </summary>
        /// <param name="useInput"></param>
        public void SetUseKeyboardInput(bool useInput)
        {
            UseKeyboardInput = useInput;
        }

        /// <summary>
        /// Enable or disable Swipe input
        /// Please be advised that if this PagedRect is using a ScrollRect, this value will be ignored
        /// </summary>
        /// <param name="useInput"></param>
        public void SetUseSwipeInput(bool useInput)
        {
            if (UsingScrollRect) useInput = false;

            MobileInput.enabled = useInput;
        }

        /// <summary>
        /// Enable or disable scroll wheel input
        /// </summary>
        /// <param name="useInput"></param>
        public void SetUseScrollWheelInput(bool useInput)
        {
            UseScrollWheelInput = useInput;
            ScrollWheelInput.enabled = useInput;
        }

        public void SetOnlyUseScrollWheelInputOnlyWhenMouseIsOver(bool onlyWhenMouseIsOver)
        {
            OnlyUseScrollWheelInputWhenMouseIsOver = onlyWhenMouseIsOver;
        }

        public void SetHighlightWhenMouseIsOver(bool highlight)
        {
            HighlightWhenMouseIsOver = highlight;

            if (!highlight)
            {
                ClearHighlight();
            }
            else
            {
                if (mouseIsOverPagedRect)
                {
                    ShowHighlight();
                }
            }
        }

        public void SetSwipeDeltaThreshold(float threshold)
        {
            SwipeDeltaThreshold = threshold;
        }
        #endregion

        #region Menu Items
#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/UI/Pagination/Horizontal Pagination")]
        static void AddHorizontalPaginationPrefab()
        {
            PaginationUtilities.InstantiatePrefab("HorizontalPagination");
        }        

        [UnityEditor.MenuItem("GameObject/UI/Pagination/Vertical Pagination")]
        static void AddVerticalPaginationPrefab()
        {
            PaginationUtilities.InstantiatePrefab("VerticalPagination");    
        }

        [UnityEditor.MenuItem("GameObject/UI/Pagination/Slider")]
        static void AddSliderPrefab()
        {
            PaginationUtilities.InstantiatePrefab("Slider");
        }

        [UnityEditor.MenuItem("GameObject/UI/Pagination/Horizontal Pagination - ScrollRect")]
        static void AddHorizontalPaginationScrollRectPrefab()
        {
            PaginationUtilities.InstantiatePrefab("HorizontalPagination - ScrollRect");
        }

        [UnityEditor.MenuItem("GameObject/UI/Pagination/Vertical Pagination - ScrollRect")]
        static void AddVerticalPaginationScrollRectPrefab()
        {
            PaginationUtilities.InstantiatePrefab("VerticalPagination - ScrollRect");
        }

        [UnityEditor.MenuItem("GameObject/UI/Pagination/Slider - ScrollRect")]
        static void AddSliderScrollRectPrefab()
        {
            PaginationUtilities.InstantiatePrefab("Slider - ScrollRect");
        }
#endif
        #endregion
    }
}
