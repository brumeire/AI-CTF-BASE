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
		DefensePlantATent, 
		DefenseShootFlagBearer, 
		DefenseBlockEnnemiesSolo, 
		DefenseBlockEnnemiesTeam
	};


	// état actuel du bot
	public BotState state = BotState.IDLE;



	// Game master (script qui gère la capture des drapeaux, le respawn des bots et le score)
	GameMaster master;
	Team team;

	int teamId;



	// go qui possède les components du bot
	GameObject bot_object;
	// script de base contenant ID, team_ID du bot
	public Bot bot;
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
		teamId = bot.team_ID;

		teamController = transform.parent.parent.GetComponentInChildren<TeamBehaviourDeusVult> ();

		teamController.teamMates.Add (this);


		SwitchState(BotState.IDLE);

		teamController.flagCarrier = this;

	}

	void Update()
	{
		//UpdateState();

		//Update Perception
		ThisIsWhatISee();

		// STRATEGIE ATTACK FLAG

		if (teamController.teamStrategy == TeamBehaviourDeusVult.TeamStrategy.AttackFlag) {
		


			if ((state != BotState.DefenseProtectBase || state != BotState.DefensePlantATent) && master.GetFlagCarrierID ((teamId + 1) % 2) == bot.ID) {

				if (state == BotState.DefenseProtectBase) {

					teamController.flagDefenser = teamController.GetCloser (team.team_base.position);

					teamController.flagDefenser.GetComponent<BotBehaviourDeusVult> ().SwitchState (BotBehaviourDeusVult.BotState.DefenseProtectBase);



				}


				else if (state == BotState.DefensePlantATent) {


					Vector3 posCamping = Vector3.zero;

					if (teamId == 1)
						posCamping = teamController.posCamping1;
					else
						posCamping = teamController.posCamping0;


					teamController.sideCamper = teamController.GetCloser (posCamping);

					teamController.sideCamper.GetComponent<BotBehaviourDeusVult> ().SwitchState (BotBehaviourDeusVult.BotState.DefensePlantATent);



				}



				SwitchState (BotState.AttackBringFlagBack);




			}







			if (state != BotState.DefenseProtectBase && state != BotState.DefensePlantATent) {
			


				if (!teamController.weStoleTheirFlag)
					SwitchState (BotState.AttackGetFlag);




				else if (teamController.weStoleTheirFlag) {


					if (master.GetFlagCarrierID ((teamId + 1) % 2) == bot.ID) {

						teamController.flagCarrier = this;

						SwitchState (BotState.AttackBringFlagBack);

					}

					else
						SwitchState (BotState.AttackHelpFlagGetter);

					Debug.Log (master.GetFlagCarrierID ((teamId + 1) % 2) + ", " + bot.ID +", " + teamId);

				}
					
			
			}




			GetComponent<DeusVultStrategyAttackFlag> ().Act ();
		
		
		}





		else if (teamController.teamStrategy == TeamBehaviourDeusVult.TeamStrategy.Defense) {


			if ((state != BotState.DefenseProtectBase || state != BotState.DefensePlantATent) && master.GetFlagCarrierID ((teamId + 1) % 2) == bot.ID) {
			
				if (state == BotState.DefenseProtectBase) {
				
					teamController.flagDefenser = teamController.GetCloser (team.team_base.position);

					teamController.flagDefenser.GetComponent<BotBehaviourDeusVult> ().SwitchState (BotBehaviourDeusVult.BotState.DefenseProtectBase);
				
				
				
				}


				else if (state == BotState.DefensePlantATent) {


					Vector3 posCamping = Vector3.zero;

					if (teamId == 1)
						posCamping = teamController.posCamping1;
					else
						posCamping = teamController.posCamping0;


					teamController.sideCamper = teamController.GetCloser (posCamping);

					teamController.sideCamper.GetComponent<BotBehaviourDeusVult> ().SwitchState (BotBehaviourDeusVult.BotState.DefensePlantATent);



				}



				SwitchState (BotState.AttackBringFlagBack);
			
			
			
			
			}


			if (state != BotState.DefenseProtectBase && state != BotState.DefensePlantATent) {



				if (!teamController.weStoleTheirFlag)
					SwitchState (BotState.AttackGetFlag);




				else if (teamController.weStoleTheirFlag) {


					if (master.GetFlagCarrierID ((teamId + 1) % 2) == bot.ID) {

						teamController.flagCarrier = this;

						SwitchState (BotState.AttackBringFlagBack);

					}

					else
						SwitchState (BotState.AttackHelpFlagGetter);

					Debug.Log (master.GetFlagCarrierID ((teamId + 1) % 2) + ", " + bot.ID +", " + teamId);

				}


			}




			GetComponent<DeusVultStrategyDefense> ().Act ();


		}









		//GizmosService.Cone(bot_object.transform.position, bot_object.transform.forward, Vector3.up, 10, 70);

	}


	void ThisIsWhatISee(){
	

		// NOTRE DRAPEAU 

		if (bot.CanSeeObject (team.team_flag.gameObject)) { // Je vois le drapeau

			teamController.weSeeOurFlag = true;
			teamController.ourFlagLastKnownPosition = team.team_flag.transform.position;

			if (!teamController.theOnesWhoSeeOurFlag.Contains (this)) { // Et la team ne le sait pas encore


				teamController.theOnesWhoSeeOurFlag.Add (this);

			}

		} else {

			if (teamController.theOnesWhoSeeOurFlag.Contains (this)) { // Je ne vois pas le drapeau


				teamController.theOnesWhoSeeOurFlag.Remove (this); // Et la team pense que je le vois

			}







			if (!IsFlagAtPos (teamController.ourFlagLastKnownPosition, teamId) && master.GetFlagCarrierID ((teamId + 1) % 2) == -1 && teamController.theOnesWhoSeeOurFlag.Count == 0 && master.IsTeamFlagHome(teamId))
				teamController.BroughtBackOurFlag ();
			
			else if (!IsFlagAtPos (teamController.ourFlagLastKnownPosition, teamId) && master.GetFlagCarrierID ((teamId + 1) % 2) > -1 && teamController.theOnesWhoSeeOurFlag.Count == 0)
				teamController.ourFlagLastKnownPosition = team.enemy_base.position;

	
	
		}



		// LEUR DRAPEAU 

		if (bot.CanSeeObject (team.enemy_flag.gameObject)) { // Je vois le drapeau

			teamController.weSeeTheirFlag = true;
			teamController.theirFlagLastKnownPosition = team.enemy_flag.transform.position;

			if (!teamController.theOnesWhoSeeTheirFlag.Contains (this)) { // Et la team ne le sait pas encore


				teamController.theOnesWhoSeeTheirFlag.Add (this);

			}

		} else {


			if (teamController.theOnesWhoSeeTheirFlag.Contains (this)) { // Je ne vois pas le drapeau


				teamController.theOnesWhoSeeTheirFlag.Remove (this); // Et la team pense que je le vois

			}









			if (!IsFlagAtPos (teamController.theirFlagLastKnownPosition, (teamId + 1) % 2) && teamController.theOnesWhoSeeTheirFlag.Count == 0 && master.IsTeamFlagHome((teamId + 1) % 2))
				teamController.BroughtBackTheirFlag ();



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

		case BotState.DefensePlantATent:
			teamController.numberOfProtecting++;
			break;

		case BotState.AttackGetFlag:
			teamController.numberOfAttacking++;
			break;

		case BotState.AttackBringFlagBack:
			teamController.numberOfAttacking++;
			break;

		case BotState.AttackHelpFlagGetter:
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

		case BotState.DefensePlantATent:
			teamController.numberOfProtecting--;
			break;

		case BotState.AttackGetFlag:
			teamController.numberOfAttacking--;
			break;

		case BotState.AttackBringFlagBack:
			teamController.numberOfAttacking--;
			break;

		case BotState.AttackHelpFlagGetter:
			teamController.numberOfAttacking--;
			break;
		}
	}


	bool IsFlagAtPos(Vector3 pos, int teamID)
	{
		Vector3 dir_to_obj = pos - transform.position;
		Ray r = new Ray(transform.position, dir_to_obj);
		RaycastHit hit;
		// si on ne cherche pas spécifiquement un drapeau, on ignore celui-ci dans le raycast
		int layer_mask = Physics.DefaultRaycastLayers;

		if(Physics.Raycast(r, out hit, Vector3.Distance(pos, transform.position), layer_mask, QueryTriggerInteraction.Ignore) || Vector3.Angle(transform.forward, dir_to_obj) > 70)
		{
			
			return true; // on a la vision et le drapeau est là ou on ne peut pas voir le drapeau

		}

		return false;
	}


}
