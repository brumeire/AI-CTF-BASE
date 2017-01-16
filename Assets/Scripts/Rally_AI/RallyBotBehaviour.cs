using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RallyBotBehaviour : MonoBehaviour
{
	// liste des states possibles pour ce comportement de bot
	public enum RallyBotState
	{
		GETFLAG, BACKTOBASE, BODYGUARD, REVENGE, DEFENSE
	}

	// état actuel du bot
	public RallyBotState state = RallyBotState.GETFLAG;


	// Game master (script qui gère la capture des drapeaux, le respawn des bots et le score)
	GameMaster master;
	Team team;
	RallyTeam rallyteam;


	// go qui possède les components du bot
	GameObject bot_object;
	// script de base contenant ID, team_ID du bot
	Bot bot;
	// components du game object de base du bot
	UnityEngine.AI.NavMeshAgent agent;
	Collider collider;
	Renderer renderer;

    Vector3 randVec;

	void Start ()
	{
		master = FindObjectOfType<GameMaster> ();
		team = transform.parent.parent.GetComponent<Team> ();
		rallyteam = team.gameObject.GetComponentInChildren<RallyTeam>();

		bot_object = transform.parent.gameObject;
		bot = bot_object.GetComponent<Bot> ();
		agent = bot_object.GetComponent<UnityEngine.AI.NavMeshAgent> ();
		collider = bot_object.GetComponent<Collider> ();
		renderer = bot_object.GetComponent<Renderer> ();

		SwitchState (RallyBotState.GETFLAG);
        rallyteam.availableTroops.Add(this);
        rallyteam.allTroops.Add(this);
	}

	void Update ()
	{
		StateAutoChange();
		UpdateState();
        TryShoot();
        Feedbacks();
        List<GameObject> flags;
        if (FlagsInSight(out flags))
        {
            foreach(GameObject go in flags)
            {
                rallyteam.AddInfo(go);
            }
        }
    }

    private void SetAvailable()
    {
        if (state != RallyBotState.BACKTOBASE)
        {
            rallyteam.availableTroops.Add(this);
        }
    }

    #region State Machine Functions
    void SwitchState (RallyBotState new_state)
	{
		OnExitState ();
		state = new_state;
		OnEnterState ();
	}

	void OnEnterState ()
	{
		switch (state)
        {
		    case RallyBotState.BACKTOBASE:
			    rallyteam.availableTroops.Remove(this);
			    break;

            case RallyBotState.DEFENSE:
                randVec = new Vector3((float)Random.value, 0f, (float)Random.value)*50.0f;
                Debug.Log(randVec);
                break;
		}
	}

	void UpdateState ()
	{
		switch (state) 
		{
		    case RallyBotState.BODYGUARD:
                if (Vector3.Distance(rallyteam.quatterback.transform.position, transform.position) > 
                    Vector3.Distance(team.team_base.position, transform.position))
                {
                    agent.SetDestination(team.team_base.position);
                }
                else
                {
                    agent.SetDestination(rallyteam.quatterback.transform.position);
                }
			    break;

		    case RallyBotState.BACKTOBASE:
			    agent.SetDestination(team.team_base.position);
			    break;

		    case RallyBotState.GETFLAG:
			    agent.SetDestination(rallyteam.enemyFlagPos);
                if (transform.position == rallyteam.enemyFlagPos)
                {
                    rallyteam.ForceOutdateEnemyFlagPos();
                }
			    break;

            case RallyBotState.REVENGE:
                agent.SetDestination(rallyteam.allyFlagPos);
                if (bot.CanSeeObject(team.team_base.gameObject))
                {
                    rallyteam.ForceOutdateAllyFlagPos();
                }
                break;

            case RallyBotState.DEFENSE:
                agent.SetDestination(team.team_base.position + new Vector3( 1, 0, 1) * (Mathf.Cos(Time.time)));
                break;

        }
	}

	void OnExitState ()
	{
		switch (state)
        {
		    case RallyBotState.BACKTOBASE:
			    rallyteam.availableTroops.Add(this);
			    break;
		}
	}

	void StateAutoChange()
	{
		switch (state)
		{
		case RallyBotState.GETFLAG :
			    if (rallyteam.quatterback != null)
			    {
				    if (rallyteam.quatterback == this.transform.parent.gameObject)
				    {
					    SwitchState(RallyBotState.BACKTOBASE);
				    }
				    else
				    {
					    SwitchState(RallyBotState.BODYGUARD);
				    }
			    }
			    break;

		case RallyBotState.BODYGUARD :
                if (rallyteam.quatterback == null)
			    {
				    SwitchState(RallyBotState.GETFLAG);
			    }
			    break;

		case RallyBotState.BACKTOBASE :
			    if (rallyteam.quatterback != this.transform.parent.gameObject)
			    {
                    if (rallyteam.quatterback == null)
                    {
                        SwitchState(RallyBotState.GETFLAG);
                    }
                    else
                    {
                        SwitchState(RallyBotState.BODYGUARD);
                    }
                }
			    break;

         case RallyBotState.REVENGE :
                if (rallyteam.quatterback == this.transform.parent.gameObject)
                {
                    SwitchState(RallyBotState.BACKTOBASE);
                }
                else if (rallyteam.state != GameState.DISADVANTAGE && rallyteam.state != GameState.TENSE)
                {
                        SwitchState(RallyBotState.GETFLAG);
                }
                break;
		}
	}
    #endregion

    #region Feedbacks

    private void Feedbacks()
    {
        GizmosService.Text(state.ToString() + bot.can_shoot.ToString(), transform.position + Vector3.forward, 0.01f, Color.white);
        GizmosService.Cone(transform.position, transform.forward, Vector3.up, 10, 70);
    }

    #endregion

    #region Shooting and Sight functions

    private void TryShoot()
	{
		List<GameObject> targets;
		GameObject target = null;

		if (EnnemiesInSight(out targets))
		{
            if (rallyteam.mainTarget != null)
            {
                foreach (GameObject go in targets)
                {
                    if (go == rallyteam.mainTarget)
                    {
                        rallyteam.AddInfo(go);
                        target = go;
                        bot.ShootInDirection(target.transform.position - transform.position);
                        break;
                    }
                }
                if (target == null)
                {
                    target = GetClosest(targets);
                    if (target != null)
                    {
                        bot.ShootInDirection(target.transform.position - transform.position);
                    }

                }
            }
            else
            {
                target = GetClosest(targets);
                if (target != null)
                {
                    bot.ShootInDirection(target.transform.position - transform.position);
                }
            }
		}
	}

    private GameObject GetClosest(List<GameObject> list)
    {
        float minDist = float.PositiveInfinity;
        GameObject target = null;
        if (list.Count == 0)
            return null;

        foreach (GameObject go in list)
        {
            if (Vector3.Distance(transform.position, go.transform.position) < minDist)
            {
                minDist = Vector3.Distance(transform.position, go.transform.position);
                target = go;
            }
        }
        return target;
    }

	private bool EnnemiesInSight(out List<GameObject> ennemiesInSight)
	{
		GameObject[] every_bots = GameObject.FindGameObjectsWithTag("Bot");
		ennemiesInSight = new List<GameObject>();

		foreach (GameObject go in every_bots)
		{
			if (go != this.gameObject && go.GetComponent<Bot>().team_ID != bot.team_ID && bot.CanSeeObject(go))
			{
				ennemiesInSight.Add(go);
			}
		}

		if (ennemiesInSight.Count > 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

    private bool FlagsInSight(out List<GameObject> flagsInSight)
    {
        GameObject[] every_flags = GameObject.FindGameObjectsWithTag("Flag");
        flagsInSight = new List<GameObject>();

        foreach (GameObject go in every_flags)
        {
            if (bot.CanSeeObject(go))
            {
                flagsInSight.Add(go);
            }
        }

        if (flagsInSight.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion
}