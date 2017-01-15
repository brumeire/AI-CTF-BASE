using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum GameState
{
    NEUTRAL, ADVANTAGE, DISADVANTAGE, TENSE
}

public class RallyTeam : MonoBehaviour
{
    public GameState state = GameState.NEUTRAL;

	private GameMaster master;
	private Team team;

	[HideInInspector]
	public GameObject quatterback;

	[HideInInspector]
	public GameObject mainTarget;

    [HideInInspector]
    public Vector3 allyFlagPos;

    [HideInInspector]
    public Vector3 enemyFlagPos;

    [HideInInspector]
    public Vector3 mainTargetPos;

    [HideInInspector]
    public List<RallyBotBehaviour> bodyguards = new List<RallyBotBehaviour>();


	void Start ()
	{
		mainTarget = null;
		quatterback = null;
		master = FindObjectOfType<GameMaster>();
		team = transform.parent.GetComponent<Team> ();
        allyFlagPos = team.enemy_base.position;
        enemyFlagPos = team.enemy_base.position;
	}

	void Update ()
	{
		SetQuatterBack();
        SetMainTarget();
        SetGameState();
        UpdateGameState();
        master.DEBUG_TEXT.text = " ";
    }

    void LateUpdate()
    {
        master.DEBUG_TEXT.text += "\n" + state.ToString();
    }

	private void SetQuatterBack()
	{
		int index = master.GetFlagCarrierID(team.enemy_team_ID);
		if (index != -1)
		{
			quatterback = master.GetBotFromID(index);
		}
		else
		{
			quatterback = null;
		}
	}

	private void SetMainTarget()
	{
		int index = master.GetFlagCarrierID(team.team_ID);
		if (index != -1)
		{
			mainTarget = master.GetBotFromID(index);
		}
		else
		{
			mainTarget = null;
		}
	}

    private void SetRevenge(float percentage)
    {
        int numberOfGuards = bodyguards.Count;
        for (int i = 0; i < numberOfGuards * percentage; ++i)
        {
            bodyguards[i].state = RallyBotBehaviour.RallyBotState.REVENGE;
        }
    }

    #region State Machine Functions
    private void SetGameState()
    {
        if (quatterback == null)
        {
            if (master.IsTeamFlagHome(team.team_ID))
            {
                SwitchState(GameState.NEUTRAL);
            }
            else
            {
                SwitchState(GameState.DISADVANTAGE);
            }
        }
        else
        {
            if (master.IsTeamFlagHome(team.team_ID))
            {
                SwitchState(GameState.ADVANTAGE);
            }
            else
            {
                SwitchState(GameState.TENSE);
            }
        }
    }

    void SwitchState(GameState new_state)
    {
        OnExitState();
        state = new_state;
        OnEnterState();
    }

    void OnEnterState()
    {
        switch (state)
        {
            case GameState.TENSE:
                SetRevenge(0.5f);
                break;

            case GameState.DISADVANTAGE:
                SetRevenge(0.75f);
                break;
        }
    }

    void UpdateGameState()
    {
    }

    void OnExitState()
    {
    }
    #endregion

    #region Information Management

    private float outDatingTime = 10.0f;

    public void AddInfo(GameObject go)
    {
        if (go.tag == "Flag")
        {
            if (go.GetComponent<Collider>() == team.team_flag)
            {
                StopCoroutine("OutdateAllyFlagPos");
                allyFlagPos = go.transform.position;
                StartCoroutine("OutdateAllyFlagPos");
            }
            else
            {
                StopCoroutine("OutdateEnemyFlagPos");
                enemyFlagPos = go.transform.position;
                StartCoroutine("OutdateEnemyFlagPos");
            }
        }
        else if (go == mainTarget)
        {
            mainTargetPos = go.transform.position;
        }
    }
    
    private IEnumerator OutdateAllyFlagPos ()
    {
        float timer = 0f;
        while (timer < outDatingTime)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
        }
        allyFlagPos = team.team_base.position; 
    }

    private IEnumerator OutdateEnemyFlagPos()
    {
        float timer = 0f;
        while (timer < outDatingTime)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
        }
        enemyFlagPos = team.enemy_base.position;
    }

    public void ForceOutdateAllyFlagPos()
    {
        StopCoroutine("OutdateAllyFlagPos");
        if (allyFlagPos == team.enemy_base.position)
        {
            allyFlagPos = team.team_base.position;
        }
        else
        {
            allyFlagPos = team.enemy_base.position;
        }
    }

    public void ForceOutdateEnemyFlagPos()
    {
        StopCoroutine("OutdateEnemyFlagPos");
        if (enemyFlagPos == team.enemy_base.position)
        {
            enemyFlagPos = team.team_base.position;
        }
        else
        {
            enemyFlagPos = team.enemy_base.position;
        }
    }

    #endregion
}