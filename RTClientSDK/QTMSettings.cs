using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace QTMRealTimeSDK.Settings
{
    /// <summary> General Settings from QTM. </summary>
    [XmlRoot("General")]
    public class SettingsGeneral
    {
        /// <summary> QTM Capture frequency </summary>
        [XmlElement("Frequency")]
        public int captureFrequency;

        /// <summary> length of QTM Capture. Time expressed in seconds</summary>
        [XmlElement("Capture_Time")]
        public float captureTime;

        /// <summary> Measurement start on external trigger</summary>
        [XmlElement("Start_On_External_Trigger")]
        public bool startOnExternalTrigger;

        [XmlElement("External_Time_Base")]
        public sSettingsGeneralExternalTimeBase externalTimebase;

        [XmlElement("Processing_Actions")]
        public sSettingProccessingActions processingActions;

        /// <summary> Camera Settings </summary>

        [XmlElement("Camera")]
        public List<sSettingsGeneralCamera> cameraSettings;

        public SettingsGeneral()
        {
        }
    }

    /// <summary> 3D Bone Settings from QTM. </summary>
    public class SettingsBone
    {
        /// <summary> name of marker bone starts from </summary>
        [XmlAttribute("From")]
        public string from;

        /// <summary> name of marker bone ends at</summary>
        [XmlAttribute("To")]
        public string to;

        SettingsBone()
        {
        }
    }

    /// <summary> 3D Settings from QTM. </summary>
    [XmlRoot("The_3D")]
    public class Settings3D
    {
        [XmlElement("AxisUpwards")]
        public eAxis axisUpwards;
        [XmlElement("CalibrationTime")]
        public string calibrationTime;
        [XmlElement("Labels")]
        public int labelsCount;
        [XmlElement("Label")]
        public List<sSettings3DLabel> labels3D;

        [XmlArray("Bones")]
        [XmlArrayItem("Bone", typeof(SettingsBone))]
        public SettingsBone[] bones;

        public Settings3D()
        {
        }
    }

    /// <summary> 6D Settings from QTM. </summary>
    [XmlRoot("The_6D")]
    public class Settings6D
    {
        [XmlElement("Bodies")]
        public int bodyCount;
        [XmlElement("Body")]
        public List<sSettings6DOF> bodies;
    }

    /// <summary> Analog Settings from QTM. </summary>
    [XmlRoot("Analog")]
    public class SettingsAnalog
    {
        [XmlElement("Device")]
        public List<sAnalogDevice> devices;

        public SettingsAnalog()
        {

        }
    }

    /// <summary> Force Settings from QTM. </summary>
    [XmlRoot("Force")]
    public class SettingsForce
    {
        [XmlElement("Unit_Length")]
        public string unitLength;
        [XmlElement("Unit_Force")]
        public string unitForce;
        [XmlElement("Plate")]
        public List<sForcePlateSettings> plates;

        public SettingsForce() { }
    }

    /// <summary> Image Settings from QTM. </summary>
    [XmlRoot("Image")]
    public class SettingsImage
    {
        [XmlElement("Camera")]
        public List<sImageCamera> cameraList;
    }
}