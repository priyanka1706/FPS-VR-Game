using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GunVR : MonoBehaviour {

    public GameObject end, start; // The gun start and end point
    public GameObject gun;
    public Animator animator;
    
    public GameObject spine;
    public GameObject handMag;
    public GameObject gunMag;

    float gunShotTime = 0.1f;
    float gunReloadTime = 1.0f;
    Quaternion previousRotation;
    public float health = 100; //reduce by 20
    public bool isDead = false;

    public Text magBullets;
    public Text remainingBullets;
    public Text health_remaining;
    public Text win;

    public GameObject shotSound;
    public GameObject muzzlePrefab;
    public GameObject bulletHole;

    int magBulletsVal = 30;
    int remainingBulletsVal = 90;
    int magSize = 30;
    string win_message="";
    public GameObject headMesh;
    public static bool leftHanded { get; private set; }

    // Use this for initialization
    void Start() {
        headMesh.GetComponent<SkinnedMeshRenderer>().enabled = false; // Hiding player character head to avoid bugs :)
    }

    // Update is called once per frame
    void Update() {
        
        // Cool down times
        if (gunShotTime >= 0.0f)
        {
            gunShotTime -= Time.deltaTime;
        }
        if (gunReloadTime >= 0.0f)
        {
            gunReloadTime -= Time.deltaTime;
        }


        OVRInput.Update();
        
        if ((OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) || Input.GetMouseButtonDown(0)) && gunShotTime <= 0 && gunReloadTime <= 0.0f && magBulletsVal > 0 && !isDead)
        { 
            shotDetection(); // Should be completed

            addEffects(); // Should be completed

            animator.SetBool("fire", true);
            gunShotTime = 0.5f;
            
            // Instantiating the muzzle prefab and shot sound
            
            magBulletsVal = magBulletsVal - 1;
            if (magBulletsVal <= 0 && remainingBulletsVal > 0)
            {
                animator.SetBool("reloadAfterFire", true);
                gunReloadTime = 2.5f;
                Invoke("reloaded", 2.5f);
            }
        }
        else
        {
            animator.SetBool("fire", false);
        }

        if ((OVRInput.GetDown(OVRInput.Button.Back) || OVRInput.Get(OVRInput.Button.Back) || OVRInput.GetDown(OVRInput.RawButton.Back) || OVRInput.Get(OVRInput.RawButton.Back) || Input.GetKey(KeyCode.R)) && gunReloadTime <= 0.0f && gunShotTime <= 0.1f && remainingBulletsVal > 0 && magBulletsVal < magSize && !isDead )
        {
            animator.SetBool("reload", true);
            gunReloadTime = 2.5f;
            Invoke("reloaded", 2.0f);
        }
        else
        {
            animator.SetBool("reload", false);
        }
        updateText();
       
    }


    public void Being_shot(float damage) // getting hit from enemy
    {
        health -= damage;
        if (health<=0)
        {
            isDead = true;
        }
        Debug.Log(health);
        if (isDead == true)
        {
            GetComponent<Animator>().SetBool("dead", true);
            GetComponent<CharacterMovement>().isDead = true;
            GetComponent<CharacterController>().enabled = false;
            //gameObject.transform.Find("Soldier_head").GetComponent<SkinnedMeshRenderer>().enabled = true;
            headMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;
            Debug.Log("Game over");
        }
    }

    
    public void ReloadEvent(int eventNumber) // appearing and disappearing the handMag and gunMag
    {
        if (eventNumber == 1)
        {
            handMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        if (eventNumber == 2)
        {
            handMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
    }

    void reloaded()
    {
        int newMagBulletsVal = Mathf.Min(remainingBulletsVal + magBulletsVal, magSize);
        int addedBullets = newMagBulletsVal - magBulletsVal;
        magBulletsVal = newMagBulletsVal;
        remainingBulletsVal = Mathf.Max(0, remainingBulletsVal - addedBullets);
        animator.SetBool("reloadAfterFire", false);
    }

    void updateText()
    {
        magBullets.text = magBulletsVal.ToString() ;
        remainingBullets.text = remainingBulletsVal.ToString();
        health_remaining.text = health.ToString();
        win.text = win_message;
    }

    void shotDetection() // Detecting the object which player shot
    {
        RaycastHit rayHit;
        if (Physics.Raycast(end.transform.position, (end.transform.position - start.transform.position), out rayHit, 100.0f))
        {
            if (rayHit.transform.tag == "enemy")
            {
                rayHit.transform.gameObject.GetComponent<Enemy>().Being_shot(20.0f);
                //Debug.Log("Calling Being_Shot from enemy");
            }
            else
            {
                //not enemy so bullet hole in wall
                Instantiate(bulletHole, rayHit.point + rayHit.transform.up*0.01f, rayHit.transform.rotation);
            }
        }
    }

    void addEffects() // Adding muzzle flash, shoot sound and bullet hole on the wall
    {
        Destroy(Instantiate(shotSound, transform.position, transform.rotation),2.0f); //shot sound deleted after 2 secs
        GameObject tempMuzzle = Instantiate(muzzlePrefab, end.transform.position, end.transform.rotation);
        tempMuzzle.GetComponent<ParticleSystem>().Play();
        Destroy(tempMuzzle, 2.0f);
    }

    void OnTriggerEnter(Collider door_col)
    {
        if (door_col.gameObject.tag=="door")
        {
            Debug.Log("Game won!");
            win_message = "You Win!";
            Invoke("restart_game", 10.0f);
            //win.GetComponent<Text>().enabled = true;
        }
        else if (door_col.gameObject.tag == "Ammo")
        {
            Debug.Log("Ammo Collected!");
            win_message = "Ammo Collected!";
            remainingBulletsVal = 90;
            Invoke("remove_message", 3.0f);
        }
        /*
        if (door_col.gameObject.tag == "cube")
        {
            Debug.Log("cube trigger");
            win_message = "You Win!";
            Invoke("restart_game", 10.0f);
            //win.GetComponent<Text>().enabled = true;
        }*/
    }

    public void restart_game()
    {
        SceneManager.LoadScene(0);
        Debug.Log("Game Restarted");
    }
    public void remove_message()
    {
        win_message = "";
    }
}
//scenemanager.loadscene for in 10 sec using time.deltatime or invoke
//raycasthitall or physics.raycast with layer mask 1-not ig or 0 ignored