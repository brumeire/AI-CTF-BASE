using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DeusVultStrategyDefense : MonoBehaviour {


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

	bool destinationReached = false;


	Coroutine shooter;


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


		agent.Resume ();

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

			Vector3 target = Vector3.zero;

			if (team.team_ID == 1)
				target = new Vector3 (-32, 2.083333f, 7);

			else
				target = new Vector3 (32, 2.083333f, -7);
			//agent.SetDestination (team.team_base.position);

			if (transform.position == target)
				destinationReached = true;

			if (Vector3.Distance (transform.position, target) > 15)
				destinationReached = false;


			if (!destinationReached)
				agent.SetDestination (target);
			else if (Vector3.Angle (transform.forward, team.team_base.position - transform.position) > 10) {
				agent.Stop ();

				float angle = Vector3.Angle (transform.forward, team.team_base.position - transform.position);

				bot_object.transform.Rotate (Vector3.up, Mathf.Clamp (360 * Time.deltaTime, 0, angle));
			}



		}



		//ATTACK FLAG


		else if (behaviour.state == BotBehaviourDeusVult.BotState.AttackGetFlag) {


			agent.SetDestination (teamController.theirFlagLastKnownPosition);


		}




		// BRING BACK FLAG


		else if (behaviour.state == BotBehaviourDeusVult.BotState.AttackHelpFlagGetter) {


			if (!teamController.theyStoleOurFlag) {

				if (Vector3.Distance (transform.position, teamController.flagCarrier.transform.position) > 15 && Vector3.Distance (transform.position, teamController.flagCarrier.transform.position) < 30) {


					agent.Stop ();
					transform.Rotate (Vector3.up, 180 * Time.deltaTime);


				} else if(Vector3.Distance (transform.position, teamController.flagCarrier.transform.position) >= 30) {


					agent.SetDestination (teamController.flagCarrier.transform.position);

				}
				else 
					agent.SetDestination (team.team_base.position);




			} else {


				agent.SetDestination (teamController.ourFlagLastKnownPosition);


			}


		} 




		else if (behaviour.state == BotBehaviourDeusVult.BotState.AttackBringFlagBack) {


			Vector3 helpersGlobalPos = Vector3.zero;
			int count = 0;

			foreach (BotBehaviourDeusVult ally in teamController.teamMates) {

				if (ally.state == BotBehaviourDeusVult.BotState.AttackHelpFlagGetter) {

					if (!ally.bot.is_dead) {

						helpersGlobalPos += ally.transform.position;
						count++;

					}
				}

			}

			if (count > 0)
				helpersGlobalPos /= count;

			if (!teamController.theyStoleOurFlag) {


				if (Vector3.Distance (transform.position, team.team_base.position) > 20){

					if (count > 0)
						//agent.SetDestination (helpersGlobalPos - 7 * (helpersGlobalPos - transform.position).normalized);
						agent.SetDestination (helpersGlobalPos);
					else
						agent.SetDestination (team.team_base.position);


				}
				else
					agent.SetDestination (team.team_base.position);




			} else
				agent.SetDestination (team.team_base.position);

		}




















		// SHOOT ON SIGHT


		//WORKING WELL

		/*if (bot.can_shoot && shooter == null) {

			foreach (GameObject ennemy in teamController.ennemies) {

				if (bot.CanSeeObject (ennemy)) {
					shooter = StartCoroutine ("Shoot", ennemy);
					break;
				
				}

			}


		}*/


		// TESTING

		if (bot.can_shoot) {

			List<GameObject> targets = new List<GameObject> ();
			bool seeCarrier = false;

			foreach (GameObject ennemy in teamController.ennemies) {

				if (bot.CanSeeObject (ennemy)) {

					targets.Add (ennemy);

					if (teamController.theyStoleOurFlag && ennemy.GetComponent<Bot> ().ID == master.GetFlagCarrierID (team.team_ID))
						seeCarrier = true;

				}

			}


			foreach (GameObject target in targets) {

				if (seeCarrier && target.GetComponent<Bot> ().ID == master.GetFlagCarrierID (team.team_ID))
					StartCoroutine ("Shoot", target);

				else if (!seeCarrier)
					StartCoroutine ("Shoot", target);

			}
		}



	}



	IEnumerator Shoot(GameObject target){

		Vector3 targetPos = target.transform.position;

		yield return null;

		if (bot.CanSeeObject (target) && bot.can_shoot) {

			Vector3 newTargetPos = target.transform.position;

			Vector3 targetDir = (newTargetPos - targetPos).normalized;


			Vector3 aimedPos = newTargetPos;

			if (newTargetPos != targetPos)
				aimedPos = newTargetPos + targetDir * Time.deltaTime * 26 * Vector3.Distance(transform.position, newTargetPos + targetDir * Time.deltaTime * 5);

			Vector3 shootDir = aimedPos - transform.position;



			Ray ray = new Ray (transform.position, shootDir);

			int layerMask = 0;
			if (team.team_ID == 0)
				layerMask = ~(1 << 9);
			else
				layerMask = ~(1 << 8);

			if (!Physics.SphereCast(ray, 0.7f, Vector3.Distance(transform.position, aimedPos), layerMask) && Vector3.Angle(transform.forward, shootDir) <= 70) {


				bot.ShootInDirection (shootDir);
			}

		}

		shooter = null;


	}

}
