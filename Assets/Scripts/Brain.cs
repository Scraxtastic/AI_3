using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    public bool alive = true;
    public bool finsihed = false;
    //currently unused
    public bool stopped = false;
    public float speed = 1.0f;
    public int maxMoves = 10;
    public bool moveOnEmptyOnly = false;
    public bool hasExisting = false;
    public int currentMove = 0;
    public float mutationRate = 0.1f;
    public float fitness = -100;
    public float minThinkTime = .5f;
    private float lastThinkTime = 0;
    public List<BrainMovement> brainMovements = new List<BrainMovement>();
    private Vector3 startPosition;

    private BotMovement botMovement;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = this.transform.position;
    }

    void Awake()
    {
        botMovement = GetComponent<BotMovement>();
        botMovement.ConsumedItem += OnItemConsumed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!moveOnEmptyOnly)
            TransmitMovement();
    }

    void OnItemConsumed(int lasting)
    {
        if (moveOnEmptyOnly && lasting == 0)
            TransmitMovement(false);
    }

    private void TransmitMovement(bool useThinkTime = true)
    {
        if (stopped)
            return;
        if (useThinkTime && lastThinkTime + minThinkTime < Time.time)
            return;
        if (brainMovements.Count < maxMoves)
        {
            if (hasExisting)
            {
                if (brainMovements.Count <= currentMove)
                {
                    hasExisting = false;
                }
                else
                {
                    botMovement.AddMovementToQueue(brainMovements[currentMove]);
                    currentMove++;
                }
            }
            if (!hasExisting)
            {
                botMovement.AddMovementToQueue(CreateAndAddBrainMovement());
                currentMove++;
            }
        }
        else
        {
            stopped = true;
        }
        lastThinkTime = Time.time;
    }
    public void Mutate()
    {
        Reset();
        float maxRandom = 100000;
        float randomNormalized = Random.Range(0, maxRandom) / maxRandom;
        if (randomNormalized < mutationRate)
        {
            for (int i = 0; i < brainMovements.Count; i++)
            {
                randomNormalized = Random.Range(0, maxRandom) / maxRandom;
                if (randomNormalized < mutationRate)
                {
                    brainMovements[i] = CreateBrainMovement();
                }
            }
        }
    }

    public void Restart()
    {
        stopped = false; 
        if (moveOnEmptyOnly)
            TransmitMovement(false);
    }
    private void Reset()
    {
        alive = true;
        finsihed = false;
        hasExisting = true;
        currentMove = 0;
        this.transform.position = startPosition;
        botMovement.ResetSpeed();
    }

    BrainMovement CreateAndAddBrainMovement()
    {
        BrainMovement bm = CreateBrainMovement();
        brainMovements.Add(bm);
        return bm;
    }

    BrainMovement CreateBrainMovement()
    {
        return new BrainMovement(RandomBool(), RandomBool(), RandomBool());
    }

    public bool RandomBool()
    {
        return Random.Range(0, 2) == 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Death")
        {
            alive = false;
            stopped = true;
        }
        if (collision.gameObject.tag == "End")
        {
            finsihed = true;
            stopped = true;
        }
    }
}
