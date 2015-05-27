using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;

// State object for reading client data asynchronously
//public class StateObject {
//	// Client  socket.
//	public Socket workSocket = null;
//	// Size of receive buffer.
//	public const int BufferSize = 1024;
//	// Receive buffer.
//	public byte[] buffer = new byte[BufferSize];
//	// Received data string.
//	public StringBuilder sb = new StringBuilder();  
//}
public class ServerAction
{
	public static bool accepted = true;
	public static bool sending;
	public static Socket logedPlayer = null;
	public static int session = 0;
	
	
}

public class ClientInfo
{
	public Socket client;
	public int session;
	public string username;
}

public class SocketListener : MonoBehaviour 
{
public static string guiDebugStr = "";
	public static SocketListener instance;
	// Thread signal.
	public static ManualResetEvent allDone = new ManualResetEvent(false);
	public static List<Socket> clients = new List<Socket>();
	public static List<ClientInfo> logedClients = new List<ClientInfo>();
	private static bool accepted = true;
	
	public string conn;
	public ServerLevelManager levelManager;
	
	
	byte[] bytes = new Byte[1024];
	
	// Create a TCP/IP socket.
	Socket listener;
	
	//Listen to external IP address
	IPHostEntry ipHostInfo;
	IPAddress ipAddress;
	IPEndPoint localEndPoint;
	
	// Listen to any IP Address
	IPEndPoint any;
	
	private static string[] stringSeparators = new string[] { "<EOF>" };
	
	void Awake()
	{
		conn = "URI=file:" + Application.dataPath + "/BombermanDB.s3db";
		if(instance != null && instance != this)
		{
			DestroyImmediate(gameObject);
			return;
		}
		
		instance = this;
		DontDestroyOnLoad(gameObject);
		
		levelManager = GameObject.Find("ServerLevelManager").GetComponent<ServerLevelManager>();
	}
	
	
	void Start()
	{
		
	
		bytes = new Byte[1024];
		conn = "URI=file:" + Application.dataPath + "/BombermanDB.s3db";
		// Create a TCP/IP socket.
		listener = new Socket(AddressFamily.InterNetwork,
		                       SocketType.Stream, ProtocolType.Tcp );
		
		//Listen to external IP address
		ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
		ipAddress = ipHostInfo.AddressList[0];
		localEndPoint = new IPEndPoint(ipAddress, 11000);
		
		// Listen to any IP Address
		any = new IPEndPoint(IPAddress.Any, 11000);
		listener.Bind(any);
		listener.Listen(100);
	}
	
	void Update()
	{
		// Set the event to nonsignaled state.
		allDone.Reset();
		
	
		if(ServerAction.accepted)
		{
			ServerAction.accepted = false;
			Console.WriteLine("Waiting for a connection..");
			Debug.Log("Waiting for a connection..");
			listener.BeginAccept( 
			                     new AsyncCallback(AcceptCallback),
			                     listener );
		}
	
		
		if(ServerAction.logedPlayer != null)
		{
			int session = 1;
			foreach(ClientInfo clientInfo in logedClients)
			{
				if(clientInfo.client == ServerAction.logedPlayer)
				{
					session = clientInfo.session;
				}
			}
			
			levelManager.AddPlayer(ServerAction.logedPlayer, session);
			ServerAction.logedPlayer = null;
			//Send(ServerAction.logedClient, "LoadLevel<EOF>");
			
			//levelManager.AddPlayer(ServerAction.logedClient);
			//ServerAction.logedClient = null;
		}
		
	
		
//		if(logedClients.Count > 2)
//		{
//			foreach(Socket clientObj in logedClients)
//			{
//				
//			}
////			if(Application.loadedLevelName != "ServerMainGame")
////			{
////				Application.LoadLevel("ServerMainGame");
////			}
//		}
		

		// Wait until a connection is made before continuing.
		//allDone.WaitOne();
	}
	
	//Handle all client messages here
	public void MessageHandler(Socket client, string message)
	{
		try
		{
			guiDebugStr = message;
		//Debug.Log("ServerMessageHandler: " + message);
		string[] token = message.Split(new Char[]{'|'});
		//parse strings here. Example:
		//guiDebugStr = token[0];
		if(token[0] == "Login")
		{
			bool loginOk = DataBaseLogin(token[1], token[2]);
			//bool loginOk = true;
			if(loginOk)
			{
				ClientInfo clientInfo = new ClientInfo();
				clientInfo.client = client;
				clientInfo.username = token[1];
				
				
				logedClients.Add(clientInfo);
				
				string userStr = "";
				
				foreach(ClientInfo obj in logedClients)
				{
					
					userStr = userStr + obj.username + "|";
					
					if(obj != null && obj.client != client)
					{
						Send(obj.client, "PlayerLoggedIn|" + clientInfo.username + "|<EOF>");
					}
				}
				
				Send(client, "LoadLobby|" + userStr + "<EOF>");
				
				
				//serverState = ServerState.PlayingMainGame;
			}
		}
		else if(token[0] == "StartSession")
		{
			foreach(ClientInfo clientInfo in logedClients)
			{
				if(clientInfo.client == client)
				{
					clientInfo.session = int.Parse(token[1]);
				}
			}
					
			Send(client, "LoadLevel|<EOF>");
			
//			if(logedClients.Contains(client))
//			{
//				Send(client, "LoadLevel|<EOF>");
//			}
		}
		else if(token[0] == "Loaded")
		{
		
			ServerAction.logedPlayer = client;
			ServerAction.session = 1;
			
		}
		else if(token[0] == "Chat")
		{
			Debug.Log ("received chat, sending chat");
			string chatstring = "Chat|" + token[1] +"|<EOF>";
			
			foreach(ClientInfo clientInfo in logedClients)
			{
					Send (clientInfo.client,chatstring);
			}
			
			//Send (client,chatstring);
			Debug.Log ("after send");
			
		}
		else if(token[0] == "Register")
		{
			DataBaseRegister(token[1], token[2]);
		}
		else if(token[0] == "PlayerPos")
		{
		//	Debug.Log("Handling position");
			PlayerAction action = new PlayerAction();
			action.client = client;
			action.session = int.Parse(token[1]);
			action.playerNum = int.Parse(token[2]);
			action.actionStr = "PlayerPos|" + token[3] + "|" + token[4];
			ServerLevelManager.actionQueue.Enqueue(action);
			
		}
		else if(token[0] == "BombDropped")
		{
			PlayerAction action = new PlayerAction();
			action.client = client;
			action.session = int.Parse(token[1]);
			action.playerNum = int.Parse(token[2]);
			action.actionStr = "BombDropped|" + token[3] + "|" + token[4];
			ServerLevelManager.actionQueue.Enqueue(action);
		}
		else if(token[0] == "Disconnect")
		{
			guiDebugStr = "Got to if";
			//logedClients.Remove(client);
			PlayerAction action = new PlayerAction();
			action.client = client;
			action.session = int.Parse(token[1]);
			
			if(action.session != 0)
			{
					action.playerNum = int.Parse(token[2]);
					action.actionStr = "Disconnect";
					ServerLevelManager.actionQueue.Enqueue(action);
			}
			
		}
		else
		{
			guiDebugStr = "string not handled";
		}
		}
		catch(Exception e)
		{
			guiDebugStr = message; //e.ToString();
		}
		
	}
	
	void OnGUI()
	{
	//Template
		
		if(listener != null)
		{
			GUI.Label(new Rect(5,5,100,25), "Server Running");
		}
		
		GUI.Label(new Rect(5,30,1000,25), "ServerMessageHandler: " + guiDebugStr);
	}
	
	
	
	public static void StartListening() {
		// Data buffer for incoming data.
		byte[] bytes = new Byte[1024];
		
		// Create a TCP/IP socket.
		Socket listener = new Socket(AddressFamily.InterNetwork,
		                             SocketType.Stream, ProtocolType.Tcp );
		
		//Listen to external IP address
		IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
		IPAddress ipAddress = ipHostInfo.AddressList[0];
		IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
		
		// Listen to any IP Address
		IPEndPoint any = new IPEndPoint(IPAddress.Any, 11000);
		
		// Bind the socket to the local endpoint and listen for incoming connections.
		try {
			listener.Bind(any);
			listener.Listen(100);
			
			while (true) {
				// Set the event to nonsignaled state.
				allDone.Reset();
				
				// Start an asynchronous socket to listen for connections.
				Console.WriteLine("Waiting for a connection..");
				Debug.Log("Waiting for a connection..");
				listener.BeginAccept( 
				                     new AsyncCallback(AcceptCallback),
				                     listener );
				// Wait until a connection is made before continuing.
				allDone.WaitOne();
			}
			
		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		}
		
		Console.WriteLine("\nPress ENTER to continue...");
		Console.Read();
		
	}
	
	public static void AcceptCallback(IAsyncResult ar) {
		// Signal the main thread to continue.
		Debug.Log("AcceptCallBack");
		allDone.Set();
		// Get the socket that handles the client request.
		Socket listener = (Socket) ar.AsyncState;
		Socket handler = listener.EndAccept(ar);
		ServerAction.accepted = true;	//static
		
		// Create the state object.
		StateObject state = new StateObject();
		state.workSocket = handler;
		
		// Games have bidirectional communication (as opposed to request/response)
		// So I need to store all clients sockets so I can send them messages later
		// TODO: store in meaningful way,such as Dictionary<string,Socket>
		clients.Add (handler);
		
		handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
		                     new AsyncCallback(ReadCallback), state);
	}
	
	public static void ReadCallback(IAsyncResult ar) {
		String content = String.Empty;
		//Debug.Log("ReadCallback");
		// Retrieve the state object and the handler socket
		// from the asynchronous state object.
		StateObject state = (StateObject) ar.AsyncState;
		Socket handler = state.workSocket;
		
		// Read data from the client socket. 
		int bytesRead = handler.EndReceive(ar);
		//Debug.Log("bytesRead at ReadCallBack" + bytesRead);
		if (bytesRead > 0) {
			// There  might be more data, so store the data received so far.
			state.sb.Append(Encoding.ASCII.GetString(
				state.buffer,0,bytesRead));
			
			// Check for end-of-file tag. If it is not there, read 
			// more data.
			content = state.sb.ToString();
			String[] message = content.Split(stringSeparators, StringSplitOptions.None);
			if (message.Length > 1) {
				// All the data has been read from the 
				// client. Display it on the console.
//				Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
//				                  content.Length, content );
//				Console.WriteLine("\n\n");
//				Debug.Log("Read " + content.Length + " bytes from socket. \n Data : " + content );
				// Echo the data back to the client.
				//Send(handler, content);
				
				//
				
				for(int i = 0; i < message.Length - 1; i++)
				{
					SocketListener.instance.MessageHandler(handler, message[i]);
				}
				
//				foreach(string token in message)
//				{
//					SocketListener.instance.MessageHandler(handler, token);
//				
//				}
				//
				
				// Setup a new state object
				StateObject newstate = new StateObject();
				newstate.workSocket = handler;
				
				// Call BeginReceive with a new state object
				handler.BeginReceive(newstate.buffer, 0, StateObject.BufferSize, 0,
				                     new AsyncCallback(ReadCallback), newstate);
			} else {
				// Not all data received. Get more.
				handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
				                     new AsyncCallback(ReadCallback), state);
			}
		}
	}
	
	public static void Send(Socket handler, String data)
	{
		// Convert the string data to byte data using ASCII encoding.
		byte[] byteData = Encoding.ASCII.GetBytes(data);
		
		Debug.Log("Sent: " + data);
		// Begin sending the data to the remote device.
		handler.BeginSend(byteData, 0, byteData.Length, 0,
		                  new AsyncCallback(SendCallback), handler);
	}
	
	
	
	private static void SendCallback(IAsyncResult ar) {
		try {
			// Retrieve the socket from the state object.
			Socket handler = (Socket) ar.AsyncState;
			
			// Complete sending the data to the remote device.
			int bytesSent = handler.EndSend(ar);
			Console.WriteLine("Sent {0} bytes to client.", bytesSent);
			//Debug.Log("Sent " + bytesSent + " bytes to client.");
			
		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		}
	}

	
	private bool DataBaseLogin(string name, string password)
	{
		bool loginok = false;
		//string conn = "URI=file:" + Application.dataPath + "/BombermanDB.s3db"; //Path to database.
		IDbConnection dbconn;
		dbconn = (IDbConnection) new SqliteConnection(conn);
		dbconn.Open(); //Open connection to the database.
		IDbCommand dbcmd = dbconn.CreateCommand();
		
		
		string inusername = name;
		string inpassword = password;
		string sqlQuery = "SELECT ID, username, password " + "FROM Users " + "WHERE username == '" + inusername + "' AND password == '" + inpassword + "'";
		dbcmd.CommandText = sqlQuery;
		IDataReader reader = dbcmd.ExecuteReader();
		Debug.Log ("sql " + sqlQuery);
		
		while (reader.Read())
		{
			int ID = reader.GetInt32(0);
			string checkname = reader.GetString(1);
			string checkpassword = reader.GetString(2);
			
			Debug.Log( "Database ID= "+ID+" username ="+checkname+"  password="+  checkpassword);
			
			if(checkname == inusername && checkpassword == inpassword){
				Debug.Log ("Found a match");
				loginok = true;
			}
			
		}
		
		
		
		reader.Close();
		reader = null;
		dbcmd.Dispose();
		dbcmd = null;
		dbconn.Close();
		dbconn = null;
		GC.Collect();
		Debug.Log("Disconnected from DB");
		
		//Load level when loged in
		//			if(loginok)
		//			{GetComponent<NetworkView> ().RPC ("LoadLevel", info.sender);}
		//			else{
		//				Debug.Log("Failed Login");
		//			}
		return loginok;
	}
	
	private void DataBaseRegister(string username, string password)
	{
		bool checkUsername = isTaken(username);
		
		if(!checkUsername)
		{
		//	string conn = "URI=file:" + Application.dataPath + "/BombermanDB.s3db"; //Path to database.
			IDbConnection dbconn;
			dbconn = (IDbConnection) new SqliteConnection(conn);
			dbconn.Open(); //Open connection to the database.
			IDbCommand dbcmd = dbconn.CreateCommand();
			
			int nextuserid = getNextUserID();
			string sqlQuery = "INSERT INTO USERS (ID, username, password) VALUES ("+nextuserid+", '"+username+"', '"+password+"')";
			dbcmd.CommandText = sqlQuery;
			IDataReader reader = dbcmd.ExecuteReader();
			
			
			Debug.Log( "user inserted into database" );
			
			reader.Close();
			reader = null;
			dbcmd.Dispose();
			dbcmd = null;
			dbconn.Close();
			dbconn = null;
			GC.Collect();
			Debug.Log("Disconnected from DB");
		}
		else
		{
			Debug.Log("User name already exists");
		}
	}
	
	public void DataBaseAddKillScore(string username, int killscore)
	{
		Debug.Log("killSQL: " + username + "|" + killscore);
		IDbConnection dbconn;
		dbconn = (IDbConnection) new SqliteConnection(conn);
		dbconn.Open(); //Open connection to the database.
		IDbCommand dbcmd = dbconn.CreateCommand();
		
		
		string sqlQuery = "UPDATE Users SET Kills = Kills + 1 WHERE username =='"+username+"'";
		dbcmd.CommandText = sqlQuery;
		IDataReader reader = dbcmd.ExecuteReader();
		
		
		Debug.Log( "kill incremented" );
		
		reader.Close();
		reader = null;
		dbcmd.Dispose();
		dbcmd = null;
		dbconn.Close();
		dbconn = null;
		GC.Collect();
	}
	
	public void DataBaseAddDeathScore(string username, int deathscore)
	{
		Debug.Log("deathSQL: " + username + "|" + deathscore);
		IDbConnection dbconn;
		dbconn = (IDbConnection) new SqliteConnection(conn);
		dbconn.Open(); //Open connection to the database.
		IDbCommand dbcmd = dbconn.CreateCommand();
		
		
		string sqlQuery = "UPDATE Users SET Deaths = Deaths + 1 WHERE username =='"+username+"'";
		dbcmd.CommandText = sqlQuery;
		IDataReader reader = dbcmd.ExecuteReader();
		
		
		Debug.Log( "death incremented in database" );
		
		reader.Close();
		reader = null;
		dbcmd.Dispose();
		dbcmd = null;
		dbconn.Close();
		dbconn = null;
		GC.Collect();
	}
	
	bool isTaken(string username)
	{
		bool returnval = false;
		//string conn = "URI=file:" + Application.dataPath + "/BombermanDB.s3db"; //Path to database.
		IDbConnection dbconn;
		dbconn = (IDbConnection)new SqliteConnection (conn);
		dbconn.Open (); //Open connection to the database.
		IDbCommand dbcmd = dbconn.CreateCommand ();
		
		
		int countamount = 0;
		string sqlQuery = "SELECT COUNT(*) FROM Users WHERE username =='"+username+"'";
		dbcmd.CommandText = sqlQuery;
		IDataReader reader = dbcmd.ExecuteReader ();
		
		while (reader.Read()) {
			countamount = reader.GetInt32 (0);
			Debug.Log ("users found is "+countamount);
		}
		
		
		reader.Close ();
		reader = null;
		dbcmd.Dispose ();
		dbcmd = null;
		dbconn.Close ();
		dbconn = null;
		GC.Collect ();
		Debug.Log ("Disconnected from DB");
		
		if (countamount > 0) {
			returnval = true;
		}
		return returnval;
		
	}
	
	int getNextUserID()
	{
		//string conn = "URI=file:" + Application.dataPath + "/BombermanDB.s3db"; //Path to database.
		IDbConnection dbconn;
		dbconn = (IDbConnection)new SqliteConnection (conn);
		dbconn.Open (); //Open connection to the database.
		IDbCommand dbcmd = dbconn.CreateCommand ();
		
		
		int nextID = 999999;
		string sqlQuery = "SELECT COUNT(*) FROM Users";
		dbcmd.CommandText = sqlQuery;
		IDataReader reader = dbcmd.ExecuteReader ();
		
		while (reader.Read()) {
			nextID = reader.GetInt32 (0);
			nextID++;
			Debug.Log ("nextID is "+nextID);
		}
		
		
		reader.Close ();
		reader = null;
		dbcmd.Dispose ();
		dbcmd = null;
		dbconn.Close ();
		dbconn = null;
		GC.Collect ();
		Debug.Log ("Disconnected from DB");
		return nextID;
	}
	
	public string Md5Sum(string strToEncrypt)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(strToEncrypt);
		
		// encrypt bytes
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);
		
		// Convert the encrypted bytes back to a string (base 16)
		string hashString = "";
		
		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}
		
		return hashString.PadLeft(32, '0');
	}
	
	void OnDestroy()
	{
		listener.Disconnect(false);
	}

	
}

