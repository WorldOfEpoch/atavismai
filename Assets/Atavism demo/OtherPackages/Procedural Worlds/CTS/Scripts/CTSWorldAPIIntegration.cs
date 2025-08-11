using UnityEngine;

#if WORLDAPI_PRESENT
using WAPI;
#endif
namespace CTS
{
    /// <summary>
    /// Drive CTS from WorldAPI - add a CTS Weather manager in the scene and drop this script onto it
    /// </summary>
    [RequireComponent(typeof(CTSWeatherManager))]
    [ExecuteInEditMode]
#if WORLDAPI_PRESENT
    public class CTSWorldAPIIntegration : MonoBehaviour, IWorldApiChangeHandler
#else
    public class CTSWorldAPIIntegration : MonoBehaviour
#endif
    {
        public bool m_updateSnow = true;
        public bool m_updateWetness = true;
        public bool m_updateSeasons = true;
        private CTSWeatherManager m_weatherManager;

        /// <summary>
        /// Set up connection to WAPI to drive CTS
        /// </summary>
        void Start()
        {
            //   CTSTerrainManager.Instance.RegisterAllShaders(true);
            CTSTerrainManager.Instance.RegisterAllControllers(true);
            m_weatherManager = GetComponent<CTSWeatherManager>();
            ConnectToWorldAPI();
            if (m_updateSnow)
            {
#if WORLDAPI_PRESENT
                m_weatherManager.SnowPower = WorldManager.Instance.SnowPowerTerrain;
                m_weatherManager.SnowMinHeight = WorldManager.Instance.SnowMinHeight;
#endif
            }
            if (m_updateWetness)
            {
#if WORLDAPI_PRESENT
                m_weatherManager.RainPower = WorldManager.Instance.RainPower;
#endif
            }
            if (m_updateSeasons)
            {
#if WORLDAPI_PRESENT
                m_weatherManager.Season = WorldManager.Instance.Season;
#endif
            }
        }

        /// <summary>
        /// Connect to world api and start listening to updates
        /// </summary>
        void ConnectToWorldAPI()
        {
#if WORLDAPI_PRESENT
            WorldManager.Instance.AddListener(this);
#endif
        }

        /// <summary>
        /// Disconnect from world api
        /// </summary>
        void DisconnectFromWorldAPI()
        {
#if WORLDAPI_PRESENT
            WorldManager.Instance.RemoveListener(this);
#endif
        }

        /// <summary>
        /// On destroy object disconnetc from world API
        /// </summary>
        private void OnDestroy()
        {
            DisconnectFromWorldAPI();
        }



        /// <summary>
        /// Handle updates from world api
        /// </summary>
        /// <param name="changeArgs">Change arguements</param>
#if WORLDAPI_PRESENT
        public void OnWorldChanged(WorldChangeArgs changeArgs)
        {
            if (m_weatherManager == null)
            {
                m_weatherManager = GetComponent<CTSWeatherManager>();
            }

            if (m_updateSnow && changeArgs.HasChanged(WorldConstants.WorldChangeEvents.SnowChanged))
            {
                m_weatherManager.SnowPower = WorldManager.Instance.SnowPowerTerrain;
                m_weatherManager.SnowMinHeight = WorldManager.Instance.SnowMinHeight;
            }
            if (m_updateWetness && changeArgs.HasChanged(WorldConstants.WorldChangeEvents.RainChanged))
            {
                m_weatherManager.RainPower = WorldManager.Instance.RainPowerTerrain;
            }
            if (m_updateSeasons && changeArgs.HasChanged(WorldConstants.WorldChangeEvents.SeasonChanged))
            {
                m_weatherManager.Season = WorldManager.Instance.Season;
            }
        }
#endif
    }
}
