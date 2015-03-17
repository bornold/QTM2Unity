using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QTM2Unity
{
    static class UnityDebug
    {
        public static void DrawRays(OpenTK.Quaternion rot, Vector3 pos, float scale)
        {

            OpenTK.Vector3 right = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitX, rot);
            OpenTK.Vector3 up = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, rot);
            OpenTK.Vector3 forward = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitZ, rot);
            Debug.DrawRay(pos, cv(up) * scale, Color.green);
            Debug.DrawRay(pos, cv(right) * scale, Color.red);
            Debug.DrawRay(pos, cv(forward) * scale, Color.blue);
        }
        public static void DrawRays(OpenTK.Quaternion rot, OpenTK.Vector3 pos, float scale)
        {
            DrawRays(rot, cv(pos), scale);
        }
        public static void DrawRays(OpenTK.Quaternion rot, Vector3 pos)
        {
            DrawRays(rot, pos, 0.07f);
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
        public static void CreateEllipse(float x, float y, int resolution)
        {
            CreateEllipse(x, y, Vector3.zero, Quaternion.identity, resolution, Color.white);
        }
        public static void CreateEllipse(float x, float y, int resolution, Color c)
        {
            CreateEllipse(x, y, Vector3.zero, Quaternion.identity, resolution, c);
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
            OpenTK.Vector3 center = top + o;//OpenTK.Vector3.Normalize(o); 
            float S = o.Length;
            strains.X = S * Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(Math.Min(89.9f,strains.X)));
            strains.Y = S * Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(Math.Min(89.9f,strains.Y)));
            strains.Z = S * Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(Math.Min(89.9f,strains.Z)));
            strains.W = S * Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(Math.Min(89.9f,strains.W)));

            OpenTK.Vector3[] positions = new OpenTK.Vector3[resolution];
            for (int i = 0; i < resolution; i++)
            {
                float a, b;
                Color c;
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
        public static void CreateIrregularCone2(OpenTK.Vector4 strains, OpenTK.Vector3 top, OpenTK.Vector3 dir,
                                                OpenTK.Quaternion rot, int resolution)
        {
            dir.Normalize();
            OpenTK.Vector3 center = top + dir;
            strains.X = Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(Math.Min(89.9f, strains.X)));
            strains.Y = Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(Math.Min(89.9f, strains.Y)));
            strains.Z = Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(Math.Min(89.9f, strains.Z)));
            strains.W = Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(Math.Min(89.9f, strains.W)));

            OpenTK.Vector3 current, last, first;

            float a = strains.X, b, angle, part;
            last = new OpenTK.Vector3(a, 0.0f, 0.0f);
            last = OpenTK.Vector3.Transform(last, rot) + center;
            last = last - top;  
            last.Normalize();
            last = top + last;
            first = last;
            Color c = Color.blue;
            DrawLine(top, first, c);
            for (int i = 1; i < resolution ; i++)
            {
                angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
                part = ((float)i % ((float)resolution / 4f)) / ((float)resolution / 4f);
                if (i < resolution * 0.25)
                {  //Q1
                    c = Color.Lerp(Color.blue, Color.red, part);
                    a = strains.X;
                    b = strains.Y;
                }
                else if (i < resolution * 0.5)
                {   //Q4
                    c = Color.Lerp(Color.red, Color.green, part);
                    a = strains.Z;
                    b = strains.Y;
                }
                else if (i < resolution * 0.75)
                {   //Q3
                    c = Color.Lerp(Color.green, Color.yellow, part);
                    a = strains.Z;
                    b = strains.W;
                }
                else
                {   //Q2
                    c = Color.Lerp(Color.yellow, Color.blue, part);
                    a = strains.X;
                    b = strains.W;

                }
                current = new OpenTK.Vector3(a * Mathf.Cos(angle), b * Mathf.Sin(angle), 0.0f);
                current = OpenTK.Vector3.Transform(current, rot) + center;
                current = current - top;
                current.Normalize();
                current = top + current;
                DrawLine(top, current, c);
                DrawLine(current, last, c);
                last = current;
            }
            DrawLine(first, last, c);
        }
        public static void CreateIrregularCone3(OpenTK.Vector4 strains, OpenTK.Vector3 top, OpenTK.Vector3 L1,
                                                OpenTK.Quaternion rot, int resolution, float scale)
        {
            List<OpenTK.Vector3> positions = new List<OpenTK.Vector3>(); 
            positions.AddRange(GetQuarter(strains.X,strains.Y, top, L1, rot, resolution, 1, scale));
            positions.AddRange(GetQuarter(strains.Z, strains.Y, top, L1, rot, resolution, 2, scale));
            positions.AddRange(GetQuarter(strains.Z, strains.W, top, L1, rot, resolution, 3, scale));
            positions.AddRange(GetQuarter(strains.X, strains.W, top, L1, rot, resolution, 4, scale));
            OpenTK.Vector3 prev = positions.First();
            Color c;
            int i = 0;
            foreach (OpenTK.Vector3 v in positions)
            {
                float part = ((float)i++ % ((float)resolution / 4f)) / ((float)resolution / 4f);
                if (i < resolution * 0.25)
                {  //Q1
                    c = Color.Lerp(Color.blue, Color.red, part);
                }
                else if (i < resolution * 0.5)
                {   //Q4
                    c = Color.Lerp(Color.red, Color.green, part);
                }
                else if (i < resolution * 0.75)
                {   //Q3
                    c = Color.Lerp(Color.green, Color.yellow, part);
                }
                else
                {   //Q2
                    c = Color.Lerp(Color.yellow, Color.blue, part);
                }
                DrawLine(v, prev , c);
                DrawLine(top, v , c);
                prev = v;
            }

            c = Color.blue;
            DrawLine(prev, positions.First(), c);
            DrawLine(top, positions.First(), c);
        }
        private static List<OpenTK.Vector3> GetQuarter(
            float a, float b, OpenTK.Vector3 top, 
            OpenTK.Vector3 L1,       OpenTK.Quaternion rot, 
            int resolution,         int p, float scale)
        {
            OpenTK.Quaternion extraRot = OpenTK.Quaternion.Identity;

            if (a > 90 && b > 90)
            {
                L1 = -L1;
                a = 180 - a;
                b = 180 - b;
            }
            else if ((a > 90) ^ (b > 90))
            {
                OpenTK.Vector3 right = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitX, rot);
                OpenTK.Vector3 up = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, rot);
                OpenTK.Vector3 L2 = right;
                switch (p)
                {
                    case 1:
                        if (a > 90) L2 = right;
                        else L2 = up;
                        break;
                    case 2:
                        if (a > 90) L2 = -right;
                        else L2 = up;
                        break;
                    case 3:
                        if (a > 90) L2 = -right;
                        else L2 = -up;
                        break;
                    case 4:
                        if (a > 90) L2 = right;
                        else L2 = -up;
                        break;
                    default:
                        break;
                }
                if (a > 90)
                    a = a - 90;
                else
                    b = b - 90;
                float angle = OpenTK.Vector3.CalculateAngle(L2, L1);
                OpenTK.Vector3 axis = OpenTK.Vector3.Cross(L2, L1);
                extraRot = OpenTK.Quaternion.FromAxisAngle(axis, angle);
                extraRot = OpenTK.Quaternion.Invert(extraRot);
                L1 = L2;
            }
            a = Math.Min(89.9f, a);
            b = Math.Min(89.9f, b);
            float A = Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(a));
            float B = Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(b));

            List<OpenTK.Vector3> te = new List<OpenTK.Vector3>();
            float part = resolution * (p / 4f);
            float start = resolution * ((p - 1f) / 4f);

            for (float i = start; i < part; i++)
            {
                float angle = i / resolution * 2.0f * Mathf.PI;
                float x = A * Mathf.Cos(angle);
                float y = B * Mathf.Sin(angle);
                OpenTK.Vector3 t = new OpenTK.Vector3(x, y, 0.0f );
                t = OpenTK.Vector3.Transform(t, rot);
                t = OpenTK.Vector3.Transform(t, extraRot);
                t += L1;
                t.Normalize();
                t = top + t;
                t *= scale;
                te.Add(t);
            }
            return te;
        }
    }
}