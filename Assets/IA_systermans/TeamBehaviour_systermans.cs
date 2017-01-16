using UnityEngine;
using System.Collections;

public class TeamBehaviour_systermans : MonoBehaviour {

  Team team;
    public Vector3 lastFoeCarrPos;


	void Start () 
  {
    team = transform.parent.GetComponent<Team>();
	}
	
	void Update () 
  {
	
	}
}
