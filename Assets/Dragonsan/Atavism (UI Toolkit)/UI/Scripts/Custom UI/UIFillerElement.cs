using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Atavism.UI
{
    public class UIFillerElement : VisualElement, INotifyValueChanged<float>
    {
        protected float m_value = float.NaN;

        public void SetValueWithoutNotify(float newValue)
        {
            m_value = newValue;
            radial.MarkDirtyRepaint();
        }

        public float value
        {
            get
            {
                m_value = Mathf.Clamp(m_value, 0, 1);
                return m_value;
            }
            set
            {
                if (EqualityComparer<float>.Default.Equals(this.m_value, value))
                    return;
                if (this.panel != null)
                {
                    using (ChangeEvent<float> pooled = ChangeEvent<float>.GetPooled(this.m_value, value))
                    {
                        pooled.target = (IEventHandler)this;
                        this.SetValueWithoutNotify(value);
                        this.SendEvent((EventBase)pooled);
                    }
                }
                else
                {
                    this.SetValueWithoutNotify(value);
                }
            }
        }

        public Color fillColor { get; set; }
        public float angleOffset { get; set; }
        public bool fillRadial { get; set; }

        // public string overlayImagePath { get; set; }

        public enum FillDirection
        {
            Clockwise,
            AntiClockwise
        }

        public FillDirection fillDirection { get; set; }
        // private float m_overlayImageScale;

        // public float overlayImageScale
        // {
        //     get
        //     {
        //         m_overlayImageScale = Mathf.Clamp(m_overlayImageScale, 0, 1);
        //         return m_overlayImageScale;
        //     }
        //     set => m_overlayImageScale = value;
        // }

        private float radius => (layout.width > layout.height) ? layout.width / 2 : layout.height / 2;

        public VisualElement radial;
        public VisualElement overlay;

        public new class UxmlFactory : UxmlFactory<UIFillerElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription m_value = new UxmlFloatAttributeDescription()
                { name = "value", defaultValue = 1f };

            UxmlFloatAttributeDescription m_angleOffset = new UxmlFloatAttributeDescription()
                { name = "angle-offset", defaultValue = 0 };

            UxmlColorAttributeDescription m_fillColor = new UxmlColorAttributeDescription()
                { name = "fill-color", defaultValue = Color.white };

            UxmlEnumAttributeDescription<FillDirection> m_fillDirection =
                new UxmlEnumAttributeDescription<FillDirection>()
                    { name = "fill-direction", defaultValue = 0 };

          //  private UxmlBoolAttributeDescription m_radialFill = new UxmlBoolAttributeDescription()
           //     { name = "fill-radial", defaultValue = false };
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }

            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as UIFillerElement;

                // Assigning uxml attributes to c# properties
                ate.value = m_value.GetValueFromBag(bag, cc);
                ate.fillColor = m_fillColor.GetValueFromBag(bag, cc);
                ate.angleOffset = m_angleOffset.GetValueFromBag(bag, cc);
                ate.fillDirection = m_fillDirection.GetValueFromBag(bag, cc);
              //  ate.fillRadial = m_radialFill.GetValueFromBag(bag, cc);
                ate.Clear();

                VisualElement boundary = new VisualElement() ;
                boundary.Add(ate.radial);
                ate.radial.Add(ate.overlay);
                ate.radial.style.flexGrow = 1;
                ate.overlay.style.flexGrow = 1;
                boundary.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
                boundary.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
                boundary.style.overflow = Overflow.Hidden;
                // if (ate.fillRadial)
                // {
                //     boundary.style.borderBottomLeftRadius = new StyleLength(new Length(50, LengthUnit.Percent));
                //     boundary.style.borderBottomRightRadius = new StyleLength(new Length(50, LengthUnit.Percent));
                //     boundary.style.borderTopLeftRadius = new StyleLength(new Length(50, LengthUnit.Percent));
                //     boundary.style.borderTopRightRadius = new StyleLength(new Length(50, LengthUnit.Percent));
                // }

                ate.radial.transform.rotation = Quaternion.Euler(0, 0, ate.angleOffset);
                ate.overlay.transform.rotation = Quaternion.Euler(0, 0, -ate.angleOffset);
                ate.Add(boundary);
            }
        }

        public UIFillerElement() : base()
        {
            radial = new VisualElement() ;
            radial.name = "filler";
            radial.AddToClassList("UIFillerElement__radial");
            overlay = new VisualElement() ;
            radial.generateVisualContent += OnGenerateVisualContent;
        }

        public void AngleUpdate(ChangeEvent<float> evt)
        {
            radial?.MarkDirtyRepaint();
        }

        public void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            Color32 color = fillColor;
            if (!radial.resolvedStyle.unityBackgroundImageTintColor.Equals(Color.white))
                color = radial.resolvedStyle.unityBackgroundImageTintColor;
            // default draw 1 triangle
            int triCount = 3;
            int indiceCount = 3;
            m_value = Mathf.Clamp(m_value, 0, 360);
            if (m_value * 360 < 240)
            {
                // Draw only 2 triangles
                if (value * 360 > 120)
                {
                    triCount = 4;
                    indiceCount = 6;
                }
            }
            // Draw 3 triangles
            else
            {
                triCount = 4;
                indiceCount = 9;
                if (m_value < 1)
                {
                    triCount = 5;
                    indiceCount = 9;
                }
            }

             // MeshWriteData mwd = mgc.Allocate(triCount, indiceCount, resolvedStyle.backgroundImage.texture);
             MeshWriteData mwd = mgc.Allocate(triCount, indiceCount);
            Vector3 origin = new Vector3((float)layout.width / 2, (float)layout.height / 2, 0);

            float diameter = 4 * radius;
            float degrees = ((m_value * 360) - 90) / Mathf.Rad2Deg;

            mwd.SetNextVertex(new Vertex()
                { position = origin + new Vector3(0 * diameter, 0 * diameter, Vertex.nearZ), tint =color });
            mwd.SetNextVertex(new Vertex()
                { position = origin + new Vector3(0 * diameter, -1 * diameter, Vertex.nearZ), tint = color });

            float direction = 1;
            if (fillDirection == FillDirection.AntiClockwise)
            {
                direction = -1;
            }

            mwd.SetNextIndex(0);
            mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)2 : (ushort)1);
            if (m_value * 360 <= 120)
            {
                mwd.SetNextVertex(new Vertex()
                {
                    position = origin + new Vector3(Mathf.Cos(degrees) * diameter * direction,
                        Mathf.Sin(degrees) * diameter, Vertex.nearZ),
                    tint = color
                });
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)1 : (ushort)2);
            }

            if (m_value * 360 > 120 && m_value * 360 <= 240)
            {
                mwd.SetNextVertex(new Vertex()
                {
                    position = origin + new Vector3(Mathf.Cos(30 / Mathf.Rad2Deg) * diameter * direction,
                        Mathf.Sin(30 / Mathf.Rad2Deg) * diameter, Vertex.nearZ),
                    tint = color
                });
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)1 : (ushort)2);
                mwd.SetNextVertex(new Vertex()
                {
                    position = origin + new Vector3(Mathf.Cos(degrees) * diameter * direction,
                        Mathf.Sin(degrees) * diameter, Vertex.nearZ),
                    tint = color
                });
                mwd.SetNextIndex(0);
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)3 : (ushort)2);
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)2 : (ushort)3);
            }

            if (m_value * 360 > 240)
            {
                mwd.SetNextVertex(new Vertex()
                {
                    position = origin + new Vector3(Mathf.Cos(30 / Mathf.Rad2Deg) * diameter * direction,
                        Mathf.Sin(30 / Mathf.Rad2Deg) * diameter, Vertex.nearZ),
                    tint = color
                });
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)1 : (ushort)2);
                mwd.SetNextVertex(new Vertex()
                {
                    position = origin + new Vector3(Mathf.Cos(150 / Mathf.Rad2Deg) * diameter * direction,
                        Mathf.Sin(150 / Mathf.Rad2Deg) * diameter, Vertex.nearZ),
                    tint = color
                });
                mwd.SetNextIndex(0);
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)3 : (ushort)2);
                mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)2 : (ushort)3);

                if (m_value * 360 >= 360)
                {
                    mwd.SetNextIndex(0);
                    mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)1 : (ushort)3);
                    mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)3 : (ushort)1);
                }
                else
                {
                    mwd.SetNextVertex(new Vertex()
                    {
                        position = origin + new Vector3(Mathf.Cos(degrees) * diameter * direction,
                            Mathf.Sin(degrees) * diameter, Vertex.nearZ),
                        tint = color
                    });
                    mwd.SetNextIndex(0);
                    mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)4 : (ushort)3);
                    mwd.SetNextIndex((fillDirection == FillDirection.AntiClockwise) ? (ushort)3 : (ushort)4);
                }
            }

        }
    }
}