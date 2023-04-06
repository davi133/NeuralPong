using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ChampionshipManager : MonoBehaviour
{
    public List<TrainingUnit> active_units;
    public List<TrainingUnit> all_units;
    NeuralNetwork best = null;
    public string theBest = "_theBest";
    bool onGoing = false;


    NeuralNetwork[] netList;
    NeuralNetwork[] winners;
    void Start()
    {
        Application.runInBackground = true;
        Time.timeScale = 8;


        best = NeuralNetwork.fromFile(theBest);
       
        //collecting all training units
        all_units = new List<TrainingUnit>(FindObjectsOfType<TrainingUnit>());
        all_units = all_units.OrderBy(g => g.gameObject.name).ToList();
        active_units = all_units.FindAll(u => u.gameObject.activeSelf);

        _prepareChampionship(best);
        
        

    }

    // Update is called once per frame
    void Update()
    {
        bool disputesEnded = true;
        foreach (TrainingUnit tu in active_units)
        {
            if (!tu.ended)
            {
                disputesEnded = false;
            }
        }

        if(disputesEnded && onGoing)
        {
            onGoing = false;
            _separateWinners();
        }
    }



    void _prepareChampionship(NeuralNetwork best)
    {
        netList = new NeuralNetwork[32];
        winners = new NeuralNetwork[16];
        onGoing = false;
        foreach (TrainingUnit tu in all_units)
        {
            tu.gameObject.SetActive(true);
        }
        active_units = new List<TrainingUnit>(all_units);

        if (best == null)
        {
            Debug.Log("best is null");
            for (int i = 0; i < netList.Length; i++)
            {
                netList[i] = new NeuralNetwork(new int[] { 8, 5, 2 });
            }
        }
        else
        {
            netList[0] = best;
            int i = 1;
            for (i = 1; i < 10; i++)
            {
                netList[i] = best.mutate(mutation_chance:0.5f,mutation_degree:0.5f);
            }
            for (; i <= 15; i++)
            {
                netList[i] = best.mutate();
            }
            for (;i< netList.Length;i++)
            {
                netList[i] = new NeuralNetwork(new int[] { 8, 5, 2 });
            }
        }
        

        _startDisputes();
    }

    void _startDisputes()
    {
        Debug.Log($"starting dispute with: {netList.Length}");
        onGoing = true;
        int nnIndex = 0;
        netList = netList.OrderBy(g => UnityEngine.Random.value).ToArray();
        foreach (TrainingUnit tu in active_units)
        {
            if (nnIndex < netList.Length)
            {
                if (netList[nnIndex] == null || netList[nnIndex + 1] == null)
                    Debug.Log("null null null null null null null null null null null null ");

                tu.setNetworks(netList[nnIndex], netList[nnIndex + 1]);
                tu.StartTraining();
                nnIndex += 2;
            }
            else
            { 
                tu.gameObject.SetActive(false);
               
            }
        }
        active_units = all_units.FindAll(u => u.gameObject.activeSelf);
        //Debug.Log($"active arenas: {active_units.Count}");
    }

    void _separateWinners()
    {
        onGoing = false;
        winners = new NeuralNetwork[netList.Length / 2];

        int index = 0;
        foreach (TrainingUnit tu in active_units)
        {
            
            winners[index++] = tu.winner.brain;
            
        }


        foreach (NeuralNetwork nn in winners)
        {
            if (nn == null)
                Debug.Log("net is null");
            else
            {
                //Debug.Log("not null");
            }
        }

        if (winners.Length == 1)
        {
            winners[0].saveToFile(theBest);
            _prepareChampionship(winners[0]);
        }
        else
        {
            netList = winners;

            //Debug.Log($"calling dispute with {netList.Length}");
            _startDisputes();
        }
    }
}
