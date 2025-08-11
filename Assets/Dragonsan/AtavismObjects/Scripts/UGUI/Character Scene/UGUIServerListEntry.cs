using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

namespace Atavism
{

    public class UGUIServerListEntry : MonoBehaviour
    {

        public Text serverName;
        public TextMeshProUGUI TMPServerName;
       [HideInInspector] public Text serverType;
       [HideInInspector] public TextMeshProUGUI TMPServerType;
        public Text serverPopulation;
        public TextMeshProUGUI TMPServerPopulation;
        public Text serverQueue;
        public TextMeshProUGUI TMPServerQueue;
        public Text serverLoad;
        public TextMeshProUGUI TMPServerLoad;
        
        public Image serverPopulationImage;
        public Sprite serverPopulationLow;
        public Sprite serverPopulationMedium;
        public Sprite serverPopulationHigh;
        WorldServerEntry entry;
        UGUIServerList serverList;
        
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetServerDetails(WorldServerEntry entry, UGUIServerList serverList)
        {
            this.entry = entry;
            if (entry.Name == AtavismClient.Instance.WorldId)
            {
                if (serverName != null)
#if AT_I2LOC_PRESET
                    this.serverName.text = I2.Loc.LocalizationManager.GetTranslation(entry.Name) +  " (" +I2.Loc.LocalizationManager.GetTranslation("current")+ ")";
#else
                    this.serverName.text = entry.Name + " (current)";
#endif                         
                      
                if (TMPServerName != null)
#if AT_I2LOC_PRESET
                    this.TMPServerName.text = I2.Loc.LocalizationManager.GetTranslation(entry.Name) +  " (" +I2.Loc.LocalizationManager.GetTranslation("current")+ ")";
#else
                    this.TMPServerName.text = entry.Name + " (current)";
#endif                        
                GetComponent<Button>().interactable = false;
                if (serverPopulationImage)
                {
                    if (!serverPopulationImage.enabled)
                    {
                        serverPopulationImage.enabled = true;
                    }

                    if (entry.Load > 0.8F)
                    {
                        serverPopulationImage.sprite = serverPopulationHigh;
                    }
                    else if (entry.Load > 0.5F)
                    {
                        serverPopulationImage.sprite = serverPopulationMedium;
                    }
                    else
                    {
                        serverPopulationImage.sprite = serverPopulationLow;
                    }
                }
            }
            else
            {
                string status = (string)entry["status"];
                if (status != "Online")
                {
                    if (serverName != null)
#if AT_I2LOC_PRESET
                        this.serverName.text = I2.Loc.LocalizationManager.GetTranslation(entry.Name) +  " (" +I2.Loc.LocalizationManager.GetTranslation(status)+ ")";
#else
                        this.serverName.text = entry.Name + " (" + status + ")";
#endif                         
                      
                    if (TMPServerName != null)
#if AT_I2LOC_PRESET
                        this.TMPServerName.text = I2.Loc.LocalizationManager.GetTranslation(entry.Name) +  " (" +I2.Loc.LocalizationManager.GetTranslation(status)+ ")";
#else
                        this.TMPServerName.text = entry.Name + " (" + status + ")";
#endif        
                    
                    GetComponent<Button>().interactable = false;
                    if (serverPopulationImage)
                    {
                        if (serverPopulationImage.enabled)
                        {
                            serverPopulationImage.enabled = false;
                        }
                    }
                }
                else
                {
                    if (serverName != null)
                        this.serverName.text = entry.Name;
                    if (TMPServerName != null)
                        this.TMPServerName.text = entry.Name;
                    GetComponent<Button>().interactable = true;
                    if (serverPopulationImage)
                    {
                        if (!serverPopulationImage.enabled)
                        {
                            serverPopulationImage.enabled = true;
                        }

                        if (entry.Load > 0.8F)
                        {
                            serverPopulationImage.sprite = serverPopulationHigh;
                        }
                        else if (entry.Load > 0.49F)
                        {
                            serverPopulationImage.sprite = serverPopulationMedium;
                        }
                        else
                        {
                            serverPopulationImage.sprite = serverPopulationLow;
                        }
                    }
                }
            }

            if (serverType != null)
                this.serverType.text = "";
            if (TMPServerType != null)
                this.TMPServerType.text = "";
            if (entry.Load == 1)
            {
#if AT_I2LOC_PRESET
                string load = I2.Loc.LocalizationManager.GetTranslation("Full");
#else
                string load = "Full";
#endif                
                if (serverLoad != null)
                    this.serverLoad.text = load;
                if (TMPServerLoad != null)
                    this.TMPServerLoad.text = load;
            }
            else if (entry.Load > 0.8F)
            {
#if AT_I2LOC_PRESET
                string load = I2.Loc.LocalizationManager.GetTranslation("High");
#else
                string load = "High";
#endif                
                if (serverLoad != null)
                    this.serverLoad.text = load;
                if (TMPServerLoad != null)
                    this.TMPServerLoad.text = load;
                
            }
            else if (entry.Load > 0.49F)
            {
#if AT_I2LOC_PRESET
                string load = I2.Loc.LocalizationManager.GetTranslation("Medium");
#else
                string load = "Medium";
#endif                
                if (serverLoad != null)
                    this.serverLoad.text = load;
                if (TMPServerLoad != null)
                    this.TMPServerLoad.text = load;
                
            }
            else
            {
#if AT_I2LOC_PRESET
                string load = I2.Loc.LocalizationManager.GetTranslation("Low");
#else
                string load = "Low";
#endif
                if (serverLoad != null)
                    this.serverLoad.text = load;
                if (TMPServerLoad != null)
                    this.TMPServerLoad.text = load;

            }


            if (serverQueue != null)
                this.serverQueue.text = entry.Queue.ToString();
            if (TMPServerQueue != null)
                this.TMPServerQueue.text = entry.Queue.ToString();

            if (serverPopulation != null)
                this.serverPopulation.text = entry.Population.ToString();
            if (TMPServerPopulation != null)
                this.TMPServerPopulation.text = entry.Population.ToString();
            
            
            this.serverList = serverList;
        }

        public void ServerSelected()
        {
            serverList.SelectEntry(entry);
            //EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}