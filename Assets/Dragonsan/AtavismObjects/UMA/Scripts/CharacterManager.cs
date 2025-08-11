using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;
using Atavism;

public class CharacterManager : MonoBehaviour 
{
	[HideInInspector]
	public DynamicCharacterAvatar avatar;
	private List<SlotRequest> addRequests = new List<SlotRequest>();
    private List<SlotRequest> removeRequests = new List<SlotRequest>();
	private bool dirty;
	private bool ready = false;
	
	protected void OnEnable()
	{
		avatar = gameObject.GetComponent<DynamicCharacterAvatar>();
		avatar.CharacterCreated.AddListener(Ready);
	}
	
	protected void OnDisable()
	{
		avatar.CharacterCreated.RemoveListener(Ready);
	}
	
	void Ready(UMAData data)
	{
		ready = true;
	}

	void Start() 
	{
		//addRequests = new List<SlotRequest>();
       // removeRequests = new List<SlotRequest>();
        GrabRecipe();
	}
	
	public void RequestSlot(string slot, string recipe)
	{
		SlotRequest request = new SlotRequest();
		request.slot = slot;
		request.recipe = recipe;
        if (request.recipe != "")
            addRequests.Add(request);
        else
            removeRequests.Add(request);
		dirty = true;
	}
	
	protected void LateUpdate()
	{
		if(dirty && ready)
		{
            foreach (SlotRequest req in removeRequests)
                    avatar.ClearSlot(req.slot);
                       
            foreach (SlotRequest req in addRequests)
					avatar.SetSlot(req.slot, req.recipe);
			
			dirty = false;
			addRequests.Clear();
            removeRequests.Clear();
			avatar.BuildCharacter();
			
		}
	}
	
	void GrabRecipe()
	{
		AtavismNode atavismNode = GetComponent<AtavismNode>();
		if(atavismNode != null)
		{
			if(atavismNode.PropertyExists("umaData"))
			{	
				//Debug.LogError("GrabRecipe");
				Dictionary<string, object> umaDictionary = (Dictionary<string, object>)atavismNode.GetProperty("umaData");
				avatar.LoadFromRecipeString((string)umaDictionary["CurrentRecipe"]);
			}
		}
	}
}

public class SlotRequest
{
	public string slot;
	public string recipe;
}
