using UnityEngine;
using UMA.CharacterSystem;

public class AtavismUMAItem : MonoBehaviour
{
    public UMAWardrobeRecipe wardrobeRecipe;
    public string wardrobeSlot = "Chest";

    public void ApplyToAvatar(DynamicCharacterAvatar dca)
    {
        if (dca == null || wardrobeRecipe == null) return;

        // UMA DCS: equip a wardrobe recipe
        dca.SetSlot(wardrobeRecipe);
        dca.BuildCharacter();
    }

    public void RemoveFromAvatar(DynamicCharacterAvatar dca)
    {
        if (dca == null) return;

        // UMA DCS: clear the wardrobe slot by name (eg. "Chest", "Head", "Hands")
        dca.ClearSlot(wardrobeSlot);
        dca.BuildCharacter();
    }
}
