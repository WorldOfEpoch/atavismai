using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;
using TMPro;
using UnityEngine.Events;

public class DNASliderPanel : MonoBehaviour
{
    public GameObject dnaSliderPrefab;
    public List<DNADisplay> dnaList;
    public List<DNADisplayColors> dnaColorsList;
    public List<DNADisplaySlots> dnaSlotsList;

    private List<DNASlider> sliders = new List<DNASlider>();

    DynamicCharacterAvatar avatar;
    private Dictionary<string, DnaSetter> dna;

    protected void Start()
    {
        UpdateAvatar();
    }

    protected void Update()
    {
        if (avatar == null)
        {
            UpdateAvatar();
        }
    }

    private void PopulateSliders()
    {
        foreach (DNADisplay display in dnaList)
        {
            foreach (string compatibleRace in display.CompatibleRaces)
            {
                if (avatar.activeRace.name == compatibleRace)
                {
                    DNASlider slider = GameObject.Instantiate(dnaSliderPrefab, transform).GetComponent<DNASlider>();
                    slider.label.text = display.displayedName;
                    slider.dna = display.dnaName;
                    slider.panel = this;
                    sliders.Add(slider);
                }
            }
        }
    }

    private void ClearSliders()
    {

        foreach (DNASlider slider in sliders)
        {
            Destroy(slider.gameObject);
        }

        sliders = new List<DNASlider>();
    }

    public void UpdateAvatar()
    {
        if(avatar == null)
            avatar = FindObjectOfType<DynamicCharacterAvatar>();
        if (avatar != null)
        {
            ClearSliders();
            PopulateSliders();
        }
    }

    public void AdjustDNA(string name, float val)
    {
        if (avatar != null)
        {
            dna = avatar.GetDNA();
                if(dna.ContainsKey(name))
                    dna[name].Set(val);
                else
                    Debug.LogWarning("Cant find " + name + " in dna of the race " + avatar.activeRace.name );
            avatar.BuildCharacter();
        }
    }

    public void AdjustColor(string sharedColor, Color col)
    {
        if (avatar != null)
        {
            avatar.SetColor(sharedColor, col);
            avatar.UpdateColors(true);
        }
    }

    public void AdjustSlot(string slot, string recipe)
    {
        if (avatar != null)
        {
            Debug.Log(slot + " " + recipe);
            if (recipe == "")
            {
                avatar.ClearSlot(slot);
                avatar.BuildCharacter();
            }
            else
            {
                avatar.SetSlot(slot, recipe);
                avatar.BuildCharacter();
            }

        }
    }

    public void Randomize()
    {
        foreach (DNASlider sl in sliders)
        {
            if (sl != null)
            {
                if (sl.slider != null)
                {
                    float val = UnityEngine.Random.Range(sl.slider.minValue, sl.slider.maxValue);
                    sl.slider.value = val;
                    sl.ValueChanged(val);
                }
                else
                    Debug.LogError("dna 1");
            }
            else
                Debug.LogError("dna 2");
        }
        foreach (DNADisplayColors col in dnaColorsList)
        {
            if (col.colors.Count > 0)
            {
                int val = UnityEngine.Random.Range(0, col.colors.Count - 1);
                if (val < col.colors.Count)
                {
                    if (col.colors[val] != null)
                        col.colors[val].ColorClicked();
                }
            }
        }
        foreach (DNADisplaySlots sl in dnaSlotsList)
        {
            if (sl.Slot != null)
            {
                if (sl.Slot.slider != null)
                {
                    int val = Mathf.FloorToInt(UnityEngine.Random.Range(sl.Slot.slider.minValue, sl.Slot.slider.maxValue));
                    sl.Slot.slider.value = val;
                    sl.Slot.ValueChanged(val);
                }
            }
        }
    }

    public DynamicCharacterAvatar Avatar
    {
        get
        {   if(avatar==null)
                avatar = FindObjectOfType<DynamicCharacterAvatar>();
            return avatar;
        }
        set 
        {
            avatar = value;
        }
    }

}

[System.Serializable]
public class DNADisplay
{
    public string dnaName;
    public string displayedName;
    public List<string> CompatibleRaces;
}

[System.Serializable]
public class DNADisplayColors
{
    public string colorType;
    public List<DNAColor> colors = new List<DNAColor>();
}
[System.Serializable]
public class DNADisplaySlots
{
    public string slotType;
    public DNASlot Slot;
}


