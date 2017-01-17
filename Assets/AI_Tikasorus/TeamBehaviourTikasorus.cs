using UnityEngine;
using System.Collections;

public class TeamBehaviourTikasorus : MonoBehaviour {

    Team team;
    GameMaster master;

    public int firstSniper;
    public int secondSniper;
    public int flagSearch;
    public int firstGuard;
    public int secondGuard;

    void Start()
    {
        team = transform.parent.GetComponent<Team>();
        master = FindObjectOfType<GameMaster>();
    }
	
	void Update () {

        firstSniper = team.bots[4].ID;
        secondSniper = team.bots[3].ID;
        flagSearch = team.bots[0].ID;
        firstGuard = team.bots[1].ID;
        secondGuard = team.bots[2].ID;


        /*foreach (Bot myBot in team.bots) // On parcours le tableau des bots de l'équipe
        {
            /*if (master.GetFlagCarrierID(team.team_ID) != myBot.ID) // Pour tous ceux qui ne portent pas le drapeau
            {
               
            }
    }*/
    }
}
