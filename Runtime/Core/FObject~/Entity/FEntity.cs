using Saro.Pool;
using System;
using System.Collections.Generic;

namespace Saro
{
    [Flags]
    public enum EEntityStatus : byte
    {
        None = 0,
        IsFromPool = 1,
        IsRegister = 1 << 1,
        IsComponent = 1 << 2,
        IsCreate = 1 << 3,
    }

    /*
     * TODO 
     * 
     * 同步 ET 的更改
     * 
     * 1. 如何序列化？
     * 2. 增加debug面板
     *    - entity数据
     *    - eventsystem数据
     */
    public partial class FEntity : FObject, IReference
    {
        public long InstanceID { get; protected set; }

        public FEntity() { }

        private EEntityStatus m_Status = EEntityStatus.None;

        private bool IsFromPool
        {
            get => (m_Status & EEntityStatus.IsFromPool) == EEntityStatus.IsFromPool;
            set
            {
                if (value)
                {
                    m_Status |= EEntityStatus.IsFromPool;
                }
                else
                {
                    m_Status &= ~EEntityStatus.IsFromPool;
                }
            }
        }

        protected bool IsRegister
        {
            get => (m_Status & EEntityStatus.IsRegister) == EEntityStatus.IsRegister;
            set
            {
                if (IsRegister == value)
                {
                    return;
                }

                if (value)
                {
                    m_Status |= EEntityStatus.IsRegister;
                }
                else
                {
                    m_Status &= ~EEntityStatus.IsRegister;
                }

                FEventSystem.Get().RegisterSystem(this, value);
            }
        }

        public bool IsComponent
        {
            get => (m_Status & EEntityStatus.IsComponent) == EEntityStatus.IsComponent;
            private set
            {
                if (value)
                {
                    m_Status |= EEntityStatus.IsComponent;
                }
                else
                {
                    m_Status &= ~EEntityStatus.IsComponent;
                }
            }
        }

        public bool IsCreate
        {
            get => (m_Status & EEntityStatus.IsCreate) == EEntityStatus.IsCreate;
            protected set
            {
                if (value)
                {
                    m_Status |= EEntityStatus.IsCreate;
                }
                else
                {
                    m_Status &= ~EEntityStatus.IsCreate;
                }
            }
        }

        public bool IsDisposed => InstanceID == 0;

        /// <summary>
        /// 父节点，非Component
        /// <code>与 ComponentParent 公用 m_Parent </code>
        /// <code>可以改变parent，但是不能设置为null</code>
        /// </summary>
        public FEntity Parent
        {
            get => m_Parent;
            set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {GetType().Name}");
                }

                if (value == this)
                {
                    throw new Exception($"can't set parent self: {GetType().Name}");
                }

                if (value.Domain == null)
                {
                    throw new Exception($"can't set parent because parent' domain is null: {GetType().Name} {value.GetType().Name}");
                }

                if (m_Parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (m_Parent == value)
                    {
                        ERROR($"already set a same parent: {GetType().Name} parent: {m_Parent.GetType().Name}");
                        return;
                    }

                    m_Parent.RemoveChild(this);
                }

                m_Parent = value;
                m_Parent.AddChild(this);
                IsComponent = false;
                Domain = m_Parent.m_Domain;
            }
        }

        /// <summary>
        /// Component 父节点
        /// <code>与 Parent 公用 m_Parent </code>
        /// <code>该方法只能在AddComponent中调用，其他人不允许调用</code>
        /// </summary>
        private FEntity ComponentParent
        {
            set
            {
                if (m_Parent != null)
                {
                    throw new Exception($"Component parent is not null: {GetType().Name}");
                }

                m_Parent = value;
                IsComponent = true;
                Domain = m_Parent.m_Domain;
            }
        }

        protected FEntity m_Parent;

        public T GetParent<T>() where T : FEntity
        {
            return Parent as T;
        }

        public long ID { get; protected set; }

        protected FEntity m_Domain;

        public FEntity Domain
        {
            get => m_Domain;
            set
            {
                if (value == null)
                {
                    throw new Exception($"domain can't set null: {GetType().Name}");
                }

                if (m_Domain == value)
                {
                    return;
                }

                FEntity preDomain = m_Domain;
                m_Domain = value;

                if (preDomain == null)
                {
                    InstanceID = IDGenerater.Instance.GenerateInstanceID();
                    IsRegister = true;

                    // 反序列化出来的需要设置父子关系
                    if (m_ComponentsDB != null)
                    {
                        foreach (FEntity component in m_ComponentsDB)
                        {
                            component.IsComponent = true;
                            Components.Add(component.GetType(), component);
                            component.m_Parent = this;
                        }
                    }

                    if (m_ChildrenDB != null)
                    {
                        foreach (FEntity child in m_ChildrenDB)
                        {
                            child.IsComponent = false;
                            Children.Add(child.ID, child);
                            child.m_Parent = this;
                        }
                    }
                }

                // 是否注册跟parent一致
                if (m_Parent != null)
                {
                    IsRegister = Parent.IsRegister;
                }

                // 递归设置孩子的Domain
                if (m_Children != null)
                {
                    foreach (FEntity entity in m_Children.Values)
                    {
                        entity.Domain = m_Domain;
                    }
                }

                if (m_Components != null)
                {
                    foreach (FEntity component in m_Components.Values)
                    {
                        component.Domain = m_Domain;
                    }
                }

                if (!IsCreate)
                {
                    IsCreate = true;
                    FEventSystem.Get().Deserialize(this);
                }
            }
        }

        private HashSet<FEntity> m_ChildrenDB;
        private Dictionary<long, FEntity> m_Children;

        public Dictionary<long, FEntity> Children => m_Children ?? (m_Children = DictionaryPool<long, FEntity>.Rent());

        private void AddChild(FEntity entity)
        {
            Children.Add(entity.ID, entity);
            AddChildDB(entity);
        }

        private void RemoveChild(FEntity entity)
        {
            if (m_Children == null)
            {
                return;
            }

            m_Children.Remove(entity.ID);

            if (m_Children.Count == 0)
            {
                DictionaryPool<long, FEntity>.Return(m_Children);
                m_Children = null;
            }

            RemoveChildDB(entity);
        }

        private void AddChildDB(FEntity entity)
        {
            if (!(entity is ISerializeToEntity))
            {
                return;
            }

            if (m_ChildrenDB == null)
            {
                m_ChildrenDB = HashSetPool<FEntity>.Rent();
            }

            m_ChildrenDB.Add(entity);
        }

        private void RemoveChildDB(FEntity entity)
        {
            if (!(entity is ISerializeToEntity))
            {
                return;
            }

            if (m_ChildrenDB == null)
            {
                return;
            }

            m_ChildrenDB.Remove(entity);

            if (m_ChildrenDB.Count == 0)
            {
                if (IsFromPool)
                {
                    HashSetPool<FEntity>.Return(m_ChildrenDB);
                    m_ChildrenDB = null;
                }
            }
        }

        private HashSet<FEntity> m_ComponentsDB;
        private Dictionary<Type, FEntity> m_Components;

        public Dictionary<Type, FEntity> Components => m_Components ?? (m_Components = DictionaryPool<Type, FEntity>.Rent());


        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            FEventSystem.Get().Remove(InstanceID);
            InstanceID = 0;

            // 清理Component
            if (m_Components != null)
            {
                foreach (KeyValuePair<Type, FEntity> kv in m_Components)
                {
                    kv.Value.Dispose();
                }

                DictionaryPool<Type, FEntity>.Return(Components);
                m_Components = null;

                // 从池中创建的才需要回到池中,从db中不需要回收
                if (m_ComponentsDB != null)
                {
                    m_ComponentsDB.Clear();

                    if (IsFromPool)
                    {
                        HashSetPool<FEntity>.Return(m_ComponentsDB);
                        m_ComponentsDB = null;
                    }
                }
            }

            // 清理Children
            if (m_Children != null)
            {
                foreach (FEntity child in m_Children.Values)
                {
                    child.Dispose();
                }

                DictionaryPool<long, FEntity>.Return(m_Children);
                m_Children = null;

                if (m_ChildrenDB != null)
                {
                    m_ChildrenDB.Clear();
                    // 从池中创建的才需要回到池中,从db中不需要回收
                    if (IsFromPool)
                    {
                        HashSetPool<FEntity>.Return(m_ChildrenDB);
                        m_ChildrenDB = null;
                    }
                }
            }

            // 触发Destroy事件
            FEventSystem.Get().Destroy(this);

            m_Domain = null;

            if (m_Parent != null && !m_Parent.IsDisposed)
            {
                if (IsComponent)
                {
                    m_Parent.RemoveComponent(this);
                }
                else
                {
                    m_Parent.RemoveChild(this);
                }
            }

            m_Parent = null;

            if (IsFromPool)
            {
                SharedPool.Return(this);
            }
            else
            {
                base.Dispose();
            }

            m_Status = EEntityStatus.None;
        }

        private void AddToComponentsDB(FEntity component)
        {
            if (m_ComponentsDB == null)
            {
                m_ComponentsDB = HashSetPool<FEntity>.Rent();
            }

            m_ComponentsDB.Add(component);
        }

        private void RemoveFromComponentsDB(FEntity component)
        {
            if (m_ComponentsDB == null)
            {
                return;
            }

            m_ComponentsDB.Remove(component);
            if (m_ComponentsDB.Count == 0 && IsFromPool)
            {
                HashSetPool<FEntity>.Return(m_ComponentsDB);
                m_ComponentsDB = null;
            }
        }

        private void AddToComponent(Type type, FEntity component)
        {
            if (m_Components == null)
            {
                m_Components = DictionaryPool<Type, FEntity>.Rent();
            }

            m_Components.Add(type, component);

            if (component is ISerializeToEntity)
            {
                AddToComponentsDB(component);
            }
        }

        private void RemoveFromComponent(Type type, FEntity component)
        {
            if (m_Components == null)
            {
                return;
            }

            m_Components.Remove(type);

            if (m_Components.Count == 0 && IsFromPool)
            {
                DictionaryPool<Type, FEntity>.Return(m_Components);
                m_Components = null;
            }

            RemoveFromComponentsDB(component);
        }

        public E GetChild<E>(long id) where E : FEntity
        {
            if (m_Children == null)
            {
                return null;
            }
            m_Children.TryGetValue(id, out FEntity child);
            return child as E;
        }

        public FEntity AddComponent(FEntity component)
        {
            Type type = component.GetType();
            if (m_Components != null && m_Components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            component.ComponentParent = this;

            AddToComponent(type, component);

            return component;
        }

        public FEntity AddComponent(Type type)
        {
            if (m_Components != null && m_Components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            FEntity component = CreateWithComponentParent(type);

            AddToComponent(type, component);

            return component;
        }

        public C AddComponent<C>(bool isFromPool = true) where C : FEntity, new()
        {
            Type type = typeof(C);
            if (m_Components != null && m_Components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            C component = CreateWithComponentParent<C>(isFromPool);

            AddToComponent(type, component);

            return component;
        }

        public C AddComponent<C, P1>(P1 p1, bool isFromPool = true) where C : FEntity, new()
        {
            Type type = typeof(C);
            if (m_Components != null && m_Components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            C component = CreateWithComponentParent<C, P1>(p1, isFromPool);

            AddToComponent(type, component);

            return component;
        }

        public C AddComponent<C, P1, P2>(P1 p1, P2 p2, bool isFromPool = true) where C : FEntity, new()
        {
            Type type = typeof(C);
            if (m_Components != null && m_Components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            C component = CreateWithComponentParent<C, P1, P2>(p1, p2, isFromPool);

            AddToComponent(type, component);

            return component;
        }

        public C AddComponent<C, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = true) where C : FEntity, new()
        {
            Type type = typeof(C);
            if (m_Components != null && m_Components.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            C component = CreateWithComponentParent<C, P1, P2, P3>(p1, p2, p3, isFromPool);

            AddToComponent(type, component);

            return component;
        }

        public void RemoveComponent<C>() where C : FEntity
        {
            if (IsDisposed)
            {
                return;
            }

            if (m_Components == null)
            {
                return;
            }

            Type type = typeof(C);
            FEntity c = GetComponent(type);
            if (c == null)
            {
                return;
            }

            RemoveFromComponent(type, c);
            c.Dispose();
        }

        public void RemoveComponent(FEntity component)
        {
            if (IsDisposed)
            {
                return;
            }

            if (m_Components == null)
            {
                return;
            }

            Type type = component.GetType();
            FEntity c = GetComponent(component.GetType());
            if (c == null)
            {
                return;
            }

            if (c.InstanceID != component.InstanceID)
            {
                return;
            }

            RemoveFromComponent(type, c);
            c.Dispose();
        }

        public void RemoveComponent(Type type)
        {
            if (IsDisposed)
            {
                return;
            }

            FEntity c = GetComponent(type);
            if (c == null)
            {
                return;
            }

            RemoveFromComponent(type, c);
            c.Dispose();
        }

        public virtual C GetComponent<C>() where C : FEntity
        {
            if (m_Components == null)
            {
                return null;
            }

            if (!m_Components.TryGetValue(typeof(C), out FEntity component))
            {
                return default;
            }

            return (C)component;
        }

        public virtual FEntity GetComponent(Type type)
        {
            if (m_Components == null)
            {
                return null;
            }

            if (!m_Components.TryGetValue(type, out FEntity component))
            {
                return null;
            }

            return component;
        }

        void IReference.IReferenceClear()
        {
        }

        private void ERROR(string msg)
        {
            Log.ERROR("[Entity]", msg);
        }
    }
}
