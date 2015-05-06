using UnityEngine;
using System.Collections;

public class ClientLevelManager : MonoBehaviour 
{
	GameObject player;
	GameObject[] enemies;
	
	private string message;
	// Use this for initialization
	void Start () 
	{
		player = GameObject.Find("BomberMan");
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(true)
		{
			ClientAction.received = false;
			StateObject recv_so = new StateObject();
			recv_so.workSocket = AsynchronousClient.client;
			AsynchronousClient.Receive(recv_so);
			
			
				//Debug.Log("ClientManager: " + recv_so.response);
			message = "Echo " + recv_so.response + " <EOF>";
			
			StateObject send_so = new StateObject();
			send_so.workSocket = AsynchronousClient.client;
			AsynchronousClient.Send(AsynchronousClient.client,message, send_so);
			
		}
	}
	
	void OnGUI()
	{
		GUI.Label(new Rect(100,100,100,25), "Received" + message);
	}
}
