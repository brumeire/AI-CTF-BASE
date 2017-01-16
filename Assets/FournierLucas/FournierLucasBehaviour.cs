using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FournierLucasBehaviour : MonoBehaviour {


	//battle variable
	Vector3 positionBattle;
	bool enterBattle = false;
	bool minusBattle = false;

	//fouille variable
	Vector3[] positionSearch = {new Vector3(34.8f ,0,-15.8f), new Vector3(-10.1f,0, -29.2f),  new Vector3(-42.2f,0, 0f), new Vector3(-33.2f,0,19.3f), new Vector3(13,0,27.8f),  new Vector3(42.2f,0, 0f) } ;   
	private int nearest;

	//allies and enemies variable
 	public	List<GameObject> AlliesInRange = new List<GameObject>();
	public	List<GameObject> inRange = new List<GameObject>();
	GameObject toKill;
	GameObject toKill1FrameAgo;
	//enemies positions
	Vector3 lastPosition;
	Vector3 lastPosition1FrameAgo;


	//flag variable
	public bool OurFlagIsDown = false;
	public bool TheirFlagIsDown = false;
	public Vector3 WhereIsOurFlag;
	public Vector3 WhereIsTheirFlag;
	//variable flag in our base
	RaycastHit hito;

	//camping variables
	Vector3[] positionCampingBottom = {new Vector3(31.8f ,0,-7.6f), new Vector3(31.8f ,0,-2.6f) /*new Vector3(47.8f ,0,-30f) , new Vector3(31f,0, -26.6f)*/} ;   
	Vector3[] positionCampingTop = { new Vector3(-31.8f ,0,7.6f),new Vector3(-31.8f ,0,2.6f)/*new Vector3(-47.8f ,0,30f), new Vector3(-31f,0, 26.6f)*/} ;
	int nearestCamping = 0;
	bool clampRotate = false;

	/// //////////////////////////////


  // état actuel du bot
	public string comportement = "Flag";



  // Game master (script qui gère la capture des drapeaux, le respawn des bots et le score)
  GameMaster master;
  Team team;

  public TeamBehaviourFournierLucas teamLucas;

  // go qui possède les components du bot
  GameObject bot_object;
  // script de base contenant ID, team_ID du bot
  Bot bot;
  // components du game object de base du bot
  UnityEngine.AI.NavMeshAgent agent;
  Collider collider;
  Renderer renderer;

	
	void Start ()   {
    master = FindObjectOfType<GameMaster>();
    team = transform.parent.parent.GetComponent<Team>();
	teamLucas = team.gameObject.transform.GetComponentInChildren<TeamBehaviourFournierLucas> ();

    bot_object = transform.parent.gameObject;
    bot = bot_object.GetComponent<Bot>();
    agent = bot_object.GetComponent<UnityEngine.AI.NavMeshAgent>();
    collider = bot_object.GetComponent<Collider>();
    renderer = bot_object.GetComponent<Renderer>();

	}


  void Update()  {

		GizmosService.Cone(transform.position, transform.forward, Vector3.up, 10, 70);
		   
		if (comportement != "Camping" && comportement != "Go to the camping") {
			CampingChange ();
		}

		detection ();

		IsTheFlagInOurBase ();

		IsThereAnAlly ();


		ChangeYourComportement ();


		switch (comportement) {

		case "Flag":

			// va directement vers le drapeau enemi

			agent.SetDestination (team.enemy_base.position);
		
			if (bot.team_ID == 0) {
				if (master.flag_carriers [1] != -1) {
					if (toKill != null) {
						comportement = "Go to enemies";
					}
				}
			} else if (bot.team_ID == 1) {
				if (master.flag_carriers [0] != -1) {
					if (toKill != null) {
						comportement = "Go to enemies";
					}
				}
			} else if (toKill != null) {
				comportement = "Hit and run";
			}


			break;

		case "Hit and run":

			//continue à aller vers le drapeau enemi tout en tirant sur les adversaires


			agent.SetDestination (team.enemy_base.position);


			shoot ();

			if (toKill == null) {
				comportement = "Flag";
			}

			if (IsThereAnAlly ()) {
				enterBattle = false;
				comportement = "Battle";
			}


			break;

		case "Battle":

			// si il a un allié avec lui, rentre en mode combat, il se décale sur les cotés et tir


			if (!enterBattle) {				
				positionBattle = transform.position;
				enterBattle = true;
			}

			shoot ();

			if (enterBattle) {
				if (toKill != null && Mathf.Abs (toKill.transform.position.x - transform.position.x) > Mathf.Abs (toKill.transform.position.z - transform.position.z)) {

					if (minusBattle) {
						agent.SetDestination (new Vector3 (positionBattle.x - 8, 0, positionBattle.z));

						if (Vector3.Distance (new Vector3 (positionBattle.x - 8, 0, positionBattle.z), transform.position) < 1) {
							minusBattle = false;
						}

					} else if (!minusBattle) {
						agent.SetDestination (new Vector3 (positionBattle.x + 8, 0, positionBattle.z));
						if (Vector3.Distance (new Vector3 (positionBattle.x + 8, 0, positionBattle.z), transform.position) < 1) {
							minusBattle = true;
						}
					}

				} else {

					if (minusBattle) {
						agent.SetDestination (new Vector3 (positionBattle.x, 0, positionBattle.z - 8));

						if (Vector3.Distance (new Vector3 (positionBattle.x, 0, positionBattle.z - 8), transform.position) < 1) {
							minusBattle = false;
						}

					} else if (!minusBattle) {
						agent.SetDestination (new Vector3 (positionBattle.x, 0, positionBattle.z + 8));
						if (Vector3.Distance (new Vector3 (positionBattle.x, 0, positionBattle.z + 8), transform.position) < 1) {
							minusBattle = true;
						}
					}

				}

			}

		

			if (!IsThereAnAlly ()) {
				//si il n'a plus d'allié, va au drapeau enemi
				comportement = "Hit and run";
			}

			if (toKill == null) {
				//si il a encore un allié va a la dernière position occupé par l'enemi
				comportement = "Poursuit enemi";
			}

			break;

		case "Poursuit enemi":
			// va a la dernière position occupé par la cible
			if (toKill == null) {
				agent.SetDestination (lastPosition);
				if (transform.position == lastPosition) {					
					comportement = "Flag";
				}
			}

			if (toKill != null) {	
				//si il retrouve la cible, retourne en mode combat
				enterBattle = false;
				comportement = "Battle";
			}


			break;

		case "Take their flag":

			// Ramasse le drapeau enemi si il est a terre
			agent.SetDestination (WhereIsTheirFlag);

			shoot ();

			if (!TheirFlagIsDown) {
				comportement = "Flag";
			}



			break;

		case "Save our flag":
			// ramasse notre drapeau si il est a terre et pas sur notre base
			agent.SetDestination (WhereIsOurFlag);

			shoot ();

			if (!OurFlagIsDown) {
				comportement = "Flag";
			}

			break;

		
			
		case "Protect":
		//suit le porteur de drapeau allié
			shoot ();

			if (bot.team_ID == 0) {
				if (master.flag_carriers [1] == -1) {
					comportement = "Flag";
				} else {
					agent.SetDestination (GameObject.Find ("bot-" + master.flag_carriers [1] + "_team-" + bot.team_ID).transform.position);
				}
			} else if (bot.team_ID == 1) {
				if (master.flag_carriers [0] == -1) {
					comportement = "Flag";
				} else {
					agent.SetDestination (GameObject.Find ("bot-" + master.flag_carriers [0] + "_team-" + bot.team_ID).transform.position);
				}
			}		

			break;

		case "SearchBegin":
			//cherche l'endroit le plus proche parmi un tableau de position
			float near = 9999999;
			for (int i = 0; i < positionSearch.Length; i++) {
				if (Vector3.Distance (positionSearch [i], transform.position) < near) {
					near = Vector3.Distance (positionSearch [i], transform.position);	
					nearest = i;
				}
			}

			comportement = "Search";

			break;

		case "Search":
			// se déplace de point en points jusqu'a trouver quelquechose
			agent.SetDestination (positionSearch [nearest]);

			if (bot.team_ID == 0) {
				if (Vector3.Distance (positionSearch [nearest], transform.position) < 5f) {
					nearest++;
					if (nearest >= positionSearch.Length) {
						nearest = 0;
					}
				}
			}if (bot.team_ID == 1) {
				if (Vector3.Distance (positionSearch [nearest], transform.position) < 5f) {
					nearest--;
					if (nearest <= -1) {
						nearest = positionSearch.Length-1;
					}
				}
			}
			if (toKill != null) {
				comportement = "Go to enemies";
			}

			break;

		case "Go to enemies":
			// se raproche des enemis si il est top loin
			shoot ();

			if (Vector3.Distance (lastPosition, transform.position) > 25f) {
				agent.SetDestination (lastPosition);
			} else {	
				enterBattle = false;
				comportement = "Battle";
			}


			break;

		case "Go to the camping":

			float nearCamping = 9999999;
			/*
			if (Vector3.Distance (transform.position, master.spawn_points [team.team_ID].transform.position) > 15f) {
				if (team.team_ID == 0) {
					for (int i = 0; i < 4; i++) {
						Debug.Log ("Camping 2");
						if (Vector3.Distance (positionCampingBottom [i], transform.position) < nearCamping) {
							Debug.Log ("Camping 3");
							near = Vector3.Distance (positionCampingBottom [i], transform.position);	
							nearestCamping = i;
						}
					}
				} else if (team.team_ID == 1) {
					for (int i = 0; i < positionCampingTop.Length; i++) {
						if (Vector3.Distance (positionCampingTop [i], transform.position) < nearCamping) {
							near = Vector3.Distance (positionCampingTop [i], transform.position);	
							nearestCamping = i;
						}
					}
				}
			} else {
			*/
			nearestCamping = 0;
		//}
			Debug.Log (comportement);
			clampRotate = false;
			comportement = "Camping";
			Debug.Log (comportement);

			break;

		case "Camping":

			Debug.Log (nearestCamping);

			if (nearestCamping < positionCampingTop.Length) {

				if (team.team_ID == 0) {
					agent.SetDestination (positionCampingBottom [nearestCamping]);
				} else if (team.team_ID == 1) {
					agent.SetDestination (positionCampingTop [nearestCamping]);
				}
			}

			if (bot.team_ID == 0) {
				if (nearestCamping < positionCampingBottom.Length && Mathf.Abs (Vector3.Distance (positionCampingBottom [nearestCamping], bot.transform.position)) < 3f  ) {
					nearestCamping++;
				}
			} else if (bot.team_ID == 1) {
				if (nearestCamping < positionCampingTop.Length && Mathf.Abs (Vector3.Distance (positionCampingTop [nearestCamping], bot.transform.position)) < 3f ) {
					nearestCamping++;
				}
			}

			if (nearestCamping >= positionCampingTop.Length && !clampRotate) {
				agent.enabled = false;
				bot.transform.localEulerAngles = new Vector3(0,Mathf.Round(bot.transform.localEulerAngles.y),0);
				clampRotate = true;
				agent.enabled = true;
			}

			shoot ();

			break;


		case "Go to base":
			// seul le porteur de drapeau peut entrer dans cet état, il retourne a sa base
			agent.SetDestination (team.team_base.position);

			shoot ();

			if (bot.team_ID == 0) {
				if (master.flag_carriers [1] != bot.ID) {
					comportement = "Flag";
				}
			}else if (bot.team_ID == 1) {
				if (master.flag_carriers [0] != bot.ID) {
					comportement = "Flag";
				}
			}
			
			break;
	 }

		lastPosition1FrameAgo = lastPosition;
		toKill1FrameAgo = toKill;
	}


	void shoot(){

		float distanceToKill = 0;
		if (toKill != null) {
			distanceToKill = Vector3.Distance (bot.transform.position, toKill.transform.position);
		}
		// tir sur l'enemi le plus proche ou le porteur de drapeau qu'il peut voir
		if (bot.can_shoot && toKill != null && toKill == toKill1FrameAgo) {

			float déplacementballe = Vector3.Distance (new Vector3 (16f, 0f, -14f), new Vector3 (15.65378f, 0f, -13.72847f));
			float timeTravelingShot = distanceToKill / déplacementballe;

			Vector3 directionToKill = (toKill.transform.position - lastPosition1FrameAgo);
			Vector3 positionAfterMove = new Vector3 (lastPosition.x + directionToKill.x * timeTravelingShot, lastPosition.y + directionToKill.y * timeTravelingShot, lastPosition.z + directionToKill.z * timeTravelingShot);

			distanceToKill = Vector3.Distance (bot.transform.position, positionAfterMove);
			float timeTravelingShot2 = (distanceToKill / déplacementballe)-timeTravelingShot;
			positionAfterMove =  new Vector3 (positionAfterMove.x + directionToKill.x * timeTravelingShot2, positionAfterMove.y + directionToKill.y * timeTravelingShot2, positionAfterMove.z + directionToKill.z * timeTravelingShot2);

			bot.ShootInDirection ((positionAfterMove - bot.transform.position));



			//bot.ShootInDirection ((toKill.transform.position- transform.position));
		} /*else if (bot.can_shoot && toKill != null) {
			bot.ShootInDirection ((toKill.transform.position- transform.position));
		}*/
	}


	void detection(){

		//Leur drapeau est il a terre ?
		if (bot.CanSeeObject (team.enemy_flag.gameObject)) {
				
			if (master.flag_carriers [bot.enemy_team_ID] == -1) {
				TheirFlagIsDown = true;
				WhereIsTheirFlag = new Vector3 (team.enemy_flag.transform.position.x, team.enemy_flag.transform.position.y, team.enemy_flag.transform.position.z);
			}
		} else {
			TheirFlagIsDown = false;
		}

		//Notre drapeau est il a terre et pas sur notre base ?
		if (bot.CanSeeObject (team.team_flag.gameObject)) {				
			if (!master.is_flag_home [bot.team_ID]) {
				OurFlagIsDown = true;
				WhereIsOurFlag = new Vector3 (team.team_flag.transform.position.x, team.team_flag.transform.position.y, team.team_flag.transform.position.z);					
			}
		} else {
			OurFlagIsDown = false;
		}

		// Is there enemies to kill ? espacially a flag carrier ? Assign a target
		inRange.Clear ();
		toKill = null;

		float near = 9999999;


		for (int f =0; f < GameObject.FindGameObjectsWithTag("Bot").Length/2; f++){
			if (GameObject.Find ("bot-" + f + "_team-" + bot.enemy_team_ID)) {
				if (bot.CanSeeObject (GameObject.Find ("bot-" + f + "_team-" + bot.enemy_team_ID))) {
					inRange.Add (GameObject.Find ("bot-" + f + "_team-" + bot.enemy_team_ID));
				}
			}
		}



		for ( int i = 0; i < inRange.Count; i++){
			if (Vector3.Distance (inRange [i].transform.position, transform.position) < near) {
				toKill = inRange [i].gameObject;
				lastPosition = toKill.transform.position;
				near = Vector3.Distance (inRange [i].transform.position, transform.position);

				//Si il porte le drapeau il le vise en priorité

				if (bot.team_ID == 0) {
					if (inRange [i].gameObject.GetComponent<Bot> ().ID == master.flag_carriers [0]) {
						near = 0;
					}
				} else if (bot.team_ID == 1) {
					if (inRange [i].gameObject.GetComponent<Bot> ().ID == master.flag_carriers [1]) {
						near = 0;
					}
				}
			}

			// Know if you still target the samebot or if it's the second time you see it
			if (toKill != toKill1FrameAgo) {
				toKill1FrameAgo = null;
			}
		}
		
	}

	public void IsTheFlagInOurBase (){
		//si il voit sa base le bot peut savoir si il y a son drapeau sur sa base ou non
		Vector3 dir_to_obj = team.team_base.position - transform.position;
		hito = new RaycastHit ();
		Physics.Linecast (transform.position, team.team_base.position);

		if (!Physics.Linecast (transform.position, team.team_base.position)) {
			if (Vector3.Angle (transform.forward, dir_to_obj) <= 70) {
				teamLucas.flagNotInOurBase = true;
			}			
		} else if (Physics.Linecast (transform.position, team.team_base.position)) {

			if (bot.CanSeeObject (team.team_flag.gameObject) && bot.team_ID == 0 && master.is_flag_home [0]) {
				teamLucas.flagNotInOurBase = false;
			}
			else if (bot.CanSeeObject (team.team_flag.gameObject) && bot.team_ID == 1 && master.is_flag_home [1]) {
				teamLucas.flagNotInOurBase = false;
			}


		}
	}


	public bool CanSeeAnAlly(GameObject obj){
		//check si il y a un allié précis autour de lui
		Vector3 dir_to_obj = obj.transform.position - transform.position;
		Ray r = new Ray(transform.position, dir_to_obj);
		RaycastHit hit;
		// si on ne cherche pas spécifiquement un drapeau, on ignore celui-ci dans le raycast
		int layer_mask = Physics.DefaultRaycastLayers;
		if(obj.tag != "Flag")
		{
			layer_mask = 1 << LayerMask.NameToLayer("Flag");
			layer_mask |= Physics.IgnoreRaycastLayer;
			layer_mask = ~layer_mask;
		}

		if(Physics.Raycast(r, out hit, Mathf.Infinity, layer_mask))
		{
			if(hit.collider.gameObject == obj)
			{
				if( Mathf.Abs(obj.transform.position.x-transform.position.x)<15  && Mathf.Abs(obj.transform.position.z-transform.position.z)<15)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsThereAnAlly(){
		//check si il y a un des allié autour de lui
	AlliesInRange.Clear ();
	bool result = false;

		for (int i =0; i < GameObject.FindGameObjectsWithTag("Bot").Length/2; i++){
			if (GameObject.Find ("bot-" + i + "_team-" + bot.team_ID)) {
				if(CanSeeAnAlly(GameObject.Find ("bot-" + i + "_team-" + bot.team_ID))){
					AlliesInRange.Add(GameObject.Find ("bot-" + i + "_team-" + bot.team_ID));
				result = true ;
				}
			}
		}
		return result;

	}

	public void allProtection(){
			//Les bots proches du porteurs du porteur de drapeau le suivent
		if (bot.team_ID == 0) {
			if (master.flag_carriers [1] != -1 && master.flag_carriers [1] != -bot.ID && comportement != "Battle" && comportement != "Search") {
				if (GameObject.Find ("bot-" + master.flag_carriers [1] + "_team-" + bot.team_ID) && Mathf.Abs (GameObject.Find ("bot-" + master.flag_carriers [1] + "_team-" + bot.team_ID).transform.position.x - transform.position.x) < 25 && Mathf.Abs (GameObject.Find ("bot-" + master.flag_carriers [1] + "_team-" + bot.team_ID).transform.position.z - transform.position.z) < 20) {
					comportement = "Protect";
				}
			}
		}else if (bot.team_ID == 1 ) {
			if (master.flag_carriers [0] != -1 && master.flag_carriers [0] != -bot.ID && comportement != "Battle" && comportement != "Search") {
				if (GameObject.Find ("bot-" + master.flag_carriers [0] + "_team-" + bot.team_ID) && Mathf.Abs (GameObject.Find ("bot-" + master.flag_carriers [0] + "_team-" + bot.team_ID).transform.position.x - transform.position.x) < 25 && Mathf.Abs (GameObject.Find ("bot-" + master.flag_carriers [0] + "_team-" + bot.team_ID).transform.position.z - transform.position.z) < 20) {
					comportement = "Protect";
				}
			}
		}
	}

	public void CampingChange(){
		if (bot.ID == 1 || bot.ID == 11 && comportement!= "Camping") {
			comportement = "Go to the camping";
		}
	}


	void ChangeYourComportement (){

		//if our flag is down
		if (OurFlagIsDown) {
			comportement = "Save our flag";
			return;
		}

		// if i'm the flag carrier, I return to the base

		if (bot.team_ID == 0) {
			if (master.flag_carriers [1] == bot.ID) {
				comportement = "Go to base";
				return;
			}
		}else if (bot.team_ID == 1) {
			if (master.flag_carriers [0] == bot.ID) {
				comportement = "Go to base";
				return;
			}
		}


		// if their flag is down
		if (TheirFlagIsDown) {
			comportement = "Take their flag";
			return;
		}

		if (comportement == "Go to base") {
			if (bot.team_ID == 0) {
				if (master.flag_carriers [1] != bot.ID) {
					comportement = "Flag";
					return;
				}
			}else if (bot.team_ID == 1) {
				if (master.flag_carriers [0] != bot.ID) {
					comportement = "Flag";
					return;
				}
			}
		}

	}

}
  


 