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
	//private Text count;
	//private Image img;
	//private Text time;
	//private Text objective;
	private GameObject livesHolder;
	private Text shapesCollected;
	private GameObject collectHolder;
	private GameObject solidHolder;
	private GameObject dangerousHolder;
	private GameObject hostileHolder;
	//private Text livesCount;
	
	//private GameObject timeFields;
	//private GameObject countFields;

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

		//count = UIPointer.transform.FindChild("CountFields").FindChild("count").GetComponent<Text>();

		//img = UIPointer.transform.FindChild("Image").GetComponent<Image>();
		//time = UIPointer.transform.FindChild("TimeFields").FindChild("time").GetComponent<Text>();
		//objective = UIPointer.transform.FindChild("Objective").GetComponent<Text>();
		livesHolder = UIPointer.transform.FindChild("LivesList").gameObject;
		Debug.Log(UIPointer);
		collectHolder = UIPointer.transform.FindChild("CollectableList").gameObject;
		shapeList.Add("collect", collectHolder);
//		solidHolder = UIPointer.transform.FindChild("SolidList").gameObject;
//		shapeList.Add("solid", solidHolder);
		dangerousHolder = UIPointer.transform.FindChild("DangerousList").gameObject;
		shapeList.Add("dangerous", dangerousHolder);
		hostileHolder = UIPointer.transform.FindChild("HostileList").gameObject;
		shapeList.Add("hostile", hostileHolder);
		shapesCollected = gameOverScreen.transform.FindChild("GoalCount").GetComponent<Text>();

		//livesCount = lives
//		Debug.Log (gameOverScreen.transform);
//		Debug.Log (gameOverScreen.transform.FindChild("GoalCount"));
//		Debug.Log (gameOverScreen.transform.FindChild("GoalCount").GetComponent<Text>());
		
		//timeFields = UIPointer.transform.FindChild("TimeFields").gameObject;
		//countFields = UIPointer.transform.FindChild("CountFields").gameObject;

	}

	void startGame(){

		UIPointer.SetActive(true);
		startScreen.SetActive(false);
		goalsAccomplished = 0;
//		goals = new Goal[]{new Goal("diamond", "collect", 10)};
		//objective.text = ""+goals[0].type;
		//count.text = ""+goals[0].num;
		gameObject.renderer.enabled = true;
		transform.position = new Vector3(0.0f,0.0f,0.0f);
		inMenu = false;
		lives = 3;
		//livesCount.text = ""+lives;

		//timeFields.SetActive(false);
		//countFields.SetActive(true);

		for(int  i = 0; i<shapeCount; i++){
			int selection = zeroToN(shapes.Length);//(int)Mathf.Floor(Random.Range(0,shapes.Length));
			//Debug.Log(selection);
			//Debug.Log(shapes[selection]);
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
		Debug.Log(kp.Key+":"+kp.Value);

		goals.Add(kp.Key, kp.Value);

		GameObject img = Instantiate(sprites[shapeIndexOf(kp.Key)]) as GameObject;//new GameObject();
		Debug.Log(img);
		img.AddComponent<Image>();

		img.GetComponent<Image>().sprite = shapes[shapeIndexOf(kp.Key)].GetComponent<SpriteRenderer>().sprite;  //.sprite = 

		img.transform.SetParent(shapeList[kp.Value].transform, false);
		img.name = kp.Key;//"bababoi";
		Debug.Log(shapeList[kp.Value].transform.FindChild(kp.Key).gameObject+":"+shapeList[kp.Value].transform.FindChild(kp.Key).gameObject.name);

	}

	void finishGoal(string k){//KeyValuePair<string, string> kp){
		Debug.Log("finishing Goal "+k);
		GameObject listPointer = null;
		if(shapeList.TryGetValue(goals[k], out listPointer)){
			GameObject img = shapeList[goals[k]].transform.FindChild(k).gameObject;//listPointer.transform.FindChild(k).gameObject;
			Destroy(img);
		}
		goals.Remove(k);
	}

	void changeGoals(){
		//Dictionary<string, string>.Enumerator e = ;
		foreach(KeyValuePair<string, string> kp in goals){
			if(kp.Value == "collect"){
				finishGoal(kp.Key);
				break;
			}
		}

		if(goals.Count>0){
			string[] keys = new string[0];
			goals.Keys.CopyTo(keys, 0);
			string k = keys[zeroToN(goals.Count)];
			finishGoal(k);
		}

		//generate new goals and add them
		addGoal(generateGoal(true));
		addGoal(generateGoal(true));
		//addGoal(generateGoal(false));

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
		Debug.Log(goalsAccomplished);
		Debug.Log(shapesCollected);
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
		//camera.transform.eulerAngles = new Vector3(0,0,0);

		float a = Mathf.Deg2Rad*transform.eulerAngles.z;
		//float velocity = 0.0f;//5.0f;
		rigidbody2D.velocity = new Vector2( -(Mathf.Sin(a)*velocity), Mathf.Cos(a)*velocity);



		//if(goals[0].type=="avoid"){
			//goals[0].time-=Time.deltaTime;
//			float oldTime = goals[0].time;
//			time.text = ""+Mathf.Ceil(goals[0].time);
//
//			if(Mathf.Ceil(goals[0].time)<Mathf.Ceil(oldTime))
//				audio.PlayOneShot(point, 0.7F);
//
//			if(goals[0].time <= 0){
//				goalsAccomplished++;
//				setGoal();
//			}
		//}

		//Debug.Log(GameObject.FindGameObjectsWithTag("Shape").Length);

	}
	
	public void replaceShape(){
		float angle = Mathf.Deg2Rad*(transform.eulerAngles.z + Random.Range(-80.0f, 80.0f));
		float magnitude = Random.Range(minDistance, maxDistance);

		Instantiate(shapes[zeroToN(shapes.Length)], new Vector3(transform.position.x -(Mathf.Sin(angle)*magnitude), transform.position.y+Mathf.Cos(angle)*magnitude), Quaternion.identity);
		//Destroy(this.gameObject);
	}



	public void addPoint(){

		points++;
		shapesCollected.text = ""+points;
		if(points%10==0){
			changeGoals();
		}
		//goals[0].num--;
//		audio.PlayOneShot(point, 0.7F);/**/
//		if(goals[0].num<=0){
//			goalsAccomplished++;
//			setGoal();
//		}
//		else{
//			//count.text = ""+goals[0].num;
//		}
	}



//	public void setGoal(){
//		//swap goal
//		Debug.Log("Accomplished goal");
//		string gT = goalTypes[(int)Mathf.Floor(Random.Range(0,goalTypes.Length))];
//		int shapeIndex = (int)Mathf.Floor(Random.Range(0,shapes.Length));
//	
//		if(gT=="hunt"){
//			//goals[0] = new Goal(shapeNames[shapeIndex],gT,0,10.0f);
////			img.sprite = shapes[shapeIndex].GetComponent<SpriteRenderer>().sprite;
////			time.text = ""+Mathf.Ceil(goals[0].time);
////			timeFields.SetActive(true);//.GetComponent<CanvasRenderer>().enabled = true;
////			countFields.SetActive(false);//.GetComponent<CanvasRenderer>().enabled = false;
//			//set Image
//
//		}else{
//
//			//
////			goals[0] = new Goal(shapeNames[shapeIndex],gT,10,0.0f);
//			//img.sprite = shapes[shapeIndex].GetComponent<SpriteRenderer>().sprite;
//			//count.text = ""+goals[0].num;
//			//timeFields.SetActive(false);//GetComponent<CanvasRenderer>().renderer.enabled = false;
////			countFields.SetActive(true);//.GetComponent<CanvasRenderer>().renderer.enabled = true;
//		}
//		//objective.text = gT;
//
//		switch(gT){
//		case "avoid":
//			audio.PlayOneShot(avoid, 0.7F);
//			break;
//		case "collect":
//			audio.PlayOneShot(collect, 0.7F);
//			break;
////		case "shoot":
////			audio.PlayOneShot(shoot, 0.7F);
////			break;
//		default:
//			break;
//		}
//
//	}

	public void die(){
		audio.PlayOneShot(damage, 0.7F);
		if(lives>1){
			lives--;
			Debug.Log (lives);
			//livesCount.text = ""+lives;
			//setGoal();
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
