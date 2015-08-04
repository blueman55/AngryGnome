﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

public class moveAndCollide : MonoBehaviour {
	List<Collider> validColliders; //holds all colliders sphere has sensed
	public float radius = .01f; //radius within which objects can be sensed. Adjust as desired.
	public float gridMultiplier = 2f; //changes the size of each square of grid, effecting precision. Adjust as desired.
	GameObject testSphere; //here for testing
	Collider[] hitColliders; // holds all colliders currently sense
	XZKey previousSquare;
	bool firstFrame;

	private List<listNode> neuralValues;
	//the environment is divided up into squares, the amount of which is set by the multiplier gridMultiplier
	//the above dictionary holds the square(accessed by the floor of squares x and y(using an XZKey))
	//each square is linked to a weight and angle(vector), and times visited.
	//the vector and times visited are updated when neccessary for each square

	public class listNode
	{
		public XZKey xzKey { get; set;}
		public weightAngleTimesVisited  watv {get; set;}
		public listNode(XZKey inKey, weightAngleTimesVisited inWATV){
			watv = inWATV;
			xzKey = inKey;
		}
	}

	///<summary>
	/// used to create a dictionary mapping one key to two values
	///</summary>
	public class weightAngleTimesVisited
	{
		public float Weight { get;  set;}
		public float Angle { get;  set;}
		public int TimesVisited{ get; set;}
		public weightAngleTimesVisited(float weight, float angle, int timesVisited){
			Weight = weight;
			Angle = angle;
			TimesVisited = timesVisited;
		}
	}

	///<summary>
	/// holds the XZ key for the dictionary, each XZ key must have a unique name
	/// for unique name ASUMPTION WALLS NEVER OVERLAP
	/// </summary>
	public class XZKey
	{
		public float X { get; set; }
		public float Z { get; set; }
		public XZKey(float x, float z){
			X = x;
			Z = z;
		}
	}

	public class Vector
	{
		public float Weight { get; set; }
		public float Angle { get; set; }
		public Vector(float weight, float angle){
			Weight = weight;
			Angle = angle;
		}
	}

	/// <summary>
	/// Find the current square on the map you reside in. Each square has a weight and angle.
	/// Grid multiplier determines how many squares the grid (map) is broken into.
	/// The smaller the gridmultiplier, the more preces the neural network.
	/// </summary>
	void currentSpace(double x, double z, Collider[] hitcolliders){
		//create a new key to compare against based on where sphere is on 'grid'
		float nearestX = findNearest(x); 
		float nearestZ = findNearest(z);
		print ("nearestx: " + nearestX);
		print ("nearestz: " + nearestZ);
		XZKey currentKey = new XZKey (nearestX,nearestZ);
		weightAngleTimesVisited currentWTV = new weightAngleTimesVisited(1,1,0);
		if (firstFrame == true) {
			previousSquare = currentKey;
			listNode firstNode = new listNode(currentKey, currentWTV);
			//neuralValues.Add (new XZKey(11,2), currentWTV);
			neuralValues.Add(firstNode);
		}
		//check if neural values has found square, if not add, if found update wtv
		bool found = false;
		for (int i=0; i<(neuralValues.Count); i++){
			print ("neuralvalues count: "+neuralValues.Count);
			print ("in neuralValues is square: X = "+ neuralValues[i].xzKey.X+ " Z = "+neuralValues[i].xzKey.Z);
			if(neuralValues[i].xzKey.X == currentKey.X && neuralValues[i].xzKey.Z == currentKey.Z){
				found = true;
				//print ("I have been at this space");
				currentWTV.Angle = neuralValues[i].watv.Angle;
				currentWTV.Weight = neuralValues[i].watv.Weight;
				currentWTV.Weight = neuralValues[i].watv.TimesVisited;
			}
		}
		if (found == false) {
			print ("adding currentkey: X = "+ currentKey.X+ " Z = " + currentKey.Z);
			listNode currentNode = new listNode(currentKey, currentWTV);
			neuralValues.Add(currentNode);
		}
		Vector finalVector = calculateFinalVector (hitcolliders,currentWTV, currentKey);
		//update weight, timesvisited, angle values for spot on 'grid'
		if (finalVector != null) {
			for (int i=0; i<neuralValues.Count; i++){
				if(neuralValues[i].xzKey.X == currentKey.X && neuralValues[i].xzKey.Z == currentKey.Z){
					neuralValues[i].watv.Angle = finalVector.Angle;
					neuralValues[i].watv.Weight = finalVector.Weight;
					if(firstFrame == true || previousSquare.X != nearestX || previousSquare.Z != nearestZ){
					neuralValues[i].watv.TimesVisited = neuralValues[i].watv.TimesVisited + 1;
					}
					currentWTV.Angle = finalVector.Angle;
					currentWTV.Weight = finalVector.Weight;
					print ("times visited: "+neuralValues[i].watv.TimesVisited);
				}
			}
			movement (currentWTV);
		}
		previousSquare.X = nearestX;
		previousSquare.Z = nearestZ;
	}

	void movement(weightAngleTimesVisited WATV){
		//Vector3 targetAngle = new Vector3(0f, WATV.Angle, 0f);
		//Quaternion targetAngle = new Quaternion(0,WATV.Angle,0,0);
		//targetAngle.y = WATV.Angle;
		//print ("angle for moving: "+WATV.Angle);
		//testSphere.transform.rotation = new Quaternion (0, WATV.Angle, 0, 0);
		//NotThisOnetestSphere.transform.eulerAngles = Vector3.Lerp (testSphere.transform.eulerAngles, (new Vector3(testSphere.transform.eulerAngles.x,WATV.Angle,testSphere.transform.eulerAngles.z)), Time.deltaTime * 5);
		//testSphere.transform.eulerAngles = new Vector3(testSphere.transform.eulerAngles.x,WATV.Angle,testSphere.transform.eulerAngles.z);
		//testSphere.transform.rotation = Vector3.Lerp (testSphere.transform.eulerAngles, (new Vector3(testSphere.transform.eulerAngles.x,WATV.Angle,testSphere.transform.eulerAngles.z)), Time.deltaTime * 5);
		//testSphere.transform.rotation = Quaternion.Lerp(testSphere.transform.rotation, targetAngle,  Time.deltaTime * 2);
		//testSphere.transform.rotation = Quaternion.Euler (0, WATV.Angle, 0);
		//testSphere.transform.eulerAngles =  Mathf.LerpAngle(testSphere.transform.rotation.y, WATV.Angle, WATV.Weight);
		//testSphere.transform.Translate (Vector3.forward * Time.deltaTime * .25f);
		//print ("testsphere angle: "+testSphere.transform.rotation.y);
		float weight = WATV.Weight; 
		//print ("typical wall weight: "+ weight);
		//testSphere.transform.rotation = Quaternion.Lerp (testSphere.transform.rotation, Quaternion.Euler (0, WATV.Angle, 0), weight*Time.deltaTime);
		//testSphere.transform.Translate(.05f,0,0);
	}


	Vector calculateFinalVector(Collider[] hitcolliders, weightAngleTimesVisited currentWATVV, XZKey currentKey){
		//calculate final vector os square
		//weights of vectors might need tweaking
		Vector wallVector = calculateWallVector (hitcolliders); //mult
		Vector timeVisitedVector = calculateTimesVisitedVector (currentWATVV, currentKey);
		print ("times visited angle: " + (timeVisitedVector.Angle+180f));
		print ("times visited magnitude: " + timeVisitedVector.Weight);
		return wallVector;
	}

	Vector calculateTimesVisitedVector(weightAngleTimesVisited currentWATV, XZKey currentKey){
		//this should return the weight of times visited (this can be fiddled with, global vsrisble)
		// and the angle generated by times visited
		float weightTimesVisitedAngle=0;
		float xmag = 0;
		float zmag = 0;
		float finalmag = 0;
		bool top = false;
		bool topRight = false;
		bool right = false;
		bool bottomRight = false;
		bool bottom = false;
		bool bottomLeft = false;
		bool left = false;
		bool topLeft = false;
		for (int i=0; i<neuralValues.Count; i++){
			print ("printing neural values for map as of now");
			if(neuralValues[i].xzKey.X == (currentKey.X+gridMultiplier) && neuralValues[i].xzKey.Z == currentKey.Z){
				right = true;
				xmag = xmag + neuralValues[i].watv.TimesVisited;
				print ("right");
			}
			if(neuralValues[i].xzKey.X == (currentKey.X+gridMultiplier) && neuralValues[i].xzKey.Z == (currentKey.Z + gridMultiplier)){
				topRight = true;
				xmag = xmag + neuralValues[i].watv.TimesVisited;
				zmag = zmag + neuralValues[i].watv.TimesVisited;
				print ("topright");
			}
			if(neuralValues[i].xzKey.X == currentKey.X && neuralValues[i].xzKey.Z == (currentKey.Z + gridMultiplier)){
				top = true;
				zmag = zmag + neuralValues[i].watv.TimesVisited;
				print ("top");
			}
			if(neuralValues[i].xzKey.X == (currentKey.X - gridMultiplier) && neuralValues[i].xzKey.Z == (currentKey.Z + gridMultiplier)){
				topLeft = true;
				zmag = zmag + neuralValues[i].watv.TimesVisited;
				xmag = xmag - neuralValues[i].watv.TimesVisited;
				print ("topleft");
			}
			if(neuralValues[i].xzKey.X == (currentKey.X - gridMultiplier) && neuralValues[i].xzKey.Z == currentKey.Z){
				left = true;
				xmag = xmag - neuralValues[i].watv.TimesVisited;
				print ("left");
			}
			if(neuralValues[i].xzKey.X == (currentKey.X - gridMultiplier) && neuralValues[i].xzKey.Z == (currentKey.Z - gridMultiplier)){
				bottomLeft = true;
				xmag = xmag - neuralValues[i].watv.TimesVisited;
				zmag = zmag - neuralValues[i].watv.TimesVisited;
				print ("bottomeleft");
			}
			if(neuralValues[i].xzKey.X == currentKey.X && neuralValues[i].xzKey.Z == (currentKey.Z - gridMultiplier)){
				bottom = true;
				zmag = zmag - neuralValues[i].watv.TimesVisited;
				print ("bottom");
			}
			if(neuralValues[i].xzKey.X == (currentKey.X+gridMultiplier) && neuralValues[i].xzKey.Z == (currentKey.Z - gridMultiplier)){
				bottomRight = true;
				zmag = zmag - neuralValues[i].watv.TimesVisited;
				xmag = xmag + neuralValues[i].watv.TimesVisited;
				print ("bottomright");
			}
		}
		print ("xmag: " + xmag);
		print ("zmag: " + zmag);
		finalmag = Mathf.Sqrt (Mathf.Pow (xmag, 2) + Mathf.Pow (zmag, 2));
		weightTimesVisitedAngle = calculateRaycastAngle (xmag, zmag);
		Vector timesVisitedVector = new Vector (finalmag, weightTimesVisitedAngle);
		return timesVisitedVector;
	}

	Vector calculateWallVector(Collider[] hitColliders){
		if (hitColliders != null) {
						float finalAngle = 0;
						float finalX =0;
						float finalY =0;
						float finalWeight = 0;
						for (int i=0; i<hitColliders.Length; i++) {
				if(hitColliders[i].name == "Cube"){
								float x1 = testSphere.transform.position.x;
								float x2 = hitColliders[i].transform.position.x;
								float z1 = testSphere.transform.position.z;
								float z2 = hitColliders[i].transform.position.z;
								float xmag = calcXmag (x2, x1);
								float zmag = calcZmag (z2, z1);
								float distance = calculateDistance (xmag, zmag);
								float angle = calculateRaycastAngle (xmag, zmag);
								angle = (angle / 360) * 2*Mathf.PI;
								//float weight = 1/distance;
								//tempX = (Mathf.Cos (angle)*weight);
								//tempY = (Mathf.Sin (angle)*weight);
								finalX = finalX + xmag;
								finalY = finalY+ zmag;
						}
			}
						finalWeight = Mathf.Sqrt (Mathf.Pow (finalX, 2) + Mathf.Pow (finalY, 2));
						finalAngle = Mathf.Atan (finalY / finalX);						

			if (finalX==0) { // edge cases first
				if(finalY>0){
					finalAngle = 90;
				}else{
					finalAngle = 270;
				}
			} else if (finalY == 0) {
				if(finalX>0){
					finalAngle = 0;
				}else{
					finalAngle = 180;
				}
			} else {
				if(finalX>0){
					if(finalY > 0){
						finalAngle = (Mathf.Atan(finalY/finalX)); //QUAD 1
					}else{
						finalAngle =Mathf.PI*2+(Mathf.Atan(finalY/finalX)); //QUAD 4
						//finalAngle = Hoopers+360;
					}
				}else{
					if(finalY>0){
						finalAngle = Mathf.PI+(Mathf.Atan(finalY/finalX));//QUAD 2
						//finalAngle = Hoopers+180;
					}else{
						finalAngle = Mathf.PI+(Mathf.Atan(finalY/finalX)); // QUAD 3
						//finalAngle = Hoopers+180;
					}
				}
			}

			finalAngle = ((finalAngle / (2 * Mathf.PI)) * 360);
			finalAngle = -(180+finalAngle) ;
						Vector wallVector = new Vector (finalWeight, finalAngle);
						return wallVector;
				}
		return null;
	}

	/// <summary>
	/// x1 is origin x of the agent
	/// y1 is the origin y of the agent
	/// x2 is the origin of the other object to find distance to.
	/// y2 is the origin of the other object to find distance to.
	/// </summary>
	float calculateDistance (float xmag, float zmag){
		float dmag=0;
		dmag = (float)Mathf.Pow(xmag, 2) + (float)Mathf.Pow(zmag, 2);
		return Mathf.Sqrt (dmag);
		}

	float calcXmag(float x2, float x1){
		return (x2 - x1);
	}

	float calcZmag(float z2, float z1){
		return (z2-z1);
	}

	/// <summary>
	/// returns the angle from the moving units point of origin to a centerpoint of a collider
	/// This is used to calculate the distance, as well as in neural networ weighting
	/// </summary>
	float calculateRaycastAngle(float xmag, float zmag){
		float foundAngle = 0;
		if (xmag == 0) { // edge cases first
			if((zmag)>0){
				foundAngle = 90;
			}else{
				foundAngle = 270;
			}
			return foundAngle;
		} else if (zmag == 0) {
			if((xmag)>0){
				foundAngle = 0;
			}else{
				foundAngle = 180;
			}
			return foundAngle;
		} else {
			if(xmag>0){
				if(zmag > 0){
					foundAngle = Mathf.Rad2Deg*Mathf.Atan((zmag/xmag)); //QUAD 1
				}else{
					foundAngle = (Mathf.Rad2Deg*Mathf.Atan((zmag/xmag)))+360; //QUAD 4
				}
			}else{
				if(zmag>0){
					foundAngle = (Mathf.Rad2Deg*Mathf.Atan((zmag/xmag)))+180; //QUAD 2
				}else{
					foundAngle = (Mathf.Rad2Deg*Mathf.Atan((zmag/xmag)))+180; // QUAD 3
				}
			}
		}
		return foundAngle;
	}

	float calcuatetimesVisitedWeight(int timesVisited){
		//timesVisited x constant
		return 1f;
	}

	bool containsKey(Dictionary<XZKey, weightAngleTimesVisited> neuralValues, XZKey currentKey){
		bool found = false;
		for (int i = 0; i<neuralValues.Count; i++){
			if(neuralValues.Keys.ElementAt(i).X == currentKey.X && neuralValues.Keys.ElementAt(i).Z == currentKey.Z){
				found = true;
			}
		}
		return found;
	}

	void printDictionary(){ //FOR TESTING
		for (int i = 0; i<neuralValues.Count; i++){
			print("current key x = " + neuralValues[i].xzKey.X);
			print("current key y = " + neuralValues[i].xzKey.Z);
			print ("current key weight = " + neuralValues[i].watv.Weight);
			}
		for (int i=0; i<validColliders.Count; i++) {
			print ("mag x: "+ (validColliders[i].transform.position.x - testSphere.transform.position.x));
			print ("mag y: "+ (validColliders[i].transform.position.z - testSphere.transform.position.z));
			//print ("angle calculation: " + calculateRaycastAngle(testSphere.transform.position.x, testSphere.transform.position.z, validColliders[i].transform.position.x, validColliders[i].transform.position.z));
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
		firstFrame = true;
		//Collider[] hitColliders; // holds all colliders currently sensed
		neuralValues = new List<listNode> {}; 
		//hitColliders = Physics.OverlapSphere (transform.position, radius); //get current colliders in range
		validColliders = new List<Collider>(); // initialize validColliders
		testSphere = GameObject.Find("Sphere"); //FOR TESTING
		//calculateValidColliders (hitColliders); // add correct objects to found list of colliders

	}

	/// Update this instance.
	/// see method headers for method descriptions
	// Update is called once per frame
	void Update () {
		hitColliders = Physics.OverlapSphere (transform.position, radius);
		calculateValidColliders(hitColliders); //see method
		//printValidCollider(); //FOR TESTING changes seen wall to blue for debugging
		//testSphere.transform.Translate(Vector3.right*Time.deltaTime*1.3f); //FOR TESTING
		currentSpace (testSphere.transform.position.x, testSphere.transform.position.z, hitColliders);
		//printDictionary ();
		printValidCollider ();
		if (firstFrame == true) {
			firstFrame = false;
				}
	}

	/// <summary>
	/// Should read a saved state for a neural network
	/// </summary>
	void readNeuralNetworkState(){

	}

	////METHOD updates list of known walls found (cubes) based on those currently in vision.
    //// after first pass, would like only segments of wall in view, instead of discovering whole cube (section of wall)
    //// once one portion is seen. 
	void calculateValidColliders(Collider[] hitColliders){
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
		foreach (Collider collider in validColliders){
			collider.renderer.material.color = Color.blue;
		}
	}

	/// <summary>
	/// The grid on which the neural network will decide values
	/// </summary>
	void setupGrid(){
		}
}
