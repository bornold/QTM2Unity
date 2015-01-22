using Debug = UnityEngine.Debug;
using GUIContent = UnityEngine.GUIContent;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Settings;
using OpenTK;

namespace QTM2Unity.Unity
{
	public class RTClient
	{
		RTProtocol mProtocol;
		private static RTClient mInstance;

		private List<sixDOFBody> mBodies;
		public List<sixDOFBody> Bodies { get { return mBodies; } }

		private List<LabeledMarker> mMarkers;
		public List<LabeledMarker> Markers { get { return mMarkers; } }

		private List<Bone> mBones;
		public List<Bone> Bones { get { return mBones; } }

		private eAxis mUpAxis;
		private Quaternion mCoordinateSystemChange;
        private RTPacket mPacket;
        private bool mStreamingStatus;


		// processor of realtime data
		// Function is called everytime protocol receives a datapacket from server
		public void process(RTPacket packet)
		{
            mPacket = packet;
			
            List<s6DOF> bodyData = packet.get6DOFData();
			List<s3D> markerData = packet.get3DMarkerData();

            if(bodyData != null)
			{
				for(int i = 0; i < bodyData.Count; i++)
				{
					Vector3 position = new Vector3(bodyData[i].position.x,
					                               bodyData[i].position.y,
					                               bodyData[i].position.z);

                    //Set rotation and position to work with unity
                    position /= 1000;

					mBodies[i].position = QuaternionHelper.Rotate(mCoordinateSystemChange, position );
                    mBodies[i].position.Z *= -1;
					
                    mBodies[i].rotation = mCoordinateSystemChange * QuaternionHelper.FromMatrix(bodyData[i].matrix);
                    mBodies[i].rotation.Z *= -1;
                    mBodies[i].rotation.W *= -1;

                    mBodies[i].rotation *= QuaternionHelper.RotationZ(Mathf.PI * .5f);
                    mBodies[i].rotation *= QuaternionHelper.RotationX(-Mathf.PI * .5f);

				}
			}

			//Get marker data that is labeled and update values
			if(markerData != null)
			{
				for(int i = 0; i < markerData.Count; i++)
				{
					s3D marker = markerData[i];
					Vector3 position = new Vector3 (marker.position.x,
					                                marker.position.y,
					                                marker.position.z);

					position /= 1000;

					mMarkers[i].position = QuaternionHelper.Rotate(mCoordinateSystemChange, position );
					mMarkers[i].position.Z *= -1;

				}
			}
		}

        // called everytime a event is broadcasted from QTM server.
		public void events(RTPacket packet)
		{
            eEvent currentEvent =  packet.getEvent();
			Debug.Log("Event occured! : " + currentEvent);

            if (currentEvent == eEvent.kEventRTFromFileStarted)
            {
                // reload settings when we start streaming to get proper settings
                Debug.Log("Reloading Settings");

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
			mMarkers = new List<LabeledMarker>();
			//list of bones
			mBones = new List<Bone>();

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

        // Get marker data from streamed data
		public LabeledMarker getMarker(string name)
		{
			if(mMarkers.Count > 0)
			{
				foreach(LabeledMarker marker in mMarkers)
				{
					if(marker.label == name)
					{
						return marker;
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
		public bool getServers(out GUIContent[] list)
		{
			//Send discovery packet
			if( mProtocol.discoverRTServers(1337))
			{
				if(mProtocol.DiscoveryResponses.Count > 0)
				{
					//Get list of all servers from protocol
					list = new GUIContent[mProtocol.DiscoveryResponses.Count];
					for(int i = 0; i < mProtocol.DiscoveryResponses.Count; i++)
					{
						//add them to our list for user to pick from
						list[i] = new GUIContent(mProtocol.DiscoveryResponses[i].hostname + " (" + mProtocol.DiscoveryResponses[i].ipAddress + ")");
					}
					return true;
				}

			}
			list = null;
			return false;
		}

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
			sDiscoveryResponse server =  mProtocol.DiscoveryResponses[pickedServer];
			if(mProtocol.connect(server, udpPort))
			{
				return connectStream(udpPort, streammode, streamval, stream6d, stream3d);
			}
			Debug.Log ("Error Creating Connection to server");
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
				mMarkers.Clear();
				foreach (sSettings3DLabel marker in mProtocol.Settings3D.labels3D)
                {
                    LabeledMarker newMarker = new LabeledMarker();
                    newMarker.label = marker.name;
                    newMarker.position = Vector3.Zero;
                   /* 
                    newMarker.color.r = (marker.colorRGB) & 0xFF;
                    newMarker.color.g = (marker.colorRGB >> 8) & 0xFF;
                    newMarker.color.b = (marker.colorRGB >> 16) & 0xFF;

                    newMarker.color /= 255;
                    */
                    Markers.Add(newMarker);
                }

				// Save bone settings
                if (mProtocol.Settings3D.bones != null)
		        {
	                Bones.Clear();

	                //Save bone settings
	                foreach (var settingsBone in mProtocol.Settings3D.bones)
	                {
	                    Bone bone = new Bone();
	                    bone.from = settingsBone.from;
	                    bone.fromMarker = getMarker(settingsBone.from);
	                    bone.to = settingsBone.to;
	                    bone.toMarker = getMarker(settingsBone.to);
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
                        Debug.Log("Error retrieving settings");
                        return false;
                    }
                }

                if (stream6d)
                {
                    if (!get6DOFSettings())
                    {
                        Debug.Log("Error retrieving settings");
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
			else
			{
				Debug.Log ("Error Creating Connection to server");
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

    // Class for labeled markers with unity datatypes
	public class LabeledMarker
	{
		public LabeledMarker() { }
		public string label;
		public Vector3 position;
		//public Color color;
	}

    // Class for bones
	public class Bone
	{
		public Bone() { }
		public string from;
		public LabeledMarker fromMarker;
		public string to;
		public LabeledMarker toMarker;
	}

}
