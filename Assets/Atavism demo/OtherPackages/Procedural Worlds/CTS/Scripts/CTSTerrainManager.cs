using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CTS
{
    /// <summary>
    /// Manages communication between weather, terrain profiles and terrain instances. General 
    /// CTS configuration and control should be performed via this class. Local weather control
    /// should be controlled via Weather Manager class.
    /// </summary>
    public class CTSTerrainManager : CTSSingleton<CTSTerrainManager>
    {
       
        /// <summary>
        /// The last time the shader list was updated
        /// </summary>
        private DateTime m_lastShaderListUpdate = DateTime.MinValue;

        /// <summary>
        /// The controllers in the scene
        /// </summary>
        private List<CTSWeatherController> m_controllerList = new List<CTSWeatherController>();

        /// <summary>
        /// The last time the controller list was updated
        /// </summary>
        private DateTime m_lastControllerListUpdate = DateTime.MinValue;

        /// <summary>
        /// Make sure its only ever a singleton by stopping direct instantiation
        /// </summary>
        protected CTSTerrainManager()
        {
        }

      
        /// <summary>
        /// Grab all the controllers in the scene
        /// </summary>
        /// <param name="force">Force an update always</param>
        public void RegisterAllControllers(bool force = false)
        {
            if (Application.isPlaying)
            {
                if (force)
                {
                    m_controllerList.Clear();
                    m_controllerList.AddRange(GameObject.FindObjectsOfType<CTSWeatherController>());
                    m_lastControllerListUpdate = DateTime.Now;
                }
            }
            else
            {
                if (force || m_controllerList.Count == 0 || (DateTime.Now - m_lastControllerListUpdate).TotalSeconds > 30)
                {
                    m_controllerList.Clear();
                    m_controllerList.AddRange(GameObject.FindObjectsOfType<CTSWeatherController>());
                    m_lastControllerListUpdate = DateTime.Now;
                }
            }
        }

  

        /// <summary>
        /// Broadcast a weather update
        /// </summary>
        /// <param name="manager">The manager with the update</param>
        public void BroadcastWeatherUpdate(CTSWeatherManager manager)
        {
            //Periodically update this list
            RegisterAllControllers();

            //And then broadcast to it
            for (int idx = 0; idx < m_controllerList.Count; idx++)
            {
                m_controllerList[idx].ProcessWeatherUpdate(manager);
            }
        }

      
    }
}