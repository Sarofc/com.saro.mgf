using UnityEditor;
using UnityEngine;

namespace Saro.SEditor
{

    public class AnimationPreviewEditor : Editor
    {
        private ModelPreview m_modelPreview;
        private AnimationClip m_clip;
        public AnimationClip clip
        {
            get
            {
                return m_clip;
            }
            set
            {
                if (m_clip == null || !m_clip.Equals(value))
                {
                    m_clip = value;
                    ModelPreview.fps = Mathf.RoundToInt(m_clip.length * m_clip.frameRate);
                    ModelPreview.TimeController.startTime = 0f;
                    ModelPreview.TimeController.stopTime = m_clip.length;
                }
            }
        }

        public ModelPreview ModelPreview
        {
            get
            {
                if (m_modelPreview == null) m_modelPreview = new ModelPreview();
                return m_modelPreview;
            }
        }

        public override void OnPreviewSettings()
        {
            ModelPreview.DoPreviewSettings();
        }

        public override GUIContent GetPreviewTitle()
        {
            if (clip)
            {
                return new GUIContent(clip.name);
            }
            else
            {
                return new GUIContent("null");
            }
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            bool isRepaint = (Event.current.type == EventType.Repaint);

            if (isRepaint) ModelPreview.TimeController.Update();

            // Set settings
            ModelPreview.TimeController.loop = true; // always looping, waiting for UI ctrl...

            // Sample Animation
            if (isRepaint && clip != null && ModelPreview.PreviewObject != null)
            {
                clip.SampleAnimation(ModelPreview.PreviewObject, ModelPreview.TimeController.currentTime);
            }

            ModelPreview.DoAvatorPreview(r, background);
        }

        private void OnDisable()
        {
            if (m_modelPreview != null)
            {
                m_modelPreview.OnDisable();
            }
        }
    }

}