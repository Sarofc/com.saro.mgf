using UnityEngine;

namespace Saro.Gameplay.Effect
{
    public struct ControlPoint
    {
        public float pointA;
        public float pointB;
        public float pointC;
    }

    public enum EAttachType
    {
        AbsOrigin,
        AbsOrigin_Follow,
        Point,
        Point_Follow
    }

    public struct ControlEntity
    {
        public int entityID;
        public EAttachType attachType;
        public string attachName;
        public Vector3 offset;
        public bool lockOrientation;
    }
}
