using AwesomeTechnologies.VegetationSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AwesomeTechnologies.Utility
{
    [Serializable]
    public class BaseControllerSettings
    {
        [FormerlySerializedAs("ControlerPropertyList")] public List<SerializedControllerProperty> controllerPropertyList = new();

        public bool HasPropertyValue(string _propertyName)
        {
            for (int i = 0; i < controllerPropertyList.Count; i++)
                if (controllerPropertyList[i].PropertyName == _propertyName)
                    return true;
            return false;
        }

        public void AddLabelProperty(string _text)
        {
            SerializedControllerProperty labelSerializedControllerProperty = new()
            {
                SerializedControlerPropertyType = SerializedControlerPropertyType.Label,
                PropertyName = _text,
                PropertyDescription = _text
            };
            controllerPropertyList.Add(labelSerializedControllerProperty);
        }

        public void AddFloatProperty(string _id, string _description, string _info, float _defaultValue, float _minValue, float _maxValue)
        {
            SerializedControllerProperty serializedControllerProperty = new()
            {
                SerializedControlerPropertyType = SerializedControlerPropertyType.Float,
                FloatValue = _defaultValue,
                FloatMinValue = _minValue,
                FloatMaxValue = _maxValue,
                PropertyName = _id,
                PropertyDescription = _description,
                PropertyInfo = _info
            };
            controllerPropertyList.Add(serializedControllerProperty);
        }

        public void AddBooleanProperty(string _id, string _description, string _info, bool _defaultValue)
        {
            SerializedControllerProperty serializedControllerProperty = new()
            {
                SerializedControlerPropertyType = SerializedControlerPropertyType.Boolean,
                BoolValue = _defaultValue,
                PropertyName = _id,
                PropertyInfo = _info,
                PropertyDescription = _description
            };
            controllerPropertyList.Add(serializedControllerProperty);
        }

        public void AddColorProperty(string _id, string _description, string _info, Color _defaultValue)
        {
            SerializedControllerProperty serializedControllerProperty = new()
            {
                SerializedControlerPropertyType = SerializedControlerPropertyType.ColorSelector,
                ColorValue = _defaultValue,
                PropertyName = _id,
                PropertyInfo = "",
                PropertyDescription = _description
            };
            controllerPropertyList.Add(serializedControllerProperty);
        }

        public void AddTextureProperty(string _id, string _description, string _info, Texture _defaultValue)
        {
            SerializedControllerProperty serializedControllerProperty = new()
            {
                SerializedControlerPropertyType = SerializedControlerPropertyType.Texture,
                TextureValue = _defaultValue,
                PropertyName = _id,
                PropertyDescription = _description,
                PropertyInfo = _info

            };
            controllerPropertyList.Add(serializedControllerProperty);
        }

        public void AddTextureProperty(string _id, string _description, string _info, Texture2D _defaultValue)
        {
            SerializedControllerProperty serializedControllerProperty = new()
            {
                SerializedControlerPropertyType = SerializedControlerPropertyType.Texture2D,
                Texture2DValue = _defaultValue,
                PropertyName = _id,
                PropertyDescription = _description,
                PropertyInfo = _info

            };
            controllerPropertyList.Add(serializedControllerProperty);
        }

        public void AddRgbaSelectorProperty(string _id, string _description, string _info, int _defaultValue)
        {
            SerializedControllerProperty serializedControllerProperty = new()
            {
                SerializedControlerPropertyType = SerializedControlerPropertyType.RgbaSelector,
                IntValue = _defaultValue,
                PropertyName = _id,
                PropertyDescription = _description,
                PropertyInfo = _info
            };
            controllerPropertyList.Add(serializedControllerProperty);
        }

        public int GetIntPropertyValue(string _propertyName)
        {
            for (int i = 0; i < controllerPropertyList.Count; i++)
                if (controllerPropertyList[i].PropertyName == _propertyName)
                    return controllerPropertyList[i].IntValue;
            return 0;
        }

        public float GetFloatPropertyValue(string _propertyName)
        {
            for (int i = 0; i < controllerPropertyList.Count; i++)
                if (controllerPropertyList[i].PropertyName == _propertyName)
                    return controllerPropertyList[i].FloatValue;
            return 0;
        }

        public bool GetBooleanPropertyValue(string _propertyName)
        {
            for (int i = 0; i < controllerPropertyList.Count; i++)
                if (controllerPropertyList[i].PropertyName == _propertyName)
                    return controllerPropertyList[i].BoolValue;
            return false;
        }

        public Color GetColorPropertyValue(string _propertyName)
        {
            for (int i = 0; i < controllerPropertyList.Count; i++)
                if (controllerPropertyList[i].PropertyName == _propertyName)
                    return controllerPropertyList[i].ColorValue;
            return Color.white;
        }

        public Texture GetTexturePropertyValue(string _propertyName)
        {
            for (int i = 0; i < controllerPropertyList.Count; i++)
                if (controllerPropertyList[i].PropertyName == _propertyName)
                    return controllerPropertyList[i].TextureValue;
            return null;
        }

        public Texture2D GetTexture2DPropertyValue(string _propertyName)
        {
            for (int i = 0; i < controllerPropertyList.Count; i++)
                if (controllerPropertyList[i].PropertyName == _propertyName)
                    return controllerPropertyList[i].Texture2DValue;
            return null;
        }
    }
}