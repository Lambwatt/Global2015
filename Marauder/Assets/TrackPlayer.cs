using UnityEngine;
using System.Collections;

//TrackPlayer------------------------------------------------------
//class used by the camera to follow the player at all times.
public class TrackPlayer : MonoBehaviour {

	public GameObject player;
	
	//match the player's position
	void Update () {
		GetComponent<Transform>().position = new Vector3(player.GetComponent<Transform>().position.x, player.GetComponent<Transform>().position.y, -10.0f);
	}
}
