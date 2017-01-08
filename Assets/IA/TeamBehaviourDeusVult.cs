using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TeamBehaviourDeusVult : MonoBehaviour
{

	Team team;

	public enum TeamStrategy{ 
		Defense, 
		AttackFlag, 
		Alternative, 
		Mixed
	};


	public TeamStrategy teamStrategy = TeamStrategy.AttackFlag;


	public List<BotBehaviourDeusVult> teamMates = new List<BotBehaviourDeusVult> ();


	public GameObject flagDefenser;



	public bool weStoleTheirFlag = false;


	public bool weSeeOurFlag = false;
	public Vector3 ourFlagLastKnownPosition;
	public List<BotBehaviourDeusVult> theOnesWhoSeeOurFlag = new List<BotBehaviourDeusVult> ();



	public bool weSeeTheirFlag = false;
	public Vector3 theirFlagLastKnownPosition;
	public List<BotBehaviourDeusVult> theOnesWhoSeeTheirFlag = new List<BotBehaviourDeusVult> ();



	// Stats de l'état du groupe
	public int numberOfProtecting = 0;

	public int numberOfAttacking = 0;



	void Start ()
	{
		team = transform.parent.GetComponent<Team> ();

		ourFlagLastKnownPosition = team.team_base.position;
		theirFlagLastKnownPosition = team.enemy_base.position;




	}

	void Update ()
	{

		// STRATEGIE DE BASE

		if (teamStrategy == TeamStrategy.AttackFlag) {
		
			if (!flagDefenser) {
			
				GetNewFlagDefenser ();			
			
			
			}
		
		
		
		}


	}


	public void GetNewFlagDefenser(){
	
	
		float distance = float.MaxValue;
		BotBehaviourDeusVult closer = teamMates[0];


		foreach (BotBehaviourDeusVult bot in teamMates) {
		

			float newDist = Vector3.Distance (bot.transform.position, team.team_flag.transform.position);

			if ( newDist < distance) {
			
				distance = newDist;
				closer = bot;
			
			}
		
		
		
		}

		closer.SwitchState (BotBehaviourDeusVult.BotState.DefenseProtectBase);

		flagDefenser = closer.gameObject;


	
	
	}




	public void BroughtBackOurFlag(){
	
		ourFlagLastKnownPosition = team.team_base.position;


	
	}



	public void BroughtBackTheirFlag(){
	
		theirFlagLastKnownPosition = team.enemy_base.position;	
	
	
	}


}
