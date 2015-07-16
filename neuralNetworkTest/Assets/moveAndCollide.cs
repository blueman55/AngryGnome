using UnityEngine;
using System.Collections;

public class moveAndCollide : MonoBehaviour {
	bool hit = false;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (hit == false) {
		print ("I'm alive");
		} else {
			print("colliding with object Master");
		}
		transform.localPosition -= transform.forward * Time.deltaTime*3f;
	}

	void OnCollisionEnter(Collision coll)
	{

		if(coll.gameObject.name=="testWall")
			hit = true;
			print ("colliding with wall master!");    
	}
}
