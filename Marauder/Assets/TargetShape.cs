using UnityEngine;
using System.Collections;

public class TargetShape : MonoBehaviour {
	
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
		if(elapsedTime<0){
			direction = Random.Range(0,360)*Mathf.Deg2Rad;
			elapsedTime = 2;
		}
		rigidbody2D.velocity += new Vector2( -(Mathf.Sin(direction)*velocity), Mathf.Cos(direction)*velocity);
	}

	void pursue(){

		Vector3 toPlayer = (player.transform.position - transform.position).normalized;
		rigidbody2D.velocity += new Vector2(toPlayer.x*velocity, toPlayer.y*velocity);
	}

	void OnTriggerEnter2D (Collider2D col)
	{
		string goal = player.getMode(label);

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
