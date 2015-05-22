using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using OpenTK;
using QTM2Unity;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Globalization;
namespace TestForQTM2UnityProject
{
    public class Program
    {
        static RTClient rtClient;
        static short portUDP = 4545;
        static int streamFreq = 60;// 30;
        static int streammode = 1;
        static int server = 0;
        static string gpath = @"C:\Users\Jonas\Google Drive\Tests";
        static bool stream6d = false;
        static bool stream3d = true;
        static string today;
        public static double ElapsedMilliS(Stopwatch sw)
        {
            return (1000.0 * (double)sw.ElapsedTicks / Stopwatch.Frequency);
        }
        public static double ElapsedNanoS(Stopwatch sw)
        {
            return 1000000000.0 * (double)sw.ElapsedTicks / Stopwatch.Frequency;
        }
        public static void Main(string[] args)
        {
            Connect();
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            today = DateTime.Now.ToString("ddMM_HHmm");
            gpath = gpath + "/" + today + "/";
            DirectoryInfo di = Directory.CreateDirectory(gpath);
            Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(gpath));            
            //Console.WriteLine("Name of the trail:");
            //today = Console.ReadLine() + today;
            int intervall = Math.Max(1, (int)Math.Ceiling((decimal)1000 / streamFreq));
            var completeMarkerRun = GetCompleteSetOfMarkers(verbose: true, intervall:intervall);
            var jl = new JointLocalization();
            var GoldenStandard = new List<BipedSkeleton>();
            Console.WriteLine("Getting golden standard...");
            foreach (var list in completeMarkerRun)
            {
                var tempSkel = new BipedSkeleton();
                jl.GetJointLocation(list.Item2, ref tempSkel);
                GoldenStandard.Add(tempSkel);
            }
            //Console.WriteLine("Save Golden to file? (y)");
            //if (Console.ReadKey().KeyChar == 'y')
            //{
            //    Console.WriteLine();
            //    Console.WriteLine("Append to name?");
            //    string appendtoname = Console.ReadLine();
            //    SaveToFile(GoldenStandard, "Golden_" + appendtoname);
            //}
            do
            {
                Console.WriteLine("1 RemoveAJointTest \n2  RemoveRandomMarkerTest\n_ Exit");
                string line = Console.ReadLine();
                int n;
                if (int.TryParse(line, out n))
                {
                    if (n == 1)
                    {
                        RemoveAJointTest(GoldenStandard);
                    }
                    else if (n == 2)
                    {
                        RemoveRandomMarkerTest(completeMarkerRun, GoldenStandard);
                    }
                }
                else break;
            } while (true);
            Disconnect();
        }

        private static void RemoveAJointTest(List<BipedSkeleton> GoldenStandard)
        {
            int jointnumber = 0;
            var map = new Dictionary<int,string>();
            Console.WriteLine("Choose joint to remove:");
            foreach (TreeNode<Bone> j in GoldenStandard[0])
            {
                Console.WriteLine("{0}: {1}",jointnumber++, j.Data.Name);
                map.Add(jointnumber,j.Data.Name);
            }
            int chosenJoint;
            var jointNames = new List<string>();
            while (int.TryParse(Console.ReadLine(), out chosenJoint))
            {
                jointNames.Add(map[chosenJoint+1]);
                Console.WriteLine("Removing all position data from Joint {0} ", map[chosenJoint+1]);
                Console.WriteLine("More?");
            }

            ConstraintsExamples konstig = new ConstraintsExamples();
            IKSolver[] solvers = { new CCD(), new CCD(), new FABRIK(), new FABRIK(), new DampedLeastSquares(), new DampedLeastSquares(), new JacobianTranspose(), new JacobianTranspose() };
            IKApplier applier = new IKApplier(solvers[0]);
            string[] ikNamse = { "CCD", "CCD Constrained", "FABRIK", "FABRIK Constrained", "DLS", "DLS Constrained", "Transpose", "Transpose Constrained" };
            int i = 0;
            for (int p = 0; p < solvers.Length; p++)
            {
                var sol = solvers[p];
                //bool constriants;
                //IK ikalg;
                applier.IKSolver = sol;// ChooseIKSolver(out constriants, out ikalg);
                var facit = new List<Vector3>();
                var result = new List<Vector3>();
                var diff = new List<float>();
                var time = new List<double>();
                Stopwatch sw = new Stopwatch();
                bool first = true;
                foreach (var skel in GoldenStandard)
                {
                    if (first)
                    { 
                        first = false; 
                        var tmp = skel;
                        applier.ApplyIK(ref tmp);
                        continue; 
                    }
                    BipedSkeleton tempskel = new BipedSkeleton();
                    if (ikNamse[p].EndsWith("Constrained")) konstig.SetConstraints(ref tempskel);
                    IEnumerator tempE = tempskel.GetEnumerator();
                    IEnumerator skelE = skel.GetEnumerator();
                    while (tempE.MoveNext() && skelE.MoveNext())
                    {
                        TreeNode<Bone> skelB = (TreeNode<Bone>)skelE.Current;
                        if (jointNames.Contains(skelB.Data.Name))
                        {
                            ((TreeNode<Bone>)tempE.Current).Data.Pos = new Vector3(float.NaN, float.NaN, float.NaN);
                            ((TreeNode<Bone>)tempE.Current).Data.Orientation = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
                        }
                        else
                        {
                            ((TreeNode<Bone>)tempE.Current).Data.Pos = new Vector3(skelB.Data.Pos);
                            ((TreeNode<Bone>)tempE.Current).Data.Orientation = new Quaternion(new Vector3(skelB.Data.Orientation.Xyz), skelB.Data.Orientation.W);
                        }
                    }
                    GC.Collect();
                    sw.Restart();
                    applier.ApplyIK(ref tempskel);
                    sw.Stop();
                    //foreach (var jointname in jointNames)
                    //{
                    facit.Add(new Vector3(skel[jointNames[0]].Pos));
                    result.Add(new Vector3(tempskel[jointNames[0]].Pos));
                    diff.Add((skel[jointNames[0]].Pos - tempskel[jointNames[0]].Pos).Length);
                    //}
                    time.Add(ElapsedMilliS(sw));
                }
                //for (int i = 0; i < result.Count; i++)
                //{
                //    float dist = (facit[i]-result[i]).Length;
                //    Console.WriteLine("Distance: {0,-30}  Time: {1,-30}", dist, time[i]);
                //}
               //Console.WriteLine("Save data? (y)");
               // if (Console.ReadKey().KeyChar == 'y')
               // {
                string name = ikNamse[i++]
                    + jointNames.Aggregate(" ", (sofar, newone) => sofar + " " + newone);
                    // +today;
                    //Console.WriteLine("append to and of " + name + ".txt");
                    //string appendtoname = Console.ReadLine();
                    //SaveToFile(facit,  name + "_facit_" + appendtoname);
                    //SaveToFile(result, name + "_result_" + appendtoname);
                Console.WriteLine("Saving: \n " +  name);
                SaveToFile(diff, "DIFF " + name );// + appendtoname);
                SaveToFile(result, "RES " + name);
                SaveToFile(facit, "RES True position " + jointNames.Aggregate(" ", (sofar, newone) => sofar + " " + newone));
                SaveToFile(time, "TIME " + name);
                    //SaveToFile(time,   name + "_time_" + appendtoname);
                //}
            }

        }
        static void Connect()
        {
            string[] servers;
            rtClient = RTClient.getInstance();

            while (!rtClient.getServers(out servers))
            {
                Console.Out.WriteLine("Connection error\nIs QTM running?");
                Console.Out.Write("Trying again");
                Thread.Sleep(1000);
                Console.Out.Write(".");
                if (rtClient.getServers(out servers)) break;
                Thread.Sleep(1000);
                Console.Out.Write(".");
                if (rtClient.getServers(out servers)) break;
                Thread.Sleep(1000);
                Console.Out.Write(".");
                if (rtClient.getServers(out servers)) break;
                Console.Clear();
            }
           // Console.Out.WriteLine("Servers found:");
           // for (int i = 0; i < servers.Length; i++) Console.Out.WriteLine(string.Format("{1,-10}", i++, servers[i]));

            Console.Out.WriteLine("Connecting to: " + servers[server]);
            while (!RTClient.getInstance().connect(server, portUDP, streammode, streamFreq, stream6d, stream3d))
            {
                Console.Out.WriteLine("Error connecting to " + servers[server]);
                Console.Out.WriteLine("Is QTM streaming?");
                Console.Out.Write("Trying again");
                Thread.Sleep(1000);
                Console.Out.Write(".");
                if (RTClient.getInstance().connect(server, portUDP, streammode, streamFreq, stream6d, stream3d)) break;
                Thread.Sleep(1000);
                Console.Out.Write(".");
                if (RTClient.getInstance().connect(server, portUDP, streammode, streamFreq, stream6d, stream3d)) break;
                Thread.Sleep(1000);
                Console.Out.Write(".");
                if (RTClient.getInstance().connect(server, portUDP, streammode, streamFreq, stream6d, stream3d)) break;
                Console.Clear();
            }
            Console.Out.WriteLine("Connection to " + servers[server] + " successfull");
        }
        static void Disconnect()
        {
            Console.Out.WriteLine("Disconnecting...");
            RTClient.getInstance().disconnect();
            Console.Out.WriteLine("Disconnected");
        }

        static IKSolver ChooseIKSolver(out bool constraints, out IK ikalg)
        {
            constraints = false;
            Console.Out.WriteLine("Choose IK algorithm:");
            int ikindex = 0;
            foreach (IK ik in Enum.GetValues(typeof(IK)))
            {
                Console.Out.WriteLine(ikindex++ + " " + ik + "");
            }
            int parsed;
            while (!int.TryParse(Console.ReadLine(), out parsed) || parsed > ikindex-1) ;
            
            ikalg = (IK)parsed;
            switch (ikalg)
            {
                case IK.CCD:
                    Console.WriteLine("Use Constraints?\n'' Constraitns\n_ No constraints");
                    constraints = (string.IsNullOrEmpty(Console.ReadLine()));
                    Console.WriteLine(constraints ? "Using constriants" : "Will not use constraints");

                    return new CCD();
                case IK.FABRIK:
                    Console.WriteLine("Use Constraints?\n'' Constraitns\n_ No constraints");
                    Console.WriteLine(constraints ? "Using constriants" : "Will not use constraints");
                    constraints = (string.IsNullOrEmpty(Console.ReadLine()));
                    return new FABRIK();
                case IK.DLS:
                    return  new DampedLeastSquares();
                case IK.TRANSPOSE:
                    return new JacobianTranspose();
                case IK.TT:
                    return new TargetTriangleIK();
                default:
                return null;
            }
        }
        static void RemoveRandomMarkerTest(List<Tuple<int, List<LabeledMarker>>> metaList, List<BipedSkeleton> GoldenStandard)
        {
            int size = metaList.Count;
            int numofgaps = size / 5;
            int maxgaplength = size / 20;
            Console.WriteLine("Creating {0} random gaps with max length of {1}", numofgaps, maxgaplength);
            var gaps = GapMatrix(gaps: numofgaps, gapSizeMax: maxgaplength, numberOfFrames: size, markers: metaList[0].Item2.Count);

            Console.Write(string.Format("Frame "));
            foreach (var n in metaList.First().Item2)
            {
                Console.Write(string.Format("{0,-36}", n.label)); ;
            }
            Console.WriteLine();
            int frameNo = 0;
            foreach (var tpl in metaList)
            {
                StringBuilder sb = new StringBuilder(string.Format("{0,-6}", tpl.Item1));
                int jointNo = 0;
                foreach (var lm in tpl.Item2)
                {
                    if (gaps[frameNo, jointNo])
                    {
                        lm.position = new OpenTK.Vector3(float.NaN, float.NaN, float.NaN);
                    }
                    sb.Append(string.Format("{0,-36}", lm.position));
                    jointNo++;
                }
                Console.WriteLine(sb);
                frameNo++;
            }
            //Console.Clear();
            var ikapplier = new IKApplier(new JacobianTranspose());
            ConstraintsExamples konstig = new ConstraintsExamples();
            bool constraints = true;
            IKSolver[] solvers = { new CCD(), new CCD(), 
                                     new FABRIK(), new FABRIK(), 
                                     new DampedLeastSquares(), new DampedLeastSquares(), 
                                     new JacobianTranspose(), new JacobianTranspose() 
                                 };
            string[] ikNamse = { "CCD", "CCD Constrained", "FABRIK", "FABRIK Constrained", "DLS", "DLS Constrained", "Transpose", "Transpose Constrained" };
            int i = 0;
            for (int p = 0; p < solvers.Length; p++)
            {
                constraints = !constraints;
                #region old ik chooser
                /*
                 * Console.Out.WriteLine(size + " frames in set..");
                Console.Out.WriteLine("Choose IK algorithm:");
                int ikindex = 0;
                foreach (IK ik in Enum.GetValues(typeof(IK)))
                {
                    Console.Out.WriteLine(ikindex++ + " " + ik + "");
                }
                Console.Out.WriteLine(ikindex + " print gapmatrix");
                Console.Out.WriteLine("_ Exit");

                int parsed;
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    continue;
                } else if (!int.TryParse(input, out parsed) || parsed > ikindex)
                {
                    break;
                }
                IK ikalg = (IK)parsed;
                Console.WriteLine("Use Constraints?\n'' Constraitns\n_ No constraints");
                bool constraints = (string.IsNullOrEmpty(Console.ReadLine()));
                switch (ikalg)
                {
                    case IK.CCD:

                        ikapplier.IKSolver = new CCD();
                        break;
                    case IK.FABRIK:
                        ikapplier.IKSolver = new FABRIK();
                        break;
                    case IK.DLS:
                        ikapplier.IKSolver = new DampedLeastSquares();
                        break;
                    case IK.TRANSPOSE:
                        ikapplier.IKSolver = new JacobianTranspose();
                        break;
                    case IK.TT:
                        ikapplier.IKSolver = new TargetTriangleIK();
                        break;
                    default:
                        for (int i = 0; i <= gaps.GetUpperBound(0); i++)
                        {
                            StringBuilder sb = new StringBuilder(string.Format("{0,-4}", i + 1));
                            for (int j = 0; j <= gaps.GetUpperBound(1); j++)
                            {
                                string p = (bool)gaps.GetValue(i, j) ? " " : "#";
                                sb.Append(p);
                            }
                            Console.WriteLine(sb);
                        }
                        Console.WriteLine("{0} random gaps with max length of {1}", size, size / 10);
                        continue;
                }
                 */
                //Console.Clear();
                #endregion
                ikapplier.IKSolver = solvers[p];

                double max = 0;
                int maxFrame = 0;
                var elapsedTimes = new List<double>();
                var result = new List<List<float[]>>();
                var skeleton = new BipedSkeleton();
                var skeleton2 = new BipedSkeleton();
                var konstigt = new ConstraintsExamples();
                var jl = new JointLocalization();
                if (constraints)
                {
                    konstigt.SetConstraints(ref skeleton);
                    konstigt.SetConstraints(ref skeleton2);
                }
                Stopwatch stopWatch = new Stopwatch();
                foreach (var list in metaList)
                {
                    Console.Write("solving frame " + list.Item1);
                    GC.Collect();
                    stopWatch.Restart();
                    BipedSkeleton temp = skeleton;
                    skeleton = skeleton2;
                    skeleton2 = temp;
                    jl.GetJointLocation(list.Item2, ref skeleton);
                    ikapplier.ApplyIK(ref skeleton);
                    stopWatch.Stop();
                    #region debug for NaNs
                    //if (skeleton.Any(x => x.Data.Pos.IsNaN()))
                    //{
                    //    TreeNode<Bone> nanBone = skeleton.First(z => z.Data.Pos.IsNaN());
                    //    Console.WriteLine("ERROR:");
                    //    Console.WriteLine(nanBone.Data);
                    //    Console.ReadLine();
                    //}
                    //if (skeleton.Any(x => x.Data.Orientation.Xyz.IsNaN()))
                    //{
                    //    TreeNode<Bone> nanBone = skeleton.First(z => z.Data.Orientation.Xyz.IsNaN());
                    //    Console.WriteLine("ERROR2;");
                    //    Console.WriteLine(nanBone.Data);
                    //    Console.ReadLine();
                    //}
                    #endregion
                    double duration = ElapsedMilliS(stopWatch);// (1000.0 * (double)stopWatch.ElapsedTicks / Stopwatch.Frequency);
                    elapsedTimes.Add(duration);
                    Console.WriteLine(" in " + duration + "ms");
                    if (duration > max)
                    {
                        max = duration;
                        maxFrame = list.Item1;
                    }
                    //var theres = ToNotSkeleton(skeleton);
                    #region debug for NAN
                    //if (theres.Any(c => c.Any(d => float.IsNaN(d))))
                    //{
                    //    StringBuilder sb = new StringBuilder();
                    //    foreach (TreeNode<Bone> t in skeleton)
                    //    {
                    //        if (t.Data.Pos.IsNaN())
                    //        {
                    //            sb.Append(t.Data.ToString() + "\n");
                    //        }
                    //    }
                    //    sb.Insert(0, string.Format("\nA NaN at frame: {0}\n", list.Item1));
                    //    foreach (var lm in list.Item2)
                    //    {
                    //        if (lm.position.IsNaN())
                    //        {
                    //            sb.Append(string.Format(lm.label + "\n\t Pos: {0}\n", lm.position));
                    //        }
                    //    }
                    //    sb.Append("\n");
                    //    Console.WriteLine(sb);
                    //    Console.ReadLine();
                    //}
                    #endregion
                    result.Add(ToNotSkeleton(skeleton));
                }
                Console.WriteLine("Calculating diffrence to golden...");
                var resDiff = DiffToGolden(result, GoldenStandard, verbose: true);
                string name = ikNamse[p];
                Console.WriteLine("Maximum time for " + name + " was " + max + "ms at frame: " + maxFrame);
                SaveToFile(result, name);
                SaveToFile(resDiff, name);

                Console.WriteLine();
            }
        }

        private static List<List<float>> DiffToGolden(List<List<float[]>> result, List<BipedSkeleton> GoldenStandard, bool verbose = false)
        {
            var gs = GoldenStandard.ConvertAll(x => ToNotSkeleton(x));
            var gsenum = gs.GetEnumerator();
            var resenum = result.GetEnumerator();
            var bsenum = GoldenStandard.GetEnumerator();
            var returnthis = new List<List<float>>();
            float max = 0;
            string maxName = "max";
            int maxFrame = 0;
            int frame = 0;
            if (verbose)
                foreach (TreeNode<Bone> n in GoldenStandard.First())
                {
                    Console.Write(string.Format("{0,-10}", n.Data.Name)); ;
                }
            Console.WriteLine();
            while (gsenum.MoveNext() && resenum.MoveNext() && bsenum.MoveNext())
            {
               
                var listoffloats = new List<float>();
                var gsframeenu = gsenum.Current.GetEnumerator();
                var resultframeenu = resenum.Current.GetEnumerator();
                var bsframeenum = bsenum.Current.GetEnumerator();
                StringBuilder sb = new StringBuilder();
                while (gsframeenu.MoveNext() && resultframeenu.MoveNext() && bsframeenum.MoveNext())
                {
                    float[] resa = (float[])resultframeenu.Current;
                    Vector3 resV = new Vector3(resa[0], resa[1], resa[2]);
                    float[] gsa = (float[])gsframeenu.Current;
                    Vector3 gsV = new Vector3(gsa[0], gsa[1], gsa[2]);
                    float dist = (gsV - resV).Length;
                   
                    if (dist > max)
                    {
                        maxFrame = frame;
                        max = dist;
                        maxName = ((TreeNode<Bone>)bsframeenum.Current).Data.Name;
                    }
                    if (verbose) sb.Append(string.Format ("{0,-10}",Math.Round(dist,6)));

                    listoffloats.Add(dist);
                }
                if (verbose) Console.WriteLine(sb);
                returnthis.Add(listoffloats);
                frame++;
            }
            if (verbose)
            {
                Console.WriteLine("\n" + maxName + " was at distance from golden: " + max + " at frame: " + maxFrame);
                StringBuilder sb2 = new StringBuilder();
                foreach (var some in returnthis[maxFrame])
                {
                    sb2.Append(string.Format("{0,-10}", Math.Round(some, 6)));
                }
                Console.WriteLine(sb2);
            }
            return returnthis;
        }
        private static void SaveToFile(List<BipedSkeleton> skel, string name)
        {
            var tofile = new List<List<float[]>>();
            //var tofile = new List<List<Tuple<float[], float[]>>>();
            foreach (var frame in skel)
            {
                tofile.Add(ToNotSkeleton(frame));
            }
            SaveToFile(tofile, name);
        }
        private static void SaveToFile(List<Vector3> facit, List<Vector3> res, string name)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name + "\n");
            var zip = facit.Zip(res, (fa,re) => fa.X + "," + fa.Y + "," + fa.Z + "," + re.X + "," + re.Y + "," + re.Z);
            foreach (var v in zip)
            {
                sb.Append(v);
                sb.Append("\n");
            }
            System.IO.File.WriteAllText(gpath + name + ".txt", sb.ToString());
        }
        private static void SaveToFile(List<Vector3> tofile, string name)
        {
            StringBuilder sb = new StringBuilder(name + "\n");
            foreach (var v in tofile)
            {
                sb.Append(v.X);
                sb.Append(',');
                sb.Append(v.Y);
                sb.Append(',');
                sb.Append(v.Z);
                sb.Append('\n');
            }
            System.IO.File.WriteAllText(gpath + name + ".txt", sb.ToString());
            //SaveToFile((Object)tofile, name);
        
        }
        private static void SaveToFile(List<float> tofile, string name)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(name+"\n");
            foreach (var v in tofile)
            {
                sb.Append(v);
                sb.Append('\n');
            }
            System.IO.File.WriteAllText(gpath + name + ".txt", sb.ToString());
            //SaveToFile((Object)tofile, name);
        }

        private static void SaveToFile(Object tofile, string name)
        {
            string json = JsonConvert.SerializeObject(tofile);
            System.IO.File.WriteAllText(gpath + name + ".txt", json);
        }
        private static List<float[]> ToNotSkeleton(BipedSkeleton skel)
        {
                var tempList = new List<float[]>();
                foreach (TreeNode<Bone> joint in skel)
                {
                    var p = joint.Data.Pos;
                    var q = joint.Data.Orientation;
                    float[] pos = new float[] { p.X, p.Y, p.Z };
                    float[] rot = new float[] { q.X, q.Y, q.Z, q.W };
                    tempList.Add(pos);
                }
                return tempList;
        }

        private static bool[,] GapMatrix
            (
            int numberOfFrames = 4000,
            int gaps = 200,
            int gapSizeMin = 5,
            int gapSizeMax = 100,
            int markers = 35, 
            bool verbose = false,
            bool writeToFile = false
            )
        {
            Random rnd = new Random();
            bool[,] array = new bool[numberOfFrames, markers];
            for (int i = 0; i < gaps; i++)
            {
                int removeAroundFrame = rnd.Next(1, numberOfFrames);
                int numberOfRemovedFrames = rnd.Next(gapSizeMin, gapSizeMax);
                int markerToRemove = rnd.Next(0, markers);
                int to = removeAroundFrame + numberOfRemovedFrames;
                to = to > numberOfFrames ? numberOfFrames : to;
                if (verbose)
                {
                    Console.WriteLine(
                        string.Format(
                            "{0,-3} - Marker {1,-2} removed from frame {2,-4} to {3,-4}",
                            i, markerToRemove, removeAroundFrame, to));
                }
                for (int frame = removeAroundFrame; frame < to; frame++)
                {
                    array[frame, markerToRemove] = true;
                }
            }
            if (writeToFile)
            {
                string json = JsonConvert.SerializeObject(array);
                System.IO.File.WriteAllText(gpath + "gaps.txt", json);
            }
            if (verbose)
            {
                for (int i = 0; i <= array.GetUpperBound(0); i++)
                {
                    StringBuilder sb = new StringBuilder(string.Format("{0,-5}", i + 1));
                    for (int j = 0; j <= array.GetUpperBound(1); j++)
                    {
                        sb.Append(string.Format("{0,-6}", array.GetValue(i, j)));
                    }
                    Console.WriteLine(sb);
                }
            }
            return array;
        }

        static List<Tuple<int, List<LabeledMarker>>> GetCompleteSetOfMarkers(int intervall = 16, bool verbose = true)
        {
            var metaList = new List<Tuple<int, List<LabeledMarker>>>(500);
            int frame = rtClient.getFrame();
            while (frame < 1 || frame > 12 )
            {
                if (verbose)
                {
                    Console.WriteLine("Waiting for trail to restart... " + frame);
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                }
                frame = rtClient.getFrame();
            }
            Console.WriteLine();
            Console.WriteLine("Adding frames to test suit.");
            int lastframe = -2;
            for (
                int thisframe = rtClient.getFrame();
                thisframe >= lastframe;
                thisframe = rtClient.getFrame()
                )
            {
                if (thisframe > lastframe)
                {
                    var things = rtClient.Markers;
                    var listthing = new List<LabeledMarker>();
                    foreach (var lm in things)
                    {
                        var lama = new LabeledMarker();
                        lama.position = new Vector3(lm.position);
                        lama.label = lm.label;
                        listthing.Add(lama);
                    }
                    if (verbose)
                    {
                        Console.WriteLine("Adding frame " + thisframe + " to test suit");
                    }
                    metaList.Add(new Tuple<int, List<LabeledMarker>>(thisframe, listthing));
                    lastframe = thisframe;
                }
                else
                {
                    Thread.Sleep(1);
                    continue;
                }
                Thread.Sleep(intervall);
            }
            if (verbose)
            {
                Console.WriteLine(metaList.Count + " frames saved in test suit.");
            }
            return metaList;
        }
    }
}