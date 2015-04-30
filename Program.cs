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
        static int streamFreq = 60;// 30;
        static int streammode = 1;
        static int server = 0;
        static string connectionStatus = "Not Connected";

        static bool connected = false;
        static bool stream6d = false;
        static bool stream3d = true;

        static void Connect()
        {
            string[] servers;
            rtClient = RTClient.getInstance();

            while (!rtClient.getServers(out servers))
            {
                Console.Clear();
                Console.Out.WriteLine("Connection error\nIs QTM running?");
                Console.Out.Write("Trying again");
                Thread.Sleep(1000);
                Console.Out.Write(".");
                Thread.Sleep(1000);
                Console.Out.Write(".");
                Thread.Sleep(1000);
                Console.Out.WriteLine(".");
            }
            Console.Clear();
            Console.Out.WriteLine("Servers found:");
            for (int i = 0; i < servers.Length; i++) Console.Out.WriteLine(string.Format("{1,-10}", i++, servers[i]));

            while (!connected)
            {
                //Console.Out.WriteLine("Choose server:");
                //for (int i = 0; i < servers.Length; i++) Console.Out.WriteLine(server++ + ":\t" + servers[i]);
                //server = int.Parse(Console.ReadLine());
                Console.Clear();
                Console.Out.WriteLine("Connecting to: " + servers[server]);
                connected = RTClient.getInstance().connect(server, portUDP, streammode, streamFreq, stream6d, stream3d, out connectionStatus);
                if (!connected)
                {
                    Console.Out.WriteLine("Error connecting to " + servers[server]);
                    Console.Out.WriteLine("Is QTM streaming?");
                    Console.Out.Write("Trying again");
                    Thread.Sleep(1000);
                    Console.Out.Write(".");
                    Thread.Sleep(1000);
                    Console.Out.Write(".");
                    Thread.Sleep(1000);
                    Console.Out.WriteLine(".");
                }
            }
            Console.Out.WriteLine("Connection to " + servers[server] + " successfull");
        }
        static void Disconnect()
        {
            RTClient.getInstance().disconnect();
            connected = false;
            connectionStatus = "Disconnected";
        }
        public static void Main(string[] args)
        {
            Connect();
            JointLocalization jl = new JointLocalization();
            IKApplier ikapplier = new IKApplier();
            ikapplier.IKSolver = new JacobianTranspose();
            BipedSkeleton skeleton = new BipedSkeleton();


            new ConstraintsExamples().SetConstraints(ref skeleton);
            int intervall = Math.Max(1, (int)Math.Ceiling((decimal)1000 / streamFreq));
            var metaList = GetCompleteSetOfMarkers(verbose: true);
            List<BipedSkeleton> GoldenStandard = new List<BipedSkeleton>();

            Console.WriteLine("Getting golden standard...");
            foreach (var list in metaList)
            {
                var tempSkel = new BipedSkeleton();
                jl.GetJointLocation(list.Item2, ref tempSkel);
                GoldenStandard.Add(tempSkel);
            }
            Console.WriteLine("Saving golden standard to file");

            SaveToFile(GoldenStandard, "GoldenStandard");

            Console.WriteLine("Creating random gaps...");
            var gaps = GapMatrix(gaps:1000,gapSizeMax:200);
            int frameNo = 0;
            foreach (var tpl in metaList)
            {
                //StringBuilder sb = new StringBuilder();
                int jointNo = 0;
                foreach (var lm in tpl.Item2)
                {
                    if (gaps[frameNo, jointNo])
                    {
                        //sb.Append(string.Format("Creating gap in frame {0} for joint {1}\n", frameNo, lm.label));
                        lm.position = new OpenTK.Vector3(float.NaN, float.NaN, float.NaN);
                    }
                    jointNo++;
                }
                //Console.WriteLine(sb);
                frameNo++;
            }
            Console.Clear();
            do
            {
                Console.Out.WriteLine("Choose IK algorithm:");
                int ikindex = 0;
                foreach (IK ik in Enum.GetValues(typeof(IK)))
                {
                    Console.Out.WriteLine(ikindex++ + " " + ik + "");
                }
                Console.Out.WriteLine("_ Exit");

                int parsed;
                if (!int.TryParse(Console.ReadLine(),out parsed) || parsed > ikindex-1)
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
                        break;
                }
                Console.Clear();
                long max = 0;
                var result = new List<List<Tuple<float[], float[]>>>();
                skeleton = new BipedSkeleton();
                new ConstraintsExamples().SetConstraints(ref skeleton);
                
                foreach (var list in metaList)
                {
                    GC.Collect();
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    jl.GetJointLocation(rtClient.Markers, ref skeleton);
                    ikapplier.ApplyIK(ref skeleton);
                    stopWatch.Stop();
                    long duration = stopWatch.ElapsedMilliseconds;
                    if (duration > max) max = duration;
                    result.Add(ToNotSkeleton(skeleton));
                }
                Console.WriteLine("Maximum time for " + ikalg + " was " + max + "ms");
                Console.WriteLine();
                Console.WriteLine("Saving result to file...");
                SaveToFile(result, ikalg.ToString());
                Console.WriteLine();

            } while (true);
            

            Console.Out.WriteLine("Disconnecting...");
            Disconnect();
            Console.Out.WriteLine(connectionStatus);
        }
        private static void SaveToFile(List<BipedSkeleton> skel, string name, string path = @"C:\Users\Jonas\Desktop\")
        {
            var tofile = new List<List<Tuple<float[], float[]>>>();
            foreach (var frame in skel)
            {
                tofile.Add(ToNotSkeleton(frame));
            }
            SaveToFile(tofile, name, path);
        }
        private static void SaveToFile(List<List<Tuple<float[], float[]>>> tofile, string name, string path = @"C:\Users\Jonas\Desktop\")
        {
            string json = JsonConvert.SerializeObject(tofile);
            System.IO.File.WriteAllText(path + name + ".txt", json);
        }
        private static List<Tuple<float[], float[]>> ToNotSkeleton(BipedSkeleton skel)
        {
                var tempList = new List<Tuple<float[], float[]>>();
                foreach (TreeNode<Bone> joint in skel)
                {
                    var p = joint.Data.Pos;
                    var q = joint.Data.Orientation;
                    float[] pos = new float[] { p.X, p.Y, p.Z };
                    float[] rot = new float[] { q.X, q.Y, q.Z, q.W };
                    tempList.Add(new Tuple<float[], float[]>(pos, rot));
                }
                return tempList;
        }

        private static bool[,] GapMatrix
            (
            bool verbose = false
            ,
            int numberOfFrames = 4000
            ,
            int gapSizeMin = 5
            ,
            int gapSizeMax = 100
            ,
            int gaps = 200
            ,
            int markers = 35
            , 
            bool writeToFile = false
            ,
            string path = @"C:\Users\Jonas\Desktop\test.txt"
            )
        {
            Random rnd = new Random();
            bool[,] array = new bool[numberOfFrames, markers];
            for (int i = 0; i < gaps; i++)
            {
                int removeAroundFrame = rnd.Next(1, numberOfFrames - gapSizeMax);
                int numberOfRemovedFrames = rnd.Next(gapSizeMin, gapSizeMax);
                int markerToRemove = rnd.Next(0, markers);
                int to = removeAroundFrame + numberOfRemovedFrames;
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
                System.IO.File.WriteAllText(path, json);
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
            int intervall = 60
            ,
            bool verbose = true
            )
        {
            var metaList = new List<Tuple<int, List<LabeledMarker>>>(500);
            int frame = rtClient.getFrame();
            while (frame == 0 || frame > 24)
            {
                if (verbose)
                {
                    Console.Clear();
                    Console.WriteLine("Waiting for trail to restart... " + frame);
                }
                Thread.Sleep(intervall);
                frame = rtClient.getFrame();
            }
            int lastframe = -2;
            for (
                int thisframe = rtClient.getFrame();
                thisframe >= lastframe;
                thisframe = rtClient.getFrame()
                )
            {
                if (thisframe > lastframe)
                {
                    if (verbose)
                    {
                        Console.Clear();
                        Console.WriteLine("Adding frame " + thisframe + " to test suit");
                    }
                    metaList.Add(new Tuple<int, List<LabeledMarker>>(thisframe, rtClient.Markers));
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