using AwesomeTechnologies.Utility;
using AwesomeTechnologies.VegetationStudio;
using AwesomeTechnologies.VegetationSystem;
using UnityEngine;

namespace AwesomeTechnologies.Vegetation
{
    [AddComponentMenu("AwesomeTechnologies/VegetationStudioPro/Masks/VegetationItemMask", 4)]
    [ScriptExecutionOrder(99)]
    [ExecuteInEditMode]
    public class VegetationItemMask : MonoBehaviour
    {
        public Vector3 Position;
        public VegetationType VegetationType;
        public string _vegetationItemID = "";
        private CircleMaskArea _currentMaskArea;

        public string VegetationMaskID;
        private bool isDirty;

        void OnEnable()
        {
            if (VegetationMaskID == "")
                VegetationMaskID = System.Guid.NewGuid().ToString();

            isDirty = true;
        }

        void Update()
        {
            if (isDirty == false)
                return;

            UpdateVegetationItemMask();
            isDirty = false;
        }

        void OnDisable()
        {
            if (_currentMaskArea != null)
            {
                VegetationStudioManager.RemoveVegetationMask(_currentMaskArea);
                _currentMaskArea = null;
            }
        }

        public void SetDirty()
        {
            isDirty = true;
        }

        public void SetVegetationItemInstanceInfo(VegetationItemInstanceInfo _vegetationItemInstanceInfo)
        {
            Position = _vegetationItemInstanceInfo.Position;
            VegetationType = _vegetationItemInstanceInfo.VegetationType;
            _vegetationItemID = _vegetationItemInstanceInfo.VegetationItemID;
            isDirty = true;
        }

        public void SetVegetationItemInstanceInfo(Vector3 _position, VegetationType _vegetationType)
        {
            Position = _position;
            VegetationType = _vegetationType;
            //_vegetationItemID = "missing" => // less performant since non-specific cell cache clearing afterwards
            isDirty = true;
        }

        public void SetVegetationItemInstanceInfo(Vector3 _position, VegetationType _vegetationType, string _vegetationItemID)
        {
            Position = _position;
            VegetationType = _vegetationType;
            this._vegetationItemID = _vegetationItemID;
            isDirty = true;
        }

        void SetRemoveVegetationTypes(CircleMaskArea _circleMaskArea)
        {
            _circleMaskArea.RemoveGrass = (VegetationType == VegetationType.Grass);
            _circleMaskArea.RemovePlants = (VegetationType == VegetationType.Plant);
            _circleMaskArea.RemoveTrees = (VegetationType == VegetationType.Tree);
            _circleMaskArea.RemoveObjects = (VegetationType == VegetationType.Objects);
            _circleMaskArea.RemoveLargeObjects = (VegetationType == VegetationType.LargeObjects);
        }

        private void UpdateVegetationItemMask()
        {
            if (enabled == false || gameObject.activeSelf == false)
                return;

            CircleMaskArea maskArea = new() { Radius = 0.0f, Position = Position, VegetationItemID = _vegetationItemID };
            maskArea.Init();
            maskArea.VegetationType = VegetationType;
            SetRemoveVegetationTypes(maskArea);

            if (_currentMaskArea != null)
            {
                VegetationStudioManager.RemoveVegetationMask(_currentMaskArea);
                _currentMaskArea = null;
            }

            _currentMaskArea = maskArea;
            VegetationStudioManager.AddVegetationMask(maskArea);
        }
    }
}