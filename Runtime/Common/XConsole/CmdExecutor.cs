using System;
using System.Collections.Generic;
using System.Reflection;

namespace Saro.XConsole
{
    /// <summary>
    /// 参数解析委托
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    public delegate bool TypeParser(string input, out object output);

    /*
     *  Warning :
     *  Don't support command overload!
     *  One command string only bind one command!
     *
     *  example :
     *  int                 - CmdStr 1
     *  float               - CmdStr 1.1
     *  bool                - CmdStr false/True
     *  vector2             - CmdStr (1,1)         // no ' '
     *  vector3             - CmdStr 1,1,1         // '(' ')' is not necessary
     *  string//gameobject  - CmdStr actor
     */
    /// <summary>
    /// 处理用户命令
    /// </summary>
    public class CmdExecutor
    {
        // 命令map
        private SortedDictionary<string, CmdData> m_CommandMap;

        // 参数解析函数map
        private Dictionary<Type, TypeParser> m_TypeMap;

        // 解析后的参数队列，包括命令名称
        private Queue<string> m_Args;

        // 自动补全缓存
        private List<string> m_AutoCompleteCache;
        private int m_IdxOfCommandCache = -1;

        // 命令历史记录
        private LiteRingBuffer<string> m_CommandHistory;
        private int m_CommandIdx;

        /// <summary>
        /// 非法字符
        /// </summary>
        private readonly List<char> m_InvalidChrsForCommandName = new() { ' ', /*'-',*/ '/', '\\', '\b', '\t', };

        private Configs m_Configs;

        public CmdExecutor(Configs configs)
        {
            m_Configs = configs;

            // 初始化各种容器
            m_CommandMap = new SortedDictionary<string, CmdData>();
            m_Args = new Queue<string>(4);
            m_AutoCompleteCache = new List<string>(8);
            m_TypeMap = new Dictionary<Type, TypeParser>();
            m_CommandHistory = new LiteRingBuffer<string>(32);

            // 注册内置参数类型解析
            RegisterBuiltInTypeParsers();

            // 注册内置命令
            AddStaticCommands(typeof(BuiltInCommands));

            SetCommandHistory();
        }

        #region Command

        private void SetCommandHistory()
        {
            var list = m_Configs.cmdHistories;
            for (int i = 0; i < list.Count; i++)
            {
                var cmd = list[i];
                m_CommandHistory.AddTail(cmd);
            }
            m_CommandIdx = m_CommandHistory.Length;
        }

        /// <summary>
        /// 注册参数类型解析函数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fn"></param>
        internal void RegisterArgTypeParser(Type type, TypeParser fn)
        {
            if (m_TypeMap.ContainsKey(type))
            {
                Log.WARN("[XConsole] Already contains this type: " + type);
                return;
            }

            m_TypeMap.Add(type, fn);
        }


        /// <summary>
        /// 绑定指定类里的静态命令
        /// </summary>
        /// <param name="classType"></param>
        internal void AddStaticCommands(Type classType)
        {
            MethodInfo[] methods = classType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo method in methods)
            {
                if (method != null)
                {
                    XCommandAttribute attribute = method.GetCustomAttribute<XCommandAttribute>();
                    if (attribute != null)
                        InternalAddCommand(attribute.cmd, attribute.desc, method, null);
                }
            }
        }

        /// <summary>
        /// 移除给定类型的所有命令，包括static/instance
        /// </summary>
        /// <param name="classType"></param>
        internal void RemoveAllCommands(Type classType)
        {
            MethodInfo[] methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo method in methods)
            {
                if (method != null)
                {
                    XCommandAttribute attribute = method.GetCustomAttribute<XCommandAttribute>();
                    if (attribute != null)
                        RemoveCommand(attribute.cmd);
                }
            }
        }

        /// <summary>
        /// 绑定实例命令
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="instance"></param>
        internal void AddInstanceCommands(Type classType, object instance)
        {
            if (instance == null)
            {
                Log.ERROR("[XConsole] Instance couldn't be null!");
                return;
            }

            MethodInfo[] methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo method in methods)
            {
                if (method != null)
                {
                    XCommandAttribute attribute = method.GetCustomAttribute<XCommandAttribute>();
                    if (attribute != null)
                        InternalAddCommand(attribute.cmd, attribute.desc, method, instance);
                }
            }
        }

        /// <summary>
        /// 移除命令
        /// </summary>
        /// <param name="cmd">命令名称</param>
        internal void RemoveCommand(string cmd)
        {
            if (m_CommandMap.ContainsKey(cmd))
            {
                m_CommandMap.Remove(cmd);
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="cmdLine">包括命令名称以及参数</param>
        internal void ExecuteCommand(string cmdLine)
        {
            // parse command
            // [0] is command
            // others are parameters
            Queue<string> args = ParseCommandLine(cmdLine);
            if (args.Count <= 0)
            {
                Log.ERROR("[XConsole] Command shouln't be null");
                return;
            }

            // 除非是空串，不管命令是否有效，先都记录下来
            PushCommandHistory(cmdLine);

            string commandStr = args.Dequeue();

            if (!m_CommandMap.TryGetValue(commandStr, out CmdData command))
            {
                Log.ERROR($"[XConsole] Can't find this command : {commandStr}");
            }
            else if (!command.IsValid())
            {
                Log.ERROR($"[XConsole] This command is not valid : {commandStr}");
            }
            else
            {
                if (!command.IsParamsCountMatch(args.Count))
                {
                    Log.ERROR($"[XConsole] {commandStr} : Parameters count mismatch, expected count : {command.ParamsTypes.Count}. Type : {string.Join<object>(",", command.ParamsTypes)}");
                    return;
                }

                object[] paramters = new object[command.ParamsTypes.Count];
                for (int i = 0; i < command.ParamsTypes.Count; i++)
                {
                    if (!m_TypeMap.TryGetValue(command.ParamsTypes[i], out TypeParser typeParse))
                    {
                        Log.ERROR("[XConsole] This paramter type is unsupported : " + command.ParamsTypes[i].Name);
                        return;
                    }

                    if (typeParse?.Invoke(args.Peek(), out paramters[i]) == false)
                    {
                        Log.ERROR($"[XConsole] Can't parse {args.Peek()} to type {command.ParamsTypes[i].Name}");
                    }

                    args.Dequeue();
                }
                // call method
                command.Execute(paramters);
                Log.INFO($"<color=#F1831D>> {cmdLine}</color>");
            }
        }

        /// <summary>
        /// 获取所有命令
        /// </summary>
        /// <returns></returns>
        internal IReadOnlyCollection<CmdData> GetAllCommands()
        {
            return m_CommandMap.Values;
        }

        /// <summary>
        /// 尝试通过“命令名称”获取命令
        /// </summary>
        /// <param name="commandStr"></param>
        /// <returns></returns>
        internal CmdData TryGetCommand(string commandStr)
        {
            if (m_CommandMap.TryGetValue(commandStr, out CmdData command))
                return command;

            return null;
        }

        private void InternalAddCommand(string command, string description, MethodInfo methodInfo, object instance)
        {
            // check command name
            foreach (char chr in command)
            {
                if (m_InvalidChrsForCommandName.Contains(chr))
                {
                    Log.ERROR("[XConsole] invalid characters : " + string.Join(",", m_InvalidChrsForCommandName));
                    return;
                }
            }

            // check parameters
            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters == null) parameters = new ParameterInfo[0];

            Type[] paramTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                Type type = parameters[i].ParameterType;
                if (m_TypeMap.ContainsKey(type))
                {
                    paramTypes[i] = type;
                }
                else
                {
                    // command is not valid, return
                    Log.ERROR("[XConsole] Unsupported type : " + type);
                    return;
                }
            }

            // parse method info
            var sb = StringBuilderCache.Get();
            sb.AppendFormat("<color=red>{0}</color>", command).Append("\t - ");

            if (!string.IsNullOrEmpty(description)) sb.Append(description).Append("\t -> ");

            sb//.Append(methodInfo.DeclaringType.ToString()).Append(".")
              .AppendFormat("<color=yellow>{0}(</color>", methodInfo.Name);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                Type type = paramTypes[i];
                sb.AppendFormat("<color=yellow>{0}</color>", type.Name);
                if (i < paramTypes.Length - 1) sb.Append(",");
            }

            sb.Append("<color=yellow>)</color>").Append(" : ").Append(methodInfo.ReturnType.Name);

            // store to map
            m_CommandMap[command] = new CmdData(methodInfo, paramTypes, instance, StringBuilderCache.GetStringAndRelease(sb));
        }

        #endregion

        #region AutoComplete

        /// <summary>
        /// 命令自动补全
        /// </summary>
        /// <returns></returns>
        internal string AutoComplete()
        {
            if (m_AutoCompleteCache.Count == 0) return null;

            if (++m_IdxOfCommandCache >= m_AutoCompleteCache.Count) m_IdxOfCommandCache = 0;
            return m_AutoCompleteCache[m_IdxOfCommandCache];
        }

        internal IReadOnlyList<string> GetSuggestionCommand()
        {
            return m_AutoCompleteCache;
        }

        /// <summary>
        /// 获取可能的命令，并缓存下来
        /// </summary>
        /// <param name="header"></param>
        internal void CollectSuggestionCommand(string header)
        {
            if (string.IsNullOrEmpty(header))
            {
                throw new Exception("Header couldn't be Null or Empty");
            }

            m_IdxOfCommandCache = -1;
            m_AutoCompleteCache.Clear();

            foreach (string k in m_CommandMap.Keys)
            {
                if (k.StartsWith(header))
                {
                    m_AutoCompleteCache.Add(k);
                }
            }

            // ---------------------------------------------
            // TEST
            //if (m_autoCompleteCache.Count == 0) return;
            //var sb = new StringBuilder();
            //foreach (var str in m_autoCompleteCache)
            //{
            //    sb.Append(str).Append('\t');
            //}
            //Log(sb.ToString());
            // ---------------------------------------------
        }

        #endregion

        #region CommandHistory

        internal string GetPrevCommand()
        {
            if (--m_CommandIdx < 0)
            {
                m_CommandIdx = 0;
            }

            return m_CommandHistory[m_CommandIdx];
        }

        internal string GetNextCommand()
        {
            if (++m_CommandIdx >= m_CommandHistory.Length)
            {
                m_CommandIdx = 0;
                m_CommandIdx = m_CommandHistory.Length;

                return string.Empty;
            }

            return m_CommandHistory[m_CommandIdx];
        }

        // 清除
        internal void ClearCommandHistory()
        {
            m_CommandHistory.FastClear();
        }

        // 命令历史缓存
        private void PushCommandHistory(string cmd)
        {
            m_CommandHistory.AddTail(cmd);
            m_CommandIdx = m_CommandHistory.Length;

            // config
            m_Configs.cmdHistories.Clear();
            for (int i = 0; i < m_CommandHistory.Length; i++)
            {
                m_Configs.cmdHistories.Add(m_CommandHistory[i]);
            }
        }

        #endregion

        #region Parse Args

        private void RegisterBuiltInTypeParsers()
        {
            RegisterArgTypeParser(typeof(string), ParseString);
            RegisterArgTypeParser(typeof(int), ParseInt);
            RegisterArgTypeParser(typeof(float), ParseFlot);
            RegisterArgTypeParser(typeof(bool), ParseBool);
            RegisterArgTypeParser(typeof(UnityEngine.Vector2), ParseVector2);
            RegisterArgTypeParser(typeof(UnityEngine.Vector3), ParseVector3);
            RegisterArgTypeParser(typeof(UnityEngine.Vector4), ParseVector4);
            RegisterArgTypeParser(typeof(UnityEngine.GameObject), ParseGameobject);
        }

        private Queue<string> ParseCommandLine(string input)
        {
            m_Args.Clear();
            if (input == null) return m_Args;
            input.Trim();
            if (input.Length == 0) return m_Args;

            string[] args = input.Split(' ');
            foreach (string arg in args)
            {
                string tmp = arg.Trim();
                if (string.IsNullOrEmpty(tmp)) continue;
                m_Args.Enqueue(tmp);
            }

            return m_Args;
        }

        private bool ParseString(string input, out object output)
        {
            output = input;
            return true;
        }

        private bool ParseInt(string input, out object output)
        {
            var result = int.TryParse(input, out int value);
            output = value;
            return result;
        }

        private bool ParseFlot(string input, out object output)
        {
            var result = float.TryParse(input, out float value);
            output = value;
            return result;
        }

        private bool ParseBool(string input, out object output)
        {
            var result = bool.TryParse(input, out bool value);
            output = value;
            return result;
        }

        // (x,y)
        private bool ParseVector2(string input, out object output)
        {
            output = UnityEngine.Vector2.zero;

            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            string[] xy = input.Replace('(', ' ').Replace(')', ' ').Split(',');

            if (xy.Length == 2)
            {
                string tmpX = xy[0].Trim();
                if (string.IsNullOrEmpty(tmpX)) return false;

                string tmpY = xy[1].Trim();
                if (string.IsNullOrEmpty(tmpY)) return false;

                if (!float.TryParse(tmpX, out float x)) return false;

                if (!float.TryParse(tmpY, out float y)) return false;

                output = new UnityEngine.Vector2(x, y);
            }

            return false;
        }

        // (x,y,z)
        private bool ParseVector3(string input, out object output)
        {
            output = UnityEngine.Vector3.zero;

            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            string[] xyz = input.Replace('(', ' ').Replace(')', ' ').Split(',');

            if (xyz.Length == 3)
            {
                string tmpX = xyz[0].Trim();
                if (string.IsNullOrEmpty(tmpX)) return false;

                string tmpY = xyz[1].Trim();
                if (string.IsNullOrEmpty(tmpY)) return false;

                string tmpZ = xyz[2].Trim();
                if (string.IsNullOrEmpty(tmpZ)) return false;

                if (!float.TryParse(tmpX, out float x)) return false;

                if (!float.TryParse(tmpY, out float y)) return false;

                if (!float.TryParse(tmpZ, out float z)) return false;

                output = new UnityEngine.Vector3(x, y, z);
                return true;
            }

            return false;
        }

        private bool ParseVector4(string input, out object output)
        {
            output = UnityEngine.Vector4.zero;

            if (string.IsNullOrEmpty(input))
            {
                output = new UnityEngine.Vector4();
                return false;
            }

            string[] xyzw = input.Replace('(', ' ').Replace(')', ' ').Split(',');

            if (xyzw.Length == 4)
            {
                string tmpX = xyzw[0].Trim();
                if (string.IsNullOrEmpty(tmpX)) return false;

                string tmpY = xyzw[1].Trim();
                if (string.IsNullOrEmpty(tmpY)) return false;

                string tmpZ = xyzw[2].Trim();
                if (string.IsNullOrEmpty(tmpZ)) return false;

                string tmpW = xyzw[3].Trim();
                if (string.IsNullOrEmpty(tmpZ)) return false;

                if (!float.TryParse(tmpX, out float x)) return false;

                if (!float.TryParse(tmpY, out float y)) return false;

                if (!float.TryParse(tmpZ, out float z)) return false;

                if (!float.TryParse(tmpW, out float w)) return false;

                output = new UnityEngine.Vector4(x, y, z, w);
                return true;
            }

            return false;
        }

        private bool ParseGameobject(string input, out object output)
        {
            output = UnityEngine.GameObject.Find(input);
            if (output != null) return true;
            else return false;
        }

        #endregion
    }
}
