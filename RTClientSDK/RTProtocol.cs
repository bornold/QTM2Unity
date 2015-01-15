using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Threading;

using QTMRealTimeSDK.Settings;
using QTMRealTimeSDK.Network;

namespace QTMRealTimeSDK
{
    #region enums
    /// <summary> streaming rate. </summary>
    public enum eStreamRate
    {
        kRateAllFrames = 1,
        kRateFrequency,
        kRateFrequnecyDivisor
    }

    /// <summary> Camera models. </summary>
    public enum eCameraModel
    {
        [XmlEnum("MacReflex")]
        kModelMacReflex = 0,
        [XmlEnum("ProReflex 120")]
        kModelProReflex120,
        [XmlEnum("ProReflex 240")]
        kModelProReflex240,
        [XmlEnum("ProReflex 500")]
        kModelProReflex500,
        [XmlEnum("ProReflex 1000")]
        kModelProReflex1000,
        [XmlEnum("Oqus 100 ")]
        kModelQqus100,
        [XmlEnum("Oqus 300")]
        kModelQqus300,
        [XmlEnum("Oqus 300 Plus")]
        kModelQqus300Plus,
        [XmlEnum("Oqus 400")]
        kModelQqus400,
        [XmlEnum("Oqus 500")]
        kModelQqus500,
        [XmlEnum("Oqus 200 C")]
        kModelQqus200C,
        [XmlEnum("Oqus 500 Plus")]
        kModelQqus500Plus,
        [XmlEnum("Oqus 700")]
        kModelQqus700,
        [XmlEnum("Oqus 700 Plus")]
        kModelQqus700Plus,
    }

    /// <summary> Camera modes. </summary>
    public enum eCameraMode
    {
        [XmlEnum("Marker")]
        kModeMarker = 0,
        [XmlEnum("Marker Intensity")]
        kModeMarkerIntensity,
        [XmlEnum("Video")]
        kModeVideo
    }

    /// <summary> Sync out modes. </summary>
    public enum eSyncOutFreqMode
    {
        [XmlEnum("Shutter out")]
        kModeShutterOut = 0,
        [XmlEnum("Multiplier")]
        kModeMultiplier,
        [XmlEnum("Divisor")]
        kModeDivisor,
        [XmlEnum("Camera independent")]
        kModeActualFreq,
        [XmlEnum("Measurement time")]
        kModeActualMeasurementTime,
        [XmlEnum("SRAM wired")]
        kModeSRAMWireSync,
        [XmlEnum("Continuous  100Hz")]
        kModeFixed100Hz
    }

    /// <summary> Signal sources. </summary>
    public enum eSignalSource
    {
        [XmlEnum("Control port")]
        kSourceControlPort = 0,
        [XmlEnum("IR_receiver")]
        kSourceIRReceiver,
        [XmlEnum("SMPTE")]
        kSourceSMPTE,
        [XmlEnum("Video_sync")]
        kSourceVideoSync
    }

    /// <summary> Signal modes. </summary>
    public enum eSignalMode
    {
        [XmlEnum("Periodic")]
        kPeriodic = 0,
        [XmlEnum("Non-periodic")]
        kNonPeriodic
    }

    /// <summary> Axises. </summary>
    public enum eAxis
    {
        [XmlEnum("+X")]
        XAxisUpwards = 0,
        [XmlEnum("-X")]
        XAxisDownwards,
        [XmlEnum("+Y")]
        YAxisUpwards,
        [XmlEnum("-Y")]
        YAxisDownwards,
        [XmlEnum("+Z")]
        ZAxisUpwards,
        [XmlEnum("-Z")]
        ZAxisDownwards
    }

    /// <summary> Processing Actions. </summary>
    public enum eProcessingActions
    {
        [XmlEnum("False")]
        kProcessingNone = 0,
        [XmlEnum("2D")]
        kProcessingTracking2D,
        [XmlEnum("3D")]
        kProcessingTracking3D
    }

    /// <summary> Signal Edge. </summary>
    public enum eSignalEdge
    {
        [XmlEnum("Negative")]
        kNegative = 0,
        [XmlEnum("Positive")]
        kPositive
    }

    /// <summary> Signal Polarity. </summary>
    public enum eSignalPolarity
    {
        [XmlEnum("Negative")]
        kNegative = 0,
        [XmlEnum("Positive")]
        kPositive
    }

    /// <summary> Image formats Available. </summary>
    public enum eImageFormat
    {
        [XmlEnum("RAWGrayscale")]
        kFormatRawGrayScale = 0,
        [XmlEnum("RAWBGR")]
        kFormatRawBGR,
        [XmlEnum("JPG")]
        kFormatJPG,
        [XmlEnum("PNG")]
        kFormatPNG
    }
    #endregion

    #region structs
    /// <summary> General settings for Camera. </summary>
    public struct sSettingsGeneralCamera
    {
        /// <summary> ID of camera </summary>
        [XmlElement("ID")]
        public int cameraID;
        /// <summary> Model of camera </summary>
        [XmlElement("Model")]
        public eCameraModel eModel;
        /// <summary> If the camera is an underwater camera </summary>
        [XmlElement("UnderWater")]
        public bool bUnderWater;
        /// <summary> Serial number of the selected camera</summary>
        [XmlElement("Serial")]
        public int nSerial;
        /// <summary> Camera mode the camera is set to </summary>
        [XmlElement("Mode")]
        public eCameraMode eMode;
        /// <summary> values for camera video exposure, current, min and max</summary>
        [XmlElement("Video_Exposure")]
        public sCameraSetting videoExposure;
        /// <summary> values for camera video flash time, current, min and max</summary>
        [XmlElement("Video_Flash_Time")]
        public sCameraSetting videoFlashTime;
        /// <summary> values for camera marker exposure, current, min and max</summary>
        [XmlElement("Marker_Exposure")]
        public sCameraSetting markerExposure;
        /// <summary> values for camera marker threshold, current, min and max</summary>
        [XmlElement("Marker_Threshold")]
        public sCameraSetting markerThreshold;
        /// <summary> Position of camera</summary>
        [XmlElement("Position")]
        public sCameraPosition position;
        /// <summary> Orientation of camera</summary>
        [XmlElement("Orientation")]
        public int orientation;
        /// <summary> Marker resolution of camera, width and height</summary>
        [XmlElement("Marker_Res")]
        public sResolution markerResolution;
        /// <summary> Video resolution of camera, width and height</summary>
        [XmlElement("Video_Res")]
        public sResolution videoResolution;
        /// <summary> Marker Field Of View, left,top,right and bottom coordinates</summary>
        [XmlElement("Marker_FOV")]
        public sFOV markerFOV;
        /// <summary> Video Field Of View, left,top,right and bottom coordinates</summary>
        [XmlElement("Video_FOV")]
        public sFOV VideoFOV;
        /// <summary> Sync settings</summary>
        [XmlElement("Sync_Out")]
        public sSync syncOut;
    }

    /// <summary> settings regarding sync for Camera. </summary>
    public struct sSync
    {
        /// <summary> Sync mode for camera</summary>
        [XmlElement("Mode")]
        public eSyncOutFreqMode syncMode;
        /// <summary> Sync value, depending on mode</summary>
        [XmlElement("Value")]
        public int syncValue;
        /// <summary> Output duty cycle in percent</summary>
        [XmlElement("Duty_Cycle")]
        public int dutyCycle;
        /// <summary> TTL signal polarity. no used in SRAM or 100Hz mode</summary>
        [XmlElement("Signal_Polarity")]
        public eSignalPolarity signalPolarity;
    }

    /// <summary> Position for a camera. </summary>
    public struct sCameraPosition
    {
        /// <summary> X position</summary>
        [XmlElement("X")]
        public float x;
        /// <summary> Y position</summary>
        [XmlElement("Y")]
        public float y;
        /// <summary> Z position</summary>
        [XmlElement("Z")]
        public float z;
        /// <summary> Rotation matrix - [1,1] value</summary>
        [XmlElement("Rot_1_1")]
        public float rot11;
        /// <summary> Rotation matrix - [2,1] value</summary>
        [XmlElement("Rot_2_1")]
        public float rot21;
        /// <summary> Rotation matrix - [3,1] value</summary>
        [XmlElement("Rot_3_1")]
        public float rot31;
        /// <summary> Rotation matrix - [1,2] value</summary>
        [XmlElement("Rot_1_2")]
        public float rot12;
        /// <summary> Rotation matrix - [2,2] value</summary>
        [XmlElement("Rot_2_2")]
        public float rot22;
        /// <summary> Rotation matrix - [3,2] value</summary>
        [XmlElement("Rot_3_2")]
        public float rot32;
        /// <summary> Rotation matrix - [1,3] value</summary>
        [XmlElement("Rot_1_3")]
        public float rot13;
        /// <summary> Rotation matrix - [2,3] value</summary>
        [XmlElement("Rot_2_3")]
        public float rot23;
        /// <summary> Rotation matrix - [3,3] value</summary>
        [XmlElement("Rot_3_3")]
        public float rot33;
    }

    /// <summary> Resolution (width/height). </summary>
    public struct sResolution
    {
        /// <summary> Width</summary>
        [XmlElement("Width")]
        public int width;
        /// <summary> Height</summary>
        [XmlElement("Height")]
        public int height;
    }

    /// <summary> Field of View</summary>
    public struct sFOV
    {
        /// <summary> Left</summary>
        [XmlElement("Left")]
        public int left;
        /// <summary> Top</summary>
        [XmlElement("Top")]
        public int top;
        /// <summary> Right</summary>
        [XmlElement("Right")]
        public int right;
        /// <summary> Bottom</summary>
        [XmlElement("Bottom")]
        public int bottom;
    }

    /// <summary> settings for Camera values (min,max and current). </summary>
    public struct sCameraSetting
    {
        /// <summary> Current value</summary>
        [XmlElement("Current")]
        public int current;
        /// <summary> Minimum value</summary>
        [XmlElement("Min")]
        public int min;
        /// <summary> Maximum value</summary>
        [XmlElement("Max")]
        public int max;
    }

    /// <summary> Settings regarding processing actions. </summary>
    public struct sSettingProccessingActions
    {
        /// <summary> Tracking processing action </summary>
        [XmlElement("Tracking")]
        public eProcessingActions actions;
        /// <summary> Twin system merge processing action status </summary>
        [XmlElement("TwinSystemMerge")]
        public bool twinSystemMerge;
        /// <summary> Spline Fill status </summary>
        [XmlElement("SplineFill")]
        public bool splineFill;
        /// <summary> AIM traciking processing status </summary>
        [XmlElement("AIM")]
        public bool aim;
        /// <summary> 6 DOF tracking processing status </summary>
        [XmlElement("Track6DOF")]
        public bool track6DOF;
        /// <summary> Force data status</summary>
        [XmlElement("ForceData")]
        public bool forceData;
        /// <summary> Export to TSV status</summary>
        [XmlElement("ExportTSV")]
        public bool exportTSV;
        /// <summary> Export to C3D status</summary>
        [XmlElement("ExportC3D")]
        public bool exportC3D;
        /// <summary> Export to Matlab status</summary>
        [XmlElement("ExportMatlabFile")]
        public bool exportMatlab;
    }

    /// <summary> Settings regarind external Time Base. </summary>
    public struct sSettingsGeneralExternalTimeBase
    {
        [XmlElement("Enabled")]
        public bool enabled;
        [XmlElement("Signal_Source")]
        public eSignalSource signalSource;
        [XmlElement("Signal_Mode")]
        public eSignalMode signalMode;
        [XmlElement("Frequency_Multiplier")]
        public int freqMultiplier;
        [XmlElement("Frequency_Divisor")]
        public int freqDivisor;
        [XmlElement("Frequency_Tolerance")]
        public int freqTolerance;
        [XmlElement("Nominal_Frequency")]
        public float nominalFrequency;
        [XmlElement("Signal_Edge")]
        public eSignalEdge signalEdge;
        [XmlElement("Signal_Shutter_Delay")]
        public int signalShutterDelay;
        [XmlElement("Non_Periodic_Timeout")]
        public float nonPeriodicTimeout;
    }

    /// <summary> settings for 6DOF bodies. </summary>
    public struct sSettings6DOF
    {
        /// <summary> Name of 6DOF body</summary>
        [XmlElement("Name")]
        public string name;
        /// <summary> Color of 6DOF body</summary>
        [XmlElement("RGBColor")]
        public int colorRGB;
        /// <summary> List of points in 6DOF body</summary>
        [XmlElement("Point")]
        public List<sPoint> sPoints;
    }

    /// <summary> General settings for Analog devices. </summary>
    public struct sAnalogDevice
    {
        /// <summary> Analog device ID</summary>
        [XmlElement("Device_ID")]
        public int deviceID;
        /// <summary> Analog device name</summary>
        [XmlElement("Device_Name")]
        public string deviceName;
        /// <summary> Number of channels in device</summary>
        [XmlElement("Channels")]
        public int channelCount;
        /// <summary> Frequency of channels </summary>
        [XmlElement("Frequency")]
        public int frequency;
        /// <summary> Range of channels </summary>
        [XmlElement("Range")]
        public sAnalogRange channelRange;
        /// <summary> Names of channels </summary>
        [XmlElement("Channel")]
        public List<sAnalogChannel> voLabels;
    }

    /// <summary> Analog range och channels. </summary>
    public struct sAnalogRange
    {
        /// <summary> Minimum value</summary>
        [XmlElement("Min")]
        public float min;
        /// <summary> Maximum value</summary>
        [XmlElement("Max")]
        public float max;
    }

    /// <summary> settings for Analog channel. </summary>
    public struct sAnalogChannel
    {
        /// <summary> Channel label</summary>
        [XmlElement("Label")]
        public string label;
        /// <summary> Unit used by channel </summary>
        [XmlElement("Unit")]
        public string unit;
    }

    /// <summary> Settings for Force plate. </summary>
    public struct sForcePlateSettings
    {
        /// <summary> ID of force plate</summary>
        [XmlElement("Plate_ID")]
        public int plateID;
        /// <summary> ID of analog device connected to force plate. 0 = no analog device associated with force plate</summary>
        [XmlElement("Analog_Device_ID")]
        public int analogDeviceID;
        /// <summary> Measurement frequency of analog device connected to force plate</summary>
        [XmlElement("Frequency")]
        public int frequency;
        /// <summary> Force plate type.</summary>
        [XmlElement("Type")]
        public string type;
        /// <summary> Name of force plate</summary>
        [XmlElement("Name")]
        public string name;
        /// <summary> Force plate length</summary>
        [XmlElement("Length")]
        public float length;
        /// <summary> Force plate width</summary>
        [XmlElement("Width")]
        public float width;
        /// <summary> four blocks with the corners of the force plate</summary>
        [XmlElement("Location")]
        public sLocation location;
        /// <summary> Force plate origin</summary>
        [XmlElement("Origin")]
        public sPoint origin;
        /// <summary> Analog channels connected to force plate</summary>
        [XmlArray("Channels")]
        public List<sForceChannel> channels;
        /// <summary> Calibration of the force plate</summary>
        [XmlElement("Calibration_Matrix")]
        public CalibrationMatrix CalibrationMatrix;

    }

    /// <summary> Struct with calibration matrix for force plate </summary>
    public struct CalibrationMatrix
    {
        [XmlElement("Row1")]
        public CalibrationRow row1;
        [XmlElement("Row2")]
        public CalibrationRow row2;
        [XmlElement("Row3")]
        public CalibrationRow row3;
        [XmlElement("Row4")]
        public CalibrationRow row4;
        [XmlElement("Row5")]
        public CalibrationRow row5;
        [XmlElement("Row6")]
        public CalibrationRow row6;
        [XmlElement("Row7")]
        public CalibrationRow row7;
        [XmlElement("Row8")]
        public CalibrationRow row8;
        [XmlElement("Row9")]
        public CalibrationRow row9;
        [XmlElement("Row10")]
        public CalibrationRow row10;
        [XmlElement("Row11")]
        public CalibrationRow row11;
        [XmlElement("Row12")]
        public CalibrationRow row12;

    }

    /// <summary> row for calibration matrix of force plates </summary>
    public struct CalibrationRow
    {
        [XmlElement("Col1")]
        public float col1;
        [XmlElement("Col2")]
        public float col2;
        [XmlElement("Col3")]
        public float col3;
        [XmlElement("Col4")]
        public float col4;
        [XmlElement("Col5")]
        public float col5;
        [XmlElement("Col6")]
        public float col6;
        [XmlElement("Col7")]
        public float col7;
        [XmlElement("Col8")]
        public float col8;
        [XmlElement("Col9")]
        public float col9;
        [XmlElement("Col10")]
        public float col10;
        [XmlElement("Col11")]
        public float col11;
        [XmlElement("Col12")]
        public float col12;
    }

    /// <summary> Settings for channel. </summary>
    [XmlType("Channel")]
    public struct sForceChannel
    {
        /// <summary> Channel number</summary>
        [XmlElement("Channel_No")]
        public int channelNumber;
        /// <summary> Conversion factor of channel</summary>
        [XmlElement("ConversionFactor")]
        public float conversionFactor;
    }

    /// <summary> Location for force plate. </summary>
    public struct sLocation
    {
        /// <summary> First corner</summary>
        [XmlElement("Corner1")]
        public sPoint corner1;
        /// <summary> Second corner</summary>
        [XmlElement("Corner2")]
        public sPoint corner2;
        /// <summary> Third corner</summary>
        [XmlElement("Corner3")]
        public sPoint corner3;
        /// <summary> Fourth corner</summary>
        [XmlElement("Corner4")]
        public sPoint corner4;
    }

    /// <summary> Settings for image from camera. </summary>
    public struct sImageCamera
    {
        /// <summary> ID of camera</summary>
        [XmlElement("ID")]
        public int cameraID;
        /// <summary> Image streaming on or off</summary>
        [XmlElement("Enabled")]
        public bool enabled;
        /// <summary> Format of image</summary>
        [XmlElement("Format")]
        public eImageFormat imageFormat;
        /// <summary> Image width</summary>
        [XmlElement("Width")]
        public int width;
        /// <summary> Image height</summary>
        [XmlElement("Height")]
        public int height;
        /// <summary> Left edge relative to original image </summary>
        [XmlElement("Left_Crop")]
        public float cropLeft;
        /// <summary> Top edge relative to original image </summary>
        [XmlElement("Top_Crop")]
        public float cropTop;
        /// <summary> Right edge relative to original image </summary>
        [XmlElement("Right_Crop")]
        public float cropRight;
        /// <summary> Bottom edge relative to original image </summary>
        [XmlElement("Bottom_Crop")]
        public float cropBottom;
    }

    /// <summary> settings for labeled marker. </summary>
    public struct sSettings3DLabel
    {
        /// <summary> Name of marker</summary>
        [XmlElement("Name")]
        public string name;
        /// <summary> Color of marker</summary>
        [XmlElement("RGBColor")]
        public int colorRGB;
    }

    #endregion


    public class RTProtocol
    {
        /// <summary> Constants relating to Protocol </summary>
        public static class Constants
        {
            /// <summary> Latest major version of protocol. </summary>
            public const int MAJOR_VERSION = 1;
            /// <summary> Latest minor version of protocol. </summary>
            public const int MINOR_VERSION = 12;
            /// <summary> Default value of big endianess in packets. </summary>
            public const bool BIG_ENDIAN = false;
            /// <summary> Maximum camera count. </summary>
            public const int MAX_CAMERA_COUNT = 256;
            /// <summary> Maximum Analog device count. </summary>
            public const int MAX_ANALOG_DEVICE_COUNT = 64;
            /// <summary> Maximum force plate count. </summary>
            public const int MAX_FORCE_PLATE_COUNT = 64;
            /// <summary> Size of all packet headers, packet size and packet type. </summary>
            public const int PACKET_HEADER_SIZE = 8;
            /// <summary> Size of data packet header, timestamp, frame number and component count. </summary>
            public const int DATA_PACKET_HEADER_SIZE = 16;
            /// <summary> Size of component header, component size and component type. </summary>
            public const int COMPONENT_HEADER = 8;
            /// <summary> Default base port used by QTM </summary>
            public const ushort STANDARD_BASE_PORT = 22222;
            /// <summary> Port QTM listens to for discovery requests</summary>
            public const ushort STANDARD_BROADCAST_PORT = 22226;

        }

		public delegate void ProcessStream(RTPacket packet);
        /// <summary> Callback for processing real time data packets </summary>
        public ProcessStream realTimeDataCallback;
        /// <summary> Callback for receiving events </summary>
		public ProcessStream eventDataCallback;

        Thread mProcessStreamthread;

        RTNetwork mNetwork;
        
        ushort mUDPport;

        RTPacket mPacket;
        /// <summary> Packet received from QTM </summary>
        public RTPacket Packet { get { return mPacket; } }

        int mMajorVersion;
        int mMinorVersion;
        bool mBigEndian;
        volatile bool mThreadActive;
        string mErrorString;

        SettingsGeneral mGeneralSettings;
        /// <summary> General settings from QTM </summary>
        public SettingsGeneral GeneralSettings { get { return mGeneralSettings; } }

        Settings3D m3DSettings;
        /// <summary> 3D settings from QTM </summary>
        public Settings3D Settings3D { get { return m3DSettings; } }

        Settings6D m6DOFSettings;
        /// <summary> 6DOF settings from QTM </summary>
        public Settings6D Settings6DOF { get { return m6DOFSettings; } }

        SettingsAnalog mAnalogSettings;
        /// <summary> Analog settings from QTM </summary>
        public SettingsAnalog AnalogSettings { get { return mAnalogSettings; } }

        SettingsForce mForceSettings;
        /// <summary> Force settings from QTM </summary>
        public SettingsForce ForceSettings { get { return mForceSettings; } }

        SettingsImage mImageSettings;
        /// <summary> Image settings from QTM </summary>
        public SettingsImage ImageSettings { get { return mImageSettings; } }

        bool mBroadcastSocketCreated;
        List<sDiscoveryResponse> mDiscoveryResponses;

        /// <summary> list of discovered QTM server possible to connect to </summary>
        public List<sDiscoveryResponse> DiscoveryResponses { get { return mDiscoveryResponses; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public RTProtocol(int majorVersion = Constants.MAJOR_VERSION, int minorVersion = Constants.MINOR_VERSION, bool bigEndian = Constants.BIG_ENDIAN)
        {
            mMajorVersion = majorVersion;
            mMinorVersion = minorVersion;
            mBigEndian = bigEndian;

            mPacket = new RTPacket(mMinorVersion, mMajorVersion, mBigEndian);
            mErrorString = "";

            mNetwork = new RTNetwork();
            mBroadcastSocketCreated = false;
            mDiscoveryResponses = new List<sDiscoveryResponse>();
        }

        /// <summary>
        /// Create connection to server
        /// </summary>
        /// <param name="serverAddr">Adress to server</param>
        /// <param name="serverPortUDP">port to use if UDP socket is desired, set to 0 for automatic port selection</param>
        /// <param name="majorVersion">Major protocol version to use, default is latest</param>
        /// <param name="minorVersion">Minor protocol version to use, default is latest</param>
        /// <param name="bigEndian">should byte order be big endian? default value no</param>
        /// <param name="port">base port for QTM server, default is 22222</param>
        /// <returns>true if connection was successful, otherwise false</returns>
        public bool connect(string serverAddr, short serverPortUDP = -1,
                            int majorVersion = Constants.MAJOR_VERSION, int minorVersion = Constants.MINOR_VERSION,
                            bool bigEndian = Constants.BIG_ENDIAN, int port = Constants.STANDARD_BASE_PORT)
        {
            disconnect();
            mMajorVersion = majorVersion;
            mMinorVersion = minorVersion;
            mBigEndian = bigEndian;

            ePacketType packetType;

            //increase port number with on or two depending on data requested
            if (mBigEndian)
                port += 2;
            else
                port += 1;

            mPacket = new RTPacket(majorVersion, minorVersion, bigEndian);

            if (mNetwork.connect(serverAddr, port))
            {
                if (serverPortUDP >= 0)
                {
                    mUDPport = (ushort)serverPortUDP;

                    if (mNetwork.createUDPSocket(ref mUDPport, false) == false)
                    {
                        mErrorString = String.Format("Error creating UDP socket: {0}", mNetwork.getErrorString());
                        disconnect();
                        return false;
                    }
                }

                //Get connection response from server
                if (receiveRTPacket(out packetType) > 0)
                {
                    if (packetType == ePacketType.kPacketError)
                    {
                        //Error from QTM
                        mErrorString = mPacket.getErrorString();
                        disconnect();
                        return false;
                    }

                    if (packetType == ePacketType.kPacketCommand)
                    {
                        string response = mPacket.getCommandString();
                        if (response == "QTM RT Interface connected")
                        {
                            if (setVersion(mMajorVersion, mMinorVersion))
                            {
                                string expectedResponse = String.Format("Version set to {0}.{1}", mMajorVersion, mMinorVersion);
                                response = mPacket.getCommandString();
                                if (response == expectedResponse)
                                {
                                    if (mMajorVersion == 1 && mMinorVersion == 0)
                                    {
                                        /*
                                        if (setByteOrder(mBigEndian))
                                        {
                                            return true;
                                        }
                                        else
                                        {
                                            //Error setting byte order
                                            mErrorString = "Error setting byte order";
                                            disconnect();
                                            return false;
                                        }
                                         */
                                    }
                                    return true;
                                }
                                else
                                {
                                    mErrorString = "Unexpected response from server";
                                    disconnect();
                                    return false;
                                }
                            }
                            else
                            {
                                //Error setting version
                                mErrorString = "Error setting version of protocol";
                                disconnect();
                                return false;
                            }
                        }
                        else
                        {
                            //missing QTM response
                            mErrorString = "Missing response from QTM Server";
                            disconnect();
                            return false;
                        }
                    }
                }
                else
                {
                    //Error receiving packet.
                    mErrorString = String.Format("Error Recieveing packet: {0}", mNetwork.getErrorString());
                    disconnect();
                    return false;
                }
            }
            else
            {
                if (mNetwork.getError() == SocketError.ConnectionRefused)
                {
                    mErrorString = "Connection refused, Check if QTM is running on target machine";
                    disconnect();
                }
                else
                {
                    mErrorString = String.Format("Error connecting TCP socket: {0}", mNetwork.getErrorString());
                    disconnect();
                }
                return false;

            }
            return false;
        }

        /// <summary>
        /// Create connection to server
        /// </summary>
        /// <param name="host">host detected via broadcast discovery</param>
        /// <param name="serverPortUDP">port to use if UDP socket is desired, set to 0 for automatic port selection</param>
        /// <param name="majorVersion">Major protocol version to use, default is latest</param>
        /// <param name="minorVersion">Minor protocol version to use, default is latest</param>
        /// <param name="bigEndian">should byte order be big endian? default value no</param>
        /// <returns>true if connection was successful, otherwise false</returns>
        public bool connect(sDiscoveryResponse host, short serverPortUDP = -1,
                            int majorVersion = Constants.MAJOR_VERSION, int minorVersion = Constants.MINOR_VERSION,
                            bool bigEndian = Constants.BIG_ENDIAN)
        {
            return connect(host.ipAddress, serverPortUDP, majorVersion, minorVersion, bigEndian, host.port);
        }

        /// <summary>
        /// Creates an UDP socket
        /// </summary>
        /// <param name="udpPort">Port to listen to. </param>
        /// <param name="broadcast">if port should be able to send broadcast packets</param>
        /// <returns></returns>
        public bool createUDPSocket(ref ushort udpPort, bool broadcast = false)
        {
            return mNetwork.createUDPSocket(ref udpPort, broadcast);
        }

        /// <summary>
        /// Disconnect sockets from server
        /// </summary>
        public void disconnect()
        {
            mBroadcastSocketCreated = false;
            if (mProcessStreamthread != null)
            {
                mProcessStreamthread.Abort();
                mProcessStreamthread = null;
            }
            mNetwork.disconnect();
        }

        /// <summary>
        /// Check it our TCP is connected
        /// </summary>
        /// <returns>connection status of TCP socket </returns>
        public bool connected()
        {
            return mNetwork.connected();
        }

        /// <summary>
        /// Receive data from sockets and save to protocol packet.
        /// </summary>
        /// <param name="packetType">type of packet received from sockets. </param>
        /// <returns>number of bytes received</returns>
        public int receiveRTPacket(out ePacketType packetType)
        {
            byte[] data = new byte[65535];
            int recvBytes = mNetwork.receive(ref data);
            if (recvBytes > 0)
            {
                mPacket.setData(data);
                packetType = mPacket.PacketType;
            }
            else
            {
                packetType = ePacketType.PacketNone;
            }

            return recvBytes;
        }

        /// <summary>
        /// Send discovery packet to network to find available QTM Servers.
        /// </summary>
        /// <param name="replyPort">port for servers to reply.</param>
        /// <param name="discoverPort">port to send discovery packet.</param>
        /// <returns>true if discovery packet was sent successfully</returns>
        public bool discoverRTServers(ushort replyPort, ushort discoverPort = Constants.STANDARD_BROADCAST_PORT)
        {
            byte[] port = BitConverter.GetBytes(replyPort);
            byte[] size = BitConverter.GetBytes(10);
            byte[] cmd = BitConverter.GetBytes((int)ePacketType.kPacketDiscover);

            Array.Reverse(port);

            List<byte> b = new List<byte>();
            b.AddRange(size);
            b.AddRange(cmd);
            b.AddRange(port);

            byte[] msg = b.ToArray();
            bool status = false;

            //if we don't have a udp broadcast socket, create one
            if (mBroadcastSocketCreated || mNetwork.createUDPSocket(ref replyPort, true))
            {
                mBroadcastSocketCreated = true;
                status = mNetwork.sendUDPBroadcast(msg, 10);

                mDiscoveryResponses.Clear();

                int receieved = 0;
                ePacketType packetType;
                do
                {
                     receieved = receiveRTPacket(out packetType);
                     if (packetType == ePacketType.kPacketCommand)
                     {
                        sDiscoveryResponse response;
                        if (mPacket.getDiscoverData(out response))
                        {
                            mDiscoveryResponses.Add(response);
                        }
                    }
                }
                while (receieved > 0);
            }

            return status;
        }

        /// <summary>
        /// tell protocol to start a new thread that listens to real time stream and send data to callback functions
        /// </summary>
        /// <returns>always returns true</returns>
        public bool listenToStream()
        {
            mProcessStreamthread = new Thread(threadedStreamFunction);
            mThreadActive = true;
            mProcessStreamthread.Start();
            return true;
        }

        /// <summary>
        /// Function used in thread to listen to real time data stream.
        /// </summary>
        private void threadedStreamFunction()
        {
            ePacketType packetType;

            while (mThreadActive)
            {
				receiveRTPacket(out packetType);

				if (mPacket != null)
                {
					if (packetType == ePacketType.kPacketData)
					{
                        if (realTimeDataCallback != null)
    						realTimeDataCallback(mPacket);
					}
					else if (packetType == ePacketType.kPacketEvent)
					{
                        if (eventDataCallback != null)
						    eventDataCallback(mPacket);
					}

                }

            }
        }

        /// <summary>
        /// Tell protocol to stop listening to stream and stop the thread.
        /// </summary>
        public void stopStreamListen()
        {
			if (mProcessStreamthread != null)
			{
                mThreadActive = false;
				mProcessStreamthread.Join();
			}
        }

        #region get set functions

        /// <summary>
        /// Get protocol version used from QTM server
        /// </summary>
        /// <param name="majorVersion">Major version of protocol used</param>
        /// <param name="minorVersion">Minor version of protocol used</param>
        /// <returns>true if command and response was successful</returns>
        public bool getVersion(out int majorVersion, out int minorVersion)
        {
            if (sendCommand("Version"))
            {
                ePacketType responsePacket = mPacket.PacketType;
                if (responsePacket != ePacketType.kPacketError)
                {
                    string versionString = mPacket.getCommandString();
                    versionString = versionString.Substring(11);
                    Version ver = new Version(versionString);
                    majorVersion = ver.Major;
                    minorVersion = ver.Minor;
                    return true;
                }
            }
            majorVersion = 0;
            minorVersion = 0;
            return false;
        }

        /// <summary>
        /// Set what version QTM server should use
        /// </summary>
        /// <param name="majorVersion">Major version of protocol used</param>
        /// <param name="minorVersion">Minor version of protocol used</param>
        /// <returns>true if command was successful</returns>
        public bool setVersion(int majorVersion, int minorVersion)
        {

            if (majorVersion < 0 || majorVersion > Constants.MAJOR_VERSION || minorVersion < 0)
            {
                mErrorString = "Incorrect version of protcol";
                return false;
            }

            if (sendCommand("version " + majorVersion + "." + minorVersion))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Ask QTM server what version is used.
        /// </summary>
        /// <param name="version">what version server uses</param>
        /// <returns>true if command was sent successfully</returns>
        public bool getQTMVersion(out string version)
        {
            if (sendCommand("QTMVersion"))
            {
				ePacketType responsePacketType = mPacket.PacketType;
                if (responsePacketType == ePacketType.kPacketCommand)
                {
                    version = mPacket.getCommandString();
					return true;
                }
            }
            version = "";
            return false;
        }

        /// <summary>
        /// Get byte order used by server
        /// </summary>
        /// <param name="bigEndian">response from server if it uses big endian or not</param>
        /// <returns>true if command was sent successfully</returns>
        public bool getByteOrder(out bool bigEndian)
        {
			if (sendCommand("ByteOrder"))
			{
				ePacketType responsePacketType = mPacket.PacketType;
				if (responsePacketType == ePacketType.kPacketCommand)
				{
					string response = mPacket.getCommandString();
					if (response == "Byte order is big endian")
						bigEndian = true;
					else
						bigEndian = false;
					return true;
				}
			}
            bigEndian = false;
            return false;
        }

        /// <summary>
        /// Check license towards QTM Server
        /// </summary>
        /// <param name="licenseCode">license code to check</param>
        /// <returns>true if command was successfully sent AND License passed, otherwise false</returns>
        public bool checkLicense(string licenseCode)
        {
			if (sendCommand("CheckLicense " + licenseCode))
			{
				ePacketType responsePacketType = mPacket.PacketType;
				if (responsePacketType == ePacketType.kPacketCommand)
				{
					string response = mPacket.getCommandString();
					if (response == "License pass")
					{
						return true;

					}
					else
					{
						mErrorString = "Wrong license code.";
						return false;
					}
				}
			}
            return false;
        }

        /// <summary>
        /// Get current frame from server
        /// </summary>
        /// <param name="streamAll">boolean if all component types should be streamed</param>
        /// <param name="components">list of specific component types to stream, ignored if streamAll is set to true</param>
        /// <returns>true if command was sent successfully and response was a datapacket with frame</returns>
        public bool getCurrentFrame(bool streamAll, List<eComponentType> components = null)
        {
            string command = "getCurrentFrame";

            if (streamAll)
                command += " all";
            else
                command += BuildStreamString(components);

            if (sendCommand(command))
            {
                ePacketType responsePacketType = mPacket.PacketType;
                if (responsePacketType == ePacketType.kPacketData)
                {
                    return true;
                }
                else if (responsePacketType == ePacketType.kPacketNoMoreData)
                {
                    mErrorString = "No data available";
                    return false;
                }
                else
                {
                    mErrorString = mPacket.getErrorString();
                    return false;

                }
            }
            return false;
        }

        /// </summary>
        /// Get current frame from server
        /// </summary>
        /// <param name="streamAll">boolean if all component types should be streamed</param>
        /// <param name="packet">packet with data returned from server</param>
        /// <param name="components">list of specific component types to stream, ignored if streamAll is set to true</param>
        /// <returns>true if command was sent successfully and response was a datapacket with frame</returns>
        public bool getCurrentFrame(out RTPacket packet,bool streamAll, List<eComponentType> components = null)
        {
            bool status;
            if (components != null)
            {
                status = getCurrentFrame(streamAll, components);
            }
            else
            {
                status = getCurrentFrame(streamAll);
            }

            if (status)
            {
                packet = mPacket;
                return true;
            }
            else
            {
                packet = RTPacket.ErrorPacket;
                return false;
            }
        }

        /// <summary>
        /// Stream frames from QTM server
        /// </summary>
        /// <param name="streamRate">what rate server should stream at</param>
        /// <param name="streamValue">related to streamrate, not used if all frames are streamed</param>
        /// <param name="streamAllComponents">If all component types should be streamed</param>
        /// <param name="components">List of all component types deisred to stream</param>
        /// <param name="port">if set, streaming will be done by UDP on this port. Has to be set if ipadress is specified</param>
        /// <param name="ipAdress">if UDP streaming should occur to other ip adress,
        /// if not set streaming occurs on same ip as command came from</param>
        /// <returns></returns>
        public bool streamFrames(eStreamRate streamRate, int streamValue,
                                bool streamAllComponents, List<eComponentType> components = null,
                                short port = -1, string ipAdress = "")
        {
            string command = "streamframes";

            switch(streamRate)
            {
                case eStreamRate.kRateAllFrames:
                    command += " allFrames";
                    break;
                case eStreamRate.kRateFrequency:
                    command += " Frequency:" + streamValue;
                    break;
                case eStreamRate.kRateFrequnecyDivisor:
                    command += " FrequencyDivisor:" + streamValue;
                    break;
            }

            if (ipAdress != "")
            {
                if (port > 0)
                {
                    command += " UDP:" + ipAdress + ":" + port;
                }
                else
                {
                    mErrorString = "If an IP-adress was specified for UDP streaming, a port must be specified aswell";
                    return false;
                }
            }
            else if (port > 0)
            {
                command += " UDP:" + port;
            }

            if (streamAllComponents)
                command += " all";
            else
                command += BuildStreamString(components);

            return sendCommand(command);
        }


        public bool streamFrames(eStreamRate streamRate, int streamValue,
                                bool streamAllComponents, eComponentType component,
                                short port = -1, string ipAdress = "")
        {
            List<eComponentType> list = new List<eComponentType>();
            list.Add(component);
            return streamFrames(streamRate, streamValue, streamAllComponents, list, port, ipAdress);
        }

        /// <summary>
        /// Tell QTM Server to stop streaming frames
        /// </summary>
        /// <returns>true if command was sent successfully</returns>
        public bool streamFramesStop()
        {
            if (sendCommand("StreamFrames Stop"))
            {
                ePacketType responsePacket;
                receiveRTPacket(out responsePacket);
                if (responsePacket == ePacketType.kPacketError)
                {
                    mErrorString = mPacket.getErrorString();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get latest event from QTM server
        /// </summary>
        /// <param name="respondedEvent">even from qtm</param>
        /// <returns>true if command was sent successfully</returns>
        public bool getState(out eEvent respondedEvent)
        {
            if (sendCommand("GetState"))
            {
                respondedEvent = mPacket.getEvent();
                return true;
            }
            respondedEvent = eEvent.kEventNone;
            return false;
        }

        /// <summary>
        /// NOT IMPLEMENTED
        /// </summary>
        /// <returns>false</returns>
        public bool getCapture()
        {
            // TODO:::
            return false;
        }

        /// <summary>
        /// Send trigger to QTM server
        /// </summary>
        /// <returns>True if command and trigger was received successfully</returns>
        public bool sendTrigger()
        {
            if (sendCommand("Trig"))
            {
                if (mPacket.getCommandString() == "Trig ok")
                {
                    return true;
                }
                else
                {
                    mErrorString = mPacket.getCommandString();
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Set an event in QTM.
        /// </summary>
        /// <param name="label">label of event</param>
        /// <returns>true if event was set successfully</returns>
        public bool setQTMEvent(string label)
        {
            if (sendCommand("setQTMEvent " + label))
            {
                string response = mPacket.getCommandString();
                if (response == "Event set")
                {
                    return true;
                }
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Take control over QTM
        /// </summary>
        /// <param name="password">Password set in client</param>
        /// <returns>True if you become the master</returns>
        public bool takeControl(string password = "")
        {
            if (sendCommand("TakeControl " + password))
            {
                string response = mPacket.getCommandString();
                if (response == "You are now master")
                {
                    return true;
                }
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Release master control over QTM
        /// </summary>
        /// <returns>true if control was released or if client already is a regular client</returns>
        public bool releaseControl()
        {
            if (sendCommand("releaseControl"))
            {
                string response = mPacket.getCommandString();
                if (response == "You are now a regular client" || response == "You are already a regular client")
                {
                    return true;
                }
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Create a new measurement in QTM, connect to the cameras and enter RT (preview) mode.
        /// Needs to have control over QTM for this command to work
        /// </summary>
        /// <returns></returns>
        public bool newMeasurement()
        {
            if (sendCommand("New"))
            {
                string response = mPacket.getCommandString();
                if (response == "Creating new connection" || response == "Already connected")
                {
                    return true;
                }
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Close the current measurement in QTM.
        /// Needs to have control over QTM for this command to work
        /// </summary>
        /// <returns>returns true if measurement was closed or if there was nothing to close</returns>
        public bool closeMeasurement()
        {
            if (sendCommand("Close"))
            {
                string response = mPacket.getCommandString();
                if (response == "Closing connection" || response == "No connection to close")
                {
                    return true;
                }
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Start capture in QTM.
        /// Needs to have control over QTM for this command to work
        /// </summary>
        /// <returns>true if measurement was started</returns>
        public bool startCapture(bool RTFromFile = false)
        {
            string command = (RTFromFile) ? "Start rtfromfile" : "Start";
            if (sendCommand(command))
            {
                string response = mPacket.getCommandString();
                if (response == "Starting measurement")
                {
                    return true;
                }
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Stop current measurement in QTM.
        /// Needs to have control over QTM for this command to work
        /// </summary>
        /// <returns>true if measurement was stopped</returns>
        public bool stopCapture()
        {
            if (sendCommand("Stop"))
            {
                string response = mPacket.getCommandString();
                if (response == "Stopping measurement")
                {
                    return true;
                }
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Load capture at path filename, both relative and absolute path works
        /// Needs to have control over QTM for this command to work
        /// </summary>
        /// <param name="filename">filename to load</param>
        /// <returns>true if measurement was loaded</returns>
        public bool loadCapture(string filename)
        {
            if (sendCommand("Load " + filename))
            {
                string response = mPacket.getCommandString();
                if (response == "Measurement loaded")
                {
                    return true;
                }
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Save capture with filename.
        /// Needs to have control over QTM for this command to work
        /// </summary>
        /// <param name="filename">filename to save at.</param>
        /// <param name="overwrite">if QTM is allowed to override existing files.</param>
        /// <param name="newFilename">if QTM is not allowed to overwrite, the new filename will be sent back </param>
        /// <returns>true if measurement was saved.</returns>
        public bool saveCapture(string filename, bool overwrite, ref string newFilename)
        {
            string command = "Save " + filename;
            if (overwrite)
                command += " overwrite";
            if (sendCommand(command))
            {
                string response = mPacket.getCommandString();
                if (response.Contains("Measurement saved"))
                {
                    if(response.Contains("Measurement saved"))
                    {
                        Regex pattern = new Regex("'.*'$");
                        Match match = pattern.Match(response);
                        newFilename = match.Value.Replace("'",""); 
                    }
                    return true;
                }
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// load project at path
        /// Needs to have control over QTM for this command to work
        /// </summary>
        /// <param name="projectPath">path to project to load</param>
        /// <returns>true if project was loaded</returns>
        public bool loadProject(string projectPath)
        {
            if (sendCommand("LoadProject " + projectPath ))
            {
                string response = mPacket.getCommandString();
                if (response.Contains("Project loaded"))
                {
                    return true;
                }
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Get general settings from QTM Server and saves data in protocol
        /// </summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool getGeneralSettings()
        {
            if (sendCommand("GetParameters general"))
            {
                string xmlString = mPacket.getXMLString();
                mGeneralSettings = readGeneralSettings(xmlString);
                if (mGeneralSettings != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get general settings from QTM Server
        /// </summary>
        /// <param name="packet">packet with </param>
        /// <returns></returns>
        //public bool getGeneralSettings(out SettingsGeneral settings)
        //{
        //    if (getGeneralSettings())
        //    {
        //        settings = mGeneralSettings;
        //        return true;
        //    }
        //    else
        //    {
        //        settings = null;
        //        return false;
        //    }
        //}

        /// <summary>
        /// Get 3D settings from QTM Server
        /// </summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool get3Dsettings()
        {
            if (sendCommand("GetParameters 3D"))
            {
                string xmlString = mPacket.getXMLString();
                m3DSettings = read3DSettings(xmlString);
                if (m3DSettings != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get 6DOF settings from QTM Server
        /// </summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool get6DSettings()
        {
            if (sendCommand("GetParameters 6D"))
            {
                string xmlString = mPacket.getXMLString();
                m6DOFSettings = read6DOFSettings(xmlString);
                if (m6DOFSettings != null)
                    return true;

            }
            return false;
        }

        /// <summary>
        /// Get Analog settings from QTM Server
        /// </summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool getAnalogSettings()
        {
            if (sendCommand("GetParameters Analog"))
            {
                string xmlString = mPacket.getXMLString();
                mAnalogSettings = readAnalogSettings(xmlString);
                if (mAnalogSettings != null)
                    return true;

            }
            return false;
        }

        /// <summary>
        /// Get Force settings from QTM Server
        /// </summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool getForceSettings()
        {
            if (sendCommand("GetParameters force"))
            {
                string xmlString = mPacket.getXMLString();
                mForceSettings = readForceSettings(xmlString);
                if (mForceSettings != null)
                    return true;

            }
            return false;
        }

        /// <summary>
        /// Get Image settings from QTM Server
        /// </summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool getImageSettings()
        {
            if (sendCommand("GetParameters Image"))
            {
                string xmlString = mPacket.getXMLString();
                mImageSettings = readImageSettings(xmlString);
                if (mImageSettings != null)
                    return true;

            }
            return false;
        }
        #endregion

        #region read settings
        /// <summary>
        /// Read general settings from XML string
        /// </summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with general settings from QTM</returns>
        public static SettingsGeneral readGeneralSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False","false").Replace("None","-1");

            XmlSerializer serializer = new XmlSerializer(typeof(SettingsGeneral));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("General");
            SettingsGeneral settings;
            try
            {
                settings = (SettingsGeneral)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();

            return settings;
        }

        /// <summary>
        /// Read 3D settings from XML string
        /// </summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with data of 3D settings from QTM</returns>
        public static Settings3D read3DSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1");

            XmlSerializer serializer = new XmlSerializer(typeof(Settings3D));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("The_3D");
            Settings3D settings;
            try
            {
                settings = (Settings3D)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();

            return settings;
        }

        /// <summary>
        /// Read 6DOFsettings from XML string
        /// </summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with data of 6DOF settings from QTM</returns>
        public static Settings6D read6DOFSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1");

            XmlSerializer serializer = new XmlSerializer(typeof(Settings6D));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("The_6D");
            Settings6D settings;
            try
            {
                settings = (Settings6D)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();

            return settings;
        }

        /// <summary>
        /// Read Analog settings from XML string
        /// </summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with data of Analog settings from QTM</returns>
        public static SettingsAnalog readAnalogSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1");

            XmlSerializer serializer = new XmlSerializer(typeof(SettingsAnalog));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("Analog");
            SettingsAnalog settings;
            try
            {
                settings = (SettingsAnalog)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();

            return settings;
        }

        /// <summary>
        /// Read Force settings from XML string
        /// </summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with data of Force settings from QTM</returns>
        public static SettingsForce readForceSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1");

            XmlSerializer serializer = new XmlSerializer(typeof(SettingsForce));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("Force");
            SettingsForce settings;
            try
            {
                settings = (SettingsForce)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();

            return settings;
        }

        /// <summary>
        /// Read Image settings from XML string
        /// </summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with data of Image settings from QTM</returns>
        public static SettingsImage readImageSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1");

            XmlSerializer serializer = new XmlSerializer(typeof(SettingsImage));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("Image");
            SettingsImage settings;
            try
            {
                settings = (SettingsImage)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();


            return settings;
        }
        #endregion

        #region set settings

        /// <summary>
        /// Creates xml string from the general settings to send to QTM
        /// </summary>
        /// <param name="generalSettings">generl settings to generate string from</param>
        /// <param name="setProcessingActions">if string should include processing actions or not</param>
        /// <returns>generated xml string from settings</returns>
        public static string setGeneralSettings(SettingsGeneral generalSettings, bool setProcessingActions = false)
        {
            StringWriter xmlString = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter xmlWriter = XmlWriter.Create(xmlString, settings);

            xmlWriter.WriteStartElement("QTM_Settings");
            {
                xmlWriter.WriteStartElement("General");
                {
                    xmlWriter.WriteElementString("Frequency", generalSettings.captureFrequency.ToString());
                    xmlWriter.WriteElementString("Capture_Time", generalSettings.captureTime.ToString("0.000"));
                    xmlWriter.WriteElementString("Start_On_ExternalTrigger", generalSettings.startOnExternalTrigger.ToString());
                    if (setProcessingActions)
                    {
                        xmlWriter.WriteStartElement("Processing_Actions");
                        switch (generalSettings.processingActions.actions)
                        {
                            case eProcessingActions.kProcessingTracking2D:
                                xmlWriter.WriteElementString("Tracking", "2D");
                                break;
                            case eProcessingActions.kProcessingTracking3D:
                                xmlWriter.WriteElementString("Tracking", "3D");
                                break;
                            default:
                                xmlWriter.WriteElementString("Tracking", "False");
                                break;
                        }

                        xmlWriter.WriteElementString("TwinSystemMerge", generalSettings.processingActions.twinSystemMerge.ToString());
                        xmlWriter.WriteElementString("SplineFill", generalSettings.processingActions.splineFill.ToString());
                        xmlWriter.WriteElementString("AIM", generalSettings.processingActions.aim.ToString());
                        xmlWriter.WriteElementString("Track6DOF", generalSettings.processingActions.track6DOF.ToString());
                        xmlWriter.WriteElementString("ForceData", generalSettings.processingActions.forceData.ToString());
                        xmlWriter.WriteElementString("ExportTSV", generalSettings.processingActions.exportTSV.ToString());
                        xmlWriter.WriteElementString("ExportC3D", generalSettings.processingActions.exportC3D.ToString());
                        xmlWriter.WriteElementString("ExportMatlabFile", generalSettings.processingActions.exportMatlab.ToString());
                        xmlWriter.WriteEndElement();

                    }
                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            return xmlString.ToString();
        }

        /// <summary>
        /// Creates xml string from the given settings to send to QTM
        /// </summary>
        /// <param name="sSettingsGeneralExternalTimeBase">time base settings to generate string from</param>
        /// <returns>generated xml string from settings</returns>
        public string setGeneralExtTimeBase(sSettingsGeneralExternalTimeBase timeBaseSettings)
        {
            StringWriter xmlString = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter xmlWriter = XmlWriter.Create(xmlString, settings);

            xmlWriter.WriteStartElement("QTM_Settings");
            {
                xmlWriter.WriteStartElement("General");
                {
                    xmlWriter.WriteStartElement("External_Time_Base");
                    {
                        xmlWriter.WriteElementString("Enabled", timeBaseSettings.enabled.ToString());
                        switch(timeBaseSettings.signalSource)
                        {
                            case eSignalSource.kSourceControlPort:
                                xmlWriter.WriteElementString("Signal_Source", "Control port");
                                break;
                            case eSignalSource.kSourceIRReceiver:
                                xmlWriter.WriteElementString("Signal_Source", "IR receiver");
                                break;
                            case eSignalSource.kSourceSMPTE:
                                xmlWriter.WriteElementString("Signal_Source", "SMPTE");
                                break;
                            case eSignalSource.kSourceVideoSync:
                                xmlWriter.WriteElementString("Signal_Source", "Video sync");
                                break;
                        }

                        if (timeBaseSettings.signalMode == eSignalMode.kPeriodic)
                        {
                            xmlWriter.WriteElementString("Signal_Mode", "True");
                        }
                        else
                        {
                            xmlWriter.WriteElementString("Signal_Mode", "False");
                        }

                        xmlWriter.WriteElementString("Frequency_Multiplier", timeBaseSettings.freqMultiplier.ToString());
                        xmlWriter.WriteElementString("Frequency_Divisor", timeBaseSettings.freqDivisor.ToString());
                        xmlWriter.WriteElementString("Frequency_Tolerance", timeBaseSettings.freqTolerance.ToString());


                        if (timeBaseSettings.nominalFrequency > 0 )
                        {
                            xmlWriter.WriteElementString("Nominal_Frequency", timeBaseSettings.nominalFrequency.ToString("0.000"));
                        }
                        else
                        {
                            xmlWriter.WriteElementString("Nominal_Frequency", "None");
                        }

                        switch(timeBaseSettings.signalEdge)
                        {
                            case eSignalEdge.kNegative:
                                xmlWriter.WriteElementString("Signal_Edge", "Negative");
                                break;
                            case eSignalEdge.kPositive:
                                xmlWriter.WriteElementString("Signal_Edge", "Positive");
                                break;
                        }

                        xmlWriter.WriteElementString("Signal_Shutter_Delay", timeBaseSettings.signalShutterDelay.ToString());
                        xmlWriter.WriteElementString("Non_Periodic_Timeout", timeBaseSettings.nominalFrequency.ToString("0.000"));

                    }
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            return xmlString.ToString();
        }

        /// <summary>
        /// Creates xml string from the given settings to send to QTM
        /// </summary>
        /// <param name="sSettingsGeneralCamera">Camera settings to generate string from. if camera ID is < 0, setting will be applied to all cameras</param>
        /// <returns>generated xml string from settings</returns>
        public string setGeneralCamera(sSettingsGeneralCamera cameraSettings)
        {
            StringWriter xmlString = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter xmlWriter = XmlWriter.Create(xmlString, settings);

            xmlWriter.WriteStartElement("QTM_Settings");
            {
                xmlWriter.WriteStartElement("General");
                {
                    xmlWriter.WriteStartElement("Camera");
                    {
                        xmlWriter.WriteElementString("ID", cameraSettings.cameraID.ToString());
                        switch(cameraSettings.eMode)
                        {
                            case eCameraMode.kModeMarker:
                                xmlWriter.WriteElementString("Mode", "Marker");
                                break;
                            case eCameraMode.kModeMarkerIntensity:
                                xmlWriter.WriteElementString("Mode", "Marker Intensity");
                                break;
                            case eCameraMode.kModeVideo:
                                xmlWriter.WriteElementString("Mode", "Video");
                                break;
                        }
                        xmlWriter.WriteElementString("Video_Exposure", cameraSettings.videoExposure.ToString());
                        xmlWriter.WriteElementString("Video_Flash_Time", cameraSettings.videoFlashTime.ToString());
                        xmlWriter.WriteElementString("Marker_Exposure", cameraSettings.markerExposure.ToString());
                        xmlWriter.WriteElementString("Marker_Threshold", cameraSettings.markerThreshold.ToString());
                        xmlWriter.WriteElementString("Orientation", cameraSettings.orientation.ToString());
                    }
                    xmlWriter.WriteEndElement();

                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            return xmlString.ToString();
        }

        /// <summary>
        /// create xml string for sync settings for camera
        /// </summary>
        /// <param name="sSettingsGeneralCamera">Camera settings to generate string from. if camera ID is < 0, setting will be applied to all cameras</param>
        /// <param name="syncSettings">settings to generate string from</param>
        /// <returns>generated xml string from settings</returns>
        public string setGeneralCameraSyncOut(int cameraID, sSync syncSettings)
        {
            StringWriter xmlString = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter xmlWriter = XmlWriter.Create(xmlString, settings);

            xmlWriter.WriteStartElement("QTM_Settings");
            {
                xmlWriter.WriteStartElement("General");
                {
                    xmlWriter.WriteStartElement("Camera");
                    {
                        xmlWriter.WriteElementString("ID", cameraID.ToString());

                        xmlWriter.WriteStartElement("Sync_Out");
                        {
                            switch (syncSettings.syncMode)
                            {
                                case eSyncOutFreqMode.kModeShutterOut:
                                    xmlWriter.WriteElementString("Mode", "Shutter out");
                                    break;
                                case eSyncOutFreqMode.kModeMultiplier:
                                    xmlWriter.WriteElementString("Mode", "Multiplier");
                                    xmlWriter.WriteElementString("Value", syncSettings.syncValue.ToString());
                                    break;
                                case eSyncOutFreqMode.kModeDivisor:
                                    xmlWriter.WriteElementString("Mode", "Divisor");
                                    xmlWriter.WriteElementString("Value", syncSettings.syncValue.ToString());
                                    break;
                                case eSyncOutFreqMode.kModeActualFreq:
                                    xmlWriter.WriteElementString("Mode", "Camera independent");
                                    break;
                                case eSyncOutFreqMode.kModeActualMeasurementTime:
                                    xmlWriter.WriteElementString("Mode", "Measurement time");
                                    xmlWriter.WriteElementString("Value", syncSettings.syncValue.ToString());
                                    break;
                                case eSyncOutFreqMode.kModeFixed100Hz:
                                    xmlWriter.WriteElementString("Mode", "Continuous 100Hz");
                                    break;
                            }
                            if (syncSettings.syncMode != eSyncOutFreqMode.kModeSRAMWireSync || syncSettings.syncMode != eSyncOutFreqMode.kModeFixed100Hz)
                            {
                                switch (syncSettings.signalPolarity)
                                {
                                    case eSignalPolarity.kNegative:
                                        xmlWriter.WriteElementString("Signal_Polarity", "Negative");
                                        break;
                                    case eSignalPolarity.kPositive:
                                        xmlWriter.WriteElementString("Signal_Polarity", "Positive");
                                        break;
                                }

                            }
                        }
                        xmlWriter.WriteEndElement();

                    }
                    xmlWriter.WriteEndElement();

                }
                xmlWriter.WriteEndElement();

            }
            xmlWriter.WriteEndElement();

            return xmlString.ToString();
        }

        /// <summary>
        /// create xml string for image settings
        /// </summary>
        /// <param name="sImageCamera">Image settings to generate string from</param>
        /// <returns>generated xml string from settings</returns>
        public string setImageSettings(sImageCamera imageSettings)
        {
            StringWriter xmlString = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter xmlWriter = XmlWriter.Create(xmlString, settings);
            xmlWriter.WriteStartElement("QTM_Settings");
            {
                xmlWriter.WriteStartElement("Image");
                {
                    xmlWriter.WriteStartElement("Camera");
                    {
                        xmlWriter.WriteElementString("ID", imageSettings.cameraID.ToString());
                        xmlWriter.WriteElementString("Enabled", imageSettings.enabled.ToString());

                        switch(imageSettings.imageFormat)
                        {
                            case eImageFormat.kFormatRawGrayScale:
                                xmlWriter.WriteElementString("Mode", "RAWGrayscale");
                                break;
                            case eImageFormat.kFormatRawBGR:
                                xmlWriter.WriteElementString("Mode", "RAWBGR");
                                break;
                            case eImageFormat.kFormatJPG:
                                xmlWriter.WriteElementString("Mode", "JPG");
                                break;
                            case eImageFormat.kFormatPNG:
                                xmlWriter.WriteElementString("Mode", "PNG");
                                break;
                        }

                        xmlWriter.WriteElementString("Format", ((int)imageSettings.imageFormat).ToString());

                        xmlWriter.WriteElementString("Width", imageSettings.width.ToString());
                        xmlWriter.WriteElementString("Height", imageSettings.height.ToString());

                        xmlWriter.WriteElementString("Left_Crop", imageSettings.cropLeft.ToString());
                        xmlWriter.WriteElementString("Top_Crop", imageSettings.cropRight.ToString());
                        xmlWriter.WriteElementString("Right_Crop", imageSettings.cropTop.ToString());
                        xmlWriter.WriteElementString("Bottom_Crop", imageSettings.cropBottom.ToString());

                    }
                    xmlWriter.WriteEndElement();

                }
                xmlWriter.WriteEndElement();

            }
            xmlWriter.WriteEndElement();

            return xmlString.ToString();
        }

        /// <summary>
        /// create xml string for force plate settings
        /// </summary>
        /// <param name="plateSettings">force plate settings to generate string from</param>
        /// <returns>generated xml string from settings</returns>
        public string setForceSettings(sForcePlateSettings plateSettings)
        {
            StringWriter xmlString = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter xmlWriter = XmlWriter.Create(xmlString, settings);
            xmlWriter.WriteStartElement("QTM_Settings");
            {
                xmlWriter.WriteStartElement("Force");
                {
                    xmlWriter.WriteStartElement("Plate");
                    {
                        if (mMajorVersion > 1 ||mMinorVersion > 7)
                        {
                            xmlWriter.WriteElementString("Plate_ID", plateSettings.analogDeviceID.ToString());
                        }
                        else
                        {
                            xmlWriter.WriteElementString("Force_Plate_Index", plateSettings.analogDeviceID.ToString());
                        }

                        xmlWriter.WriteStartElement("Corner1");
                        {
                            xmlWriter.WriteElementString("X", plateSettings.location.corner1.x.ToString());
                            xmlWriter.WriteElementString("Y", plateSettings.location.corner1.y.ToString());
                            xmlWriter.WriteElementString("Z", plateSettings.location.corner1.z.ToString());
                        }
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("Corner2");
                        {
                            xmlWriter.WriteElementString("X", plateSettings.location.corner2.x.ToString());
                            xmlWriter.WriteElementString("Y", plateSettings.location.corner2.y.ToString());
                            xmlWriter.WriteElementString("Z", plateSettings.location.corner2.z.ToString());
                        }
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("Corner3");
                        {
                            xmlWriter.WriteElementString("X", plateSettings.location.corner3.x.ToString());
                            xmlWriter.WriteElementString("Y", plateSettings.location.corner3.y.ToString());
                            xmlWriter.WriteElementString("Z", plateSettings.location.corner3.z.ToString());
                        }
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("Corner4");
                        {
                            xmlWriter.WriteElementString("X", plateSettings.location.corner4.x.ToString());
                            xmlWriter.WriteElementString("Y", plateSettings.location.corner4.y.ToString());
                            xmlWriter.WriteElementString("Z", plateSettings.location.corner4.z.ToString());
                        }
                        xmlWriter.WriteEndElement();

                    }
                    xmlWriter.WriteEndElement();

                }
                xmlWriter.WriteEndElement();

            }
            xmlWriter.WriteEndElement();

            return xmlString.ToString();
        }

        #endregion

        #region generic send functions

        /// <summary>
        /// Send string to QTM server
        /// </summary>
        /// <param name="strtoSend">string with data to send</param>
        /// <param name="packetType">what type of packet it should be sent as</param>
        /// <returns>true if string was sent successfully</returns>
        public bool sendString(string strtoSend, ePacketType packetType)
        {
            if (mNetwork.connected())
            {
                byte[] str = Encoding.ASCII.GetBytes(strtoSend);
                byte[] size = BitConverter.GetBytes(str.Length + Constants.PACKET_HEADER_SIZE);
                byte[] cmd = BitConverter.GetBytes((int)packetType);

                List<byte> b = new List<byte>();
                b.AddRange(size);
                b.AddRange(cmd);
                b.AddRange(str);

                

                byte[] msg = b.ToArray();
                bool status = mNetwork.send(msg, str.Length + Constants.PACKET_HEADER_SIZE);

                return status;
            }
            return false;
        }

        /// <summary>
        /// Send command to QTM server that TCP socket is connected to
        /// </summary>
        /// <param name="command">command to send</param>
        /// <returns>true if server doesnt reply with error packet</returns>
        public bool sendCommand(string command)
        {
            bool status = sendString(command, ePacketType.kPacketCommand);
            if (status)
            {
                ePacketType responsePacket;
				Thread.Sleep(10); //avoid missing packets
                receiveRTPacket(out responsePacket);
                if (responsePacket != ePacketType.kPacketError)
                {
                    return true;
                }
                else
                {
                    mErrorString = mPacket.getErrorString();
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Send XML data to QTM server
        /// </summary>
        /// <param name="xmlString">string with XML data to send</param>
        /// <returns>true if xml was sent successfully</returns>
        public bool sendXML(string xmlString)
        {
            if (sendString(xmlString, ePacketType.kPacketXML))
            {
                ePacketType eType;

                if (receiveRTPacket(out eType) > 0)
                {
                    if (eType == ePacketType.kPacketCommand)
                    {
                        return true;
                    }
                    else
                    {
                        mErrorString = mPacket.getErrorString();
                    }
                }
            }

            return false;
        }

        #endregion

        /// <summary>
        /// Error reported by protocol or from server packet
        /// </summary>
        /// <returns>Error message</returns>
        public string getErrorString()
        {
            return mErrorString;
        }

        /// <summary>
        /// Builds string for components when using streamframes function
        /// </summary>
        /// <param name="componentTypes">component types to stream</param>
        /// <returns>string with protocol names of components</returns>
        private string BuildStreamString(List<eComponentType> componentTypes)
        {
            if (componentTypes == null)
            {
                return "";
            }

            string command = "";

            foreach (eComponentType type in componentTypes)
            {
                switch(type)
                {
                    case eComponentType.kComponent3d:
                        command += " 3D";
                    break;
                    case eComponentType.kComponent3dNoLabels:
                        command += " 3DNoLabels";
                    break;
                    case eComponentType.kComponentAnalog:
                        command += " Analog";
                    break;
                    case eComponentType.kComponentForce:
                        command += " Force";
                    break;
                    case eComponentType.kComponent6d:
                        command += " 6D";
                    break;
                    case eComponentType.kComponent6dEuler:
                        command += " 6DEuler";
                    break;
                    case eComponentType.kComponent2d:
                        command += " 2D";
                    break;
                    case eComponentType.kComponent2dLin:
                        command += " 2DLin";
                    break;
                    case eComponentType.kComponent3dRes:
                        command += " 3DRes";
                    break;
                    case eComponentType.kComponent3dNoLabelsRes:
                        command += " 3DNoLabelsRes";
                    break;
                    case eComponentType.kComponent6dRes:
                        command += " 6DRes";
                    break;
                    case eComponentType.kComponent6dEulerRes:
                        command += " 6DEulerRes";
                    break;
                    case eComponentType.kComponentAnalogSingle:
                        command += " AnalogSingle";
                    break;
                    case eComponentType.kComponentImage:
                        command += " Image";
                    break;
                    case eComponentType.kComponentForceSingle:
                        command += " ForceSingle";
                    break;
                }
            }
            return command;
        }
    }
}