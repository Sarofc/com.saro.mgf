using Cysharp.Threading.Tasks;
using Saro.Collections;
using Saro.Pool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Saro
{
    using OneTypeSystems = TMultiMap<Type, object>;

    public sealed class FEventSystem : IDisposable
    {
        private class TypeSystems
        {
            private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new Dictionary<Type, OneTypeSystems>();

            public OneTypeSystems GetOrCreateOneTypeSystems(Type type)
            {
                typeSystemsMap.TryGetValue(type, out OneTypeSystems systems);
                if (systems != null)
                {
                    return systems;
                }

                systems = new OneTypeSystems();
                typeSystemsMap.Add(type, systems);
                return systems;
            }

            public OneTypeSystems GetOneTypeSystems(Type type)
            {
                typeSystemsMap.TryGetValue(type, out OneTypeSystems systems);
                return systems;
            }

            public TLinkedListRange<object> GetSystems(Type type, Type systemType)
            {
                if (!typeSystemsMap.TryGetValue(type, out OneTypeSystems oneTypeSystems))
                {
                    return TLinkedListRange<object>.s_Empty;
                }

                if (!oneTypeSystems.TryGetValue(systemType, out var systems))
                {
                    return TLinkedListRange<object>.s_Empty;
                }

                return systems;
            }
        }


        private static FEventSystem instance;

        public static FEventSystem Get()
        {
            if (instance == null)
            {
                instance = new FEventSystem();
            }
            return instance;
        }

        private readonly Dictionary<long, FEntity> allEntities = new Dictionary<long, FEntity>();

        private readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        private readonly Dictionary<Type, HashSet<Type>> types = new Dictionary<Type, HashSet<Type>>();

        private readonly Dictionary<Type, List<object>> allEvents = new Dictionary<Type, List<object>>();

        private TypeSystems typeSystems = new TypeSystems();

        private Queue<long> updates = new Queue<long>();
        private Queue<long> updates2 = new Queue<long>();

        private Queue<long> loaders = new Queue<long>();
        private Queue<long> loaders2 = new Queue<long>();

        private Queue<long> lateUpdates = new Queue<long>();
        private Queue<long> lateUpdates2 = new Queue<long>();

        private FEventSystem()
        {
            Add(typeof(FEventSystem).Assembly);
        }

        public void Add(Assembly assembly)
        {
            assemblies[$"{assembly.GetName().Name}.dll"] = assembly;
            types.Clear();
            foreach (Assembly value in assemblies.Values)
            {
                foreach (Type type in value.GetTypes())
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    object[] objects = type.GetCustomAttributes(typeof(BaseAttribute), true);
                    if (objects.Length == 0)
                    {
                        continue;
                    }

                    foreach (BaseAttribute baseAttribute in objects)
                    {
                        if (!types.TryGetValue(baseAttribute.AttributeType, out var typeSet))
                        {
                            typeSet = new HashSet<Type>();
                            types.Add(baseAttribute.AttributeType, typeSet);
                        }
                        typeSet.Add(type);
                    }
                }
            }

            typeSystems = new TypeSystems();

            foreach (Type type in GetTypes(typeof(FObjectSystemAttribute)))
            {
                object obj = Activator.CreateInstance(type);

                if (obj is ISystemType iSystemType)
                {
                    OneTypeSystems oneTypeSystems = typeSystems.GetOrCreateOneTypeSystems(iSystemType.Type());
                    oneTypeSystems.Add(iSystemType.SystemType(), obj);
                }
            }

            allEvents.Clear();

            if (types.TryGetValue(typeof(FEventAttribute), out var eventTypeSet))
            {
                foreach (var evtType in eventTypeSet)
                {
                    IEvent obj = Activator.CreateInstance(evtType) as IEvent;
                    if (obj == null)
                    {
                        throw new Exception($"type not is AEvent: {obj.GetType().Name}");
                    }

                    Type eventType = obj.GetEventType();
                    if (!allEvents.ContainsKey(eventType))
                    {
                        allEvents.Add(eventType, new List<object>());
                    }
                    allEvents[eventType].Add(obj);
                }
            }

            Load();
        }



        public Assembly GetAssembly(string name)
        {
            return assemblies[name];
        }

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }
            return types[systemAttributeType];
        }

        public List<Type> GetTypes()
        {
            List<Type> allTypes = new List<Type>();
            foreach (Assembly assembly in assemblies.Values)
            {
                allTypes.AddRange(assembly.GetTypes());
            }
            return allTypes;
        }

        public Type GetType(string typeName)
        {
            return typeof(FGame).Assembly.GetType(typeName);
        }

        public void RegisterSystem(FEntity component, bool isRegister = true)
        {
            if (!isRegister)
            {
                Remove(component.InstanceID);
                return;
            }
            allEntities.Add(component.InstanceID, component);

            Type type = component.GetType();

            OneTypeSystems oneTypeSystems = typeSystems.GetOneTypeSystems(type);
            if (oneTypeSystems == null)
            {
                return;
            }

            if (oneTypeSystems.ContainsKey(typeof(ILoadSystem)))
            {
                loaders.Enqueue(component.InstanceID);
            }

            if (oneTypeSystems.ContainsKey(typeof(IUpdateSystem)))
            {
                updates.Enqueue(component.InstanceID);
            }

            if (oneTypeSystems.ContainsKey(typeof(ILateUpdateSystem)))
            {
                lateUpdates.Enqueue(component.InstanceID);
            }
        }

        public void Remove(long InstanceID)
        {
            allEntities.Remove(InstanceID);
        }

        public FEntity Get(long InstanceID)
        {
            allEntities.TryGetValue(InstanceID, out FEntity component);
            return component;
        }

        public bool IsRegister(long InstanceID)
        {
            return allEntities.ContainsKey(InstanceID);
        }

        public void Deserialize(FEntity component)
        {
            var iDeserializeSystems = typeSystems.GetSystems(component.GetType(), typeof(IDeserializeSystem));
            if (!iDeserializeSystems.IsValid)
            {
                return;
            }

            foreach (IDeserializeSystem deserializeSystem in iDeserializeSystems)
            {
                if (deserializeSystem == null)
                {
                    continue;
                }

                try
                {
                    deserializeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.ERROR(e);
                }
            }
        }

        public void Awake(FEntity component)
        {
            var iAwakeSystems = typeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem));
            if (!iAwakeSystems.IsValid)
            {
                return;
            }

            foreach (IAwakeSystem aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.ERROR(e);
                }
            }
        }

        public void Awake<P1>(FEntity component, P1 p1)
        {
            var iAwakeSystems = typeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem<P1>));
            if (!iAwakeSystems.IsValid)
            {
                return;
            }

            foreach (IAwakeSystem<P1> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1);
                }
                catch (Exception e)
                {
                    Log.ERROR(e);
                }
            }
        }

        public void Awake<P1, P2>(FEntity component, P1 p1, P2 p2)
        {
            var iAwakeSystems = typeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem<P1, P2>));
            if (!iAwakeSystems.IsValid)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2);
                }
                catch (Exception e)
                {
                    Log.ERROR(e);
                }
            }
        }

        public void Awake<P1, P2, P3>(FEntity component, P1 p1, P2 p2, P3 p3)
        {
            var iAwakeSystems = typeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem<P1, P2, P3>));
            if (!iAwakeSystems.IsValid)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3);
                }
                catch (Exception e)
                {
                    Log.ERROR(e);
                }
            }
        }

        public void Awake<P1, P2, P3, P4>(FEntity component, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            var iAwakeSystems = typeSystems.GetSystems(component.GetType(), typeof(IAwakeSystem<P1, P2, P3, P4>));
            if (!iAwakeSystems.IsValid)
            {
                return;
            }

            foreach (IAwakeSystem<P1, P2, P3, P4> aAwakeSystem in iAwakeSystems)
            {
                if (aAwakeSystem == null)
                {
                    continue;
                }

                try
                {
                    aAwakeSystem.Run(component, p1, p2, p3, p4);
                }
                catch (Exception e)
                {
                    Log.ERROR(e);
                }
            }
        }

        public void Load()
        {
            while (loaders.Count > 0)
            {
                long InstanceID = loaders.Dequeue();
                if (!allEntities.TryGetValue(InstanceID, out FEntity component))
                {
                    continue;
                }
                if (component.IsDisposed)
                {
                    continue;
                }

                var iLoadSystems = typeSystems.GetSystems(component.GetType(), typeof(ILoadSystem));
                if (!iLoadSystems.IsValid)
                {
                    continue;
                }

                loaders2.Enqueue(InstanceID);

                foreach (ILoadSystem iLoadSystem in iLoadSystems)
                {
                    try
                    {
                        iLoadSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.ERROR(e);
                    }
                }
            }

            Swap(ref loaders, ref loaders2);
        }

        public void Destroy(FEntity component)
        {
            var iDestroySystems = typeSystems.GetSystems(component.GetType(), typeof(IDestroySystem));
            if (!iDestroySystems.IsValid)
            {
                return;
            }

            foreach (IDestroySystem iDestroySystem in iDestroySystems)
            {
                if (iDestroySystem == null)
                {
                    continue;
                }

                try
                {
                    iDestroySystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.ERROR(e);
                }
            }
        }

        public void Update()
        {
            while (updates.Count > 0)
            {
                long InstanceID = updates.Dequeue();
                if (!allEntities.TryGetValue(InstanceID, out FEntity component))
                {
                    continue;
                }
                if (component.IsDisposed)
                {
                    continue;
                }

                var iUpdateSystems = typeSystems.GetSystems(component.GetType(), typeof(IUpdateSystem));
                if (!iUpdateSystems.IsValid)
                {
                    continue;
                }

                updates2.Enqueue(InstanceID);

                foreach (IUpdateSystem iUpdateSystem in iUpdateSystems)
                {
                    try
                    {
                        iUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.ERROR(e);
                    }
                }
            }

            Swap(ref updates, ref updates2);
        }

        public void LateUpdate()
        {
            while (lateUpdates.Count > 0)
            {
                long InstanceID = lateUpdates.Dequeue();
                if (!allEntities.TryGetValue(InstanceID, out FEntity component))
                {
                    continue;
                }
                if (component.IsDisposed)
                {
                    continue;
                }

                var iLateUpdateSystems = typeSystems.GetSystems(component.GetType(), typeof(ILateUpdateSystem));
                if (!iLateUpdateSystems.IsValid)
                {
                    continue;
                }

                lateUpdates2.Enqueue(InstanceID);

                foreach (ILateUpdateSystem iLateUpdateSystem in iLateUpdateSystems)
                {
                    try
                    {
                        iLateUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.ERROR(e);
                    }
                }
            }

            Swap(ref lateUpdates, ref lateUpdates2);
        }

        public async UniTask Publish<T>(T a) where T : struct
        {
            if (!allEvents.TryGetValue(typeof(T), out List<object> iEvents))
            {
                return;
            }

            using (var pooledObj = ListPool<UniTask>.Get(out var list))
            {
                foreach (object obj in iEvents)
                {
                    if (!(obj is FEvent<T> aEvent))
                    {
                        Log.ERROR($"event error: {obj.GetType().Name}");
                        continue;
                    }

                    list.Add(aEvent.Handle(a));
                }
                try
                {
                    await UniTask.WhenAll(list);
                }
                catch (Exception e)
                {
                    Log.ERROR(e);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            HashSet<Type> noParent = new HashSet<Type>();
            Dictionary<Type, int> typeCount = new Dictionary<Type, int>();

            HashSet<Type> noDomain = new HashSet<Type>();

            foreach (var kv in allEntities)
            {
                Type type = kv.Value.GetType();
                if (kv.Value.Parent == null)
                {
                    noParent.Add(type);
                }

                if (kv.Value.Domain == null)
                {
                    noDomain.Add(type);
                }

                if (typeCount.ContainsKey(type))
                {
                    typeCount[type]++;
                }
                else
                {
                    typeCount[type] = 1;
                }
            }

            sb.AppendLine("not set parent type: ");
            foreach (Type type in noParent)
            {
                sb.AppendLine($"\t{type.Name}");
            }

            sb.AppendLine("not set domain type: ");
            foreach (Type type in noDomain)
            {
                sb.AppendLine($"\t{type.Name}");
            }

            IOrderedEnumerable<KeyValuePair<Type, int>> orderByDescending = typeCount.OrderByDescending(s => s.Value);

            sb.AppendLine("Entity Count: ");
            foreach (var kv in orderByDescending)
            {
                if (kv.Value == 1)
                {
                    continue;
                }
                sb.AppendLine($"\t{kv.Key.Name}: {kv.Value}");
            }

            return sb.ToString();
        }

        public void Dispose()
        {
            instance = null;
        }

        private void Swap<T>(ref T r1, ref T r2)
        {
            var tmp = r1;
            r1 = r2;
            r2 = tmp;
        }
    }
}