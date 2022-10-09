﻿using Saro.Pool;
using UnityEngine;

namespace Saro.Gameplay.Effect
{
    public class VfxScriptBase : MonoBehaviour, IHandledObject
    {
        public int ObjectID { get; internal set; }

        /// <summary>
        /// 有限制，特效不能同名
        /// </summary>
        public string EffectName { get; internal set; }

        public ControlPoint[] cps;
        public ControlEntity[] ces;

        public virtual void Init()
        {

        }

        public virtual void Clean()
        {
            ObjectID = 0;
        }
    }
}
