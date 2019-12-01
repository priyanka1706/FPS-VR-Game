using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//enemy detect player
//gun firing afer death
public class Enemy : MonoBehaviour
{
    public GameObject[] target;
    int index = 0; //to track target being traced
    public GameObject player;
    private float cool_period = 0.4f;
    private float timer = 0.0f;

    public GameObject shotSound;
    public GameObject muzzlePrefab;
    public GameObject bulletHole;
    public GameObject end;
    public GameObject start;
    public GameObject gun;

    bool player_shot_at_me = false;
    
    public float health = 100; //reduce by 20
    public bool isDead = false;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animator>().SetBool("dead", false);
        //GetComponent<Animator>().SetBool("fire", false);
    }

    void Update()
    {
        // Detecting player
        // to define viewpoint we need angle bw first and second vector
        Vector3 vec1 = new Vector3((player.transform.position.x - transform.position.x), (player.transform.position.y - transform.position.y), (player.transform.position.z - transform.position.z)).normalized; // first vector
        Vector3 vec2 = transform.forward; //second vector
        //Debug.Log(Vector3.Distance(transform.position, player.transform.position));

        if (Vector3.Dot(vec1, vec2) >= 0.7f)
        {
            //print(Vector3.Distance(transform.position, player.transform.position));
        }

        if (((Vector3.Dot(vec1, vec2) >=0.7f && Vector3.Distance(transform.position, player.transform.position) <= 15.0f) || (player_shot_at_me==true)) && isDead == false)
        {
            Vector3 tempPos = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
            Quaternion desiredRotation = Quaternion.LookRotation(tempPos - transform.position); //smoothly look at target
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime); //interpolate between current rot and desired rot

            if (Vector3.Distance(transform.position, player.transform.position) <= 10.0f)
            {
                //shoot player
                GetComponent<Animator>().SetBool("run", false);
                GetComponent<Animator>().SetBool("fire", false); 
                
                //add fire=false for 0.2 sec 
                timer += Time.deltaTime;
            }
            else
            {
                GetComponent<Animator>().SetBool("run", true);
                GetComponent<Animator>().SetBool("fire", false);
            }

            if (timer >= cool_period)
            {
                if (isDead == true)
                {
                    GetComponent<Animator>().SetBool("fire", false);
                }
                else
                {
                    GetComponent<Animator>().SetBool("run", false);
                    GetComponent<Animator>().SetBool("fire", true); //false
                }
                shotDetection();
                addEffects();
                timer = 0.0f;
            }
        }
        else
        {
            Vector3 tempPos = new Vector3(target[index].transform.position.x, transform.position.y, target[index].transform.position.z); // to keep enemy height const
                                                                                                                                         //transform.LookAt(tempPos); //look at the target's position
            Quaternion desiredRotation = Quaternion.LookRotation(tempPos - transform.position); //smoothly look at target
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime); //interpolate between current rot and desired rot
            if (Vector3.Distance(target[index].transform.position, transform.position) <= 0.5f)
            {
                index = (index + 1) % target.Length;
            }
        }
    }

    public void Being_shot(float damage) // getting hit from player
    {
        health -= damage;
        Debug.Log(health);

        if (health <= 0)
        {
            isDead = true;
            player_shot_at_me = false;
            GetComponent<Animator>().SetBool("fire", false);
            GetComponent<Animator>().SetBool("dead", true);
            GetComponent<CharacterController>().enabled = false;
            gun.GetComponent<Animator>().enabled = false;
            gun.transform.parent = null;
            gun.GetComponent<Rigidbody>().isKinematic = false;
            //GetComponent<CharacterMovement>().isDead = true;
            //GetComponent<CharacterController>().enabled = false;
            Debug.Log("enemy ded!");
            this.enabled = false;
        }
        else
        {
            player_shot_at_me = true;
        }
        /*
        if ((isDead == true)||(health<=0))
        {
            GetComponent<Animator>().SetBool("dead", true);
            //GetComponent<CharacterController>().enabled = false;
            gameObject.transform.Find("M4A1_PBR").transform.parent = null;
            gameObject.transform.Find("M4A1_PBR").GetComponent<Rigidbody>().isKinematic = false;
            //GetComponent<CharacterMovement>().isDead = true;
            //GetComponent<CharacterController>().enabled = false;
            //gameObject.transform.Find("Soldier_head").GetComponent<SkinnedMeshRenderer>().enabled = true;
            //headMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
            Debug.Log("enemy ded");
        }
        */
    }

    void shotDetection() // Detecting the object which enemy shot
    {

        RaycastHit rayHit;
        Vector3 end_temp = end.transform.position + (end.transform.forward*Random.Range(-0.1f, 0.1f)) + (end.transform.right*Random.Range(-0.5f, 0.5f));
        if (Physics.Raycast(end_temp, (end_temp - start.transform.position), out rayHit, 100.0f))
        {
            //print(rayHit.transform.tag);
            if (rayHit.transform.tag == "Player")
            {
                player.GetComponent<GunVR>().Being_shot(20.0f);
                //Debug.Log("Calling Being_Shot from player");
            }
            else
            {
                //not player so bullet hole in wall
                Destroy(Instantiate(bulletHole, rayHit.point + rayHit.transform.up * 0.01f, rayHit.transform.rotation), 2.0f);
            }
        }
    }

    void addEffects() // Adding muzzle flash, shoot sound 
    {
        Destroy(Instantiate(shotSound, transform.position, transform.rotation), 2.0f); //shot sound deleted after 2 secs
        GameObject tempMuzzle = Instantiate(muzzlePrefab, end.transform.position, end.transform.rotation);
        tempMuzzle.GetComponent<ParticleSystem>().Play();
        Destroy(tempMuzzle, 2.0f);
    }
}
