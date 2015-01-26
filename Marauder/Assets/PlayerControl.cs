using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Goal{

	public string target;
	public string type;//shoot, touch, avoid, free otherwise
	public float time;
	public int num;

	public Goal(string tar, string typ, int nu, float tim){
		target = tar;
		type = typ;
		time = tim;
		num = nu;
	}
}

public class PlayerControl : MonoBehaviour {

	public delegate void EndGame();
	public static event EndGame onEndGame;

	public GameObject projectile1;
	public GameObject[] shapes;
	public string[] shapeNames;
	public float minDistance;
	public float maxDistance;
	public int shapeCount;
	public float velocity;
	private int points;
	private int lives;
	private int goalsAccomplished;

	private bool inMenu = true;
	private bool gameOver = false;

	//Things that are in the UI
	private GameObject UIPointer;
	private Text count;
	private Image img;
	private Text time;
	private Text objective;
	private Text livesCount;
	private Text goalCount;


	private GameObject timeFields;
	private GameObject countFields;

	private GameObject gameOverScreen;
	private GameObject startScreen;


	private string[] goalTypes = {"collect","avoid","shoot"};

	//Things that are changed on goal swap
	private Goal[] goals;


	void Awake(){
		this.gameObject.renderer.enabled = false;
	}

	// Use this for initialization
	void Start () {

		gameOverScreen = GameObject.Find("GameOver");
		gameOverScreen.SetActive(false);
		startScreen = GameObject.Find("MainMenu");
		UIPointer = GameObject.Find("Goal");
		UIPointer.SetActive(false);

		count = UIPointer.transform.FindChild("CountFields").FindChild("count").GetComponent<Text>();

		img = UIPointer.transform.FindChild("Image").GetComponent<Image>();
		time = UIPointer.transform.FindChild("TimeFields").FindChild("time").GetComponent<Text>();
		objective = UIPointer.transform.FindChild("Objective").GetComponent<Text>();
		livesCount = UIPointer.transform.FindChild("Lives").GetComponent<Text>();
		goalCount = gameOverScreen.transform.FindChild("GoalCount").GetComponent<Text>();
//		Debug.Log (gameOverScreen.transform);
//		Debug.Log (gameOverScreen.transform.FindChild("GoalCount"));
//		Debug.Log (gameOverScreen.transform.FindChild("GoalCount").GetComponent<Text>());
		
		timeFields = UIPointer.transform.FindChild("TimeFields").gameObject;
		countFields = UIPointer.transform.FindChild("CountFields").gameObject;

	}

	void startGame(){

		UIPointer.SetActive(true);
		startScreen.SetActive(false);
		goalsAccomplished = 0;
		goals = new Goal[]{new Goal("diamond", "shoot", 10, 0)};
		objective.text = ""+goals[0].type;
		count.text = ""+goals[0].num;
		gameObject.renderer.enabled = true;
		transform.position = new Vector3(0.0f,0.0f,0.0f);
		inMenu = false;
		lives = 3;
		livesCount.text = ""+lives;

		timeFields.SetActive(false);
		countFields.SetActive(true);

		for(int  i = 0; i<shapeCount; i++){
			int selection = (int)Mathf.Floor(Random.Range(0,shapes.Length));
			//Debug.Log(selection);
			//Debug.Log(shapes[selection]);
			GameObject shape = Instantiate(shapes[selection], 
			                               new Vector3(transform.position.x + Random.Range(-maxDistance, maxDistance), 
			            transform.position.y+Random.Range(-maxDistance, maxDistance)), 
			                               Quaternion.identity) as GameObject;
			
			shape.GetComponent<TargetShape>().setParams(shapeNames[selection]);
			
		}

		inMenu = false;


	}

	void goToStartMenu(){

		onEndGame();
		gameOver = false;
		inMenu = true;
		gameOverScreen.SetActive(false);
		startScreen.SetActive(true);

		//Hide game over and show start menu
	}

	void goToGameOver(){

		//Explode, wait, and go to game over screen;
		//hide goals and Show Game over screen
		Debug.Log(goalsAccomplished);
		Debug.Log(goalCount);
		goalCount.text = ""+goalsAccomplished;
		gameOver = true;
		this.gameObject.renderer.enabled = false;
		UIPointer.SetActive(false);
		gameOverScreen.SetActive(true);


	}


	void Update () {

		if(inMenu){
			if (Input.GetKeyUp(KeyCode.Return)) {
				startGame();
			}
			return;
		}

		if(gameOver){
			if (Input.GetKeyUp(KeyCode.Return)) {
				goToStartMenu();
			}
			return;
		}

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

			GameObject laser2 = Instantiate(projectile1, transform.position + new Vector3(Mathf.Cos(a)*(-0.5f), Mathf.Sin(a)*(-0.5f), 0), transform.rotation) as GameObject;
			laser2.GetComponent<Projectile>().setParams(5, a, transform.position, 10);

			//spawn left
		}

		if(goals[0].type=="avoid"){
			goals[0].time-=Time.deltaTime;
			time.text = ""+Mathf.Ceil(goals[0].time);
			if(goals[0].time <= 0){
				goalsAccomplished++;
				setGoal();
			}
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
			goalsAccomplished++;
			setGoal();
		}
		else{
			count.text = ""+goals[0].num;
		}
	}

	public void setGoal(){
		//swap goal
		Debug.Log("Accomplished goal");
		string gT = goalTypes[(int)Mathf.Floor(Random.Range(0,goalTypes.Length))];
		int shapeIndex = (int)Mathf.Floor(Random.Range(0,shapes.Length));
	
		if(gT=="avoid"){
			goals[0] = new Goal(shapeNames[shapeIndex],gT,0,10.0f);
			img.sprite = shapes[shapeIndex].GetComponent<SpriteRenderer>().sprite;
			time.text = ""+Mathf.Ceil(goals[0].time);
			timeFields.SetActive(true);//.GetComponent<CanvasRenderer>().enabled = true;
			countFields.SetActive(false);//.GetComponent<CanvasRenderer>().enabled = false;
			//set Image

		}else{

			//
			goals[0] = new Goal(shapeNames[shapeIndex],gT,10,0.0f);
			img.sprite = shapes[shapeIndex].GetComponent<SpriteRenderer>().sprite;
			count.text = ""+goals[0].num;
			timeFields.SetActive(false);//GetComponent<CanvasRenderer>().renderer.enabled = false;
			countFields.SetActive(true);//.GetComponent<CanvasRenderer>().renderer.enabled = true;
		}
		objective.text = gT;

	}

	public void die(){

		if(lives>1){
			lives--;
			livesCount.text = ""+lives;
			setGoal();
		}else{
			goToGameOver();

		}
	}
}
