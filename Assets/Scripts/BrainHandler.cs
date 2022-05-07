using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrainHandler : MonoBehaviour
{
    public List<GameObject> brainModels;
    public int modelIndex = 0;
    public int spawnCount = 10;
    public float mutationRate = 0.1f;
    public bool shallHitOthers = false;
    public Vector3 spawnPoint = new Vector3(0, 0, 0);
    public bool resetAutomatically = true;
    public float checkReadyInterval = 0.5f;
    private float lastCheckTime = 0;
    public float updateCamObjectInterval = 0.1f;
    private float lastCamUpdate = 0;
    public int moveAddPerRound = 5;
    public int copyBestCountAfterRound = 2;
    public float timeScale = 1;

    private List<Brain> brains = new List<Brain>();
    private FollowObject camFollowProg;
    // Start is called before the first frame update
    void Start()
    {
        camFollowProg = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowObject>();
        List<GameObject> brainObjects = new List<GameObject>();
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject brainObj = Instantiate(brainModels[modelIndex]);
            brainObj.transform.parent = this.transform;
            brainObj.transform.localPosition = spawnPoint;
            brainObjects.Add(brainObj);
            Brain brain = brainObj.GetComponent<Brain>();
            brains.Add(brain);
            brain.mutationRate = mutationRate;
            brain.minThinkTime = 1;
            brain.moveOnEmptyOnly = true;
            if (brain.moveOnEmptyOnly)
                brain.Restart();
        }
        if (!shallHitOthers)
            for (int i = 0; i < brainObjects.Count; i++)
            {
                Collider collider1 = brainObjects[i].GetComponent<Collider>();
                for (int j = 0; j < brainObjects.Count; j++)
                {
                    if (i == j) continue;
                    Collider collider2 = brainObjects[j].GetComponent<Collider>();
                    Physics.IgnoreCollision(collider1, collider2);
                }
            }
    }

    // Update is called once per frame
    void Update()
    {
        if(lastCheckTime + checkReadyInterval < Time.time)
        {
            Check();
            if (timeScale <= 0)
                timeScale = 1;
            Time.timeScale = timeScale;
            lastCheckTime = Time.time;
        }
        if(lastCamUpdate + updateCamObjectInterval < Time.time)
        {
            CalcFitnessAndSaveInBot();
            SortBotsByFitness();
            SetCamFollowObject();
            lastCamUpdate = Time.time;
        }
    }

    void Check()
    {
        if (!isEveryoneStopped()) return;
        CalcFitnessAndSaveInBot();
        SortBotsByFitness();
        ReplaceWithBest(copyBestCountAfterRound);
        AddMovesToBrains(moveAddPerRound);
        Mutate();
        RestartAllBrains();

    }

    public void SetCamFollowObject()
    {
        camFollowProg.objectToFollow = brains[0].gameObject;
    }
    public void RestartAllBrains()
    {
        foreach (Brain brain in brains)
        {
            brain.Restart();
        }
    }

    public void AddMovesToBrains(int movesToAdd)
    {
        foreach(Brain brain in brains)
        {
            brain.maxMoves += movesToAdd;
        }
    }

    private void CalcFitnessAndSaveInBot()
    {
        //Calc x distance from the destination to the bot
        GameObject end = GameObject.FindGameObjectWithTag("End");
        for(int i = 0; i < brains.Count; i++)
        {
            GameObject brainObj = brains[i].gameObject;
            float perc = end.transform.position.x / brainObj.transform.position.x;
            brains[i].fitness = perc;
        }
    }

    private void SortBotsByFitness()
    {
        brains = QuickSortByFitness(brains);
    }

    List<Brain> QuickSortByFitness(List<Brain> lst)
    {
        if (lst.Count <= 1)
            return lst;
        int pivotIndex = lst.Count / 2;
        Brain pivot = lst[pivotIndex];
        List<Brain> left = new List<Brain>();
        List<Brain> right = new List<Brain>();

        for (int i = 0; i < lst.Count; i++)
        {
            if (i == pivotIndex) continue;

            if (lst[i].fitness <= pivot.fitness)
            {
                left.Add(lst[i]);
            }
            else
            {
                right.Add(lst[i]);
            }
        }

        List<Brain> sorted = QuickSortByFitness(left);
        sorted.Add(pivot);
        sorted.AddRange(QuickSortByFitness(right));
        return sorted;
    }

    private void ReplaceWithBest(int bestcount)
    {
        for(int i = bestcount; i < brains.Count; i++)
        {
            Brain current = brains[i];
            Brain currentBest = brains[i % bestcount];
            List<BrainMovement> cloned = new List<BrainMovement>();
            foreach(BrainMovement bm in currentBest.brainMovements)
            {
                cloned.Add(bm.Clone());
            }
            current.brainMovements = cloned;
        }
    }

    private void Mutate()
    {
        foreach (Brain brain in brains)
        {
            //Mutates and resets the current brain
            brain.Mutate();
        }
    }

    bool isEveryoneStopped()
    {
        foreach (Brain brain in brains)
        {
            if (!brain.stopped)
                return false;
        }
        return true;
    }
}
