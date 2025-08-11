using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

[RequireComponent(typeof(UIDocument))]
    public class UIAtavismLoadingPrefabData : MonoBehaviour
    {
        [SerializeField] private VisualElement background;
        [SerializeField] private UIProgressBar progressBar;
        private UIDocument uiDocument;
        private VisualElement uiScreen;

        void Start()
        {
            if(background !=null)
                background.HideVisualElement();
           

        }

        protected  void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            unregisterUI();
            registerEvents();
        }

        protected bool unregisterUI()
        {
            uiDocument = GetComponent<UIDocument>();
            uiDocument.enabled = true;
            uiScreen = uiDocument.rootVisualElement.Query<VisualElement>("Screen");
            background = uiDocument.rootVisualElement.Query<VisualElement>("background");
            progressBar = uiDocument.rootVisualElement.Query<UIProgressBar>("slider");
            if(background !=null)
                background.HideVisualElement();
            
            return true;
        }
        protected virtual void OnDisable()
        {
            unregisterEvents();
        }
        protected void registerEvents()
        { 
            AtavismEventSystem.RegisterEvent("LOADING_PREFAB_UPDATE", this);
            AtavismEventSystem.RegisterEvent("LOADING_PREFAB_SHOW", this);
            AtavismEventSystem.RegisterEvent("LOADING_PREFAB_HIDE", this);
        }


        protected void unregisterEvents()
        {
            AtavismEventSystem.UnregisterEvent("LOADING_PREFAB_UPDATE", this);
            AtavismEventSystem.UnregisterEvent("LOADING_PREFAB_SHOW", this);
            AtavismEventSystem.UnregisterEvent("LOADING_PREFAB_HIDE", this);

        }
        
        public void OnEvent(AtavismEventData eData)
        {
            if (eData.eventType == "LOADING_PREFAB_UPDATE")
            {
                int value = int.Parse(eData.eventArgs[0]);
                int max = int.Parse(eData.eventArgs[1]);
                if (progressBar!=null)
                {
                    progressBar.highValue = max;
                    progressBar.value = value;
                }
            }
            else if (eData.eventType == "LOADING_PREFAB_SHOW")
            {
                if (background !=null)
                    background.ShowVisualElement();
            }
            else if (eData.eventType == "LOADING_PREFAB_HIDE")
            {
                if (background !=null)
                    background.HideVisualElement();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}