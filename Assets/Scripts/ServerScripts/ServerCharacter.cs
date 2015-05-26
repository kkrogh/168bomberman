using UnityEngine;
using System.Collections;

public class ServerCharacter : MonoBehaviour {

	public int life;
	public int bombLimit;
	public int liveBombs;
	public int bombPower;
	public float speed;
	public bool kickItem;
	private Animator animator;
	public int deathCount;
	public bool dying;
	public GameObject bomb;
	
	void Awake()
	{
		animator = GetComponent<Animator> ();
		bomb = Resources.Load("ServerBomb") as GameObject;
		deathCount = 0;
		dying = false;
	}
	
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		//when the player dies, give them few frames of break with death animation
		
		if (deathCount > 1) {
			deathCount--;
		} else if(deathCount==1){
			this.transform.position = new Vector2 (1, 1);
			deathCount=0;
			dying=false;
		}
		
	}
	
	
	void OnTriggerEnter2D(Collider2D col)
	{
		//when the player dies
		if (col.tag == "Explosion" && !dying) 
		{
			life--;
			deathCount=90;
			//animator.SetTrigger ("Death");
			dying=true;
		}
		if (col.tag == "Item_BombPower") {
			this.bombPower++;
			Destroy(col.gameObject);
		}
	}
	
	public void DropBomb(float x, float y)
	{
		Debug.Log("DropBomb function");		
		Vector2 bombPos = new Vector2(x,y);
		
		GameObject obj = Instantiate(bomb, bombPos,transform.rotation) as GameObject;
		ServerBomb bombObj = obj.GetComponent<ServerBomb> ();
		bombObj.owner = this;
	}
}
