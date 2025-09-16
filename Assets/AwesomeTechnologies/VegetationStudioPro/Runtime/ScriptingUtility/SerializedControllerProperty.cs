using System;
using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.VegetationSystem
{
    [Serializable]
    public enum SerializedControlerPropertyType
    {
        Integer,
        Float,
        RgbaSelector,
        ColorSelector,
        Boolean,
        DropDownStringList,
        Label,
        Texture,
        Texture2D
    }

    [Serializable]
    public class SerializedControllerProperty
    {
        public SerializedControlerPropertyType SerializedControlerPropertyType;
        public string PropertyName;
        public string PropertyDescription;
        public string PropertyInfo;
        public int IntValue;
        public int IntMinValue;
        public int IntMaxValue;
        public float FloatValue;
        public float FloatMinValue;
        public float FloatMaxValue;
        public Color ColorValue;
        public bool BoolValue;
        public Texture TextureValue;
        public Texture2D Texture2DValue;
        public List<string> StringList = new();

        public SerializedControllerProperty()
        {

        }

        public SerializedControllerProperty(SerializedControllerProperty _original)
        {
            SerializedControlerPropertyType = _original.SerializedControlerPropertyType;
            PropertyName = _original.PropertyName;
            PropertyDescription = _original.PropertyDescription;
            PropertyInfo = _original.PropertyInfo;
            IntValue = _original.IntValue;
            IntMinValue = _original.IntMinValue;
            IntMaxValue = _original.IntMaxValue;
            FloatValue = _original.FloatValue;
            FloatMinValue = _original.FloatMinValue;
            FloatMaxValue = _original.FloatMaxValue;
            ColorValue = _original.ColorValue;
            BoolValue = _original.BoolValue;
            Texture2DValue = _original.Texture2DValue;
            StringList.AddRange(_original.StringList);
        }
    }
}