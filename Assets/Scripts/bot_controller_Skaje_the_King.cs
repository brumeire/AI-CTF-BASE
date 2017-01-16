using UnityEngine;
using System.Collections;

public class bot_controller_Skaje_the_King : MonoBehaviour {


	// liste des states possibles pour ce comportement de bot
	public enum BotState{ 
		IDLE,
		GOBACK,
		RAID,
		GOTOENNEMYBASE
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

	// COPAINS

	GameObject bot0;
	GameObject bot1;
	GameObject bot2;
	GameObject bot3;
	GameObject bot4;

	// ENEMY

	GameObject foe0;
	GameObject foe1;
	GameObject foe2;
	GameObject foe3;
	GameObject foe4;

	int enemyLayer;

	Collider[] hit;	


	void Start () 
	{
		master = FindObjectOfType<GameMaster>();
		team = transform.parent.parent.GetComponent<Team>();

		bot_object = transform.parent.gameObject;
		bot = bot_object.GetComponent<Bot>();
		agent = bot_object.GetComponent<UnityEngine.AI.NavMeshAgent>();
		collider = bot_object.GetComponent<Collider>();
		renderer = bot_object.GetComponent<Renderer>();

		SwitchState(BotState.IDLE);

		//COPAINS
		if (team.team_ID == 0) {
			bot0 = master.GetBotFromID (0);
			bot1 = master.GetBotFromID (1);
			bot2 = master.GetBotFromID (2);
			bot3 = master.GetBotFromID (3);
			bot4 = master.GetBotFromID (4);
		}
		if (team.team_ID == 1) {
			bot0 = master.GetBotFromID (5);
			bot1 = master.GetBotFromID (6);
			bot2 = master.GetBotFromID (7);
			bot3 = master.GetBotFromID (8);
			bot4 = master.GetBotFromID (9);
		}

		//ENEMY
		if (team.team_ID == 1) {
			foe0 = master.GetBotFromID (0);
			foe1 = master.GetBotFromID (1);
			foe2 = master.GetBotFromID (2);
			foe3 = master.GetBotFromID (3);
			foe4 = master.GetBotFromID (4);
		}
		if (team.team_ID == 0) {
			foe0 = master.GetBotFromID (5);
			foe1 = master.GetBotFromID (6);
			foe2 = master.GetBotFromID (7);
			foe3 = master.GetBotFromID (8);
			foe4 = master.GetBotFromID (9);
		}

		if (team.enemy_team_ID == 1) {
			enemyLayer = 9;
		} else {
			enemyLayer = 8;
		}
	}

	void Update()
	{
		// Doit servir à update les STATES uniquement
		// si je voit le drapeau
		/*	if (bot.CanSeeObject (master.flag_prefab)) 
		{
			print ("I SEE A FLAG");
			// Je le récupère
			agent.SetDestination (master.flag_prefab.transform.position);
		}*/


		//Si je voit un ennemy je tir
		hit = Physics.OverlapSphere (this.transform.position, 100f);
		int max = hit.Length;
		for (int i = 0; i < max; i++) {
			if (hit [i] != null && hit [i].gameObject.layer == enemyLayer) {
				if (bot.CanSeeObject (hit [i].gameObject)) {
					bot.ShootInDirection (hit [i].transform.position - this.transform.position);
				}
			}
		}
		// fin de shoot
	
	
		UpdateState ();
	
		// Si j'ai le flag enemy

		// Si le flag est la base enemy je retourne le chercher
		if (master.IsTeamFlagHome (team.enemy_team_ID)) {

			SwitchState (BotState.IDLE);
		}
		// si le flag ennemi est tombé je retourne le chercher
		if (master.flag_carriers [team.enemy_team_ID] == -1) {

			agent.SetDestination (team.enemy_flag.transform.position);
		}
		// si mon flag est tombé je retourne le chercher
		if (master.flag_carriers [team.team_ID] == -1 && master.flag_carriers [team.enemy_team_ID] != -1) {

			agent.SetDestination (team.team_flag.transform.position);
		}
		// si je voit un mon drapeau sur la map, raid 
		if (!master.IsTeamFlagHome (team.enemy_team_ID)) {
			SwitchState (BotState.RAID);
		}	
		if (!master.IsTeamFlagHome (team.enemy_team_ID)) {

			SwitchState (BotState.GOBACK);
		}
		if (master.flag_carriers [team.enemy_team_ID] != -1) {


			if (bot.CanSeeObject (master.GetBotFromID (master.flag_carriers [team.team_ID]))) {
				SwitchState (BotState.RAID);
			}
		}
		print (master.flag_carriers [team.team_ID]);
		/*
		// si j'ai le flag ennemi et que mon flag n'est pas à ma base.
		if (master.is_flag_home[team.team_ID] == false && master.flag_carriers [team.enemy_team_ID] != -1)  {

			SwitchState (BotState.GOTOENNEMYBASE);
		}*/
	}


	void SwitchState(BotState new_state)
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
		}
	}

	void UpdateState ()
	{
		switch (state) {
		case BotState.IDLE:

			// go to other team camp	
			if (master.is_flag_home [team.enemy_team_ID]) {
				agent.SetDestination (team.enemy_base.position);
			} 
			break;
		case BotState.RAID:
			// ** Go on the ennemy team ** //
			//FAIRE PLUS DE STATE 
		
			
			agent.SetDestination (master.GetBotFromID (master.flag_carriers [team.enemy_team_ID]).transform.position);
			bot.ShootInDirection (-bot.transform.position - (master.GetBotFromID (master.flag_carriers [team.team_ID]).transform.position));
		/*else {
					
					agent.SetDestination (team.enemy_base.position); 
				}*/

		//agent.SetDestination (team.team_base.position);
			

		break;
	

		case BotState.GOTOENNEMYBASE:

			agent.SetDestination (team.enemy_base.position);
			break;
		case BotState.GOBACK:
			//** Go back to base
			print ("BOBACK");
				agent.SetDestination (team.team_base.position);

			break;
		}
	}

	void OnExitState()
	{
		switch(state)
		{
		case BotState.IDLE:
			break;
		}
	}
}

/* Rap du panda
 * 
 *Hey J'suis l'panda
 *J'suis le grand Papa du pera
 *Avec mon poto el pinguino
 *on va t'apprendre à manier les mots 
 */
