﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;
using System.Collections;

// State object for receiving data from remote device.
//public class StateObject {
//	// Client socket.
//	public Socket workSocket = null;
//	// Size of receive buffer.
//	public const int BufferSize = 256;
//	// Receive buffer.
//	public byte[] buffer = new byte[BufferSize];
//	// Received data string.
//	public StringBuilder sb = new StringBuilder();
//	// ManualResetEvent instances signal completion.
//	public ManualResetEvent connectDone =
//		new ManualResetEvent(false);
//	
//	public ManualResetEvent receiveDone =
//		new ManualResetEvent(false);
//	
//	public ManualResetEvent sendDone =
//		new ManualResetEvent(false);
//	
//	// The response from the remote device.
//	public String response = String.Empty;
//}

public class PlayerInfo
{
	public int playerNum;
	public float x;
	public float y;
	public bool updated = false;
}

public class BombInfo
{
	public int playerNum;
	public float x;
	public float y;
	public bool droppedBomb = false;
}

public class ClientAction
{
	public static bool received = true;
	public static bool loadLevel = false;
	public static int playerNum = 0;
	public static int logedEnemyNum = 0;
	public static PlayerInfo playerInfo = new PlayerInfo();
	public static BombInfo bombInfo = new BombInfo();
}

public class AsynchronousClient : MonoBehaviour{

public static string guiDebugStr = "";
	// The port number for the remote device.
	public static AsynchronousClient instance;
	public static Socket client;
	
	public string ipStr = "192.168.0.16";
	private const int port = 11000;
	private string message;
	
	private static string[] stringSeparators = new string[] { "<EOF>" };
	private static bool received = true;
	
	void Awake()
	{
		if(instance != null && instance != this)
		{
			DestroyImmediate(gameObject);
			return;
		}
		
		instance = this;
		DontDestroyOnLoad(gameObject);
	}
	
	void Start()
	{
	//	StartClient();
	}
	
	void Update()
	{
		if(ClientAction.loadLevel)
		{
			Application.LoadLevel("MainGame");
			ClientAction.loadLevel = false;
		}
		
		if(ClientAction.playerNum > 0)
		{
			ClientLevelManager.instance.LoadPlayer(ClientAction.playerNum);
			ClientAction.playerNum = 0;
		}
		
		if(ClientAction.logedEnemyNum > 0)
		{
			ClientLevelManager.instance.AddEnemy(ClientAction.logedEnemyNum);
			ClientAction.logedEnemyNum = 0;
		}
		
		if(ClientAction.playerInfo.updated)
		{			
			ClientLevelManager.instance.SetPlayerPos(ClientAction.playerInfo.playerNum,
													 ClientAction.playerInfo.x,
													 ClientAction.playerInfo.y);
													 
			ClientAction.playerInfo.updated = false;
		}
		
		if(ClientAction.bombInfo.droppedBomb)
		{
			ClientLevelManager.instance.ClientDropBomb(ClientAction.bombInfo.playerNum,
													   ClientAction.bombInfo.x,
													   ClientAction.bombInfo.y);
			ClientAction.bombInfo.droppedBomb = false;
		}
		
	}
	
	void OnGUI()
	{
		GUI.Label(new Rect(50, 50, 200, 50),  "ThreadExeption: " + guiDebugStr);
	}
	
	public void MessageHandler(string message)
	{
		try
		{
		//guiDebugStr = message;
		Debug.Log("ClientMessageHandler : " + message);
		string[] token = message.Split(new Char[]{' '});
		
		if(token[0] == "LoadLevel")
		{
			ClientAction.loadLevel = true;
		}
		if(token[0] == "PlayerNum")
		{
			ClientAction.playerNum = int.Parse(token[1]);
			//ClientLevelManager.instance.SetPlayerStartPosition(1);
		}
		if(token[0] == "NewPlayer")
		{
			ClientAction.logedEnemyNum = int.Parse(token[1]);
		}
		if(token[0] == "EnemyPos")
		{
			ClientAction.playerInfo.playerNum = int.Parse(token[1]);
			ClientAction.playerInfo.x = float.Parse(token[2]);
			ClientAction.playerInfo.y = float.Parse(token[3]);
			ClientAction.playerInfo.updated = true;
		}
		if(token[0] == "BombDropped")
		{
			ClientAction.bombInfo.playerNum = int.Parse(token[1]);
			ClientAction.bombInfo.x = float.Parse(token[2]);
			ClientAction.bombInfo.y = float.Parse(token[3]);
			ClientAction.bombInfo.droppedBomb = true;
		}
		
		}
		catch(Exception e)
		{
			guiDebugStr = e.ToString();
		}
	}
//	void OnGUI()
//	{
//		if(GUI.Button(new Rect(100,100,100,25), "Start Client"))
//		{
//			StartClient();
//			Debug.Log("client connected: " + client.Connected);
//		}
//		if(GUI.Button(new Rect(100,125,100,25), "Send 'Test'"))
//		{
//			StateObject send_so = new StateObject();
//			send_so.workSocket = client;
//			Send(client,"This is a test<EOF>", send_so);
//			send_so.sendDone.WaitOne(5000);
//			
//			StateObject recv_so = new StateObject();
//			recv_so.workSocket = client;
//			
//			Receive(recv_so);
//			recv_so.receiveDone.WaitOne(5000);
//			Debug.Log("Response received : " + recv_so.response);
//		}
//		if(GUI.Button(new Rect(100,150,100,25), "Send 'Login'"))
//		{
//			StateObject send_so = new StateObject();
//			send_so.workSocket = client;
//			Send(client,"Login myname mypassword<EOF>", send_so);
//			send_so.sendDone.WaitOne(5000);
//			
//			StateObject recv_so = new StateObject();
//			recv_so.workSocket = client;
//			
//			Receive(recv_so);
//			recv_so.receiveDone.WaitOne(5000);
//			Debug.Log("Response received : " + recv_so.response);
//		}
//		
//	}
	
	public void StartClient(string ipAdd) {
		// Connect to a remote device.
		try {
		Debug.Log("Client Connect Attempt");
			// Establish the remote endpoint for the socket.
			// The name of the 
			// remote device is "host.contoso.com".
			IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			//IPAddress ipAddress = ipHostInfo.AddressList[0];
			IPAddress ipAddress = IPAddress.Parse(ipAdd);
			IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
			
			// Create a TCP/IP socket.
			client = new Socket(AddressFamily.InterNetwork,
			                           SocketType.Stream, ProtocolType.Tcp);
			
			StateObject send_so = new StateObject();
			send_so.workSocket = client;
			// Connect to the remote endpoint.
			client.BeginConnect( remoteEP, 
			                    new AsyncCallback(ConnectCallback), send_so);
			
			// Waits for 5 seconds for connection to be done
			send_so.connectDone.WaitOne(5000);
			
			// Send test data to the remote device.
//			Send(client,"This is a test<EOF>", send_so);
//			send_so.sendDone.WaitOne(5000);
			
			// Receive the response from the remote device.
			// Create the state object for receiving.
//			StateObject recv_so = new StateObject();
//			recv_so.workSocket = client;
//			
//			Receive(recv_so);
//			recv_so.receiveDone.WaitOne(5000);
			
			// Write the response to the console.
			//Console.WriteLine("Response received : {0}", recv_so.response);
//			Debug.Log("Response received : " + recv_so.response);
			
		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		}
	}
	
	private static void ConnectCallback(IAsyncResult ar) {
		try {
			// Create the state object.
			StateObject state = (StateObject)ar.AsyncState;
			// Retrieve the socket from the state object.
			Socket client = state.workSocket;
			
			// Complete the connection.
			client.EndConnect(ar);
			
			Console.WriteLine("Socket connected to {0}",
			                  client.RemoteEndPoint.ToString());
			Debug.Log("Socket connected to " + 
			                  client.RemoteEndPoint.ToString());
			// Signal that the connection has been made.
			state.connectDone.Set();
		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		}
	}
	
	public static void Receive(StateObject state) {
		try
		{
			Socket client = state.workSocket;
			
			// Begin receiving the data from the remote device.
			client.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
			                    new AsyncCallback(ReceiveCallback), state);
		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		}
	}
	
	private static void ReceiveCallback( IAsyncResult ar ) {
		try {
			// Retrieve the state object and the client socket 
			// from the asynchronous state object.
			ClientAction.received = true;
			StateObject state = (StateObject) ar.AsyncState;
			Socket client = state.workSocket;
			
			// Read data from the remote device.
			int bytesRead = client.EndReceive(ar); 
			
			if (bytesRead > 0) {
				// Found a 
				state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
				string content = state.sb.ToString();
				
				String[] message = content.Split(stringSeparators, StringSplitOptions.None);
				if (message.Length > 1)
				{
					state.receiveDone.Set();
					state.response = message[0];
					
					
					instance.MessageHandler(message[0]);
					
				//	state.workSocket.Shutdown(SocketShutdown.Both);
				//	state.workSocket.Close();
					
					StateObject newstate = new StateObject();
					newstate.workSocket = client;
					// Call BeginReceive with a new state object
					client.BeginReceive(newstate.buffer, 0, StateObject.BufferSize, 0,
					                     new AsyncCallback(ReceiveCallback), newstate);
				}
				else
				{
					// Get the rest of the data.
					client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
					                    new AsyncCallback(ReceiveCallback), state);
				}
			} 
			else 
			{
				guiDebugStr = "Connection close has been requested";
				//Console.WriteLine("Connection close has been requested.");
				// Signal that all bytes have been received.
				
			}
		} catch (Exception e) {
			//Console.WriteLine(e.ToString());
			guiDebugStr = e.ToString();
		}
	}
	
	public static void Send(Socket client, String data, StateObject so) {
		// Convert the string data to byte data using ASCII encoding.
		byte[] byteData = Encoding.ASCII.GetBytes(data);
		
		// Begin sending the data to the remote device.
		client.BeginSend(byteData, 0, byteData.Length, 0,
		                 new AsyncCallback(SendCallback), so);
	}
	
	private static void SendCallback(IAsyncResult ar) {
		try {
			// Retrieve the socket from the state object.
			StateObject so = (StateObject) ar.AsyncState;
			Socket client = so.workSocket;
			
			// Complete sending the data to the remote device.
			int bytesSent = client.EndSend(ar);
			Console.WriteLine("Sent {0} bytes to server.", bytesSent);
			
			// Signal that all bytes have been sent.
			so.sendDone.Set();
		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		}
	}
	
	
	
	public bool isConnected()
	{
		return client.Connected;
	}

}

