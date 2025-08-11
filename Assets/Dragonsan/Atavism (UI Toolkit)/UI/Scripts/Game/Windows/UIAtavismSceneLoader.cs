using UnityEngine;
// using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
// using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
#if AT_STREAMER2
using WorldStreamer2;
#endif

namespace Atavism.UI
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(UIDocument))]
    public class UIAtavismSceneLoader : MonoBehaviour
    {
        [SerializeField] protected UIDocument uiDocument;
        [Header("Settings")]
        public SceneSkipType SkipType = SceneSkipType.Button;
        [Range(0.5f, 7)] public float SceneSmoothLoad = 3;
        [Range(0.5f, 7)] public float FadeInSpeed = 2;
        [Range(0.5f, 7)] public float FadeOutSpeed = 2;
        public bool useTimeScale = false;
        public float minTeleportDistanceToShow = 15f;
        [Header("Background")]
        public bool useBackgrounds = true;
        public bool ShowDescription = true;
        [Range(1, 60)] public float TimePerBackground = 5;
        [Range(0.5f, 7)] public float FadeBackgroundSpeed = 2;
        [Range(0.5f, 5)] public float TimeBetweenBackground = 0.5f;
        [Header("Tips")]
        public bool RandomTips = false;
        [Range(1, 60)] public float TimePerTip = 5;
        [Range(0.5f, 5)] public float FadeTipsSpeed = 2;
        [Header("Loading")]
        public bool FadeLoadingBarOnFinish = false;
        [Range(50, 1000)] public float LoadingCircleSpeed = 300;
        [TextArea(2, 2)] public string LoadingTextFormat = "{0}";

        [Header("Audio")]
        [Range(0.1f, 1)] public float AudioVolume = 1f;
        [Range(0.5f, 5)] public float FadeAudioSpeed = 0.5f;
        [Range(0.1f, 1)] public float FinishSoundVolume = 0.5f;
        [SerializeField] private AudioClip FinishSound = null;
        [SerializeField] private AudioClip BackgroundAudio = null;

        [Header("References")]
        [SerializeField] private Label SceneNameText = null;
        [SerializeField] private Label DescriptionText = null;
        [SerializeField] private Label ProgressText = null;
        [SerializeField] private Label TipText = null;
        [SerializeField] private VisualElement BackgroundImage = null;//image
        [SerializeField] private UIProgressBar FilledImage = null;//image
        [SerializeField] private Slider LoadBarSlider = null;
        [SerializeField] private VisualElement ContinueUI = null;//GameObject
        [SerializeField] private VisualElement RootUI;//GameObject
        [SerializeField] private VisualElement FlashImage = null;//GameObject
        [SerializeField] private VisualElement SkipKeyText = null;//GameObject
        [SerializeField] private VisualElement LoadingCircle = null;//RectTransform
        [SerializeField] private VisualElement LoadingCircleBackground = null;//CanvasGroup
        [SerializeField] private VisualElement FadeImage = null;//CanvasGroup
        protected Button ContinueButton;
        private bl_SceneLoaderManager Manager = null;
        private AsyncOperation async;
        private bool isOperationStarted = false;
        private bool FinishLoad = false;
        private bool isTipFadeOut = false;
        private int CurrentTip = 0;
        private TheTipList cacheTips = null;
        private int CurrentBackground = 0;
        private List<Sprite> cacheBackgrounds = new List<Sprite>();
        private AudioSource Source = null;
        private float lerpValue = 0;
        private bool canSkipWithKey = false;
        private bl_SceneLoaderInfo CurrentLoadLevel = null;
        
        
        protected bool isRegisteredUI, isInitialize;
#if AT_STREAMER || AT_STREAMER2
        [SerializeField] private List<Streamer> streamers;//Dragonsan
#endif
        bool load_leval = false;
        float fillAmount = 0f;
        public UnityEvent onDone;
        bool teleport = false;
        bool loadedMainScene = false;
        private bool prevSceneLoaded = false;
        private string prevSceneName = "";
        #region Initiate
        protected virtual void Reset()
        {
            uiDocument = GetComponent<UIDocument>();
        }
        protected virtual void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();


            Initialize();
            RootUI.HideVisualElement();
        }
        public virtual void Initialize()
        {
            if (isInitialize)
                return;

            registerUI();
            registerEvents();
            registerExtensionMessages();

            isInitialize = true;
        }

        public virtual void Deinitialize()
        {
            if (!isInitialize)
                return;

            unregisterExtensionMessages();
            unregisterEvents();
            unregisterUI();

            isInitialize = false;
        }
          protected virtual bool registerUI()
        {
            if (isRegisteredUI)
                return false;

            uiDocument.enabled = true;
            
            SceneNameText = uiDocument.rootVisualElement.Query<Label>("SceneNameText");
            DescriptionText = uiDocument.rootVisualElement.Query<Label>("DescriptionText");
            ProgressText = uiDocument.rootVisualElement.Query<Label>("LoadingText");
            TipText = uiDocument.rootVisualElement.Query<Label>("TipsText");

            BackgroundImage = uiDocument.rootVisualElement.Query<VisualElement>("BackgroundImage");
            FilledImage = uiDocument.rootVisualElement.Query<UIProgressBar>("BarSlider");
            LoadBarSlider = uiDocument.rootVisualElement.Query<Slider>("LoadBarSlider");
            
            ContinueUI = uiDocument.rootVisualElement.Query<VisualElement>("ContinueUI");
            RootUI = uiDocument.rootVisualElement.Query<VisualElement>("LoaderRoot");
            FlashImage = uiDocument.rootVisualElement.Query<VisualElement>("FlashImage");
            FadeImage = uiDocument.rootVisualElement.Query<VisualElement>("FadeImage");
            SkipKeyText = uiDocument.rootVisualElement.Query<VisualElement>("SkipKeyText");
            //LoadingCircle
            
            LoadingCircleBackground = uiDocument.rootVisualElement.Query<VisualElement>("LoadingCircleBackground");
            LoadingCircle = uiDocument.rootVisualElement.Query<VisualElement>("LoadingCircle");
            ContinueButton = uiDocument.rootVisualElement.Query<Button>("ContinueButton");
            if (ContinueButton != null)
                ContinueButton.clicked += LoadNextScene;

            // // Check if any of the UI elements are null and log an error message if they are
         /*   if (SceneNameText == null)
                Debug.LogError("UI SceneNameText element not found.");
            if (DescriptionText == null)
                Debug.LogError("UI DescriptionText element not found.");
            if (ProgressText == null)
                Debug.LogError("UI ProgressText element not found.");
            if (TipText == null)
                Debug.LogError("UI TipText element not found.");
            if (BackgroundImage == null)
                Debug.LogError("UI BackgroundImage element not found.");
            if (FilledImage == null)
                Debug.LogError("UI FilledImage element not found.");
            if (LoadBarSlider == null)
                Debug.LogError("UI LoadBarSlider element not found.");
            if (ContinueUI == null)
                Debug.LogError("UI ContinueUI element not found.");
            if (RootUI == null)
                Debug.LogError("UI RootUI element not found.");
            if (FlashImage == null)
                Debug.LogError("UI FlashImage element not found.");
            if (SkipKeyText == null)
                Debug.LogError("UI SkipKeyText element not found.");
         */
            if (LoadingCircleBackground == null)
                Debug.LogError("UI LoadingCircleBackground element not found.");
            if (LoadingCircle == null)
                Debug.LogError("UI LoadingCircle element not found.");
            // if (ContinueButton == null)
                // Debug.LogError("UI ContinueButton element not found.");
         
            // if (UIAtavismAudioManager.Instance != null)
            //     UIAtavismAudioManager.Instance.RegisterSFX(uiDocument);

            isRegisteredUI = true;

            return true;
        }

        protected virtual bool unregisterUI()
        {
            if (!isRegisteredUI)
                return false;

            if (ContinueButton != null)
                ContinueButton.clicked -= LoadNextScene;

            // if (UIAtavismAudioManager.Instance != null)
            //     UIAtavismAudioManager.Instance.UnregisterSFX(uiDocument);

            isRegisteredUI = false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void registerEvents()
        {
        }
        protected virtual void unregisterEvents()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void registerPropertyChangedHandlers()
        {
        }
        protected virtual void unregisterPropertyChangedHandlers()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void registerExtensionMessages()
        {
        }
        protected virtual void unregisterExtensionMessages()
        {
        }
        #endregion
        void Start()
        {
#if AT_STREAMER || AT_STREAMER2
            streamers = new List<Streamer>();
#endif
            // Atavism Link
            AtavismClient.Instance.LoadLevelAsync = LoadLevel;
            // if (LoadBarSlider != null)
            // {
            //     LoadingBarAlpha = LoadBarSlider.GetComponent<CanvasGroup>();
            // }
            // if (BackgroundImage != null)
            // {
            //     BackgroundAlpha = BackgroundImage.GetComponent<CanvasGroup>();
            // }

            AtavismEventSystem.RegisterEvent("PLAYER_TELEPORTED", this);
            if (ProgressText != null)
                ProgressText.text = "0";
           
        }
        private void OnDestroy()
        {
            AtavismEventSystem.UnregisterEvent("PLAYER_TELEPORTED", this);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
          //  Debug.LogWarning("OnSceneLoaded: Scene Loaded " + scene.name);
          // Debug.LogWarning("OnSceneLoaded: Scene Loaded " + scene.name+" "+sceneNameInLoading+" | "+loadedMainScene);
            if (sceneNameInLoading.Length > 0)
            {
                // Debug.LogWarning("OnSceneLoaded: Scene Loaded " + scene.name+" "+sceneNameInLoading+" ||");
                if (scene.name.Equals(prevSceneName))
                {
                    prevSceneLoaded = true;
                }
                if (scene.name.Equals(sceneNameInLoading))
                {
                    // Debug.LogWarning("OnSceneLoaded: Scene Loaded " + scene.name+" "+sceneNameInLoading);
                    loadedMainScene = true;
                }
            }
            // Debug.LogWarning("OnSceneLoaded: Scene Loaded " + scene.name+" loadedMainScene="+loadedMainScene+" ");

        }



        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            Manager = bl_SceneLoaderUtils.GetSceneLoaderManager();
            SceneManager.sceneLoaded += OnSceneLoaded;
            //Setup Audio Source
            Source = GetComponent<AudioSource>();
            Source.volume = 0;
            Source.loop = true;
            if (BackgroundAudio != null)
            {
                Source.clip = BackgroundAudio;
            }

            //Setup UI
            RootUI.HideVisualElement();
            if (ContinueUI != null)
            {
                ContinueUI.HideVisualElement();
            }
            if (FlashImage != null)
            {
                FlashImage.HideVisualElement();
            }
            if (FadeImage != null)
            {
                FadeImage.style.opacity = new StyleFloat(1);
                StartCoroutine(FadeOutCanvas(FadeImage));
            }
            if (SkipKeyText != null)
            {
                SkipKeyText.HideVisualElement();
            }
            if (LoadBarSlider != null)
            {
                LoadBarSlider.value = 0;
            }
            if (Manager.HasTips)
            {
                cacheTips = Manager.TipList;
            }
            if (FilledImage != null)
            {
                FilledImage.value = 0;
            }
            transform.SetAsLastSibling();
        }

        /// <summary>
        /// 
        /// </summary>
        void Update()
        {
            if (!isOperationStarted)
                return;
            if (teleport)
                LoadingRotator();
            if (async == null)
                return;

            UpdateUI();
            LoadingRotator();
            SkipWithKey();
        }

        /// <summary>
        /// 
        /// </summary>
        void SkipWithKey()
        {
            if (!canSkipWithKey)
                return;

            if (Input.anyKeyDown)
            {
                LoadNextScene();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        void UpdateUI()
        {
            if (CurrentLoadLevel.LoadingType == LoadingType.Async)
            {
                //Get progress of load level
                float Extra = (GetSkipType == SceneSkipType.InstantComplete) ? 0 : 0.1f;
                float p = (async.progress + Extra); //Fix problem of 90%
                lerpValue = Mathf.Lerp(lerpValue, p, DeltaTime * SceneSmoothLoad);
                if (async.isDone || lerpValue > 0.99f)
                {
                    //Called one time what is inside in this function.
                    if (!FinishLoad)
                    {
                        OnFinish();
                    }
                }
            }
            else
            {
                if (lerpValue >= 1)
                {
                    //Called one time what is inside in this function.
                    if (!FinishLoad)
                    {
                        OnFinish();
                    }
                }
            }
            if (FilledImage != null)
            {
                FilledImage.value = lerpValue;
            }
            if (LoadBarSlider != null)
            {
                LoadBarSlider.value = lerpValue;
            }
            if (ProgressText != null)
            {
                string percent = (lerpValue * 100).ToString("F0");
                ProgressText.text = string.Format(LoadingTextFormat, percent);
            }
           
        }

        /// <summary>
        /// 
        /// </summary>
        void LoadingRotator()
        {
            // Debug.LogError("LoadingRotator");
            if (LoadingCircle == null)
                return;
            // Debug.LogError("LoadingRotator |");
            var rot = LoadingCircle.style.rotate.value; 
                rot.angle = rot.angle.value + DeltaTime * LoadingCircleSpeed;
                LoadingCircle.style.rotate = rot;
               
        }

        /// <summary>
        /// 
        /// </summary>
        void OnFinish()
        {
            FinishLoad = true;
            if (FlashImage != null)
            {
                FlashImage.ShowVisualElement();
            }
            //Can skip when next level is loaded.
            if (GetSkipType == SceneSkipType.Button)
            {
                if (ContinueUI != null)
                {
                    ContinueUI.ShowVisualElement();
                }
                if (FinishSound != null)
                {
                    AudioSource.PlayClipAtPoint(FinishSound, transform.position, FinishSoundVolume);
                }
                if (LoadingCircleBackground != null)
                {
                    StartCoroutine(FadeOutCanvas(LoadingCircleBackground, 0.5f));
                }
                if (LoadBarSlider != null && FadeLoadingBarOnFinish)
                {
                    StartCoroutine(FadeOutCanvas(LoadBarSlider, 1));
                }

            }
            else if (GetSkipType == SceneSkipType.Instant)
            {
                LoadNextScene();
            }
            else if (GetSkipType == SceneSkipType.InstantComplete)
            {
                LoadNextScene();
            }
            else if (GetSkipType == SceneSkipType.AnyKey)
            {
                canSkipWithKey = true;
                if (SkipKeyText != null)
                {
                    SkipKeyText.ShowVisualElement();
                }
                if (FinishSound != null)
                {
                    AudioSource.PlayClipAtPoint(FinishSound, transform.position, FinishSoundVolume);
                }
                if (LoadingCircleBackground != null)
                {
                    StartCoroutine(FadeOutCanvas(LoadingCircleBackground, 0.5f));
                }
                if (LoadBarSlider != null && FadeLoadingBarOnFinish)
                {
                    StartCoroutine(FadeOutCanvas(LoadBarSlider, 1));
                }

            }

        }
        string sceneNameInLoading = "";
        
        private IEnumerator LoadSceneDelay(string sceneName)
        {
            WaitForSeconds delay =  new WaitForSeconds(1f);
            while (!loadedMainScene)
            {
                // Debug.LogError("LoadSceneDelay loadedMainScene="+loadedMainScene);
                yield return delay;
            }

            async = null;
            LoadLevel(sceneName);
        }
        
        
        
        /// <summary>
        /// 
        /// </summary>
        public void LoadLevel(string level)
        {
          //  Debug.LogError("LoadLevel level=" + level);
            if (async != null && !async.isDone)
            {
                StartCoroutine(LoadSceneDelay(level));
                return;
            }

            uiDocument.sortingOrder = 1000;
            StopAllCoroutines();
            sceneNameInLoading = level;
            prevSceneLoaded = false;
            load_leval = true;
            FinishLoad = false;
            teleport = false;
            loadedMainScene = false;
         //   Debug.LogError("LoadLevel level="+level+" loadedMainScene="+loadedMainScene+" to false");
            if (LoadBarSlider != null)
            {
                LoadBarSlider.value = 0;
            }
            if (ProgressText != null)
            {
                ProgressText.text = string.Format(LoadingTextFormat, 0);
            }
            if (FilledImage != null)
            {
                // FilledImage.type = Image.Type.Filled;
                FilledImage.value = 0f;
            }
            StartCoroutine(FadeOutCanvas(FadeImage));
            if (LoadingCircleBackground != null)
            {
                StartCoroutine(FadeInCanvas(LoadingCircleBackground, 0.1f));
            }
            CurrentLoadLevel = Manager.GetSceneInfo(level);
            if (CurrentLoadLevel == null)
                return;

            SetupUI(CurrentLoadLevel);
            StartCoroutine(StartAsyncOperation(CurrentLoadLevel.SceneName));
            if (CurrentLoadLevel.LoadingType == LoadingType.Fake)
            {
                StartCoroutine(StartFakeLoading());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        void SetupUI(bl_SceneLoaderInfo info)
        {
            if (BackgroundImage != null && useBackgrounds)
            {
                if (info.Backgrounds != null && info.Backgrounds.Length > 1)
                {
                    cacheBackgrounds.AddRange(info.Backgrounds);
                    for (int i = 0; i < cacheBackgrounds.Count; i++)
                    {
                        Sprite temp = cacheBackgrounds[i];
                        int randomIndex = Random.Range(i, cacheBackgrounds.Count);
                        cacheBackgrounds[i] = cacheBackgrounds[randomIndex];
                        cacheBackgrounds[randomIndex] = temp;
                    }
                    StartCoroutine(BackgroundTransition());
                    BackgroundImage.style.unityBackgroundImageTintColor = Color.white;
                }
                else if (info.Backgrounds != null && info.Backgrounds.Length > 0)
                {
                    BackgroundImage.style.backgroundImage = info.Backgrounds[0].texture;
                    BackgroundImage.style.unityBackgroundImageTintColor = Color.white;
                }
            }
            if (SceneNameText != null)
            {
                SceneNameText.text = info.DisplayName;
            }
            if (DescriptionText != null)
            {
                if (ShowDescription)
                {
                    DescriptionText.text = info.Description;
                }
                else
                {
                    DescriptionText.text = string.Empty;
                }
            }
            if (LoadBarSlider != null)
            {
                LoadBarSlider.value = 0;
            }
            if (ProgressText != null)
            {
                ProgressText.text = string.Format(LoadingTextFormat, 0);
            }
            if (Manager.HasTips && (TipText != null ))
            {
                if (RandomTips)
                {
                    CurrentTip = Random.Range(0, cacheTips.Count);
                    if (TipText != null)
                        TipText.text = cacheTips[CurrentTip];
                }
                else
                {
                    if (TipText != null)
                        TipText.text = cacheTips[0];
                }
                StartCoroutine(TipsLoop());
            }
            //Show all UI
            RootUI.style.opacity = new StyleFloat(0f);
            RootUI.ShowVisualElement();

            //start audio loop
            Source.Play();
            StartCoroutine(FadeAudio(true));
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadNextScene()
        {
            //fade audio loop
            StartCoroutine(FadeAudio(false));
            //StartCoroutine(LoadNextSceneIE());
            StartCoroutine(SceneCheck());
        }

        /// <summary>
        /// 
        /// </summary>
        private IEnumerator StartAsyncOperation(string level)
        {
            if(async!=null)
                while (!async.isDone)
                {
                    // async.allowSceneActivation = false;
                    Debug.LogWarning("wait for loading end");
                    yield return new WaitForSeconds(1f);
                }
            
            while (RootUI.style.opacity.value < 1)
            {
                var alpha = RootUI.style.opacity;
                alpha.value += DeltaTime * FadeInSpeed;
                RootUI.style.opacity = alpha;
                yield return null;
            }
          

            async = bl_SceneLoaderUtils.LoadLevelAsync(level);
            if(async != null)
            if (GetSkipType != SceneSkipType.InstantComplete || CurrentLoadLevel.LoadingType == LoadingType.Fake)
            {
                async.allowSceneActivation = false;
            }
            else
            {
                async.allowSceneActivation = true;
            }
            isOperationStarted = true;
            yield return async;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator StartFakeLoading()
        {
            lerpValue = 0;
            while (lerpValue < 1)
            {
                lerpValue += Time.deltaTime / CurrentLoadLevel.FakeLoadingTime;
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator BackgroundTransition()
        {
            while (true)
            {
                BackgroundImage.style.backgroundImage = cacheBackgrounds[CurrentBackground].texture;
                while (BackgroundImage.style.opacity.value < 1)
                {
                    var alpha = BackgroundImage.style.opacity;
                    alpha.value += DeltaTime * FadeBackgroundSpeed * 0.8f;
                    BackgroundImage.style.opacity = alpha;
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForSeconds(TimePerBackground);
                while (BackgroundImage.style.opacity.value > 0)
                {
                    var alpha = BackgroundImage.style.opacity;
                    alpha.value -= DeltaTime * FadeBackgroundSpeed ;
                    BackgroundImage.style.opacity = alpha;
                    yield return new WaitForEndOfFrame();
                }
                CurrentBackground = (CurrentBackground + 1) % cacheBackgrounds.Count;
                yield return new WaitForSeconds(TimeBetweenBackground);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator TipsLoop()
        {
            if (TipText == null )
                yield break;
           
            Color alpha = Color.white;
            if (TipText != null)
                alpha = TipText.style.color.value;
            if (isTipFadeOut)
            {
                while (alpha.a < 1)
                {
                    alpha.a += DeltaTime * FadeTipsSpeed;
                    if (TipText != null)
                        TipText.style.color = alpha;
                    yield return null;
                }
                StartCoroutine(WaitNextTip(TimePerTip));
            }
            else
            {
                while (alpha.a > 0)
                {
                    alpha.a -= DeltaTime * FadeTipsSpeed;
                    if (TipText != null)
                        TipText.style.color = alpha;
                    yield return null;
                }
                StartCoroutine(WaitNextTip(0.75f));
            }
            if (isTipFadeOut)
            {
                if (RandomTips)
                {
                    int lastTip = CurrentTip;
                    CurrentTip = Random.Range(0, cacheTips.Count);
                    while (CurrentTip == lastTip)
                    {
                        CurrentTip = Random.Range(0, cacheTips.Count);
                        yield return null;
                    }
                    if (TipText != null)
                        TipText.text = cacheTips[CurrentTip];
                }
                else
                {
                    CurrentTip = (CurrentTip + 1) % cacheTips.Count;
                    if (TipText != null)
                        TipText.text = cacheTips[CurrentTip];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator FadeAudio(bool FadeIn)
        {
            if (BackgroundAudio == null)
                yield break;

            if (FadeIn)
            {
                while (Source.volume < AudioVolume)
                {
                    Source.volume += DeltaTime * FadeAudioSpeed;
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                while (Source.volume > 0)
                {
                    Source.volume -= DeltaTime * FadeAudioSpeed * 3;
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator WaitNextTip(float t)
        {
            isTipFadeOut = !isTipFadeOut;
            yield return new WaitForSeconds(t);
            StartCoroutine(TipsLoop());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadNextSceneIE()
        {
            load_leval = false;

            FadeImage.style.opacity = new StyleFloat(0f);
            while (FadeImage.style.opacity.value < 1)
            {
                var alpha = FadeImage.style.opacity;
                alpha.value += DeltaTime * FadeInSpeed;
                FadeImage.style.opacity = alpha;
                yield return null;
            }
            if (async != null)
                async.allowSceneActivation = true;
            RootUI.HideVisualElement();
            isOperationStarted = false;
            async = null;
            teleport = false;
            loadedMainScene = false;
        //    Debug.LogError("LoadNextSceneIE="+loadedMainScene+" to false");
            sceneNameInLoading = "";
            //Debug.LogError("LoadingScreen Disable Screen");
        }

        /// <summary>
        /// 
        /// </summary>
        private IEnumerator FadeOutCanvas(VisualElement alpha, float delay = 0)
        {
            yield return new WaitForSeconds(delay);
            while (alpha.style.opacity.value > 0f)
            {
               // Debug.LogWarning("FadeOutCanvas "+ alpha.name+" "+ alpha.alpha);
               var opacity = alpha.style.opacity;
               opacity.value = opacity.value - DeltaTime * FadeOutSpeed;
               alpha.style.opacity = opacity;
                yield return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private IEnumerator FadeInCanvas(VisualElement alpha, float delay = 0)
        {
            yield return new WaitForSeconds(delay);
            while (alpha.style.opacity.value < 1)
            {
              //  Debug.LogWarning("FadeInCanvas " + alpha.name + " " + alpha.alpha);

                var opacity = alpha.style.opacity;
                opacity.value = opacity.value + DeltaTime * FadeOutSpeed;
                alpha.style.opacity = opacity;
                yield return null;
            }
        }

        private SceneSkipType GetSkipType
        {
            get
            {
                if (CurrentLoadLevel != null)
                {
                    if (CurrentLoadLevel.SkipType != SceneSkipType.None)
                    {
                        return CurrentLoadLevel.SkipType;
                    }
                }
                if (SkipType != SceneSkipType.None)
                {
                    return SkipType;
                }
                else
                {
                    return SceneSkipType.AnyKey;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private float DeltaTime
        {
            get
            {
                return (useTimeScale) ? Time.deltaTime : Time.unscaledDeltaTime;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eData"></param>
        public void OnEvent(AtavismEventData eData)
        {
            AtavismLogger.LogDebugMessage("Scene Loader " + eData.eventType + " " + load_leval);

            if (eData.eventType == "PLAYER_TELEPORTED" && !load_leval)
            {
                if (float.Parse(eData.eventArgs[0]) > minTeleportDistanceToShow)
                {
                    FinishLoad = false;
                    isOperationStarted = true;
                    teleport = true;
                    StartCoroutine(FadeOutCanvas(FadeImage));
                    if (LoadingCircleBackground != null)
                    {
                        StartCoroutine(FadeInCanvas(LoadingCircleBackground, 0.1f));
                    }

                    if (BackgroundImage != null && useBackgrounds)
                    {
                        if (cacheBackgrounds.Count > 1)
                        {
                            for (int i = 0; i < cacheBackgrounds.Count; i++)
                            {
                                Sprite temp = cacheBackgrounds[i];
                                int randomIndex = Random.Range(i, cacheBackgrounds.Count);
                                cacheBackgrounds[i] = cacheBackgrounds[randomIndex];
                                cacheBackgrounds[randomIndex] = temp;

                            }

                            CurrentBackground = 0;
                            BackgroundImage.style.unityBackgroundImageTintColor = Color.white;
                        }
                        else
                        {
                            BackgroundImage.style.unityBackgroundImageTintColor = Color.white;
                        }
                    }

                    if (LoadBarSlider != null)
                    {
                        LoadBarSlider.value = 0;
                    }

                    if (ProgressText != null)
                    {
                        ProgressText.text = string.Format(LoadingTextFormat, 0);
                    }

                    if (Manager.HasTips && (TipText != null ))
                    {
                        if (RandomTips)
                        {
                            CurrentTip = Random.Range(0, cacheTips.Count);
                            if (TipText != null)
                                TipText.text = cacheTips[CurrentTip];
                        }
                        else
                        {
                            if (TipText != null)
                                TipText.text = cacheTips[0];
                        }

                        StartCoroutine(TipsLoop());
                    }

                    //Show all UI
                    RootUI.style.opacity = new StyleFloat(1f);
                    RootUI.ShowVisualElement();

                    StartCoroutine(SceneCheckTeleport());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator SceneCheckTeleport()
        {
            AtavismLogger.LogDebugMessage("Loading screen stremer SceneCheckTeleport");
            //fade audio loop
            WaitForSeconds delay = new WaitForSeconds(1f);
            yield return delay;
#if AT_STREAMER || AT_STREAMER2
            GameObject[] gos = GameObject.FindGameObjectsWithTag("SceneStreamer");
            foreach (GameObject go in gos)
            {
                Streamer s = go.GetComponent<Streamer>();
                if (s != null)
                {
                    streamers.Add(s);
                    s.showLoadingScreen = true;
                }
            }
            AtavismLogger.LogDebugMessage("Loading screen  streamer LoadNextScene count streamers " + streamers.Count);

            yield return delay;
#endif
            StartCoroutine(StreamerCheck());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator SceneCheck()
        {
            AtavismLogger.LogDebugMessage("Loading screen  streamer SceneCheck");
            //fade audio loop
            //  StartCoroutine(FadeAudio(false));
            async.allowSceneActivation = true;
            WaitForSeconds delay = new WaitForSeconds(1f);
            yield return delay;
#if AT_STREAMER || AT_STREAMER2
            GameObject[] gos = GameObject.FindGameObjectsWithTag("SceneStreamer");
            foreach (GameObject go in gos)
            {
                Streamer s = go.GetComponent<Streamer>();
                if (s != null)
                {
                    streamers.Add(s);
                    s.showLoadingScreen = true;
                }
            }
            AtavismLogger.LogDebugMessage("Loading screen  streamer SceneCheck count streamers " + streamers.Count);

         
#endif
            yield return delay;
            StartCoroutine(StreamerCheck());

        }


        IEnumerator StreamerCheck()
        {
            AtavismLogger.LogDebugMessage("Loading screen StreamerCheck  ");
            fillAmount = 0f;
            WaitForSeconds delay2 = new WaitForSeconds(2f);
#if AT_STREAMER || AT_STREAMER2
            WaitForSeconds delay = new WaitForSeconds(0.3f);
             if (sceneNameInLoading.Length > 0)
            {
                WaitForSeconds delay3 = new WaitForSeconds(0.3f);
                while (!loadedMainScene)
                {
                    Debug.LogWarning("Loading screen wait");
                    yield return delay3;
                }
            }
            if (streamers != null && streamers.Count > 0 && RootUI.IsVisibleElement())
            {
                bool initialized = true;
                bool run = true;
                while (run)
                {
                    while (fillAmount < 1)
                    {
                        AtavismLogger.LogDebugMessage("Loading screen + streamer StreamerCheck " + fillAmount);
                        fillAmount = 0f;
                        foreach (var s in streamers)
                        {
                            if (s != null)
                            {
                                fillAmount += s.GetLoadingProgress() / (float)streamers.Count;
                                AtavismLogger.LogDebugMessage("Loading screen + streamer " + s.name + " -> " + s.tilesLoaded + " / " + s.tilesToLoad + "| fillAmount ->" + fillAmount);
                                initialized = initialized && s.initialized;
                            }
                        }
                        if (initialized)
                        {
                            if (fillAmount >= 1)
                            {
                                try
                                {
                                    if (onDone != null)
                                        onDone.Invoke();
                                }
                                catch (System.Exception e)
                                {
                                    AtavismLogger.LogError("Loading screen + streamer StreamerCheck Exception " + e.Message + " " + e);
                                }
                            }
                        }
                        yield return delay;
                    }
                    fillAmount = 0f;
                    yield return delay2;
                    if (streamers.Count > 0)
                        foreach (var s in streamers)
                        {
                            if (s != null)
                            {
                                fillAmount += s.GetLoadingProgress() / (float)streamers.Count;
                                AtavismLogger.LogDebugMessage("Loading screen + streemner " + s.name + " -> " + s.tilesLoaded + " / " + s.tilesToLoad + "| fillAmount ->" + fillAmount);
                                initialized = initialized && s.initialized;
                            }
                        }
                    if (fillAmount >= 1)
                        run = false;
                }
            }
            streamers.Clear();
#else
            if (sceneNameInLoading.Length > 0)
            {
                WaitForSeconds delay3 = new WaitForSeconds(0.3f);
                while (!loadedMainScene)
                {
                    // Debug.LogWarning("Loading screen wait");
                    if (async != null && async.isDone)
                    {
                        
                        if(!SceneManager.GetActiveScene().name.Equals(sceneNameInLoading))
                            async = bl_SceneLoaderUtils.LoadLevelAsync(sceneNameInLoading);
                    }
                    yield return delay3;
                }
            }
            yield return delay2;
            AtavismEventSystem.DispatchEvent("LOADING_SCENE_END", new string[1]);
#endif

            //  for (int i = 0; i < SceneManager.sceneCount; i++)
            //  {

            // }
            ///  foreach (Scene s in SceneManager.GetAllScenes())
            //  {

            // }

            CurrentBackground = 0;

            if (FinishSound != null)
            {
                AudioSource.PlayClipAtPoint(FinishSound, transform.position, FinishSoundVolume);
            }
            if (LoadingCircleBackground != null)
            {
                StartCoroutine(FadeOutCanvas(LoadingCircleBackground, 0.5f));
            }
            if (LoadBarSlider != null && FadeLoadingBarOnFinish)
            {
                StartCoroutine(FadeOutCanvas(LoadBarSlider, 1));
            }

            StartCoroutine(LoadNextSceneIE());
        }
    }
}