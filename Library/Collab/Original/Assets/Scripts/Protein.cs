using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Protein : MonoBehaviour
{
    // Start is called before the first frame update


    [SerializeField]
    public Rigidbody rb;
    private GameObject wall;
    private GameObject floor;
    public Collider collider;
    public int proteinID;

    // Self destruct settings
    private float selfTimer = 30;   // Time to destruct
    public float timer;             // Time counter

    private Vector3 pos;
    private Quaternion rot;
    private Vector3 scale;
    public bool thrown = false;

    private ProteinManager proteinManager;

    // Bounce settings
    private double maxBounce;    // y position
    private double minBounce;    // y position
    private double grav = -0.01;
    private double vel = 0.09;
    private int bounceDir = -1;
    private float bounceTime = 0;
    
    private List<string>[] proteinGroups;
    private string groupName = "Protein Group";
    private AudioSource audioSourceArray;

    // Dissolve
    public Dissolve Dissolve;
    private bool destroyed = false;
    private bool isBeingThrown = false;

    
    private void Throw(Vector3 vel)
    {
        thrown = true;
        if (proteinManager.PlayerIsHost())
        {
            return;
        }
        else
        {
            proteinManager.SendThrow(proteinID, vel);
        }
    }

    private void ResetTimer(Protein p)
    {
        p.timer = this.selfTimer;
    }

    private int GetProteinGroup(string proteinName)
    {
        for (int i = 0; i < proteinGroups.Length; i++)
        {
            if (proteinGroups[i].Contains(proteinName))
                return i;
        }

        return -1;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.name == "FollowCube")
        {
            if (!thrown && isBeingThrown)
            {
                Debug.Log("Cube touched by " + this.name);
                Throw(rb.velocity);
                if (!proteinManager.PlayerIsHost())
                {
                    rb.isKinematic = true; // must go after throw()
                }
                
                isBeingThrown = false;
                return;
            }

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // stick the colliders if not colliding with cylinder and if they dont already have a joint

        if (collision.collider.gameObject.name == "FollowCube")
        {
            isBeingThrown = true;
        }

        else if (proteinManager.PlayerIsHost())
        {

            // only process collision if player is the host

            if (collision.gameObject != wall) //&& collision.gameObject != floor)
            {

                GameObject collidedProtein = collision.collider.gameObject;

                if (GetProteinGroup(this.name) == GetProteinGroup(collidedProtein.name)){

                    /*GameObject effectPositive = Instantiate(proteinManager.collisionEffect);
                    effectPositive.transform.position = this.transform.position;
                    effectPositive.GetComponent<ParticleSystem>().Play();
                    Debug.Log("Pos Effect Placed");
                    Destroy(effectPositive, 2.7f);
                    */
                    this.GetComponent<Light>().enabled = true;
                    Debug.Log("Light on for " + this.name);
                    
                    

                    this.audioSourceArray.Play(0); //Confirmation sound triggered here on collision
                    if (this.name != collidedProtein.name)
                    {

                        Debug.Log("Collided: " + this.name + " <-> " + collidedProtein.name);

                        if (this.transform.parent.name != groupName || collidedProtein.transform.parent.name != groupName)
                        {
                            // Find protein group
                            GameObject pGroup;

                            if (this.transform.parent.name == groupName)
                            {
                                pGroup = this.transform.parent.gameObject;
                            }
                            else if (collidedProtein.transform.parent.name == groupName)
                            {
                                pGroup = collidedProtein.transform.parent.gameObject;
                            }
                            else
                            {
                                pGroup = new GameObject(groupName);
                                pGroup.AddComponent<ProteinGroup>();
                            }

                            string inProteinGroup = "";
                            foreach (Transform c in pGroup.transform)
                                inProteinGroup += c.gameObject.name + ",";

                            Debug.Log("ProteinGroup: " + inProteinGroup);

                            if (!inProteinGroup.Contains(collidedProtein.name + ","))
                            {
                                // Attach proteins
                                FixedJoint joint = gameObject.AddComponent<FixedJoint>();
                                joint.anchor = collision.contacts[0].point;
                                joint.connectedBody = collision.contacts[0].otherCollider.transform.GetComponentInParent<Rigidbody>();
                                //rb.mass = (float) 0.0001;
                                //rb.freezeRotation = true;
                                joint.enableCollision = false;

                                pGroup.transform.parent = this.transform.parent;
                                gameObject.transform.parent = pGroup.transform;
                                collidedProtein.transform.parent = pGroup.transform;

                                // Reset timer


                                foreach (Transform c in pGroup.transform)
                                    ResetTimer(c.gameObject.GetComponent<Protein>());
                            }
                        }
                    }

                }

                else
                {
                    this.audioSourceArray.Play(1); //Discordant sound triggered here on collision
                }
            }
        }

     
    }

    void Bounce()
    {
      

        float x = gameObject.transform.position.x;
        float z = gameObject.transform.position.z;

        if (bounceDir == -1)
        {
            // Down
            if (this.transform.position.y <= minBounce)
            {
                bounceDir = 1;
                bounceTime = 1;
            }
            else
            {
                float y = (float)(this.transform.position.y + (0.5 * grav * bounceTime * bounceTime));
                this.transform.position = new Vector3(x, y, z);
            }
        }
        else
        {
            // Up
            if (this.transform.position.y >= maxBounce)
            {
                bounceDir = -1;
                bounceTime = 0;
            }
            else
            {
                float y = (float)(this.transform.position.y + (vel * Math.Exp(-bounceTime)));
                this.transform.position = new Vector3(x, y, z);
            }
        }
        bounceTime += Time.deltaTime;
        
        
    }


    void Start()
    {
        proteinGroups = new List<string>[] {
                            new List<string> {"GLP1R", "GDP", "GLP1", "G_PRO"},
                            new List<string> {"AC", "ATP", "G_ALP"},
                            new List<string> {"PKA_CAT", "PKA", "CAMP"}};

        

        //gameObject.transform.position = position;
        rb = GetComponent<Rigidbody>();
        rb.transform.position = this.pos;
        rb.transform.rotation = this.rot;
        rb.transform.localScale = this.scale;
        audioSourceArray = GetComponent<AudioSource>();

        //positionOffset = this.pos;

        /* collider = GetComponent<SphereCollider>();*/
        while (wall == null)
        {
            wall = GameObject.Find("Cylinder");
        }
        while (proteinManager == null)
        {
            proteinManager = GameObject.Find("Manager").GetComponent<ProteinManager>();
        }
        
        //floor = GameObject.Find("Floor");
        timer = selfTimer;

        Dissolve = GetComponent<Dissolve>();

    }

    // Update is called once per frame
    void Update()
    {

        if (proteinManager.PlayerIsHost())
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else if (!destroyed)
            {
                // Debug.Log("Protein timer ended: " + this.proteinID.ToString());
                proteinManager.destroyProtein(this.proteinID);
                destroyed = true;
            }

            if (!thrown)
            {
                Bounce();
            }
        }

    }

    public void setDynamics(Vector3 position, Quaternion rotation, Vector3 scale, double minBounce, double maxBounce)
    {
        this.pos = position;
        this.rot = rotation;
        this.scale = scale;
        this.minBounce = minBounce;
        this.maxBounce = maxBounce;
    }


    public void updateTransform(Vector3 pos, Quaternion rot)
    {
        if (!isBeingThrown) {

            if (!proteinManager.PlayerIsHost() && (rb.velocity.x != 0 || rb.velocity.y != 0 || rb.velocity.z != 0))
            {
                // if not the host, remove velocity from protein to avoid jitter
                //rb.velocity = new Vector3(0, 0, 0);
                
            }


            gameObject.transform.position = pos;
            gameObject.transform.rotation = rot;
        }

        /*gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, pos, 0.2f);
        gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, rot, 0.2f);*/

    }

  
}
