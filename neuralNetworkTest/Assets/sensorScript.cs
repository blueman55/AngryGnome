using UnityEngine;
using System.Collections;

public class sensorScript : MonoBehaviour {
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		//while time
		transform.Translate (0.05f,0,0);
		Destroy (gameObject, 1f);

	}
	void OnCollisionEnter(Collision coll)
	{
		
		//if(coll.gameObject.name=="testWall")    
		//}

		//do other stuff
	}
}
