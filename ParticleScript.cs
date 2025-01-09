using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleScript : MonoBehaviour
{
    ParticleSystem _pS;
    // Start is called before the first frame update
    void Start()
    {
        _pS = GetComponent<ParticleSystem>(); //get the particle script attatched to the gameobject

        if (transform.CompareTag("explode")) _pS.Stop(); //stop the particle system 
    }

    //this method detatches the gameobject from its parent (the rockett)
    public void Detatch() {
        if (transform.CompareTag("explode")) Explode(); //explode if the explode particle
        else StartSelfDestruct(); //if it's a trail, start self destruct
        transform.SetParent(null); //remove the parent
    }

    //this method plays the explode particle
    public void Explode() {
       if (transform.CompareTag("explode")) _pS.Play(); //play the explosion particle 
        StartSelfDestruct(); //start the self destruct timer
    }

    //this method starts the self destruct coroutine
    public void StartSelfDestruct() {
        StartCoroutine("SelfDestruct"); //start the self destruct coroutine
    }

    //this coroutine
    IEnumerator SelfDestruct() {
        yield return new WaitForSeconds(3f); //wait for 3 seconds
        Destroy(gameObject); //destroy self
    }
}
