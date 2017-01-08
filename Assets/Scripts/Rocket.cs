using UnityEngine;
using System.Collections;

public class Rocket : MonoBehaviour {

  float speed = 22; // vitesse des roquette (ne pas modifier)
	Rigidbody rigid;

	void Awake () 
  {
    rigid = GetComponent<Rigidbody>();
	}
	
	void FixedUpdate () 
  {
    rigid.velocity = Vector3.zero;
    rigid.AddForce(transform.forward * speed, ForceMode.VelocityChange);
	}

  void OnCollisionEnter(Collision col)
  {
    if(col.collider.tag == "Bot")
    {
      GameObject bot = col.collider.gameObject;
      FindObjectOfType<GameMaster>().OnBotDestroyed(bot);
    }
    Destroy(gameObject);
  }
}
