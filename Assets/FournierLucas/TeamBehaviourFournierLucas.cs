using UnityEngine;
using System.Collections;

public class TeamBehaviourFournierLucas : MonoBehaviour {

	public bool flagNotInOurBase = false;

	float numberSearcher =0;

	bool alreadyACamping = false;

  Team team;

	void Start () 
  {
    team = transform.parent.GetComponent<Team>();
	}
	
	void Update () {
		if (team.bots [1].team_ID == 0 && FindObjectOfType<GameMaster> ().is_flag_home [0] == false) {
			flagNotInOurBase = true;
		} else if (team.bots [1].team_ID == 1 && FindObjectOfType<GameMaster> ().is_flag_home [1] == false) {
			flagNotInOurBase = true;
		} else {
			flagNotInOurBase = false;
		}


		//in progress
		alreadyACamping = false;
		/*
		if (!alreadyACamping) {
			for (int i = 0; i < team.bots.Length; i++) {
				if (team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement == "Go to the camping" || team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement == "Camping") {
					alreadyACamping = true;
				}
				if (team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Go to base" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Protect" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Go to enemies") {
					foreach (Bot bot in team.bots) {
						bot.GetComponentInChildren<FournierLucasBehaviour> ().CampingChange (i);
					}

					team.SendMessageToTeam ("CampingChange");
					alreadyACamping = true;
				}
			}
		}
		*/


		if (team.enemy_flag.transform.position != team.enemy_base.position) {
			team.SendMessageToTeam ("allProtection");
		}


		if ( flagNotInOurBase ){
			numberSearcher =0;
			if (numberSearcher < 2) {
				for (int i = 0; i < team.bots.Length; i++) {
					if (numberSearcher < 2 && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Camping" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Go to base" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Battle" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Protect" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Go to enemies") {
						if (team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Take their flag" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Save our flag" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Search" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "SearchBegin") {
							team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement = "SearchBegin";
							numberSearcher++;
						}
					}
				}
			}
			if (numberSearcher < 2) {
				for (int i = 0; i < team.bots.Length; i++) {
					if (numberSearcher < 2 && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Camping" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Go to base" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Battle"  && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Search"  && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Go to enemies" ) {
						if (team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Take their flag" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Save our flag" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "SearchBegin") {
							team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement = "SearchBegin";
							numberSearcher++;
						}
					}
				}
			}
			if (numberSearcher == 0) {
				for (int i = 0; i < team.bots.Length; i++) {
					if (numberSearcher == 0 && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Camping" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Go to base"  && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Search" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "SearchBegin" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Go to enemies") {
						if (team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Take their flag" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Save our flag"  && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Save our flag") {
							team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement = "SearchBegin";
							numberSearcher++;
						}
					}
				}
			}
		}else if (!flagNotInOurBase) {
			for (int i = 0; i < team.bots.Length; i++) {
				if (team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Camping" &&  team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Go to base" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Search" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "SearchBegin" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Go to enemies") {
					if (team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Take their flag" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Save our flag" && team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement != "Save our flag") {
						team.bots [i].GetComponentInChildren<FournierLucasBehaviour> ().comportement = "Flag";
					}
				}
			}
		}


	}
		
}
