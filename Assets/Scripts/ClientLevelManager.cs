using UnityEngine;
using System.Collections;

public enum GameState
{	
	Loading,
	WaitingForPlayers,
	Playing
};

public class ClientLevelManager : MonoBehaviour 
{
	public static ClientLevelManager instance = null;
	
	public GameState gameState = GameState.WaitingForPlayers;
	public GameObject[] bManPrefabs;
	
	public GameObject player;
	public int playerNum;
	
	GameObject[] playerArray;
	
	private float timer = 0;
	
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
	// Use this for initialization
	void Start () 
	{
		bManPrefabs = new GameObject[4];
		bManPrefabs[0] = Resources.Load("ClientMan") as GameObject;
		bManPrefabs[1] = Resources.Load("ClientMan") as GameObject;
		
		player = GameObject.Find("Bomberman");
		playerNum = 0;
		
		playerArray = new GameObject[4];
		
		StateObject send_so = new StateObject();
		send_so.workSocket = AsynchronousClient.client;
		AsynchronousClient.Send(AsynchronousClient.client,"Loaded <EOF>", send_so);
		send_so.sendDone.WaitOne(5000);
		Debug.Log("sent loaded");
	}
	
	// Update is called once per frame
	void Update () 
	{
		
		if(timer > 0.05)
		{
		
//			string content =  "PlayerPos " + ClientLevelManager.instance.playerNum + " " + player.transform.position.x.ToString () 
//				+ " " + player.transform.position.y.ToString () + " <EOF>";
//			StateObject send_so = new StateObject ();
//			send_so.workSocket = AsynchronousClient.client;
//			AsynchronousClient.Send (AsynchronousClient.client, content, send_so);
			
			timer = 0;
		}
		
		timer = timer + Time.deltaTime;
	}
	
	
	
	void OnGUI()
	{
		if(gameState == GameState.WaitingForPlayers)
		{
			
		}
		
	}
	
	public void LoadPlayer(int playerNum)
	{
		this.playerNum = playerNum;
		int index = playerNum - 1;
		playerArray[index] = player;
		
		if(playerNum == 1)
		{
			player.transform.position = new Vector2(1,9);
			Debug.Log("pos 1 9");
		}
		else if(playerNum == 2)
		{
			player.transform.position = new Vector2(1,1);
			Debug.Log("pos 1 1");
		}
	}
	
	public void AddEnemy(int enemyNum)
	{
		int index = enemyNum - 1;
		playerArray[index] = Instantiate(bManPrefabs[enemyNum-1]) as GameObject;
		
		if(enemyNum == 1)
		{
			playerArray[index].transform.position = new Vector2(1,9);
		}
		else if(enemyNum == 2)
		{
			playerArray[index].transform.position = new Vector2(1,1);
		}
	}
	
	public void SetPlayerPos(int playerNum, float x, float y)
	{
		int index = playerNum - 1;
		
		if(index > -1 && index < 4 && playerArray[index] != null)
		{
			playerArray[index].transform.position = new Vector2(x,y);
		}
	}
	
	public void ClientDropBomb(int playerNum, float x, float y)
	{
		int index = playerNum - 1;
		
		if(index > -1 && index < 4 && playerArray[index] != null)
		{
			Character clientChar = playerArray[index].GetComponent<Character>();
			clientChar.DropBomb(x,y);
		}
		
		
		
	}
}
