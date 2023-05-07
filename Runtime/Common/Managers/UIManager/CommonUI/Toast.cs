using Cysharp.Threading.Tasks;
using DG.Tweening;
using Saro.Core;
using Saro.Pool;
using Saro.Utility;
using System.Collections.Generic;
using System.Threading;
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
    }

    public partial class Toast
    {
        //通过这里来创建Toast预制体
        public static void AddToast(string content)
        {
        }

        /// <summary>
        /// TODO test
        /// </summary>
        public static void ClearAllToast()
        {
        }
    }
}