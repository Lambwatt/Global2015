using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	private float velocity;
	private float direction;
	private Vector3 start;	
	private float range;
	public float laserSpeed;

	public void setParams(float v, float d, Vector3 s, float r){

		rigidbody2D.velocity = new Vector2( -(Mathf.Sin(d)*v*laserSpeed), Mathf.Cos(d)*v*laserSpeed);
		start = s;
		range = r;

	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {


		if( Vector3.Distance(transform.position, start) > range ){
			Destroy(this);
		}
	}

//	void OnCollisionEnter2D (Collision2D col)
//	{
//		
//		//Destroy(this.gameObject);
//		
//	}
}
