using UnityEngine;
using System.Collections;

public class TargetShape : MonoBehaviour {

	//private string mode = "target";
	private float direction;
	public float velocity;
	public float threshold;
	private PlayerControl player;
	private float elapsedTime;
	public string label;

	// Use this for initialization
	void Start () {
		player = GameObject.Find("Player").GetComponent<PlayerControl>();
		PlayerControl.onEndGame+=dieOnEnd;
		//player.onSolid(shape);
	}

	public void setParams(string l){
		label = l;
	}


	private void dieOnEnd(){
		Destroy(this.gameObject);
	}

	void OnDestroy(){
		PlayerControl.onEndGame-=dieOnEnd;
	}

//	private void hitbox(string l){
//		if(label==l){
//			//Add hitbox
//		}else{
//
//		}
//	}


	// Update is called once per frame
	void Update () {
		if(player.getMode(label)=="hostile"){
			pursue();
		}else{
			fly();
		}

		if(Vector3.Distance(player.transform.position, transform.position)>threshold){
			Destroy(this.gameObject);
			player.replaceShape();
		}
	}

	void fly(){
		elapsedTime -= Time.deltaTime;
		//Debug.Log(elapsedTime);
		if(elapsedTime<0){
			direction = Random.Range(0,360)*Mathf.Deg2Rad;
			elapsedTime = 2;
		}
		rigidbody2D.velocity += new Vector2( -(Mathf.Sin(direction)*velocity), Mathf.Cos(direction)*velocity);
	}

	void pursue(){
		//direction = Vector3.Angle(transform.position, player.transform.position);
		Vector3 toPlayer = (player.transform.position - transform.position).normalized;
		//Debug.Log (toPlayer);
		rigidbody2D.velocity += new Vector2(toPlayer.x*velocity, toPlayer.y*velocity);//toPlayer;//new Vector2( -(Mathf.Sin(direction)*velocity), Mathf.Cos(direction)*velocity);
	}

	void OnTriggerEnter2D (Collider2D col)
	{
		string goal = player.getMode(label);
		//Debug.Log (label+"'s goal is "+goal);
//		if(goal!="hunt")
//		{
//			//Should spawn explosion
//
//			//Destroy(col.gameObject);
//			player.replaceShape();
//			Destroy(this.gameObject);			
//			//if(goal=="shoot")
////				player.checkGoal();
//		}

		if(col.gameObject.name == "Player"){
			if(goal=="collect"){
				player.addPoint();
				Destroy(this.gameObject);
			}

			if(goal=="dangerous"||goal=="hostile"){
				player.die();
			}
		}
	}
}
