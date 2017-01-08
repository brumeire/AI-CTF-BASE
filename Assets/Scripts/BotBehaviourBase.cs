using UnityEngine;
using System.Collections;

public class BotBehaviourBase : MonoBehaviour {


  // liste des states possibles pour ce comportement de bot
  public enum BotState{ 
    IDLE
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

  void SeenEnemyFlag()
  {
    
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
    }
  }

  void UpdateState()
  {
    switch(state)
    {
      case BotState.IDLE:
      if(Input.GetMouseButtonDown(0))
      {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(r, out hit))
        {
          agent.SetDestination(hit.point);
        }
      }

      if(Input.GetMouseButtonDown(1))
      {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(r, out hit))
        {
          Vector3 dir = hit.point - transform.position;
          bot.ShootInDirection(dir);
        }
      }


      // debug pour afficher si on voit notre drapeau et éventuellement son porteur
      bool sees_flag = bot.CanSeeObject(team.team_flag.gameObject);
      bool sees_carrier = false;
      int carrier_ID = master.GetFlagCarrierID(team.team_ID);
      if(carrier_ID != -1 && bot.CanSeeObject(master.GetBotFromID(carrier_ID)))
      {
        sees_carrier = true;
      }

      GizmosService.Text("flag ? " + sees_flag, transform.position - Vector3.right * 4);
      GizmosService.Text("carrier ? " + sees_carrier, transform.position - Vector3.right * 8);

      GizmosService.Cone( transform.position, 
                          transform.forward, 
                          transform.up, 
                          100, 
                          70);

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
