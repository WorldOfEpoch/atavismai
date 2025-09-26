using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InfinityPBR.Demo
{
    public class ScorpionDemo : InfinityDemoCharacter
    {
        [Header("Texture Buttons")]
        public Button[] textureButtons;
        
        public void RandomizeTextures() => textureButtons[Random.Range(0, textureButtons.Length)].onClick.Invoke();
        
    }
}