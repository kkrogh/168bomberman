using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	////////////////////////////////////////////////////////////////////////	
	public string IP = "192.168.0.16";//// Local IP here  //////////////////////////////
	////////////////////////////////////////////////////////////////////////	
	public int Port = 25001;


	public string username = "";
	bool RegisterUI = false;
	bool LoginUI = false;

	void OnGUI()
	{
		if (Network.peerType == NetworkPeerType.Disconnected) {
			if (GUI.Button (new Rect (100, 100, 100, 25), "Start Client")) {
				Network.Connect (IP, Port);
			}
			if (GUI.Button (new Rect (100, 125, 100, 25), "Start Server")) {
				Network.InitializeServer (4, Port);
			}
		}
			
		else
		{
			if(Network.peerType==NetworkPeerType.Client)
			{
				if(RegisterUI == true && LoginUI == false)
				{
					username = GUI.TextArea (new Rect(100,125,110,25),username);	
					if(GUI.Button (new Rect(100,150,110,25),"Register"))
					{
						GetComponent<NetworkView>().RPC ("Register",RPCMode.Server,username);
						RegisterUI = false;
					}if(GUI.Button (new Rect(100,175,110,25),"Back"))
					{
						RegisterUI = false;
					}
				}
				else if(RegisterUI == false && LoginUI == true)
				{
					username = GUI.TextArea (new Rect(100,125,110,25),username);
					if(GUI.Button (new Rect(100,150,110,25),"Login"))
					{
						GetComponent<NetworkView>().RPC ("Login",RPCMode.Server,username);
					}if(GUI.Button (new Rect(100,175,110,25),"Back"))
					{
						LoginUI = false;
					}
				}
				else{

					GUI.Label(new Rect(100,100,100,25),"Client");

					if(GUI.Button (new Rect(100,125,110,25),"Login"))
					{
						LoginUI = true;
					}
					if(GUI.Button (new Rect(100,150,110,25),"Register"))
					{
						RegisterUI = true;
					}
					if(GUI.Button (new Rect(100,175,110,25),"Logout"))
					{
						Network.Disconnect(250);
					}
				}
			}
			if(Network.peerType == NetworkPeerType.Server)
			{				
				GUI.Label(new Rect(100,100,100,25),"Server");
				GUI.Label(new Rect(100,125,100,25),"Connections: " + Network.connections.Length);
				if(GUI.Button (new Rect(100,150,100,25),"Logout"))
				{
					Network.Disconnect(250);
				}
			}
		}
	}
	[RPC]
	void Login(string username)
	{
		if (Network.isServer) {
			bool checkUsername = PlayerPrefs.HasKey(username);

			if( checkUsername)
			{
				GetComponent<NetworkView>().RPC ("LoadLevel",RPCMode.Others);
			}
		}
	}
	[RPC]
	void LoadLevel()
	{
		if (Network.isClient) {
			if(Application.loadedLevel == 0)
			{
				Application.LoadLevel ("MainGame");
			}
		}
	}


	[RPC]
	void Register(string username)
	{
		if (Network.isServer) {
			PlayerPrefs.SetString (username,username);
		}
	}

}