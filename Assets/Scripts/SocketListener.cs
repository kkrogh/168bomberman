using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

public class SocketListener : MonoBehaviour 
{
	public static SocketListener instance;
	// Thread signal.
	public static ManualResetEvent allDone = new ManualResetEvent(false);
	public static List<Socket> clients = new List<Socket>();
	
	private static bool accepted = true;
	
	byte[] bytes = new Byte[1024];
	
	// Create a TCP/IP socket.
	Socket listener;
	
	//Listen to external IP address
	IPHostEntry ipHostInfo;
	IPAddress ipAddress;
	IPEndPoint localEndPoint;
	
	// Listen to any IP Address
	IPEndPoint any;
	
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
		bytes = new Byte[1024];
		
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
		
		if(accepted)
		{
			accepted = false;
			Console.WriteLine("Waiting for a connection..");
			Debug.Log("Waiting for a connection..");
			listener.BeginAccept( 
			                     new AsyncCallback(AcceptCallback),
			                     listener );
		}

		// Wait until a connection is made before continuing.
		//allDone.WaitOne();
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
		accepted = true;	//static
		
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
		Debug.Log("ReadCallback");
		// Retrieve the state object and the handler socket
		// from the asynchronous state object.
		StateObject state = (StateObject) ar.AsyncState;
		Socket handler = state.workSocket;
		
		// Read data from the client socket. 
		int bytesRead = handler.EndReceive(ar);
		
		if (bytesRead > 0) {
			// There  might be more data, so store the data received so far.
			state.sb.Append(Encoding.ASCII.GetString(
				state.buffer,0,bytesRead));
			
			// Check for end-of-file tag. If it is not there, read 
			// more data.
			content = state.sb.ToString();
			if (content.IndexOf("<EOF>") > -1) {
				// All the data has been read from the 
				// client. Display it on the console.
				Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
				                  content.Length, content );
				Console.WriteLine("\n\n");
				Debug.Log("Read " + content.Length + " bytes from socket. \n Data : " + content );
				// Echo the data back to the client.
				//Send(handler, content);
				
				//
				SocketListener.instance.MessageHandler(handler, content);
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
	
	public static void Send(Socket handler, String data) {
		// Convert the string data to byte data using ASCII encoding.
		byte[] byteData = Encoding.ASCII.GetBytes(data);
		
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
			Debug.Log("Sent " + bytesSent + " bytes to client.");
		} catch (Exception e) {
			Console.WriteLine(e.ToString());
		}
	}
	
	//Handle all client messages here
	public void MessageHandler(Socket client, string message)
	{
		string[] token = message.Split(new Char[]{' '});
		//parse strings here. Example:
		if(token[0] == "Login")
		{
			Send(client, "name: " + token[1] + "  ps: " + token[2]);
			
		}
		else if(token[0] == "Register")
		{
			//
		}
		else if(token[0] == "This")
		{
			Send(client, message);
		}
		
	}
}

