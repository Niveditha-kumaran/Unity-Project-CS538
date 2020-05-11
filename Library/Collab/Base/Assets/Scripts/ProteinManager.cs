//using Mixed
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Academy.HoloToolkit.Unity;
using Academy.HoloToolkit.Sharing;
using System.Threading.Tasks;
using System;

public class ProteinManager : MonoBehaviour
{

    [SerializeField]
    private float spawnDelay;

    [SerializeField]
    public Protein GLP1R;
    public Protein AC;
    public Protein ATP;
    public Protein CAMP;
    public Protein GDP;
    public Protein GLP1;
    public Protein G_PRO;
    public Protein PKA_CAT;
    public Protein PKA;
    public Protein G_ALP;

    [SerializeField]
    public GameObject hologramCollection;
    public GameObject proteinParent;
    public GameObject playSpace;
    private double playSpaceHeight;

    [SerializeField]
    public int maxProteinCount;
    public int numPlayers;

    [SerializeField]
    public GameObject[] players;

    private Rigidbody newProteinRb;
    private float[] timers;

    private Dictionary<string, Vector3[]> proteinScales;
    private Protein[] proteinPrefabs;
    private string[] proteinNames;
    private System.Random rand;
    private int proteinIndex = 0;
    private int[] randomProteinIndex;
    [SerializeField]
    public int proteinVersion;

    private static ProteinManager _instance;
    public static ProteinManager Instance => _instance;

    private Dictionary<int, Protein> proteinDict;
    public long hostID = -1;
    private static float hostMessageDelay = 1.0f;
    private float waitForHost = 10.0f;
    private float hostMessageTimer = hostMessageDelay;

    private AudioSource spawnSound;

    CustomMessages customMessages;
    private void Awake()
    {
        if (_instance != null)
            Destroy(gameObject);
        else
            _instance = this;
    }

    void Start()
    {

        //hologramCollection = GameObject.Find("HologramCollection");
        // MixedRealityToolkit.SpatialAwarenessSystem.Disable();
        this.rand = new System.Random();
        this.timers = new float[numPlayers];
        this.proteinDict = new Dictionary<int, Protein>();
        this.spawnSound = GetComponent<AudioSource>();
        
        // Get play space height to calculate bouncing bound from play space
        playSpaceHeight = playSpace.GetComponent<Collider>().bounds.size.y;

        // GROUP A: GLP1, GLP1-R, GDP, G Protein
        // GROUP B: AC, G Alpha, ATP
        // GROUP C: PKA (Regulatory), PKA (Catalytic), cAMP

        proteinPrefabs = new Protein[] { GLP1R, AC, ATP, CAMP, GDP, GLP1, G_PRO, PKA_CAT, PKA, G_ALP };
        proteinNames = new string[] { "GLP1R", "AC", "ATP", "CAMP", "GDP", "GLP1", "G_PRO", "PKA_CAT", "PKA", "G_ALP" };

        proteinScales = new Dictionary<string, Vector3[]>();
        proteinScales.Add("GLP1R", new Vector3[] { new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0, 0, 0) });
        proteinScales.Add("AC", new Vector3[] { new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0, 0, 0) });
        proteinScales.Add("ATP", new Vector3[] { new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0, 0, 0) });
        proteinScales.Add("CAMP", new Vector3[] { new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0, 0, 0) });
        proteinScales.Add("GDP", new Vector3[] { new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0, 0, 0) });
        proteinScales.Add("GLP1", new Vector3[] { new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0, 0, 0) });
        proteinScales.Add("G_PRO", new Vector3[] { new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0, 0, 0) });
        proteinScales.Add("PKA_CAT", new Vector3[] { new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0, 0, 0) });
        proteinScales.Add("PKA", new Vector3[] { new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0, 0, 0) });
        proteinScales.Add("G_ALP", new Vector3[] { new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0, 0, 0) });


        // custom messages

        customMessages = CustomMessages.Instance;
        customMessages.MessageHandlers[CustomMessages.TestMessageID.ProteinTransform] = this.UpdateProteinTransform;
        customMessages.MessageHandlers[CustomMessages.TestMessageID.HostID] = this.UpdateHostID;
        customMessages.MessageHandlers[CustomMessages.TestMessageID.DestroyProtein] = this.UpdateDestroyProtein;
        customMessages.MessageHandlers[CustomMessages.TestMessageID.Throw] = this.UpdateThrow;

        SharingSessionTracker.Instance.SessionJoined += Instance_SessionJoined;
        SharingSessionTracker.Instance.SessionLeft += Instance_SessionLeft;

        //Task.Delay(2500).Wait(); // sleep for two seconds, if we havent received hostid in this time then we become the host

    }

    private void Instance_SessionLeft(object sender, SharingSessionTracker.SessionLeftEventArgs e)
    {
        return;
        //if (remoteHeads.ContainsKey(e.exitingUserId))
        //{
        //    RemoveRemoteHead(this.remoteHeads[e.exitingUserId].HeadObject);
        //    this.remoteHeads.Remove(e.exitingUserId);
        //}
    }

    // <summary>
    // Called when a user is joining.
    // </summary>
    // <param name="sender"></param>
    // <param name="e"></param>
    private void Instance_SessionJoined(object sender, SharingSessionTracker.SessionJoinedEventArgs e)
    {
        if (PlayerIsHost() && waitForHost <= 0)
        {
            GameObject.Find("1").transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            customMessages.SendHostID();
        }
       
    }


    // Update is called once per frame
    void Update()
    {

        // Debug.Log("userid " + GetUserID().ToString());

        if (hostID == -1)
        {
            if (waitForHost > 0)
            {
                waitForHost -= Time.deltaTime;
                return;
            }
            else
            {
                hostID = GetUserID();
                Debug.Log("host set to " + hostID.ToString());
            }
        }

        else if (PlayerIsHost()) {

            for (int i=0; i<timers.Length; i++)
            {
                if (timers[i] <= 0 && (proteinParent.transform.childCount < maxProteinCount))
                {
                    int id = rand.Next(0, 100000000);

                    //make sure key isnt being used
                    while (proteinDict.ContainsKey(id))
                    {
                        id = rand.Next(0, 100000000);
                    }

                    proteinDict.Add(id, initProtein(players[i].transform.position, players[0].transform.rotation));
                    proteinDict[id].proteinID = id;
                    
                    timers[i] = spawnDelay;
                   // customMessages.SendProteinTransform(id, proteinDict[id].transform.position, proteinDict[id].transform.rotation, proteinDict[id].name);

                    //Debug.Log("spawned " + id.ToString() + "from update");

                }
                else
                {
                    timers[i] -= Time.deltaTime;
                }

            }

            foreach (KeyValuePair<int, Protein> protein in proteinDict) {
                
                if(protein.Value == null)
                {
                    proteinDict.Remove(protein.Key);
                }
                else /*if (ImportExportAnchorManager.Instance.AnchorEstablished)*/
                {
                    Transform proteinTransform = protein.Value.transform;
                    Vector3 pos = hologramCollection.transform.InverseTransformPoint(proteinTransform.position);
                    Quaternion rot = Quaternion.Inverse(hologramCollection.transform.rotation) * proteinTransform.rotation;
                   
                    customMessages.SendProteinTransform(protein.Key, pos, rot, protein.Value.name);
                }

            }

        }   

    }

    private void UpdateProteinTransform(NetworkInMessage msg)
    {
        long userID = msg.ReadInt64();

        
        int proteinID = msg.ReadInt32();
        string name = msg.ReadString();
        Vector3 pos = customMessages.ReadVector3(msg);
        Quaternion rot = customMessages.ReadQuaternion(msg);

        if (PlayerIsHost())
        {
        
            return;
        
        }

        if (this.proteinDict.ContainsKey(proteinID))
        {
            // pos = pos + rot * proteinDict[proteinID].transform.localPosition;
            pos = hologramCollection.transform.TransformPoint(pos);
            rot = hologramCollection.transform.rotation * rot;
            this.proteinDict[proteinID].updateTransform(pos, rot);
        }
        else
        {
            // new protein spawned
            proteinDict.Add(proteinID, initProtein(pos, rot, name));
            proteinDict[proteinID].proteinID = proteinID;
        }

    }

    private void UpdateHostID(NetworkInMessage msg)
	{
        GameObject.Find("1").transform.localScale = new Vector3(5, 5, 5);
        long userID = msg.ReadInt64();
        this.hostID = userID;
        Debug.Log("host ID: " + userID.ToString());
    }

    private void UpdateThrow(NetworkInMessage msg)
    {

        if (PlayerIsHost())
        {
            long userID = msg.ReadInt64();
            int id = msg.ReadInt32();
            Vector3 vel = customMessages.ReadVector3(msg);


            if (proteinDict.ContainsKey(id))
            {
                proteinDict[id].thrown = true;
                Vector3 newVel = hologramCollection.transform.TransformVector(vel);
                proteinDict[id].rb.velocity = newVel;
            }
            else
            {
                Debug.Log("trying to throw protein " + id.ToString() + " but it doesnt exist");
            }
        }
    }

    private void UpdateDestroyProtein(NetworkInMessage msg)
    {
        long userID = msg.ReadInt64();
        int id = msg.ReadInt32();
        DissolveAndDestroy(id);
    }

    public void SendThrow(int proteinID, Vector3 vel)
    {
        if (PlayerIsHost())
        {
            return;
        }

        Protein protein;
        if (proteinDict.TryGetValue(proteinID, out protein))
        {
            Vector3 transformedVel = hologramCollection.transform.InverseTransformVector(vel);
            customMessages.SendThrow(proteinID, transformedVel);
        }
        else
        {
            Debug.Log("trying to throw protein " + proteinID.ToString() + " but it doesnt exist");
        }
        
    }

    private int[] generateRandomInt()
    {

        int[] array = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        int randNumber;
        int tmp;

        for (int i = 0; i < 10; i++)
        {
            randNumber = rand.Next(i, 10);
            tmp = array[i];
            array[i] = array[randNumber];
            array[randNumber] = tmp;
        }

        return array;
    }

    private Protein initProtein(Vector3 position, Quaternion rotation, string name="")
    {

        Protein newProtein;

        if (name.Equals(""))
        {
            if (proteinIndex == 0)
            {
                randomProteinIndex = generateRandomInt();
            }
            newProtein = Instantiate(proteinPrefabs[randomProteinIndex[proteinIndex]]);
            newProtein.name = proteinNames[randomProteinIndex[proteinIndex]];
        }

        else
        {
            int index = System.Array.IndexOf(proteinNames, name);
            newProtein = Instantiate(proteinPrefabs[index]);
            newProtein.name = name;
        }

        this.spawnSound.Play(0);
        newProtein.transform.parent = proteinParent.transform;
        newProtein.setDynamics(position,
                               rotation,
                               proteinScales[newProtein.name][proteinVersion],
                               playSpace.transform.position.y - playSpaceHeight / 2,
                               playSpace.transform.position.y + playSpaceHeight / 2);

        if (name.Equals(""))
        {
            proteinIndex = (proteinIndex + 1) % proteinPrefabs.Length;
        }

        
        return newProtein;
    }

    private long GetUserID()
	{
        try
        {
        return SharingStage.Instance.Manager.GetLocalUser().GetID();
        }
        catch
        {
            return 111;
        }
    }

    public bool PlayerIsHost()
    {
        return GetUserID() == hostID;
    }

    public void destroyProtein(int id)
    {
        // sends destroy message and dissolves protein
        // ONLY CALL THIS IF YOU ARE THE HOST

        Debug.Assert(PlayerIsHost());

        customMessages.SendDestroyProtein(id);
        DissolveAndDestroy(id);
    }

    private void DissolveAndDestroy(int id)
    {
        Protein temp;

        if (proteinDict.TryGetValue(id, out temp))
        {
            proteinDict.Remove(id);
            if (temp != null)
            {   
                temp.Dissolve.enabled = true;
            }

        }
    }
}