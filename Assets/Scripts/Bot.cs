using UnityEngine;
using System.Collections;

public class Bot : MonoBehaviour {
  // identifiant du bot (unique)
  public int ID;
  // identifiant de l'équippe du bot
  public int team_ID; 
  // identifiant de l'équippe adverse
  public int enemy_team_ID; 


  // valeur du cooldown de tir (ne pas modifier)
  float shoot_cooldown = 2;
  // l'avatar est-il en cooldown ? (ne pas modifier)
  public bool can_shoot = true; 
  // demi-angle de vision (ne pas modifier)
  float vision_angle = 70; 

  // permet de savoir si on est mort (géré par le game master)
  public bool is_dead = false;


  // CONTRAINTES : 
  // La distance de vision d'un bot est infinie.
  // Il ne voit pas un objet s'il y a un mur entre lui et cet objet
  // Par contre le bot a un champ de vision de 140° autour de lui
  public bool CanSeeObject(GameObject obj)
  {
    Vector3 dir_to_obj = obj.transform.position - transform.position;
    Ray r = new Ray(transform.position, dir_to_obj);
    RaycastHit hit;
    // si on ne cherche pas spécifiquement un drapeau, on ignore celui-ci dans le raycast
    int layer_mask = Physics.DefaultRaycastLayers;
    if(obj.tag != "Flag")
    {
      layer_mask = 1 << LayerMask.NameToLayer("Flag");
      layer_mask |= Physics.IgnoreRaycastLayer;
      layer_mask = ~layer_mask;
    }

    if(Physics.Raycast(r, out hit, Mathf.Infinity, layer_mask))
    {
      if(hit.collider.gameObject == obj)
      {
        if(Vector3.Angle(transform.forward, dir_to_obj) <= vision_angle)
        {
          return true;
        }
      }
    }
    return false;
  }
	
  // tire une roquette dans la direction donnée en paramètre
  // (la direction est dans le repère du monde)
  // La roquette n'est pas tirée si on est en cooldown
	public void ShootInDirection(Vector3 dir)
  {
    if(can_shoot)
    {
      GameObject bullet = null;
      Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
      bullet = (GameObject)GameObject.Instantiate(Resources.Load("rocket_prefab"), transform.position, Quaternion.identity);
      bullet.layer = gameObject.layer;
      bullet.transform.rotation = rot;
      can_shoot = false;
      Invoke("Reload", shoot_cooldown);
    }
  }

  // Gestion du reload, Ne pas appeler vous-même
  void Reload()
  {
    can_shoot = true;
  }
}
