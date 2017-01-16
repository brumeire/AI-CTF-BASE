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

    public Vector3 flag_Enemy_Pos; // position du drapeau ennemi
    public Vector3 flag_Ally_Pos; // position du drapeau allié

    void Start()
    {
        team = transform.parent.GetComponent<Team>();
        master = FindObjectOfType<GameMaster>();
        flag_Enemy_Pos = team.enemy_flag.transform.position;
        flag_Ally_Pos = team.team_flag.transform.position;
    }
	
	void Update () {

        firstSniper = team.bots[4].ID;
        secondSniper = team.bots[3].ID;
        flagSearch = team.bots[0].ID;
        firstGuard = team.bots[1].ID;
        secondGuard = team.bots[2].ID;

    }
}
