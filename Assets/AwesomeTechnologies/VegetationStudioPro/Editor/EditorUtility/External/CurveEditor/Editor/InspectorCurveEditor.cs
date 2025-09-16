using AwesomeTechnologies.VegetationStudio;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AwesomeTechnologies.External.CurveEditor
{
    public static class FxStyles
    {
        public static GUIStyle TickStyleRight;
        public static GUIStyle TickStyleLeft;
        public static GUIStyle TickStyleCenter;

        public static GUIStyle PreSlider;
        public static GUIStyle PreSliderThumb;
        public static GUIStyle PreButton;
        public static GUIStyle PreDropdown;

        public static GUIStyle PreLabel;
        public static GUIStyle HueCenterCursor;
        public static GUIStyle HueRangeCursor;

        public static GUIStyle CenteredBoldLabel;
        public static GUIStyle WheelThumb;
        public static Vector2 WheelThumbSize;

        public static GUIStyle Header;
        public static GUIStyle HeaderCheckbox;
        public static GUIStyle HeaderFoldout;

        public static Texture2D PlayIcon;
        public static Texture2D CheckerIcon;
        public static Texture2D PaneOptionsIcon;

        public static GUIStyle CenteredMiniLabel;

        static FxStyles()
        {
            TickStyleRight = new GUIStyle("Label")
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 9
            };

            TickStyleLeft = new GUIStyle("Label")
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 9
            };

            TickStyleCenter = new GUIStyle("Label")
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9
            };

            PreSlider = new GUIStyle("PreSlider");
            PreSliderThumb = new GUIStyle("PreSliderThumb");
            PreButton = new GUIStyle("PreButton");
            PreDropdown = new GUIStyle("preDropdown");

            PreLabel = new GUIStyle("ShurikenLabel")
            {
                normal = { textColor = Color.white }
            };

            HueCenterCursor = new GUIStyle("ColorPicker2DThumb")
            {
                normal = { background = (Texture2D)EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/ShurikenPlus.png") },
                fixedWidth = 6,
                fixedHeight = 6
            };

            HueRangeCursor = new GUIStyle(HueCenterCursor)
            {
                normal = { background = (Texture2D)EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/CircularToggle_ON.png") }
            };

            WheelThumb = new GUIStyle("ColorPicker2DThumb");

            CenteredBoldLabel = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold
            };

            CenteredMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.UpperCenter
            };

            WheelThumbSize = new Vector2(
                !Mathf.Approximately(WheelThumb.fixedWidth, 0f) ? WheelThumb.fixedWidth : WheelThumb.padding.horizontal,
                !Mathf.Approximately(WheelThumb.fixedHeight, 0f) ? WheelThumb.fixedHeight : WheelThumb.padding.vertical
            );

            Header = new GUIStyle("ShurikenModuleTitle")
            {
                font = (new GUIStyle("Label")).font,
                border = new RectOffset(15, 7, 4, 4),
                fixedHeight = 22,
                contentOffset = new Vector2(20f, -2f)
            };

            HeaderCheckbox = new GUIStyle("ShurikenCheckMark");
            HeaderFoldout = new GUIStyle("Foldout");

            PlayIcon = (Texture2D)EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/IN foldout act.png");
            CheckerIcon = (Texture2D)EditorGUIUtility.LoadRequired("Icons/CheckerFloor.png");

            if (EditorGUIUtility.isProSkin)
                PaneOptionsIcon = (Texture2D)EditorGUIUtility.LoadRequired("Builtin Skins/DarkSkin/Images/pane options.png");
            else
                PaneOptionsIcon = (Texture2D)EditorGUIUtility.LoadRequired("Builtin Skins/LightSkin/Images/pane options.png");
        }
    }

    public class InspectorCurveEditor
    {
        #region Enums
        public enum InspectorCurveType
        {
            Height = 0,
            Steepness = 1,
            Distance = 2,
            Falloff = 3,
            FalloffInverted = 4,
            Scale = 5
        }

        enum EditMode
        {
            None,
            Moving,
            TangentEdit
        }

        enum Tangent
        {
            In,
            Out
        }
        #endregion

        #region Structs
        public struct Settings
        {
            public Rect Bounds;
            public RectOffset Padding;
            public Color SelectionColor;
            public float CurvePickingDistance;
            public float KeyTimeClampingDistance;

            public static Settings DefaultSettings
            {
                get
                {
                    return new Settings
                    {
                        Bounds = new Rect(0f, 0f, 1f, 1f),
                        Padding = new RectOffset(5, 5, 5, 5),
                        SelectionColor = new Color(1, 0.9f, 0.25f, 0.9f),
                        CurvePickingDistance = 5,
                        KeyTimeClampingDistance = 0.0001f
                    };
                }
            }
        }

        public struct CurveState
        {
            public bool Visible;
            public bool Editable;
            public uint MinPointCount;
            public float ZeroKeyConstantValue;
            public Color Color;
            public float Width;
            public float HandleWidth;
            public bool ShowNonEditableHandles;
            public bool OnlyShowHandlesOnSelection;
            public bool LoopInBounds;

            public static CurveState DefaultState
            {
                get
                {
                    return new CurveState
                    {
                        Visible = true,
                        Editable = true,
                        MinPointCount = 2,
                        ZeroKeyConstantValue = 0f,
                        Color = Color.white,
                        Width = 2f,
                        HandleWidth = 2f,
                        ShowNonEditableHandles = true,
                        OnlyShowHandlesOnSelection = false,
                        LoopInBounds = false
                    };
                }
            }
        }

        public struct Selection
        {
            public AnimationCurve Curve;
            public int KeyframeIndex;
            public Keyframe? Keyframe;

            public Selection(AnimationCurve curve, int keyframeIndex, Keyframe? keyframe)
            {
                Curve = curve;
                KeyframeIndex = keyframeIndex;
                Keyframe = keyframe;
            }
        }

        internal struct MenuAction
        {
            internal AnimationCurve Curve;
            internal int Index;
            internal Vector3 Position;

            internal MenuAction(AnimationCurve curve)
            {
                Curve = curve;
                Index = -1;
                Position = Vector3.zero;
            }

            internal MenuAction(AnimationCurve curve, int index)
            {
                Curve = curve;
                Index = index;
                Position = Vector3.zero;
            }

            internal MenuAction(AnimationCurve curve, Vector3 position)
            {
                Curve = curve;
                Index = -1;
                Position = position;
            }
        }
        #endregion

        #region Fields & properties
        public Settings settings { get; private set; }

        private readonly Dictionary<AnimationCurve, CurveState> _mCurves;
        Rect _mCurveArea;

        public float SeaLevel;
        public float MaxValue = 90;

        public AnimationCurve selectedCurve;
        public int selectedKeyframeIndex = -1;
        bool isDirty;

        public InspectorCurveType CurveType;
        EditMode editMode = EditMode.None;
        Tangent tangentEditMode;
        #endregion

        #region Constructors & destructors
        public InspectorCurveEditor() : this(Settings.DefaultSettings)
        {

        }

        public InspectorCurveEditor(Settings settings)
        {
            this.settings = settings;
            _mCurves = new Dictionary<AnimationCurve, CurveState>();
        }

        #endregion

        #region Public API
        public void Add(params AnimationCurve[] curves)
        {
            foreach (AnimationCurve curve in curves)
                Add(curve, CurveState.DefaultState);
        }

        public void Add(AnimationCurve curve)
        {
            Add(curve, CurveState.DefaultState);
        }

        public void Add(AnimationCurve curve, CurveState state)
        {
            _mCurves.Add(curve, state);
        }

        public void Remove(AnimationCurve curve)
        {
            _mCurves.Remove(curve);
        }

        public void RemoveAll()
        {
            _mCurves.Clear();
        }

        public CurveState GetCurveState(AnimationCurve curve)
        {
            if (_mCurves.TryGetValue(curve, out CurveState state) == false)
                throw new KeyNotFoundException("curve");
            return state;
        }

        public void SetCurveState(AnimationCurve curve, CurveState state)
        {
            if (_mCurves.ContainsKey(curve) == false)
                throw new KeyNotFoundException("curve");
            _mCurves[curve] = state;
        }

        public Selection GetSelection()
        {
            Keyframe? key = null;
            if (selectedKeyframeIndex > -1)
            {
                AnimationCurve curve = selectedCurve;  //.animationCurveValue;

                if (selectedKeyframeIndex >= curve.length)
                    selectedKeyframeIndex = -1;
                else
                    key = curve[selectedKeyframeIndex];
            }

            return new Selection(selectedCurve, selectedKeyframeIndex, key);
        }

        //public void SetKeyframe(AnimationCurve curve, int keyframeIndex, Keyframe keyframe)
        //{
        //    var animCurve = curve;//.animationCurveValue;
        //    SetKeyframe(animCurve, keyframeIndex, keyframe);
        //    SaveCurve(curve, animCurve);
        //}

        public bool OnGUI(Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
                isDirty = false;

            GUI.BeginClip(rect);
            {
                Rect area = new(Vector2.zero, rect.size);
                _mCurveArea = settings.Padding.Remove(area);

                foreach (var curve in _mCurves)
                    OnCurveGUI(area, curve.Key, curve.Value);

                OnGeneralUI();
            }
            GUI.EndClip();

            return isDirty;
        }

        #endregion

        #region UI & events
        void OnCurveGUI(Rect rect, AnimationCurve curve, CurveState state)
        {
            // Discard invisible curves
            if (!state.Visible)
                return;

            var animCurve = curve;//.animationCurveValue;
            var keys = animCurve.keys;
            var length = keys.Length;

            // Curve drawing
            // Slightly dim non-editable curves
            var color = state.Color;
            if (!state.Editable)
                color.a *= 0.5f;

            Handles.color = color;
            var bounds = settings.Bounds;

            if (length == 0)
            {
                var p1 = CurveToCanvas(new Vector3(bounds.xMin, state.ZeroKeyConstantValue));
                var p2 = CurveToCanvas(new Vector3(bounds.xMax, state.ZeroKeyConstantValue));
                Handles.DrawAAPolyLine(state.Width, p1, p2);
            }
            else if (length == 1)
            {
                var p1 = CurveToCanvas(new Vector3(bounds.xMin, keys[0].value));
                var p2 = CurveToCanvas(new Vector3(bounds.xMax, keys[0].value));
                Handles.DrawAAPolyLine(state.Width, p1, p2);
            }
            else
            {
                var prevKey = keys[0];
                for (int k = 1; k < length; k++)
                {
                    var key = keys[k];
                    var pts = BezierSegment(prevKey, key);

                    if (float.IsInfinity(prevKey.outTangent) || float.IsInfinity(key.inTangent))
                    {
                        var s = HardSegment(prevKey, key);
                        Handles.DrawAAPolyLine(state.Width, s[0], s[1], s[2]);
                    }
                    else Handles.DrawBezier(pts[0], pts[3], pts[1], pts[2], color, null, state.Width);

                    prevKey = key;
                }

                // Curve extents & loops
                if (keys[0].time > bounds.xMin)
                {
                    if (state.LoopInBounds)
                    {
                        var p1 = keys[length - 1];
                        p1.time -= settings.Bounds.width;
                        var p2 = keys[0];
                        var pts = BezierSegment(p1, p2);

                        if (float.IsInfinity(p1.outTangent) || float.IsInfinity(p2.inTangent))
                        {
                            var s = HardSegment(p1, p2);
                            Handles.DrawAAPolyLine(state.Width, s[0], s[1], s[2]);
                        }
                        else Handles.DrawBezier(pts[0], pts[3], pts[1], pts[2], color, null, state.Width);
                    }
                    else
                    {
                        var p1 = CurveToCanvas(new Vector3(bounds.xMin, keys[0].value));
                        var p2 = CurveToCanvas(keys[0]);
                        Handles.DrawAAPolyLine(state.Width, p1, p2);
                    }
                }

                if (keys[length - 1].time < bounds.xMax)
                {
                    if (state.LoopInBounds)
                    {
                        var p1 = keys[length - 1];
                        var p2 = keys[0];
                        p2.time += settings.Bounds.width;
                        var pts = BezierSegment(p1, p2);

                        if (float.IsInfinity(p1.outTangent) || float.IsInfinity(p2.inTangent))
                        {
                            var s = HardSegment(p1, p2);
                            Handles.DrawAAPolyLine(state.Width, s[0], s[1], s[2]);
                        }
                        else Handles.DrawBezier(pts[0], pts[3], pts[1], pts[2], color, null, state.Width);
                    }
                    else
                    {
                        var p1 = CurveToCanvas(keys[length - 1]);
                        var p2 = CurveToCanvas(new Vector3(bounds.xMax, keys[length - 1].value));
                        Handles.DrawAAPolyLine(state.Width, p1, p2);
                    }
                }
            }

            // Make sure selection is correct (undo can break it)
            bool isCurrentlySelectedCurve = curve == selectedCurve;

            if (isCurrentlySelectedCurve && selectedKeyframeIndex >= length)
                selectedKeyframeIndex = -1;

            // Handles & keys
            for (int k = 0; k < length; k++)
            {
                bool isCurrentlySelectedKeyframe = k == selectedKeyframeIndex;
                var e = Event.current;

                var pos = CurveToCanvas(keys[k]);
                var hitRect = new Rect(pos.x - 8f, pos.y - 8f, 16f, 16f);
                var offset = isCurrentlySelectedCurve ? new RectOffset(5, 5, 5, 5) : new RectOffset(6, 6, 6, 6);

                var outTangent = pos + CurveTangentToCanvas(keys[k].outTangent).normalized * 40f;
                var inTangent = pos - CurveTangentToCanvas(keys[k].inTangent).normalized * 40f;
                var inTangentHitRect = new Rect(inTangent.x - 7f, inTangent.y - 7f, 14f, 14f);
                var outTangentHitrect = new Rect(outTangent.x - 7f, outTangent.y - 7f, 14f, 14f);

                // Draw
                if (state.ShowNonEditableHandles)
                {
                    if (e.type == EventType.Repaint)
                    {
                        var selectedColor = (isCurrentlySelectedCurve && isCurrentlySelectedKeyframe) ? settings.SelectionColor : state.Color;

                        // Keyframe
                        EditorGUI.DrawRect(offset.Remove(hitRect), selectedColor);

                        // Tangents
                        if (isCurrentlySelectedCurve && (!state.OnlyShowHandlesOnSelection || (state.OnlyShowHandlesOnSelection && isCurrentlySelectedKeyframe)))
                        {
                            Handles.color = selectedColor;

                            if (k > 0 || state.LoopInBounds)
                            {
                                Handles.DrawAAPolyLine(state.HandleWidth, pos, inTangent);
                                EditorGUI.DrawRect(offset.Remove(inTangentHitRect), selectedColor);
                            }

                            if (k < length - 1 || state.LoopInBounds)
                            {
                                Handles.DrawAAPolyLine(state.HandleWidth, pos, outTangent);
                                EditorGUI.DrawRect(offset.Remove(outTangentHitrect), selectedColor);
                            }
                        }
                    }
                }

                // Events
                if (state.Editable)
                {
                    // Keyframe move
                    if (editMode == EditMode.Moving && e.type == EventType.MouseDrag && isCurrentlySelectedCurve && isCurrentlySelectedKeyframe)
                        EditMoveKeyframe(animCurve, keys, k);

                    // Tangent editing
                    if (editMode == EditMode.TangentEdit && e.type == EventType.MouseDrag && isCurrentlySelectedCurve && isCurrentlySelectedKeyframe)
                    {
                        bool alreadyBroken = !(Mathf.Approximately(keys[k].inTangent, keys[k].outTangent) || (float.IsInfinity(keys[k].inTangent) && float.IsInfinity(keys[k].outTangent)));
                        EditMoveTangent(animCurve, keys, k, tangentEditMode, e.shift || !(alreadyBroken || e.control));
                    }

                    // Keyframe selection & context menu
                    if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                        if (hitRect.Contains(e.mousePosition))
                            if (e.button == 0)
                            {
                                SelectKeyframe(curve, k);
                                editMode = EditMode.Moving;
                                e.Use();
                            }
                            else if (e.button == 1)
                            {
                                // Keyframe context menu
                                var menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete Key"), false, (x) =>
                                {
                                    var action = (MenuAction)x;
                                    var curveValue = action.Curve;//.animationCurveValue;
                                    //TUDO Event to update object
                                    //action.curve.serializedObject.Update();
                                    RemoveKeyframe(curveValue, action.Index);
                                    selectedKeyframeIndex = -1;
                                    SaveCurve();
                                    //TUDO Event to update object
                                    //action.curve.serializedObject.ApplyModifiedProperties();
                                }, new MenuAction(curve, k));
                                menu.ShowAsContext();
                                e.Use();
                            }

                    // Tangent selection & edit mode
                    if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                        if (inTangentHitRect.Contains(e.mousePosition) && (k > 0 || state.LoopInBounds))
                        {
                            SelectKeyframe(curve, k);
                            editMode = EditMode.TangentEdit;
                            tangentEditMode = Tangent.In;
                            e.Use();
                        }
                        else if (outTangentHitrect.Contains(e.mousePosition) && (k < length - 1 || state.LoopInBounds))
                        {
                            SelectKeyframe(curve, k);
                            editMode = EditMode.TangentEdit;
                            tangentEditMode = Tangent.Out;
                            e.Use();
                        }

                    // Mouse up - clean up states
                    if (e.rawType == EventType.MouseUp && editMode != EditMode.None)
                        editMode = EditMode.None;

                    // Set cursors
                    {
                        EditorGUIUtility.AddCursorRect(hitRect, MouseCursor.MoveArrow);

                        if (k > 0 || state.LoopInBounds)
                            EditorGUIUtility.AddCursorRect(inTangentHitRect, MouseCursor.RotateArrow);

                        if (k < length - 1 || state.LoopInBounds)
                            EditorGUIUtility.AddCursorRect(outTangentHitrect, MouseCursor.RotateArrow);
                    }
                }
            }

            Handles.color = Color.white;
            SaveCurve();
        }

        private void OnGeneralUI()
        {
            var e = Event.current;

            // Selection
            if (e.type == EventType.MouseDown)
            {
                GUI.FocusControl(null);
                selectedCurve = null;
                selectedKeyframeIndex = -1;
                bool used = false;

                var hit = CanvasToCurve(e.mousePosition);
                float curvePickValue = CurveToCanvas(hit).y;

                // Try and select a curve
                foreach (var curve in _mCurves)
                {
                    if (!curve.Value.Editable || !curve.Value.Visible)
                        continue;

                    var prop = curve.Key;
                    var state = curve.Value;
                    var animCurve = prop;//.animationCurveValue;
                    float hitY = animCurve.length == 0 ? state.ZeroKeyConstantValue : animCurve.Evaluate(hit.x);

                    var curvePos = CurveToCanvas(new Vector3(hit.x, hitY));

                    if (Mathf.Abs(curvePos.y - curvePickValue) < settings.CurvePickingDistance)
                    {
                        selectedCurve = prop;

                        if (e.clickCount == 2 && e.button == 0)
                        {
                            // Create a keyframe on double-click on this curve
                            EditCreateKeyframe(animCurve, hit, true, state.ZeroKeyConstantValue);
                            SaveCurve();
                        }
                        else if (e.button == 1)
                        {
                            // Curve context menu
                            var menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Add Key"), false, (x) =>
                            {
                                var action = (MenuAction)x;
                                var curveValue = action.Curve;//.animationCurveValue;
                                // TODO event to update object
                                //action.curve.serializedObject.Update();
                                EditCreateKeyframe(curveValue, hit, true, 0f);
                                SaveCurve();
                                // TODO event to update object
                                //action.curve.serializedObject.ApplyModifiedProperties();
                            }, new MenuAction(prop, hit));



                            menu.ShowAsContext();
                            e.Use();
                            used = true;
                        }
                    }
                }

                if (e.clickCount == 2 && e.button == 0 && selectedCurve == null)
                {
                    // Create a keyframe on every curve on double-click
                    foreach (var curve in _mCurves)
                    {
                        if (!curve.Value.Editable || !curve.Value.Visible)
                            continue;

                        var prop = curve.Key;
                        var state = curve.Value;
                        var animCurve = prop;//.animationCurveValue;
                        EditCreateKeyframe(animCurve, hit, e.alt, state.ZeroKeyConstantValue);
                        SaveCurve();
                    }
                }
                else if (!used && e.button == 1)
                {
                    // Global context menu
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Add Key At Position"), false, () => ContextMenuAddKey(hit, false));
                    menu.AddItem(new GUIContent("Add Key On Curves"), false, () => ContextMenuAddKey(hit, true));
                    menu.AddItem(new GUIContent("Copy Curve"), false, () => CopyCurve());
                    if (VegetationStudioManager.GetAnimationCurveFromClippboard() != null)
                        menu.AddItem(new GUIContent("Paste Curve"), false, () => PasteCurve());
                    menu.ShowAsContext();
                }

                e.Use();
            }

            // delete currently selected key
            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Delete))
                if (selectedKeyframeIndex != -1 && selectedCurve != null)
                {
                    AnimationCurve animCurve = selectedCurve;
                    int length = animCurve.length;

                    if (_mCurves[selectedCurve].MinPointCount < length && length >= 0)
                    {
                        EditDeleteKeyframe(animCurve, selectedKeyframeIndex);
                        selectedKeyframeIndex = -1;
                        SaveCurve();
                    }

                    e.Use();
                }
        }

        void CopyCurve()
        {
            if (_mCurves.Count > 0)
            {
                foreach (var curve in _mCurves)
                {
                    VegetationStudioManager.AddAnimationCurveToClipboard(curve.Key);
                    break;
                }
            }
            else
            {
                VegetationStudioManager.AddAnimationCurveToClipboard(null);
            }
        }

        void PasteCurve()
        {
            AnimationCurve animationCurve = VegetationStudioManager.GetAnimationCurveFromClippboard();
            if (animationCurve != null)
            {
                foreach (var curve in _mCurves)
                {
                    Keyframe[] keyframes = animationCurve.keys;
                    curve.Key.keys = keyframes;
                    //curve.Key.keys = new Keyframe[0];                  
                    //for (var i = 0; i < keyframes.Length; i++)
                    //    curve.Key.AddKey(keyframes[i].time, keyframes[i].value);
                    break;
                }
            }
        }

        private static void SaveCurve()
        {
            //prop.animationCurveValue = curve;
        }

        void Invalidate()
        {
            isDirty = true;
        }
        #endregion

        #region Keyframe manipulations
        void SelectKeyframe(AnimationCurve curve, int keyframeIndex)
        {
            selectedKeyframeIndex = keyframeIndex;
            selectedCurve = curve;
            Invalidate();
        }

        void ContextMenuAddKey(Vector3 hit, bool createOnCurve)
        {
            // SerializedObject serializedObject = null;

            foreach (var curve in _mCurves)
            {
                if (!curve.Value.Editable || !curve.Value.Visible)
                    continue;

                var prop = curve.Key;
                var state = curve.Value;

                //if (serializedObject == null)
                //{
                //    serializedObject = prop.serializedObject;
                //    serializedObject.Update();
                //}

                var animCurve = prop;//.animationCurveValue;
                EditCreateKeyframe(animCurve, hit, createOnCurve, state.ZeroKeyConstantValue);
                SaveCurve();
            }

            //if (serializedObject != null)
            //    serializedObject.ApplyModifiedProperties();

            Invalidate();
        }

        void EditCreateKeyframe(AnimationCurve curve, Vector3 position, bool createOnCurve, float zeroKeyConstantValue)
        {
            float tangent = EvaluateTangent(curve, position.x);

            if (createOnCurve)
                position.y = curve.length == 0 ? zeroKeyConstantValue : curve.Evaluate(position.x);

            AddKeyframe(curve, new Keyframe(position.x, position.y, tangent, tangent));
        }

        void EditDeleteKeyframe(AnimationCurve curve, int keyframeIndex)
        {
            RemoveKeyframe(curve, keyframeIndex);
        }

        void AddKeyframe(AnimationCurve curve, Keyframe newValue)
        {
            curve.AddKey(newValue);
            Invalidate();
        }

        void RemoveKeyframe(AnimationCurve curve, int keyframeIndex)
        {
            curve.RemoveKey(keyframeIndex);
            Invalidate();
        }

        void SetKeyframe(AnimationCurve curve, int keyframeIndex, Keyframe newValue)
        {
            var keys = curve.keys;

            if (keyframeIndex > 0)
                newValue.time = Mathf.Max(keys[keyframeIndex - 1].time + settings.KeyTimeClampingDistance, newValue.time);

            if (keyframeIndex < keys.Length - 1)
                newValue.time = Mathf.Min(keys[keyframeIndex + 1].time - settings.KeyTimeClampingDistance, newValue.time);

            curve.MoveKey(keyframeIndex, newValue);
            Invalidate();
        }

        void EditMoveKeyframe(AnimationCurve curve, Keyframe[] keys, int keyframeIndex)
        {
            var key = CanvasToCurve(Event.current.mousePosition);
            float inTgt = keys[keyframeIndex].inTangent;
            float outTgt = keys[keyframeIndex].outTangent;
            SetKeyframe(curve, keyframeIndex, new Keyframe(key.x, key.y, inTgt, outTgt));
        }

        void EditMoveTangent(AnimationCurve curve, Keyframe[] keys, int keyframeIndex, Tangent targetTangent, bool linkTangents)
        {
            var pos = CanvasToCurve(Event.current.mousePosition);

            float time = keys[keyframeIndex].time;
            float value = keys[keyframeIndex].value;

            pos -= new Vector3(time, value);

            if (targetTangent == Tangent.In && pos.x > 0f)
                pos.x = 0f;

            if (targetTangent == Tangent.Out && pos.x < 0f)
                pos.x = 0f;

            float tangent;

            if (Mathf.Approximately(pos.x, 0f))
                tangent = pos.y < 0f ? float.PositiveInfinity : float.NegativeInfinity;
            else
                tangent = pos.y / pos.x;

            float inTangent = keys[keyframeIndex].inTangent;
            float outTangent = keys[keyframeIndex].outTangent;

            if (targetTangent == Tangent.In || linkTangents)
                inTangent = tangent;
            if (targetTangent == Tangent.Out || linkTangents)
                outTangent = tangent;

            SetKeyframe(curve, keyframeIndex, new Keyframe(time, value, inTangent, outTangent));
        }
        #endregion

        #region Maths utilities

        Vector3 CurveToCanvas(Keyframe keyframe)
        {
            return CurveToCanvas(new Vector3(keyframe.time, keyframe.value));
        }

        Vector3 CurveToCanvas(Vector3 position)
        {
            var bounds = settings.Bounds;
            var output = new Vector3((position.x - bounds.x) / (bounds.xMax - bounds.x), (position.y - bounds.y) / (bounds.yMax - bounds.y));
            output.x = output.x * (_mCurveArea.xMax - _mCurveArea.xMin) + _mCurveArea.xMin;
            output.y = (1f - output.y) * (_mCurveArea.yMax - _mCurveArea.yMin) + _mCurveArea.yMin;
            return output;
        }

        Vector3 CanvasToCurve(Vector3 position)
        {
            var bounds = settings.Bounds;
            var output = position;
            output.x = (output.x - _mCurveArea.xMin) / (_mCurveArea.xMax - _mCurveArea.xMin);
            output.y = (output.y - _mCurveArea.yMin) / (_mCurveArea.yMax - _mCurveArea.yMin);
            output.x = Mathf.Lerp(bounds.x, bounds.xMax, output.x);
            output.y = Mathf.Lerp(bounds.yMax, bounds.y, output.y);
            return output;
        }

        Vector3 CurveTangentToCanvas(float tangent)
        {
            if (!float.IsInfinity(tangent))
            {
                var bounds = settings.Bounds;
                float ratio = (_mCurveArea.width / _mCurveArea.height) / ((bounds.xMax - bounds.x) / (bounds.yMax - bounds.y));
                return new Vector3(1f, -tangent / ratio).normalized;
            }

            return float.IsPositiveInfinity(tangent) ? Vector3.up : Vector3.down;
        }

        Vector3[] BezierSegment(Keyframe start, Keyframe end)
        {
            var segment = new Vector3[4];

            segment[0] = CurveToCanvas(new Vector3(start.time, start.value));
            segment[3] = CurveToCanvas(new Vector3(end.time, end.value));

            float middle = start.time + ((end.time - start.time) * 0.333333f);
            float middle2 = start.time + ((end.time - start.time) * 0.666666f);

            segment[1] = CurveToCanvas(new Vector3(middle, ProjectTangent(start.time, start.value, start.outTangent, middle)));
            segment[2] = CurveToCanvas(new Vector3(middle2, ProjectTangent(end.time, end.value, end.inTangent, middle2)));

            return segment;
        }

        Vector3[] HardSegment(Keyframe start, Keyframe end)
        {
            var segment = new Vector3[3];

            segment[0] = CurveToCanvas(start);
            segment[1] = CurveToCanvas(new Vector3(end.time, start.value));
            segment[2] = CurveToCanvas(end);

            return segment;
        }

        float ProjectTangent(float inPosition, float inValue, float inTangent, float projPosition)
        {
            return inValue + ((projPosition - inPosition) * inTangent);
        }

        float EvaluateTangent(AnimationCurve curve, float time)
        {
            int prev = -1, next = 0;
            for (int i = 0; i < curve.keys.Length; i++)
            {
                if (time > curve.keys[i].time)
                {
                    prev = i;
                    next = i + 1;
                }
                else break;
            }

            if (next == 0)
                return 0f;

            if (prev == curve.keys.Length - 1)
                return 0f;

            const float kD = 1e-3f;
            float tp = Mathf.Max(time - kD, curve.keys[prev].time);
            float tn = Mathf.Min(time + kD, curve.keys[next].time);

            float vp = curve.Evaluate(tp);
            float vn = curve.Evaluate(tn);

            if (Mathf.Approximately(tn, tp))
                return (vn - vp > 0f) ? float.PositiveInfinity : float.NegativeInfinity;

            return (vn - vp) / (tn - tp);
        }

        #endregion

        public void DrawCurve(Editor _editor)
        {
            Rect rect = GUILayoutUtility.GetAspectRect(2.5f);
            Rect innerRect = settings.Padding.Remove(rect);
            Rect rectHeight = rect;
            Rect innerHeightRect = innerRect;
            float waterLevelPercent = Mathf.Clamp(SeaLevel / MaxValue, 0, 0.33f);

            if (CurveType == InspectorCurveType.Height && SeaLevel > 0)
            {   // adjust outlines / animation curve space => make space for water level preview
                SeaLevel *= 0.5f;   // reduce pixels taken away
                innerRect.xMin += waterLevelPercent * innerRect.width;
                rectHeight.xMin += waterLevelPercent * rectHeight.width;
            }

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, new Color(0.125f, 0.125f, 0.125f, 1));    // background
                Handles.color = Color.white;
                Handles.DrawSolidRectangleWithOutline(innerRect, Color.clear, new Color(0.75f, 0.75f, 0.75f, 0.75f));   // inner outline
            }

            if (CurveType == InspectorCurveType.Height && SeaLevel > 0)
            {   // water level preview
                Handles.color = new(0.125f, 0.33f, 1, 1);
                Handles.DrawSolidRectangleWithOutline(new Rect(new Vector2(innerHeightRect.x, innerHeightRect.y), new Vector2(waterLevelPercent * innerHeightRect.width, innerHeightRect.height)), new Color(0.66f, 0.66f, 0.66f, 0.1f), Color.cyan);
            }

            if (OnGUI(CurveType == InspectorCurveType.Height ? rectHeight : rect))
                if (isDirty)
                {
                    _editor.Repaint();
                    GUI.changed = true;
                }

            if (Event.current.type == EventType.Repaint)
            {
                Handles.color = Color.black;    // outer outline 
                Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.xMax, rect.y));
                Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x, rect.yMax));
                Handles.DrawLine(new Vector2(rect.x, rect.yMax), new Vector2(rect.xMax, rect.yMax));
                Handles.DrawLine(new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.y));

                Selection selection = GetSelection();   // animation curve selection info, per node
                if (selection.Curve != null && selection.KeyframeIndex > -1)
                    if (selection.Keyframe != null)
                    {
                        Keyframe key = selection.Keyframe.Value;
                        Rect infoRect = innerRect;
                        infoRect.x += 5;
                        infoRect.width = 150;
                        infoRect.height = 30;

                        switch (CurveType)  // display detailed info for users
                        {
                            case InspectorCurveType.Height: // shows value of what's being used internally -- currently only "above sea level" logic
                                GUI.Label(infoRect, string.Format("{0}\n{1}", (key.time * MaxValue).ToString("F2") + " meter above sea level", (key.value * 100).ToString("F0") + "% density"), FxStyles.PreLabel);
                                break;
                            case InspectorCurveType.Steepness:
                                GUI.Label(infoRect, string.Format("{0}\n{1}", (key.time * MaxValue).ToString("F2") + " degree/-s steepness", (key.value * 100).ToString("F0") + "% density"), FxStyles.PreLabel);
                                break;
                            case InspectorCurveType.Falloff:
                                GUI.Label(infoRect, string.Format("{0}\n{1}", (key.time * 100).ToString("F0") + " % distance", (key.value * 100).ToString("F0") + "% density"), FxStyles.PreLabel);
                                break;
                            case InspectorCurveType.FalloffInverted:
                                GUI.Label(infoRect, string.Format("{0}\n{1}", (100 - key.time * 100).ToString("F0") + " % density", (key.value * 100).ToString("F0") + "% distance"), FxStyles.PreLabel);
                                break;
                            case InspectorCurveType.Scale:
                                GUI.Label(infoRect, string.Format("{0}\n{1}", (100 - key.time * 100).ToString("F0") + " % of min scale", (key.value * 100).ToString("F0") + "% of max scale"), FxStyles.PreLabel);
                                break;
                        }
                    }
            }
        }

        public void AddCurve(AnimationCurve _curve, Color _color, uint _minPointCount, bool _loop)
        {
            CurveState state = CurveState.DefaultState;
            state.Color = _color;
            //state.Visible = false;
            state.MinPointCount = _minPointCount;
            state.OnlyShowHandlesOnSelection = true;
            state.ZeroKeyConstantValue = 0.5f;
            state.LoopInBounds = _loop;
            state.Visible = true;
            Add(_curve, state);
        }

        public bool EditCurve(AnimationCurve _animationCurve, Editor _editor)
        {
            Keyframe[] keyframes = _animationCurve.keys;

            RemoveAll();

            switch (CurveType)
            {
                case InspectorCurveType.Distance:
                    AddCurve(_animationCurve, new Color(0f, 1f, 0f), 2, false);
                    break;
                case InspectorCurveType.Height:
                    AddCurve(_animationCurve, new Color(1f, 0f, 0f), 2, false);
                    break;
                case InspectorCurveType.Steepness:
                    AddCurve(_animationCurve, new Color(0f, 1f, 0f), 2, false);
                    break;
                case InspectorCurveType.Falloff:
                    AddCurve(_animationCurve, new Color(0f, 1f, 0f), 2, false);
                    break;
                case InspectorCurveType.FalloffInverted:
                    AddCurve(_animationCurve, new Color(0f, 1f, 0f), 2, false);
                    break;
                case InspectorCurveType.Scale:
                    AddCurve(_animationCurve, new Color(0f, 1f, 0f), 2, false);
                    break;
                default:
                    AddCurve(_animationCurve, new Color(0f, 1f, 0f), 2, false);
                    break;
            }

            DrawCurve(_editor);

            if (_animationCurve.keys.Length != keyframes.Length)
                return true;

            for (int i = 0; i < _animationCurve.keys.Length; i++)
                if (keyframes[i].Equals(_animationCurve.keys[i]) == false)
                    return true;
            return false;
        }

        public bool EditCurves(AnimationCurve _animationCurve, AnimationCurve _animationCurve2, Editor _editor)
        {
            Keyframe[] keyframes = _animationCurve.keys;
            Keyframe[] keyframes2 = _animationCurve2.keys;

            RemoveAll();

            AddCurve(_animationCurve, new Color(0f, 1f, 0f), 2, false);
            AddCurve(_animationCurve2, new Color(1f, 0f, 0f), 2, false);

            DrawCurve(_editor);

            if (_animationCurve.keys.Length != keyframes.Length)
                return true;

            for (int i = 0; i < _animationCurve.keys.Length; i++)
                if (keyframes[i].Equals(_animationCurve.keys[i]) == false)
                    return true;

            if (_animationCurve2.keys.Length != keyframes2.Length)
                return true;

            for (int i = 0; i < _animationCurve2.keys.Length; i++)
                if (keyframes2[i].Equals(_animationCurve2.keys[i]) == false)
                    return true;
            return false;
        }
    }
}