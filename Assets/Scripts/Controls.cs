using UnityEngine;
using System.Collections;

public class Controls : MonoBehaviour {
	public float runSpeed = 8f;
	public GameObject bomb;

	private Character bomberman;
	// Use this for initialization
	void Start () {
		//bomb = Resources.Load ("asset/Bomb");
		bomberman = this.transform.GetComponent<Character> ();
	}
	
	// Update is called once per frame
	void Update () {
	
		if( Input.GetKey( KeyCode.UpArrow ) )
		{
			transform.Translate(new Vector2(0,runSpeed*Time.deltaTime));
		}
		if( Input.GetKey( KeyCode.DownArrow ) )
		{
			transform.Translate(new Vector2(0,-runSpeed*Time.deltaTime));
		}
		if( Input.GetKey( KeyCode.LeftArrow ) )
		{
			transform.Translate(new Vector2(-runSpeed*Time.deltaTime,0));
		}
		if( Input.GetKey( KeyCode.RightArrow ) )
		{
			transform.Translate(new Vector2(runSpeed*Time.deltaTime,0));
		}

		if( Input.GetKeyDown( KeyCode.Space ) )
		{
			if(bomberman.liveBombs < bomberman.bombLimit)
			{
				DropBomb();
				bomberman.liveBombs++;
			}

		}


	}


	void DropBomb()
	{


		float x = Mathf.Round (this.transform.position.x);
		float y = Mathf.Round (this.transform.position.y);
		Vector2 bombPos = new Vector2(x,y);


		GameObject obj = Instantiate (bomb, bombPos,transform.rotation) as GameObject;
		Bomb bombObj = obj.GetComponent<Bomb> ();
		bombObj.owner = this.bomberman;
	}


}
