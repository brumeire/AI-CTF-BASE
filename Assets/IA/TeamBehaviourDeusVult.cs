using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeamBehaviourDeusVult : MonoBehaviour
{

	Team team;

	int teamID;

	public enum TeamStrategy{ 
		Defense, 
		AttackFlag, 
		Alternative, 
		Mixed
	};


	public TeamStrategy teamStrategy = TeamStrategy.Defense;


	public List<BotBehaviourDeusVult> teamMates = new List<BotBehaviourDeusVult> ();


	public List<GameObject> ennemies = new List<GameObject> ();



	public GameObject flagDefenser;

	public GameObject sideCamper;



	public bool weStoleTheirFlag = false;

	public bool theyStoleOurFlag = false;


	public bool weSeeOurFlag = false;
	public Vector3 ourFlagLastKnownPosition;
	public List<BotBehaviourDeusVult> theOnesWhoSeeOurFlag = new List<BotBehaviourDeusVult> ();



	public bool weSeeTheirFlag = false;
	public Vector3 theirFlagLastKnownPosition;
	public List<BotBehaviourDeusVult> theOnesWhoSeeTheirFlag = new List<BotBehaviourDeusVult> ();



	// Stats de l'état du groupe
	public int numberOfProtecting = 0;

	public int numberOfAttacking = 0;


	GameMaster master;

	int pointsTeam = 0;
	int pointsEnnemy = 0;


	public BotBehaviourDeusVult flagCarrier;


	public Vector3 posCamping0;

	public Vector3 posCamping1;


	void Start ()
	{

		posCamping1 = new Vector3 (18, 0, -21);

		posCamping0 = new Vector3 (-18, 0, 21);



		teamStrategy = TeamStrategy.Defense;

		master = FindObjectOfType<GameMaster>();
		team = transform.parent.GetComponent<Team> ();

		teamID = team.team_ID;






		ourFlagLastKnownPosition = team.team_base.position;
		theirFlagLastKnownPosition = team.enemy_base.position;




		foreach (GameObject go in GameObject.FindGameObjectsWithTag("Bot")) {
		
			if (go.GetComponent<Bot> ().team_ID != teamID)
				ennemies.Add (go);

		
		}


	}

	void Update ()
	{

		// STRATEGIE DE BASE

		if (teamStrategy == TeamStrategy.AttackFlag) {
		
			if (!flagDefenser) {
			
				flagDefenser = GetCloser (team.team_base.position);

				flagDefenser.GetComponent<BotBehaviourDeusVult> ().SwitchState (BotBehaviourDeusVult.BotState.DefenseProtectBase);
			
			
			}

		
		
		
		}



		// STRATEGIE DEFENSE

		else if (teamStrategy == TeamStrategy.Defense) {

			if (!flagDefenser) {

				flagDefenser = GetCloser (team.team_base.position);

				flagDefenser.GetComponent<BotBehaviourDeusVult> ().SwitchState (BotBehaviourDeusVult.BotState.DefenseProtectBase);


			}


			if (!sideCamper) {
			
				Vector3 posCamping = Vector3.zero;

				if (teamID == 1)
					posCamping = posCamping1;
				else
					posCamping = posCamping0;


				sideCamper = GetCloser (posCamping);

				sideCamper.GetComponent<BotBehaviourDeusVult> ().SwitchState (BotBehaviourDeusVult.BotState.DefensePlantATent);


			}



		}





		//MAJ Variables

		int flagCarrier = master.GetFlagCarrierID ((teamID + 1) % 2);
		if (flagCarrier > -1) { // On a récupéré le drapeau


			weStoleTheirFlag = true;

			theirFlagLastKnownPosition = master.GetBotFromID (flagCarrier).transform.position;

		} 

		else {
		
		
			weStoleTheirFlag = false;
		
		
		}





		if (master.is_flag_home [teamID]) {
		

			theyStoleOurFlag = false;
		
		
		}




		else {


			theyStoleOurFlag = true;


		}




		if (master.GetScore (teamID) > pointsTeam) {
		
			BroughtBackTheirFlag ();
			pointsTeam++;
		
		
		}

		if (master.GetScore ((teamID + 1) % 2) > pointsEnnemy) {
		
			BroughtBackOurFlag ();
			pointsEnnemy++;
		
		
		}


		if (master.GetFlagCarrierID((teamID + 1) % 2) == -1)
			BroughtBackOurFlag ();

		if (master.GetFlagCarrierID(teamID) == -1)
			BroughtBackTheirFlag ();



	}


	/*public void GetNewFlagDefenser(){
	
	
		float distance = float.MaxValue;
		BotBehaviourDeusVult closer = teamMates[0];


		foreach (BotBehaviourDeusVult bot in teamMates) {
		

			float newDist = Vector3.Distance (bot.transform.position, team.team_flag.transform.position);

			if (newDist < distance) {
			
				distance = newDist;
				closer = bot;
			
			}
		
		
		
		}

		closer.SwitchState (BotBehaviourDeusVult.BotState.DefenseProtectBase);

		flagDefenser = closer.gameObject;


	
	
	}*/

	public GameObject GetCloser(Vector3 pos){


		float distance = float.MaxValue;
		BotBehaviourDeusVult closer = teamMates[0];


		foreach (BotBehaviourDeusVult bot in teamMates) {

			if (bot.state != BotBehaviourDeusVult.BotState.DefenseProtectBase && bot.state != BotBehaviourDeusVult.BotState.DefensePlantATent) {

				float newDist = Vector3.Distance (bot.transform.position, pos);

				if (newDist < distance) {

					distance = newDist;
					closer = bot;

				}
			
			}

		}

			return closer.gameObject;


	}




	public void BroughtBackOurFlag(){
	
		ourFlagLastKnownPosition = team.team_base.position;


	
	}



	public void BroughtBackTheirFlag(){
	
		theirFlagLastKnownPosition = team.enemy_base.position;	
	
	
	}


}
