using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Goal{

	public string target;
	public string type;//shoot, touch, avoid, free otherwise
	//public float timeout;
	public int num;

	public Goal(string tar, string typ, int nu, float tim){
		target = tar;
		type = typ;
		//time = tim;
		num = nu;
	}
}

public class PlayerControl : MonoBehaviour {

	public GameObject projectile1;
	public GameObject[] shapes;
	public string[] shapeNames;
	public float minDistance;
	public float maxDistance;
	public int shapeCount;
	public float velocity;
	private int points;
	private int level;

	//Things that are in the UI
	private GameObject UIPointer;
	private Text count;


	//Things that are changed on goal swap
	private Goal[] goals = {new Goal("diamond", "avoid", 10, 0)};


	void Awake(){
		for(int  i = 0; i<shapeCount; i++){
			int selection = (int)Mathf.Floor(Random.Range(0,shapes.Length));
			Debug.Log(selection);
			Debug.Log(shapes[selection]);
			GameObject shape = Instantiate(shapes[selection], 
                       new Vector3(transform.position.x + Random.Range(-maxDistance, maxDistance), 
    								transform.position.y+Random.Range(-maxDistance, maxDistance)), 
                       				Quaternion.identity) as GameObject;

			shape.GetComponent<TargetShape>().setParams(shapeNames[selection]);
		}
	}

	// Use this for initialization
	void Start () {

		UIPointer = GameObject.Find("Goal");
		count = UIPointer.transform.FindChild("count").GetComponent<Text>();
		count.text = ""+goals[0].num;
	}


	void Update () {

		int arrowIn = 0; //< ^ v >
		
		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
			arrowIn+=1;
		}

		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
			arrowIn += 8;
		}
		
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
			arrowIn += 4;
		}

		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
			arrowIn += 2;
		}

		switch(arrowIn){

			case 1:
			case 7:
				transform.eulerAngles = new Vector3(0,0,270);
				break;
			case 2:
			case 11:
			transform.eulerAngles = new Vector3(0,0,180);
				break;
			case 3:
                transform.eulerAngles = new Vector3(0,0,225);
				break;
			case 4:
			case 13:
			     transform.eulerAngles = new Vector3(0,0,0);
				break;
			case 5:
				transform.eulerAngles = new Vector3(0,0,315);
				break;
			case 8:
			case 14:
				transform.eulerAngles = new Vector3(0,0,90);
				break;
			case 10:
				transform.eulerAngles = new Vector3(0,0,135);
				break;
			case 12:
				transform.eulerAngles = new Vector3(0,0,45);
				break;
			case 0:
			case 6:
			case 9:
			case 15:
			default:
				break;
		}
		//camera.transform.eulerAngles = new Vector3(0,0,0);

		float a = Mathf.Deg2Rad*transform.eulerAngles.z;
		//float velocity = 0.0f;//5.0f;
		rigidbody2D.velocity = new Vector2( -(Mathf.Sin(a)*velocity), Mathf.Cos(a)*velocity);

		if(Input.GetKeyDown(KeyCode.Space)){

			//spawn right
			GameObject laser = Instantiate(projectile1, transform.position + new Vector3(Mathf.Cos(a)*0.5f, Mathf.Sin(a)*0.5f, 0), transform.rotation) as GameObject;
			laser.GetComponent<Projectile>().setParams(5, a, transform.position, 10);

			GameObject laser2 = Instantiate(projectile1, transform.position + new Vector3(Mathf.Cos(a)*0/*(-0.5f)*/, Mathf.Sin(a)*(-0.5f), 0), transform.rotation) as GameObject;
			laser2.GetComponent<Projectile>().setParams(5, a, transform.position, 10);

			//spawn left
		}

		//Debug.Log(GameObject.FindGameObjectsWithTag("Shape").Length);

	}

	public void replaceShape(){
		float angle = Mathf.Deg2Rad*(transform.eulerAngles.z + Random.Range(-80.0f, 80.0f));
		float magnitude = Random.Range(minDistance, maxDistance);

		Instantiate(shapes[(int)Mathf.Floor(Random.Range(0,shapes.Length))], new Vector3(transform.position.x -(Mathf.Sin(angle)*magnitude), transform.position.y+Mathf.Cos(angle)*magnitude), Quaternion.identity);
	}

	public string getMode(string label){
		foreach(Goal g in goals){
			if(g.target == label){
				return g.type;
			}
		}

		return "free";
	}

	public void checkGoal(){

		goals[0].num--;
		if(goals[0].num<=0){
			accomplishGoal();
		}
		else{
			count.text = ""+goals[0].num;
		}
		//update goal tracking
		//if()
		//Check goal

	}

	public void accomplishGoal(){
		//swap goal
		Debug.Log("Accomplished goal");
		goals[0].num = 10;
		count.text = ""+goals[0].num;
	}
}
