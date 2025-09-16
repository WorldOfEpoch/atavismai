using System;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [Serializable]
    public class EnvironmentSettings
    {
        public float snowAmount = 0;
        public float snowMinimumVariation = 0;
        public float snowBlendPower = 1;
        public float billboardSnowBlendPower = 1;
        public float snowMinHeight = 150;
        public float snowMinHeightVariation = 25;
        public float snowMinHeightBlendPower = 0.5f;

        public Color snowColor = new(0.75f, 0.75f, 0.75f, 1);
        public Color snowSpecularColor = new(0.2f, 0.2f, 0.2f, 0.25f);

        public float rainAmount = 0;
    }
}