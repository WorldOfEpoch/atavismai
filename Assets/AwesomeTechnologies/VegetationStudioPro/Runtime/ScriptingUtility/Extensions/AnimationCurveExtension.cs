using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public static class AnimationCurveExtension
    {
        public static float[] GenerateCurveArray(this AnimationCurve _self, int _sampleCount)
        {
            float[] returnArray = new float[_sampleCount];
            for (int j = 0; j < _sampleCount; j++)
                returnArray[j] = _self.Evaluate(j / (float)_sampleCount);
            return returnArray;
        }
    }
}