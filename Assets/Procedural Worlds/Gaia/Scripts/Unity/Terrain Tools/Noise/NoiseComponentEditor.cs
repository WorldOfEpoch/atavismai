#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
// TerrainAPI has moved out of the experimental namespace in newer Unity versions.
// Use the non‑experimental TerrainTools namespace instead.
using UnityEngine.TerrainTools;

namespace Gaia
{
    [CustomEditor(typeof(NoiseComponent))]
    public class NoiseComponentEditor : Editor
    {

    }
}
#endif