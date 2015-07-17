using UnityEngine;
using System.Collections;

public class moveAndCollide : MonoBehaviour {
	//var sensorPrefab:Transform;
	bool hit = false;
	// Use this for initialization
	void Start () {
		GameObject sensor = (GameObject)Instantiate(Resources.Load("sensorPrefab"), GameObject.Find("spawnPoint").transform.position, Quaternion.identity); //change myprefab to name
	}
	
	// Update is called once per frame
	void Update () {

		//movement for units
		//transform.localPosition -= transform.forward * Time.deltaTime*3f;
		// end movement

		//sensor stuff
		// HERE GOES time controls on how often fires
		//GameObject sensor = (GameObject)Instantiate(Resources.Load("sensorPrefab"), GameObject.Find("spawnPoint").transform.position, Quaternion.identity); //change myprefab to name
		//
	}

	void OnCollisionEnter(Collision coll)
	{

		//if(coll.gameObject.name=="testWall")    
	//}
}
}
