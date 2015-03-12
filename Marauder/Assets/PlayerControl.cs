﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//PlayerControl-------------------------------------------------------
//Contains player control, menu and ui control and general game logic
public class PlayerControl : MonoBehaviour {

	//Events----------------------------------------------------------
	public delegate void EndGame();
	public static event EndGame onEndGame;

	//Variables set int editor----------------------------------------

	//Audio clips
	public AudioClip avoid;
	public AudioClip collect;
	public AudioClip dieSound;
	public AudioClip point;
	public AudioClip damage;

	//shape names, sprites, and prefabs
	public GameObject[] shapes;
	public GameObject[] sprites;
	public string[] shapeNames;

	//shape generation variables
	public float minDistance;
	public float maxDistance;
	public int shapeCount;

	//static player variables
	public float velocity;
	public GameObject playerSprite;

	//Internal variables-----------------------------------------------
	private int points;
	private int lives;

	//state controls
	private bool inMenu = true;
	private bool gameOver = false;

	//refernces to the ui 
	private GameObject goalUI;
	private GameObject livesHolder;
	private Text shapesCollected;
	private GameObject collectHolder;
	private GameObject solidHolder;
	private GameObject dangerousHolder;
	private GameObject hostileHolder;
	private Text pointsHolder;
	private GameObject gameOverScreen;
	private GameObject startScreen;

	//list of goal labels
	private string[] goalTypes = {"collect", "dangerous", "hostile"};

	//dictionaries
	Dictionary<string, string> goals = new Dictionary <string,string>();
	Dictionary<string, GameObject> shapeList = new Dictionary <string,GameObject>();

	//hide player initially since the game opens on a menu
	void Awake(){
		this.gameObject.renderer.enabled = false;
	}

	//set up pointers and set initial menu state
	void Start () {

		//get canvas pointers and show only start canvas
		gameOverScreen = GameObject.Find("GameOver");
		gameOverScreen.SetActive(false);

		startScreen = GameObject.Find("MainMenu");

		goalUI = GameObject.Find("Goal");
		goalUI.SetActive(false);

		//get on screen lists to put images into later
		livesHolder = goalUI.transform.FindChild("LivesList").gameObject;

		collectHolder = goalUI.transform.FindChild("CollectableList").gameObject;
		shapeList.Add("collect", collectHolder);
		dangerousHolder = goalUI.transform.FindChild("DangerousList").gameObject;
		shapeList.Add("dangerous", dangerousHolder);
		hostileHolder = goalUI.transform.FindChild("HostileList").gameObject;
		shapeList.Add("hostile", hostileHolder);
		//		solidHolder = UIPointer.transform.FindChild("SolidList").gameObject;
		//		shapeList.Add("solid", solidHolder);

		//get ui text areas to write to later
		pointsHolder = goalUI.transform.FindChild("ScoreSet").FindChild("Score").GetComponent<Text>();
		shapesCollected = gameOverScreen.transform.FindChild("GoalCount").GetComponent<Text>();
	}

	//set initial game values and update ui accordingly. Then start game.
	void startGame(){

		//swap canvas shown to player
		goalUI.SetActive(true);
		startScreen.SetActive(false);
		inMenu = false;

		//set and show initial points
		points = 0;
		pointsHolder.text = ""+points;

		//show player at screen centre
		gameObject.renderer.enabled = true;
		transform.position = new Vector3(0.0f,0.0f,0.0f);

		//set and show lives
		lives = 3;	
		for (int j = 0; j<lives; j++){
			GameObject img = Instantiate(playerSprite) as GameObject;
			img.AddComponent<Image>();
			
			img.GetComponent<Image>().sprite = playerSprite.GetComponent<SpriteRenderer>().sprite; 
			
			img.transform.SetParent(livesHolder.transform, false);
			img.name = "life";
		}

		//populate space with shapes
		for(int  i = 0; i<shapeCount; i++){
			int selection = zeroToN(shapes.Length);
			GameObject shape = Instantiate(shapes[selection], 
			                               	new Vector3(transform.position.x + Random.Range(-maxDistance, maxDistance), 
			            								transform.position.y + Random.Range(-maxDistance, maxDistance)), 
			                               				Quaternion.identity) 
											as GameObject;
			
			shape.GetComponent<TargetShape>().setParams(shapeNames[selection]);
		}

		//set goals
		changeGoals();

	}

	//Goal management---------------------------------------------------------------

	//given a shape, goal pair, add pair dictionary and create an image of the shape in the correct goal's shape list
	void addGoal(KeyValuePair<string, string> kp){

		goals.Add(kp.Key, kp.Value);

		GameObject img = Instantiate(sprites[shapeIndexOf(kp.Key)]) as GameObject;
		img.AddComponent<Image>();

		img.GetComponent<Image>().sprite = shapes[shapeIndexOf(kp.Key)].GetComponent<SpriteRenderer>().sprite; 

		img.transform.SetParent(shapeList[kp.Value].transform, false);
		img.name = kp.Key;

	}

	//remove goal with key k from dictionary and corresponding goal's shape list
	void finishGoal(string k){

		GameObject listPointer = null;
		if(shapeList.TryGetValue(goals[k], out listPointer)){
			GameObject img = shapeList[goals[k]].transform.FindChild(k).gameObject;
			Destroy(img);
		}
		goals.Remove(k);
	}

	//remove some old goals and add some new goals
	void changeGoals(){

		//remove one 'collect' goal if any are present
		foreach(KeyValuePair<string, string> kp in goals){
			if(kp.Value == "collect"){
				finishGoal(kp.Key);
				break;
			}
		}

		//remove a random goal if there are goals to remove
		if(goals.Count>0){
			string[] keys = new string[goals.Count];
			goals.Keys.CopyTo(keys, 0);
			string k = keys[zeroToN(goals.Count)];
			finishGoal(k);
		}

		//add one new collect goal and one other goal
		addGoal(generateGoal(true));
		addGoal(generateGoal(false));

	}

	//generates and returns a goal. only creates 'collect' goals if forceCollect is true
	KeyValuePair<string, string> generateGoal(bool forceCollect){

		//locate a 
		List<string> freeShapes = new List<string>();
		foreach(string k in shapeNames){
			if(!goals.ContainsKey(k)){
				freeShapes.Add(k);
			}
		}

		//assigne a goal for the shape and return the pair
		if(forceCollect){
			return new KeyValuePair<string, string>(freeShapes.ToArray()[zeroToN(freeShapes.Count)],"collect");
		}else{
			return new KeyValuePair<string, string>(freeShapes.ToArray()[zeroToN(freeShapes.Count)],goalTypes[zeroToN(goalTypes.Length)]);
		}
	}

	
	//game flow--------------------------------------------

	//signal all shapes to destroy themselves and switch to start gma ui
	void goToStartMenu(){

		onEndGame();

		gameOver = false;
		inMenu = true;
		gameOverScreen.SetActive(false);
		startScreen.SetActive(true);

	}

	//build over screen and replace game screen
	void goToGameOver(){

		gameOver = true;

		shapesCollected.text = ""+points;

		this.gameObject.renderer.enabled = false;
		goalUI.SetActive(false);
		gameOverScreen.SetActive(true);

	}

	//handle lost life
	public void die(){
		if(lives<0){
			return;
		}
		
		Debug.Log ("proceeding with "+lives);

		Destroy(livesHolder.transform.FindChild("life").gameObject);
		
		if(lives>1){
			audio.PlayOneShot(damage, 0.7F);
			lives--;
		}else{
			audio.PlayOneShot(dieSound, 0.7F);
			goToGameOver();
			
		}
	}

	//game loop
	void Update () {

		//swap ui if necessary
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

		//proces navigation key states through a hexadecimal hash--------
		int arrowIn = 0; //< ^ v >

		//set a different power of 2 for each arrow/wasd key
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

		//map compinations to resulting directions
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
			//These combinations cancel themselves out
			case 0:
			case 6:
			case 9:
			case 15:
			default:
				break;
		}

		//update position
		float a = Mathf.Deg2Rad*transform.eulerAngles.z;
		rigidbody2D.velocity = new Vector2( -(Mathf.Sin(a)*velocity), Mathf.Cos(a)*velocity);

	}
	
	//utilities----------------------------------------------------------------------------------

	//gets the goal associated with a shape
	public string getMode(string label){
		string objective = null;
		if(goals.TryGetValue(label, out objective)){
			return objective;
		}
		return "free";
	}

	//creates a new shape off screen in the direction the player is traveling
	public void replaceShape(){
		float angle = Mathf.Deg2Rad*(transform.eulerAngles.z + Random.Range(-80.0f, 80.0f));
		float magnitude = Random.Range(minDistance, maxDistance);

		Instantiate(shapes[zeroToN(shapes.Length)], new Vector3(transform.position.x -(Mathf.Sin(angle)*magnitude), transform.position.y+Mathf.Cos(angle)*magnitude), Quaternion.identity);
	}

	//increment points and play point collected sfx. Call for goal change if enough points have been scored
	public void addPoint(){

		points++;
		audio.PlayOneShot(point, 0.7F);
		pointsHolder.text = ""+points;
		if(points%10==0){
			changeGoals();
		}

	}

	//returns a random int within a range 0 - n
	int zeroToN(int n){
		return (int)Mathf.Floor(Random.Range(0,(float)n));
	}

	//get the index of a shape in all arrays based on its name
	int shapeIndexOf(string label){
		for(int i =0; i<shapeNames.Length; i++){
			if(shapeNames[i]==label){
				return i;
			}
		}
		return -1;
	}
}
