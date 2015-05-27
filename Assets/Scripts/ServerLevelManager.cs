using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Data;

public class ServerPlayer
{
	public GameObject bomberman;
	public int playerNum;
	public Socket client;
}

public struct PlayerAction
{
	public Socket client;
	public int playerNum;
	public string actionStr;
	
}

public class ServerLevelManager : MonoBehaviour 
{
	public static ServerLevelManager instance;
	public static Queue<PlayerAction> actionQueue = new Queue<PlayerAction>();
	
	public GameObject[] bManPrefabs;
	
	public List<ServerPlayer> playerList;
	
	void Awake()
	{
		if(instance != null && instance != this)
		{
			DestroyImmediate(gameObject);
			return;
		}
		
		instance = this;
		
		bManPrefabs = new GameObject[4];
		bManPrefabs[0] = Resources.Load("ServerMan") as GameObject;
		bManPrefabs[1] = Resources.Load("ServerMan2") as GameObject;
		bManPrefabs[2] = Resources.Load("ServerMan2") as GameObject;
		bManPrefabs[3] = Resources.Load("ServerMan2") as GameObject;
		
		//bManPrefab = Resources.Load("ServerMan") as GameObject;
		playerList = new List<ServerPlayer>();
	}
	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		//if queue != empty
		while(actionQueue.Count > 0)
		{
			PlayerAction action = actionQueue.Dequeue();
			string[] token = action.actionStr.Split(new Char[]{'|'});
			//Debug.Log ("Queue msg: " + token[0]);
			
			if(token[0] == "PlayerPos")
			{	
				for(int i = 0; i < playerList.Count; i++)
				{
					if(playerList[i] != null && playerList[i].client == action.client)
					{
						float x = float.Parse(token[1]);
						float y = float.Parse(token[2]);
						playerList[i].bomberman.transform.position = new Vector2(x,y);
						
					}
					else if(action.client != null && playerList[i] != null)
					{
						int movedPlayerNum = action.playerNum;
						string message = "EnemyPos|" + movedPlayerNum + "|" + token[1] + "|" + token[2] + "|<EOF>";
						SocketListener.Send(playerList[i].client, message);
					}

				}	
			}
			else if(token[0] == "BombDropped")
			{
				Debug.Log("action: " + action.actionStr);
				float x = float.Parse(token[1]);
				float y = float.Parse(token[2]);
				
				int index = action.playerNum - 1;
				
				ServerCharacter serverChar = playerList[index].bomberman.GetComponent<ServerCharacter>();
				
				
				serverChar.DropBomb(x, y);
				
				for(int i = 0; i < playerList.Count; i++)
				{
					
					if(playerList[i] != null && playerList[i].client != action.client)
					{
						int movedPlayerNum = action.playerNum;
						string message = "BombDropped|" + action.playerNum + "|" + token[1] + "|" + token[2] + "|<EOF>";
						SocketListener.Send(playerList[i].client, message);
					}
					
				}
			}
			else if(token[0] == "Disconnect")
			{
				Debug.Log("LevelManager: Disconnect");
				int index = action.playerNum - 1;
				
				if(index > -1 && playerList[index] != null)
				{
					Destroy(playerList[index].bomberman);
					playerList[index] = null;
				}
				
				for(int i = 0; i < playerList.Count; i++)
				{
					
					if(playerList[i] != null && playerList[i].client != action.client)
					{
						int movedPlayerNum = action.playerNum;
						string message = "Disconnect|" + action.playerNum + "|<EOF>";
						SocketListener.Send(playerList[i].client, message);
					}
					
				}
				
				
//				for(int i = 0; i < playerList.Count; i++)
//				{
//					
//					if(playerList[i].client != action.client && action.client != null)
//					{
//						int movedPlayerNum = action.playerNum;
//						string message = "BombDropped " + action.playerNum + " " + token[1] + " " + token[2] + " <EOF>";
//						SocketListener.Send(playerList[i].client, message);
//					}
//					
//				}
			}
			
		}
		
	}
	
	public void AddPlayer(Socket client)
	{	
		if(playerList.Count <= 4)
		{
			int playerNum = playerList.Count+1;
			ServerPlayer player = new ServerPlayer();
			player.bomberman = Instantiate(bManPrefabs[playerNum-1]) as GameObject;
			player.playerNum = playerNum;
			player.bomberman.GetComponent<ServerCharacter>().playerNum = playerNum;
			
			SetPlayerStartPosition(player.bomberman, playerNum);
			
			
			player.client = client;
			
			foreach(ClientInfo clientInfo in SocketListener.logedClients)
			{
				if(clientInfo.client == client)
				{
					player.bomberman.GetComponent<ServerCharacter>().playername = clientInfo.username;
				}
			}
			
			playerList.Add(player);
			Debug.Log("PlayerNum " + playerNum);
			string message = "PlayerNum|" + playerNum + "|<EOF>";
			//Send player number to newest player
			SocketListener.Send(client, message);
			
			for(int i = 0; i < playerList.Count; i++)
			{	
				if(playerList[i].client != client)
				{
					message = "NewPlayer|" + playerNum + "|<EOF>";
					//send new player message to existing players
					SocketListener.Send(playerList[i].client, message);
					//send a message to the new player for each existing player
					message = "NewPlayer|" + playerList[i].playerNum + "|<EOF>";
					SocketListener.Send(client, message);
				}
			}
		}
		
	}
	
	public void SetPlayerStartPosition(GameObject gameObj, int playerNum)
	{
		if(playerNum == 1)
		{
			gameObj.transform.position = new Vector2(1,9);
		}
		else if(playerNum == 2)
		{
			gameObj.transform.position = new Vector2(1,1);
		}
		else if(playerNum == 3)
		{
			gameObj.transform.position = new Vector2(11,9);
		}
		else if(playerNum == 4)
		{
			gameObj.transform.position = new Vector2(11,1);
		}
	}
	
}
