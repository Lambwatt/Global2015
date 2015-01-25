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
	}

	public void setParams(string l){
		label = l;
	}
//	public void setParams(string l){
//		label = l;
//	}

	// Update is called once per frame
	void Update () {
		if(player.getMode(label)=="avoid"){
			//Debug.Log("run");
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

	void OnCollisionEnter2D (Collision2D col)
	{
		if(col.gameObject.name == "Laser(Clone)")
		{
			//Should spawn explosion
			Destroy(this.gameObject);
			Destroy(col.gameObject);
			player.replaceShape();
			player.checkGoal();
		}
	}
}
