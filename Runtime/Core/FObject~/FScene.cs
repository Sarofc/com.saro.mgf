namespace Saro
{
    public enum SceneType
    {
        Process = 0,
        Manager = 1,
        Realm = 2,
        Gate = 3,
        Http = 4,
        Location = 5,
        Map = 6,

        // 客户端Model层
        Client = 30,
        Zone = 31,
        Login = 32,
    }

    public sealed class FScene : FEntity
    {
        public int Zone
        {
            get;
        }

        public SceneType SceneType
        {
            get;
        }

        public string Name
        {
            get;
            set;
        }

        public FScene(long id, int zone, SceneType sceneType, string name, FEntity parent)
        {
            ID = id;
            InstanceID = id;
            Zone = zone;
            SceneType = sceneType;
            Name = name;
            IsCreate = true;
            IsRegister = true;
            Parent = parent;
            Domain = this;

            Log.INFO("[Scene]", $"scene create: {SceneType} {Name} {ID} {InstanceID} {Zone}");
        }

        public FScene(long id, long instanceId, int zone, SceneType sceneType, string name, FEntity parent)
        {
            ID = id;
            InstanceID = instanceId;
            Zone = zone;
            SceneType = sceneType;
            Name = name;
            IsCreate = true;
            IsRegister = true;
            Parent = parent;
            Domain = this;

            Log.INFO("[Scene]", $"scene create: {SceneType} {Name} {ID} {InstanceID} {Zone}");
        }

        public override void Dispose()
        {
            base.Dispose();

            Log.INFO("Scene", $"scene dispose: {SceneType} {Name} {ID} {InstanceID} {Zone}");
        }

        public FScene Get(long id)
        {
            if (Children == null)
            {
                return null;
            }

            if (!Children.TryGetValue(id, out FEntity entity))
            {
                return null;
            }

            return entity as FScene;
        }

        public new FEntity Domain
        {
            get => m_Domain;
            set => m_Domain = value;
        }

        public new FEntity Parent
        {
            get
            {
                return m_Parent;
            }
            set
            {
                if (value == null)
                {
                    m_Parent = this;
                    return;
                }

                m_Parent = value;
                m_Parent.Children.Add(ID, this);
#if UNITY_EDITOR && VIEWGO
                if (this.ViewGO != null)
                {
                    this.ViewGO.transform.SetParent(this.parent.ViewGO.transform, false);
                }
#endif
            }
        }
    }
}