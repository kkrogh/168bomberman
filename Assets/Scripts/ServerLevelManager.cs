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
	public Socket client;
}

public struct PlayerAction
{
	public Socket client;
	public string actionStr;
	
}

public class ServerLevelManager : MonoBehaviour 
{
	public static Queue<PlayerAction> actionQueue = new Queue<PlayerAction>();
	
	public GameObject[] bManPrefabs;
	
	public List<ServerPlayer> playerList;
	
	void Awake()
	{
		bManPrefabs = new GameObject[4];
		bManPrefabs[0] = Resources.Load("ServerMan") as GameObject;
		bManPrefabs[1] = Resources.Load("ServerMan2") as GameObject;
		
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
			string[] token = action.actionStr.Split(new Char[]{' '});
			
			if(token[0] == "PlayerPos")
			{
				foreach(ServerPlayer obj in playerList)
				{
					if(obj.client == action.client)
					{
						float x = float.Parse(token[1]);
						float y = float.Parse(token[2]);
						obj.bomberman.transform.position = new Vector2(x,y);
						
					}

				}	
				
//				foreach(ServerPlayer obj in playerList)
//				{
//					if(obj.client != action.client)
//					{
//						float x = float.Parse(token[1]);
//						float y = float.Parse(token[2]);
//						obj.bomberman.transform.position = new Vector2(x,y);
//						
//					}
//					
//				}	
			}
			
		}
		
	}
	
	public void AddPlayer(Socket client)
	{
		ServerPlayer player = new ServerPlayer();
		if(playerList.Count == 0)
		{
			player.bomberman = Instantiate(bManPrefabs[0]) as GameObject;
		}
		else
		{
			player.bomberman = Instantiate(bManPrefabs[1]) as GameObject;
		}
		
		player.client = client;
		playerList.Add(player);
		
		for(int i = 0; i < playerList.Count; i++)
		{
			string message = "PlayerNum " + (i+1) + " <EOF>";
			
			SocketListener.Send(playerList[i].client, message);
		}
	}
}
