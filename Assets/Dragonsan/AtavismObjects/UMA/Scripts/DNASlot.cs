using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DNASlot : MonoBehaviour {

	public string slot;
	public List<RacialAlternatives> alternatives = new List<RacialAlternatives>();
	public Slider slider;
	public DNASliderPanel panel;

    private void Start()
    {
        slider = gameObject.GetComponent<Slider>();
        //slider.maxValue = alternatives.Count -1;
        int max = 0;

        for (int i = 0; i < alternatives.Count; i++)
        {
            foreach (RaceRecipe raceRecipe in alternatives[i].raceRecipes)
            {
                if (raceRecipe.race == panel.Avatar.activeRace.name)
                {
                    max = i;
                }
            }
        }
        slider.maxValue = max;
    }
    // This function is called when the object becomes enabled and active.
    protected void OnEnable()
	{
		slider = gameObject.GetComponent<Slider>();
		//slider.maxValue = alternatives.Count -1;
        int max = 0;

        for (int i = 0; i < alternatives.Count; i++)
        {
            foreach (RaceRecipe raceRecipe in alternatives[i].raceRecipes)
            {
                if (raceRecipe.race == panel.Avatar.activeRace.name)
                {
                    max = i;
                }
            }
        }
        slider.maxValue = max;
    }
	
	public void ValueChanged(float val)
	{
		int selection = Mathf.FloorToInt(val);
        if(alternatives.Count>selection)
		foreach(RaceRecipe raceRecipe in alternatives[selection].raceRecipes)
		{
			if(raceRecipe.race == panel.Avatar.activeRace.name)
			{
                    panel.AdjustSlot(slot, raceRecipe.recipe);
               
			}
		}
	}
}

[System.Serializable]
public class RacialAlternatives
{
	public List<RaceRecipe> raceRecipes;
}


