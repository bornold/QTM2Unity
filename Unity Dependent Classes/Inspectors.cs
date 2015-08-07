#region --- LINCENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Jonas Bornold

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using UnityEngine;

namespace QTM2Unity {
   [System.Serializable]
    public class Debugging
    {
        public Markers markers;
        public BodyRig bodyRig;
        public Vector3 Offset = new Vector3(0, 0, 0);

    }
   [System.Serializable]
   public class Markers
   {
       public bool ShowMarkers = false;
       [Range(0.001f, 0.05f)]
       public float MarkerScale = 0.01f;
       public bool MarkerBones = false;
       public Color boneColor = Color.blue;
   }
    [System.Serializable]
    public class BodyRig
    {
        public bool showSkeleton = false;
        public Color skelettColor = Color.black;
        public bool showJoints = false;
        [Range(0.01f, 0.05f)]
        public float jointScale = 0.015f;
        public Color jointColor = Color.green;
        public bool showRotationTrace = false;
        [Range(0.01f, 0.5f)]
        public float traceLength = 0.08f;
        public JointsConstrains jointsConstrains;
        [System.Serializable]
        public class JointsConstrains
        {
            public bool showConstraints = false;
            [Range(0.01f, 0.5f)]
            public float coneSize = 0.05f;
            [Range(1, 150)]
            public int coneResolution = 50;
            public bool showTwistConstraints = false;
        }
    }
    [System.Serializable]
    public class CharacterModel
    {
        
        public CharactersModel model = CharactersModel.Model1;
        public BoneRotations boneRotatation = new Model1();
    }

}
