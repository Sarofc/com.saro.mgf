using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Saro.SEditor
{
    using Object = UnityEngine.Object;
    public class ModelPreview
    {
        private class Styles
        {
            // TODO
            // public GUIContent pivot = EditorGUIUtility.TrIconContent("AvatarPivot", "Displays avatar's pivot and mass center");
            // public GUIContent ik = EditorGUIUtility.TrTextContent("IK", "Toggles feet IK preview");
            // public GUIContent is2D = EditorGUIUtility.TrIconContent("SceneView2D", "Toggles 2D preview mode");
            // public GUIContent avatarIcon = EditorGUIUtility.TrIconContent("AvatarSelector", "Changes the model to use for previewing.");

            public GUIStyle preButton = "toolbarbutton";
            public GUIStyle preSlider = "preSlider";
            public GUIStyle preSliderThumb = "preSliderThumb";
        }
        private static Styles s_Styles;

        public GameObject PreviewObject
        {
            get
            {
                return m_PreviewInstance;
            }
        }

        public TimeController TimeController
        {
            get
            {
                if (m_TimeController == null)
                {
                    m_TimeController = new TimeController();
                }

                return m_TimeController;
            }
        }

        private PreviewRenderUtility PreviewUtility
        {
            get
            {
                if (m_PreviewUtility == null)
                {
                    m_PreviewUtility = new PreviewRenderUtility();
                    m_PreviewUtility.camera.fieldOfView = 30.0f;
                    m_PreviewUtility.camera.allowHDR = false;
                    m_PreviewUtility.camera.allowMSAA = false;
                    m_PreviewUtility.ambientColor = new Color(.1f, .1f, .1f, 0);
                    m_PreviewUtility.lights[0].intensity = 1.4f;
                    m_PreviewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);
                    m_PreviewUtility.lights[1].intensity = 1.4f;
                }
                return m_PreviewUtility;
            }
        }

        private PreviewRenderUtility m_PreviewUtility;
        private GameObject m_PreviewInstance;
        private TimeController m_TimeController;
        private Vector2 m_PreviewDir = new Vector2(120, -20);
        private float m_AvatarScale = 1.0f;
        private float m_ZoomFactor = 1.0f;
        private float m_BoundingVolumeScale = 1.0f;
        private const string s_PreviewStr = "Preview";
        private int m_PreviewHint = s_PreviewStr.GetHashCode();
        private const string s_PreviewSceneStr = "PreviewSene";
        private int m_PreviewSceneHint = s_PreviewSceneStr.GetHashCode();
        private const string kSpeedPref = "AvatarpreviewSpeed";
        private const float kTimeControlRectHeight = 20;
        public int fps = 60;


        private Vector3 m_PivotPositionOffset = Vector3.zero;


        protected enum ViewTool { None, Pan, Zoom, Orbit }
        protected ViewTool m_ViewTool = ViewTool.None;
        protected ViewTool viewTool
        {
            get
            {
                Event evt = Event.current;
                if (m_ViewTool == ViewTool.None)
                {
                    bool controlKeyOnMac = (evt.control && Application.platform == RuntimePlatform.OSXEditor);

                    // actionKey could be command key on mac or ctrl on windows
                    bool actionKey = EditorGUI.actionKey;

                    bool noModifiers = (!actionKey && !controlKeyOnMac && !evt.alt);

                    if ((evt.button <= 0 && noModifiers) || (evt.button <= 0 && actionKey) || evt.button == 2)
                        m_ViewTool = ViewTool.Pan;
                    else if ((evt.button <= 0 && controlKeyOnMac) || (evt.button == 1 && evt.alt))
                        m_ViewTool = ViewTool.Zoom;
                    else if (evt.button <= 0 && evt.alt || evt.button == 1)
                        m_ViewTool = ViewTool.Orbit;
                }
                return m_ViewTool;
            }
        }

        public void DoAvatarRender(Rect r, GUIStyle background)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            PreviewUtility.BeginPreview(r, background);
            Camera camera = PreviewUtility.camera;
            camera.clearFlags = CameraClearFlags.Nothing;

            PreviewUtility.camera.nearClipPlane = 0.5f * m_ZoomFactor;
            PreviewUtility.camera.farClipPlane = 100.0f * m_AvatarScale;
            Quaternion camRot = Quaternion.Euler(-m_PreviewDir.y, -m_PreviewDir.x, 0);

            // Add panning offset
            Vector3 camPos = camRot * (Vector3.forward * -5.5f * m_ZoomFactor) + m_PreviewInstance.transform.position + m_PivotPositionOffset;
            PreviewUtility.camera.transform.position = camPos;
            PreviewUtility.camera.transform.rotation = camRot;

            camera.Render();
            PreviewUtility.EndAndDrawPreview(r);
        }

        public void SetPreview(GameObject go)
        {
            Object.DestroyImmediate(m_PreviewInstance);

            SetBounds(go);
        }

        public void SetBounds(GameObject go)
        {
            if (m_PreviewInstance == null)
            {
                m_PreviewInstance = GameObject.Instantiate(go);

                Bounds bounds = new Bounds(m_PreviewInstance.transform.position, Vector3.zero);
                Saro.Utility.ModelUtility.GetRenderableBoundsRecurse(ref bounds, m_PreviewInstance);
                m_BoundingVolumeScale = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));


                m_AvatarScale = m_ZoomFactor = m_BoundingVolumeScale / 2;
                PreviewUtility.AddSingleGO(m_PreviewInstance);
            }
        }

        protected void HandleViewTool(Event evt, EventType eventType, int id, Rect previewRect)
        {
            switch (eventType)
            {
                case EventType.ScrollWheel: DoAvatarPreviewZoom(evt, HandleUtility.niceMouseDeltaZoom * (evt.shift ? 2.0f : 0.5f)); break;
                case EventType.MouseDown: HandleMouseDown(evt, id, previewRect); break;
                case EventType.MouseUp: HandleMouseUp(evt, id); break;
                case EventType.MouseDrag: HandleMouseDrag(evt, id, previewRect); break;
            }
        }

        protected void HandleMouseDown(Event evt, int id, Rect previewRect)
        {
            if (viewTool != ViewTool.None && previewRect.Contains(evt.mousePosition))
            {
                EditorGUIUtility.SetWantsMouseJumping(1);
                evt.Use();
                GUIUtility.hotControl = id;
            }
        }

        protected void HandleMouseUp(Event evt, int id)
        {
            if (GUIUtility.hotControl == id)
            {
                m_ViewTool = ViewTool.None;

                GUIUtility.hotControl = 0;
                EditorGUIUtility.SetWantsMouseJumping(0);
                evt.Use();
            }
        }

        protected void HandleMouseDrag(Event evt, int id, Rect previewRect)
        {
            if (m_PreviewInstance == null)
                return;

            if (GUIUtility.hotControl == id)
            {
                switch (m_ViewTool)
                {
                    case ViewTool.Orbit: DoAvatarPreviewOrbit(evt, previewRect); break;
                    case ViewTool.Pan: DoAvatarPreviewPan(evt); break;
                    // case 605415 invert zoom delta to match scene view zooming
                    case ViewTool.Zoom: DoAvatarPreviewZoom(evt, -HandleUtility.niceMouseDeltaZoom * (evt.shift ? 2.0f : 0.5f)); break;
                    default: Debug.Log("Enum value not handled"); break;
                }
            }
        }

        public void DoAvatarPreviewPan(Event evt)
        {
            Camera cam = PreviewUtility.camera;
            Vector3 screenPos = cam.WorldToScreenPoint(m_PreviewInstance.transform.position + m_PivotPositionOffset);
            Vector3 delta = new Vector3(-evt.delta.x, evt.delta.y, 0);
            // delta panning is scale with the zoom factor to allow fine tuning when user is zooming closely.
            screenPos += delta * Mathf.Lerp(0.25f, 2.0f, m_ZoomFactor * 0.5f);
            Vector3 worldDelta = cam.ScreenToWorldPoint(screenPos) - (m_PreviewInstance.transform.position + m_PivotPositionOffset);
            m_PivotPositionOffset += worldDelta;
            evt.Use();
        }

        public void DoAvatarPreviewDrag(Event evt, EventType type)
        {
            if (type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                evt.Use();
            }
            else if (type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                GameObject newPreviewObject = DragAndDrop.objectReferences[0] as GameObject;

                if (newPreviewObject)
                {
                    DragAndDrop.AcceptDrag();
                    SetPreview(newPreviewObject);
                }

                evt.Use();
            }
        }


        public void DoAvatarPreviewOrbit(Event evt, Rect previewRect)
        {
            //Reset 2D on Orbit
            // if (is2D)
            // {
            //     is2D = false;
            // }
            m_PreviewDir -= evt.delta * (evt.shift ? 3 : 1) / Mathf.Min(previewRect.width, previewRect.height) * 140.0f;
            m_PreviewDir.y = Mathf.Clamp(m_PreviewDir.y, -90, 90);
            evt.Use();
        }

        public void DoAvatarPreviewZoom(Event evt, float delta)
        {
            float zoomDelta = -delta * 0.05f;
            m_ZoomFactor += m_ZoomFactor * zoomDelta;

            // zoom is clamp too 10 time closer than the original zoom
            m_ZoomFactor = Mathf.Max(m_ZoomFactor, m_AvatarScale / 10.0f);
            evt.Use();
        }

        public void AvatarTimeControlGUI(Rect rect)
        {
            const float kSliderWidth = 150f;
            const float kSpacing = 4f;
            Rect timeControlRect = rect;

            // background
            GUI.Box(rect, GUIContent.none, EditorStyles.toolbar);

            timeControlRect.height = kTimeControlRectHeight;
            timeControlRect.xMax -= kSliderWidth;

            Rect sliderControlRect = rect;
            sliderControlRect.height = kTimeControlRectHeight;
            sliderControlRect.yMin += 1;
            sliderControlRect.yMax -= 1;
            sliderControlRect.xMin = sliderControlRect.xMax - kSliderWidth + kSpacing;

            TimeController.DoTimeControl(timeControlRect);
            Rect labelRect = new Rect(new Vector2(rect.x, rect.y), EditorStyles.toolbar.CalcSize(EditorGUIUtility.TrTempContent("xxxxxx"))); ;
            labelRect.x = rect.xMax - labelRect.width;
            labelRect.yMin = rect.yMin;
            labelRect.yMax = rect.yMax;

            sliderControlRect.xMax = labelRect.xMin;

            EditorGUI.BeginChangeCheck();
            TimeController.playbackSpeed = PreviewSlider(sliderControlRect, TimeController.playbackSpeed, 0.03f);
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetFloat(kSpeedPref, TimeController.playbackSpeed);
            GUI.Label(labelRect, TimeController.playbackSpeed.ToString("f2", CultureInfo.InvariantCulture.NumberFormat) + "x", EditorStyles.toolbar);

            // Show current time in seconds:frame and in percentage
            rect.y = rect.yMax - 24;
            float time = TimeController.currentTime - TimeController.startTime;
            EditorGUI.DropShadowLabel(new Rect(rect.x, rect.y, rect.width, 20),
                string.Format("{0,2}:{1:00} ({2:000.0%}) Frame {3}", (int)time, Repeat(Mathf.FloorToInt(time * fps), fps), TimeController.normalizedTime, Mathf.FloorToInt(TimeController.currentTime * fps))
            );
        }

        private void Init()
        {
            if (s_Styles == null) s_Styles = new Styles();
        }

        public void DoPreviewSettings()
        {
            Init();
        }

        public void DoAvatorPreview(Rect rect, GUIStyle background)
        {
            Init();

            Rect previewRect = rect;
            previewRect.yMin += kTimeControlRectHeight;
            previewRect.height = Mathf.Max(previewRect.height, 64f);

            int previewID = GUIUtility.GetControlID(m_PreviewHint, FocusType.Passive, previewRect);
            Event evt = Event.current;
            EventType type = evt.GetTypeForControl(previewID);

            if (m_PreviewInstance != null && type == EventType.Repaint)
            {
                DoAvatarRender(previewRect, background);
            }

            if (m_PreviewInstance == null)
            {
                Rect warningRect = previewRect;
                warningRect.yMax -= warningRect.height / 2 - 16;
                EditorGUI.DropShadowLabel(
                    warningRect,
                    "No model is available for preview.\nPlease drag a model into this Preview Area.");
            }

            AvatarTimeControlGUI(rect);

            int previewSceneID = GUIUtility.GetControlID(m_PreviewSceneHint, FocusType.Passive);
            type = evt.GetTypeForControl(previewSceneID);

            DoAvatarPreviewDrag(evt, type);
            HandleViewTool(evt, type, previewSceneID, previewRect);
            // DoAvatarPreviewFrame(evt, type, previewRect);
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }
        }

        private float PreviewSlider(Rect rect, float val, float snapThreshold)
        {
            val = GUI.HorizontalSlider(rect, val, 0.1f, 2.0f, s_Styles.preSlider, s_Styles.preSliderThumb);//, GUILayout.MaxWidth(64));
            if (val > 0.25f - snapThreshold && val < 0.25f + snapThreshold)
                val = 0.25f;
            else if (val > 0.5f - snapThreshold && val < 0.5f + snapThreshold)
                val = 0.5f;
            else if (val > 0.75f - snapThreshold && val < 0.75f + snapThreshold)
                val = 0.75f;
            else if (val > 1.0f - snapThreshold && val < 1.0f + snapThreshold)
                val = 1.0f;
            else if (val > 1.25f - snapThreshold && val < 1.25f + snapThreshold)
                val = 1.25f;
            else if (val > 1.5f - snapThreshold && val < 1.5f + snapThreshold)
                val = 1.5f;
            else if (val > 1.75f - snapThreshold && val < 1.75f + snapThreshold)
                val = 1.75f;

            return val;
        }

        private int Repeat(int t, int length)
        {
            // Have to do double modulo in order to work for negative numbers.
            // This is quicker than a branch to test for negative number.
            return ((t % length) + length) % length;
        }

        public void OnDisable()
        {
            if (m_PreviewUtility != null)
            {
                m_PreviewUtility.Cleanup();
                m_PreviewUtility = null;
            }
        }
    }

}