using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace QTM2Unity.Unity
{

    public class RTGUI : EditorWindow
    {
        short portUDP = 4545;
        int streamFreq = 60;
        int streammode = 0;
        int server = 0;

        string connectionStatus = "Not Connected";

        bool connected = false;
        bool stream6d = true;
        bool stream3d = true;

        GUIContent[] popuplist;
        GUIContent refreshlist;
        private List<sixDOFBody> Availablebodies;
            
        [MenuItem("Window/Qualisys/RTClient")]
        public static void ShowWindow()
        {
            //UnityEngine.Debug.Log("ShowWindow()");

            EditorWindow.GetWindow(typeof(RTGUI));
        }

        public void OnEnable()
        {
            //UnityEngine.Debug.Log("OnEnable()");
            refreshServerList();
            Repaint();
        }
        private void refreshServerList()
        {
            String[] servers;
            if (RTClient.getInstance().getServers(out servers))
            {
                popuplist = new GUIContent[servers.Length];
                for (int i = 0; i < servers.Length; i++)
                {
                    popuplist[i] = new GUIContent(servers[i]);
                }
            }
            else
            {
                connectionStatus = "No servers found, is QTM running?";
                popuplist = new GUIContent[0];
            }
        }

        /// This makes sure we only can connect when in playing mode
        /// otherwise IK cant be controlled
        void OnInspectorUpdate()
        {
            //UnityEngine.Debug.Log("OnInspectorUpdate()");
            if (!Application.isPlaying && connected)
            {
                OnDisconnect();
                connected = false;
                popuplist = null;
            }
        }

        void OnGUI()
        {
            //UnityEngine.Debug.Log("OnGUI()");
            title = "QTM Streaming";
            GUILayout.Label("Server Settings", EditorStyles.boldLabel);
            if (Application.isPlaying)
            {
                if (!connected)
                {
                    GUILayout.Label("QTM Servers:");
                    server = EditorGUILayout.Popup(server, popuplist);
                    if (GUILayout.Button("Refresh Server List"))
                    {
                        refreshServerList();
                    }
                }
                else
                {
                    GUILayout.Label("Connected to: " + popuplist[server].text);
                }
            }
            else
            {
                GUILayout.Label("(Unity needs to be in play mode to set server)");
            }
            if (connected)
            {
                GUI.enabled = false;
            }
            GUILayout.Label("Stream Settings", EditorStyles.boldLabel);
            portUDP = (short)EditorGUILayout.IntField("UDP Port:", portUDP);
            string[] popupStrings = new string[3] { "All Frames", "Frequency", "Frequency divisor" };
            streammode = EditorGUILayout.Popup("Stream modes", streammode, popupStrings);
            streamFreq = EditorGUILayout.IntField("Frequency:", streamFreq);

            stream3d = EditorGUILayout.Toggle("3D Labeled Data", stream3d);
            stream6d = EditorGUILayout.Toggle("6 DOF Data", stream6d);
            GUI.enabled = true;

            if (Application.isPlaying)
            {
                GUILayout.Label("Status: " + connectionStatus);

                if (connected)
                {
                    if (GUILayout.Button("Disconnect"))
                    {
                        OnDisconnect();
                    }
                    GUILayout.Label("Available Bodies:");
                    if (Availablebodies != null)
                    {
                        foreach (sixDOFBody body in Availablebodies)
                        {
                            GUILayout.Label(body.name);
                        }
                    }
                }
                else
                {
                    if (popuplist.Length > 0)
                    {
                        if (GUILayout.Button("Connect"))
                        {
                            OnConnect();
                        }
                    }
                }
            }
            else
            {
                GUILayout.Label("Please start Play to start streaming", EditorStyles.boldLabel);
            }
        }

        void OnDestroy()
        {
            //UnityEngine.Debug.Log("OnDestroy()");
            RTClient.getInstance().disconnect();
            connected = false;
        }

        void OnDisconnect()
        {
            //UnityEngine.Debug.Log("OnDisconnect()");
            RTClient.getInstance().disconnect();
            connected = false;

            connectionStatus = "Disconnected";
            Repaint();
        }

        void OnConnect()
        {
            //UnityEngine.Debug.Log("OnConnect() " + server );
            connected = RTClient.getInstance().connect(server, portUDP, streammode, streamFreq, stream6d, stream3d);
            if (connected)
            {
                connectionStatus = "Connected";
                Availablebodies = RTClient.getInstance().Bodies;
            }
            else
            {
                connectionStatus = "connection Error - check console";
            }
        }
    }
}