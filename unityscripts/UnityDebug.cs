using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QTM2Unity
{
    static class UnityDebug
    {
        public static void DrawRays(OpenTK.Quaternion rot, Vector3 pos)
        {
            
            OpenTK.Vector3 right = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitX, rot);
            OpenTK.Vector3 up = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, rot);
            OpenTK.Vector3 forward = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitZ, rot);
            Debug.DrawRay(pos, cv(up) * 0.07f, Color.green);
            Debug.DrawRay(pos, cv(right) * 0.07f, Color.red);
            Debug.DrawRay(pos, cv(forward) * 0.07f, Color.blue);
        }
        public static void DrawRay(OpenTK.Vector3 pos, OpenTK.Vector3 dir, Color c)
        {
            Debug.DrawRay(cv(pos), cv(dir) * 1f, c);

        }
        public static void DrawRay(UnityEngine.Vector3 pos, OpenTK.Vector3 dir, Color c)
        {
            Debug.DrawRay(pos, cv(dir) * 1f, c);

        }
        public static void DrawRay(OpenTK.Vector3 pos, OpenTK.Vector3 dir)
        {
            DrawRay(pos, dir * 1f, Color.black);

        }
        public static void DrawRays(OpenTK.Quaternion rot, OpenTK.Vector3 pos)
        {
            DrawRays(rot, cv(pos));
        }
        public static void DrawVector(OpenTK.Vector3 start, OpenTK.Vector3 vec)
        {
            DrawLine(start, start + vec);
        }
        public static void DrawLine(OpenTK.Vector3 start, OpenTK.Vector3 end)
        {
            Debug.DrawLine(cv(start) , cv(end));
        }
        public static void DrawLine(OpenTK.Vector3 start, OpenTK.Vector3 end, Color c)
        {
            Debug.DrawLine(cv(start), cv(end),c);
        }

        public static Vector3 cv(OpenTK.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        public static Quaternion cq(OpenTK.Quaternion q)
        {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }
        public static Vector3[] CreateEllipse(float a, float b, Vector3 pos, Quaternion rot, int resolution)
        {

            Vector3[] positions = new Vector3[resolution + 1]; Vector3 center = pos;

            for (int i = 0; i <= resolution; i++)
            {
                float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
                positions[i] = new Vector3(a * Mathf.Cos(angle), b * Mathf.Sin(angle), 0.0f);
                positions[i] = rot * positions[i] + center;
            }

            return positions;
        }
        public static Vector3[] CreateEllipse(float a, float b, OpenTK.Vector3 pos, Quaternion rot, int resolution)
        {
            return CreateEllipse(a, b, cv(pos), rot, resolution);
        }
        public static Vector3[] CreateEllipse(float a, float b, OpenTK.Vector3 pos, OpenTK.Quaternion rot, int resolution)
        {
            return CreateEllipse(a, b, cv(pos), cq(rot), resolution);
        }
    }
}
