using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    class Bon : IEquatable<Bon>
    {
        public Vector3  Pos;
        public Quaternion Rot;
        public readonly string Name;
        public Bon(string name)
        {
            this.Name = name;
        }
        public Bon(string name,Vector3 possition) : this(name)
        {
            Pos = possition;
        }
        public Bon(string name, Vector3 possition, Quaternion rotation) : this (name, possition)
        {
            Rot = rotation;
        }
        public bool Equals(Bon other)
        {
            return Name.Equals(other.Name) && Rot.Equals(other.Rot) && Pos.Equals(other.Pos);
        }
        public string ToString()
        {
            return string.Format("{0} at Pos: {1} with Rot: {2}",Name,Pos,Rot);
        }
    }
}
