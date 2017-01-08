using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public struct RespawnData
{
  public Bot bot;
  public float timer;
}


////////////////////////////////
// script qui gère la capture des drapeaux, 
// le respawn des bots et le score
////////////////////////////////
public class GameMaster : MonoBehaviour 
{
  // Textes pour le score (ne pas toucher)
  public Text text_score_team_red;
  public Text text_score_team_blue;
  // Text pour afficher du DEBUG
  // (peut être utilisé)
  public Text DEBUG_TEXT;

  
  public Team[] teams = new Team[2];
  List<Bot> every_bots = new List<Bot>();
  GameObject[] flags = new GameObject[2];


  // données de paramétrage de la partie
  public GameObject bot_prefab;
  public GameObject flag_prefab;
  public Transform[] spawn_points;
  public Transform[] respawn_points;
  public Color[] team_color;
  public string[] layers;
  public int nb_bot_per_team = 5;


  // Prefabs contenant vos scripts d'IA
  public GameObject[] team_controller_prefab;
  public GameObject[] bot_controller_prefab;

  List<RespawnData> to_respawn = new List<RespawnData>();
  // delai de respawn quand un bot a été éliminé
  // (ne pas modifier)
  float respawn_delay = 8;




  // variables remplies par le GameMaster
  // donant les informations sur l'état de la partie
  // (drapeaux, porteurs de drapeaux, score)
  public bool[] is_flag_home = new bool[2];
  public int[] flag_carriers = new int[2];
  public int[] score = new int[2];




  /*******************************************
  /*******************************************
  /*******************************************
  /* POUR VOUS !
  /* FONCTIONS PERMETTANT DE RECUPPERER
  /* CES INFOS
  /*******************************************
  /*******************************************
  /*******************************************/

  // retourne l'ID du bot qui porte le drapeau de l'équippe
  // passée en paramètre
  // renvoie -1 si personne ne porte le drapeau
  // (le drapeau peut alors être à la base ou avoir été laché par le porteur s'il a été éliminé)
  public int GetFlagCarrierID(int flag_team_ID)
  {
    return flag_carriers[flag_team_ID];
  }

  // dit si le flag de l'équippe passée en paramètre
  // est dans sa base
  public bool IsTeamFlagHome(int team_ID)
  {
    return is_flag_home[team_ID];
  }


  // récupère le score de l'équippe passée en paramètre
  public int GetScore(int team_ID)
  {
    return score[team_ID];
  }


  // retourne le GameObject du bot possédant l'ID passée en paramètre
  // retourne null si cette ID n'est pas trouvée
  // retourne l'ID du porteur du flag de l'équippe
  // passée en paramètre
  // retourne -1 si personne ne porte ce drapeau
  public GameObject GetBotFromID(int ID)
  {
    for(int i = 0; i < every_bots.Count; i++)
    {
      if(every_bots[i].ID == ID)
      {
        return every_bots[i].gameObject;
      }
    }
    return null;
  }




  /////////////////////////////
  // CODE DU GAME MASTER
  /////////////////////////////

  void Awake()
  {
    SetupForLocalPlay();
    StartGame();
  }


  public void SetupForLocalPlay()
  {
    for(int i = 0; i < 2; i++)
    {
      GameObject team_root = new GameObject("TEAM_" + i);
      teams[i] = team_root.AddComponent<Team>();
      teams[i].team_ID = i;
      teams[i].team_color = team_color[i];
      teams[i].layer = LayerMask.NameToLayer(layers[i]);
      teams[i].team_base = spawn_points[i];

      if(team_controller_prefab[i] != null)
      {
        GameObject team_controller = (GameObject)GameObject.Instantiate(team_controller_prefab[i], Vector3.zero, Quaternion.identity);
        team_controller.transform.parent = team_root.transform;
        team_controller.transform.localPosition = Vector3.zero;
      }

      List<Bot> bots = new List<Bot>();

      for(int j = 0; j < nb_bot_per_team; j++)
      {
        GameObject bot_go = (GameObject)GameObject.Instantiate(Resources.Load("bot_prefab"), respawn_points[i].position, Quaternion.identity);
        GameObject bot_controller = (GameObject)GameObject.Instantiate(bot_controller_prefab[i]);
        bot_controller.transform.parent = bot_go.transform;
        bot_controller.transform.localPosition = Vector3.zero;

        bot_go.GetComponent<Renderer>().material.color = team_color[i];
        bot_go.layer = teams[i].layer;

        bot_go.transform.parent = team_root.transform;
        bot_go.name = "bot-"+j+"_team-"+i;

        Bot bot_infos = bot_go.GetComponent<Bot>();
        bot_infos.ID = i * 10 + j;
        bot_infos.team_ID = i;
        bot_infos.enemy_team_ID = (i+1)%2;

        bots.Add(bot_infos);
      }
      teams[i].bots = bots.ToArray();
      every_bots.AddRange(bots);


      flags[i] = (GameObject)GameObject.Instantiate( Resources.Load("flag_prefab"), 
                                                spawn_points[i].position, 
                                                Quaternion.identity);
      flags[i].name = "flag_" + i;
      flags[i].tag = "Flag";
      flags[i].GetComponent<Renderer>().material.color = team_color[i];
    }
  }

  

  void StartGame()
  {
    is_flag_home[0] = true;
    is_flag_home[1] = true;
    flag_carriers[0] = -1;
    flag_carriers[1] = -1;
    score[0] = 0;
    score[1] = 0;

    teams[0].team_ID = 0;
    teams[0].enemy_team_ID = 1;
    teams[0].team_base = spawn_points[0];
    teams[0].enemy_base = spawn_points[1];
    teams[0].team_flag = flags[0].GetComponent<Collider>();
    teams[0].enemy_flag = flags[1].GetComponent<Collider>();

    teams[1].team_ID = 1;
    teams[1].enemy_team_ID = 0;
    teams[1].team_base = spawn_points[1];
    teams[1].enemy_base = spawn_points[0];
    teams[1].team_flag = flags[1].GetComponent<Collider>();
    teams[1].enemy_flag = flags[0].GetComponent<Collider>();
  }



  public void OnBotDestroyed(GameObject bot)
  {
    Bot bot_infos = bot.GetComponent<Bot>();
    if(bot_infos.ID == flag_carriers[0])
    {
      flag_carriers[0] = -1;
    }
    if(bot_infos.ID == flag_carriers[1])
    {
      flag_carriers[1] = -1;
    }

    bot_infos.is_dead = true;
    RespawnData rd;
    rd.timer = respawn_delay;
    rd.bot = bot_infos;
    to_respawn.Add(rd);
    bot.SetActive(false);
  }

  void Respawn(Bot bot)
  {
    bot.transform.position = respawn_points[bot.team_ID].position;
    bot.gameObject.SetActive(true);
    bot.is_dead = false;
  }


	void Update () 
  {
    // CHECK PLAYERS TO RESPAWN
    int k = 0;
    while(k < to_respawn.Count)
    {
      RespawnData dat = to_respawn[k];
      dat.timer -= Time.deltaTime;
      to_respawn[k] = dat;
      
      if(dat.timer < 0)
      {
        Respawn(dat.bot);
        to_respawn.RemoveAt(k);
      }else
      {
        k++;
      }
    }
    

    // FLAG INTERACTIONS FOR RED TEAM
    for(int i = 0; i < teams[0].bots.Length; i++)
    {
      if(!teams[0].bots[i].gameObject.activeSelf) continue;

      Collider c = teams[0].bots[i].GetComponent<Collider>();
      // Got enemy flag
      if(c.bounds.Intersects(teams[0].enemy_flag.bounds))
      {
        // check if no one already carries this flag
        if(flag_carriers[1] == -1)
        {
          is_flag_home[1] = false;
          flag_carriers[1] = teams[0].bots[i].ID;
        }
      }else if(c.bounds.Intersects(teams[0].team_flag.bounds)) // get back team flag
      {
        // check if flag is not home AND no enemy is carrying it
        if(!is_flag_home[0] && flag_carriers[0] == -1)
        {
          // bring back flag to base
          teams[0].team_flag.transform.position = teams[0].team_base.transform.position;
          is_flag_home[0] = true;
        }
      }
    }

    // FLAG INTERACTIONS FOR BLUE TEAM
    for(int i = 0; i < teams[1].bots.Length; i++)
    {
      if(!teams[1].bots[i].gameObject.activeSelf) continue;

      Collider c = teams[1].bots[i].GetComponent<Collider>();
      // Got master flag
      if(c.bounds.Intersects(teams[1].enemy_flag.bounds))
      {
        // check if no one already carries this flag
        if(flag_carriers[0] == -1)
        {
          is_flag_home[0] = false;
          flag_carriers[0] = teams[1].bots[i].ID;
        }
      }else if(c.bounds.Intersects(teams[1].team_flag.bounds))
      {
        // check if flag is not home AND no bot from team red is carrying it
        if(!is_flag_home[1] && flag_carriers[1] == -1)
        {
          // bring back flag to base
          teams[1].team_flag.transform.position = teams[1].team_base.position;
          is_flag_home[1] = true;
        }
      }
    }


    // FLAG MOVEMENT
    if(flag_carriers[1] != -1)
    {
      teams[0].enemy_flag.transform.position = GetBotFromID(flag_carriers[1]).transform.position;
    }

    if(flag_carriers[0] != -1)
    {
      teams[0].team_flag.transform.position = GetBotFromID(flag_carriers[0]).transform.position;
    }



    if(teams[0].team_flag.bounds.Intersects(teams[0].enemy_flag.bounds))
    {
      if(is_flag_home[0])
      {
        // red scores
        score[0]++;
        
        is_flag_home[1] = true;
        is_flag_home[0] = true;
        teams[0].enemy_flag.transform.position = teams[0].enemy_base.position;
        flag_carriers[1] = -1;
        flag_carriers[0] = -1;

        teams[0].SendMessageToTeam("OnScored");

      }else if(is_flag_home[1])
      {
        // blue scores
        score[1]++;

        is_flag_home[1] = true;
        is_flag_home[0] = true;
        teams[1].enemy_flag.transform.position = teams[1].enemy_base.position;
        flag_carriers[1] = -1;
        flag_carriers[0] = -1;

        teams[1].SendMessageToTeam("OnScored");
      }
    }

    text_score_team_red.text = score[0].ToString();
    text_score_team_blue.text = score[1].ToString();
	}
}
