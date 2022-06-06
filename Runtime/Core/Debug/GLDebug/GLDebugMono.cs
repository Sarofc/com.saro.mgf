using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saro.Diagnostics
{
    [RequireComponent(typeof(Camera))]
    public class GLDebugMono : MonoBehaviour
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

        private static GLDebugMono s_Instance = null;

        private static Material s_Mat = null;

        private List<Line> m_Lines;

        public bool displayLines = true;

        public Shader zOnShader;

        public static GLDebugMono Instance
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

                    s_Instance = main.gameObject.AddComponent<GLDebugMono>();
                }

                return s_Instance;
            }
        }

        private void Awake()
        {
            SetMaterial();
            m_Lines = new List<Line>();
            if (s_Instance == null)
            {
                s_Instance = this;
            }
            else if (s_Instance != this)
            {
                UnityEngine.Object.Destroy(this);
            }
        }

        private void SetMaterial()
        {
            if (s_Mat == null)
            {
                if (zOnShader == null)
                {
                    Shader shader = Shader.Find("Sprites/Default");
                    s_Mat = new Material(shader);
                }
                else
                {
                    s_Mat = new Material(zOnShader);
                }

                // 内置的shader 不要改！！！
                //s_Mat.hideFlags = HideFlags.HideAndDontSave;
                //s_Mat.shader.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        protected void OnEnable()
        {
#if UNITY_2017_1_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
                UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += endCameraRendering;
            else
#endif
                Camera.onPostRender += OnCameraRender;
        }

        protected void OnDisable()
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
                RenderLines();
        }
#endif

        protected void OnCameraRender(Camera camera)
        {
            if (CheckFilter(camera))
                RenderLines();
        }

        bool CheckFilter(Camera camera)
        {
            return camera == Camera.main;
        }

        private void RenderLines()
        {
            if (!displayLines)
            {
                return;
            }

            GL.PushMatrix();
            s_Mat.SetPass(0);
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

        public static void DrawLine(Vector3 start, Vector3 end, Color? color = null, float duration = 0f)
        {
            DrawLine_Internal(start, end, color ?? Color.white, duration);
        }

        public static void DrawRay(Vector3 start, Vector3 dir, Color? color = null, float duration = 0f)
        {
            if (!(dir == Vector3.zero))
            {
                DrawLine(start, start + dir, color, duration);
            }
        }

        private static void DrawLine_Internal(Vector3 start, Vector3 end, Color color, float duration)
        {
            if ((duration != 0f || Instance.displayLines) && !(start == end))
            {
                Instance.m_Lines.Add(new Line(start, end, color, Time.time, duration));
            }
        }
    }
}