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
namespace TestForQTM2UnityProject
{
    class Program
    {
        static RTClient rtClient;

        static short portUDP = 4545;
        static int streamFreq = 100;// 30;
        static int streammode = 1;
        static int server = 0;
        static string gpath = @"C:\Users\Jonas\Desktop\result\";
        static bool stream6d = false;
        static bool stream3d = true;

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
            Console.Out.WriteLine("Servers found:");
            for (int i = 0; i < servers.Length; i++) Console.Out.WriteLine(string.Format("{1,-10}", i++, servers[i]));

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

        public static void Main(string[] args)
        {
            Connect();
            int intervall = Math.Max(1, (int)Math.Ceiling((decimal)1000 / streamFreq));
            var metaList = GetCompleteSetOfMarkers(verbose: true, intervall:intervall);
            Disconnect();

            var jl = new JointLocalization();

            var skeleton = new BipedSkeleton();
            var konstigt = new ConstraintsExamples();
            konstigt.SetConstraints(ref skeleton);
            var GoldenStandard = new List<BipedSkeleton>();
            Console.WriteLine("Getting golden standard...");
            foreach (var list in metaList)
            {
                var tempSkel = new BipedSkeleton();
                jl.GetJointLocation(list.Item2, ref tempSkel);
                GoldenStandard.Add(tempSkel);
            }
            Console.WriteLine("Saving golden standard to file");

            SaveToFile(GoldenStandard, "GoldenStandard");

            int size = metaList.Count;
            Console.WriteLine("Creating {0} random gaps with max length of {1}", size / 10, size / 20);
            var gaps = GapMatrix(gaps:size/10, gapSizeMax:size/20, numberOfFrames:size, markers:metaList.First().Item2.Count);
            int frameNo = 0;
            Console.Write(string.Format("Frame "));
            foreach (var n in metaList.First().Item2)
            {
                Console.Write(string.Format("{0,-36}", n.label)); ;
            }
            Console.WriteLine();
            foreach (var tpl in metaList)
            {
                StringBuilder sb = new StringBuilder(string.Format("{0,-6}", tpl.Item1));
                int jointNo = 0;
                foreach (var lm in tpl.Item2)
                {
                    if (gaps[frameNo, jointNo])
                    {
                        //sb.Append(string.Format("Creating gap in frame {0} for joint {1}\n", frameNo, lm.label));
                        lm.position = new OpenTK.Vector3(float.NaN, float.NaN, float.NaN);
                    }
                    sb.Append(string.Format("{0,-36}",lm.position));
                    jointNo++;
                }
                Console.WriteLine(sb);
                frameNo++;
            }
            //Console.Clear();
            var ikapplier = new IKApplier();
            ikapplier.IKSolver = new JacobianTranspose();
            do
            {
                Console.Out.WriteLine(size + " frames in set..");
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
                }
                if (!int.TryParse(input,out parsed) || parsed > ikindex)
                {
                    break;
                }
                IK ikalg = (IK)parsed;
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
                        //Console.Clear();
                        for (int i = 0; i <= gaps.GetUpperBound(0); i++)
                        {
                            StringBuilder sb = new StringBuilder(string.Format("{0,-4}", i + 1));
                            for (int j = 0; j <= gaps.GetUpperBound(1); j++)
                            {
                                string p = (bool)gaps.GetValue(i, j) ? " " : "#";
                                sb.Append(p);//string.Format("{0,-6}", p));
                            }
                            Console.WriteLine(sb);

                        }
                        Console.WriteLine("{0} random gaps with max length of {1}", size, size / 10);
                        continue;
                }
                //Console.Clear();
                long max = 0;
                int maxFrame = 0;
                var result = new List<List<float[]>>();
                //var result = new List<List<Tuple<float[], float[]>>>();

                skeleton = new BipedSkeleton();
                var skeleton2 = new BipedSkeleton();

                konstigt.SetConstraints(ref skeleton);
                konstigt.SetConstraints(ref skeleton2);
                foreach (var list in metaList)
                {
                    Console.Write("solving frame " + list.Item1);
                    GC.Collect();
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    BipedSkeleton temp = skeleton;
                    skeleton = skeleton2;
                    skeleton2 = temp;
                    jl.GetJointLocation(list.Item2, ref skeleton);
                    ikapplier.ApplyIK(ref skeleton);
                    stopWatch.Stop();
                    long duration = stopWatch.ElapsedMilliseconds;
                    Console.WriteLine(" in " + duration + "ms");
                    if (duration > max)
                    {
                        max = duration;
                        maxFrame = list.Item1;
                    }
                    var theres = ToNotSkeleton(skeleton);
                    if (theres.Any(c => c.Any(d => float.IsNaN(d))) )
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (TreeNode<Bone> t in skeleton)
                        {
                            if (t.Data.Pos.IsNaN())
                            {
                                sb.Append(t.Data.ToString() + "\n");
                            }
                        }
                        sb.Insert(0, string.Format("\nA NaN at frame: {0}\n",list.Item1));
                        foreach (var lm in list.Item2)
                        {
                            if (lm.position.IsNaN())
                            {
                                sb.Append(string.Format(lm.label + "\n\t Pos: {0}\n", lm.position));
                            }
                        }
                        sb.Append("\n");
                        Console.WriteLine(sb);
                        Console.ReadLine();
                    }
                    result.Add(theres);
                }
                Console.WriteLine("Saving result to file...");
                Console.WriteLine("Calculating diffrence to golden...");
                var resDiff = DiffToGolden(result, GoldenStandard);
                Console.WriteLine("Maximum time for " + ikalg + " was " + max + "ms at frame: " + maxFrame);
                Console.WriteLine("Save data? (y)");
                if (Console.ReadKey().KeyChar == 'y')
                {
                    SaveToFile(result, ikalg.ToString());
                    SaveToFile(resDiff, ikalg.ToString() + "_res");
                }
                Console.WriteLine();
            } while (true);
        }

        private static List<List<float>> DiffToGolden(List<List<float[]>> result, List<BipedSkeleton> GoldenStandard)
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
                    sb.Append(string.Format ("{0,-10}",Math.Round(dist,6)));

                    listoffloats.Add(dist);
                }
                Console.WriteLine(sb);
                returnthis.Add(listoffloats);
                frame++;
            }
            Console.WriteLine("\n" + maxName + " was at distance from golden: " + max + " at frame: " + maxFrame);
            StringBuilder sb2 = new StringBuilder();
            foreach (var some in returnthis[maxFrame])
            {
                sb2.Append(string.Format("{0,-10}", Math.Round(some, 6)));
            }
            Console.WriteLine(sb2);
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
        private static void SaveToFile(List<List<float[]>> tofile, string name)
        {
            string json = JsonConvert.SerializeObject(tofile);
            System.IO.File.WriteAllText(gpath + name + ".txt", json);
        }
        private static void SaveToFile(List<List<float>> tofile, string name)
        {
            string json = JsonConvert.SerializeObject(tofile);
            System.IO.File.WriteAllText(gpath + name + ".txt", json);
        }
        private static List<float[]> ToNotSkeleton(BipedSkeleton skel)
            //private static List<Tuple<float[], float[]>> ToNotSkeleton(BipedSkeleton skel)
        {
                var tempList = new List<float[]>();
                //var tempList = new List<Tuple<float[], float[]>>();

                foreach (TreeNode<Bone> joint in skel)
                {
                    var p = joint.Data.Pos;
                    var q = joint.Data.Orientation;
                    float[] pos = new float[] { p.X, p.Y, p.Z };
                    float[] rot = new float[] { q.X, q.Y, q.Z, q.W };
                    tempList.Add(pos);
                    //tempList.Add(new Tuple<float[], float[]>(pos, rot));
                }
                return tempList;
        }

        private static bool[,] GapMatrix
            (
            int numberOfFrames = 4000
            ,
            int gaps = 200
            ,
            int gapSizeMin = 5
            ,
            int gapSizeMax = 100
            ,
            int markers = 35
            , 
            bool verbose = false
            ,
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

        static List<Tuple<int, List<LabeledMarker>>> GetCompleteSetOfMarkers(
            int intervall = 16
            ,
            bool verbose = true
            )
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
                    var thing = rtClient.Markers;
                    var listthing = new List<LabeledMarker>();
                    foreach (var lm in thing)
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
                Thread.Sleep(intervall);
            }


            if (verbose)
            {
                Console.WriteLine(metaList.Count + " frames saved in test suit.");
            }

            return metaList;//.ConvertAll<List<LabeledMarker>>(tpl => tpl.Item2); 
        }
    }
}