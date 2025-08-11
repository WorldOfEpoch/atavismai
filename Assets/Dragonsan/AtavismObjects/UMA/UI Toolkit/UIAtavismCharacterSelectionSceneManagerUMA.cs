
using UnityEngine;

namespace Atavism.UI
{
    
    public class UIAtavismCharacterSelectionSceneManageUMA : UIAtavismCharacterSelectionSceneManager
    {

        /// <summary>
        /// 
        /// </summary>
        public override void CustomizationManagerCheck()
        {
            if (selectedCharacterEntry == null)
                return;
            if (characterModel == null)
                return;
            if(!showing)
                return;
            Debug.LogError("UIAtavismCharacterSelectionSceneManageUMA");
            // Dictionary<string, object> appearanceProps = new Dictionary<string, object>();
            // foreach (string key in selectedCharacterEntry.Keys)
            // {
			         //
            //     if (key.StartsWith("custom:appearanceData:"))
            //     {
            //         appearanceProps.Add(key.Substring(23), selectedCharacterEntry[key]);
            //     }
            // }
            
            UMA.CharacterSystem.DynamicCharacterAvatar avatar = CharacterModel.GetComponent<UMA.CharacterSystem.DynamicCharacterAvatar>();
            if(avatar != null)
            {
                if (selectedCharacterEntry.ContainsKey("custom:umaData:CurrentRecipe"))
                {
                    string recipe = (string)selectedCharacterEntry["custom:umaData:CurrentRecipe"];
                    avatar.LoadFromRecipeString(recipe);
                }
                avatar.BuildCharacter();
            }

        }

    }
}