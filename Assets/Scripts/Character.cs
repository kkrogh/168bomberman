using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

	public int life;
	public int bombLimit;
	public int liveBombs;
	public int bombPower;
	public float speed;
	public bool kickItem;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}


	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.tag == "Explosion") 
		{
			life--;
			this.transform.position = new Vector2(1,1);
		}
	}
}
