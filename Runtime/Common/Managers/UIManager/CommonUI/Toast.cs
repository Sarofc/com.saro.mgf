using Cysharp.Threading.Tasks;
using DG.Tweening;
using Saro.Core;
using Saro.Pool;
using Saro.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Saro.UI
{
    /*
     * TODO 
     * 
     * 使用 Component 重构
     * 
     */
    public sealed partial class Toast : MonoBehaviour
    {
        public Image image;
        public Text text;

        //初始化
        private void InitToast(string content)
        {
            transform.SetParent(UIManager.Current.Top.transform);
            transform.localPosition = Vector3.zero;

            var color = image.color;
            color.a = 1f;
            image.color = color;

            color = text.color;
            color.a = 1f;
            text.color = color;

            text.text = content;
            int StrLenth = content.Length;
            int ToastWidth;
            if (StrLenth > 10)
            {
                ToastWidth = 10;
            }
            else
            {
                ToastWidth = StrLenth;
            }

            gameObject.SetActive(true);

            s_ToastList.Insert(0, this);

            image.rectTransform.sizeDelta = new Vector2(50 * ToastWidth, 50);
            FadeOut();
        }

        private void FadeOut()
        {
            image.DOFade(0, 2).OnComplete(() =>
            {
                Toast.Release(this);
            });
            text.DOFade(0, 2);
        }

        //堆叠向上移动
        private void Move(float speed, int targetPos)
        {
            transform.DOLocalMoveY(targetPos * image.rectTransform.sizeDelta.y, speed);
        }
    }

    public partial class Toast
    {
        #region Toast Manager

        private static List<Toast> s_ToastList = new List<Toast>();

        //通过这里来创建Toast预制体
        public static void AddToast(string content)
        {
            //有新的Toast出现，之前的Toast向上移动
            Toast.Create(content).ContinueWith(_ => ToastMove(0.2f)).Forget();
        }

        /// <summary>
        /// TODO test
        /// </summary>
        public static void ClearAllToast()
        {
            s_ToastList.Clear();
            s_ObjectPool.Clear();

            GameObject.Destroy(s_ToastPoolRoot);
        }

        private static void ToastMove(float speed)
        {
            for (int i = 0; i < s_ToastList.Count - 1; i++)
            {
                s_ToastList[i].Move(speed, i + 1);
            }
        }

        #endregion
    }

    public partial class Toast
    {
        #region Object Manager

        public static string s_AssetName = "Assets/Res/Prefab/UI/Default/Toast.prefab";

        private static Transform s_ToastPoolRoot;

        private static IObjectPool<Toast> s_ObjectPool = new ObjectPool<Toast>(OnCreateAsync, null, OnRelease, OnDestroy, true, 10, 20);

        private static async UniTask<Toast> OnCreateAsync()
        {
            if (s_ToastPoolRoot == null)
            {
                s_ToastPoolRoot = new GameObject("ToastPoolRoot").transform;
                s_ToastPoolRoot.gameObject.SetActive(false);

                GameObject.DontDestroyOnLoad(s_ToastPoolRoot.gameObject);
            }

            // TODO
            var prefab = await IAssetManager.Current.LoadAssetAsync(s_AssetName, typeof(Toast)) as Toast;
            var toast = GameObject.Instantiate(prefab, UIManager.Current.Top.transform);

            var canvas = toast.GetOrAddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = (int)EUILayer.Top + 3000;

            return toast;
        }

        private static void OnRelease(Toast obj)
        {
            if (s_ToastPoolRoot != null)
            {
                obj.transform.SetParent(s_ToastPoolRoot);
            }
            else
            {
                obj.gameObject.SetActive(false);
            }
            s_ToastList.Remove(obj);
        }

        private static void OnDestroy(Toast obj)
        {
            GameObject.Destroy(obj.gameObject);
        }

        private static async UniTask<Toast> Create(string content)
        {
            var toast = await s_ObjectPool.RentAsync();

            toast.InitToast(content);

            return toast;
        }

        private static void Release(Toast toast)
        {
            s_ObjectPool.Return(toast);
        }

        #endregion
    }
}