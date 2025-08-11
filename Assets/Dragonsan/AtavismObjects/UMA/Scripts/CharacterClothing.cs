using System.Collections;
using System.Collections.Generic;
using Atavism;
using UnityEngine;
using UMA.CharacterSystem;

public class CharacterClothing : MonoBehaviour {

	public string targetSlot;
	public List<RaceRecipe> recipeList;
	
	[HideInInspector]
	public CharacterManager manager;

    private string attachedRecipe;

	private void Start()
	{
		if (manager == null)
		{
			manager = transform.parent.gameObject.GetComponent<CharacterManager>();
			if (manager != null)
			{
				//Debug.LogWarning("CharacterManager found");

				foreach (RaceRecipe raceRecipe in recipeList)
				{
					if (manager.avatar.activeRace.name == raceRecipe.race)
					{

						manager.RequestSlot(targetSlot, raceRecipe.recipe);
						attachedRecipe = raceRecipe.recipe;
					}
				}
			}
			else
			{
			//	Debug.LogWarning("CharacterManager not found");
			}
		}
	}

	public void SetRootGameObject(GameObject go)
	{
		if (manager == null)
		{
			manager = go.GetComponent<CharacterManager>();
			if (manager != null)
			{
			//	Debug.LogWarning("CharacterManager found");
				foreach (RaceRecipe raceRecipe in recipeList)
				{
					if (manager.avatar.activeRace.name == raceRecipe.race)
					{

						manager.RequestSlot(targetSlot, raceRecipe.recipe);
						attachedRecipe = raceRecipe.recipe;
					}
				}
			}
			else
			{
				//	Debug.LogWarning("CharacterManager not found");
			}
		}
	}

	protected void OnDestroy()
	{
		if (manager != null)
		{
			//Debug.LogWarning("CharacterManager found");

			manager.RequestSlot(targetSlot,"");
		}	else
		{
		//	Debug.LogWarning("CharacterManager not found");
		}
	}
	
}

[System.Serializable]
public class RaceRecipe
{
	public string race;
	public string recipe;
}
