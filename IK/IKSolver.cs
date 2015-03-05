using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace QTM2Unity
{
    abstract class IKSolver
    {
        abstract public Bone[] solveBoneChain(Bone[] bones, Vector3 target); // TODO bones as ref instead?
    }
}
