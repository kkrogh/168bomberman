using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	////////////////////////////////////////////////////////////////////////	
	public string IP = "192.168.0.16";//// Local IP here  //////////////////
	////////////////////////////////////////////////////////////////////////	
	public int Port = 25001;


	public string username = "";
	public string password = "";
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
					GUI.Label(new Rect(100, 125, 100, 25), "User Name: ");
					username = GUI.TextArea (new Rect(200,125,110,25),username);
					GUI.Label(new Rect(100, 150, 100, 25), "Password: ");
					password = GUI.TextArea(new Rect(200,150,110,25),password);
					if(GUI.Button (new Rect(100,175,110,25),"Register"))
					{
						GetComponent<NetworkView>().RPC ("Register",RPCMode.Server,username,password);
						RegisterUI = false;
						username = "";
						password = "";
					}if(GUI.Button (new Rect(100,200,110,25),"Back"))
					{
						RegisterUI = false;
						username = "";
						password = "";
					}
				}
				else if(RegisterUI == false && LoginUI == true)
				{
					GUI.Label(new Rect(100, 125, 100, 25), "User Name: ");
					username = GUI.TextArea (new Rect(200,125,110,25),username);
					GUI.Label(new Rect(100, 150, 100, 25), "Password: ");
					password = GUI.TextArea(new Rect(200,150,110,25),password);
					
					if(GUI.Button (new Rect(100,175,110,25),"Login"))
					{
						GetComponent<NetworkView>().RPC ("Login",RPCMode.Server,username,password);
						username = "";
						password = "";
					}if(GUI.Button (new Rect(100,200,110,25),"Back"))
					{
						LoginUI = false;
						username = "";
						password = "";
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
	void Login(string username, string password, NetworkMessageInfo info)
	{
		if (Network.isServer) {
			bool checkUsername = PlayerPrefs.HasKey(username);
			if( checkUsername && PlayerPrefs.GetString(username) == password)
			{
				GetComponent<NetworkView>().RPC ("LoadLevel",info.sender);
			}
			else
			{
				Debug.Log("Username or password incorrect");
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
	void Register(string username, string password)
	{
		Debug.Log(username + " + " + password);
		if (Network.isServer) 
		{
			bool checkUsername = PlayerPrefs.HasKey(username);
			
			if(!checkUsername)
			{
				PlayerPrefs.SetString (username,password);
			}
			else
			{
				Debug.Log("User name already exists");
			}
		}
	}

}