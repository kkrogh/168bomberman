using UnityEngine;
using System.Collections;

public class Login : MonoBehaviour 
{
	SocketListener serverInstance;
	AsynchronousClient clientInstance;
	
	public string username = "";
	public string password = "";
	bool registerUI = false;
	bool loginUI = false;
	// Use this for initialization
	void Start () 
	{
		serverInstance = SocketListener.instance;
		clientInstance = AsynchronousClient.instance;
	}
	
	// Update is called once per frame
	void OnGUI()
	{
		if(AsynchronousClient.client == null || !AsynchronousClient.client.Connected)
		{
			if(GUI.Button(new Rect(100,100,100,25),"Connect to Server"))
			{
				clientInstance.StartClient();
			}
		}
		else
		{
			if(loginUI == true && registerUI == false)
			{
				GUI.Label(new Rect(100, 125, 100, 25), "User Name: ");
				username = GUI.TextArea (new Rect(200,125,110,25),username);
				GUI.Label(new Rect(100, 150, 100, 25), "Password: ");
				password = GUI.TextArea(new Rect(200,150,110,25),password);
				
				if(GUI.Button (new Rect(100,175,110,25),"Login"))
				{
					string content = "Login " + username + " " + password;
					Debug.Log( "Attempting login with username ="+username+"  password="+  password);
					
					StateObject send_so = new StateObject();
					send_so.workSocket = AsynchronousClient.client;
					AsynchronousClient.Send(AsynchronousClient.client,content, send_so);
					send_so.sendDone.WaitOne(5000);
					
					StateObject recv_so = new StateObject();
					recv_so.workSocket = AsynchronousClient.client;
					
					AsynchronousClient.Receive(recv_so);
					recv_so.receiveDone.WaitOne(5000);
					Debug.Log("Response received : " + recv_so.response);
					
					username = "";
					password = "";
				}if(GUI.Button (new Rect(100,200,110,25),"Back"))
				{
					loginUI = false;
					username = "";
					password = "";
				}
			}
			else if(loginUI == false && registerUI == true)
			{
			
			}
			else
			{
				if(GUI.Button(new Rect(100,100,100,25),"Login"))
				{
					loginUI =true;
				}
				if(GUI.Button(new Rect(100,125,100,25),"Login"))
				{
					registerUI = true;
				}
			}

		}
	}
}
