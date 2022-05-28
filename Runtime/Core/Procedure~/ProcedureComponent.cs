using System;
using System.Collections.Generic;
using UnityEngine;
using MGF;

namespace Saro.Core
{
    [ObjectSystem]
    public sealed class ProcedureComponentAwakeSystem : AwakeSystem<ProcedureComponent>
    {
        public override void Awake(ProcedureComponent self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public sealed class ProcedureComponentUpdateSystem : UpdateSystem<ProcedureComponent>
    {
        public override void Update(ProcedureComponent self)
        {
            self.Update();
        }
    }

    /*
        游戏状态管理

        贯穿整个游戏程序的生命周期
    */
    public sealed partial class ProcedureComponent : Entity/* : IHasUpdate, IService*/
    {
        public const string k_Assembly = "Assembly-CSharp";
        public const string k_ProcedureSettingsPath = "Assets/Res/ScriptableObjects/ProcedureMgr/ProcedureSettings.asset";

        public string Start => m_ProcedureSettings.start;
        private List<string> ProcedureList => m_ProcedureSettings.procedureList;
        private ProcedureSettings m_ProcedureSettings;

        private Type m_StartType;
        private FSM<AProcedureBase> m_ProcedureFSM;
        private Dictionary<Type, AProcedureBase> m_ProcedureLut;
        private float m_InitialTime;

        public static ProcedureComponent Get()
        {
            return Game.Resolve<ProcedureComponent>();
        }

        /// <summary>
        /// 进入第一个流程
        /// </summary>
        public void ToEntry()
        {
            ToProcedure(m_StartType);
        }

        /// <summary>
        /// 改变流程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ToProcedure<T>() where T : AProcedureBase
        {
            ToProcedure(typeof(T));
        }

        /// <summary>
        /// 改变流程
        /// </summary>
        /// <param name="procedureType"></param>
        public void ToProcedure(Type procedureType)
        {
            if (m_ProcedureLut.TryGetValue(procedureType, out AProcedureBase procedure))
            {
                m_ProcedureFSM.CurrentState = procedure;
            }
            else
            {
                Log.WARN("ProcedureMgr", "Type Error: " + procedureType.ToString());
            }
        }

        /// <summary>
        /// 添加流程
        /// </summary>
        /// <param name="procedure"></param>
        public void AddProcedure(AProcedureBase procedure)
        {
            if (m_ProcedureFSM.ContainsState(procedure))
            {
                Log.WARN("ProcedureMgr", "Already contains: " + procedure.GetType().Name);
                return;
            }

            m_ProcedureFSM.Add(procedure)
                 .OnEnter(procedure.OnEnter)
                 .OnUpdate(procedure.OnUpdate)
                 .OnLeave(procedure.OnLeave);

            m_ProcedureLut.Add(procedure.GetType(), procedure);
        }

        /// <summary>
        /// 获取当前流程
        /// </summary>
        /// <returns></returns>
        public Type CurrentProcedure => m_ProcedureFSM.CurrentState == null ? default(Type) : m_ProcedureFSM.CurrentState.GetType();

        /// <summary>
        /// 获取所有流程状态
        /// </summary>
        /// <returns></returns>
        public Dictionary<Type, AProcedureBase>.KeyCollection GetAllProcedures => m_ProcedureLut.Keys;

        /// <summary>
        /// 是否包含这个流程
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsProcedure(Type key)
        {
            return m_ProcedureLut.ContainsKey(key);
        }

        /// <summary>
        /// 获取流程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetProcedure<T>() where T : AProcedureBase
        {
            if (m_ProcedureLut.TryGetValue(typeof(T), out AProcedureBase procedure))
            {
                return (T)procedure;
            }

            return default;
        }

        /// <summary>
        /// 获取流程
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public AProcedureBase GetProcedure(Type type)
        {
            if (m_ProcedureLut.TryGetValue(type, out AProcedureBase procedure))
            {
                return procedure;
            }

            return default;
        }

        public void Update()
        {
            if (m_ProcedureFSM.CurrentState == null) return;

            // 流程状态机不应该受到 timescale 的影响
            m_ProcedureFSM.DoUpdate(Time.realtimeSinceStartup - m_InitialTime);
        }

        // TODO 需要实现初始化
        // 思考下这个流程状态及是否还有用？
        public void Awake()
        {
            var asset = Game.Resolve<EntityAssetInterface>().LoadAsset(k_ProcedureSettingsPath, typeof(ProcedureSettings));

            if (asset != null && asset.Asset != null)
            {
                m_ProcedureSettings = asset.Asset as ProcedureSettings;
            }

            if (m_ProcedureSettings == null) Log.ERROR("ProcedureMgr", "Loading settings fail!");

            m_InitialTime = Time.realtimeSinceStartup;

            CreateProcedureInstances();
        }

        private void CreateProcedureInstances()
        {
            m_ProcedureFSM = new FSM<AProcedureBase>("Procedure FSM");
            m_ProcedureLut = new Dictionary<Type, AProcedureBase>();

            for (int i = 0; i < ProcedureList.Count; i++)
            {
                string typeName = ProcedureList[i];
                Type type = Utility.RefelctionUtility.GetType(k_Assembly, typeName);
                if (type == null)
                {
                    Log.ERROR("ProcedureMgr", "type is null: " + typeName);
                }

                AProcedureBase procedure = (AProcedureBase)Activator.CreateInstance(type);
                if (procedure == null)
                {
                    Log.WARN("ProcedureMgr", "null procedure: " + typeName);
                    continue;
                }

                if (typeName == Start) m_StartType = type;

                AddProcedure(procedure);
            }

            if (m_StartType == null)
            {
                Log.ERROR("ProcedureMgr", "null start procedure: " + Start);
            }
        }
    }
}