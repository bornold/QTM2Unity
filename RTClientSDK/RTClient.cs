using System;
using System.Collections.Generic;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Settings;
using OpenTK;
using System.Linq;

namespace QTM2Unity
{
	public class RTClient
	{
		RTProtocol mProtocol;
		private static RTClient mInstance;

		private List<sixDOFBody> mBodies;
		public List<sixDOFBody> Bodies { get { return mBodies; } }

        private List<string> markerNames;
        private Dictionary<string,Vector3> mMarkers;
        private Dictionary<string, Vector3> mMarkersBuffer;
        private volatile bool dataFetched = false;
        public Dictionary<string, Vector3> Markers
        { 
            get
            {
                dataFetched = true;
                return mMarkers;
            } 
        }

        private List<MarkerBone> mBones;
		public List<MarkerBone> Bones { get { return mBones; } }

		private eAxis mUpAxis;
		private Quaternion mCoordinateSystemChange;
        private RTPacket mPacket;
        private bool mStreamingStatus;


		// processor of realtime data
		// Function is called everytime protocol receives a datapacket from server
		public void process(RTPacket packet)
		{
            mPacket = packet;
            #region ...
            //List<s6DOF> bodyData = packet.get6DOFData();
            //if(bodyData != null)
            //{
            //    for(int i = 0; i < bodyData.Count; i++)
            //    {
            //        Vector3 position = new Vector3(bodyData[i].position.x,
            //                                       bodyData[i].position.y,
            //                                       bodyData[i].position.z);

            //        //Set rotation and position to work with unity
            //        position /= 1000;

            //        mBodies[i].position = Vector3.Transform(position, mCoordinateSystemChange);//QuaternionHelper.Rotate(mCoordinateSystemChange, position );
            //        mBodies[i].position.Z *= -1;
					
            //        mBodies[i].rotation = mCoordinateSystemChange * QuaternionHelper.FromMatrix(bodyData[i].matrix);
            //        mBodies[i].rotation.Z *= -1;
            //        mBodies[i].rotation.W *= -1;

            //        mBodies[i].rotation *= QuaternionHelper.RotationZ(Mathf.PI * .5f);
            //        mBodies[i].rotation *= QuaternionHelper.RotationX(-Mathf.PI * .5f);

            //    }
            //}
            #endregion

            List<s3D> markerData = packet.get3DMarkerData();
			//Get marker data that is labeled and update values
            if(markerData != null)
            {
                if (dataFetched)
                {
                    var tmp = mMarkers;
                    mMarkers = mMarkersBuffer;
                    mMarkersBuffer = tmp;
                    dataFetched = false;
                }
                var it = markerNames.GetEnumerator();
                foreach (var md in markerData)
                {
                    it.MoveNext();
                    Vector3 position = new Vector3(md.position.x,
                                                    md.position.y,
                                                    md.position.z);
                    position /= 1000;
                    position = Vector3.Transform(position, mCoordinateSystemChange);
                    position.Z *= -1;
                    string key = it.Current;
                    if (mMarkers.ContainsKey(key))
                    {
                        mMarkers[key] = position;
                    }
                    else
                    {
                        mMarkers.Add(key, position);
                    }
                }
                if (dataFetched)
                {
                    dataFetched = false;
                }
            }
		}

        // called everytime a event is broadcasted from QTM server.d
		public void events(RTPacket packet)
		{
            eEvent currentEvent =  packet.getEvent();
			//Debug.Log("Event occured! : " + currentEvent);

            if (currentEvent == eEvent.kEventRTFromFileStarted)
            {
                // reload settings when we start streaming to get proper settings
                get3DSettings();
                get6DOFSettings();
            }
		}

        // get frame from latest packet
        public int getFrame()
        {
            return mPacket.Frame;
        }

        // Constructor
        private RTClient()
		{
			//New instance of protocol, contains a RT packet
			mProtocol = new RTProtocol();
			//list of bodies that server streams
			mBodies = new List<sixDOFBody>();
			//list of markers
            markerNames = new List<string>();
            mMarkers = new Dictionary<string, Vector3>();
            mMarkersBuffer = new Dictionary<string, Vector3>();
			//list of bones
			mBones = new List<MarkerBone>();

            mStreamingStatus = false;

            mPacket = RTPacket.ErrorPacket;
		}

        public static RTClient getInstance()
		{
			//Singleton method since we only want one instance (one connection to server)
			if(mInstance == null)
			{
                mInstance = new RTClient();
			}
			return mInstance;
		}

		//Method for objects to call to get data from body
		public sixDOFBody getBody(string name)
		{
			if (string.IsNullOrEmpty(name))
				return null;
			if(mBodies.Count > 0)
			{
				foreach(sixDOFBody body in mBodies)
				{
					if(body.name == name)
					{
						return body;
					}
				}
			}
			return null;
		}

        /// <summary>
        /// Get list of servers available on network
        /// </summary>
        /// <returns><c>true</c>, if discovery packet was sent, <c>false</c> otherwise.</returns>
        /// <param name="list">List of discovered servers</param>
        public bool getServers(out String[] list)
        {
            //Send discovery packet
            if (mProtocol.discoverRTServers(1337))
            {
                if (mProtocol.DiscoveryResponses.Count > 0)
                {
                    //Get list of all servers from protocol
                    list = new String[mProtocol.DiscoveryResponses.Count];
                    for (int i = 0; i < mProtocol.DiscoveryResponses.Count; i++)
                    {
                        //add them to our list for user to pick from
                        list[i] = mProtocol.DiscoveryResponses[i].hostname + " (" + mProtocol.DiscoveryResponses[i].ipAddress + ")";
                    }
                    return true;
                }
            }
            list = null;
            return false;
        }


        public bool _connected;
        public int _pickedServer;
        public short _udpPort;
        public int _streammode;
        public int _streamval;
        public bool _stream6d;
        public bool _stream3d;
		/// <summary>
		/// Connect the specified pickedServer.
		/// </summary>
		/// <param name="pickedServer">Picked server.</param>
		/// <param name="udpPort">UDP port streaming should occur on.</param>
		/// <param name="streammode">how data should be streamed.</param>
        /// <param name="streamval">if not stream all frames is picked, this holds frequency or frequency divisor.</param>
        /// <param name="stream6d"> if 6 DOF data should be streamed.</param>
        /// <param name="stream3d"> if labeled markers should be streamed.</param>
		public bool connect(int pickedServer, short udpPort, int streammode, int streamval, bool stream6d, bool stream3d)
		{
            sDiscoveryResponse server;
            try
            {
			    server =  mProtocol.DiscoveryResponses[pickedServer];
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
			if(mProtocol.connect(server, udpPort))
			{
                if (connectStream(udpPort, streammode, streamval, stream6d, stream3d))
                {
                    _connected = true;
                    _pickedServer = pickedServer;
                    _udpPort= udpPort;
                    _streammode = streammode;
                    _streamval = streamval;
                    _stream6d = stream6d;
                    _stream3d = stream3d;
                    return true;
                }
			}
			return false;
		}

        // streaming status of client
        public bool getStreamingStatus()
        {
            return mStreamingStatus;
        }

        // Disconnect from server
		public void disconnect()
		{
            mStreamingStatus = false;
			mProtocol.streamFramesStop(); 
			mProtocol.stopStreamListen();
			mProtocol.disconnect();
		}

        private bool get6DOFSettings()
        {
            //get settings and information for streamed bodies
            bool getstatus = mProtocol.get6DSettings();

            if (getstatus)
            {
                mBodies.Clear();
                Settings6D settings = mProtocol.Settings6DOF;
                foreach (sSettings6DOF body in settings.bodies)
                {
                    sixDOFBody newbody = new sixDOFBody();
                    newbody.name = body.name;
                    newbody.position = Vector3.Zero;
                    newbody.rotation = Quaternion.Identity;
                    mBodies.Add(newbody);
                }
                return true;
            }
            return false;
        }

        private bool get3DSettings()
        {
            bool getstatus = mProtocol.get3Dsettings();
            if (getstatus)
            {
                mUpAxis = mProtocol.Settings3D.axisUpwards;

                Rotation.ECoordinateAxes xAxis, yAxis, zAxis;
                Rotation.GetCalibrationAxesOrder(mUpAxis, out xAxis, out yAxis, out zAxis);

                mCoordinateSystemChange = Rotation.GetAxesOrderRotation(xAxis, yAxis, zAxis);

                // Save marker settings
                markerNames.Clear();
                mMarkers.Clear();
                mMarkersBuffer.Clear();
				foreach (sSettings3DLabel marker in mProtocol.Settings3D.labels3D)
                {
                    markerNames.Add(marker.name);
                }

				// Save bone settings
                if (mProtocol.Settings3D.bones != null)
		        {
	                Bones.Clear();

	                //Save bone settings
	                foreach (var settingsBone in mProtocol.Settings3D.bones)
	                {
	                    MarkerBone bone = new MarkerBone();
                        bone.from = settingsBone.from;
                        bone.to = settingsBone.to;
	                    Bones.Add(bone);
	                }
                    
		        }

                return true;
            }
            return false;
        }

		public bool connectStream(short udpPort, int streamMode, int streamVal, bool stream6d, bool stream3d)
		{
			streamMode++; // add one to stream mode for correct typecast

			List<eComponentType> streamedTypes = new List<eComponentType>();
			if(stream3d)
				streamedTypes.Add(eComponentType.kComponent3d);
			if(stream6d)
				streamedTypes.Add(eComponentType.kComponent6d);

            //Start streaming and get the settings
			if (mProtocol.streamFrames((eStreamRate)streamMode, streamVal, false, streamedTypes, udpPort))
			{
                if (stream3d)
                {
                    if(!get3DSettings())
                    {
                        return false;
                    }
                }

                if (stream6d)
                {
                    if (!get6DOFSettings())
                    {
                        return false;
                    }
                }

                // we register our function "process" as a callback for when protocol receives real time data packets
                // (eventDataCallback is also available to listen to events)
                mProtocol.realTimeDataCallback += process;
                mProtocol.eventDataCallback += events;

                //Tell protocol to start listening to real time data
                if (mProtocol.listenToStream())
                {
                    mStreamingStatus = true;
                    return true;
                }
			}
			return false;
		}
	}

    // Class for 6DOF with unity datatypes
	public class sixDOFBody
	{
		public sixDOFBody() {}
		public string name;
		public Vector3 position;
		public Quaternion rotation;
	}

    // Class for bones
	public class MarkerBone
	{
        public string from;
        public string to;
	}

}
