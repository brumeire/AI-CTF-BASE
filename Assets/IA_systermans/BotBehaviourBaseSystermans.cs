using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class BotBehaviourBaseSystermans : MonoBehaviour {

    Text txt;
    public bool CarrierSeen = false;
    public bool ICanSeeTheFoeCarrier = false;
    public bool IAmRounding = false;
  // liste des states possibles pour ce comportement de bot
  public enum BotState{ 
    ROUNDS, //Guardian qui fait sa ronde pour surveiller le flag
    WATCH, //Defensor qui surveille le passage principal
    CHASE_CARRIER, //Poursuivre le porteur ennemi
    PROTECT, //Rester autour du drapeau allié tombé au sol
    WANDER, //MassKiller se balade sur la map
    CHASE_RANDOM, //MassKiller a repéré une cible et la poursuit
    GET_FLAG, //Attaquant va chercher le flag ennemi
    RETURN, //Attaquant revient à la base en portant le flag ennemi
    E_ATTACK, //Escorte et couvre l'attaquant lorsqu'il va chercher le flag ennemi
    E_RETURN, //Escorte et couvre l'attaquant lorsqu'il revient à la base
    SHOOT
  };
    // état actuel du bot

        public enum BotRole
    {
        ATTACK,
        ESCORT,
        DEFENSE,
        GUARDIAN,
        MASS_KILLER
    }

    public BotRole role;
    public List<BotBehaviourBaseSystermans> myMates;
    public BotBehaviourBaseSystermans[] myMatesArray;
  public BotState state;
    Vector3 defensorPos; //Le point où se posera le défenseur pour surveiller
    BotState defaultState; //Chaque bot a un état par défaut, s'il n'a rien de particulier à faire (genre taper le porteur ennemi) il retournera dans cet état

   
  // Game master (script qui gère la capture des drapeaux, le respawn des bots et le score)
  GameMaster master;
  Team team;
    static Vector3 lastFoeCarrierPos;
  // go qui possède les components du bot
  GameObject bot_object;
    Bot target=null;
  // script de base contenant ID, team_ID du bot
  Bot mybot;
  // components du game object de base du bot
  UnityEngine.AI.NavMeshAgent agent;
  Collider collider;
  Renderer renderer;

   public BotBehaviourBaseSystermans IEscortThisGuy; //Le botcontroller de l'attaquant, utile uniquement pour son escorte

    Vector3[] wayPoints = new Vector3[4]; //Les endroits par lesquels passe le MassKiller lorsqu'il est en state Wander
    int currentWayPoint;


	void Start () 
  {
       


        txt = GetComponentInChildren<Text>(); //Chaque bot a un canvas avec un texte affichant son rôle et son état
    master = FindObjectOfType<GameMaster>();
    team = transform.parent.parent.GetComponent<Team>();
        currentWayPoint = Random.Range(0, 4);
        bot_object = transform.parent.gameObject;
    mybot = bot_object.GetComponent<Bot>();
    agent = bot_object.GetComponent<UnityEngine.AI.NavMeshAgent>();
    collider = bot_object.GetComponent<Collider>();
    renderer = bot_object.GetComponent<Renderer>();

        //Les 4 coins de la map
        wayPoints[0] = new Vector3(-42, 0, -23);
        wayPoints[1] = new Vector3(46, 0, -27.7f);
        wayPoints[2] = new Vector3(44.7f, 0, 28);
        wayPoints[3] = new Vector3(-47, 0, 30);

        if (team.team_ID==0)
        {
            defensorPos = new Vector3(26, 0, 10);
        }
        else
            defensorPos = new Vector3(-26, 0, -10);


        switch (transform.parent.GetComponent<Bot>().ID)//Set le role de chaque bot en fonction de son ID
        {
            case 0:
                role = BotRole.ATTACK;
                defaultState = BotState.GET_FLAG;
                break;
           case 1:
                role = BotRole.GUARDIAN;
                defaultState = BotState.ROUNDS;
            
                break;
           case 2:
                role = BotRole.ESCORT;
                IEscortThisGuy = master.GetBotFromID(0 + team.team_ID * 10).GetComponentInChildren<BotBehaviourBaseSystermans>();
                Debug.Log(IEscortThisGuy.gameObject.name);
                defaultState = BotState.E_ATTACK;
                break;
            case 3:
                role = BotRole.DEFENSE;
                defaultState = BotState.WATCH;
                break;
            case 4:
                role = BotRole.MASS_KILLER;
                defaultState = BotState.WANDER;
                break;
            case 10:
                role = BotRole.ATTACK;
                defaultState = BotState.GET_FLAG;
                break;
            case 11:
                role = BotRole.GUARDIAN;
                defaultState = BotState.ROUNDS;
                break;
            case 12:
                role = BotRole.ESCORT;
                IEscortThisGuy = master.GetBotFromID(0 + team.team_ID * 10).GetComponentInChildren<BotBehaviourBaseSystermans>();
                Debug.Log(IEscortThisGuy.gameObject.name);
                defaultState = BotState.E_ATTACK;
                break;
            case 13:
                role = BotRole.DEFENSE;
                defaultState = BotState.WATCH;
                break;
            case 14:
                role = BotRole.MASS_KILLER;
                defaultState = BotState.WANDER;
                break;

        }
        Debug.Log(team.bots.Length);
        for (int i = 0; i < 5; i++)
        {
            
            myMates.Add(team.bots[i].GetComponentInChildren<BotBehaviourBaseSystermans>());
           
        }
        
        myMatesArray = myMates.ToArray();
        state = defaultState;
        SwitchState(defaultState);
	}


    void OurFlagTaken()
    {
        
        master.is_flag_home[team.team_ID] = false;
        if (role != BotRole.GUARDIAN && role!=BotRole.ATTACK)
            SwitchState(BotState.CHASE_CARRIER);
    }
    
  void Update()
  {
        txt.text = role + "\n" + state;
    UpdateState();
        if (master.flag_carriers[team.team_ID] != -1)
        {
            team.SendMessageToTeam("OurFlagTaken");
        }
        foreach (Bot foe in master.teams[team.enemy_team_ID].bots)
        {
            if(foe== master.GetBotFromID(master.flag_carriers[team.enemy_team_ID]))
            {
                team.BroadcastMessage("SeeCarrier");
                lastFoeCarrierPos = foe.transform.position;
            }
            if (mybot.CanSeeObject(foe.gameObject)&& state!=BotState.SHOOT)
            {
                SwitchState(BotState.SHOOT);
            }
        }
        if(role==BotRole.GUARDIAN &&Vector3.Distance(bot_object.transform.position, master.respawn_points[team.team_ID].position)<2)
        {
            IAmRounding = false;
        }

        //Doit permettre à l'équipe de savoir si l'un de ses bots voit le porteur de drapeau ennemi
        int nbWhoSeeTheFoeCarrier = 0;
       foreach(BotBehaviourBaseSystermans mate in myMates)
        {
            if (mate.ICanSeeTheFoeCarrier)
                nbWhoSeeTheFoeCarrier++;
        }
        if (nbWhoSeeTheFoeCarrier == 0)
            CarrierSeen = false;
        else
            CarrierSeen = true;

    }


    void SwitchState(BotState new_state)
  {
    OnExitState();
    state = new_state;
    OnEnterState();
  }

  void OnEnterState()
  {
        switch (state)
        {
            case BotState.RETURN:
               
                break;
            case BotState.GET_FLAG:
                agent.SetDestination(team.enemy_base.position);
                break;
            case BotState.ROUNDS:
                StartCoroutine(Rounds());
               
                break;
            case BotState.CHASE_CARRIER:
              
                break;
            case BotState.E_ATTACK:
               
               
                break;

            case BotState.WANDER:
                agent.SetDestination(wayPoints[currentWayPoint]);
                break;

            case BotState.SHOOT:
                foreach (Bot foe in master.teams[team.enemy_team_ID].bots)
                {
                    if (mybot.CanSeeObject(foe.gameObject))
                    {
                        target = foe;
                    }
                }
                break;
        }
   }
   
  void UpdateState()
  {
    switch(state)
    {
      case BotState.GET_FLAG:

      // go to other team camp
     
                if (master.flag_carriers[team.enemy_team_ID] != -1)
                {
                    Debug.Log("Gotcha");
                    SwitchState(BotState.RETURN);
                }
                    break;

            case BotState.ROUNDS:
                if(!IAmRounding)
                {
                    StartCoroutine(Rounds());
                }
                break;

            case BotState.WATCH:
                agent.SetDestination(defensorPos);
                mybot.transform.RotateAround(transform.position, transform.up, Mathf.Sin( Time.time));//Balaye du regard d'un côté à l'autre
                break;

            case BotState.CHASE_CARRIER:

                
                if(mybot.CanSeeObject(team.enemy_flag.gameObject))
                {
                    mybot.ShootInDirection(team.enemy_flag.gameObject.transform.position - bot_object.transform.position);
                }

                if (CarrierSeen)
                    {
                        agent.SetDestination(master.GetBotFromID(master.flag_carriers[team.enemy_team_ID]).transform.position);
                        mybot.ShootInDirection(team.team_flag.transform.position - transform.position);
                    }
                    else
                    {
                        agent.SetDestination(team.enemy_base.position);
                        if(Vector3.Distance(transform.position, team.enemy_base.position)<2)
                        {
                            agent.Stop();
                       
                        mybot.transform.RotateAround(transform.position, transform.up, 3);
                    }
                       
                    }
                   
                if (master.flag_carriers[team.enemy_team_ID] == -1)
                {
                    SwitchState(defaultState);
                }
                break;
                

            case BotState.WANDER:
                if(agent.remainingDistance<4)
                {
                    currentWayPoint++;
                    if (currentWayPoint > 3)
                        currentWayPoint = 0;
                    
                }
                agent.SetDestination(wayPoints[currentWayPoint]);
               
                foreach (Bot foe in master.teams[team.enemy_team_ID].bots)
                {
                    if (mybot.CanSeeObject(foe.gameObject))
                    {
                        SwitchState(BotState.SHOOT);
                    }
                    
                }
                break;

            case BotState.RETURN:
                Debug.Log("I'mComingHome");
                agent.SetDestination(team.team_base.position);
                if(master.is_flag_home[team.enemy_team_ID])
                {
                    SwitchState(BotState.GET_FLAG);
                }
                break;

            case BotState.E_ATTACK:
               
                agent.SetDestination(IEscortThisGuy.transform.position + IEscortThisGuy.transform.forward * 5);//Reste toujours 5m devant l'attaquant

                if (IEscortThisGuy.state == BotState.RETURN)
                    SwitchState(BotState.E_RETURN);
                break;

            case BotState.E_RETURN:
                agent.SetDestination(IEscortThisGuy.transform.position-IEscortThisGuy.transform.forward*5); //Reste toujours 5m derrière l'attaquant
                transform.LookAt(-IEscortThisGuy.transform.forward);
                if (master.flag_carriers[team.enemy_team_ID] != -1)
                    SwitchState(BotState.CHASE_CARRIER);
                    break;

            case BotState.SHOOT:
                mybot.ShootInDirection(target.transform.position - transform.position);
                if(target.is_dead || !mybot.CanSeeObject(target.gameObject))
                {
                    SwitchState(defaultState);
                }
                break;
        }
  }
    
    IEnumerator Rounds()//Coroutine permettant au guardian de faire des rondes aléatoires autour de son drapeau
    {
        IAmRounding = true;
        while(true)
        {
            Vector3 randAround = master.teams[team.team_ID].team_base.position + new Vector3(Random.Range(-8, 8), 0, Random.Range(-8, 8));
            agent.SetDestination(randAround);
            if(Vector3.Distance(agent.destination, bot_object.transform.position)==0)
            transform.LookAt(master.teams[team.team_ID].team_base.position);
            yield return new WaitForSeconds(5);
        }
    }
  
  

  void OnExitState()
  {
      switch(state)
        {
            case BotState.GET_FLAG:
                Debug.Log("Je sors de Get_Flag");
                break;
            case BotState.ROUNDS:
                IAmRounding = false;
                break;
        }
  }

    public void SeeCarrier()
    {
          lastFoeCarrierPos = master.GetBotFromID(master.flag_carriers[team.enemy_team_ID]).transform.position;
    }
}
