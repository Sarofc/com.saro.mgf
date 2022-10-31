#if UNITY_EDITOR

using UnityEngine;
using System;
using System.Reflection;

namespace Saro.SEditor
{
    ///Do not derive this. Derive from it's generic version only, where T is the type we care for.
    abstract public class ObjectDrawer
    {
        abstract public object DrawGUI(string label, object instance, FieldInfo fieldInfo, Attribute attribute, object context);
    }

    ///Derive this to create custom drawers for T assignable object types.
    abstract public class ObjectDrawer<T> : ObjectDrawer
    {
        ///The instance of the object being drawn/serialized and for which this drawer is for
        public T instance { get; set; }
        ///The reflected FieldInfo representation
        public FieldInfo fieldInfo { get; set; }
        ///The object the instance is drawn/serialized within
        public object context { get; set; }

        ///Begin GUI
        sealed public override object DrawGUI(string label, object instance, FieldInfo fieldInfo, Attribute attribute, object context)
        {
            this.fieldInfo = fieldInfo;
            this.context = context;
            var value = (T)instance;
            OnGUI(label, ref value, context);
            return value;
        }

        ///Override to implement GUI. Return the modified instance at the end.
        abstract public void OnGUI(string label, ref T instance, object context);
    }


    ///Derive this to create custom drawers for T ObjectDrawerAttributes.
    abstract public class AttributeDrawer<T> : ObjectDrawer where T : CustomDrawerAttribute
    {
        ///The instance of the object being drawn/serialized
        public object instance { get; set; }
        ///The reflection FieldInfo representation
        public FieldInfo fieldInfo { get; set; }
        ///The attribute against this drawer is for.
        public T attribute { get; set; }
        ///The object the instance is drawn/serialized within
        public object context { get; set; }

        ///Begin GUI
        sealed public override object DrawGUI(string label, object instance, FieldInfo fieldInfo, Attribute attribute, object context)
        {
            this.fieldInfo = fieldInfo;
            this.context = context;
            this.attribute = (T)attribute;
            OnGUI(label, ref instance, context);
            return instance;
        }

        ///Override to implement GUI. Return the modified instance at the end.
        abstract public void OnGUI(string label, ref object instance, object context);
    }

    ///A stub
    sealed public class NoDrawer : ObjectDrawer
    {
        public override object DrawGUI(string label, object instance, FieldInfo fieldInfo, Attribute attribute, object context) { return instance; }
    }
}

#endif