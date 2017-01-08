using UnityEngine;
using System.Collections;

public class Team : MonoBehaviour
{
	// tableau contenant tous les bots de l'équippe
	public Bot[] bots;

	public int team_ID = 0;
	public int enemy_team_ID = 0;

	// couleur de l'équippe (rempli par le GameMaster)
	public Color team_color = Color.white;
	// layer utilisée pour les drapeaux de cette équippe
	public int layer;
	// position de la base (là où se trouve le drapeau de cette équippe)
	public Transform team_base = null;
	// collider du drapeau de cette équippe
	public Collider team_flag = null;
	// collider du drapeau de l'équippe adverse
	public Collider enemy_flag = null;
	// position de la base adverse
	public Transform enemy_base = null;
  
	// Cette fonction permet d'envoyer un message à tous les bots de l'équippe
	// Il suffit d'ajouter une fonction du type void NomFonction() dans
	// le script de comportement des bots pour qu'elle soit appelée
	public void SendMessageToTeam (string message)
	{
		BroadcastMessage (message, null, SendMessageOptions.DontRequireReceiver);
	}
}
