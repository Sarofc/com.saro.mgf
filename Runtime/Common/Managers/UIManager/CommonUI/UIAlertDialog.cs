using System;
using System.Collections;

namespace Saro.UI
{
    /// <summary>
    /// 消息信息
    /// </summary>
    public class AlertDialogInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string content;
        /// <summary>
        /// 
        /// </summary>
        public string title;

        /// <summary>
        /// 
        /// </summary>
        public string rightText;

        /// <summary>
        /// 
        /// </summary>
        public string leftText;

        /// <summary>
        ///
        /// <code>0: left 1: right</code>
        /// </summary>
        public Action<int> clickHandler;
    }

    [UIWindow((int)EDefaultUI.UIAlertDialog, "Assets/Res/Prefabs/UI/Default/UIAlertDialog.prefab")]
    public partial class UIAlertDialog : UIWindow, IEnumerator
    {
        public UIAlertDialog(string resPath) : base(resPath)
        {
        }

        public int ClickResult { get; private set; } = -1;

        public AlertDialogInfo AlertDialogInfo => UserData as AlertDialogInfo;

        protected override void Awake()
        {
            base.Awake();

            Listen(btn_left.onClick, OnClick_Left);
            Listen(btn_right.onClick, OnClick_Right);
        }

        protected override void OnShow()
        {
            base.OnShow();

            if (AlertDialogInfo == null)
            {
                Log.ERROR("MUST use UserData to set AlertDialogInfo");
                return;
            }

            var info = AlertDialogInfo;

            txt_content.text = info.content;
            txt_title.text = info.title;

            txt_left.text = info.leftText;

            if (!string.IsNullOrEmpty(info.rightText))
            {
                txt_right.text = info.rightText;
            }
            else
            {
                btn_right.gameObject.SetActive(false);
            }
        }

        private void OnClick_Left()
        {
            ClickResult = 0;
            AlertDialogInfo.clickHandler?.Invoke(ClickResult);
            AlertDialogInfo.clickHandler = null;

            UIManager.Current.HideWindow(EDefaultUI.UIAlertDialog);
        }

        private void OnClick_Right()
        {
            ClickResult = 1;
            AlertDialogInfo.clickHandler?.Invoke(ClickResult);
            AlertDialogInfo.clickHandler = null;

            UIManager.Current.HideWindow(EDefaultUI.UIAlertDialog);
        }

        #region IEnumerator Impl

        bool IEnumerator.MoveNext() => ClickResult == -1;

        void IEnumerator.Reset()
        { }

        object IEnumerator.Current => null;

        #endregion

    }

    public partial class UIAlertDialog
    {
        // =============================================
        // code generate between >>begin and <<end
        // don't modify this scope

        //>>begin
        public UnityEngine.UI.Text txt_title => Binder.GetRef<UnityEngine.UI.Text>("txt_title");
        public UnityEngine.UI.Text txt_content => Binder.GetRef<UnityEngine.UI.Text>("txt_content");
        public UnityEngine.UI.Button btn_left => Binder.GetRef<UnityEngine.UI.Button>("btn_left");
        public UnityEngine.UI.Text txt_left => Binder.GetRef<UnityEngine.UI.Text>("txt_left");
        public UnityEngine.UI.Button btn_right => Binder.GetRef<UnityEngine.UI.Button>("btn_right");
        public UnityEngine.UI.Text txt_right => Binder.GetRef<UnityEngine.UI.Text>("txt_right");

        //<<end

        // =============================================
    }
}
