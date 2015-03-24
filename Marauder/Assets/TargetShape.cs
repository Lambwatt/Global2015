using UnityEngine;
using System.Collections;

//PlayerControl-------------------------------------------------------
//Contains logic for managing each shape
public class TargetShape : MonoBehaviour {

	//variables set in editor----------------------------------
	public float velocity;
	public float threshold;
	public string label;

	//internal variables
	private float direction;
	private PlayerControl player;
	private float elapsedTime;

	//set up pointer to player and register event
	void Start () {
		player = GameObject.Find("Player").GetComponent<PlayerControl>();
		PlayerControl.onEndGame+=dieOnEnd;
	}

	//set label
	public void setParams(string l){
		label = l;
	}

	//destroy self
	private void dieOnEnd(){
		Destroy(this.gameObject);
	}

	//de-register event
	void OnDestroy(){
		PlayerControl.onEndGame-=dieOnEnd;
	}

	//behaviour loop
	void Update () {

		//check if far enough from player to die and replace
		if(Vector3.Distance(player.GetComponent<Transform>().position, GetComponent<Transform>().position)>threshold){
			Destroy(this.gameObject);
			player.replaceShape();
		}

		//decide if flying randomly or following player based on goal of shape
		if(player.getMode(label)=="hostile"){
			pursue();
		}else{
			fly();
		}
	}

	//fly in a random direction
	void fly(){
		elapsedTime -= Time.deltaTime;
		if(elapsedTime<0){
			direction = Random.Range(0,360)*Mathf.Deg2Rad;
			elapsedTime = 2;
		}
		GetComponent<Rigidbody2D>().velocity += new Vector2( -(Mathf.Sin(direction)*velocity), Mathf.Cos(direction)*velocity);
	}

	//fly toward the player
	void pursue(){

		Vector3 toPlayer = (player.GetComponent<Transform>().position - GetComponent<Transform>().position).normalized;
		GetComponent<Rigidbody2D>().velocity += new Vector2(toPlayer.x*velocity, toPlayer.y*velocity);
	}

	//resolve collisions by adding a point or hurting the player
	void OnTriggerEnter2D (Collider2D col)
	{

		//only count player collision
		if(col.gameObject.name == "Player"){

			string goal = player.getMode(label);
			if(goal=="collect"){
				player.addPoint();
				Destroy(this.gameObject);
			}else if(goal=="dangerous"||goal=="hostile"){
				player.die();
			}
		}
	}
}
