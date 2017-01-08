using UnityEngine;
using System.Collections;

public class TeamBehaviourBase : MonoBehaviour
{

	Team team;

	void Start ()
	{
		team = transform.parent.GetComponent<Team> ();
	}

	void Update ()
	{
	
	}
}
