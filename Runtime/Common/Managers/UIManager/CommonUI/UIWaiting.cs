using UnityEngine;

namespace Saro.UI
{
    // TODO UIManager 需要一个将ui调整到顶层的函数

    [UIWindow((int)EDefaultUI.UIWaiting, "Assets/Res/Prefabs/UI/Default/UIWaiting.prefab")]
    public sealed partial class UIWaiting : UIWindow
    {
        protected override void Awake()
        {
            base.Awake();

            Listen(btn_cancel.onClick, () => UIManager.Current.HideWindow(EDefaultUI.UIWaiting));
        }

        protected override void OnShow()
        {
            base.OnShow();

            m_DefaultColor = img_bg.color;

            SetDefault();
        }

        protected override void OnUpdate(float dt)
        {
            float nowTime = Time.realtimeSinceStartup;

            if (m_ShowUITime > 0f && nowTime >= m_ShowUITime)
            {
                m_ShowUITime = -1;

                img_bg.color = m_DefaultColor;
                go_root.SetActive(true);
            }

            if (m_Timeout > 0f && nowTime >= m_Timeout)
            {
                btn_cancel.gameObject.SetActive(true);
            }

            if (m_IntervalTiemr <= 0)
            {
                img_wait.transform.Rotate(Vector3.forward, m_Angle);

                m_IntervalTiemr += m_Interval;
            }
            else
            {
                m_IntervalTiemr -= dt;
            }
        }

        private float m_Angle = -360 / 12f;
        private float m_Interval = 0.1f;
        private float m_IntervalTiemr;

        private const float k_SHOW_UI_DELAY = 0.5f;

        private const float k_TIMEOUT_DELAY = 10f;

        private float m_ShowUITime = -1f;

        private float m_Timeout = -1f;

        private Color m_DefaultColor;

        public UIWaiting(string resPath) : base(resPath)
        {
        }

        public void SetEntry(string entry)
        {
            txt_userinfo.text += "\n" + entry;
        }

        public void SetDefault()
        {
            var color = img_bg.color;
            color.a = 0f;
            img_bg.color = color;
            go_root.SetActive(false);
            btn_cancel.gameObject.SetActive(false);

            m_ShowUITime = -1;
            m_Timeout = -1;
        }

        public void Refresh()
        {
            Root.transform.SetAsLastSibling();

            float startTime = Time.realtimeSinceStartup;
            m_ShowUITime = startTime + k_SHOW_UI_DELAY;
            m_Timeout = startTime + k_TIMEOUT_DELAY;
        }
    }


    public partial class UIWaiting
    {
        // =============================================
        // code generate between >>begin and <<end
        // don't modify this scope

        //>>begin
        public UnityEngine.UI.Image img_bg => Binder.GetRef<UnityEngine.UI.Image>("img_bg");
        public UnityEngine.GameObject go_root => Binder.GetRef<UnityEngine.GameObject>("go_root");
        public UnityEngine.UI.Image img_wait => Binder.GetRef<UnityEngine.UI.Image>("img_wait");
        public UnityEngine.UI.Text txt_wait => Binder.GetRef<UnityEngine.UI.Text>("txt_wait");
        public UnityEngine.UI.Button btn_cancel => Binder.GetRef<UnityEngine.UI.Button>("btn_cancel");
        public UnityEngine.UI.Text txt_cancel => Binder.GetRef<UnityEngine.UI.Text>("txt_cancel");
        public UnityEngine.UI.Text txt_userinfo => Binder.GetRef<UnityEngine.UI.Text>("txt_userinfo");

        //<<end
        // =============================================
    }
}