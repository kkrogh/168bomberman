using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class LobbyScript : MonoBehaviour {

	SocketListener serverInstance;
	AsynchronousClient clientInstance;

	public Text userstext;
	public Text chattext;

	public InputField inputfield;


	private string userstextstring = "";
	private string chattextstring = "";
	
		// Use this for initialization
	void Start () {

		serverInstance = SocketListener.instance;
		clientInstance = AsynchronousClient.instance;
			
		userstextstring = "";
		//userstext.text = userstextstring;
		chattextstring = "";

	}
	
	// Update is called once per frame
	void Update () {

		userstext.text = userstextstring;
		chattext.text = chattextstring;
		
	}

	void OnGUI(){

		if(inputfield.isFocused && inputfield.text != "" && Input.GetKey(KeyCode.Return)) {
			sendChat();
			inputfield.text = "";
		}

		chattextstring = "";
		userstextstring = "";
		string[] stringy = new string[5];
		ClientAction.lobbyInfo.chatstrings.CopyTo (stringy, 0);

		foreach (string s in stringy) {
			chattextstring+= "\n"+s;

		}
		foreach (string s in ClientAction.lobbyInfo.usersstrings) {
			userstextstring+= "\n"+s;
			
		}

		userstext.text = userstextstring;
		chattext.text = chattextstring;

	}


	public void sendChat()
	{
		Debug.Log ("sending chat");
		string content = "Chat|" + clientInstance.playername +":" + inputfield.text + "|<EOF>";
		StateObject send_so = new StateObject ();
		send_so.workSocket = AsynchronousClient.client;
		AsynchronousClient.Send (AsynchronousClient.client, content, send_so);
		send_so.sendDone.WaitOne(5000);
		Debug.Log("chat sent");

		//chattextstring += inputfield.text;
	}

	public void sendLoadGame(int session)
	{
		AsynchronousClient.instance.session = session;
		StateObject send_so = new StateObject();
		send_so.workSocket = AsynchronousClient.client;
		AsynchronousClient.Send(AsynchronousClient.client,"StartSession|"+session+"|<EOF>", send_so);
		send_so.sendDone.WaitOne(5000);
	}


}
