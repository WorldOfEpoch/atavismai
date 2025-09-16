using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationStudio;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Masks/VegetationBeacon", 1)]
    [ScriptExecutionOrder(99)]
    [ExecuteInEditMode]
    public class VegetationBeacon : MonoBehaviour
    {
        public bool RemoveGrass;
        public bool RemovePlants;
        public bool RemoveObjects;
        public bool RemoveLargeObjects;
        public bool RemoveTrees;
        [Range(0.1f, 150)] public float Radius = 12.5f;
        [Range(0, 1)] public float blendFactor = 0.5f;
        public AnimationCurve FalloffCurve = new(new Keyframe[] { new(0, 1, 0, 0), new(1, 0, 0, 0) });
        public List<VegetationTypeSettings> VegetationTypeList = new();

        private float3 lastPosition;
        private BeaconMaskArea currentMaskArea;
        private bool isDirty;

        void OnEnable()
        {
            isDirty = true;
        }

        void Update()
        {
#if !UNITY_EDITOR
            if (Application.isPlaying && isDirty == false)
                return;
#endif

            if (isDirty || lastPosition.Equals(transform.position) == false)
            {
                UpdateVegetationBeacon();
                lastPosition = transform.position;
                isDirty = false;
            }
        }

        void OnDisable()
        {
            if (currentMaskArea != null)
            {
                VegetationStudioManager.RemoveVegetationMask(currentMaskArea);
                currentMaskArea = null;
            }
        }

        public void AddVegetationTypes(BaseMaskArea _maskArea)
        {
            for (int i = 0; i < VegetationTypeList.Count; i++)
                _maskArea.VegetationTypeList.Add(new VegetationTypeSettings(VegetationTypeList[i]));
        }

        public void UpdateVegetationBeacon()
        {
            if (enabled == false || gameObject.activeSelf == false)
                return;

            BeaconMaskArea maskArea = new()
            {
                RemoveGrass = RemoveGrass,
                RemovePlants = RemovePlants,
                RemoveObjects = RemoveObjects,
                RemoveLargeObjects = RemoveLargeObjects,
                RemoveTrees = RemoveTrees,

                Radius = Radius,
                blendFactor = blendFactor,
                Position = transform.position
            };

            maskArea.SetFalloffCurve(FalloffCurve.GenerateCurveArray(4096));
            maskArea.Init();
            AddVegetationTypes(maskArea);

            if (currentMaskArea != null)
            {
                VegetationStudioManager.RemoveVegetationMask(currentMaskArea);
                currentMaskArea = null;
            }

            currentMaskArea = maskArea;
            VegetationStudioManager.AddVegetationMask(maskArea);
        }
    }
}