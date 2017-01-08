using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeusVultStrategyAttackFlag : MonoBehaviour {


	// Game master (script qui gère la capture des drapeaux, le respawn des bots et le score)
	GameMaster master;
	Team team;


	// go qui possède les components du bot
	GameObject bot_object;
	// script de base contenant ID, team_ID du bot
	Bot bot;
	// components du game object de base du bot
	public UnityEngine.AI.NavMeshAgent agent;
	Collider collider;
	Renderer renderer;



	TeamBehaviourDeusVult teamController;

	BotBehaviourDeusVult behaviour;

	BotBehaviourDeusVult.BotState state;

	void Start(){
	
		master = FindObjectOfType<GameMaster>();
		team = transform.parent.parent.GetComponent<Team>();

		bot_object = transform.parent.gameObject;
		bot = bot_object.GetComponent<Bot>();
		agent = bot_object.GetComponent<UnityEngine.AI.NavMeshAgent>();
		collider = bot_object.GetComponent<Collider>();
		renderer = bot_object.GetComponent<Renderer>();



		behaviour = GetComponent<BotBehaviourDeusVult> ();
		teamController = transform.parent.parent.GetComponentInChildren<TeamBehaviourDeusVult> ();


	
	}


	public void Act (){
	

		if (behaviour.state == BotBehaviourDeusVult.BotState.IDLE) { 
		
			if (Input.GetMouseButtonDown (0)) {
				Ray r = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (r, out hit)) {
					agent.SetDestination (hit.point);
				}
			}

			if (Input.GetMouseButtonDown (1)) {
				Ray r = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (r, out hit)) {
					Vector3 dir = hit.point - transform.position;
					bot.ShootInDirection (dir);
				}
			}
		
		
		
		
		} 



		//DEFENSE BASE


		else if (behaviour.state == BotBehaviourDeusVult.BotState.DefenseProtectBase) {
		
		
			agent.SetDestination (team.team_base.position);
		
		
		
		}



		//ATTACK FLAG


		else if (behaviour.state == BotBehaviourDeusVult.BotState.AttackGetFlag) {
		
		
			agent.SetDestination (teamController.theirFlagLastKnownPosition);
		
		
		}
	
	
	
	}
		
}
