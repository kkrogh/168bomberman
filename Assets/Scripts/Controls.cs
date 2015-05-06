﻿using UnityEngine;
using System.Collections;

public class Controls : MonoBehaviour {
	public float runSpeed = 8f;
	public GameObject bomb;

	private Animator animator;
	public Character _Character;

	private Character bomberman;
	private Rigidbody2D rigidBody;
	// Use this for initialization
	void Start () {
		//bomb = Resources.Load ("asset/Bomb");
		bomberman = this.transform.GetComponent<Character> ();
		rigidBody = this.GetComponent<Rigidbody2D>();

		_Character = GetComponent<Character> ();

	}
	void Awake()
	{
		animator = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		//character can not move during dying animation
	if (!_Character.dying) {
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				animator.SetTrigger ("Up");
			}
			if (Input.GetKeyDown (KeyCode.DownArrow)) {
				animator.SetTrigger ("Down");
			}
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				animator.SetTrigger ("Right");
			}
			if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				animator.SetTrigger ("Left");
			}




			if (Input.GetKey (KeyCode.UpArrow)) {
				//transform.Translate(new Vector2(0,runSpeed*Time.deltaTime));
				rigidBody.MovePosition ((Vector2)this.transform.position + new Vector2 (0, 1) * runSpeed * Time.deltaTime);


			}
			if (Input.GetKey (KeyCode.DownArrow)) {
				//transform.Translate(new Vector2(0,-runSpeed*Time.deltaTime));
				rigidBody.MovePosition ((Vector2)this.transform.position + new Vector2 (0, -1) * runSpeed * Time.deltaTime);
				animator.SetInteger ("Direction", 3);
			}
			if (Input.GetKey (KeyCode.LeftArrow)) {
				//transform.Translate(new Vector2(-runSpeed*Time.deltaTime,0));
				rigidBody.MovePosition ((Vector2)this.transform.position + new Vector2 (-1, 0) * runSpeed * Time.deltaTime);
				animator.SetInteger ("Direction", 4);
			}
			if (Input.GetKey (KeyCode.RightArrow)) {
				//transform.Translate(new Vector2(runSpeed*Time.deltaTime,0));
				rigidBody.MovePosition ((Vector2)this.transform.position + new Vector2 (1, 0) * runSpeed * Time.deltaTime);

			}

			if (Input.GetKeyDown (KeyCode.Space)) {
				float x = Mathf.Round (this.transform.position.x);
				float y = Mathf.Round (this.transform.position.y);
				Collider2D[] colObjs = Physics2D.OverlapCircleAll (new Vector2 (x, y), 0.3f);
				bool bombPlaced = false;
			
				foreach (Collider2D col in colObjs) {
					if (col.tag == "Bomb") {
						bombPlaced = true;
					}
				}
			
				if (bomberman.liveBombs < bomberman.bombLimit && !bombPlaced) {
					DropBomb ();
					bomberman.liveBombs++;
				}

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
