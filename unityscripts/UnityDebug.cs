using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QTM2Unity
{
    static class UnityDebug
    {
        public static void sanity(BipedSkeleton test, string message = "")
        {
            UnityDebug.sanity(test.Root, message);
        }
        public static void sanity(TreeNode<Bone> test, string message = "")
        {
            foreach (TreeNode<Bone> b in test)
            {
                if (b.IsRoot || b.Parent.IsRoot || b.Data.ParentPointer != OpenTK.Quaternion.Identity) continue;
                if (b.Data.Pos == b.Parent.Data.Pos)
                {
                    UnityEngine.Debug.LogWarning(b.Parent.Data + " and " + b.Data + " is the same");
                    continue;
                }
                OpenTK.Vector3 ray1 = b.Parent.Data.GetYAxis();
                OpenTK.Vector3 ray2 = (b.Data.Pos - b.Parent.Data.Pos);
                bool para = Vector3Helper.Parallel(ray1, ray2, 10.02f);
                if (!para)
                {

                    //UnityEngine.Debug.LogError(b.Parent.Data + " and " + b.Data);
                    //UnityEngine.Debug.LogError(ray1 + " and " + ray2);
                    ray1.NormalizeFast();
                    ray2.NormalizeFast();
                    UnityDebug.DrawVector(b.Parent.Data.Pos, ray1, UnityEngine.Color.black);
                    UnityDebug.DrawVector(b.Parent.Data.Pos, ray2, UnityEngine.Color.blue);

                }
            }
        }
        public static void sanity(Bone[] test, string message = "")
        {
            for (int i = 1; i < test.Length; i++)
            {
                OpenTK.Vector3 ray1 = test[i - 1].GetYAxis();
                OpenTK.Vector3 ray2 = (test[i].Pos - test[i - 1].Pos);
                if (!Vector3Helper.Parallel(ray1, ray2, 0.02f))
                {
                    UnityEngine.Debug.LogError(message + '\n' + test[i - 1]);
                }
            }
        }

        public static void DrawTwistConstraints(Bone b, Bone refBone, OpenTK.Vector3 poss, float scale)
        {
            if (b.Orientation.Xyz.IsNaN() || refBone.Orientation.Xyz.IsNaN())
            {
                return;
            }
            OpenTK.Vector3 thisY = b.GetYAxis();

            OpenTK.Quaternion referenceRotation = refBone.Orientation * b.ParentPointer;
            OpenTK.Vector3 parentY = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, referenceRotation);
            OpenTK.Vector3 parentZ = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitZ, referenceRotation);

            OpenTK.Quaternion rot = QuaternionHelper2.GetRotationBetween(parentY, thisY);
            OpenTK.Vector3 reference = OpenTK.Vector3.Transform(parentZ, rot);
            reference.Normalize();
            Debug.DrawRay(poss.Convert(), (b.GetZAxis() * scale*2).Convert(), Color.cyan);


            float startTwistLimit = OpenTK.MathHelper.DegreesToRadians(b.StartTwistLimit);
            OpenTK.Vector3 m = OpenTK.Vector3.Transform(reference, OpenTK.Quaternion.FromAxisAngle(thisY, startTwistLimit));
            m.Normalize();
            Debug.DrawRay(poss.Convert(), m.Convert() * scale, Color.yellow);

            float endTwistLimit = OpenTK.MathHelper.DegreesToRadians(b.EndTwistLimit);
            OpenTK.Vector3 m2 = OpenTK.Vector3.Transform(reference, OpenTK.Quaternion.FromAxisAngle(thisY, endTwistLimit));
            m2.Normalize();
            Debug.DrawRay(poss.Convert(), m2.Convert() * scale, Color.magenta);

            Debug.DrawLine((poss + (m*scale)).Convert(), (poss + (m2*scale)).Convert(), Color.cyan);
        }
        public static void DrawTwistConstraints(Bone b, Bone refBone, OpenTK.Vector3 poss)
        {
            DrawTwistConstraints(b, refBone, poss, 0.1f);
        }
        public static void DrawRays(OpenTK.Quaternion rot, Vector3 pos, float scale)
        {
            OpenTK.Vector3 right = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitX, rot);
            OpenTK.Vector3 up = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, rot);
            OpenTK.Vector3 forward = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitZ, rot);
            Debug.DrawRay(pos, up.Convert() * scale, Color.green);
            Debug.DrawRay(pos, right.Convert() * scale, Color.red);
            Debug.DrawRay(pos, forward.Convert() * scale, Color.blue);
        }
        public static void DrawRays(OpenTK.Quaternion rot, OpenTK.Vector3 pos, float scale)
        {
            DrawRays(rot, pos.Convert(), scale);
        }
        public static void DrawRays(OpenTK.Quaternion rot, Vector3 pos)
        {
            DrawRays(rot, pos, 0.07f);
        }
        public static void DrawRays(OpenTK.Quaternion rot, OpenTK.Vector3 pos)
        {
            DrawRays(rot, pos.Convert());
        }
        
        public static void DrawRays2(Quaternion rot, Vector3 pos, float scale)
        {
            OpenTK.Vector3 right = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitX, rot.Convert());
            OpenTK.Vector3 up = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, rot.Convert());
            OpenTK.Vector3 forward = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitZ, rot.Convert());
            Debug.DrawRay(pos, up.Convert() * scale, Color.yellow);
            Debug.DrawRay(pos, right.Convert() * scale, Color.magenta);
            Debug.DrawRay(pos, forward.Convert() * scale, Color.cyan);
        }
        public static void DrawRays2(OpenTK.Quaternion rot, OpenTK.Vector3 pos, float scale)
        {
            DrawRays2(rot.Convert(), pos.Convert(), scale);
        }
        public static void DrawRays2(OpenTK.Quaternion rot, OpenTK.Vector3 pos)
        {
            DrawRays2(rot.Convert(), pos.Convert(), 0.07f);
        }
        
        public static void DrawVector(OpenTK.Vector3 pos, OpenTK.Vector3 dir, Color c)
        {
            Debug.DrawRay(pos.Convert(), dir.Convert(), c);
        }
        public static void DrawVector(OpenTK.Vector3 pos, OpenTK.Vector3 dir, float size)
        {
            Debug.DrawRay(pos.Convert(), Vector3.Normalize(dir.Convert()) * size, Color.black);
        }
        public static void DrawVector(OpenTK.Vector3 pos, OpenTK.Vector3 dir, float size, Color c)
        {
            Debug.DrawRay(pos.Convert(), Vector3.Normalize(dir.Convert()) * size, c);
        }
        public static void DrawVector(OpenTK.Vector3 pos, OpenTK.Vector3 dir)
        {
            DrawVector(pos, dir, Color.black);
        }

        public static void DrawLine(OpenTK.Vector3 start, OpenTK.Vector3 end)
        {
            Debug.DrawLine(start.Convert(), end.Convert());
        }
        public static void DrawLine(OpenTK.Vector3 start, OpenTK.Vector3 end, Color c)
        {
            Debug.DrawLine(start.Convert(), end.Convert(), c);
        }

        public static void CreateEllipse(float x, float y, Vector3 pos, Quaternion rot, int resolution, Color c)
        {

            Vector3[] positions = new Vector3[resolution + 1];
            for (int i = 0; i <= resolution; i++)
            {
                float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
                positions[i] = new Vector3(x * Mathf.Cos(angle), 0.0f, y * Mathf.Sin(angle));
                positions[i] = rot * positions[i] + pos;
                if (i > 0)
                {
                    Debug.DrawLine(positions[i], positions[i - 1], c);
                }
            }
            Debug.DrawLine(positions[0], positions[positions.Length-1], c);

        }

        public static OpenTK.Vector3[] CreateIrregularCone(OpenTK.Vector4 strains, OpenTK.Vector3 top, OpenTK.Vector3 L1, OpenTK.Quaternion rot, int resolution, float scale)
        {
            //L1 = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, rot);
            L1.Normalize();
            List<OpenTK.Vector3> positions = new List<OpenTK.Vector3>();
            positions.AddRange(GetQuarter(strains.X, strains.Y, top, L1, rot, resolution, 1, scale));
            positions.AddRange(GetQuarter(strains.Z, strains.Y, top, L1, rot, resolution, 2, scale));
            positions.AddRange(GetQuarter(strains.Z, strains.W, top, L1, rot, resolution, 3, scale));
            positions.AddRange(GetQuarter(strains.X, strains.W, top, L1, rot, resolution, 4, scale));
            OpenTK.Vector3 prev = positions.First();
            Color c;
            Color c2 = Color.black;
            int i = 0;
            foreach (OpenTK.Vector3 v in positions)
            {
                float part = ((float)i % ((float)resolution / 4f)) / ((float)resolution / 4f);
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
                i++;
                DrawLine(v, prev , c2);
                DrawLine(top, v , c);
                prev = v;
            }

            c = Color.blue;
            DrawLine(prev, positions.First(), c);
            DrawLine(top, positions.First(), c);
            return positions.ToArray();
        }

        private static List<OpenTK.Vector3> GetQuarter(
            float a, float b, OpenTK.Vector3 top, 
            OpenTK.Vector3 L1,       OpenTK.Quaternion rot, 
            int resolution,         int p, float scale)
        {
            OpenTK.Quaternion extraRot = OpenTK.Quaternion.Identity;
            OpenTK.Vector3 L2 = L1;
            if (a > 90 && b > 90)
            {
                L2 = -L1;
                a = 180 - a;
                b = 180 - b;
            }
            else if ((a > 90) ^ (b > 90))
            {
            #region Crazy cone
                OpenTK.Vector3 right = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitX, rot);
                OpenTK.Vector3 forward = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitZ, rot);
                L2 = right;
                switch (p)
                {
                case 1:
                    if (a > 90) L2 = right;
                    else L2 = forward;
                    break;
                case 2:
                    if (a > 90) L2 = -right;
                    else L2 = forward;
                    break;
                case 3:
                    if (a > 90) L2 = -right;
                    else L2 = -forward;
                    break;
                case 4:
                    if (a > 90) L2 = right;
                    else L2 = -forward;
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
            #endregion
            }

            float A = Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(Math.Min(89.99f, a)));
            float B = Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(Math.Min(89.99f, b)));
            List<OpenTK.Vector3> te = new List<OpenTK.Vector3>();
            float part = resolution * (p / 4f);
            float start = resolution * ((p - 1f) / 4f);
            for (float i = start; i < part; i++)
            {
                float angle = i / resolution * 2.0f * Mathf.PI;
                float x = A * Mathf.Cos(angle);
                float z = B * Mathf.Sin(angle);
                OpenTK.Vector3 t = new OpenTK.Vector3(x, 0.0f, z );
                t = OpenTK.Vector3.Transform(t, extraRot * rot );
                t += L2;
                t.Normalize();
                t *= scale;
                t += top;
                te.Add(t);
            }
            return te;
        }
    }
}