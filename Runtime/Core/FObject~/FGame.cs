using System;
using System.Collections.Generic;

namespace Saro
{
    public static class FGame
    {
        public static ThreadSynchronizationContext ThreadSynchronizationContext => ThreadSynchronizationContext.Get();

        public static TimeInfo TimeInfo => TimeInfo.Instance;

        public static FEventSystem EventSystem => FEventSystem.Get();

        private static FScene s_Scene;
        public static FScene Scene
        {
            get
            {
                if (s_Scene != null)
                {
                    return s_Scene;
                }
                InstanceIDStruct instanceIdStruct = new InstanceIDStruct(s_Options.Process, 0);
                s_Scene = EntitySceneFactory.CreateScene(instanceIdStruct.ToLong(), 0, SceneType.Process, "Process");
                return s_Scene;
            }
        }

        public static IDGenerater IdGenerater => IDGenerater.Instance;

        public static Options s_Options;

        public static List<Action> s_FrameFinishCallback = new List<Action>();

        /// <summary>
        /// 注册全局组件，用来替代单例！
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Register<T>() where T : FEntity, new()
        {
            return Scene.AddComponent<T>(false);
        }

        /// <summary>
        /// 注册全局组件，用来替代单例！
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Register<T, C>(C c1) where T : FEntity, new()
        {
            return Scene.AddComponent<T, C>(c1, false);
        }

        /// <summary>
        /// 获取全局组件，用来替代单例！
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Resolve<T>() where T : FEntity
        {
            if (Scene.IsDisposed) throw new Exception("Scene has been disposed");

            var ret = Scene.GetComponent<T>();

            if (ret == null)
            {
                Log.ERROR($"请先 FGame.Register {typeof(T)} 组件");
            }

            return ret;
        }

        /// <summary>
        /// 反注册全局组件，用来替代单例！
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Unregister<T>() where T : FEntity, new()
        {
            if (Scene.IsDisposed) throw new Exception("Scene  has been disposed");

            Scene.RemoveComponent<T>();
        }

        /// <summary>
        /// 游戏循环
        /// </summary>
        public static void Update()
        {
            ThreadSynchronizationContext.Update();
            TimeInfo.Update();
            EventSystem.Update();
        }

        /// <summary>
        /// 游戏循环，在update后调用
        /// </summary>
        public static void LateUpdate()
        {
            EventSystem.LateUpdate();
        }

        /// <summary>
        /// 游戏循环 每帧结束回调
        /// </summary>
        public static void FrameFinish()
        {
            foreach (Action action in s_FrameFinishCallback)
            {
                action.Invoke();
            }
            s_FrameFinishCallback.Clear();
        }

        /// <summary>
        /// 关闭游戏程序
        /// </summary>
        public static void Close()
        {
            s_Scene?.Dispose();
            s_Scene = null;
            SharedPool.ClearAll();
            EventSystem.Dispose();
            IdGenerater.Dispose();
        }

#if UNITY_EDITOR
        public static void DumpEntityTree()
        {
            var builder = new System.Text.StringBuilder(512);
            __print(Scene, builder, "   ", Scene.Children.Count > 1 ? false : true, true);
            UnityEngine.Debug.LogError(builder.ToString());
        }

        private static void __print(FEntity root, System.Text.StringBuilder builder, string indent, bool isLastNode, bool isRoot = false)
        {
            if (root == null) return;

            var children = new List<FEntity>();
            children.AddRange(root.Children.Values);
            children.AddRange(root.Components.Values);

            if (!isRoot)
            {
                builder.AppendLine();
                builder.Append(indent);

                builder.Append(isLastNode ? "└──" : "├──");

                if (root.IsCreate)
                {
                    builder.Append(root.IsComponent ? "[C]" : "[E]");
                    builder.Append(root.GetType().Name);
                }
                else
                    builder.Append("Invalid");

                if (isLastNode)
                    indent += "     ";
                else
                    indent += "├    ";
            }
            else
            {
                builder.Append(root.IsComponent ? "[C]" : "[E]");
                builder.Append(root.GetType().Name);
            }

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                isLastNode = i == children.Count - 1;
                __print(child, builder, indent, isLastNode);
            }
        }
#endif
    }
}
