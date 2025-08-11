using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{

    public class UIAtavismServerListEntry 
    {

        private Label m_serverName;
         private Label m_serverType;
         private Label m_serverPopulation;
         private Label m_serverQueue;
         private Label m_serverLoad;
         private Button m_background;
         private VisualElement m_serverPopulationImage;
         private VisualElement m_root;
        WorldServerEntry entry;
        UIAtavismCharacterServerSelection serverList;
        
        // Use this for initialization

        public void SetVisualElement(VisualElement visualElement)
        {
            m_root = visualElement;
            m_serverName = visualElement.Q<Label>("server-name");
            m_serverType = visualElement.Q<Label>("server-type");
            m_serverPopulation = visualElement.Q<Label>("server-population");
            m_serverQueue = visualElement.Q<Label>("server-queue");
            m_serverLoad = visualElement.Q<Label>("server-load");
            m_background = visualElement.Q<Button>("server-button");
            m_serverPopulationImage = visualElement.Q<VisualElement>("server-load-image");
            if (m_background != null)
            {
                m_background.clicked += ServerSelected;
            }
        }

        
        
        
        public void SetServerDetails(WorldServerEntry entry, UIAtavismCharacterServerSelection serverList)
        {
            this.entry = entry;
            if (entry.Name == AtavismClient.Instance.WorldId)
            {
                if (m_serverName != null)
#if AT_I2LOC_PRESET
                    this.m_serverName.text = I2.Loc.LocalizationManager.GetTranslation(entry.Name) +  " (" +I2.Loc.LocalizationManager.GetTranslation("current")+ ")";
#else
                    this.m_serverName.text = entry.Name + " (current)";
#endif

                m_background.SetEnabled(false);
                if (m_serverPopulationImage!=null)
                {
                    m_serverPopulationImage.RemoveFromClassList("population-low");
                    m_serverPopulationImage.RemoveFromClassList("population-medium");
                    m_serverPopulationImage.RemoveFromClassList("population-high");
                    if (entry.Load > 0.8F)
                    {
                        m_serverPopulationImage.AddToClassList("population-high");
                    }
                    else if (entry.Load > 0.5F)
                    {
                        m_serverPopulationImage.AddToClassList("population-medium");   
                    }
                    else
                    {
                        m_serverPopulationImage.AddToClassList("population-low"); 
                    }
                }
            }
            else
            {
                string status = (string)entry["status"];
                if (status != "Online")
                {
                    if (m_serverName != null)
#if AT_I2LOC_PRESET
                        this.m_serverName.text = I2.Loc.LocalizationManager.GetTranslation(entry.Name) +  " (" +I2.Loc.LocalizationManager.GetTranslation(status)+ ")";
#else
                        this.m_serverName.text = entry.Name + " (" + status + ")";
#endif                         
                      
                    m_background.SetEnabled(false);
                  
                }
                else
                {
                    if (m_serverName != null)
                        this.m_serverName.text = entry.Name;
                    m_background.SetEnabled(true);
                    if (m_serverPopulationImage!=null)
                    {
                        m_serverPopulationImage.RemoveFromClassList("population-low");
                        m_serverPopulationImage.RemoveFromClassList("population-medium");
                        m_serverPopulationImage.RemoveFromClassList("population-high");
                        if (entry.Load > 0.8F)
                        {
                            m_serverPopulationImage.AddToClassList("population-high");
                        }
                        else if (entry.Load > 0.5F)
                        {
                            m_serverPopulationImage.AddToClassList("population-medium");   
                        }
                        else
                        {
                            m_serverPopulationImage.AddToClassList("population-low"); 
                        }
                    }
                }
            }

            if (m_serverType != null)
                this.m_serverType.text = "";
            if (entry.Load == 1)
            {
#if AT_I2LOC_PRESET
                string load = I2.Loc.LocalizationManager.GetTranslation("Full");
#else
                string load = "Full";
#endif                
                if (m_serverLoad != null)
                    this.m_serverLoad.text = load;
            }
            else if (entry.Load > 0.8F)
            {
#if AT_I2LOC_PRESET
                string load = I2.Loc.LocalizationManager.GetTranslation("High");
#else
                string load = "High";
#endif                
                if (m_serverLoad != null)
                    this.m_serverLoad.text = load;
                
            }
            else if (entry.Load > 0.49F)
            {
#if AT_I2LOC_PRESET
                string load = I2.Loc.LocalizationManager.GetTranslation("Medium");
#else
                string load = "Medium";
#endif                
                if (m_serverLoad != null)
                    this.m_serverLoad.text = load;
                
            }
            else
            {
#if AT_I2LOC_PRESET
                string load = I2.Loc.LocalizationManager.GetTranslation("Low");
#else
                string load = "Low";
#endif
                if (m_serverLoad != null)
                    this.m_serverLoad.text = load;

            }


            if (m_serverQueue != null)
                this.m_serverQueue.text = entry.Queue.ToString();

            if (m_serverPopulation != null)
                this.m_serverPopulation.text = entry.Population.ToString();
            
            this.serverList = serverList;
        }

        public void ServerSelected()
        {
            serverList.SelectEntry(entry);
            //EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}