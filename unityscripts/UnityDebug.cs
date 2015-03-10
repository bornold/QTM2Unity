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
            Debug.DrawRay(cv(pos), cv(dir) * 10f, c);

        }
        public static void DrawRay(UnityEngine.Vector3 pos, OpenTK.Vector3 dir, Color c)
        {
            Debug.DrawRay(pos, cv(dir) * 10f, c);

        }
        public static void DrawRay(OpenTK.Vector3 pos, OpenTK.Vector3 dir)
        {
            DrawRay(pos, dir * 10f, Color.black);

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
        public static OpenTK.Vector3 cv(Vector3 v)
        {
            return new OpenTK.Vector3(v.x, v.y, v.z);
        }
        public static Quaternion cq(OpenTK.Quaternion q)
        {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }
        public static void CreateEllipse(float x, float y, Vector3 pos, Quaternion rot, int resolution, Color c)
        {

            Vector3[] positions = new Vector3[resolution + 1];
            Vector3 center = pos;

            for (int i = 0; i <= resolution; i++)
            {
                float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
                positions[i] = new Vector3(x * Mathf.Cos(angle), y * Mathf.Sin(angle), 0.0f);
                positions[i] = rot * positions[i] + center;
            }
            for (int i = 0; i < resolution; i++)
            {
                Debug.DrawLine(positions[i], positions[i + 1], c);
            }
        }
        public static void CreateEllipse(float x, float y, OpenTK.Vector3 pos, Quaternion rot, int resolution,Color c)
        {
            CreateEllipse(x, y, cv(pos), rot, resolution,c);
        }
        public static void CreateEllipse(float x, float y, OpenTK.Vector3 pos, OpenTK.Quaternion rot, int resolution, Color c)
        {
            CreateEllipse(x, y, cv(pos), cq(rot), resolution,c);
        }
        public static void CreateIrregularCone(OpenTK.Vector4 strains, OpenTK.Vector3 top, OpenTK.Vector3 o,
            OpenTK.Quaternion rot, int resolution)
        {

            OpenTK.Vector4 rests = new OpenTK.Vector4();
            if (strains.X >= 90f)
            {
                Debug.Log("X is Over 90  " + strains.X % 90f);
                rests.X = 89.9f;
            }
            if (strains.Y >= 90f)
            {
                Debug.Log("Y is Over 90  " + strains.Y % 90f);
                rests.Y = 89.9f;
            }
            if (strains.Z >= 90f)
            {
                Debug.Log("Z is Over 90  " + strains.Z % 90f);
                rests.Z = 89.9f;
            }
            if (strains.W >= 90f)
            {
                Debug.Log("W is Over 90  " + strains.W % 90f);
                rests.W = 89.9f;
            }
            OpenTK.Vector3 center = top + o;
            float S = (top - o).Length;
            strains.X = S * Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(strains.X));
            strains.Y = S * Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(strains.Y));
            strains.Z = S * Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(strains.Z));
            strains.W = S * Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(strains.W));

            OpenTK.Vector3[] positions = new OpenTK.Vector3[resolution + 1];
            float a, b;
            Color c;
            for (int i = 0; i <= resolution; i++)
            {
                float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
                if (i < resolution * 0.25)
                {
                    //Q1
                    c = Color.blue;
                    a = strains.X;
                    b = strains.Y;
                }
                else if (i < resolution * 0.5)
                {
                    //Q4
                    c = Color.red;
                    a = strains.Z;
                    b = strains.Y;
                }
                else if (i < resolution * 0.75)
                {
                    //Q3
                    c = Color.green;
                    a = strains.Z;
                    b = strains.W;
                }
                else
                {
                    //Q2
                    c = Color.yellow;
                    a = strains.X;
                    b = strains.W;

                }
                positions[i] = new OpenTK.Vector3(a * Mathf.Cos(angle), b * Mathf.Sin(angle), 0.0f);
                positions[i] = OpenTK.Vector3.Transform(positions[i], rot) + center;
                if (i > 0)
                {
                    DrawLine(positions[i], positions[i - 1], c);
                    DrawLine(top, positions[i], c);
                }
            }
        }
    }
}
