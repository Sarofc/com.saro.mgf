using Cysharp.Threading.Tasks;
using Saro.Collections;
using Saro.Pool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Saro.UI
{
    public enum EUILayer
    {
        Bottom = 1000,
        Center = 4000,
        Top = 8000,
    }

    [System.Obsolete("已废弃，请使用其他UI框架")]
    public partial class UIManager
    {
        public static UIManager Current => Main.Resolve<UIManager>();

        [System.Obsolete("use ‘Current’ instead")]
        public static UIManager Instance => Current;

        public T GetWindow<T>(Enum ui)
        {
            throw new NotImplementedException();
        }

        public void HideWindow(Enum ui)
        {
            throw new NotImplementedException();
        }

        public T LoadAndShowWindow<T>(Enum ui, EUILayer layer)
        {
            throw new NotImplementedException();
        }

        public Task<T> LoadAndShowWindowAsync<T>(Enum ui)
        {
            throw new NotImplementedException();
        }

        public Task<T> LoadAndShowWindowAsync<T>(Enum ui, EUILayer layer)
        {
            throw new NotImplementedException();
        }

        public Task LoadWindowAsync(Enum ui)
        {
            throw new NotImplementedException();
        }

        public void QueueAsync(Enum ui, int v, object userData, EUILayer layer)
        {
            throw new NotImplementedException();
        }

        public void QueueAsync(Enum ui, int v, object userData)
        {
            throw new NotImplementedException();
        }

        public T ShowWindow<T>(Enum ui, EUILayer layer)
        {
            throw new NotImplementedException();
        }
    }

    partial class UIManager : IService
    {
    }
}
