using UnityEngine;
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
	public float wallWeighting = 1;
	public float visitedWeighting = 1;

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
		public float TimesVisited{ get; set;}
		public float WallAvoid{ get; set;}
		public weightAngleTimesVisited(float weight, float angle, float timesVisited, float wallAvoid){
			Weight = weight;
			Angle = angle;
			TimesVisited = timesVisited;
			WallAvoid = wallAvoid;
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
		XZKey currentKey = new XZKey (nearestX,nearestZ);
		weightAngleTimesVisited currentWTV = new weightAngleTimesVisited(0,0,0,0);
		if (firstFrame == true) {
			previousSquare.X = currentKey.X;
			previousSquare.Z = currentKey.Z;
			listNode firstNode = new listNode(currentKey, currentWTV);
			neuralValues.Add (firstNode);
		}
		//check if neural values has found square, if not add, if found update wtv
		bool found = false;
		for (int i=0; i<(neuralValues.Count); i++){
			if(neuralValues[i].xzKey.X == currentKey.X && neuralValues[i].xzKey.Z == currentKey.Z){
				found = true;
				currentWTV.Angle = neuralValues[i].watv.Angle;
				currentWTV.Weight = neuralValues[i].watv.Weight;
				currentWTV.Weight = neuralValues[i].watv.TimesVisited;
			}
		}
		if (found == false) {
			listNode currentNode = new listNode(currentKey, currentWTV);
			neuralValues.Add(currentNode);
		}
		Vector finalVector = calculateFinalVector (hitcolliders, ref currentWTV, currentKey);
		//update weight, timesvisited, angle values for spot on 'grid'
		if (finalVector != null) {
			for (int i=0; i<neuralValues.Count; i++){
				if(neuralValues[i].xzKey.X == currentKey.X && neuralValues[i].xzKey.Z == currentKey.Z){
					neuralValues[i].watv.Angle = finalVector.Angle;
					neuralValues[i].watv.WallAvoid = currentWTV.WallAvoid;
					neuralValues[i].watv.Weight = finalVector.Weight;
					if(firstFrame == true || previousSquare.X != nearestX || previousSquare.Z != nearestZ){
						if(neuralValues[i].watv.TimesVisited < 5){
					neuralValues[i].watv.TimesVisited = neuralValues[i].watv.TimesVisited + .4f;
						}
					}
					currentWTV.Angle = finalVector.Angle;
					currentWTV.Weight = finalVector.Weight;
				}
			}
			movement (currentWTV);
		}
		previousSquare.X = nearestX;
		previousSquare.Z = nearestZ;
	}

	void movement(weightAngleTimesVisited WATV){
		float weight = WATV.Weight; 
		//acceptable wall weight (with good results) is between .5 - 3
		testSphere.transform.rotation = Quaternion.Lerp (testSphere.transform.rotation, Quaternion.Euler (0, WATV.Angle, 0), WATV.WallAvoid*Time.deltaTime);
		testSphere.transform.Translate(.05f,0,0);
	}


	Vector calculateFinalVector(Collider[] hitcolliders, ref weightAngleTimesVisited currentWATVV, XZKey currentKey){
		//calculate final vector os square
		//weights of vectors might need tweaking
		Vector wallVector = calculateWallVector (hitcolliders); //mult
		Vector timeVisitedVector = calculateTimesVisitedVector (currentWATVV, currentKey);
		timeVisitedVector.Weight = timeVisitedVector.Weight * visitedWeighting;
		wallVector.Weight = wallVector.Weight * wallWeighting;
		float xmagWall = 0;
		float zmagWall = 0;
		float xmagVisited = 0;
		float zmagVisited = 0;
		float xmagFinal = 0;
		float zmagFinal = 0;
		float tempAngleW = wallVector.Angle;
		float tempAngleT = timeVisitedVector.Angle;
		tempAngleT = ((tempAngleT * (2 * Mathf.PI)) / 360);
		tempAngleW = ((tempAngleW * (2 * Mathf.PI)) / 360);
		zmagWall = ((Mathf.Sin(tempAngleW))*wallVector.Weight);
		zmagVisited = ((Mathf.Sin(tempAngleT))*timeVisitedVector.Weight)*-1;
		zmagFinal = zmagWall + zmagVisited;
		xmagWall = ((Mathf.Cos(tempAngleW))*wallVector.Weight)*-1;
		xmagVisited = ((Mathf.Cos(tempAngleT))*timeVisitedVector.Weight);
		xmagFinal = xmagWall + xmagVisited;
		float finalVectorMag = Mathf.Sqrt((float)Mathf.Pow(xmagFinal, 2) + (float)Mathf.Pow(zmagFinal, 2));
		Vector wallVectorTEST = calculateWallVector (hitcolliders); //mult
		float finalVectorAngle = ((calculateRaycastAngle (xmagFinal, zmagFinal)));
		print ("wallvector angle: "+wallVector.Angle);
		print ("visitedvector angle: "+timeVisitedVector.Angle);
		print ("final angle: "+finalVectorAngle);
		print ("final mag: "+finalVectorMag);
		currentWATVV.WallAvoid = wallVector.Weight;
		Vector finalVector = new Vector (finalVectorMag, finalVectorAngle);
		return finalVector;
	}

	Vector calculateTimesVisitedVector(weightAngleTimesVisited currentWATV, XZKey currentKey){
		//this should return the weight of times visited (this can be fiddled with, global vsrisble)
		// and the angle generated by times visited
		float weightTimesVisitedAngle=0;
		float xmag = 0;
		float zmag = 0;
		float finalmag = 0;
		for (int i=0; i<neuralValues.Count; i++){
			if(neuralValues[i].xzKey.X == (currentKey.X+gridMultiplier) && neuralValues[i].xzKey.Z == currentKey.Z){
				//right
				xmag = xmag + neuralValues[i].watv.TimesVisited;
			}
			if(neuralValues[i].xzKey.X == (currentKey.X+gridMultiplier) && neuralValues[i].xzKey.Z == (currentKey.Z + gridMultiplier)){
				//topright
				xmag = xmag + neuralValues[i].watv.TimesVisited;
				zmag = zmag + neuralValues[i].watv.TimesVisited;
			}
			if(neuralValues[i].xzKey.X == currentKey.X && neuralValues[i].xzKey.Z == (currentKey.Z + gridMultiplier)){
				//top
				zmag = zmag + neuralValues[i].watv.TimesVisited;
			}
			if(neuralValues[i].xzKey.X == (currentKey.X - gridMultiplier) && neuralValues[i].xzKey.Z == (currentKey.Z + gridMultiplier)){
				//topleft
				zmag = zmag + neuralValues[i].watv.TimesVisited;
				xmag = xmag - neuralValues[i].watv.TimesVisited;
			}
			if(neuralValues[i].xzKey.X == (currentKey.X - gridMultiplier) && neuralValues[i].xzKey.Z == currentKey.Z){
				//left
				xmag = xmag - neuralValues[i].watv.TimesVisited;
			}
			if(neuralValues[i].xzKey.X == (currentKey.X - gridMultiplier) && neuralValues[i].xzKey.Z == (currentKey.Z - gridMultiplier)){
				//bottomleft
				xmag = xmag - neuralValues[i].watv.TimesVisited;
				zmag = zmag - neuralValues[i].watv.TimesVisited;
			}
			if(neuralValues[i].xzKey.X == currentKey.X && neuralValues[i].xzKey.Z == (currentKey.Z - gridMultiplier)){
				//bottom
				zmag = zmag - neuralValues[i].watv.TimesVisited;
			}
			if(neuralValues[i].xzKey.X == (currentKey.X+gridMultiplier) && neuralValues[i].xzKey.Z == (currentKey.Z - gridMultiplier)){
				//bottomright
				zmag = zmag - neuralValues[i].watv.TimesVisited;
				xmag = xmag + neuralValues[i].watv.TimesVisited;
			}
		}
		finalmag = Mathf.Sqrt (Mathf.Pow (xmag, 2) + Mathf.Pow (zmag, 2));
		weightTimesVisitedAngle = ((calculateRaycastAngle (xmag, zmag))*-1-180);
		weightTimesVisitedAngle = (weightTimesVisitedAngle * Mathf.PI * 2 / 360);
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
								finalX = finalX + xmag;
								finalY = finalY+ zmag;
						}
			}
				finalWeight = Mathf.Sqrt (Mathf.Pow (finalX, 2) + Mathf.Pow (finalY, 2));
				finalAngle = calculateRaycastAngle(finalX, finalY);
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
		float finalAngle = Mathf.Atan (zmag / xmag);						
		if (xmag==0) { // edge cases first
			if(zmag>0){
				finalAngle = 90;
			}else{
				finalAngle = 270;
			}
		} else if (zmag == 0) {
			if(xmag>0){
				finalAngle = 0;
			}else{
				//print ("I HERE");
				finalAngle = 180;
			}
		} else {
			if(xmag>0){
				if(zmag> 0){
					finalAngle = (Mathf.Atan(zmag/xmag)); //QUAD 1
				}else{
					finalAngle =Mathf.PI*2+(Mathf.Atan(zmag/xmag)); //QUAD 4
				}
			}else{
				if(zmag>0){
					finalAngle = Mathf.PI+(Mathf.Atan(zmag/xmag));//QUAD 2
				}else{
					finalAngle = Mathf.PI+(Mathf.Atan(zmag/xmag)); // QUAD 3
				}
			}
		}
		finalAngle = ((finalAngle / (2 * Mathf.PI)) * 360);
		finalAngle = -(180+finalAngle) ;
		return finalAngle;
	}

	float calcuatetimesVisitedWeight(int timesVisited){
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
		neuralValues = new List<listNode> {}; 
		validColliders = new List<Collider>(); // initialize validColliders
		testSphere = GameObject.Find("Sphere"); //FOR TESTING
		previousSquare = new XZKey (0, 0);

	}

	/// Update this instance.
	/// see method headers for method descriptions
	// Update is called once per frame
	void Update () {
		Vector3 lockedAxis = testSphere.transform.eulerAngles;
		lockedAxis.x = 0;
		lockedAxis.z = 0;
		testSphere.transform.eulerAngles = lockedAxis;
		hitColliders = Physics.OverlapSphere (transform.position, radius);
		calculateValidColliders(hitColliders); //see method
		currentSpace (testSphere.transform.position.x, testSphere.transform.position.z, hitColliders);
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
