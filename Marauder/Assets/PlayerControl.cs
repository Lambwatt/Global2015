using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour {

	public delegate void EndGame();
	public static event EndGame onEndGame;

	//Audio clips. Set outside.
	public AudioClip avoid;
	public AudioClip shoot;
	public AudioClip collect;
	public AudioClip dieSound;
	public AudioClip point;
	public AudioClip damage;

	//Game paramaters
	public GameObject projectile1;
	public GameObject playerSprite;
	public GameObject[] shapes;
	public GameObject[] sprites;
	public string[] shapeNames;
	public float minDistance;
	public float maxDistance;
	public int shapeCount;
	public float velocity;
	private int points;
	private int lives;
	private int goalsAccomplished;//Posibly unnecessary

	//Canvas controls
	private bool inMenu = true;
	private bool gameOver = false;

	//Things that are in the UI
	private GameObject UIPointer;
	private GameObject livesHolder;
	private Text shapesCollected;
	private GameObject collectHolder;
	private GameObject solidHolder;
	private GameObject dangerousHolder;
	private GameObject hostileHolder;

	private GameObject gameOverScreen;
	private GameObject startScreen;


	private string[] goalTypes = {"collect", "dangerous", "hostile"};//{"collect","hurt","solid","hunt"};

	//Things that are changed on goal swap
	Dictionary<string, string> goals = new Dictionary <string,string>();
	Dictionary<string, GameObject> shapeList = new Dictionary <string,GameObject>();

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

		livesHolder = UIPointer.transform.FindChild("LivesList").gameObject;
		collectHolder = UIPointer.transform.FindChild("CollectableList").gameObject;
		shapeList.Add("collect", collectHolder);
//		solidHolder = UIPointer.transform.FindChild("SolidList").gameObject;
//		shapeList.Add("solid", solidHolder);
		dangerousHolder = UIPointer.transform.FindChild("DangerousList").gameObject;
		shapeList.Add("dangerous", dangerousHolder);
		hostileHolder = UIPointer.transform.FindChild("HostileList").gameObject;
		shapeList.Add("hostile", hostileHolder);
		shapesCollected = gameOverScreen.transform.FindChild("GoalCount").GetComponent<Text>();

	}

	void startGame(){

		UIPointer.SetActive(true);
		startScreen.SetActive(false);
		goalsAccomplished = 0;
		gameObject.renderer.enabled = true;
		transform.position = new Vector3(0.0f,0.0f,0.0f);
		inMenu = false;
		lives = 3;
		for (int j = 0; j<lives; j++){
			GameObject img = Instantiate(playerSprite) as GameObject;
			img.AddComponent<Image>();
			
			img.GetComponent<Image>().sprite = playerSprite.GetComponent<SpriteRenderer>().sprite; 
			
			img.transform.SetParent(livesHolder.transform, false);
			img.name = "life";
		}


		for(int  i = 0; i<shapeCount; i++){
			int selection = zeroToN(shapes.Length);
			GameObject shape = Instantiate(shapes[selection], 
			                               new Vector3(transform.position.x + Random.Range(-maxDistance, maxDistance), 
			            transform.position.y+Random.Range(-maxDistance, maxDistance)), 
			                               Quaternion.identity) as GameObject;
			
			shape.GetComponent<TargetShape>().setParams(shapeNames[selection]);
			
		}

		changeGoals();
		inMenu = false;
		//audio.PlayOneShot(shoot, 0.7F);


	}

	//Goal management---------------------------------------------------------------
	void addGoal(KeyValuePair<string, string> kp){

		goals.Add(kp.Key, kp.Value);

		GameObject img = Instantiate(sprites[shapeIndexOf(kp.Key)]) as GameObject;
		img.AddComponent<Image>();

		img.GetComponent<Image>().sprite = shapes[shapeIndexOf(kp.Key)].GetComponent<SpriteRenderer>().sprite; 

		img.transform.SetParent(shapeList[kp.Value].transform, false);
		img.name = kp.Key;

	}

	void finishGoal(string k){

		GameObject listPointer = null;
		if(shapeList.TryGetValue(goals[k], out listPointer)){
			GameObject img = shapeList[goals[k]].transform.FindChild(k).gameObject;//listPointer.transform.FindChild(k).gameObject;
			Destroy(img);
		}
		goals.Remove(k);
	}

	void changeGoals(){

		foreach(KeyValuePair<string, string> kp in goals){
			if(kp.Value == "collect"){
				finishGoal(kp.Key);
				break;
			}
		}

		if(goals.Count>0){
			string[] keys = new string[goals.Count];
			goals.Keys.CopyTo(keys, 0);
			string k = keys[zeroToN(goals.Count)];
			finishGoal(k);
		}

		//generate new goals and add them
		addGoal(generateGoal(true));
		addGoal(generateGoal(false));

	}

	//generates and adds a goal
	KeyValuePair<string, string> generateGoal(bool forceCollect){

		List<string> freeShapes = new List<string>();
		foreach(string k in shapeNames){
			if(!goals.ContainsKey(k)){
				freeShapes.Add(k);
			}
		}

		if(forceCollect){
			return new KeyValuePair<string, string>(freeShapes.ToArray()[zeroToN(freeShapes.Count)],"collect");
		}else{
			return new KeyValuePair<string, string>(freeShapes.ToArray()[zeroToN(freeShapes.Count)],goalTypes[zeroToN(goalTypes.Length)]);
		}
	}

	public string getMode(string label){
		string objective = null;
		if(goals.TryGetValue(label, out objective)){
			return objective;
		}
		return "free";
	}

	//UI management--------------------------------------------
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
		audio.PlayOneShot(dieSound, 0.7F);
		shapesCollected.text = ""+goalsAccomplished;
		gameOver = true;
		this.gameObject.renderer.enabled = false;
		UIPointer.SetActive(false);
		gameOverScreen.SetActive(true);


	}

	//game loop---------------------------------------------
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

		float a = Mathf.Deg2Rad*transform.eulerAngles.z;
		rigidbody2D.velocity = new Vector2( -(Mathf.Sin(a)*velocity), Mathf.Cos(a)*velocity);

	}
	
	public void replaceShape(){
		float angle = Mathf.Deg2Rad*(transform.eulerAngles.z + Random.Range(-80.0f, 80.0f));
		float magnitude = Random.Range(minDistance, maxDistance);

		Instantiate(shapes[zeroToN(shapes.Length)], new Vector3(transform.position.x -(Mathf.Sin(angle)*magnitude), transform.position.y+Mathf.Cos(angle)*magnitude), Quaternion.identity);
	}



	public void addPoint(){

		points++;
		audio.PlayOneShot(collect, 0.7F);
		shapesCollected.text = ""+points;
		if(points%10==0){
			changeGoals();
		}

	}

	public void die(){
		if(lives<0)
			return;

		audio.PlayOneShot(damage, 0.7F);
		Destroy(livesHolder.transform.FindChild("life").gameObject);

		if(lives>1){
			lives--;
		}else{
			goToGameOver();

		}
	}

	int zeroToN(int n){
		return (int)Mathf.Floor(Random.Range(0,(float)n));
	}

	int shapeIndexOf(string label){
		for(int i =0; i<shapeNames.Length; i++){
			if(shapeNames[i]==label){
				return i;
			}
		}
		return -1;
	}
}
