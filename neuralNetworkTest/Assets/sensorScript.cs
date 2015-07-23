using UnityEngine;
using System.Collections;

public class sensorScript : MonoBehaviour {
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		//while time
		transform.Translate (Vector3.forward*Time.deltaTime*0.09f);
		//transform.Translate (0.05f,0,0);
		Destroy (gameObject, 3f);

	}
	void OnCollisionEnter(Collision coll)
	{
		
		//if(coll.gameObject.name=="testWall")    
		//}

		//do other stuff
	}
}
