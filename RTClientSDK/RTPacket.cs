using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;

namespace QTMRealTimeSDK
{
#region Enums related to RTPacket
	/// <summary> Type of package sent via RT. </summary>
	public enum ePacketType
	{
		kPacketError = 0,
		kPacketCommand,
		kPacketXML,
		kPacketData,
		kPacketNoMoreData,
		kPacketC3DFile,
		kPacketEvent,
		kPacketDiscover,
		kPacketQTMFile,
		PacketNone
	}

	/// <summary> Type of component sent with RT. </summary>
	public enum eComponentType
	{
		kComponent3d = 1,
		kComponent3dNoLabels,
		kComponentAnalog,
		kComponentForce,
		kComponent6d,
		kComponent6dEuler,
		kComponent2d,
		kComponent2dLin,
		kComponent3dRes,
		kComponent3dNoLabelsRes,
		kComponent6dRes,
		kComponent6dEulerRes,
		kComponentAnalogSingle,
		kComponentImage,
		kComponentForceSingle,
		kComponentNone
	}

	/// <summary> Events sent from QTM via RT. </summary>
	public enum eEvent
	{
		kEventConnected = 1,
		kEventConnectionClosed,
		kEventCaptureStarted,
		kEventCaptureStopped,
		kEventCaptureFetchingFinished,
		kEventCalibrationStarted,
		kEventCalibrationStopped,
		kEventRTFromFileStarted,
		kEventRTFromFileSTopped,
		kEventWaitingForTrigger,
		kEventCameraSettingsChanged,
		kEventQTMShuttingDown,
		kEventCaptureSaved,
		kEventNone
	}
#endregion

#region Structs related to RTPacket

	/// <summary> Data for cameras, includes 2D marker data. </summary>
	public struct sCamera
	{
		/// <summary> Number of markers. </summary>
		public uint markerCount;
		/// <summary> Only first bits is used, too much light enters camera. </summary>
		public byte statusFlags;
		/// <summary> Marker data for this camera. </summary>
		public s2D[] markerData2D;
	}

	/// <summary> Struct for xyz coordinates. </summary>
	public struct sPoint
	{
        [XmlElement("X")]
		public float x;
        [XmlElement("Y")]
        public float y;
        [XmlElement("Z")]
        public float z;
	}

    /// <summary> Data from a force plate, includes samples. </summary>
	public struct sForcePlate
	{
        /// <summary> ID of plate. </summary>
		public int plateID;
        /// <summary> Number of forces in frame. </summary>
		public int forceCount;
        /// <summary> Force number, increased with the force frequency. </summary>
		public int forceNumber;
        /// <summary> Samples collected from plate. </summary>
		public sForceSample[] forceSamples;
	}

	/// <summary> samples for a force plat. </summary>
	public struct sForceSample
	{
		/// <summary> Coordinate of the forc. </summary>
		public sPoint force;
		/// <summary> Coordinate of the momen. </summary>
		public sPoint moment;
		/// <summary> Coordinate of the force application poin. </summary>
		public sPoint applicationPoint;
	}

	/// <summary> 2D Data for markers from cameras. used by both for non- and linearized marker. </summary>
	public struct s2D
	{
		/// <summary> X coordinate of the marker. </summary>
		public uint x;
		/// <summary> Y coordinate of the marker. </summary>
		public uint y;
		/// <summary> X diameter of the marker. </summary>
		public ushort diameterX;
		/// <summary> Y diameter of the marker. </summary>
		public ushort diameterY;
	}

	/// <summary> 3D data for marker. </summary>
	public struct s3D
	{
		/// <summary> ID of marker, 0 of marker has label. </summary>
		public uint id;
		/// <summary> Position data for marker. </summary>
		public sPoint position;
		/// <summary> Residual for marker, -1 if residual was not requested. </summary>
		public float residual;
	}

	/// <summary> Data for 6DOF (6 Degrees Of Freedom) Body. </summary>
	public struct s6DOF
	{
		/// <summary> Position data for bod. </summary>
		public sPoint position;
		/// <summary> Rotation matrix for bod. </summary>
		public float[] matrix;
		/// <summary> Residual for body, -1 if residual was not requested. </summary>
		public float residual;
	}

	/// <summary> Data for 6DOF (6 Degrees Of Freedom) Body, Euler angles instead of matri. </summary>
	public struct s6DOFEuler
	{
		/// <summary> Position data for bod. </summary>
		public sPoint position;
		/// <summary> Euler angles for bod. </summary>
		public sPoint rotation;
		/// <summary> Residual for body, -1 if residual was not requested. </summary>
		public float residual;
	}

    /// <summary> Data from a analog device, includes samples. </summary>
    public struct sAnalog
    {
        /// <summary> Device ID. </summary>
        public uint deviceID;
        /// <summary> Number of channels. </summary>
        public uint channelCount;

        /// <summary> Samples for all chanels. </summary>
        public sAnalogSample[] channels;

    }

    /// <summary> Channel data from analog device. </summary>
    public struct sAnalogSample
	{
        /// <summary> Number of samples. </summary>
        public uint sampleCount;
		/// <summary> Sample data for channel. </summary>
		public float[] sampleData;
	}

    /// <summary> Data for image. </summary>
	public struct sImage
	{
        /// <summary> Id of camera image originates from. </summary>
		public uint cameraID;

        /// <summary> Image format. </summary>
		public eImageFormat imageFormat;
        /// <summary> Width of image. </summary>
		public uint width;
        /// <summary> Height of image. </summary>
		public uint height;
        /// <summary> Scaled value of cropping from left. </summary>
		public float leftCrop;
        /// <summary> Scaled value of cropping from top. </summary>
        public float topCrop;
        /// <summary> Scaled value of cropping from right. </summary>
        public float rightCrop;
        /// <summary> Scaled value of cropping from bottom. </summary>
        public float bottomCrop;
        /// <summary> Size of image data. </summary>
		public int imageSize;
        /// <summary> Actual image data. </summary>
		public byte[] imageData;
	}

    /// <summary> Data with response from Discovery broadcast. </summary>
    public struct sDiscoveryResponse
    {
        /// <summary> Hostname of server. </summary>
        public string hostname;
        /// <summary> IP to server. </summary>
        public string ipAddress;
        /// <summary> Base port. </summary>
        public short port;
        /// <summary> info text about host. </summary>
        public string infoText;
        /// <summary> Number of cameras connected to server. </summary>
        public int cameraCount;
    }

#endregion

	public class RTPacket
	{
        /// <summary> return packet with no data but only type set to error packet. </summary>
        public static RTPacket ErrorPacket{ get { return new RTPacket(ePacketType.kPacketError); } }

		int mMajorVersion;
        /// <summary> Major protocol version of packet. </summary>
        public int MajorVersion { get { return mMajorVersion; } }

        int mMinorVersion;
        /// <summary> Minor protocol version of packet. </summary>
        public int MinorVersion { get { return mMinorVersion; } }

        bool mBigEndian;
        /// <summary> if packet is using big endian. </summary>
        public bool IsBigEndian { get { return mBigEndian; } }

		int mPacketSize;
        /// <summary> size of packet in bytes. </summary>
        public int PacketSize { get { return mPacketSize; } }

		ePacketType mPacketType;
		 /// <summary> what type of packet. </summary>
        public ePacketType PacketType { get { return mPacketType; } }

		long mTimestamp;
		 /// <summary> if the packet is a data packet, this will return the timestamp, otherwise -1. </summary>
        public long TimeStamp { get { return mTimestamp; } }

		int mFrameNumber;
		 /// <summary> if the packet is a data packet, this will return the frame number, otherwise -1. </summary>
        public int Frame { get { return mFrameNumber; } }

		int mComponentCount;
		 /// <summary> if the packet is a data packet, this will return the number of component types in packet, otherwise -1. </summary>
        public int ComponentCount { get { return mComponentCount; } }

        uint m2DDropRate;
        /// <summary>Drop rate from cameras</summary>
        public uint DropRate { get { return m2DDropRate; } }
        uint m2DOutOfSyncRate;
        /// <summary>Out of sync rate from cameras</summary>
        public uint OutOfSyncRate { get { return m2DOutOfSyncRate; } }

        /// <summary>Number of cameras</summary>
        public int CameraCount { get { return (m2DMarkerData != null) ? m2DMarkerData.Count : -1 ; } }
        /// <summary>Number of Mmarkers</summary> //FIXME
        public int MarkerCount { get { return (m3DMarkerData != null) ? m3DMarkerData.Count : -1; } }
        /// <summary>Number of bodies</summary> //FIXME
        public int BodyCount { get { return (m6DOFData != null) ? m6DOFData.Count : -1; } }

        byte[] mData;
        public byte[] Data { get { return mData; } }
		List<sCamera> m2DMarkerData;
		List<sCamera> m2DLinearMarkerData;
		List<s3D> m3DMarkerData;
		List<s3D> m3DMarkerResData;
		List<s3D> m3DMarkerNoLabelData;
		List<s3D> m3DMarkerNoLabelResData;
        List<s6DOF> m6DOFData;
        List<s6DOF> m6DOFResData;
        List<s6DOFEuler> m6DOFEulerData;
        List<s6DOFEuler> m6DOFEulerResData;
		List<sAnalog> mAnalogSamples;
        List<sAnalog> mAnalogSingleSample;
		List<sForcePlate> mForcePlates;
		List<sForcePlate> mForceSinglePlate;
		List<sImage> mImageData;


        /// <summary>
        /// Private constructor only used for static error packet.
        /// </summary>
        /// <param name="type"></param>
        private RTPacket(ePacketType type)
        {
            mPacketType = type;
        }
        /// <summary>
        /// Constructor for packet.
        /// </summary>
        /// <param name="majorVersion">Major version of packet, default is latest version.</param>
        /// <param name="minorVersion">Minor version of packet, default is latest version.</param>
        /// <param name="bigEndian">if packet should use big endianess, default is false.</param>
		public RTPacket(int majorVersion = RTProtocol.Constants.MAJOR_VERSION,
                        int minorVersion = RTProtocol.Constants.MINOR_VERSION,
                        bool bigEndian = RTProtocol.Constants.BIG_ENDIAN)
		{
			mMajorVersion = majorVersion;
			mMinorVersion = minorVersion;
			mBigEndian = bigEndian;

			m2DMarkerData = new List<sCamera>();
			m2DLinearMarkerData = new List<sCamera>();

            m3DMarkerData = new List<s3D>();
            m3DMarkerResData = new List<s3D>();
            m3DMarkerNoLabelData = new List<s3D>();
            m3DMarkerNoLabelResData = new List<s3D>();

			m6DOFData = new List<s6DOF>();
            m6DOFResData = new List<s6DOF>();
            m6DOFEulerData = new List<s6DOFEuler>();
            m6DOFEulerResData = new List<s6DOFEuler>();

            mAnalogSamples = new List<sAnalog>();
		    mAnalogSingleSample = new List<sAnalog>();

            mForcePlates = new List<sForcePlate>();
			mForceSinglePlate = new List<sForcePlate>();

            mImageData = new List<sImage>();

            clearData();
		}

        /// <summary>
        /// Get version of packet.
        /// </summary>
        /// <param name="majorVersion">Major version of packet.</param>
        /// <param name="minorVersion">Minor version of packet.</param>
		public void getVersion(ref int majorVersion, ref int minorVersion)
		{
			majorVersion = mMajorVersion;
			minorVersion = mMinorVersion;
		}

        /// <summary>
        /// Clear packet from data.
        /// </summary>
		public void clearData()
		{
			mData = null;
			mPacketSize = -1;
			mPacketType = ePacketType.PacketNone;

			mTimestamp = -1;
			mFrameNumber = -1;
			mComponentCount = -1;


			m2DMarkerData.Clear();
			m2DLinearMarkerData.Clear();
            m3DMarkerData.Clear();
            m3DMarkerResData.Clear();
            m3DMarkerNoLabelData.Clear();
            m3DMarkerNoLabelResData.Clear();
            m6DOFData.Clear();
            m6DOFResData.Clear();
            m6DOFEulerData.Clear();
            m6DOFEulerResData.Clear();
			mAnalogSamples.Clear();
			mAnalogSingleSample.Clear();
			mForcePlates.Clear();
			mForceSinglePlate.Clear();
			mImageData.Clear();

		}

        #region Set packet data function
        /// <summary>
        /// Set the data of packet.
        /// </summary>
        /// <param name="data">byte data recieved from server</param>
		public void setData(byte[] data)
		{
			/*  === Data packet setup ===
			 *  Packet size - 4 bytes
			 *  packet type - 4 bytes
			 *  timestamp - 8 bytes
			 *  Component count - 4 bytes
			 *  [for each component]
			 *    Component size - 4 bytes
			 *    Component type - 4 bytes
			 *    Component Data - [Component size] bytes
			 */

            clearData();
			mData = data;
            setPacketHeader();

			if (mPacketType == ePacketType.kPacketData)
			{
				setTimeStamp();
				setFrameNumber();
				setComponentCount();

                int position = RTProtocol.Constants.PACKET_HEADER_SIZE + RTProtocol.Constants.DATA_PACKET_HEADER_SIZE;

				for (int component = 1; component <= mComponentCount; component++)
				{

					eComponentType componentType = getComponentType(position);
                    position += RTProtocol.Constants.COMPONENT_HEADER;
					if (componentType == eComponentType.kComponent3d)
					{
						/* Marker count - 4 bytes
						 * 2D Drop rate - 2 bytes
						 * 2D Out of sync rate - 2 bytes
						 * [Repeated per marker]
						 *   X - 4 bytes
						 *   Y - 4 bytes
						 *   Z - 4 bytes
						*/
                        uint markerCount = BitConvert.getUInt32(mData, ref position, mBigEndian);
                        m2DDropRate = BitConvert.getUShort(mData, ref position, mBigEndian);
                        m2DOutOfSyncRate = BitConvert.getUShort(mData, ref position, mBigEndian);

						for(int i = 0; i < markerCount; i++)
						{
							s3D marker;
							marker.id = 0;
							marker.residual = -1;
							marker.position = BitConvert.getPoint(mData, ref position, mBigEndian);

							m3DMarkerData.Add(marker);
						}
					}
					else if (componentType == eComponentType.kComponent3dNoLabels)
					{
						/* Marker count - 4 bytes
						 * 2D Drop rate - 2 bytes
						 * 2D Out of sync rate - 2 bytes
						 * [Repeated per marker]
						 *   X - 4 bytes
						 *   Y - 4 bytes
						 *   Z - 4 bytes
						 *   ID - 4 bytes
						 */

						int markerCount = BitConvert.getInt32(mData, ref position, mBigEndian);
                        m2DDropRate = BitConvert.getUShort(mData, ref position, mBigEndian);
                        m2DOutOfSyncRate = BitConvert.getUShort(mData, ref position, mBigEndian);

						for (int i = 0; i < markerCount; i++)
						{
							s3D marker;
							marker.residual = -1;
							marker.position = BitConvert.getPoint(mData, ref position, mBigEndian);
							marker.id = BitConvert.getUInt32(mData, ref position, mBigEndian);
							m3DMarkerNoLabelData.Add(marker);
						}

					}
					else if (componentType == eComponentType.kComponentAnalog)
					{
                        /* Analog Device count - 4 bytes
						 * [Repeated per device]
                         *   Device id - 4 bytes
                         *   Channel count - 4 bytes
                         *   Sample count - 4 bytes
                         *   Sample number - 4 bytes
                         *   Analog data - 4 * channelcount * sampleCount
						 */

                        uint deviceCount = BitConvert.getUInt32(mData, ref position, mBigEndian);
                        for (int i = 0; i < deviceCount; i++)
                        {
                            sAnalog analogDeviceData;
                            analogDeviceData.deviceID = BitConvert.getUInt32(mData, ref position, mBigEndian);
                            analogDeviceData.channelCount = BitConvert.getUInt32(mData, ref position, mBigEndian);
                            analogDeviceData.channels = new sAnalogSample[analogDeviceData.channelCount];

                            uint sampleCount = BitConvert.getUInt32(mData, ref position, mBigEndian);

                            for (int j = 0; j < analogDeviceData.channelCount; j++)
                            {
                                sAnalogSample sample;
                                sample.sampleCount = sampleCount;
                                sample.sampleData = new float[sampleCount];
                                for (int k = 0; k < sampleCount; k++)
                                {
                                    sample.sampleData[k] = BitConvert.getInt32(mData, ref position, mBigEndian);
                                }

                                analogDeviceData.channels[j] = sample;
                            }
                            mAnalogSamples.Add(analogDeviceData);
                        }


                    }
					else if (componentType == eComponentType.kComponentForce)
					{
                        /* Force plate count - 4 bytes
                         * [Repeated per plate]
						 *   Force plate ID - 4 bytes
						 *   Force count - 4 bytes
						 *   forceNumber - 4 bytes
						 *   Force data - 36 * force count bytes
						 */

                        int forcePlateCount = BitConvert.getInt32(mData, ref position, mBigEndian);
                        for (int i = 0; i < forcePlateCount; i++)
                        {
                            sForcePlate plate;
                            plate.plateID = BitConvert.getInt32(mData, ref position, mBigEndian);
                            plate.forceCount = BitConvert.getInt32(mData, ref position, mBigEndian);
                            plate.forceNumber = BitConvert.getInt32(mData, ref position, mBigEndian);
                            plate.forceSamples = new sForceSample[plate.forceCount];

                            for (int j = 0; j < plate.forceCount; j++)
                            {
                                sForceSample sample;
                                sample.force = BitConvert.getPoint(mData, ref position, mBigEndian);
                                sample.moment = BitConvert.getPoint(mData, ref position, mBigEndian);
                                sample.applicationPoint = BitConvert.getPoint(mData, ref position, mBigEndian);
                                plate.forceSamples[j] = sample;
                            }

                            mForcePlates.Add(plate);
                        }

                    }
					else if (componentType == eComponentType.kComponent6d)
					{
						/* Body count - 4 bytes
						 * 2D Drop rate - 2 bytes
						 * 2D Out of sync rate - 2 bytes
						 * [Repeated per body]
						 *   X - 4 bytes
						 *   Y - 4 bytes
						 *   Z - 4 bytes
						 *   rotation matrix - 9*4 bytes
						 */

						int bodyCount = BitConvert.getInt32(mData, ref position, mBigEndian);
                        m2DDropRate = BitConvert.getUShort(mData, ref position, mBigEndian);
                        m2DOutOfSyncRate = BitConvert.getUShort(mData, ref position, mBigEndian);

						for (int i = 0; i < bodyCount; i++)
						{
							s6DOF body;
							body.position = BitConvert.getPoint(mData, ref position, mBigEndian);
							body.matrix = new float[9];

                            for (int j = 0; j < 9; j++)
                            {
                                body.matrix[j] = BitConvert.getFloat(mData, ref position, mBigEndian);
                            }

                            body.residual = -1;
							m6DOFData.Add(body);
						}
					}
					else if (componentType == eComponentType.kComponent6dEuler)
					{

						/* Body count - 4 bytes
						 * 2D Drop rate - 2 bytes
						 * 2D Out of sync rate - 2 bytes
						 * [Repeated per body]
						 *   X - 4 bytes
						 *   Y - 4 bytes
						 *   Z - 4 bytes
						 *   Euler Angles - 3*4 bytes
						 */

						int bodyCount = BitConvert.getInt32(mData, ref position, mBigEndian);
                        m2DDropRate = BitConvert.getUShort(mData, ref position, mBigEndian);
                        m2DOutOfSyncRate = BitConvert.getUShort(mData, ref position, mBigEndian);

						for (int i = 0; i < bodyCount; i++)
						{
							s6DOFEuler body;
							body.position = BitConvert.getPoint(mData, ref position, mBigEndian);
							body.rotation = BitConvert.getPoint(mData, ref position, mBigEndian);
							body.residual = -1;
							m6DOFEulerData.Add(body);
						}

					}
					else if (componentType == eComponentType.kComponent2d || componentType == eComponentType.kComponent2dLin)
					{
						/* Camera Count - 4 bytes
						 * 2D Drop rate - 2 bytes
						 * 2D Out of sync rate - 2 bytes
						 * [Repeated per Camera]
						 *   Marker Count - 4 bytes
						 *   Status Flags - 1 byte
						 *   [Repeated per Marker]
						 *     X - 4 Bytes
						 *     Y - 4 Bytes
						 *     Diameter X - 4 bytes
						 *     Diameter Y - 4 bytes
						 */

						uint cameraCount = BitConvert.getUInt32(mData, ref position, mBigEndian);
                        m2DDropRate  = BitConvert.getUShort(mData, ref position, mBigEndian);
                        m2DOutOfSyncRate = BitConvert.getUShort(mData, ref position, mBigEndian);

						for(int i = 0; i < cameraCount; i++)
						{
							sCamera camera;
							camera.markerCount = BitConvert.getUInt32(mData, ref position, mBigEndian);
							camera.statusFlags = mData[position++];
							camera.markerData2D = new s2D[camera.markerCount];
							for(int j = 0; j < camera.markerCount; j++)
							{
								s2D marker;
								marker.x = BitConvert.getUInt32(mData, ref position, mBigEndian);
								marker.y = BitConvert.getUInt32(mData, ref position, mBigEndian);
								marker.diameterX = BitConvert.getUShort(mData, ref position, mBigEndian);
								marker.diameterY = BitConvert.getUShort(mData, ref position, mBigEndian);
								camera.markerData2D[j] = marker;
							}
							if (componentType == eComponentType.kComponent2d)
								m2DMarkerData.Add(camera);
							else if (componentType == eComponentType.kComponent2dLin)
								m2DLinearMarkerData.Add(camera);
						}

					}
					else if (componentType == eComponentType.kComponent3dRes)
					{
						/* Marker count - 4 bytes
						 * 2D Drop rate - 2 bytes
						 * 2D Out of sync rate - 2 bytes
						 * [Repeated per marker]
						 *   X - 4 bytes
						 *   Y - 4 bytes
						 *   Z - 4 bytes
						 *   Residual - 4 bytes
						*/
						int markerCount = BitConvert.getInt32(mData, ref position, mBigEndian);
                        m2DDropRate = BitConvert.getUShort(mData, ref position, mBigEndian);
                        m2DOutOfSyncRate = BitConvert.getUShort(mData, ref position, mBigEndian);

						for (int i = 0; i < markerCount; i++)
						{
							s3D marker;
							marker.id = 0;
							marker.position = BitConvert.getPoint(mData, ref position, mBigEndian);
                            marker.residual = BitConvert.getInt32(mData, ref position, mBigEndian);

							m3DMarkerResData.Add(marker);
						}
					}
					else if (componentType == eComponentType.kComponent3dNoLabelsRes)
					{
						/* Marker count - 4 bytes
						 * 2D Drop rate - 2 bytes
						 * 2D Out of sync rate - 2 bytes
						 * [Repeated per marker]
						 *   X - 4 bytes
						 *   Y - 4 bytes
						 *   Z - 4 bytes
						 *   Residual - 4 bytes
						 *   ID - 4 bytes
						*/
						int markerCount = BitConvert.getInt32(mData, ref position, mBigEndian);
                        m2DDropRate = BitConvert.getUShort(mData, ref position, mBigEndian);
                        m2DOutOfSyncRate = BitConvert.getUShort(mData, ref position, mBigEndian);

						for (int i = 0; i < markerCount; i++)
						{
							s3D marker;
							marker.position = BitConvert.getPoint(mData, ref position, mBigEndian);
                            marker.residual = BitConvert.getInt32(mData, ref  position, mBigEndian);
                            marker.id = BitConvert.getUInt32(mData, ref  position, mBigEndian);

							m3DMarkerNoLabelResData.Add(marker);
						}

					}
					else if (componentType == eComponentType.kComponent6dRes)
					{
						/* Body count - 4 bytes
						 * 2D Drop rate - 2 bytes
						 * 2D Out of sync rate - 2 bytes
						 * [Repeated per marker]
						 *   X - 4 bytes
						 *   Y - 4 bytes
						 *   Z - 4 bytes
						 *   rotation matrix - 9*4 bytes
						 *   residual - 9*4 bytes
						 */

						int bodyCount = BitConvert.getInt32(mData, ref position, mBigEndian);
                        m2DDropRate = BitConvert.getUShort(mData, ref position, mBigEndian);
                        m2DOutOfSyncRate = BitConvert.getUShort(mData, ref position, mBigEndian);

						for (int i = 0; i < bodyCount; i++)
						{
							s6DOF body;
							body.position = BitConvert.getPoint(mData, ref position, mBigEndian);
							body.matrix = new float[9];
                            for (int j = 0; j < 9; j++)
                                body.matrix[j] = BitConvert.getFloat(mData, ref position, mBigEndian);
							body.residual = BitConvert.getInt32(mData, ref position, mBigEndian); ;
							m6DOFResData.Add(body);
						}

					}
					else if (componentType == eComponentType.kComponent6dEulerRes)
					{

						/* Body count - 4 bytes
						 * 2D Drop rate - 2 bytes
						 * 2D Out of sync rate - 2 bytes
						 * [Repeated per marker]
						 *   X - 4 bytes
						 *   Y - 4 bytes
						 *   Z - 4 bytes
						 *   Euler Angles - 3*4 bytes
						 *   residual - 9*4 bytes
						 */

						int bodyCount = BitConvert.getInt32(mData, ref position, mBigEndian);
                        m2DDropRate = BitConvert.getUShort(mData, ref position, mBigEndian);
                        m2DOutOfSyncRate = BitConvert.getUShort(mData, ref position, mBigEndian);

						for (int i = 0; i < bodyCount; i++)
						{
							s6DOFEuler body;
							body.position = BitConvert.getPoint(mData, ref position, mBigEndian);
							body.rotation = BitConvert.getPoint(mData, ref position, mBigEndian);
							body.residual = BitConvert.getInt32(mData, ref position, mBigEndian);
							m6DOFEulerResData.Add(body);
						}

					}
					else if (componentType == eComponentType.kComponentAnalogSingle)
					{
                        /* Analog Device count - 4 bytes
                         * [Repeated per device]
                         *   Device id - 4 bytes
                         *   Channel count - 4 bytes
                         *   Analog data - 4 * channelcount
                         */

                        int deviceCount = BitConvert.getInt32(mData, ref position, mBigEndian);
                        for (int i = 0; i < deviceCount; i++)
                        {
                            sAnalog device;
                            device.deviceID = BitConvert.getUInt32(mData, ref position, mBigEndian);
                            device.channelCount = BitConvert.getUInt32(mData, ref position, mBigEndian);
                            device.channels = new sAnalogSample[device.channelCount];
                            for (int j = 0; j < device.channelCount; j++)
                            {
                                sAnalogSample sample;
                                sample.sampleCount = 1;
                                sample.sampleData = new float[1];
                                sample.sampleData[0] = BitConvert.getInt32(mData, ref position, mBigEndian);

                                device.channels[j] = sample;
                            }

                            mAnalogSingleSample.Add(device);
                        }


					}
					else if (componentType == eComponentType.kComponentImage)
					{
						/* Camera count - 4 bytes
						 * [Repeated per marker]
						 *   Camera ID - 4 bytes
						 *   Image Format - 4 bytes
						 *   Width - 4 bytes
						 *   Height- 4 bytes
						 *   Left crop - 4 bytes
						 *   Top crop - 4 bytes
						 *   Right crop - 4 bytes
						 *   Bottom crop - 4 bytes
						 *   Image size- 4 bytes
						 *   Image data - [Image size bytes]
						 */

						int cameraCount = BitConvert.getInt32(mData, ref position, mBigEndian);
						for (int i = 0; i < cameraCount; i++)
						{
							sImage image;
							image.cameraID = BitConvert.getUInt32(mData, ref position, mBigEndian);
							image.imageFormat = (eImageFormat)BitConvert.getUInt32(mData, ref position, mBigEndian);
							image.width = BitConvert.getUInt32(mData, ref position, mBigEndian);
							image.height = BitConvert.getUInt32(mData, ref position, mBigEndian);
							image.leftCrop = BitConvert.getFloat(mData, ref position, mBigEndian);
                            image.topCrop = BitConvert.getFloat(mData, ref position, mBigEndian);
                            image.rightCrop = BitConvert.getFloat(mData, ref position, mBigEndian);
                            image.bottomCrop = BitConvert.getFloat(mData, ref position, mBigEndian);
							image.imageSize = BitConvert.getInt32(mData, ref position, mBigEndian);
							image.imageData = new byte[image.imageSize];
							Array.Copy(mData, position, image.imageData, 0, image.imageSize);
							position += image.imageSize;

							mImageData.Add(image);

						}
					}
					else if (componentType == eComponentType.kComponentForceSingle)
					{
                        /* Force plate count - 4 bytes
                         * [Repeated per plate]
						 *   Force plate ID - 4 bytes
						 *   Force data - 36 bytes
						 */

                        int forcePlateCount = BitConvert.getInt32(mData, ref position, mBigEndian);
                        for (int i = 0; i < forcePlateCount; i++)
                        {
                            sForcePlate plate;
                            plate.plateID = BitConvert.getInt32(mData, ref position, mBigEndian);
                            plate.forceCount = 1;
                            plate.forceNumber = -1;
                            plate.forceSamples = new sForceSample[plate.forceCount];
                            plate.forceSamples[0].force = BitConvert.getPoint(mData, ref position, mBigEndian);
                            plate.forceSamples[0].moment = BitConvert.getPoint(mData, ref position, mBigEndian);
                            plate.forceSamples[0].applicationPoint = BitConvert.getPoint(mData, ref position, mBigEndian);

                            mForceSinglePlate.Add(plate);
                        }
					}
				}
			}
		}
        #endregion

#region private set functions for packet header data
        /// <summary>
        /// Set this packet's header.
        /// </summary>
        private void setPacketHeader()
		{
            mPacketSize = getSize();
            setType();
		}

        /// <summary>
        /// Get the packet type of this packet.
        /// </summary>
        /// <returns>Packet type</returns>
		private void setType()
		{
            if (mPacketSize < 4)
				mPacketType = ePacketType.PacketNone;

			byte[] packetData = new byte[4];
			Array.Copy(mData, 4, packetData, 0, 4);
			if (mBigEndian)
				Array.Reverse(packetData);
			mPacketType = (ePacketType)BitConverter.ToInt32(packetData, 0);
		}

       /// <summary>
        /// set timestamp for this packet
        /// </summary>
        private void setTimeStamp()
        {
            if (mPacketType == ePacketType.kPacketData)
            {
                byte[] timeStampData = new byte[8];
                Array.Copy(mData, RTProtocol.Constants.PACKET_HEADER_SIZE, timeStampData, 0, 8);
                if (mBigEndian)
                    Array.Reverse(timeStampData);
                mTimestamp = BitConverter.ToInt64(timeStampData, 0);
            }
            else
            {
                mTimestamp = -1;
            }
        }

        /// <summary>
        /// Set frame number for this packet
        /// </summary>
        private void setFrameNumber()
        {
            if (mPacketType == ePacketType.kPacketData)
            {
                byte[] frameData = new byte[4];
                Array.Copy(mData, RTProtocol.Constants.PACKET_HEADER_SIZE + 8, frameData, 0, 4);
                if (mBigEndian)
                    Array.Reverse(frameData);
                mFrameNumber = BitConverter.ToInt32(frameData, 0);
            }
            else
            {
                mFrameNumber = -1;
            }
        }

        /// <summary>
        /// set component count for this function
        /// </summary>
        private void setComponentCount()
        {
            if (mPacketType == ePacketType.kPacketData)
            {
                byte[] componentCountData = new byte[4];
                Array.Copy(mData, RTProtocol.Constants.PACKET_HEADER_SIZE + 12, componentCountData, 0, 4);
                if (mBigEndian)
                    Array.Reverse(componentCountData);
                mComponentCount = BitConverter.ToInt32(componentCountData, 0);
            }
            else
            {
                mComponentCount = -1;
            }
        }
#endregion

#region get functions for packet header data

        /// <summary>
        /// Get the size and packet type of a packet.
        /// </summary>
        /// <param name="data">byte data for packet</param>
        /// <param name="size">returns size of packet</param>
        /// <param name="type">returns type of packet</param>
        /// <param name="bigEndian">if packet is big endian or not, default is false</param>
        /// <returns>true if header was retrieved successfully </returns>
        public static bool getPacketHeader(byte[] data, out int size, out ePacketType type, bool bigEndian = false)
        {
            if (bigEndian)
            {
                Array.Reverse(data, 0, 4);
                Array.Reverse(data, 4, 4);
            }

            size = BitConverter.ToInt32(data, 0);
            type = (ePacketType)BitConverter.ToInt32(data, 4);

            return true;
        }

        /// <summary>
        /// Get number of bytes in packet.
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <param name="bigEndian">if packet is big endian or not, default is false</param>
        /// <returns>Size of packet.</returns>
        public static int getSize(byte[] data, bool bigEndian = false)
        {
            int size = BitConverter.ToInt32(data, 0);
            return size;
        }

        /// <summary>
        /// Get the packet type of packet.
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <param name="bigEndian">if packet is big endian or not. default is false</param>
        /// <returns>packet type</returns>
        public static ePacketType getType(byte[] data, bool bigEndian = false)
        {
            if (data.GetLength(0) < 4)
                return ePacketType.PacketNone;

            if (bigEndian)
                Array.Reverse(data, 4, 4);
            return (ePacketType)BitConverter.ToInt32(data, 4);
        }

        /// <summary>
        /// Get time stamp in a data packet.
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <param name="bigEndian">if packet is big endian or not. default is false</param>
        /// <returns>time stamp from packet</returns>
        public static long getTimeStamp(byte[] data, bool bigEndian = false)
        {
            if (getType(data, bigEndian) == ePacketType.kPacketData)
            {
                if (bigEndian)
                    Array.Reverse(data, RTProtocol.Constants.PACKET_HEADER_SIZE, 8);
                return BitConverter.ToInt64(data, RTProtocol.Constants.PACKET_HEADER_SIZE);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Get frame number from a data packet.
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <param name="bigEndian">if packet is big endian or not. default is false</param>
        /// <returns>frame number from packet</returns>
        public static int getFrameNumber(byte[] data, bool bigEndian = false)
        {
            if (getType(data, bigEndian) == ePacketType.kPacketData)
            {
                if (bigEndian)
                    Array.Reverse(data, RTProtocol.Constants.PACKET_HEADER_SIZE + 8, 4);
                return BitConverter.ToInt32(data, RTProtocol.Constants.PACKET_HEADER_SIZE + 8);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Get count of different component types from a datapacket
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <param name="bigEndian">if packet is big endian or not. default is false</param>
        /// <returns>number of component types in packet</returns>
        public static int getComponentCount(byte[] data, bool bigEndian = false)
        {
            if (getType(data, bigEndian) == ePacketType.kPacketData)
            {
                if (bigEndian)
                    Array.Reverse(data, RTProtocol.Constants.PACKET_HEADER_SIZE + 12, 4);
                return BitConverter.ToInt32(data, RTProtocol.Constants.PACKET_HEADER_SIZE + 12);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Get the size and packet type of a packet.
        /// </summary>
        /// <param name="data">byte data for packet</param>
        /// <param name="size">returns size of packet</param>
        /// <param name="type">returns type of packet</param>
        /// <param name="bigEndian">if packet is big endian or not, default is false</param>
        /// <returns>true if header was retrieved successfully </returns>
        public bool getPacketHeader(out int size, out ePacketType type)
        {
            byte[] data = new byte[8];
            Array.Copy(mData, 0, data, 0, 8);
            if (mBigEndian)
            {
                Array.Reverse(data, 0, 4);
                Array.Reverse(data, 4, 4);
            }

            size = BitConverter.ToInt32(data, 0);
            type = (ePacketType)BitConverter.ToInt32(data, 4);

            return true;
        }

        /// <summary>
        /// Get number of bytes in packet.
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <returns>Size of packet.</returns>
        public int getSize()
        {
            byte[] data = new byte[4];
            Array.Copy(mData, 0, data, 0, 4);
            if (mBigEndian)
            {
                Array.Reverse(data);
            }

            return BitConverter.ToInt32(data, 0);
        }

        /// <summary>
        /// Get the packet type of packet.
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <param name="bigEndian">if packet is big endian or not. default is false</param>
        /// <returns>packet type</returns>
        public ePacketType getType()
        {
           byte[] data = new byte[4];
            Array.Copy(mData, 4, data, 0, 4);

            if (data.GetLength(0) < 4)
                return ePacketType.PacketNone;

            if (mBigEndian)
                Array.Reverse(data);
            return (ePacketType)BitConverter.ToInt32(data, 0);
        }

        /// <summary>
        /// Get time stamp in a data packet.
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <param name="bigEndian">if packet is big endian or not. default is false</param>
        /// <returns>time stamp from packet</returns>
        public long getTimeStamp()
        {
            if (getType() == ePacketType.kPacketData)
            {
                byte[] data = new byte[8];
                Array.Copy(mData, RTProtocol.Constants.PACKET_HEADER_SIZE, data, 0, 8);

                if (mBigEndian)
                    Array.Reverse(data);
                return BitConverter.ToInt64(data,0);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Get frame number from a data packet.
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <param name="bigEndian">if packet is big endian or not. default is false</param>
        /// <returns>frame number from packet</returns>
        public int getFrameNumber()
        {
            if (getType() == ePacketType.kPacketData)
            {
                byte[] data = new byte[4];
                Array.Copy(mData, RTProtocol.Constants.PACKET_HEADER_SIZE+8, data, 0, 4);

                if (mBigEndian)
                    Array.Reverse(data);
                return BitConverter.ToInt32(data,0);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Get count of different component types from a datapacket
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <param name="bigEndian">if packet is big endian or not. default is false</param>
        /// <returns>number of component types in packet</returns>
        public int getComponentCount()
        {
            if (getType() == ePacketType.kPacketData)
            {
                byte[] data = new byte[4];
                Array.Copy(mData, RTProtocol.Constants.PACKET_HEADER_SIZE + 12, data, 0, 4);

                if (mBigEndian)
                    Array.Reverse(data);
                return BitConverter.ToInt32(data, 0);
            }
            else
            {
                return -1;
            }
        }

#endregion

#region Component related get functions

        /// <summary>
        /// Get component type at position in this packet.
        /// </summary>
        /// <param name="position">position in packet where the component starts</param>
        /// <returns>Component type</returns>
        private eComponentType getComponentType(int position)
		{
			byte[] componentData = new byte[4];
			Array.Copy(mData, position+4, componentData, 0, 4);
			if (mBigEndian)
				Array.Reverse(componentData);

			return (eComponentType)BitConverter.ToInt32(componentData, 0);
		}

        /// <summary>
        /// Get size of component at position of this packet.
        /// </summary>
        /// <param name="position">position in packet where the component starts</param>
        /// <returns>size of component.</returns>
        private int getComponentSize(int position)
		{
			byte[] componentData = new byte[4];
			Array.Copy(mData, position, componentData, 0, 4);
			if (mBigEndian)
				Array.Reverse(componentData);
			return BitConverter.ToInt32(componentData, 0);
		}

        /// <summary>
        /// Get error string from this packet.
        /// </summary>
        /// <returns>error string, null if the packet is not an error packet.</returns>
		public string getErrorString()
		{
			if (mPacketType == ePacketType.kPacketError)
                return System.Text.Encoding.Default.GetString(mData, RTProtocol.Constants.PACKET_HEADER_SIZE,getSize()-RTProtocol.Constants.PACKET_HEADER_SIZE-1);
			else
				return null;
		}

        /// <summary>
        /// Get command string from this packet.
        /// </summary>
        /// <returns>command string, null if the packet is not a command packet.</returns>
		public string getCommandString()
		{
            if (mPacketType == ePacketType.kPacketCommand)
                //return BitConverter.ToString(mData, RTProtocol.Constants.PACKET_HEADER_SIZE);
                return System.Text.Encoding.Default.GetString(mData, RTProtocol.Constants.PACKET_HEADER_SIZE,getSize()-RTProtocol.Constants.PACKET_HEADER_SIZE-1);
            else
                return null;
		}

       /// <summary>
        /// Get XML string from this packet.
        /// </summary>
        /// <returns>XML string, null if the packet is not a XML packet.</returns>
		public string getXMLString()
		{
			if (mPacketType == ePacketType.kPacketXML)
                return System.Text.Encoding.Default.GetString(mData, RTProtocol.Constants.PACKET_HEADER_SIZE,getSize()-RTProtocol.Constants.PACKET_HEADER_SIZE-1);
			else
				return null;
		}

        /// <summary>
        /// Get event type from this packet.
        /// </summary>
        /// <returns>event type</returns>
        public eEvent getEvent()
        {
            if (mPacketType == ePacketType.kPacketEvent)
            {
                return (eEvent)mData[RTProtocol.Constants.PACKET_HEADER_SIZE];
            }
            return eEvent.kEventNone;
        }

        /// <summary>
        /// Get port from discovery packet
        /// </summary>
        /// <returns>port number, -1 if packet is not a response</returns>
        public short getDiscoverResponseBasePort()
        {
            if (mPacketType == ePacketType.kPacketCommand)
            {
                byte[] portData = new byte[2];
                Array.Copy(mData, getSize()-2, portData, 0, 2);
                if (mBigEndian)
                    Array.Reverse(portData);
                return BitConverter.ToInt16(portData, 0);
            }

            return -1;
        }

        /// <summary>
        /// get all data from discovery packet
        /// </summary>
        /// <param name="discoveryResponse">data from packet</param>
        /// <returns>true if </returns>
        public bool getDiscoverData(out sDiscoveryResponse discoveryResponse)
        {
            if (mPacketType == ePacketType.kPacketCommand)
            {
                byte[] portData = new byte[2];
                Array.Copy(mData, getSize() - 2, portData, 0, 2);
                Array.Reverse(portData);
                discoveryResponse.port = BitConverter.ToInt16(portData, 0);

                byte[] stringData = new byte[getSize() - 10];
                Array.Copy(mData, 8, stringData, 0, getSize() - 10);
                string data = System.Text.Encoding.Default.GetString(stringData);
                string[] splittedData = data.Split(',');
                
                discoveryResponse.hostname = splittedData[0].Trim();
                discoveryResponse.infoText = splittedData[1].Trim();
				
                string camcount = splittedData[2].Trim();
                Regex pattern = new Regex("\\d*");
                Match camMatch = pattern.Match(camcount);
               
                if (camMatch.Success)
                {
                    camcount = camMatch.Groups[0].Value;
                    discoveryResponse.cameraCount = int.Parse(camcount);
                }
                else
                {
                    discoveryResponse.cameraCount = -1;
                }
				try
				{
                    discoveryResponse.ipAddress = "";
                    IPAddress[] adresses = System.Net.Dns.GetHostAddresses(discoveryResponse.hostname);
                    foreach(IPAddress ip in adresses)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            discoveryResponse.ipAddress = ip.ToString();
                            break;
                        }
                    }
				}
				catch
				{
					discoveryResponse.ipAddress = "";

					return false;
				}
                return true;
            }

            discoveryResponse.cameraCount = -1;
            discoveryResponse.hostname = "";
            discoveryResponse.infoText = "";
            discoveryResponse.ipAddress = "";
            discoveryResponse.port = -1;

            return false;
        }

          //////////////////////////////////////////////////////////////////
         ////////////////////     STATIC FUNCTIONS     ////////////////////
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get component type at position in a packet.
        /// </summary>
        /// <param name="data">packet data</param>
        /// <param name="position">position in packet where the component starts</param>
        /// <param name="bigEndian">if packet is big endian, default is false</param>
        /// <returns>Component type.</returns>
        public static eComponentType getComponentType(byte[] data, int position, bool bigEndian = false)
        {
            if (bigEndian)
                Array.Reverse(data, position + 4, 4);

            return (eComponentType)BitConverter.ToInt32(data, position + 4);
        }

        /// <summary>
        /// Get component type at position in a packet.
        /// </summary>
        /// <param name="data">packet data</param>
        /// <param name="position">position in packet where the component starts</param>
        /// <param name="bigEndian">if packet is big endian, default is false</param>
        /// <returns>size of component.</returns>
        public static int getComponentSize(byte[] data, int position, bool bigEndian)
        {
            if (bigEndian)
                Array.Reverse(data, position, 4);

            return BitConverter.ToInt32(data, position);
        }

        /// <summary>
        /// Get error string from a packet.
        /// </summary>
        /// <param name="data">packet data</param>
        /// <param name="bigEndian">if packet is big endian, default is false</param>
        /// <returns>error string, null if the packet is not an error packet.</returns>
        public static string getErrorString(byte[] data, bool bigEndian = false)
        {
            if (getType(data, bigEndian) == ePacketType.kPacketError)
                return BitConverter.ToString(data, RTProtocol.Constants.PACKET_HEADER_SIZE);
            else
                return null;
        }

        /// <summary>
        /// Get command string from a packet.
        /// </summary>
        /// <param name="data">packet data</param>
        /// <param name="bigEndian">if packet is big endian, default is false</param>
        /// <returns>command string, null if the packet is not a command packet.</returns>
        public static string getCommandString(byte[] data, bool bigEndian = false)
        {
            if (getType(data, bigEndian) == ePacketType.kPacketCommand)
                return BitConverter.ToString(data, RTProtocol.Constants.PACKET_HEADER_SIZE);
            else
                return null;
        }

        /// <summary>
        /// Get XML string from a packet.
        /// </summary>
        /// <param name="data">packet data</param>
        /// <param name="bigEndian">if packet is big endian, default is false</param>
        /// <returns>XML string, null if the packet is not a XML packet.</returns>
		public string getXMLString(byte[] data, bool bigEndian = false)
		{
			if (getType(data, bigEndian) == ePacketType.kPacketXML)
                return BitConverter.ToString(mData, RTProtocol.Constants.PACKET_HEADER_SIZE);
			else
				return null;
		}

        /// <summary>
        /// Get event type of a event packet
        /// </summary>
        /// <param name="data">packet data</param>
        /// <param name="bigEndian">if packet is big endian, default is false</param>
        /// <returns>event type of packet</returns>
        public static eEvent getEvent(byte[] data, bool bigEndian = false)
        {
            if (getType(data,bigEndian) == ePacketType.kPacketEvent)
            {
                return (eEvent)data[RTProtocol.Constants.PACKET_HEADER_SIZE + 1];
            }
            return eEvent.kEventNone;
        }

        /// <summary>
        /// Get base port from Discovery response packet
        /// </summary>
        /// <param name="data">packet data</param>
        /// <param name="bigEndian">if packet is big endian, default is false</param>
        /// <returns>port from response</returns>
        public static short GetDiscoverResponseBasePort(byte[] data, bool bigEndian = false)
        {
            if (getType(data, bigEndian) == ePacketType.kPacketCommand)
            {
                if (bigEndian)
                    Array.Reverse(data,getSize(data) - 2, 2);
                return BitConverter.ToInt16(data, getSize(data) - 2);
            }

            return -1;
        }

#endregion

#region get functions for streamed data

        /// <summary>
        /// Get 2D marker data
        /// </summary>
        /// <returns>List of all 2D marker data</returns>
        public List<sCamera> get2DMarkerData()
        {
	        return m2DMarkerData;
        }

        /// <summary>
        /// Get 2D marker data at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>2D marker data</returns>
        public sCamera get2DMarkerData(int index)
        {
	        return m2DMarkerData[index];
        }

        /// <summary>
        /// Get linear 2D marker data
        /// </summary>
        /// <returns>List of all linear 2D marker data</returns>
        public List<sCamera> get2DLinearMarkerData()
        {
	        return m2DLinearMarkerData;
        }

        /// <summary>
        /// Get linear 2D marker data at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>linear 2D marker data</returns>
        public sCamera get2DLinearMarkerData(int index)
        {
	        return m2DLinearMarkerData[index];
        }

         /// <summary>
        /// Get 3D marker data
        /// </summary>
        /// <returns>List of all 3D marker data</returns>
        public List<s3D> get3DMarkerData()
        {
	        return m3DMarkerData;
        }

        /// <summary>
        /// Get 3D marker data at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>3D marker data</returns>
        public s3D get3DMarkerData(int index)
        {
	        return m3DMarkerData[index];
        }

        /// <summary>
        /// Get 6DOF data
        /// </summary>
        /// <returns>List of all 6DOF body data (orientation described with rotation matrix)</returns>
        public List<s6DOF> get6DOFData()
        {
	        return m6DOFData;
        }

        /// <summary>
        /// Get 6DOF data of body at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>6DOF body data (orientation described with rotation matrix)</returns>
        public s6DOF get6DOFData(int index)
        {
	        return m6DOFData[index];
        }

         /// <summary>
        /// Get 6DOF data
        /// </summary>
        /// <returns>List of all 6DOF body data (orientation described with Euler angles)</returns>
        public List<s6DOFEuler> get6DOFEulerData()
        {
	        return m6DOFEulerData;
        }

        /// <summary>
        /// Get 6DOF data of body at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>6DOF body data (orientation described with Euler angles)</returns>
        public s6DOFEuler get6DOFEulerData(int index)
        {
	        return m6DOFEulerData[index];
        }

        /// <summary>
        /// Get all samples from all analog devices
        /// </summary>
        /// <returns>List of analog devices containing all samples gathered this frame</returns>
        public List<sAnalog> getAnalogSamples()
        {
	        return mAnalogSamples;
        }

        /// <summary>
        /// Get all samples from analog device at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>Analog device containing all samples gathered this frame</returns>
        public sAnalog getAnalogSample(int index)
        {
	        return mAnalogSamples[index];
        }

        /// <summary>
        /// Get sample from all analog devices(only one sample per frame)
        /// </summary>
        /// <returns>List of analog devices containing only one sample gathered this frame</returns>
        public List<sAnalog> getAnalogSingleSamples()
        {
	        return mAnalogSingleSample;
        }

        /// <summary>
        /// Get sample from analog device at index (only one sample per frame)
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>Analog device containing one sample gathered this frame</returns>
        public sAnalog getAnalogSingleSample(int index)
        {
	        return mAnalogSingleSample[index];
        }

        /// <summary>
        /// Get samples from all force plates
        /// </summary>
        /// <returns>List of all force plates containing all samples gathered this frame</returns>
        public List<sForcePlate> getForcePlates()
        {
	        return mForcePlates;
        }

        /// <summary>
        /// Get samples from force plate at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>Force plate containing all samples gathered this frame</returns>
        public sForcePlate getForcePlate(int index)
        {
	        return mForcePlates[index];
        }

        /// <summary>
        /// Get sample from all force plates (only one sample per frame)
        /// </summary>
        /// <returns>List of all force plates containing one sample gathered this frame</returns>
        public List<sForcePlate> getForceSinglePlates()
        {
	        return mForceSinglePlate;
        }

        /// <summary>
        /// Get samples from force plate at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>Force plate containing all samples gathered this frame</returns>
        public sForcePlate getForceSinglePlate(int index)
        {
	        return mForceSinglePlate[index];
        }

        /// <summary>
        /// Get images from all cameras
        /// </summary>
        /// <returns>list of all images</returns>
        public List<sImage> getImageData()
        {
	        return mImageData;
        }

        /// <summary>
        /// Get image from cameras at index
        /// </summary>
        /// <param name="index">index to get data from.(not camera index!)</param>
        /// <returns>Image from index</returns>
        public sImage getImageData(int index)
        {
	        return mImageData[index];
        }
#endregion


    }
}
