using UnityEngine;

namespace Saro.Diagnostics
{
    public enum EGLDebug
    {
        Editor,
        Game,
        Both
    }

    public static class GLDebugHelper
    {
        public static void DebugSquare(Vector3 origin, Vector3 halfExtents, Color color, Quaternion orientation,
                                       float drawDuration = 0, EGLDebug preview = EGLDebug.Editor)
        {
            Vector3 forward = orientation * Vector3.forward;
            Vector3 up = orientation * Vector3.up;
            Vector3 right = orientation * Vector3.right;

            Vector3 topMinY1 = origin + (right * halfExtents.x) + (up * halfExtents.y) + (forward * halfExtents.z);
            Vector3 topMaxY1 = origin - (right * halfExtents.x) + (up * halfExtents.y) + (forward * halfExtents.z);
            Vector3 botMinY1 = origin + (right * halfExtents.x) - (up * halfExtents.y) + (forward * halfExtents.z);
            Vector3 botMaxY1 = origin - (right * halfExtents.x) - (up * halfExtents.y) + (forward * halfExtents.z);

            bool drawEditor = false;
            bool drawGame = false;

            switch (preview)
            {
                case EGLDebug.Editor:
                    drawEditor = true;
                    break;

                case EGLDebug.Game:
                    drawGame = true;
                    break;

                case EGLDebug.Both:
                    drawEditor = true;
                    drawGame = true;
                    break;
            }

            if (drawEditor)
            {
                Debug.DrawLine(topMinY1, botMinY1, color, drawDuration);
                Debug.DrawLine(topMaxY1, botMaxY1, color, drawDuration);
                Debug.DrawLine(topMinY1, topMaxY1, color, drawDuration);
                Debug.DrawLine(botMinY1, botMaxY1, color, drawDuration);
            }

            if (drawGame)
            {
                GLDebug.DrawLine(topMinY1, botMinY1, color, drawDuration);
                GLDebug.DrawLine(topMaxY1, botMaxY1, color, drawDuration);
                GLDebug.DrawLine(topMinY1, topMaxY1, color, drawDuration);
                GLDebug.DrawLine(botMinY1, botMaxY1, color, drawDuration);
            }
        }

        public static void DebugBox(Vector3 origin, Vector3 halfExtents, Vector3 direction, float maxDistance, Color color,
                                    Quaternion orientation, Color endColor, bool drawBase = true, float drawDuration = 0,
                                    EGLDebug preview = EGLDebug.Editor)
        {
            Vector3 end = origin + direction * (float.IsPositiveInfinity(maxDistance) ? 1000 * 1000 : maxDistance);

            Vector3 forward = orientation * Vector3.forward;
            Vector3 up = orientation * Vector3.up;
            Vector3 right = orientation * Vector3.right;

            #region Coords
            #region End coords
            //trans.position = end;
            Vector3 topMinX0 = end + (right * halfExtents.x) + (up * halfExtents.y) - (forward * halfExtents.z);
            Vector3 topMaxX0 = end - (right * halfExtents.x) + (up * halfExtents.y) - (forward * halfExtents.z);
            Vector3 topMinY0 = end + (right * halfExtents.x) + (up * halfExtents.y) + (forward * halfExtents.z);
            Vector3 topMaxY0 = end - (right * halfExtents.x) + (up * halfExtents.y) + (forward * halfExtents.z);

            Vector3 botMinX0 = end + (right * halfExtents.x) - (up * halfExtents.y) - (forward * halfExtents.z);
            Vector3 botMaxX0 = end - (right * halfExtents.x) - (up * halfExtents.y) - (forward * halfExtents.z);
            Vector3 botMinY0 = end + (right * halfExtents.x) - (up * halfExtents.y) + (forward * halfExtents.z);
            Vector3 botMaxY0 = end - (right * halfExtents.x) - (up * halfExtents.y) + (forward * halfExtents.z);
            #endregion

            #region Origin coords
            //trans.position = origin;
            Vector3 topMinX1 = origin + (right * halfExtents.x) + (up * halfExtents.y) - (forward * halfExtents.z);
            Vector3 topMaxX1 = origin - (right * halfExtents.x) + (up * halfExtents.y) - (forward * halfExtents.z);
            Vector3 topMinY1 = origin + (right * halfExtents.x) + (up * halfExtents.y) + (forward * halfExtents.z);
            Vector3 topMaxY1 = origin - (right * halfExtents.x) + (up * halfExtents.y) + (forward * halfExtents.z);

            Vector3 botMinX1 = origin + (right * halfExtents.x) - (up * halfExtents.y) - (forward * halfExtents.z);
            Vector3 botMaxX1 = origin - (right * halfExtents.x) - (up * halfExtents.y) - (forward * halfExtents.z);
            Vector3 botMinY1 = origin + (right * halfExtents.x) - (up * halfExtents.y) + (forward * halfExtents.z);
            Vector3 botMaxY1 = origin - (right * halfExtents.x) - (up * halfExtents.y) + (forward * halfExtents.z);
            #endregion
            #endregion

            #region Draw lines
            bool drawEditor = false;
            bool drawGame = false;

            switch (preview)
            {
                case EGLDebug.Editor:
                    drawEditor = true;
                    break;

                case EGLDebug.Game:
                    drawGame = true;
                    break;

                case EGLDebug.Both:
                    drawEditor = true;
                    drawGame = true;
                    break;
            }

            if (drawEditor)
            {
                #region Origin box
                if (drawBase)
                {
                    Debug.DrawLine(topMinX1, botMinX1, color, drawDuration);
                    Debug.DrawLine(topMaxX1, botMaxX1, color, drawDuration);
                    Debug.DrawLine(topMinY1, botMinY1, color, drawDuration);
                    Debug.DrawLine(topMaxY1, botMaxY1, color, drawDuration);

                    Debug.DrawLine(topMinX1, topMaxX1, color, drawDuration);
                    Debug.DrawLine(topMinX1, topMinY1, color, drawDuration);
                    Debug.DrawLine(topMinY1, topMaxY1, color, drawDuration);
                    Debug.DrawLine(topMaxY1, topMaxX1, color, drawDuration);

                    Debug.DrawLine(botMinX1, botMaxX1, color, drawDuration);
                    Debug.DrawLine(botMinX1, botMinY1, color, drawDuration);
                    Debug.DrawLine(botMinY1, botMaxY1, color, drawDuration);
                    Debug.DrawLine(botMaxY1, botMaxX1, color, drawDuration);
                }
                #endregion

                #region Connection between boxes
                Debug.DrawLine(topMinX0, topMinX1, color, drawDuration);
                Debug.DrawLine(topMaxX0, topMaxX1, color, drawDuration);
                Debug.DrawLine(topMinY0, topMinY1, color, drawDuration);
                Debug.DrawLine(topMaxY0, topMaxY1, color, drawDuration);

                Debug.DrawLine(botMinX0, botMinX1, color, drawDuration);
                Debug.DrawLine(botMinX0, botMinX1, color, drawDuration);
                Debug.DrawLine(botMinY0, botMinY1, color, drawDuration);
                Debug.DrawLine(botMaxY0, botMaxY1, color, drawDuration);
                #endregion

                #region End box
                color = endColor;

                Debug.DrawLine(topMinX0, botMinX0, color, drawDuration);
                Debug.DrawLine(topMaxX0, botMaxX0, color, drawDuration);
                Debug.DrawLine(topMinY0, botMinY0, color, drawDuration);
                Debug.DrawLine(topMaxY0, botMaxY0, color, drawDuration);

                Debug.DrawLine(topMinX0, topMaxX0, color, drawDuration);
                Debug.DrawLine(topMinX0, topMinY0, color, drawDuration);
                Debug.DrawLine(topMinY0, topMaxY0, color, drawDuration);
                Debug.DrawLine(topMaxY0, topMaxX0, color, drawDuration);

                Debug.DrawLine(botMinX0, botMaxX0, color, drawDuration);
                Debug.DrawLine(botMinX0, botMinY0, color, drawDuration);
                Debug.DrawLine(botMinY0, botMaxY0, color, drawDuration);
                Debug.DrawLine(botMaxY0, botMaxX0, color, drawDuration);
                #endregion
            }

            if (drawGame)
            {
                #region Origin box
                if (drawBase)
                {
                    GLDebug.DrawLine(topMinX1, botMinX1, color, drawDuration);
                    GLDebug.DrawLine(topMaxX1, botMaxX1, color, drawDuration);
                    GLDebug.DrawLine(topMinY1, botMinY1, color, drawDuration);
                    GLDebug.DrawLine(topMaxY1, botMaxY1, color, drawDuration);

                    GLDebug.DrawLine(topMinX1, topMaxX1, color, drawDuration);
                    GLDebug.DrawLine(topMinX1, topMinY1, color, drawDuration);
                    GLDebug.DrawLine(topMinY1, topMaxY1, color, drawDuration);
                    GLDebug.DrawLine(topMaxY1, topMaxX1, color, drawDuration);

                    GLDebug.DrawLine(botMinX1, botMaxX1, color, drawDuration);
                    GLDebug.DrawLine(botMinX1, botMinY1, color, drawDuration);
                    GLDebug.DrawLine(botMinY1, botMaxY1, color, drawDuration);
                    GLDebug.DrawLine(botMaxY1, botMaxX1, color, drawDuration);
                }
                #endregion

                #region Connection between boxes
                GLDebug.DrawLine(topMinX0, topMinX1, color, drawDuration);
                GLDebug.DrawLine(topMaxX0, topMaxX1, color, drawDuration);
                GLDebug.DrawLine(topMinY0, topMinY1, color, drawDuration);
                GLDebug.DrawLine(topMaxY0, topMaxY1, color, drawDuration);

                GLDebug.DrawLine(botMinX0, botMinX1, color, drawDuration);
                GLDebug.DrawLine(botMinX0, botMinX1, color, drawDuration);
                GLDebug.DrawLine(botMinY0, botMinY1, color, drawDuration);
                GLDebug.DrawLine(botMaxY0, botMaxY1, color, drawDuration);
                #endregion

                #region End box
                color = endColor;

                GLDebug.DrawLine(topMinX0, botMinX0, color, drawDuration);
                GLDebug.DrawLine(topMaxX0, botMaxX0, color, drawDuration);
                GLDebug.DrawLine(topMinY0, botMinY0, color, drawDuration);
                GLDebug.DrawLine(topMaxY0, botMaxY0, color, drawDuration);

                GLDebug.DrawLine(topMinX0, topMaxX0, color, drawDuration);
                GLDebug.DrawLine(topMinX0, topMinY0, color, drawDuration);
                GLDebug.DrawLine(topMinY0, topMaxY0, color, drawDuration);
                GLDebug.DrawLine(topMaxY0, topMaxX0, color, drawDuration);

                GLDebug.DrawLine(botMinX0, botMaxX0, color, drawDuration);
                GLDebug.DrawLine(botMinX0, botMinY0, color, drawDuration);
                GLDebug.DrawLine(botMinY0, botMaxY0, color, drawDuration);
                GLDebug.DrawLine(botMaxY0, botMaxX0, color, drawDuration);
                #endregion
            }
            #endregion
        }

        public static void DebugBox(Vector3 origin, Vector3 halfExtents, Color color, Quaternion orientation,
                                    float drawDuration = 0, EGLDebug preview = EGLDebug.Editor)
        {
            Vector3 forward = orientation * Vector3.forward;
            Vector3 up = orientation * Vector3.up;
            Vector3 right = orientation * Vector3.right;

            Vector3 topMinX1 = origin + (right * halfExtents.x) + (up * halfExtents.y) - (forward * halfExtents.z);
            Vector3 topMaxX1 = origin - (right * halfExtents.x) + (up * halfExtents.y) - (forward * halfExtents.z);
            Vector3 topMinY1 = origin + (right * halfExtents.x) + (up * halfExtents.y) + (forward * halfExtents.z);
            Vector3 topMaxY1 = origin - (right * halfExtents.x) + (up * halfExtents.y) + (forward * halfExtents.z);

            Vector3 botMinX1 = origin + (right * halfExtents.x) - (up * halfExtents.y) - (forward * halfExtents.z);
            Vector3 botMaxX1 = origin - (right * halfExtents.x) - (up * halfExtents.y) - (forward * halfExtents.z);
            Vector3 botMinY1 = origin + (right * halfExtents.x) - (up * halfExtents.y) + (forward * halfExtents.z);
            Vector3 botMaxY1 = origin - (right * halfExtents.x) - (up * halfExtents.y) + (forward * halfExtents.z);

            bool drawEditor = false;
            bool drawGame = false;

            switch (preview)
            {
                case EGLDebug.Editor:
                    drawEditor = true;
                    break;

                case EGLDebug.Game:
                    drawGame = true;
                    break;

                case EGLDebug.Both:
                    drawEditor = true;
                    drawGame = true;
                    break;
            }

            if (drawEditor)
            {
                Debug.DrawLine(topMinX1, botMinX1, color, drawDuration);
                Debug.DrawLine(topMaxX1, botMaxX1, color, drawDuration);
                Debug.DrawLine(topMinY1, botMinY1, color, drawDuration);
                Debug.DrawLine(topMaxY1, botMaxY1, color, drawDuration);

                Debug.DrawLine(topMinX1, topMaxX1, color, drawDuration);
                Debug.DrawLine(topMinX1, topMinY1, color, drawDuration);
                Debug.DrawLine(topMinY1, topMaxY1, color, drawDuration);
                Debug.DrawLine(topMaxY1, topMaxX1, color, drawDuration);

                Debug.DrawLine(botMinX1, botMaxX1, color, drawDuration);
                Debug.DrawLine(botMinX1, botMinY1, color, drawDuration);
                Debug.DrawLine(botMinY1, botMaxY1, color, drawDuration);
                Debug.DrawLine(botMaxY1, botMaxX1, color, drawDuration);
            }

            if (drawGame)
            {
                GLDebug.DrawLine(topMinX1, botMinX1, color, drawDuration);
                GLDebug.DrawLine(topMaxX1, botMaxX1, color, drawDuration);
                GLDebug.DrawLine(topMinY1, botMinY1, color, drawDuration);
                GLDebug.DrawLine(topMaxY1, botMaxY1, color, drawDuration);

                GLDebug.DrawLine(topMinX1, topMaxX1, color, drawDuration);
                GLDebug.DrawLine(topMinX1, topMinY1, color, drawDuration);
                GLDebug.DrawLine(topMinY1, topMaxY1, color, drawDuration);
                GLDebug.DrawLine(topMaxY1, topMaxX1, color, drawDuration);

                GLDebug.DrawLine(botMinX1, botMaxX1, color, drawDuration);
                GLDebug.DrawLine(botMinX1, botMinY1, color, drawDuration);
                GLDebug.DrawLine(botMinY1, botMaxY1, color, drawDuration);
                GLDebug.DrawLine(botMaxY1, botMaxX1, color, drawDuration);
            }
        }

        public static void DebugOneSidedCapsule(Vector3 baseSphere, Vector3 endSphere, Color color, float radius = 1,
                                                bool colorizeBase = false, float drawDuration = 0,
                                                EGLDebug preview = EGLDebug.Editor)
        {
            Vector3 up = (endSphere - baseSphere).normalized * radius;
            Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * radius;

            bool drawEditor = false;
            bool drawGame = false;

            switch (preview)
            {
                case EGLDebug.Editor:
                    drawEditor = true;
                    break;

                case EGLDebug.Game:
                    drawGame = true;
                    break;

                case EGLDebug.Both:
                    drawEditor = true;
                    drawGame = true;
                    break;
            }

            if (drawEditor)
            {
                //Side lines
                Debug.DrawLine(baseSphere + right, endSphere + right, color, drawDuration);
                Debug.DrawLine(baseSphere - right, endSphere - right, color, drawDuration);

                //Draw end caps
                for (int i = 1; i < 26; i++)
                {
                    //Start endcap
                    Debug.DrawLine(Vector3.Slerp(right, -up, i / 25.0f) + baseSphere, Vector3.Slerp(right, -up, (i - 1) / 25.0f) + baseSphere, colorizeBase ? color : Color.red, drawDuration);
                    Debug.DrawLine(Vector3.Slerp(-right, -up, i / 25.0f) + baseSphere, Vector3.Slerp(-right, -up, (i - 1) / 25.0f) + baseSphere, colorizeBase ? color : Color.red, drawDuration);

                    //End endcap
                    Debug.DrawLine(Vector3.Slerp(right, up, i / 25.0f) + endSphere, Vector3.Slerp(right, up, (i - 1) / 25.0f) + endSphere, color, drawDuration);
                    Debug.DrawLine(Vector3.Slerp(-right, up, i / 25.0f) + endSphere, Vector3.Slerp(-right, up, (i - 1) / 25.0f) + endSphere, color, drawDuration);
                }
            }

            if (drawGame)
            {
                //Side lines
                GLDebug.DrawLine(baseSphere + right, endSphere + right, color, drawDuration);
                GLDebug.DrawLine(baseSphere - right, endSphere - right, color, drawDuration);

                //Draw end caps
                for (int i = 1; i < 26; i++)
                {
                    //Start endcap
                    GLDebug.DrawLine(Vector3.Slerp(right, -up, i / 25.0f) + baseSphere, Vector3.Slerp(right, -up, (i - 1) / 25.0f) + baseSphere, colorizeBase ? color : Color.red, drawDuration);
                    GLDebug.DrawLine(Vector3.Slerp(-right, -up, i / 25.0f) + baseSphere, Vector3.Slerp(-right, -up, (i - 1) / 25.0f) + baseSphere, colorizeBase ? color : Color.red, drawDuration);

                    //End endcap
                    GLDebug.DrawLine(Vector3.Slerp(right, up, i / 25.0f) + endSphere, Vector3.Slerp(right, up, (i - 1) / 25.0f) + endSphere, color, drawDuration);
                    GLDebug.DrawLine(Vector3.Slerp(-right, up, i / 25.0f) + endSphere, Vector3.Slerp(-right, up, (i - 1) / 25.0f) + endSphere, color, drawDuration);
                }
            }
        }

        public static void DebugCapsule(Vector3 baseSphere, Vector3 endSphere, Color color, float radius = 1,
                                        bool colorizeBase = true, float drawDuration = 0,
                                        EGLDebug preview = EGLDebug.Editor)
        {
            Vector3 up = (endSphere - baseSphere).normalized * radius;
            Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * radius;

            //Radial circles
            DebugCircle(baseSphere, up, colorizeBase ? color : Color.red, radius, drawDuration, preview);
            DebugCircle(endSphere, -up, color, radius, drawDuration, preview);

            bool drawEditor = false;
            bool drawGame = false;

            switch (preview)
            {
                case EGLDebug.Editor:
                    drawEditor = true;
                    break;

                case EGLDebug.Game:
                    drawGame = true;
                    break;

                case EGLDebug.Both:
                    drawEditor = true;
                    drawGame = true;
                    break;
            }

            if (drawEditor)
            {
                //Side lines
                Debug.DrawLine(baseSphere + right, endSphere + right, color, drawDuration);
                Debug.DrawLine(baseSphere - right, endSphere - right, color, drawDuration);

                Debug.DrawLine(baseSphere + forward, endSphere + forward, color, drawDuration);
                Debug.DrawLine(baseSphere - forward, endSphere - forward, color, drawDuration);

                //Draw end caps
                for (int i = 1; i < 26; i++)
                {
                    //End endcap
                    Debug.DrawLine(Vector3.Slerp(right, up, i / 25.0f) + endSphere, Vector3.Slerp(right, up, (i - 1) / 25.0f) + endSphere, color, drawDuration);
                    Debug.DrawLine(Vector3.Slerp(-right, up, i / 25.0f) + endSphere, Vector3.Slerp(-right, up, (i - 1) / 25.0f) + endSphere, color, drawDuration);
                    Debug.DrawLine(Vector3.Slerp(forward, up, i / 25.0f) + endSphere, Vector3.Slerp(forward, up, (i - 1) / 25.0f) + endSphere, color, drawDuration);
                    Debug.DrawLine(Vector3.Slerp(-forward, up, i / 25.0f) + endSphere, Vector3.Slerp(-forward, up, (i - 1) / 25.0f) + endSphere, color, drawDuration);

                    //Start endcap
                    Debug.DrawLine(Vector3.Slerp(right, -up, i / 25.0f) + baseSphere, Vector3.Slerp(right, -up, (i - 1) / 25.0f) + baseSphere, colorizeBase ? color : Color.red, drawDuration);
                    Debug.DrawLine(Vector3.Slerp(-right, -up, i / 25.0f) + baseSphere, Vector3.Slerp(-right, -up, (i - 1) / 25.0f) + baseSphere, colorizeBase ? color : Color.red, drawDuration);
                    Debug.DrawLine(Vector3.Slerp(forward, -up, i / 25.0f) + baseSphere, Vector3.Slerp(forward, -up, (i - 1) / 25.0f) + baseSphere, colorizeBase ? color : Color.red, drawDuration);
                    Debug.DrawLine(Vector3.Slerp(-forward, -up, i / 25.0f) + baseSphere, Vector3.Slerp(-forward, -up, (i - 1) / 25.0f) + baseSphere, colorizeBase ? color : Color.red, drawDuration);
                }
            }

            if (drawGame)
            {
                //Side lines
                GLDebug.DrawLine(baseSphere + right, endSphere + right, color, drawDuration);
                GLDebug.DrawLine(baseSphere - right, endSphere - right, color, drawDuration);

                GLDebug.DrawLine(baseSphere + forward, endSphere + forward, color, drawDuration);
                GLDebug.DrawLine(baseSphere - forward, endSphere - forward, color, drawDuration);

                //Draw end caps
                for (int i = 1; i < 26; i++)
                {
                    //End endcap
                    GLDebug.DrawLine(Vector3.Slerp(right, up, i / 25.0f) + endSphere, Vector3.Slerp(right, up, (i - 1) / 25.0f) + endSphere, color, drawDuration);
                    GLDebug.DrawLine(Vector3.Slerp(-right, up, i / 25.0f) + endSphere, Vector3.Slerp(-right, up, (i - 1) / 25.0f) + endSphere, color, drawDuration);
                    GLDebug.DrawLine(Vector3.Slerp(forward, up, i / 25.0f) + endSphere, Vector3.Slerp(forward, up, (i - 1) / 25.0f) + endSphere, color, drawDuration);
                    GLDebug.DrawLine(Vector3.Slerp(-forward, up, i / 25.0f) + endSphere, Vector3.Slerp(-forward, up, (i - 1) / 25.0f) + endSphere, color, drawDuration);

                    //Start endcap
                    GLDebug.DrawLine(Vector3.Slerp(right, -up, i / 25.0f) + baseSphere, Vector3.Slerp(right, -up, (i - 1) / 25.0f) + baseSphere, colorizeBase ? color : Color.red, drawDuration);
                    GLDebug.DrawLine(Vector3.Slerp(-right, -up, i / 25.0f) + baseSphere, Vector3.Slerp(-right, -up, (i - 1) / 25.0f) + baseSphere, colorizeBase ? color : Color.red, drawDuration);
                    GLDebug.DrawLine(Vector3.Slerp(forward, -up, i / 25.0f) + baseSphere, Vector3.Slerp(forward, -up, (i - 1) / 25.0f) + baseSphere, colorizeBase ? color : Color.red, drawDuration);
                    GLDebug.DrawLine(Vector3.Slerp(-forward, -up, i / 25.0f) + baseSphere, Vector3.Slerp(-forward, -up, (i - 1) / 25.0f) + baseSphere, colorizeBase ? color : Color.red, drawDuration);
                }
            }
        }

        public static void DebugCircle(Vector3 position, Vector3 up, Color color, float radius = 1.0f,
                                       float drawDuration = 0, EGLDebug preview = EGLDebug.Editor)
        {
            Vector3 upDir = up.normalized * radius;
            Vector3 forwardDir = Vector3.Slerp(upDir, -upDir, 0.5f);
            Vector3 rightDir = Vector3.Cross(upDir, forwardDir).normalized * radius;

            Matrix4x4 matrix = new Matrix4x4();

            matrix[0] = rightDir.x;
            matrix[1] = rightDir.y;
            matrix[2] = rightDir.z;

            matrix[4] = upDir.x;
            matrix[5] = upDir.y;
            matrix[6] = upDir.z;

            matrix[8] = forwardDir.x;
            matrix[9] = forwardDir.y;
            matrix[10] = forwardDir.z;

            Vector3 lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            Vector3 nextPoint = Vector3.zero;

            color = (color == default(Color)) ? Color.white : color;

            bool drawEditor = false;
            bool drawGame = false;

            switch (preview)
            {
                case EGLDebug.Editor:
                    drawEditor = true;
                    break;

                case EGLDebug.Game:
                    drawGame = true;
                    break;

                case EGLDebug.Both:
                    drawEditor = true;
                    drawGame = true;
                    break;
            }

            for (var i = 0; i < 91; i++)
            {
                nextPoint.x = Mathf.Cos((i * 4) * Mathf.Deg2Rad);
                nextPoint.z = Mathf.Sin((i * 4) * Mathf.Deg2Rad);
                nextPoint.y = 0;

                nextPoint = position + matrix.MultiplyPoint3x4(nextPoint);

                if (drawEditor)
                    Debug.DrawLine(lastPoint, nextPoint, color, drawDuration);

                if (drawGame)
                    GLDebug.DrawLine(lastPoint, nextPoint, color, drawDuration);

                lastPoint = nextPoint;
            }
        }

        public static void DebugLine(Vector3 p1, Vector3 p2, Color color, float drawDuration = 0,
                                      EGLDebug preview = EGLDebug.Editor)
        {
            bool drawEditor = false;
            bool drawGame = false;

            switch (preview)
            {
                case EGLDebug.Editor:
                    drawEditor = true;
                    break;

                case EGLDebug.Game:
                    drawGame = true;
                    break;

                case EGLDebug.Both:
                    drawEditor = true;
                    drawGame = true;
                    break;
            }

            if (drawEditor)
            {
                Debug.DrawLine(p1, p2, color, drawDuration);
            }

            if (drawGame)
            {
                GLDebug.DrawLine(p1, p2, color, drawDuration);
            }
        }

        public static void DebugPoint(Vector3 position, Color color, float scale = 0.5f, float drawDuration = 0,
                                      EGLDebug preview = EGLDebug.Editor)
        {
            color = (color == default(Color)) ? Color.white : color;

            bool drawEditor = false;
            bool drawGame = false;

            switch (preview)
            {
                case EGLDebug.Editor:
                    drawEditor = true;
                    break;

                case EGLDebug.Game:
                    drawGame = true;
                    break;

                case EGLDebug.Both:
                    drawEditor = true;
                    drawGame = true;
                    break;
            }

            if (drawEditor)
            {
                Debug.DrawRay(position + (Vector3.up * (scale * 0.5f)), -Vector3.up * scale, color, drawDuration);
                Debug.DrawRay(position + (Vector3.right * (scale * 0.5f)), -Vector3.right * scale, color, drawDuration);
                Debug.DrawRay(position + (Vector3.forward * (scale * 0.5f)), -Vector3.forward * scale, color, drawDuration);
            }

            if (drawGame)
            {
                GLDebug.DrawRay(position + (Vector3.up * (scale * 0.5f)), -Vector3.up * scale, color, drawDuration);
                GLDebug.DrawRay(position + (Vector3.right * (scale * 0.5f)), -Vector3.right * scale, color, drawDuration);
                GLDebug.DrawRay(position + (Vector3.forward * (scale * 0.5f)), -Vector3.forward * scale, color, drawDuration);
            }
        }

        public static void DebugWireSphere(Vector3 position, Color color, float radius = 1.0f, float drawDuration = 0,
                                           EGLDebug preview = EGLDebug.Editor)
        {
            float angle = 10.0f;

            Vector3 x = new Vector3(position.x, position.y + radius * Mathf.Sin(0), position.z + radius * Mathf.Cos(0));
            Vector3 y = new Vector3(position.x + radius * Mathf.Cos(0), position.y, position.z + radius * Mathf.Sin(0));
            Vector3 z = new Vector3(position.x + radius * Mathf.Cos(0), position.y + radius * Mathf.Sin(0), position.z);

            bool drawEditor = false;
            bool drawGame = false;

            switch (preview)
            {
                case EGLDebug.Editor:
                    drawEditor = true;
                    break;

                case EGLDebug.Game:
                    drawGame = true;
                    break;

                case EGLDebug.Both:
                    drawEditor = true;
                    drawGame = true;
                    break;
            }

            for (int i = 1; i < 37; i++)
            {
                Vector3 new_x = new Vector3(position.x, position.y + radius * Mathf.Sin(angle * i * Mathf.Deg2Rad), position.z + radius * Mathf.Cos(angle * i * Mathf.Deg2Rad));
                Vector3 new_y = new Vector3(position.x + radius * Mathf.Cos(angle * i * Mathf.Deg2Rad), position.y, position.z + radius * Mathf.Sin(angle * i * Mathf.Deg2Rad));
                Vector3 new_z = new Vector3(position.x + radius * Mathf.Cos(angle * i * Mathf.Deg2Rad), position.y + radius * Mathf.Sin(angle * i * Mathf.Deg2Rad), position.z);

                if (drawEditor)
                {
                    Debug.DrawLine(x, new_x, color, drawDuration);
                    Debug.DrawLine(y, new_y, color, drawDuration);
                    Debug.DrawLine(z, new_z, color, drawDuration);
                }

                if (drawGame)
                {
                    GLDebug.DrawLine(x, new_x, color, drawDuration);
                    GLDebug.DrawLine(y, new_y, color, drawDuration);
                    GLDebug.DrawLine(z, new_z, color, drawDuration);
                }

                x = new_x;
                y = new_y;
                z = new_z;
            }
        }

        public static void DebugConeSight(Vector3 position, Vector3 direction, float length, Color color, float angle = 45,
                                          float drawDuration = 0, EGLDebug preview = EGLDebug.Editor)
        {
            if (angle > 0)
                angle = Mathf.Min(angle, 360);
            else
                angle = Mathf.Max(angle, -360);

            Vector3 forwardDir = direction * length;
            Vector3 upDir = Vector3.Slerp(forwardDir, -forwardDir, 0.5f);
            Vector3 rightDir = Vector3.Cross(forwardDir, upDir).normalized * length;

            Vector3 up;
            Vector3 rightUp;
            Vector3 right;
            Vector3 rightDown;
            Vector3 down;
            Vector3 leftDown;
            Vector3 left;
            Vector3 leftUp;

            if (angle <= 180)
            {
                float percentage = angle / 180f;

                up = position + Vector3.Slerp(forwardDir, -rightDir, percentage).normalized * length;
                rightUp = position + Vector3.Slerp(forwardDir, -upDir - rightDir, percentage).normalized * length;
                right = position + Vector3.Slerp(forwardDir, -upDir, percentage).normalized * length;
                rightDown = position + Vector3.Slerp(forwardDir, -upDir + rightDir, percentage).normalized * length;
                down = position + Vector3.Slerp(forwardDir, rightDir, percentage).normalized * length;
                leftDown = position + Vector3.Slerp(forwardDir, upDir + rightDir, percentage).normalized * length;
                left = position + Vector3.Slerp(forwardDir, upDir, percentage).normalized * length;
                leftUp = position + Vector3.Slerp(forwardDir, upDir - rightDir, percentage).normalized * length;
            }
            else
            {
                float percentage = (angle - 180) / 180f;

                up = position + Vector3.Slerp(-rightDir, -forwardDir, percentage).normalized * length;
                rightUp = position + Vector3.Slerp(-upDir - rightDir, -forwardDir, percentage).normalized * length;
                right = position + Vector3.Slerp(-upDir, -forwardDir, percentage).normalized * length;
                rightDown = position + Vector3.Slerp(-upDir + rightDir, -forwardDir, percentage).normalized * length;
                down = position + Vector3.Slerp(rightDir, -forwardDir, percentage).normalized * length;
                leftDown = position + Vector3.Slerp(upDir + rightDir, -forwardDir, percentage).normalized * length;
                left = position + Vector3.Slerp(upDir, -forwardDir, percentage).normalized * length;
                leftUp = position + Vector3.Slerp(upDir - rightDir, -forwardDir, percentage).normalized * length;
            }

            bool drawEditor = false;
            bool drawGame = false;

            switch (preview)
            {
                case EGLDebug.Editor:
                    drawEditor = true;
                    break;

                case EGLDebug.Game:
                    drawGame = true;
                    break;

                case EGLDebug.Both:
                    drawEditor = true;
                    drawGame = true;
                    break;
            }

            #region Rays Logic
            if (drawEditor)
            {
                //Forward
                Debug.DrawRay(position, forwardDir, color, drawDuration);

                //Left Down
                Debug.DrawLine(position, leftDown, color, drawDuration);
                //Left Up
                Debug.DrawLine(position, leftUp, color, drawDuration);
                //Right Down
                Debug.DrawLine(position, rightDown, color, drawDuration);
                //Right Up
                Debug.DrawLine(position, rightUp, color, drawDuration);

                //Left
                Debug.DrawLine(position, left, color, drawDuration);
                //Right
                Debug.DrawLine(position, right, color, drawDuration);
                //Down
                Debug.DrawLine(position, down, color, drawDuration);
                //Up
                Debug.DrawLine(position, up, color, drawDuration);
            }

            if (drawGame)
            {
                //Forward
                GLDebug.DrawRay(position, forwardDir, color, drawDuration);

                //Left Down
                GLDebug.DrawLine(position, leftDown, color, drawDuration);
                //Left Up
                GLDebug.DrawLine(position, leftUp, color, drawDuration);
                //Right Down
                GLDebug.DrawLine(position, rightDown, color, drawDuration);
                //Right Up
                GLDebug.DrawLine(position, rightUp, color, drawDuration);

                //Left
                GLDebug.DrawLine(position, left, color, drawDuration);
                //Right
                GLDebug.DrawLine(position, right, color, drawDuration);
                //Down
                GLDebug.DrawLine(position, down, color, drawDuration);
                //Up
                GLDebug.DrawLine(position, up, color, drawDuration);
            }
            #endregion

            #region Circles
            Vector3 midUp = (up + position) / 2;
            Vector3 midDown = (down + position) / 2;

            Vector3 endPoint = (up + down) / 2;
            Vector3 midPoint = (midUp + midDown) / 2;

            DebugCircle(endPoint, direction, color, (up - down).sqrMagnitude / 2, drawDuration, preview);
            DebugCircle(midPoint, direction, color, (midUp - midDown).sqrMagnitude / 2, drawDuration, preview);
            #endregion

            #region Cone base sphere logic
            Vector3 lastLdPosition = leftDown;
            Vector3 lastLuPosition = leftUp;
            Vector3 lastRdPosition = rightDown;
            Vector3 lastRuPosition = rightUp;

            Vector3 lastLPosition = left;
            Vector3 lastRPosition = right;
            Vector3 lastDPosition = down;
            Vector3 lastUPosition = up;

            int index = 1;

            for (int i = 0; i < 7; i++)
            {
                float tempAngle = angle - index;

                Vector3 nextLdPosition;
                Vector3 nextLuPosition;
                Vector3 nextRdPosition;
                Vector3 nextRuPosition;

                Vector3 nextLPosition;
                Vector3 nextRPosition;
                Vector3 nextDPosition;
                Vector3 nextUPosition;

                if (tempAngle <= 180)
                {
                    float percentage = tempAngle / 180f;

                    nextLdPosition = position + Vector3.Slerp(forwardDir, upDir + rightDir, percentage).normalized * length;
                    nextLuPosition = position + Vector3.Slerp(forwardDir, upDir - rightDir, percentage).normalized * length;
                    nextRdPosition = position + Vector3.Slerp(forwardDir, -upDir + rightDir, percentage).normalized * length;
                    nextRuPosition = position + Vector3.Slerp(forwardDir, -upDir - rightDir, percentage).normalized * length;

                    nextDPosition = position + Vector3.Slerp(forwardDir, rightDir, percentage).normalized * length;
                    nextUPosition = position + Vector3.Slerp(forwardDir, -rightDir, percentage).normalized * length;
                    nextRPosition = position + Vector3.Slerp(forwardDir, -upDir, percentage).normalized * length;
                    nextLPosition = position + Vector3.Slerp(forwardDir, upDir, percentage).normalized * length;
                }
                else
                {
                    float percentage = (tempAngle - 180) / 180f;

                    nextLdPosition = position + Vector3.Slerp(upDir + rightDir, -forwardDir, percentage).normalized * length;
                    nextLuPosition = position + Vector3.Slerp(upDir - rightDir, -forwardDir, percentage).normalized * length;
                    nextRdPosition = position + Vector3.Slerp(-upDir + rightDir, -forwardDir, percentage).normalized * length;
                    nextRuPosition = position + Vector3.Slerp(-upDir - rightDir, -forwardDir, percentage).normalized * length;

                    nextDPosition = position + Vector3.Slerp(rightDir, -forwardDir, percentage).normalized * length;
                    nextUPosition = position + Vector3.Slerp(-rightDir, -forwardDir, percentage).normalized * length;
                    nextRPosition = position + Vector3.Slerp(-upDir, -forwardDir, percentage).normalized * length;
                    nextLPosition = position + Vector3.Slerp(upDir, -forwardDir, percentage).normalized * length;
                }

                if (drawEditor)
                {
                    Debug.DrawLine(lastLdPosition, nextLdPosition, color, drawDuration);
                    Debug.DrawLine(lastLuPosition, nextLuPosition, color, drawDuration);
                    Debug.DrawLine(lastRdPosition, nextRdPosition, color, drawDuration);
                    Debug.DrawLine(lastRuPosition, nextRuPosition, color, drawDuration);

                    Debug.DrawLine(lastDPosition, nextDPosition, color, drawDuration);
                    Debug.DrawLine(lastUPosition, nextUPosition, color, drawDuration);
                    Debug.DrawLine(lastRPosition, nextRPosition, color, drawDuration);
                    Debug.DrawLine(lastLPosition, nextLPosition, color, drawDuration);
                }

                if (drawGame)
                {
                    GLDebug.DrawLine(lastLdPosition, nextLdPosition, color, drawDuration);
                    GLDebug.DrawLine(lastLuPosition, nextLuPosition, color, drawDuration);
                    GLDebug.DrawLine(lastRdPosition, nextRdPosition, color, drawDuration);
                    GLDebug.DrawLine(lastRuPosition, nextRuPosition, color, drawDuration);

                    GLDebug.DrawLine(lastDPosition, nextDPosition, color, drawDuration);
                    GLDebug.DrawLine(lastUPosition, nextUPosition, color, drawDuration);
                    GLDebug.DrawLine(lastRPosition, nextRPosition, color, drawDuration);
                    GLDebug.DrawLine(lastLPosition, nextLPosition, color, drawDuration);
                }

                lastLdPosition = nextLdPosition;
                lastLuPosition = nextLuPosition;
                lastRdPosition = nextRdPosition;
                lastRuPosition = nextRuPosition;

                lastDPosition = nextDPosition;
                lastUPosition = nextUPosition;
                lastRPosition = nextRPosition;
                lastLPosition = nextLPosition;
                index += 60;
            }
            #endregion
        }
    }
}