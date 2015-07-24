using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class moveAndCollide : MonoBehaviour {
	List<Collider> validColliders; //holds all colliders sphere has sensed
	Collider[] hitColliders; // holds all colliders currently sensed
	private float radius = 1;
	private int size;
	GameObject testSphere;
	private Dictionary<string, weightAngle> neuralValues;//holds square and weight ie {0,0|1,1|0,1|1,0 : .83};

	///<summary>
	/// used to create a dictionary mapping one key to two values
	///</summary>
	public class weightAngle
	{
		public float Weight {get; protected set;};
		public float Angle {get; protected set;};
		public weightAngle(float weight, float angle){
			Weight = weight;
			Angle = angle;
		}
	}

	/// 
	///	
	// Use this for initialization
	void Start () {
		neuralValues = new Dictionary<string,weightAngle> {}; 
		hitColliders = Physics.OverlapSphere (transform.position, radius); //get current colliders in range
		validColliders = new List<Collider>(); // initialize validColliders
		testSphere = GameObject.Find("Sphere"); //FOR TESTING
		calculateValidColliders (); // add correct objects to found list of colliders

	}

	/// Update this instance.
	/// see method headers for method descriptions
	// Update is called once per frame
	void Update () {
		hitColliders = Physics.OverlapSphere (transform.position, radius);
		calculateValidColliders(); //see method
		printValidCollider(); //FOR TESTING changes seen wall to blue for debugging
		testSphere.transform.Translate(Vector3.right*Time.deltaTime*1.3f); //FOR TESTING
	}

	/// <summary>
	/// Should read a saved state for a neural network
	/// </summary>
	void readNeuralNetworkState(){

	}

	////METHOD updates list of known walls found (cubes) based on those currently in vision
    //// after first pass, would like only segments of wall in view, instead of discovering whole cube (section of wall)
    //// once one portion is seen. 
	void calculateValidColliders(){
		bool matchFound = false; // starts false at default to no matching wall found/remembered
		for (int i=0; i<hitColliders.Length; i++){ //
			for(int j=0; j<validColliders.Count; j++){ // go through all walls already found, compare to those seen currently
				if(hitColliders[i].transform.position == validColliders[j].transform.position){ // see if object already is known. ASSUMES SQUARES DO NOT OVERLAP, POSITION IS UNIQUE
					matchFound = true; // a match has been found
				}
			}
			if(hitColliders[i].name == "Cube"){ //add only valid colliders (cubes which are walls) 
				if(matchFound == false){
					validColliders.Add(hitColliders[i]); 
				}else {
					matchFound = false; //set back to default not found
				}
			}
		}
	}

	void calculateCollisionPosition(){

	}


	/// 
	/// 
	/// Excecute on a collision
	void OnCollisionEnter(Collision coll)
	{
		//if(coll.gameObject.name=="testWall")    
	//}
	}

	/// <summary>
	/// turns found walls blue, for testing purposes
	/// </summary>
	void printValidCollider(){
		int total_objects = 0;
		foreach (Collider collider in validColliders){
			collider.renderer.material.color = Color.blue;
			total_objects+=1;
		}
		print ("Total number wall cubes found: " + total_objects);
	}

	/// <summary>
	/// The grid on which the neural network will decide values
	/// </summary>
	void setupGrid(){
		}
}
