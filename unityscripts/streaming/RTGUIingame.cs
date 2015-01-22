using UnityEngine;
//using UnityEditor;
using System;
using System.Collections;

namespace QTM2Unity.Unity
{

	public class RTGUIingame : MonoBehaviour
	{
		short portUDP = 4545;
		int streamFreq = 60;
		int streammode = 0;
		int server = 0;

		string connectionStatus = "Not Connected";

		bool connected = false;

		GUIContent[] popuplist;

		public void OnEnable()
		{
            String[] servers;
            RTClient.getInstance().getServers(out servers);
            popuplist = new GUIContent[servers.Length];
            for (int i = 0; i < servers.Length; i++)
            {
                popuplist[i] = new GUIContent(servers[i]);
            }
		}

		/// This makes sure we only can connect when in playing mode
		void OnInspectorUpdate()
		{

			if(!Application.isPlaying)
			{
				onDisconnect();
				connected = false;
				popuplist = null;
			}
		}

		void OnGUI()
		{
            GUIStyle style = new GUIStyle();
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
			GUI.Box(new Rect(10,10,220,155), "Qualisys Realtime Streamer");

			GUI.Label(new Rect(20,40,200,40),"QTM Server:\n(switch with arrow keys)");
            GUI.Label(new Rect(20, 75, 200, 40), popuplist[server], style);

            if(Input.GetKeyDown(KeyCode.LeftArrow) && !connected)
            {
                server--;
                if(server < 0)
                {
                    server += popuplist.Length;
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && !connected)
            {
                server++;
                if(server > popuplist.Length-1)
                {
                    server = 0;
                }
            }
			//server = EditorGUI.Popup(new Rect(20,40,200,40),server,popuplist);

            //GUI.Label(new Rect(20,80,200,40), "Stream Settings", EditorStyles.boldLabel);
            //GUI.Label(new Rect(25,100,80,20), "UDP Port:");
            //GUI.TextField(new Rect(90,100,60,20),"4545", portUDP);
            //string[] popupStrings = new string[3]{"All Frames", "Frequency", "Frequency divisor"};
            //GUI.Label(new Rect(20,120,80,20), "Frequency:");
            //GUI.TextField(new Rect(90,120,60,20),"60", streamFreq);

            //streammode = EditorGUI.Popup(new Rect(90,120,60,20), "Stream modes",streammode ,popupStrings);
			if(connected)
			{
				if(GUI.Button(new Rect(20,115,200,40),"Disconnect"))
				{
					onDisconnect();
				}
			}
			else
			{
				if(GUI.Button(new Rect(20,115,200,40),"Connect"))
				{
					onConnect();
				}
			}
			GUI.Label(new Rect(20,90,200,40),"Status: " + connectionStatus);
		}

		void OnDestroy()
		{
			RTClient.getInstance().disconnect();
			connected = false;
		}

		void onDisconnect()
		{
			RTClient.getInstance().disconnect();
			connected = false;

			connectionStatus = "Disconnected";
		}

		void onConnect()
		{
			connected = RTClient.getInstance().connect(server, portUDP, streammode, streamFreq, true, true);

			if(connected)
				connectionStatus = "Connected";
			else
				connectionStatus = "connection Error - check console";
		}
	}
}