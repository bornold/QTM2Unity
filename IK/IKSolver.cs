using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    abstract class IKSolver
    {
        abstract public Bone[] solveBoneChain(Bone[] bones, Bone target, Vector3 L1); 
    }
}
