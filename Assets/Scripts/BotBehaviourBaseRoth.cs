using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BotBehaviourBaseRoth : MonoBehaviour {

	private int nearPos;
	Vector3[] roundForFind = new Vector3[4];
	Vector3[] roundOfDefender = new Vector3[5];
	Vector3[] roundOfTeamBase = new Vector3[4];
	Vector3[] roundOfEnemyBase = new Vector3[4];

	public	List<GameObject> detectBot = new List<GameObject>();
	public GameObject enemyTarget;
	GameObject lastEnemyTarget; // Permet de Tag après perte de vue d'une cible ennemie

	public bool allyFlagLost = false;
	public bool enemyFlagLost = false;
	public Vector3 positionAllyFlag;
	public Vector3 positionEnemyFlag;

	public bool seeEnemyFlag;
	public bool seeAllyFlag;

	public int ID;

	float countdownTag;
	float timeValueForTag;

	string TEXT;
	bool TAG;

	GameObject nameAbove;
	string NAME;
	GameObject stateAbove;
	string currentSTATE;

	// liste des states possibles pour ce comportement de bot
	public enum BotState {
		TEAM_BASE,	      // => Se dirige vers l'une ou l'autre base.
		ATTACK_BASE,
		DEFEND_BASE,	  // => Suit un pattern de déplacement pour défendre la base et ses environs.
		GO_TO_MATCH,
			  
		FIND_ELEMENT,	  // => Si mémoire position drapeau erronée, fait un pattern de déplacement pour le retrouver.

		TAKE_TEAM_FLAG,   // => Permet de se diriger vers la dernière position connu du drapeau.
		TAKE_ENEMY_FLAG,

		FOLLOW_CAPTAIN,   // => Va retrouver la position de tel ou tel bot.
		FOLLOW_ASSAULT,
		FOLLOW_DEFENDER
	};

	// état du bot
	public BotState state;

	// Game master (script qui gère la capture des drapeaux, le respawn des bots et le score)
	GameMaster master;
	public Team team;

	// go qui possède les components du bot
	GameObject bot_object;
	// script de base contenant ID, team_ID du bot
	Bot bot;
	// components du game object de base du bot
	UnityEngine.AI.NavMeshAgent agent;
	Collider collider;
	Renderer renderer;

	// go qui possèdent les membre de l'équipe
	GameObject captain;
	GameObject second;
	GameObject assault;
	GameObject defender;
	GameObject support;


	//==========================================//
	// ----------------- START ---------------- //
	//==========================================//

	void Start () 
	{
		ID = transform.parent.GetComponent<Bot> ().ID;

		master = FindObjectOfType<GameMaster>();
		team = transform.parent.parent.GetComponent<Team>();

		bot_object = transform.parent.gameObject;
		bot = bot_object.GetComponent<Bot>();
		agent = bot_object.GetComponent<UnityEngine.AI.NavMeshAgent>();
		collider = bot_object.GetComponent<Collider>();
		renderer = bot_object.GetComponent<Renderer>();

		if (ID < 10) {	// team 0 (rouge)
			captain = GameObject.Find ("bot-0_team-0");
			second = GameObject.Find ("bot-1_team-0");
			assault = GameObject.Find ("bot-2_team-0");
			defender = GameObject.Find ("bot-3_team-0");
			support = GameObject.Find ("bot-4_team-0");

			roundForFind [0] = new Vector3 (30, 0, -27);
			roundForFind [1] = new Vector3 (-31, 0, -22.5f);
			roundForFind [2] = new Vector3 (-21.5f, 0, 20.5f);
			roundForFind [3] = new Vector3 (20, 0, 0.5f);

			roundOfDefender [0] = new Vector3 (41.5f, 0, -10.5f);
			roundOfDefender [1] = new Vector3 (21.5f, 0, -20f);
			roundOfDefender [2] = new Vector3 (18, 0, 0);
			roundOfDefender [3] = new Vector3 (39, 0, 16);
			roundOfDefender [4] = new Vector3 (32, 0, 6.5f);

			roundOfTeamBase [0] = new Vector3 (46, 0, -6);
			roundOfTeamBase [1] = new Vector3 (34, 0, -6);
			roundOfTeamBase [2] = new Vector3 (46, 0, -6);
			roundOfTeamBase [3] = new Vector3 (46, 0, 6);

			roundOfEnemyBase [0] = new Vector3 (-46, 0, 6);
			roundOfEnemyBase [1] = new Vector3 (-34, 0, -6);
			roundOfEnemyBase [2] = new Vector3 (-34, 0, 6);
			roundOfEnemyBase [3] = new Vector3 (-46, 0, -6);
		}

		if (ID >= 10) {	// team 1 (bleu)
			captain = GameObject.Find ("bot-0_team-1");
			second = GameObject.Find ("bot-1_team-1");
			assault = GameObject.Find ("bot-2_team-1");
			defender = GameObject.Find ("bot-3_team-1");
			support = GameObject.Find ("bot-4_team-1");

			roundForFind [0] = new Vector3 (-30, 0, 27);
			roundForFind [1] = new Vector3 (31, 0, 22.5f);
			roundForFind [2] = new Vector3 (21.5f, 0, -20.5f);
			roundForFind [3] = new Vector3 (-20, 0, 0.5f);

			roundOfDefender [0] = new Vector3 (-41.5f, 0, 10.5f);
			roundOfDefender [1] = new Vector3 (-21.5f, 0, 20);
			roundOfDefender [2] = new Vector3 (-18, 0, 0);
			roundOfDefender [3] = new Vector3 (-39, 0, -16);
			roundOfDefender [4] = new Vector3 (-32, 0, -6.5f);

			roundOfTeamBase [0] = new Vector3 (-46, 0, 6);
			roundOfTeamBase [1] = new Vector3 (-34, 0, 6);
			roundOfTeamBase [2] = new Vector3 (-46, 0, 6);
			roundOfTeamBase [3] = new Vector3 (-46, 0, -6);

			roundOfEnemyBase [0] = new Vector3 (46, 0, -6);
			roundOfEnemyBase [1] = new Vector3 (34, 0, 6);
			roundOfEnemyBase [2] = new Vector3 (34, 0, -6);
			roundOfEnemyBase [3] = new Vector3 (46, 0, 6);
		}

		positionAllyFlag = team.team_base.position;
		positionEnemyFlag = team.enemy_base.position;

		if (ID == 0 || ID == 10)
			NAME = "Captain";
		else if (ID == 1 || ID == 11)
			NAME = "Second";
		else if (ID == 2 || ID == 12)
			NAME = "Assault";
		else if (ID == 3 || ID == 13)
			NAME = "Defender";
		else if (ID == 4 || ID == 14)
			NAME = "Support";

		if (NAME == "Second") {
			TAG = true;
			TEXT = "KEK";
		}
		if (NAME == "Defender") {
			TAG = true;
			TEXT = "pixelArt";
		}

		GameObject nameAbove = new GameObject ();
		nameAbove.transform.parent = this.transform;
		nameAbove.name = "NAME : " + transform.parent.GetComponent<Bot> ().team_ID + ID + NAME;
		// position name
		nameAbove.AddComponent<Canvas> ();
		nameAbove.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 10;
		nameAbove.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 270, 0);
		nameAbove.GetComponent<RectTransform> ().sizeDelta = new Vector2 (9, 3);
		nameAbove.transform.position = new Vector3 (transform.position.x, 5f, transform.position.z);
		// text tag
		Font ArialFont = (Font)Resources.GetBuiltinResource (typeof(Font), "Arial.ttf");
		nameAbove.AddComponent<Text> ().text = NAME;
		nameAbove.GetComponent<Text> ().fontStyle = FontStyle.Bold;
		nameAbove.GetComponent<Text> ().alignment = TextAnchor.MiddleCenter;
		nameAbove.GetComponent<Text> ().font = ArialFont;
		nameAbove.GetComponent<Text> ().fontSize = 2;
		nameAbove.GetComponent<Text> ().color = new Color (master.team_color[transform.parent.GetComponent<Bot> ().team_ID].r + 0.4f, master.team_color[transform.parent.GetComponent<Bot> ().team_ID].g + 0.4f, master.team_color[transform.parent.GetComponent<Bot> ().team_ID].b + 0.4f, 1);

		GameObject stateAbove = new GameObject ();
		stateAbove.transform.parent = this.transform;
		stateAbove.name = "STATE : " + transform.parent.GetComponent<Bot> ().team_ID + ID + NAME;
		// position name
		stateAbove.AddComponent<Canvas> ();
		stateAbove.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 10;
		stateAbove.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 270, 0);
		stateAbove.GetComponent<RectTransform> ().sizeDelta = new Vector2 (8, 2);
		stateAbove.GetComponent<RectTransform> ().localScale = new Vector3 (1.5f, 1.5f, 1.5f);
		stateAbove.transform.position = new Vector3 (transform.position.x + 1.5f, 5f, transform.position.z);
		// text tag
		stateAbove.AddComponent<Text> ().text = NAME;
		stateAbove.GetComponent<Text> ().fontStyle = FontStyle.Bold;
		stateAbove.GetComponent<Text> ().alignment = TextAnchor.MiddleCenter;
		stateAbove.GetComponent<Text> ().font = ArialFont;
		stateAbove.GetComponent<Text> ().fontSize = 1;
		stateAbove.GetComponent<Text> ().color = new Color (0.3f, 1f, 0.3f, 1);

		timeValueForTag = Random.Range (15, 31);
	}

	//==========================================//
	// ---------------- UPDATE ---------------- //
	//==========================================//

	void Update()
	{
		detectElements ();
		launchRocket ();


		// ---------------- CAPTAIN --------------- //	=> Ses états varient selon les circonstances entre Attaque et Défense.

		if (NAME == "Captain") {
			if (TAG) {
				DrawnTag ();
			}

			SwitchState(BotState.ATTACK_BASE);

			if (master.flag_carriers [bot.enemy_team_ID] == -1)
				SwitchState(BotState.TAKE_ENEMY_FLAG);

			if (positionEnemyFlag != team.enemy_base.position)
				SwitchState(BotState.FIND_ELEMENT);

			if (allyFlagLost)
				SwitchState(BotState.TEAM_BASE);
		}
													
		// ---------------- SECOND ---------------- //	=> Va suivre tout le temps le Captain, si le Captain meurt, il prend son rôle.

		if (NAME == "Second") {
			if (TAG) {
				DrawnTag ();
			}

			SwitchState(BotState.FOLLOW_CAPTAIN);
		}

		// ---------------- ASSAULT --------------- //	=> Va tout le temps avoir des états fait pour l'Attaque du drapeau ennemi.

		if (NAME == "Assault") {
			if (TAG) {
				DrawnTag ();
			}
			
			SwitchState(BotState.ATTACK_BASE);
		}

		// --------------- DEFENDER --------------- //	=> Va tout le temps avoir des états fait pour la Défense du drapeau alliée.

		if (NAME == "Defender") {
			if (TAG) {
				DrawnTag ();
			}

			SwitchState(BotState.DEFEND_BASE);

			if (positionAllyFlag != team.team_base.position && allyFlagLost)
				SwitchState (BotState.ATTACK_BASE);
		}
													
		// ---------------- SUPPORT --------------- //	=> Va aider le Assault ou le Defender en cas de besoin (autrement, se balade sur le terrain ou reprend le drapeau allié)

		if (NAME == "Support") {
			if (TAG) {
				DrawnTag ();
			}

			SwitchState(BotState.FIND_ELEMENT);

			if (positionAllyFlag != team.team_base.position && allyFlagLost)
				SwitchState(BotState.TAKE_TEAM_FLAG);
			
			if (master.flag_carriers [bot.enemy_team_ID] != -1)
				SwitchState (BotState.FOLLOW_ASSAULT);
			
			if (master.flag_carriers [bot.team_ID] != -1)
				SwitchState (BotState.FOLLOW_DEFENDER);
		}

		// ---------------------------------------- //

		// EN COMMUN A TOUS //

		if (master.flag_carriers [bot.enemy_team_ID] == ID)	// Si possède le drapeau, se déplacement dans la base alliée.
			SwitchState (BotState.TEAM_BASE);

		if (master.flag_carriers [bot.enemy_team_ID] == ID && !allyFlagLost) // Si possède le dreapeau, et drapeau alliée présent, va marquer le point.
			SwitchState (BotState.GO_TO_MATCH);

		if (seeAllyFlag && allyFlagLost)  // A vue le drapeau alliée libre : va le prendre (si ne possède pas le drapeau ennemi sur lui).
			SwitchState (BotState.TAKE_TEAM_FLAG);
		if (seeEnemyFlag)  // A vue le drapeau ennemi libre : va le prendre.
			SwitchState (BotState.TAKE_ENEMY_FLAG);
		

		TextAboveUpdate (); //Afficher texte au dessus de l'objet
		UpdateState();

		countdownTag += Time.deltaTime;

		if (bot.team_ID == 0) {
			if (countdownTag >= timeValueForTag) {
				timeValueForTag = Random.Range (15, 31);
				countdownTag = 0;

				int randomTag = Random.Range (0, 6);

				if (randomTag == 0)
					TEXT = "LOL";
				else if (randomTag == 1)
					TEXT = "GG";
				else if (randomTag == 2)
					TEXT = "KEK";
				else if (randomTag == 3)
					TEXT = "swag";
				else if (randomTag == 4)
					TEXT = "yolo";
				else if (randomTag == 5)
					TEXT = "pixelArt";
				
				TAG = true;
			}
		}
	}

	//==========================================//
	// ------------ STATE FUNCTIONS ----------- //
	//==========================================//

	// ---------------- SWITCH ---------------- //

	void SwitchState(BotState new_state)
	{
		state = new_state;
		OnEnterState ();
	}

	// ---------------- ENTER ----------------- //

	void OnEnterState()
	{
		switch(state)
		{
		case BotState.FIND_ELEMENT:
			break;

		case BotState.TAKE_ENEMY_FLAG:
			break;

		case BotState.TAKE_TEAM_FLAG:
			break;

		case BotState.ATTACK_BASE:
			break;

		case BotState.DEFEND_BASE:
			break;

		case BotState.TEAM_BASE:
			break;

		case BotState.GO_TO_MATCH:
			break;

		case BotState.FOLLOW_CAPTAIN:
			break;

		case BotState.FOLLOW_ASSAULT:
			break;
		
		case BotState.FOLLOW_DEFENDER:
			break;
		}
	}

	// ---------------- UPDATE ---------------- //

	void UpdateState()
	{
		switch(state)
		{

		case BotState.FIND_ELEMENT:
			currentSTATE = "SEARCH";
			// move around the map
			agent.SetDestination (roundForFind [nearPos]);
			if (Vector3.Distance (roundForFind [nearPos], transform.position) < 5f) {
				nearPos++;
				if (nearPos >= 4)
					nearPos = 0;
			}
			break;

		case BotState.ATTACK_BASE:
			currentSTATE = "ATTACK";
			// move into the enemy base
			agent.SetDestination (roundOfEnemyBase [nearPos]);
			if (Vector3.Distance (roundOfEnemyBase [nearPos], transform.position) < 5f) {
				nearPos++;
				if (nearPos >= 4)
					nearPos = 0;
			}

			break;

		case BotState.DEFEND_BASE:
			currentSTATE = "DEFEND";
			// move around the team base
			agent.SetDestination (roundOfDefender [nearPos]);
			if (Vector3.Distance (roundOfDefender [nearPos], transform.position) < 5f) {
				nearPos++;
				if (nearPos >= 4)
					nearPos = 0;
			}
			break;

        case BotState.TAKE_ENEMY_FLAG:
			currentSTATE = "ENEMY FLAG";
			// go to enemy flag position
            agent.SetDestination(positionEnemyFlag);
            break;

        case BotState.TAKE_TEAM_FLAG:
			currentSTATE = "ALLY FLAG";
			// go to ally flag position
           agent.SetDestination(positionAllyFlag);
           break;

        case BotState.TEAM_BASE:
			currentSTATE = "TEAM BASE";
			// move into the enemy base
			agent.SetDestination (roundOfTeamBase [nearPos]);
			if (Vector3.Distance (roundOfTeamBase [nearPos], transform.position) < 5f) {
				nearPos++;
				if (nearPos >= 4)
					nearPos = 0;
			}
			if (positionAllyFlag == team.team_base.position)
				SwitchState (BotState.GO_TO_MATCH);
			break;

		case BotState.GO_TO_MATCH:
			currentSTATE = "GO GO GO";
			// go to scored the flag
			agent.SetDestination(team.team_base.position);
			break;

		case BotState.FOLLOW_CAPTAIN:
			currentSTATE = "FOLLOW CAP";
			// go to captain position
			if (captain.activeInHierarchy == true)
				agent.SetDestination (captain.transform.position);
			else {
				currentSTATE = "REVENGE CAP";
				// move into the enemy base
				agent.SetDestination (roundOfEnemyBase [nearPos]);
				if (Vector3.Distance (roundOfEnemyBase [nearPos], transform.position) < 5f) {
					nearPos++;
					if (nearPos >= 4)
						nearPos = 0;
				}
			}
			break;

		case BotState.FOLLOW_ASSAULT:
			currentSTATE = "FOLLOW ASS";
			// go to assault position
			if (assault.activeInHierarchy == true)
				agent.SetDestination(assault.transform.position);
			else {
				currentSTATE = "REVENGE ASS";
				// move into the enemy base
				agent.SetDestination (roundOfEnemyBase [nearPos]);
				if (Vector3.Distance (roundOfEnemyBase [nearPos], transform.position) < 5f) {
					nearPos++;
					if (nearPos >= 4)
						nearPos = 0;
				}
			}
			break;

		case BotState.FOLLOW_DEFENDER:
			currentSTATE = "FOLLOW DEF";
			// go to defender position
			if (defender.activeInHierarchy == true)
				agent.SetDestination(defender.transform.position);
			else {
				currentSTATE = "REVENGE DEF";
				// move into the enemy base
				agent.SetDestination (roundOfEnemyBase [nearPos]);
				if (Vector3.Distance (roundOfEnemyBase [nearPos], transform.position) < 5f) {
					nearPos++;
					if (nearPos >= 4)
						nearPos = 0;
				}
			}
			break;
		}

	}

	//==========================================//
	// ----------- ACTION FUNCTIONS ----------- //
	//==========================================//


	// ---------------- TIRER ----------------- //

	void launchRocket() {
		if (bot.can_shoot && enemyTarget != null) {
			agent.SetDestination (transform.position);
			bot.ShootInDirection ((enemyTarget.transform.position- transform.position));
		}
	}

	// ------------- ELEMENTS VUS ------------- //

	void detectElements() {
		lastEnemyTarget = enemyTarget;

		// Detect Enemy Flag ?
		if (bot.CanSeeObject (team.enemy_flag.gameObject)) {
			
			if (master.flag_carriers [bot.enemy_team_ID] == -1) {
				enemyFlagLost = false;
				positionEnemyFlag = new Vector3 (team.enemy_flag.transform.position.x, team.enemy_flag.transform.position.y, team.enemy_flag.transform.position.z);

				seeEnemyFlag = true;
			}

			if (seeEnemyFlag)
				transform.LookAt (positionEnemyFlag);

		} else {
			seeEnemyFlag = false;
			enemyFlagLost = true;
		}

		// Detect Ally Flag ?
		if (bot.CanSeeObject (team.team_flag.gameObject)) {
			
			if (!master.is_flag_home [bot.team_ID]) {
				positionAllyFlag = new Vector3 (team.team_flag.transform.position.x, team.team_flag.transform.position.y, team.team_flag.transform.position.z);
				allyFlagLost = true;
			}

			if (master.is_flag_home [bot.team_ID]) {
				positionAllyFlag = new Vector3 (team.team_flag.transform.position.x, team.team_flag.transform.position.y, team.team_flag.transform.position.z);
				allyFlagLost = false;
			}

			if (positionAllyFlag != team.team_base.position && master.flag_carriers [bot.enemy_team_ID] != ID)
				seeAllyFlag = true;
		} else {
			seeAllyFlag = false;
		}

		if (master.is_flag_home [bot.team_ID]) {
			allyFlagLost = false;
		}

		// Detect Enemy ?
		detectBot.Clear ();
		enemyTarget = null;

		float range = 99999;

		for (int f =0; f < GameObject.FindGameObjectsWithTag("Bot").Length/2; f++) {
			if (GameObject.Find ("bot-" + f + "_team-" + bot.enemy_team_ID)) {
				if (bot.CanSeeObject (GameObject.Find ("bot-" + f + "_team-" + bot.enemy_team_ID))) {
					detectBot.Add (GameObject.Find ("bot-" + f + "_team-" + bot.enemy_team_ID));
				}
			}
		}

		for ( int i = 0; i < detectBot.Count; i++){
			if (Vector3.Distance (detectBot [i].transform.position, transform.position) < range) {
				enemyTarget = detectBot [i].gameObject;
				range = Vector3.Distance (detectBot [i].transform.position, transform.position);
				if (bot.team_ID == 0) {
					if (detectBot [i].gameObject.GetComponent<Bot> ().ID == master.flag_carriers [0]) {
						return;
					}
				} else if (bot.team_ID == 1) {
					if (detectBot [i].gameObject.GetComponent<Bot> ().ID == master.flag_carriers [1]) {
						return;
					}
				}
			}
		}
	}

	// ------------- DESSINER TAG ------------- //

	void DrawnTag ()
	{
		/// create tag "text" in ground
		if (TEXT != "pixelArt") {
			// create tag
			GameObject tag = new GameObject ();
			tag.name = "Tag : " + TEXT;
			// position tag
			tag.AddComponent<Canvas> ();
			tag.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 5;
			tag.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, Random.Range (240, 300), 0);
			tag.GetComponent<RectTransform> ().sizeDelta = new Vector2 (10, 5);
			tag.transform.position = new Vector3 (transform.position.x + Random.Range (-4.5f, 4.5f), 0.1f, transform.position.z + Random.Range (-4.5f, 4.5f));
			// text tag
			Font ArialFont = (Font)Resources.GetBuiltinResource (typeof(Font), "Arial.ttf");
			tag.AddComponent<Text> ().text = TEXT;
			tag.GetComponent<Text> ().fontStyle = FontStyle.Bold;
			tag.GetComponent<Text> ().alignment = TextAnchor.MiddleCenter;
			tag.GetComponent<Text> ().font = ArialFont;
			tag.GetComponent<Text> ().fontSize = 4;
			tag.GetComponent<Text> ().color = Random.ColorHSV (0, 1f, 1f, 0.9f, 1f, 0.9f, 0.7f, 0.7f);
		}

		/// create tag "pixel art" in ground
		else {
			float randomPosX = transform.position.x + Random.Range (-2.5f, 2.5f);
			float randomPosZ = transform.position.z + Random.Range (-2.5f, 2.5f);
			Color colorSprite = Random.ColorHSV (0, 1f, 1f, 0.9f, 1f, 0.9f, 0.7f, 0.7f);

			// create tag
			GameObject tag1 = new GameObject ();
			tag1.name = "Tag1 : PixelArt";
			// position tag
			tag1.AddComponent<Canvas> ();
			tag1.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag1.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag1.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag1.transform.position = new Vector3 (randomPosX, 0.1f, randomPosZ);
			// sprite tag
			tag1.AddComponent<Image> ().color = colorSprite;

			GameObject tag2 = new GameObject ();
			tag2.name = "Tag2 : PixelArt";
			// position tag
			tag2.AddComponent<Canvas> ();
			tag2.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag2.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag2.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag2.transform.position = new Vector3 (randomPosX, 0.1f, randomPosZ + 2);
			// sprite tag
			tag2.AddComponent<Image> ().color = colorSprite;

			GameObject tag3 = new GameObject ();
			tag3.name = "Tag3 : PixelArt";
			// position tag
			tag3.AddComponent<Canvas> ();
			tag3.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag3.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag3.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag3.transform.position = new Vector3 (randomPosX, 0.1f, randomPosZ - 2);
			// sprite tag
			tag3.AddComponent<Image> ().color = colorSprite;

			GameObject tag4 = new GameObject ();
			tag4.name = "Tag4 : PixelArt";
			// position tag
			tag4.AddComponent<Canvas> ();
			tag4.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag4.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag4.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag4.transform.position = new Vector3 (randomPosX - 1, 0.1f, randomPosZ);
			// sprite tag
			tag4.AddComponent<Image> ().color = colorSprite;

			GameObject tag5 = new GameObject ();
			tag5.name = "Tag5 : PixelArt";
			// position tag
			tag5.AddComponent<Canvas> ();
			tag5.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag5.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag5.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag5.transform.position = new Vector3 (randomPosX + 1, 0.1f, randomPosZ);
			// sprite tag
			tag5.AddComponent<Image> ().color = colorSprite;

			GameObject tag6 = new GameObject ();
			tag6.name = "Tag6 : PixelArt";
			// position tag
			tag6.AddComponent<Canvas> ();
			tag6.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag6.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag6.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag6.transform.position = new Vector3 (randomPosX - 1, 0.1f, randomPosZ + 1);
			// sprite tag
			tag6.AddComponent<Image> ().color = colorSprite;

			GameObject tag7 = new GameObject ();
			tag7.name = "Tag7 : PixelArt";
			// position tag
			tag7.AddComponent<Canvas> ();
			tag7.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag7.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag7.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag7.transform.position = new Vector3 (randomPosX + 1, 0.1f, randomPosZ + 2);
			// sprite tag
			tag7.AddComponent<Image> ().color = colorSprite;

			GameObject tag8 = new GameObject ();
			tag8.name = "Tag8 : PixelArt";
			// position tag
			tag8.AddComponent<Canvas> ();
			tag8.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag8.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag8.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag8.transform.position = new Vector3 (randomPosX - 1, 0.1f, randomPosZ - 1);
			// sprite tag
			tag8.AddComponent<Image> ().color = colorSprite;

			GameObject tag9 = new GameObject ();
			tag9.name = "Tag9 : PixelArt";
			// position tag
			tag9.AddComponent<Canvas> ();
			tag9.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag9.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag9.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag9.transform.position = new Vector3 (randomPosX - 1, 0.1f, randomPosZ - 2);
			// sprite tag
			tag9.AddComponent<Image> ().color = colorSprite;

			GameObject tag10 = new GameObject ();
			tag10.name = "Tag10 : PixelArt";
			// position tag
			tag10.AddComponent<Canvas> ();
			tag10.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag10.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag10.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag10.transform.position = new Vector3 (randomPosX - 2, 0.1f, randomPosZ + 1);
			// sprite tag
			tag10.AddComponent<Image> ().color = colorSprite;

			GameObject tag11 = new GameObject ();
			tag11.name = "Tag11 : PixelArt";
			// position tag
			tag11.AddComponent<Canvas> ();
			tag11.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag11.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag11.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag11.transform.position = new Vector3 (randomPosX - 2, 0.1f, randomPosZ - 1);
			// sprite tag
			tag11.AddComponent<Image> ().color = colorSprite;

			GameObject tag12 = new GameObject ();
			tag12.name = "Tag12 : PixelArt";
			// position tag
			tag12.AddComponent<Canvas> ();
			tag12.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag12.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag12.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag12.transform.position = new Vector3 (randomPosX + 1, 0.1f, randomPosZ + 1);
			// sprite tag
			tag12.AddComponent<Image> ().color = colorSprite;

			GameObject tag13 = new GameObject ();
			tag13.name = "Tag13 : PixelArt";
			// position tag
			tag13.AddComponent<Canvas> ();
			tag13.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag13.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag13.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag13.transform.position = new Vector3 (randomPosX - 1, 0.1f, randomPosZ + 2);
			// sprite tag
			tag13.AddComponent<Image> ().color = colorSprite;

			GameObject tag14 = new GameObject ();
			tag14.name = "Tag14 : PixelArt";
			// position tag
			tag14.AddComponent<Canvas> ();
			tag14.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag14.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag14.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag14.transform.position = new Vector3 (randomPosX + 1, 0.1f, randomPosZ + 3);
			// sprite tag
			tag14.AddComponent<Image> ().color = colorSprite;

			GameObject tag15 = new GameObject ();
			tag15.name = "Tag15 : PixelArt";
			// position tag
			tag15.AddComponent<Canvas> ();
			tag15.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag15.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag15.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag15.transform.position = new Vector3 (randomPosX + 1, 0.1f, randomPosZ - 1);
			// sprite tag
			tag15.AddComponent<Image> ().color = colorSprite;

			GameObject tag16 = new GameObject ();
			tag16.name = "Tag16 : PixelArt";
			// position tag
			tag16.AddComponent<Canvas> ();
			tag16.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag16.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag16.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag16.transform.position = new Vector3 (randomPosX + 1, 0.1f, randomPosZ - 2);
			// sprite tag
			tag16.AddComponent<Image> ().color = colorSprite;

			GameObject tag17 = new GameObject ();
			tag17.name = "Tag17 : PixelArt";
			// position tag
			tag17.AddComponent<Canvas> ();
			tag17.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag17.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag17.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag17.transform.position = new Vector3 (randomPosX + 1, 0.1f, randomPosZ - 3);
			// sprite tag
			tag17.AddComponent<Image> ().color = colorSprite;

			GameObject tag18 = new GameObject ();
			tag18.name = "Tag18 : PixelArt";
			// position tag
			tag18.AddComponent<Canvas> ();
			tag18.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag18.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag18.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag18.transform.position = new Vector3 (randomPosX + 2, 0.1f, randomPosZ + 1);
			// sprite tag
			tag18.AddComponent<Image> ().color = colorSprite;

			GameObject tag19 = new GameObject ();
			tag19.name = "Tag19 : PixelArt";
			// position tag
			tag19.AddComponent<Canvas> ();
			tag19.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag19.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag19.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag19.transform.position = new Vector3 (randomPosX + 2, 0.1f, randomPosZ + 3);
			// sprite tag
			tag19.AddComponent<Image> ().color = colorSprite;

			GameObject tag20 = new GameObject ();
			tag20.name = "Tag20 : PixelArt";
			// position tag
			tag20.AddComponent<Canvas> ();
			tag20.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag20.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag20.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag20.transform.position = new Vector3 (randomPosX + 2, 0.1f, randomPosZ - 1);
			// sprite tag
			tag20.AddComponent<Image> ().color = colorSprite;

			GameObject tag21 = new GameObject ();
			tag21.name = "Tag21 : PixelArt";
			// position tag
			tag21.AddComponent<Canvas> ();
			tag21.AddComponent<CanvasScaler> ().dynamicPixelsPerUnit = 1;
			tag21.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 0, 0);
			tag21.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1,1);
			tag21.transform.position = new Vector3 (randomPosX + 2, 0.1f, randomPosZ - 3);
			// sprite tag
			tag21.AddComponent<Image> ().color = colorSprite;
		}

		TAG = false;
	}

	void TextAboveUpdate ()
	{
		GameObject nameAboveUpdate;
		nameAboveUpdate = GameObject.Find ("NAME : " + transform.parent.GetComponent<Bot> ().team_ID + ID + NAME);
		nameAboveUpdate.transform.position = new Vector3 (transform.position.x, 5f, transform.position.z);
		nameAboveUpdate.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 270, 0);

		GameObject stateAboveUpdate;
		stateAboveUpdate = GameObject.Find ("STATE : " + transform.parent.GetComponent<Bot> ().team_ID + ID + NAME);
		stateAboveUpdate.transform.position = new Vector3 (transform.position.x + 1.5f, 5f, transform.position.z);
		stateAboveUpdate.GetComponent<Text> ().text = "" + currentSTATE;
		stateAboveUpdate.GetComponent<RectTransform> ().eulerAngles = new Vector3 (90, 270, 0);
	}
}