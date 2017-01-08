using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BotBehaviourDeusVult : MonoBehaviour {


	// liste des states possibles pour ce comportement de bot
	public enum BotState{
		IDLE, 
		AttackGetFlag, 
		AttackHelpFlagGetter, 
		AttackBringFlagBack, 
		AttackShootFlagFollowers, 
		DefenseProtectBase, 
		DefenseShootFragBearer, 
		DefenseBlockEnnemiesSolo, 
		DefenseBlockEnnemiesTeam
	};


	// état actuel du bot
	public BotState state = BotState.IDLE;



	// Game master (script qui gère la capture des drapeaux, le respawn des bots et le score)
	GameMaster master;
	Team team;


	// go qui possède les components du bot
	GameObject bot_object;
	// script de base contenant ID, team_ID du bot
	Bot bot;
	// components du game object de base du bot
	UnityEngine.AI.NavMeshAgent agent;
	Collider collider;
	Renderer renderer;


	TeamBehaviourDeusVult teamController;


	void Start () 
	{
		master = FindObjectOfType<GameMaster>();
		team = transform.parent.parent.GetComponent<Team>();

		bot_object = transform.parent.gameObject;
		bot = bot_object.GetComponent<Bot>();
		agent = bot_object.GetComponent<UnityEngine.AI.NavMeshAgent>();
		collider = bot_object.GetComponent<Collider>();
		renderer = bot_object.GetComponent<Renderer>();


		teamController = transform.parent.parent.GetComponentInChildren<TeamBehaviourDeusVult> ();

		teamController.teamMates.Add (this);


		SwitchState(BotState.IDLE);
	}

	void Update()
	{
		//UpdateState();

		//Update Perception
		ThisIsWhatISee();

		// STRATEGIE ATTACK FLAG

		if (teamController.teamStrategy == TeamBehaviourDeusVult.TeamStrategy.AttackFlag) {
		

			if (state != BotState.DefenseProtectBase && !teamController.weStoleTheirFlag) {
			
				SwitchState (BotState.AttackGetFlag);
			
			}

			int flagCarrier = master.GetFlagCarrierID (team.team_ID);
			if (flagCarrier > -1) { // On a récupéré le drapeau
			
			
			
			
			
			
			}




			GetComponent<DeusVultStrategyAttackFlag> ().Act ();
		
		
		}



	}


	void ThisIsWhatISee(){
	

		// NOTRE DRAPEAU 

		if (bot.CanSeeObject (team.team_flag.gameObject)) { // Je vois le drapeau

			teamController.weSeeOurFlag = true;
			teamController.ourFlagLastKnownPosition = team.team_flag.transform.position;

			if (!teamController.theOnesWhoSeeOurFlag.Contains (this)) { // Et la team ne le sait pas encore


				teamController.theOnesWhoSeeOurFlag.Add (this);

			}

		} else if (!teamController.theOnesWhoSeeOurFlag.Contains (this)) { // Je ne vois pas le drapeau


			teamController.theOnesWhoSeeOurFlag.Remove (this); // Et la team pense que je le vois

		}
	
	




		// LEUR DRAPEAU 

		if (bot.CanSeeObject (team.enemy_flag.gameObject)) { // Je vois le drapeau

			teamController.weSeeTheirFlag = true;
			teamController.theirFlagLastKnownPosition = team.enemy_flag.transform.position;

			if (!teamController.theOnesWhoSeeTheirFlag.Contains (this)) { // Et la team ne le sait pas encore


				teamController.theOnesWhoSeeTheirFlag.Add (this);

			}

		} else if (!teamController.theOnesWhoSeeTheirFlag.Contains (this)) { // Je ne vois pas le drapeau


			teamController.theOnesWhoSeeTheirFlag.Remove (this); // Et la team pense que je le vois

		}




		if (teamController.theOnesWhoSeeOurFlag.Count <= 0)
			teamController.weSeeOurFlag = false;

		if (teamController.theOnesWhoSeeTheirFlag.Count <= 0)
			teamController.weSeeTheirFlag = false;
	
	}




	public void SwitchState(BotState new_state)
	{
		OnExitState();
		state = new_state;
		OnEnterState();
	}

	void OnEnterState()
	{
		switch(state)
		{
		case BotState.IDLE:
			break;

		case BotState.DefenseProtectBase:
			teamController.numberOfProtecting++;
			break;

		case BotState.AttackGetFlag:
			teamController.numberOfAttacking++;
			break;
		}
	}

	void UpdateState()
	{
		switch(state)
		{
		case BotState.IDLE:
			if(Input.GetMouseButtonDown(0))
			{
				Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(r, out hit))
				{
					agent.SetDestination(hit.point);
				}
			}

			if(Input.GetMouseButtonDown(1))
			{
				Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if(Physics.Raycast(r, out hit))
				{
					Vector3 dir = hit.point - transform.position;
					bot.ShootInDirection(dir);
				}
			}

			break;
		}
	}

	void OnExitState()
	{
		switch(state)
		{
		case BotState.IDLE:
			break;

		case BotState.DefenseProtectBase:
			teamController.numberOfProtecting--;
			break;

		case BotState.AttackGetFlag:
			teamController.numberOfAttacking--;
			break;
		}
	}
}
