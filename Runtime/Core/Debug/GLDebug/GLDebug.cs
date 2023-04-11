using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saro.Diagnostics
{
    [RequireComponent(typeof(Camera))]
    public sealed partial class GLDebug : MonoBehaviour
    {
        private struct Line
        {
            public Vector3 start;

            public Vector3 end;

            public Color color;

            public float startTime;

            public float duration;

            public Line(Vector3 start, Vector3 end, Color color, float startTime, float duration)
            {
                this.start = start;
                this.end = end;
                this.color = color;
                this.startTime = startTime;
                this.duration = duration;
            }

            public bool DurationElapsed(bool drawLine)
            {
                if (drawLine)
                {
                    GL.Color(color);
                    GL.Vertex(start);
                    GL.Vertex(end);
                }

                return Time.time - startTime >= duration;
            }
        }

        private static GLDebug s_Instance = null;

        private static Material s_Mat = null;

        private List<Line> m_Lines;

        public bool displayLines = true;

        internal static GLDebug Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    Camera main = Camera.main;
                    if (main == null)
                    {
                        throw new Exception("Couldn't find any main camera to attach the GLDebug script. System will not work");
                    }

                    s_Instance = main.gameObject.AddComponent<GLDebug>();
                }

                return s_Instance;
            }
        }

        void Awake()
        {
            SetMaterial();

            m_Lines = new List<Line>(4096);
            if (s_Instance == null)
            {
                s_Instance = this;
            }
            else if (s_Instance != this)
            {
                UnityEngine.Object.Destroy(this);
            }
        }

        void SetMaterial()
        {
            if (s_Mat == null)
            {
                Shader shader = Shader.Find("GLDebug/GLine");

                if (shader == null)
                {
                    Log.ERROR("s_Mat is null. please check GLine.shader is include build.");
                    return;
                }

                s_Mat = new Material(shader);
                s_Mat.enableInstancing = true;

                // 内置的shader 不要改！！！
                //s_Mat.hideFlags = HideFlags.HideAndDontSave;
                //s_Mat.shader.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        void OnEnable()
        {
#if UNITY_2017_1_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
                UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += endCameraRendering;
            else
#endif
                Camera.onPostRender += OnCameraRender;
        }

        void OnDisable()
        {
#if UNITY_2017_1_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
                UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= endCameraRendering;
            else
#endif
                Camera.onPostRender -= OnCameraRender;
        }

#if UNITY_2017_1_OR_NEWER
        void endCameraRendering(UnityEngine.Rendering.ScriptableRenderContext src, Camera camera)
        {
            if (CheckFilter(camera))
                RenderLines(camera);
        }
#endif

        void OnCameraRender(Camera camera)
        {
            if (CheckFilter(camera))
                RenderLines(camera);
        }

        bool CheckFilter(Camera camera)
        {
            return camera == Camera.main;
        }

        void RenderLines(Camera camera)
        {
            if (!displayLines)
                return;

            if (s_Mat == null)
                return;

            if (m_Lines.Count > 0)
            {
                s_Mat.SetPass(0);

                GL.PushMatrix();
                GL.LoadProjectionMatrix(camera.projectionMatrix);
                GL.modelview = camera.worldToCameraMatrix;

                GL.Begin(GL.LINES);
                for (int num = m_Lines.Count - 1; num >= 0; num--)
                {
                    if (m_Lines[num].DurationElapsed(drawLine: true))
                    {
                        m_Lines.RemoveAt(num);
                    }
                }
                GL.End();
                GL.PopMatrix();
            }
        }

        static void DrawLine_Internal(Vector3 start, Vector3 end, Color color, float duration)
        {
            if ((duration != 0f || Instance.displayLines) && !(start == end))
            {
                Instance.m_Lines.Add(new Line(start, end, color, Time.time, duration));
            }
        }
    }
}