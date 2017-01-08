using UnityEngine;
using System.Collections;

public class TeamBehaviourBaseLesur : MonoBehaviour {

  Team team;

	void Start () 
  {
    team = transform.parent.GetComponent<Team>();
    Invoke("GiveOrder", 1.0f);
	}

  void GiveOrder()
  {
    team.bots[0].GetComponentInChildren<BotBehaviourBaseLesur>().SwitchState(BotBehaviourBaseLesur.BotState.FIND_ENEMY_FLAG);
  }
}
