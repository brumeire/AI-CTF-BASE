﻿using UnityEngine;
using System.Collections;

public class BotBehaviourBaseExample : MonoBehaviour {


  // liste des states possibles pour ce comportement de bot
  public enum BotState{ 
    IDLE,
    FIND_ENEMY_FLAG
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
	}

  void Update()
  {
    UpdateState();
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

      case BotState.FIND_ENEMY_FLAG:

      break;
    }
  }

  void UpdateState()
  {
    switch(state)
    {
      case BotState.IDLE:

      // go to other team camp
      agent.SetDestination(team.enemy_base.position);
      int carrier_ID = master.GetFlagCarrierID(team.team_ID);
      if(carrier_ID != -1)
      {
        // il y a un carrier
      }

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
