namespace Saro
{
    public enum ServerType
    {
        Game,
        Watcher,
    }

    [System.Serializable]
    public class Options
    {
        /* [UnityEngine.SerializeField]*/
        private ServerType m_ServerType;
        /* [UnityEngine.SerializeField]*/
        private int m_Process = 1;
        /* [UnityEngine.SerializeField]*/
        private int m_Develop = 0;
        /* [UnityEngine.SerializeField]*/
        private int m_LogLevel = 2;

        // TODO CommandLine nuget

        //[Option("ServerType", Required = false, Default = ServerType.Game, HelpText = "serverType enum")]
        public ServerType ServerType { get => m_ServerType; set => m_ServerType = value; }

        //[Option("Process", Required = false, Default = 1)]
        public int Process { get => m_Process; set => m_Process = value; }
        //[Option("Develop", Required = false, Default = 0, HelpText = "develop mode, 0正式 1开发 2压测")]
        public int Develop { get => m_Develop; set => m_Develop = value; }
        //[Option("LogLevel", Required = false, Default = 0)]
        public int LogLevel { get => m_LogLevel; set => m_LogLevel = value; }
    }
}