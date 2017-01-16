using UnityEngine;
using System.Collections;

public class BotBehaviourBaseTikasorus : MonoBehaviour {
    //ID du bot qui possède le drapeau
    public int carrier_ID; // ID du porteur de drapeau ennemi
    public GameObject Carrier_obj; // Bot porteur de drapeau ennemi
    public GameObject EnemyIKill; // L'ennemi que je vise mouhaha

    public bool weMark = false; // A-t'on marqué ?
    public int nextScore = 1; // score actuel +1 
    public int score; // score actuel

    // liste des states possibles pour ce comportement de bot
    public enum BotState { 
    IDLE, // Au respawn --> indique marche à suivre
    SEARCHFLAG, // Un seul bot va chercher le drapeau ennemi
    RETURNHOMEWITHFLAG, // Retourne à la base avec le drapeau ennemi
    ESCORTE, // Escorter le porteur de drapeau
    DEFEND, // quand t'es en position sniper --> aller-retours en défense 
    SNIPER, // va se poster aux positions sniper --> aller-retours en défense 
    RESCUEOURFLAG // va chercher le drapeau allié à la base ennemie
    };
  
    public BotState state = BotState.IDLE; // état actuel du bot



    // Game master (script qui gère la capture des drapeaux, le respawn des bots et le score)
    GameMaster master;
    Team team;
    TeamBehaviourTikasorus espritEquipe;

    // go qui possède les components du bot
    GameObject bot_object;
    // script de base contenant ID, team_ID du bot
    Bot bot;
    // components du game object de base du bot
    UnityEngine.AI.NavMeshAgent agent;
    Collider colliderC;
    Renderer rendererR;

	
	void Start () 
    {
    master = FindObjectOfType<GameMaster>();
    team = transform.parent.parent.GetComponent<Team>();
    espritEquipe = transform.parent.parent.GetComponentInChildren<TeamBehaviourTikasorus>();

    bot_object = transform.parent.gameObject;
    bot = bot_object.GetComponent<Bot>();
    agent = bot_object.GetComponent<UnityEngine.AI.NavMeshAgent>();
    colliderC = bot_object.GetComponent<Collider>();
    rendererR = bot_object.GetComponent<Renderer>();

    SwitchState(BotState.IDLE);
	}

    void Update()
    {
        carrier_ID = master.GetFlagCarrierID(team.enemy_team_ID); // récupère l'ID du porteur de drapeau ennemi
        Carrier_obj = master.GetBotFromID(carrier_ID); // récupère le bot porteur de drapeau ennemi
        GizmosService.Text(state.ToString(), transform.position + Vector3.forward, 0.01f, Color.white); // --> affiche l'état du bot
        GizmosService.Cone(transform.position, transform.forward, Vector3.up, 10, 70); // --> affiche le cône de vision
        score = master.GetScore(bot.team_ID); // récupère le score actuel
        //LetGo(); // --> 
        CheckFlag(); // --> si je porte le drapeau, passe en mode ReturnHomeWithFlag
        UpdateState(); // --> exécute les instructions de chaque état
        CheckEnnemy(); // --> permet de tier sur les ennemis
        if (bot.is_dead == true)
        {
            SwitchState(BotState.IDLE); // --> si je suis mort, passe dans l'état Idle
            team.SendMessageToTeam("ImDead"); // --> si je veux envoyer un message à ma mort
        }
    }

    void GetFlag() // Que se passe-t'il quand un bot obtient le drapeau    
    {
		if (Carrier_obj != null)
        	espritEquipe.flag_Enemy_Pos = Carrier_obj.transform.position; // update de la position du drapeau ennemi sur le porteur de drapeau
    }

    public void LetGo()
    {
        
    }

    public void CheckEnnemy()  // --> permet de tier sur les ennemis
    {
        Collider[] ennemies = Physics.OverlapSphere(transform.position, 50);
        for (int i=0; i<ennemies.Length; i++)
        {
            if (ennemies[i].gameObject.tag == "Bot") // Si les objets sont des Bots
            {
                Bot enemy_bot = ennemies[i].gameObject.GetComponent<Bot>();
                if (enemy_bot.team_ID != bot.team_ID) // Si les Bot sont de la team ennemie
                {
                    if (bot.CanSeeObject(ennemies[i].gameObject))
                    {
                        EnemyIKill = ennemies[i].gameObject; // Le Bot que je vise
                        bot.ShootInDirection(EnemyIKill.transform.position - transform.position); // Je le shoote
                    }
                }
            }
        }
    }

    void ImDead() //Que se passe-t'il quand je suis mort ?
    {
        if (carrier_ID == bot.ID)
        {
            Debug.Log("FlagCarrier is dead"); // J'annonce : le porteur de drapeau est mort
        }
        else if (carrier_ID != bot.ID)
        {
            if (espritEquipe.secondGuard == bot.ID || espritEquipe.firstGuard == bot.ID)
            {
                Debug.Log("Guard is dead : " + bot.ID); // J'annonce : le garde est mort
            }
            else if (espritEquipe.firstSniper == bot.ID || espritEquipe.secondSniper == bot.ID)
            {
                Debug.Log("Sniper is dead : " + bot.ID); // J'annonce : le sniper est mort
            }
        }
    }

    public void SearchForFlag()  // --> chercher le drapeau allié si je ne le vois pas
    {
        if (master.IsTeamFlagHome(bot.team_ID) == false) // Si je ne vois pas le drapeau allié
        {
            SwitchState(BotState.RESCUEOURFLAG); //Je passe dans l'état : je cherche le drapeau
        }
        else if (master.IsTeamFlagHome(bot.team_ID) == true) // Si je vois le drapeau allié
        {
            SwitchState(BotState.IDLE); //Je passe dans l'état : je retourne à ma position sniper (aller-retours)
        }
    }

    public void RescueFlag()
    {
        agent.SetDestination(team.enemy_base.position);
        Collider ourFlag = team.team_flag;
        GameObject ourFlagGo = ourFlag.gameObject;
        if (bot.CanSeeObject(ourFlagGo) == true)
        {
            agent.SetDestination(ourFlagGo.transform.position);
            if (master.IsTeamFlagHome(bot.team_ID) == true)
            {
                SwitchState(BotState.SNIPER);
            }
        }
    }

    public void CheckFlag() // Si par hasard un joueur tombe sur le drapeau, va le chercher et le ramène à la base
    {
        if (Carrier_obj != null) // Si personne ne porte le drapeau
        {
            if (carrier_ID == bot.ID) // Si mon ID correspond au porteur de drapeau
            {
                team.SendMessageToTeam("GetFlag"); // J'envoie le message : je suis le porteur de drapeau --> les sniper passent en mode attaque ?
                SwitchState(BotState.RETURNHOMEWITHFLAG); // Je retourne à la maison
            }
        }
    }

    public void WeMark() //vérifie si on a marqué un point
    {
                    if (nextScore < score) // 
                    {
                        weMark = true;
                    }
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

      case BotState.SEARCHFLAG:
                nextScore = score;
                break;

      case BotState.RETURNHOMEWITHFLAG:
      break;

      case BotState.ESCORTE:
      break;

      case BotState.DEFEND:
      break;

      case BotState.SNIPER:
      break;

            case BotState.RESCUEOURFLAG:
                break;
        }
  }

  void UpdateState()
  {
        switch (state)
        {
            case BotState.IDLE:
                if (bot.ID == espritEquipe.flagSearch)
                {
                    SwitchState(BotState.SEARCHFLAG);
                    Debug.Log("I'm flagSearch");
                }
                else if (bot.ID == espritEquipe.firstGuard)
                {
                    SwitchState(BotState.ESCORTE);
                    Debug.Log("I'm firstGuard");
                }
                else if (bot.ID == espritEquipe.secondGuard)
                {
                    SwitchState(BotState.ESCORTE);
                    Debug.Log("I'm secondGuard");
                }
                else if (bot.ID == espritEquipe.firstSniper)
                {
                    SwitchState(BotState.SNIPER);
                    Debug.Log("I'm firstSniper");
                }
                else if (bot.ID == espritEquipe.secondSniper)
                {
                    SwitchState(BotState.SNIPER);
                    Debug.Log("I'm secondSniper");
                }
                break;

            case BotState.SEARCHFLAG:
                //go to the ennemy camp to get their flag mouhahaha
                agent.SetDestination(team.enemy_base.position);
                weMark = false;
                break;

            case BotState.RETURNHOMEWITHFLAG:
                //return to the ally camp with enemy flag
                if (carrier_ID == bot.ID)
                {
                    agent.SetDestination(team.team_base.position);
                    WeMark();
                    if (weMark == true)
                    {
                        espritEquipe.flag_Enemy_Pos = team.enemy_base.position; // ré instancier la position du drapeau ennemi à la base ennemie
                        SwitchState(BotState.IDLE);
                    }
                }
                if (carrier_ID != bot.ID)
                {
                    SwitchState(BotState.IDLE);
                }
                break;

      case BotState.ESCORTE: 
                // follow the flag carrier
      carrier_ID = master.GetFlagCarrierID(team.enemy_team_ID);
      Carrier_obj = master.GetBotFromID(carrier_ID);
                GameObject Should_carrierFlag = master.GetBotFromID(espritEquipe.flagSearch);
                if (Carrier_obj != null) // S'il y a un porteur de drapeau : je le suis 
                {
                    Vector3 escorteCarrier = Carrier_obj.transform.position;
                    agent.SetDestination(escorteCarrier);
                }
                else // S'il n'y a pas de porteur de drapeau, je suis celui qui doit être le porteur
                {
                    agent.SetDestination(Should_carrierFlag.transform.position);
                }
                if (Should_carrierFlag == null) //si le porteur de drapeau est mort : que faire ??
                {
                    agent.SetDestination(team.enemy_base.position);
                    /*if (carrier_ID == bot.ID)
                    {
                        SwitchState(BotState.RETURNHOMEWITHFLAG);
                    }*/
                }
      break;

      case BotState.DEFEND:
                //position base (oui le nom est inaproprié)
                if(bot.team_ID == 0)
                {
                    if (bot.ID == espritEquipe.firstSniper)
                    {
                        agent.SetDestination(new Vector3(40, transform.position.y, 30));
                        if (transform.position == new Vector3(40, transform.position.y, 30))
                        {
                            SearchForFlag();
                        }
                    }
                    if (bot.ID == espritEquipe.secondSniper)
                    {
                        agent.SetDestination(new Vector3(41, transform.position.y, -27));
                        if (transform.position == new Vector3(41, transform.position.y, -27))
                        {
                            SearchForFlag();
                        }
                    }
                }
                else if (bot.team_ID == 1)
                {
                    if (bot.ID == espritEquipe.firstSniper)
                    {
                        agent.SetDestination(new Vector3(-40, transform.position.y, 28));
                        if (transform.position == new Vector3(-40, transform.position.y, 28))
                        {
                            SearchForFlag();
                        }
                    }
                    if (bot.ID == espritEquipe.secondSniper)
                    {
                        agent.SetDestination(new Vector3(-40, transform.position.y, -30));
                        if (transform.position == new Vector3(-40, transform.position.y, -30))
                        {
                            SearchForFlag();
                        }
                    }
                }
                break;

      case BotState.SNIPER:
                //position SNIPER
                if (bot.team_ID == 0)
                {
                    if (bot.ID == espritEquipe.firstSniper)
                    {
                        agent.SetDestination(new Vector3(23, transform.position.y, 30));
                        if (transform.position == new Vector3 (23, transform.position.y, 30))
                        {
                            SwitchState(BotState.DEFEND);
                        }
                    }
                    if (bot.ID == espritEquipe.secondSniper)
                    {
                        agent.SetDestination(new Vector3(27, transform.position.y, -27));
                        if (transform.position == new Vector3(27, transform.position.y, -27))
                        {
                            SwitchState(BotState.DEFEND);
                        }
                    }
                }
                else if (bot.team_ID == 1)
                {
                    if (bot.ID == espritEquipe.firstSniper)
                    {
                        agent.SetDestination(new Vector3(-31, transform.position.y, 28));
                        if (transform.position == new Vector3(-31, transform.position.y, 28))
                        {
                            SwitchState(BotState.DEFEND);
                        }
                    }
                    if (bot.ID == espritEquipe.secondSniper)
                    {
                        agent.SetDestination(new Vector3(-22, transform.position.y, -30));
                        if (transform.position == new Vector3(-22, transform.position.y, -30))
                        {
                            SwitchState(BotState.DEFEND);
                        }
                    }
                }
                break;

            case BotState.RESCUEOURFLAG:
                RescueFlag();
                break;
        }
  }

  void OnExitState()
  {
    switch(state)
    {
      case BotState.IDLE:
      break;

      case BotState.RETURNHOMEWITHFLAG:
      break;

            case BotState.ESCORTE:
                break;

      case BotState.SEARCHFLAG:
      break;

      case BotState.DEFEND:
      break;

      case BotState.SNIPER:
      break;
            case BotState.RESCUEOURFLAG:
                break;
        }
  }
}
