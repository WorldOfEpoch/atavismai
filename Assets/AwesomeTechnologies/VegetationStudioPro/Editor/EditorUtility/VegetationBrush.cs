using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    internal class VegetationBrush
    {
        public int size;
        private float[] strength;
        private Texture2D brush;

        public bool Load(Texture2D _brushTex, int _size)
        {
            if (brush == _brushTex && _size == size && strength != null)
                return true;    // return since up to date

            if (_brushTex != null)  // built preview nodes structure from given data
            {
                float num = _size;
                size = _size;
                strength = new float[size * size];

                if (size > 3)
                {
                    for (int i = 0; i < size; i++)
                        for (int j = 0; j < size; j++)
                            strength[i * size + j] = _brushTex.GetPixelBilinear((j + 0.5f) / num, i / num).a;
                }
                else
                {
                    for (int k = 0; k < strength.Length; k++)
                        strength[k] = 1f;
                }

                return brush = _brushTex;
            }
            else
            {
                strength = new float[1];
                strength[0] = 1f;
                size = 1;
                return false;
            }
        }
    }
}