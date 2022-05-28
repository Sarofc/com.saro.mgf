using UnityEngine;

namespace Saro.Utility
{
    public static class MatrixUtility
    {
        public static Quaternion ExtractRotation(this Matrix4x4 matrix)
        {
            Vector3 forward = new Vector3(
                matrix.m02,
                matrix.m12,
                matrix.m22
            );

            Vector3 upwards = new Vector3(
                matrix.m01,
                matrix.m11,
                matrix.m21
            );

            if (forward == Vector3.zero && upwards == Vector3.zero) return Quaternion.identity;

            return Quaternion.LookRotation(forward, upwards);
        }

        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            Vector3 position = new Vector3(
                matrix.m03,
                matrix.m13,
                matrix.m23
            );
            return position;
        }

        public static Vector3 ExtractScale(this Matrix4x4 matrix)
        {
            Vector3 scale = new Vector3(
                matrix.GetColumn(0).magnitude,
                matrix.GetColumn(1).magnitude,
                matrix.GetColumn(2).magnitude
            );
            return scale;
        }
    }
}
