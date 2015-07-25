using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class moveAndCollide : MonoBehaviour {
	List<Collider> validColliders; //holds all colliders sphere has sensed
	Collider[] hitColliders; // holds all colliders currently sensed
	public float radius = 1f;
	public float gridMultiplier = 0.25f; //changes the size of path grid for effecting precision
	GameObject testSphere;

	private Dictionary<XZKey, weightAngle> neuralValues;//holds square and weight ie {"(0,0)" : one};
	// neuralValues{"0|1|1|0"} == one; one.Weight.get   

	///<summary>
	/// used to create a dictionary mapping one key to two values
	///</summary>
	public class weightAngle
	{
		public float Weight { get;  set;}
		public float Angle { get;  set;}
		public weightAngle(float weight, float angle){
			Weight = weight;
			Angle = angle;
		}
	}

	///<summary>
	/// holds the XZ key for the dictionary, each XZ key must have a unique name
	/// for unique name ASUMPTION WALLS NEVER OVERLAP
	/// </summary>
	public class XZKey
	{
		public float X { get; protected set; }
		public float Z { get; protected set; }
		public XZKey(float x, float z){
			X = x;
			Z = z;
		}
	}

	/// <summary>
	/// Find the current square on the map you reside in. Each square has a weight and angle.
	/// Grid multiplier determines how many squares the grid (map) is broken into.
	/// The smaller the gridmultiplier, the more preces the neural network.
	/// </summary>
	void currentSpace(double x, double z){
		float nearestX = findNearest(x); 
		float nearestZ = findNearest(z);
		XZKey currentKey = new XZKey (nearestX,nearestZ);
		weightAngle test = new weightAngle (22, 45); //FOR TESTING
		if (containsKey(neuralValues, currentKey) == false) {
			neuralValues.Add(currentKey,test);
		}
	}



	bool containsKey(Dictionary<XZKey, weightAngle> neuralValues, XZKey currentKey){
		bool found = false;
		for (int i = 0; i<neuralValues.Count; i++){
			if(neuralValues.Keys.ElementAt(i).X == currentKey.X && neuralValues.Keys.ElementAt(i).Z == currentKey.Z){
				found = true;
			}
		}
		return found;
	}

	void printDictionary(){
		for (int i = 0; i<neuralValues.Count; i++){
			print("current key x = " + neuralValues.Keys.ElementAt(i).X);
			print("current key y = " + neuralValues.Keys.ElementAt(i).Z);
			print ("current key angle = " + neuralValues.Values.ElementAt(i).Angle);
			print ("current key weight = " + neuralValues.Values.ElementAt(i).Weight);
			}
		}

	/// <summary>
	/// Find nearest multiple of gridmultiplier, using the floor.
	/// </summary>
	/// <returns>The nearest multiple of grid multiplier (bounds of current square).</returns>
	float findNearest(double currentPoint){ 
		float floatedCurrentPoint = (float)currentPoint;
		float gridResolutionFactor;
		gridResolutionFactor = Mathf.Floor((floatedCurrentPoint / gridMultiplier));
		return gridResolutionFactor * gridMultiplier;
	}

	
	/// 
	///	
	// Use this for initialization
	void Start () {
		neuralValues = new Dictionary<XZKey,weightAngle> {}; 
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
		currentSpace (testSphere.transform.position.x, testSphere.transform.position.z);
		printDictionary ();
	}

	/// <summary>
	/// Should read a saved state for a neural network
	/// </summary>
	void readNeuralNetworkState(){

	}

	////METHOD updates list of known walls found (cubes) based on those currently in vision.
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
		int x = 10 / 4;
		int total_objects = 0;
		foreach (Collider collider in validColliders){
			collider.renderer.material.color = Color.blue;
			total_objects+=1;
		}
	}

	/// <summary>
	/// The grid on which the neural network will decide values
	/// </summary>
	void setupGrid(){
		}
}
