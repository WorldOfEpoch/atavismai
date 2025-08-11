using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Atavism
{
    public class PrefabBrowser : EditorWindow
    {
        [MenuItem("Window/Atavism/Atavism Prefab Browser")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(PrefabBrowser));
        }
        //int size = 64;
        bool item = false;
        bool itemSet = false;
        bool skills = false;
        bool abilities = false;
        bool effects = false;
        bool craftRecipes = false;
        bool currencies = false;
        bool buildObjects = false;
        bool resourceNode = false;
        bool globalEvents = false;
        bool reces = false;
        bool quests = false;
        bool icons = false;
        bool weaponProfiles = false;
        bool itemAudioProfiles = false;
        bool stats = false;
        bool interactiveObjectProfiles = false;
        bool slotsProfiles = false;

        Vector2 scrollPos;
        Vector2 scrollPos2;
        Vector2 scrollPos3;
        Vector2 scrollPos4;
        Vector2 scrollPos5;
        Vector2 scrollPos6;
        Vector2 scrollPos7;
        Vector2 scrollPos8;
        Vector2 scrollPos9;
        Vector2 scrollPos10;
        private Vector2 scrollPos11;
        private Vector2 scrollPos12;
        Vector2 scrollPos13;
        Vector2 scrollPos14;
        private Vector2 scrollPos15;
        private Vector2 scrollPos16;
        Vector2 scrollPos17;
        Vector2 scrollPos18;
        string itemSearche = "";
        string itemSetSearche = "";
        string skillSearche = "";
        string abilitiesSearche = "";
        string effectsSearche = "";
        string currencySearche = "";
        string craftingSearche = "";
        string buildObjSearche = "";
        string questSearche = "";
        string iconsSearche = "";
        string weaponProfileSearche = "";
        string itemAudioProfileSearche = "";
        string statsSearche = "";
        string interactiveObjectProfileSearche = "";
        string slotsProfileSearche = "";

        int limit = 100;
        public void OnGUI()
        {
            EditorGUILayout.LabelField("Select Only In Run");
            EditorGUILayout.Space();
            limit = EditorGUILayout.IntField("Display Limit", limit);
            EditorGUILayout.Space();
            currencies = EditorGUILayout.Toggle("Show Currencies", currencies);
            item = EditorGUILayout.Toggle("Show Items", item);
            itemAudioProfiles = EditorGUILayout.Toggle("Show Item Audio Profiles", itemAudioProfiles);
            itemSet = EditorGUILayout.Toggle("Show Item Sets", itemSet);
            craftRecipes = EditorGUILayout.Toggle("Show Crafting Recipes", craftRecipes);
            skills = EditorGUILayout.Toggle("Show Skills", skills);
            abilities = EditorGUILayout.Toggle("Show Abilities", abilities);
            effects = EditorGUILayout.Toggle("Show Effects", effects);
            buildObjects = EditorGUILayout.Toggle("Show Build Objects", buildObjects);
            resourceNode = EditorGUILayout.Toggle("Show ResouceNodes Profiles", resourceNode);
            globalEvents = EditorGUILayout.Toggle("Show Global Events", globalEvents);
            reces = EditorGUILayout.Toggle("Show Races", reces);
            quests = EditorGUILayout.Toggle("Show Quests", quests);
            weaponProfiles = EditorGUILayout.Toggle("Show Weapon Profiles", weaponProfiles);
            interactiveObjectProfiles = EditorGUILayout.Toggle("Show Interactive Object Profiles", interactiveObjectProfiles);
            slotsProfiles = EditorGUILayout.Toggle("Show Slots Profiles", slotsProfiles);

            stats = EditorGUILayout.Toggle("Show Stats", stats);
            icons = EditorGUILayout.Toggle("Show Icons", icons);
            EditorGUILayout.Space();
            if (!Application.isPlaying)
                return;
            if (currencies)
            {
                showCurrencies();
            }
            if (item)
            {
                showItems();
            }
            if (itemAudioProfiles)
            {
                showItemAudioProfiles();
            }
            if (craftRecipes)
            {
                showCraftRecipes();
            }
            if (itemSet)
            {
                showItemSets();
            }
            if (skills)
            {
                showSkills();
            }
            if (abilities)
            {
                showAbilities();
            }
            if (effects)
            {
                showEffects();
            }
            if (buildObjects)
            {
                showBuildObjects();
            }

            if (resourceNode)
            {
                showResourceNodeProfiles();
            }
            
            if (globalEvents)
            {
                showGlobalEvents();
            }  
            if (reces)
            {
                showRaces();
            }
            if (quests)
            {
                showQuests();
            }

            if (icons)
            {
                ShowIcons();
            }
            if (weaponProfiles)
            {
                showWeaponProfiles();
            }
            
            if (stats)
            {
                showStats();
            }

            if (interactiveObjectProfiles)
            {
                showInteractiveObjectProfiles();
            }
            
            if (slotsProfiles)
            {
                showSlotsProfiles();
            }
        }

         void ShowIcons()
        {
            EditorGUILayout.LabelField("List of Icons " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetIconsPrefabData() != null ? AtavismPrefabManager.Instance.GetIconsPrefabData().Count : 0));
            iconsSearche = EditorGUILayout.TextField("Search", iconsSearche);
            EditorGUILayout.Space();
            scrollPos13 = EditorGUILayout.BeginScrollView(scrollPos13, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;
                foreach (string key in AtavismPrefabManager.Instance.GetIconsPrefabData().Keys)
                {
                    AtavismIconData obj = AtavismPrefabManager.Instance.GetIconsPrefabData()[key];
                    if (key.ToLower().Contains(iconsSearche.ToLower()) || iconsSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.TextField("icon path", key);
                            EditorGUILayout.TextField("icon data", obj.iconData);
                            
                            if (obj.icon == null)
                            {
                                if (obj.iconData.Length > 0)
                                {
                                    Texture2D tex = new Texture2D(2, 2);
                                    bool wyn = tex.LoadImage(System.Convert.FromBase64String(obj.iconData));
                                    Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                                    obj.icon = sprite;
                                }
                            }
                            if (obj.icon != null)
                                EditorGUILayout.TextField("Icon size", obj.icon.texture.width + "x" + obj.icon.texture.height);
                            EditorGUILayout.ObjectField("Icon", obj.icon, typeof(Sprite), false);

                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }
                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }
        
        
        void showCurrencies()
        {
            EditorGUILayout.LabelField("List of Currencies " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetCurrencyPrefabData() != null ? AtavismPrefabManager.Instance.GetCurrencyPrefabData().Count : 0));
            currencySearche = EditorGUILayout.TextField("Search", currencySearche);
            EditorGUILayout.Space();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;
                foreach (CurrencyPrefabData obj in AtavismPrefabManager.Instance.GetCurrencyPrefabData())
                {
                    if (obj.name.ToLower().Contains(currencySearche.ToLower()) || currencySearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.IntField("id", obj.id);
                            EditorGUILayout.TextField("name", obj.name);
                            //  EditorGUILayout.TextField("icon name", obj.iconData);
                            EditorGUILayout.TextField("icon path", obj.iconPath);
                            Sprite icon = AtavismPrefabManager.Instance.GetCurrencyIconByID(obj.id);
                            
                            if(icon!=null)
                                EditorGUILayout.TextField("Icon size", icon.texture.width + "x" + icon.texture.height);
                            EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);

                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }
                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }

        void showItems()
        {
            EditorGUILayout.LabelField("List of Items " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetItemPrefabData() != null ? AtavismPrefabManager.Instance.GetItemPrefabData().Count : 0));
            itemSearche = EditorGUILayout.TextField("Search", itemSearche);
            EditorGUILayout.Space();
            scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;
                foreach (ItemPrefabData obj in AtavismPrefabManager.Instance.GetItemPrefabData())
                {

                    if (obj.name.ToLower().Contains(itemSearche.ToLower()) || itemSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                           
                            EditorGUILayout.IntField("id", obj.templateId);
                            EditorGUILayout.TextField("name", obj.name);
                            EditorGUILayout.TextField("icon path", obj.iconPath);
                            Sprite icon  = AtavismPrefabManager.Instance.GetItemIconByID(obj.templateId);
                           
                            if(icon!=null)
                                EditorGUILayout.TextField("Icon size", icon.texture.width + "x" + icon.texture.height);
                            EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);

                            EditorGUILayout.IntField("Set Id", obj.setId);
                            EditorGUILayout.IntField("Weapon Profile Id", obj.weaponProfile);
                            EditorGUILayout.IntField("Audio Profile Id", obj.audioProfile);
                            EditorGUILayout.IntField("Durability", obj.durability);
                            EditorGUILayout.Toggle("Repairable", obj.repairable);
                            EditorGUILayout.LongField("Cost", obj.cost);
                            EditorGUILayout.IntField("Currency", obj.currencyType);
                            EditorGUILayout.TextField("Ground Prefab", obj.groundPrefab);


                            /*    EditorGUILayout.TextField("Item Type", obj.itemType);
                                EditorGUILayout.TextField("Tooltip", obj.tooltip);
                                EditorGUILayout.TextField("Sub Type", obj.subType);
                                EditorGUILayout.TextField("Slot", obj.slot);
                                EditorGUILayout.IntField("Quality", obj.quality);
                                EditorGUILayout.IntField("CurrencyType", obj.currencyType);
                                EditorGUILayout.LongField("Cost", obj.cost);
                                EditorGUILayout.IntField("Binding", obj.binding);
                                EditorGUILayout.Toggle("Sellable", obj.sellable);
                                EditorGUILayout.IntField("Damage Value", obj.damageValue);
                                EditorGUILayout.IntField("Damage Max Value", obj.damageMaxValue);
                                EditorGUILayout.IntField("SetId", obj.setId);
                                EditorGUILayout.IntField("Enchant Id", obj.enchantId);
                                EditorGUILayout.IntField("Weapon Speed", obj.weaponSpeed);
                                EditorGUILayout.IntField("Stack Limit", obj.stackLimit);
                                EditorGUILayout.Toggle("Auction House", obj.auctionHouse);
                                EditorGUILayout.Toggle("Unique", obj.unique);
                                EditorGUILayout.IntField("Gear Score", obj.gear_score);

                                EditorGUILayout.TextField("Socket Type", obj.sockettype);
                                EditorGUILayout.IntField("Durability", obj.durability);
                                EditorGUILayout.IntField("Weight", obj.weight);
                                EditorGUILayout.IntField("Auto Attack", obj.autoattack);
                                EditorGUILayout.IntField("Ammo Type", obj.ammoType);
                                EditorGUILayout.IntField("Death Loss", obj.deathLoss);
                                EditorGUILayout.Toggle("Parry", obj.parry);*/
                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }
                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }
   void showItemAudioProfiles()
        {
            EditorGUILayout.LabelField("List of Item Audio Profiles " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetItemAudioProfilePrefabData() != null ? AtavismPrefabManager.Instance.GetItemAudioProfilePrefabData().Count : 0));
            itemAudioProfileSearche = EditorGUILayout.TextField("Search", itemAudioProfileSearche);
            EditorGUILayout.Space();
            scrollPos15 = EditorGUILayout.BeginScrollView(scrollPos15, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;
                foreach (ItemAudioProfileData obj in AtavismPrefabManager.Instance.GetItemAudioProfilePrefabData())
                {

                    if (obj.name.ToLower().Contains(itemSearche.ToLower()) || itemSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                           
                            EditorGUILayout.IntField("Id", obj.id);
                            EditorGUILayout.TextField("Name", obj.name);
                            EditorGUILayout.TextField("Audio Name for Use", obj.use);
                            EditorGUILayout.TextField("Audio Name for Begin Drag", obj.drag_begin);
                            EditorGUILayout.TextField("Audio Name for End Drag", obj.drag_end);
                            EditorGUILayout.TextField("Audio Name for Delete", obj.delete);
                            EditorGUILayout.TextField("Audio Name for Broke", obj.broke);
                            EditorGUILayout.TextField("Audio Name for Pick Up", obj.pick_up);
                            EditorGUILayout.TextField("Audio Name for Fall", obj.fall);
                            EditorGUILayout.TextField("Audio Name for Drop", obj.drop);
                            EditorGUILayout.LongField("Date", obj.date);
                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }
                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }

        void showCraftRecipes()
        {
            EditorGUILayout.LabelField("List of Crafting Recipes " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetCraftingRecipesPrefabData() != null ? AtavismPrefabManager.Instance.GetCraftingRecipesPrefabData().Count : 0));
            craftingSearche = EditorGUILayout.TextField("Search", craftingSearche);
            EditorGUILayout.Space();
            scrollPos3 = EditorGUILayout.BeginScrollView(scrollPos3, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;

                foreach (CraftingRecipePrefabData obj in AtavismPrefabManager.Instance.GetCraftingRecipesPrefabData())
                {
                    if (obj.recipeName.ToLower().Contains(craftingSearche.ToLower()) || craftingSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.IntField("Id", obj.recipeID);
                            EditorGUILayout.TextField("Name", obj.recipeName);
                            EditorGUILayout.TextField("icon path", obj.iconPath);
                            //  EditorGUILayout.TextField("icon name", obj.iconData);
                            Sprite icon = AtavismPrefabManager.Instance.GetCraftingRecipeIconByID(obj.recipeID);
                            if(icon!=null)
                                EditorGUILayout.TextField("Icon size", icon.texture.width + "x" + icon.texture.height);
                            EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);
                            EditorGUILayout.TextField("Required station", obj.stationReq);
                            EditorGUILayout.IntField("Skill Id", obj.skillID);
                            EditorGUILayout.IntField("Skill Level", obj.skillLevelReq);
                            EditorGUILayout.LongField("date", obj.date);
                            EditorGUILayout.LabelField("List of Required Items"); 
                            if (obj.itemsReq.Count > 0)
                            {
                                for(int r = 0; r < obj.itemsReq.Count; r++){
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.IntField("Item Id", obj.itemsReq[r]);
                                    EditorGUILayout.IntField("Count ", obj.itemsReqCounts[r]);
                                    EditorGUILayout.EndHorizontal();
                                }
                               
                            }
                            else
                            {
                                EditorGUILayout.LabelField("No assigned item");
                            }
                            
                            EditorGUILayout.LabelField("List of Create Items Group #1"); 
                            if (obj.createsItems.Count > 0)
                            {
                                for(int r = 0; r < obj.createsItems.Count; r++){
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.IntField("Item Id", obj.createsItems[r]);
                                    EditorGUILayout.IntField("Count ", obj.createsItemsCounts[r]);
                                    EditorGUILayout.EndHorizontal();
                                }
                               
                            }
                            else
                            {
                                EditorGUILayout.LabelField("No assigned item");
                            }
                            EditorGUILayout.LabelField("List of Create Items Group #2"); 
                            if (obj.createsItems2.Count > 0)
                            {
                                for(int r = 0; r < obj.createsItems2.Count; r++){
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.IntField("Item Id", obj.createsItems2[r]);
                                    EditorGUILayout.IntField("Count ", obj.createsItemsCounts2[r]);
                                    EditorGUILayout.EndHorizontal();
                                }
                               
                            }
                            else
                            {
                                EditorGUILayout.LabelField("No assigned item");
                            }
                            EditorGUILayout.LabelField("List of Create Items Group #3"); 
                            if (obj.createsItems3.Count > 0)
                            {
                                for(int r = 0; r < obj.createsItems3.Count; r++){
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.IntField("Item Id", obj.createsItems3[r]);
                                    EditorGUILayout.IntField("Count ", obj.createsItemsCounts3[r]);
                                    EditorGUILayout.EndHorizontal();
                                }
                               
                            }
                            else
                            {
                                EditorGUILayout.LabelField("No assigned item");
                            }
                            EditorGUILayout.LabelField("List of Create Items Group #4"); 
                            if (obj.createsItems4.Count > 0)
                            {
                                for(int r = 0; r < obj.createsItems4.Count; r++){
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.IntField("Item Id", obj.createsItems4[r]);
                                    EditorGUILayout.IntField("Count ", obj.createsItemsCounts4[r]);
                                    EditorGUILayout.EndHorizontal();
                                }
                               
                            }
                            else
                            {
                                EditorGUILayout.LabelField("No assigned item");
                            }

                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }
                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }

        void showItemSets()
        {
            EditorGUILayout.LabelField("List of Item Sets " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetItemSetPrefabData() != null ? AtavismPrefabManager.Instance.GetItemSetPrefabData().Count : 0));
            itemSetSearche = EditorGUILayout.TextField("Search", itemSetSearche);
            EditorGUILayout.Space();
            scrollPos4 = EditorGUILayout.BeginScrollView(scrollPos4, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;

                foreach (ItemSetPrefabData obj in AtavismPrefabManager.Instance.GetItemSetPrefabData())
                {
                    if (obj.Name.ToLower().Contains(itemSetSearche.ToLower()) || itemSetSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.IntField("id", obj.Setid);
                            EditorGUILayout.TextField("name", obj.Name);
                            //  EditorGUILayout.TextField("icon name", obj.iconData);
                            //   EditorGUILayout.ObjectField("Icon", obj.icon, typeof(Sprite), false);
                            EditorGUILayout.LongField("date", obj.date);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("List of Items"); 
                            EditorGUILayout.BeginVertical();
                            if (obj.itemList.Count > 0)
                            {
                                foreach (var it in obj.itemList)
                                {
                                    EditorGUILayout.IntField(it);
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField("No assigned item");
                            }

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();    
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Levels"); 
                            EditorGUILayout.BeginVertical();
                            int c = 0;
                            if (obj.levelList.Count > 0)
                            {
                                foreach (var level in obj.levelList)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField(""+(c++));
                                    EditorGUILayout.BeginVertical();
                                    EditorGUILayout.IntField("Number of Parts",level.NumerOfParts);
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.IntField("Damage",level.DamageValue);
                                    EditorGUILayout.IntField("Percentage",level.DamageValuePercentage);
                                    EditorGUILayout.EndHorizontal();
                                    for (int st = 0; st < level.itemStatName.Count; st++)
                                    {
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.IntField(level.itemStatName[st],level.itemStatValues[st]);
                                        EditorGUILayout.IntField("Percentage",level.itemStatValuesPercentage[st]);
                                        EditorGUILayout.EndHorizontal();
                                    }
                                    
                                    EditorGUILayout.Space();

                                    
                                    if (level.itemEffects != null && level.itemEffects.Count > 0)
                                    {                                        
                                        EditorGUILayout.LabelField("Effects:");
                                        EditorGUILayout.BeginVertical();
                                        for (int ef = 0; ef < level.itemEffects.Count; ef++)
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.IntField("ID:", level.itemEffects[ef]);
                                            EffectsPrefabData effectData = AtavismPrefabManager.Instance.GetEffectPrefab(level.itemEffects[ef]);
                                            if (effectData != null) {
                                                EditorGUILayout.LabelField("Name:", effectData.name);
                                            }
                                            else {
                                                EditorGUILayout.LabelField("Unknown Effect");
                                            }                                            
                                            EditorGUILayout.EndHorizontal();                                            
                                        }
                                        EditorGUILayout.EndVertical();                                        
                                        EditorGUILayout.Space();
                                    }

                                    if (level.itemAbilities != null && level.itemAbilities.Count > 0)
                                    {                                        
                                        EditorGUILayout.LabelField("Abilities:");
                                        EditorGUILayout.BeginVertical();
                                        for (int ab = 0; ab < level.itemAbilities.Count; ab++)
                                        {
                                            EditorGUILayout.BeginHorizontal();
                                            EditorGUILayout.IntField("ID:", level.itemAbilities[ab]);
                                            AbilityPrefabData abilityData = AtavismPrefabManager.Instance.GetAbilityPrefab(level.itemAbilities[ab]);
                                            if (abilityData != null) {
                                                EditorGUILayout.LabelField("Name:", abilityData.name);
                                            }
                                            else {
                                                EditorGUILayout.LabelField("Unknown Ability");
                                            }
                                            EditorGUILayout.EndHorizontal();                                            
                                        }
                                        EditorGUILayout.EndVertical();                                        
                                        EditorGUILayout.Space();
                                    }

                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.Space();
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField("No assigned item");
                            }

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();        
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }

                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }

        void showSkills()
        {
            EditorGUILayout.LabelField("List of Skills " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetSkillPrefabData() != null ? AtavismPrefabManager.Instance.GetSkillPrefabData().Count : 0));
            skillSearche = EditorGUILayout.TextField("Search", skillSearche);
            EditorGUILayout.Space();
            scrollPos5 = EditorGUILayout.BeginScrollView(scrollPos5, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;

                foreach (SkillPrefabData obj in AtavismPrefabManager.Instance.GetSkillPrefabData())
                {
                    if (obj.skillname.ToLower().Contains(skillSearche.ToLower()) || skillSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.IntField("id", obj.id);
                            EditorGUILayout.TextField("name", obj.skillname);
                            EditorGUILayout.LongField("date", obj.date);
                            //  EditorGUILayout.TextField("icon name", obj.iconData);
                            EditorGUILayout.TextField("icon path", obj.iconPath);
                            Sprite icon = AtavismPrefabManager.Instance.GetSkillIconByID(obj.id);
                            
                            if(icon!=null)
                                EditorGUILayout.TextField("Icon size", icon.texture.width + "x" + icon.texture.height);
                            EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);
                            EditorGUILayout.BeginVertical();
                            for(int a =0 ; a<obj.abilities.Count;a++)
                         //   foreach (var abi in obj.abilities)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.IntField("Ability ", obj.abilities[a]);
                                EditorGUILayout.IntField("Req level ", obj.abilityLevelReqs[a]);
                                EditorGUILayout.EndHorizontal();

                            }
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space(5);
                            EditorGUILayout.EndVertical();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }

                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }

        void showAbilities()
        {
            EditorGUILayout.LabelField("List of Abilities " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetAbilityPrefabData() != null ? AtavismPrefabManager.Instance.GetAbilityPrefabData().Count : 0));
            abilitiesSearche = EditorGUILayout.TextField("Search", abilitiesSearche);
            EditorGUILayout.Space();
            scrollPos6 = EditorGUILayout.BeginScrollView(scrollPos6, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;

                foreach (AbilityPrefabData obj in AtavismPrefabManager.Instance.GetAbilityPrefabData())
                {
                    if (obj.name.ToLower().Contains(abilitiesSearche.ToLower()) || abilitiesSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.IntField("id", obj.id);
                            EditorGUILayout.TextField("name", obj.name);
                            // EditorGUILayout.TextField("icon name", obj.iconData);
                            EditorGUILayout.TextField("icon path", obj.iconPath);
                            Sprite icon  = AtavismPrefabManager.Instance.GetAbilityIconByID(obj.id);
                            
                            if(icon!=null)
                                EditorGUILayout.TextField("Icon size", icon.texture.width + "x" + icon.texture.height);
                            EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);
                            EditorGUILayout.TextField("Target Type", obj.targetType.ToString());
                            EditorGUILayout.TextField("Target Sub Type", obj.targetSubType.ToString());

                            if(obj.powerup != null)
                            foreach (var p in obj.powerup)
                            {
                                EditorGUILayout.FloatField("PowerUp time", p/1000F);  
                            }
                            EditorGUILayout.LongField("Date", obj.date);
                          
                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }

                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }

        void showEffects()
        {
            EditorGUILayout.LabelField("List of Effects " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetEffectPrefabData() != null ? AtavismPrefabManager.Instance.GetEffectPrefabData().Count : 0));
            effectsSearche = EditorGUILayout.TextField("Search", effectsSearche);
            EditorGUILayout.Space();
            scrollPos7 = EditorGUILayout.BeginScrollView(scrollPos7, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;

                foreach (EffectsPrefabData obj in AtavismPrefabManager.Instance.GetEffectPrefabData())
                {
                    if (obj.name.ToLower().Contains(effectsSearche.ToLower()) || effectsSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.IntField("id", obj.id);
                            EditorGUILayout.TextField("name", obj.name);
                            EditorGUILayout.TextField("tooltip", obj.tooltip);
                            EditorGUILayout.Toggle("allowMultiple", obj.allowMultiple);
                            EditorGUILayout.Toggle("stackTime", obj.stackTime);
                            EditorGUILayout.Toggle("show", obj.show);
                            // EditorGUILayout.TextField("icon name", obj.iconData);
                            EditorGUILayout.TextField("icon path", obj.iconPath);
                            Sprite icon = AtavismPrefabManager.Instance.GetEffectIconByID(obj.id);
                            
                            if(icon!=null)
                                EditorGUILayout.TextField("Icon size", icon.texture.width + "x" + icon.texture.height);
                            EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);
                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }

                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }

        void showBuildObjects()
        {
            EditorGUILayout.LabelField("List of Build Objects " + (AtavismPrefabManager.Instance.GetBuildObjPrefabData() != null ? AtavismPrefabManager.Instance.GetBuildObjPrefabData().Count : 0));
            buildObjSearche = EditorGUILayout.TextField("Search", buildObjSearche);
            EditorGUILayout.Space();
            scrollPos8 = EditorGUILayout.BeginScrollView(scrollPos8, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;

                foreach (BuildObjPrefabData obj in AtavismPrefabManager.Instance.GetBuildObjPrefabData())
                {
                    if (obj.buildObjectName.ToLower().Contains(buildObjSearche.ToLower()) || buildObjSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.IntField("id", obj.id);
                            EditorGUILayout.TextField("name", obj.buildObjectName);
                            EditorGUILayout.TextField("prefab", obj.gameObject);
                            // EditorGUILayout.TextField("icon name", obj.iconData);
                            EditorGUILayout.TextField("icon path", obj.iconPath);
                            Sprite icon = AtavismPrefabManager.Instance.GetBuildingObjectIconByID(obj.id);
                           
                            if(icon!=null)
                                EditorGUILayout.TextField("Icon size", icon.texture.width + "x" + icon.texture.height);
                            EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);
                            EditorGUILayout.IntField("Category", obj.category);
                            EditorGUILayout.Toggle("Only Available From Item", obj.onlyAvailableFromItem);
                            EditorGUILayout.LongField("Date", obj.date);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("List of ObjectCategory"); 
                            EditorGUILayout.BeginVertical();
                            if (obj.objectCategory.Count > 0)
                            {
                                foreach (var cat in obj.objectCategory)
                                {
                                    EditorGUILayout.IntField(cat);
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField("No assigned Category");
                            }

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("List of Valid Claim Types"); 
                            EditorGUILayout.BeginVertical();
                            if (obj.validClaimTypes.Count > 0)
                            {
                                foreach (var cat in obj.validClaimTypes)
                                {
                                    EditorGUILayout.IntField(cat);
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField("No assigned Claim Types");
                            }

                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();

                            
                            
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }

                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }
          void showResourceNodeProfiles()
        {
            EditorGUILayout.LabelField("List of Global Events " + (AtavismPrefabManager.Instance.GetResourceNodePrefabData() != null ? AtavismPrefabManager.Instance.GetResourceNodePrefabData().Count : 0));
          //  buildObjSearche = EditorGUILayout.TextField("Search", buildObjSearche);
            EditorGUILayout.Space();
            scrollPos9 = EditorGUILayout.BeginScrollView(scrollPos9, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;
                EditorGUILayout.BeginVertical();
                foreach (RecourceNodeProfileData obj in AtavismPrefabManager.Instance.GetResourceNodePrefabData())
                {
                  //  if (obj.buildObjectName.ToLower().Contains(buildObjSearche.ToLower()) || buildObjSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.IntField("profile id", obj.id);
                            EditorGUILayout.LongField("date", obj.date);
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.BeginVertical();
                            for (int s = 0; s < obj.settingList.Count; s++ )
                            {
                               
                                var setting = obj.settingList[s];
                                EditorGUILayout.Space();
                                EditorGUILayout.IntField("Settings Id", setting.id);
                                EditorGUILayout.TextField("icon path", setting.cursorIconPath);

                               Texture2D cursorIcon =  AtavismPrefabManager.Instance.GetResourceNodeCursorIconById(obj.id, setting.id); 
                               
                                if(cursorIcon!=null)
                                    EditorGUILayout.TextField("Cursor Icon size", cursorIcon.width + "x" + cursorIcon.height);
                               EditorGUILayout.ObjectField("Cursor Icon", cursorIcon, typeof(Texture2D), false);

                               EditorGUILayout.TextField("icon path", setting.selectedIconPath);
                               Sprite selectedIcon = AtavismPrefabManager.Instance.GetResourceNodeIconById(obj.id, setting.id); 
                               
                                if(selectedIcon!=null)
                                    EditorGUILayout.TextField("Selected Icon size", selectedIcon.texture.width + "x" + selectedIcon.texture.height);
                                EditorGUILayout.ObjectField("Selected Icon", selectedIcon, typeof(Sprite), false);
                               
                            }
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();

                            EditorGUILayout.Space();
                            EditorGUILayout.EndHorizontal();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }

                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }
          
        void showGlobalEvents()
        {
            EditorGUILayout.LabelField("List of Global Events " + (AtavismPrefabManager.Instance.GetGlobalEventPrefabData() != null ? AtavismPrefabManager.Instance.GetGlobalEventPrefabData().Count : 0));
          //  buildObjSearche = EditorGUILayout.TextField("Search", buildObjSearche);
            EditorGUILayout.Space();
            scrollPos10 = EditorGUILayout.BeginScrollView(scrollPos10, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;

                foreach (GlobalEventData obj in AtavismPrefabManager.Instance.GetGlobalEventPrefabData())
                {
                  //  if (obj.buildObjectName.ToLower().Contains(buildObjSearche.ToLower()) || buildObjSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.IntField("id", obj.id);
                          //  EditorGUILayout.TextField("name", obj.buildObjectName);
                          //  EditorGUILayout.TextField("prefab", obj.gameObject);
                            // EditorGUILayout.TextField("icon name", obj.iconData);
                            EditorGUILayout.TextField("icon path", obj.iconPath);
                            Sprite icon = AtavismPrefabManager.Instance.GetGlobalEventIconByID(obj.id);
                            
                            if(icon!=null)
                                EditorGUILayout.TextField("Icon size", icon.texture.width + "x" + icon.texture.height);
                            EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);
                            EditorGUILayout.LongField("Date", obj.date);

                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }

                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }
        
          void showRaces()
        {
            EditorGUILayout.LabelField("List of Racess " + (AtavismPrefabManager.Instance.GetRacesPrefabData() != null ? AtavismPrefabManager.Instance.GetRacesPrefabData().Count : 0));
          //  buildObjSearche = EditorGUILayout.TextField("Search", buildObjSearche);
            EditorGUILayout.Space();
            scrollPos11 = EditorGUILayout.BeginScrollView(scrollPos11, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;
                EditorGUILayout.BeginVertical();
                foreach (RaceData obj in AtavismPrefabManager.Instance.GetRacesPrefabData())
                {
                  //  if (obj.buildObjectName.ToLower().Contains(buildObjSearche.ToLower()) || buildObjSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.IntField("Race id", obj.id);
                            EditorGUILayout.LongField("Race date", obj.date);
                            EditorGUILayout.TextField("Race Name", obj.name);
                            EditorGUILayout.TextField("Race Desc", obj.description);
                            EditorGUILayout.TextField("icon path", obj.iconPath);
                           // EditorGUILayout.LongField("date", obj.date);
                            Sprite icon = AtavismPrefabManager.Instance.GetRaceIconByID(obj.id);
                           
                            if(icon!=null)
                                EditorGUILayout.TextField("Race Icon size", icon.texture.width + "x" + icon.texture.height);
                            EditorGUILayout.ObjectField("Race Icon", icon, typeof(Texture2D), false);
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.BeginVertical();
                            foreach (var cl in obj.classList.Values )
                            {
                                EditorGUILayout.Space();
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.BeginVertical();
                                EditorGUILayout.IntField("Class Id", cl.id);
                                EditorGUILayout.TextField("Class Name", cl.name);
                                EditorGUILayout.TextField("Class Desc", cl.description);
                                EditorGUILayout.TextField("icon path", cl.iconPath);
                                Sprite cicon = AtavismPrefabManager.Instance.GetClassIconByID(obj.id, cl.id);
                                
                                if(cicon!=null)
                                    EditorGUILayout.TextField("Class Icon size", cicon.texture.width + "x" + cicon.texture.height);
                                EditorGUILayout.ObjectField("Class Icon", cicon, typeof(Texture2D), false);
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.BeginVertical();
                                foreach (var gl in cl.genderList.Values)
                                {
                                    EditorGUILayout.BeginVertical();
                                    EditorGUILayout.IntField("Gender Id", gl.id);
                                    EditorGUILayout.TextField("Gender Name", gl.name);
                                    EditorGUILayout.TextField("Gender Prefab", gl.prefab);
                                    EditorGUILayout.TextField("icon path", gl.iconPath);

                                    Sprite gicon = AtavismPrefabManager.Instance.GetGenderIconByID(obj.id, cl.id, gl.id);
                                    
                                    if(gicon!=null)
                                        EditorGUILayout.TextField("Gender Icon size", gicon.texture.width + "x" + gicon.texture.height);
                                    EditorGUILayout.ObjectField("Gender Icon", gicon, typeof(Texture2D), false);
                                    EditorGUILayout.EndVertical();
                                }
                                EditorGUILayout.EndVertical();



                                EditorGUILayout.EndHorizontal();
                               
                            }
                            EditorGUILayout.EndVertical();
                          
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }

                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }
               void showQuests()
        {
            EditorGUILayout.LabelField("List of Quests " + (AtavismPrefabManager.Instance.GetQuestsPrefabData() != null ? AtavismPrefabManager.Instance.GetQuestsPrefabData().Count : 0));
            questSearche = EditorGUILayout.TextField("Search", questSearche);
            EditorGUILayout.Space();
            scrollPos12 = EditorGUILayout.BeginScrollView(scrollPos12, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;
                EditorGUILayout.BeginVertical();
                foreach (QuestData obj in AtavismPrefabManager.Instance.GetQuestsPrefabData())
                {
                    if (obj.title.ToLower().Contains(questSearche.ToLower()) || questSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.IntField("Quest Id", obj.id);
                            EditorGUILayout.LongField("Date", obj.date);
                            EditorGUILayout.TextField("Title", obj.title);
                            EditorGUILayout.TextField("Description", obj.description);
                            EditorGUILayout.TextField("Progress", obj.progress);
                            EditorGUILayout.TextField("Objective", obj.objective);

                           foreach (var ob in obj.objectives)
                           {
                               EditorGUILayout.TextField("Objective Text", ob.name);
                               EditorGUILayout.TextField("Objective Type", ob.type);
                               EditorGUILayout.IntField("Objective Count", ob.count);
                               
                           }
                           
                           foreach (var grade in obj.rewardsGrades.Values)
                           {
                               EditorGUILayout.TextField("Complete Text", grade.completeText);
                               EditorGUILayout.IntField("Experience", grade.exp);
                               EditorGUILayout.LabelField("Reward Items ");
                               EditorGUILayout.BeginHorizontal();
                               EditorGUILayout.BeginVertical();
                               if (grade.rewardsItems.Count == 0)
                               {
                                   EditorGUILayout.LabelField("No items ");
                               }
                               foreach (var items in grade.rewardsItems)
                               {
                                   EditorGUILayout.BeginHorizontal();
                                   EditorGUILayout.IntField("Item Id", items.Key);
                                   EditorGUILayout.IntField("Item Count", items.Value);
                                   EditorGUILayout.EndHorizontal();    
                               
                               }   
                               EditorGUILayout.EndVertical();
                               EditorGUILayout.EndHorizontal();
                               EditorGUILayout.LabelField("Reward of Choice Items ");
                               EditorGUILayout.BeginHorizontal();
                               EditorGUILayout.BeginVertical();
                               if (grade.rewardsToChooseItems.Count == 0)
                               {
                                   EditorGUILayout.LabelField("No Items to choose ");
                               }
                               foreach (var items in grade.rewardsToChooseItems)
                               {
                                   EditorGUILayout.BeginHorizontal();
                                   EditorGUILayout.IntField("Item Id", items.Key);
                                   EditorGUILayout.IntField("Item Count", items.Value);
                                   EditorGUILayout.EndHorizontal();
                               
                               }                           
                               EditorGUILayout.EndVertical();
                               EditorGUILayout.EndHorizontal();
                           
                               EditorGUILayout.LabelField("Reward Currency ");
                               EditorGUILayout.BeginHorizontal();
                               EditorGUILayout.BeginVertical();
                               if (grade.rewardsCurrency.Count == 0)
                               {
                                   EditorGUILayout.LabelField("No Currency ");
                               }
                               foreach (var items in grade.rewardsCurrency)
                               {
                                   EditorGUILayout.BeginHorizontal();
                                   EditorGUILayout.IntField("Currency Id", items.Key);
                                   EditorGUILayout.IntField("Currency Count", items.Value);
                                   EditorGUILayout.EndHorizontal();
                               
                               }                           
                               EditorGUILayout.EndVertical();
                               EditorGUILayout.EndHorizontal();

                               EditorGUILayout.LabelField("Reward Reputation ");
                               EditorGUILayout.BeginHorizontal();
                               EditorGUILayout.BeginVertical();
                               if (grade.rewardsReputation.Count == 0)
                               {
                                   EditorGUILayout.LabelField("No Reputation ");
                               }
                               foreach (var items in grade.rewardsReputation)
                               {
                                   EditorGUILayout.BeginHorizontal();
                                   EditorGUILayout.TextField("Faction", items.Key);
                                   EditorGUILayout.IntField("Reputation", items.Value);
                                   EditorGUILayout.EndHorizontal();
                               
                               }                           
                               EditorGUILayout.EndVertical();
                               EditorGUILayout.EndHorizontal();
                            }
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }
                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }

               void showWeaponProfiles()
               {
                   EditorGUILayout.LabelField("List of Weapon Profiles " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetWeaponProfilePrefabData() != null ? AtavismPrefabManager.Instance.GetWeaponProfilePrefabData().Count : 0));
                   weaponProfileSearche = EditorGUILayout.TextField("Search", weaponProfileSearche);
                   EditorGUILayout.Space();
                   scrollPos14 = EditorGUILayout.BeginScrollView(scrollPos14, GUILayout.ExpandHeight(true));
                   if (AtavismPrefabManager.Instance != null)
                   {
                       int i = 0;
                       foreach (WeaponProfileData obj in AtavismPrefabManager.Instance.GetWeaponProfilePrefabData())
                       {

                           if (obj.name.ToLower().Contains(weaponProfileSearche.ToLower()) || weaponProfileSearche.Equals(""))
                               if (i < limit || limit == -1)
                               {

                                   EditorGUILayout.IntField("id", obj.id);
                                   EditorGUILayout.TextField("name", obj.name);
                                   int k = 1;
                                   foreach (var a in obj.actions)
                                   {
                                       EditorGUILayout.BeginHorizontal();
                                       EditorGUILayout.LabelField("Action #" + (k++));
                                       EditorGUILayout.BeginVertical();
                                       EditorGUILayout.EndVertical();
                                       EditorGUILayout.BeginVertical();
                                       EditorGUILayout.Space();
                                       EditorGUILayout.TextField("Action", a.action);
                                       EditorGUILayout.TextField("Slot", a.slot);
                                       EditorGUILayout.IntField("Ability", a.abilityId);
                                       EditorGUILayout.Toggle("Zoom", a.zoom);
                                       EditorGUILayout.TextField("CoordEffect", a.coordEffect);
                                       EditorGUILayout.EndVertical();
                                       EditorGUILayout.EndHorizontal();
                                   }
                                   EditorGUILayout.Space();
                                   i++;
                               }
                               else
                               {
                                   EditorGUILayout.LabelField("The result display limit has been reached");
                                   break;
                               }
                       }

                       if (i == 0)
                           EditorGUILayout.LabelField("There are no result to display");
                   }
                   else
                   {
                       EditorGUILayout.LabelField("Enter play mode to load data ");
                   }

                   EditorGUILayout.EndScrollView();
               }
               
                void showStats()
        {
            EditorGUILayout.LabelField("List of Stats " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetStatsData() != null ? AtavismPrefabManager.Instance.GetStatsData().Count : 0));
            statsSearche = EditorGUILayout.TextField("Search", statsSearche);
            EditorGUILayout.Space();
            scrollPos16 = EditorGUILayout.BeginScrollView(scrollPos16, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;
                foreach (StatProfileData obj in AtavismPrefabManager.Instance.GetStatsData())
                {
                    if (obj.name.ToLower().Contains(statsSearche.ToLower()) || statsSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.IntField("id", obj.id);
                            EditorGUILayout.TextField("name", obj.name);
                            EditorGUILayout.IntField("precision", obj.precision);
                            EditorGUILayout.LongField("Date", obj.date);
                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }
                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }
                
       void showInteractiveObjectProfiles()
        {
            EditorGUILayout.LabelField("List of Interactive Object Profiles " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetInteractiveObjectProfilePrefabData() != null ? AtavismPrefabManager.Instance.GetInteractiveObjectProfilePrefabData().Count : 0));
            interactiveObjectProfileSearche = EditorGUILayout.TextField("Search", interactiveObjectProfileSearche);
            EditorGUILayout.Space();
            scrollPos17 = EditorGUILayout.BeginScrollView(scrollPos17, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;
                foreach (InteractiveObjectProfileData obj in AtavismPrefabManager.Instance.GetInteractiveObjectProfilePrefabData())
                {
                    if (obj.name.ToLower().Contains(interactiveObjectProfileSearche.ToLower()) || interactiveObjectProfileSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.IntField("id", obj.id);
                            EditorGUILayout.TextField("name", obj.name);
                            EditorGUILayout.TextField("interactionType", obj.interactionType);
                            
                            EditorGUILayout.IntField("useLimit", obj.useLimit);
                            EditorGUILayout.IntField("Quest Requirement", obj.questRequirement);
                            EditorGUILayout.IntField("Min Level", obj.minLevel);
                            EditorGUILayout.IntField("Max Level", obj.maxLevel);

                            EditorGUILayout.IntField("Item Requirement", obj.itemRequirement);
                            EditorGUILayout.IntField("Item Count Requirement", obj.itemCountRequirement);
                            EditorGUILayout.Toggle("Take item", obj.itemRequirementGet);

                            EditorGUILayout.IntField("Currency Requirement", obj.currencyRequirement);
                            EditorGUILayout.IntField("Currency Count Requirement", obj.currencyCountRequirement);
                            EditorGUILayout.Toggle("Take Currency", obj.currencyRequirementGet);

                            
                            EditorGUILayout.TextField("Cursor path", obj.iconPath);
                            Texture2D icon = AtavismPrefabManager.Instance.GetIcon2d(obj.iconPath);
                            if(icon!=null)
                                EditorGUILayout.TextField("Cursor size", icon.width + "x" + icon.height);
                            EditorGUILayout.ObjectField("Cursor", icon, typeof(Texture2D), false);

                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }
                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }

        void showSlotsProfiles()
        {
            EditorGUILayout.LabelField("List of Slots Profiles " + (AtavismPrefabManager.Instance != null && AtavismPrefabManager.Instance.GetSlotsProfilePrefabData() != null ? AtavismPrefabManager.Instance.GetSlotsProfilePrefabData().Count : 0));
            slotsProfileSearche = EditorGUILayout.TextField("Search", slotsProfileSearche);
            EditorGUILayout.Space();
            scrollPos18 = EditorGUILayout.BeginScrollView(scrollPos18, GUILayout.ExpandHeight(true));
            if (AtavismPrefabManager.Instance != null)
            {
                int i = 0;
                foreach (SlotsProfileData obj in AtavismPrefabManager.Instance.GetSlotsProfilePrefabData())
                {
                    if (obj.name.ToLower().Contains(slotsProfileSearche.ToLower()) || slotsProfileSearche.Equals(""))
                        if (i < limit || limit == -1)
                        {
                            EditorGUILayout.IntField("id", obj.id);
                            EditorGUILayout.TextField("name", obj.name);
                            
                            
                            EditorGUILayout.BeginVertical();
                            if (obj.slots.Count == 0)
                            {
                                EditorGUILayout.LabelField("No Slots");
                            }
                            foreach (var slot in obj.slots)
                            {
                                EditorGUILayout.TextField("Slot ", slot);
                            }                           
                            EditorGUILayout.EndVertical();
                            
                            

                            EditorGUILayout.Space();
                            i++;
                        }
                        else
                        {
                            EditorGUILayout.LabelField("The result display limit has been reached");
                            break;
                        }
                }
                if (i == 0)
                    EditorGUILayout.LabelField("There are no result to display");
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to load data ");
            }
            EditorGUILayout.EndScrollView();
        }  
    }
}