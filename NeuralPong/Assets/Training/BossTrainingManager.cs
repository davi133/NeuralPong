using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class BossTrainingManager : MonoBehaviour
{
    //[SerializeField] bool loadOnGoing = false;

    [SerializeField] float scoreReward = 1;
    [SerializeField] float enemyScorePunition = 1;
    [SerializeField] float scoreToPromotion = 25;

    NeuralNetwork best;
    NeuralNetwork boss;
    [SerializeField] string _theBest = "bestAss1";

    NeuralNetwork[] _all_nets = new NeuralNetwork[64];
    //_points foi movido pra baixo pra ficar legal no inspector



    public List<TrainingUnit> _training_units;


    //logic variables
    [SerializeField] float timeScale = 10;
    [SerializeField] float timeToNext = 0;
    float lastTimeScale = 1;
    [ReadOnly] public int runningMatches = 0;

    [SerializeField] float[] _points = new float[64];


    void Start()
    {
        Application.runInBackground = true;
        Time.timeScale = timeScale;


        //_training_units = new List<TrainingUnit>(FindObjectsOfType<TrainingUnit>());
        //_training_units = _training_units.OrderBy(g => g.gameObject.name).ToList();

        _training_units = new List<TrainingUnit>(FindObjectsOfType<TrainingUnit>());
        _training_units = _training_units.OrderBy(g => g.gameObject.name).ToList();


        int index = 0;
        foreach (TrainingUnit tu in _training_units)
        {
            tu.onEnd += endMatch;
            tu.index = index;
            index++;
        }


        best = NeuralNetwork.fromFile("/Assisted/" + _theBest);
        if (best == null) Debug.Log("best is truly null");
        boss = NeuralNetwork.fromFile("/Assisted/Boss");
        Debug.Log($"we have a boss {boss.Layers_Sizes[0]}");
        _prepareBrawl(best);
    }

    private void Update()
    {
        if (lastTimeScale != timeScale)
        {
            Time.timeScale = timeScale;
            lastTimeScale = timeScale;
        }
    }

    void _prepareBrawl(NeuralNetwork champion)
    {
        if (boss == null) throw new FileNotFoundException("É preciso ter um boss para começar");

        _all_nets = new NeuralNetwork[_training_units.Count];
        _points = new float[_all_nets.Length];
        if (champion == null)
        {
            Debug.Log("starting brawl - champion is null");
            for (int i = 0; i < _all_nets.Length; i++)
            {
                _all_nets[i] = new NeuralNetwork(new int[] { 6, 8, 8, 8, 8, 2 });
            }
        }
        else
        {
            Debug.Log("starting brawl - we have achampion");
            _all_nets[0] = champion;//1
            int i = 1;
            for (; i <= 15; i++)//15
            {
                _all_nets[i] = champion.mutate(mutation_degree: .1f, mutation_chance: .1f);
            }
            for (; i <= 35; i++)//20
            {
                _all_nets[i] = champion.mutate();
            }
            for (; i <= 50; i++)//15
            {
                _all_nets[i] = champion.mutate(mutation_chance: 0.5f, mutation_degree: .5f);
            }
            for (; i < _all_nets.Length; i++)//13
            {
                _all_nets[i] = new NeuralNetwork(new int[] { 6, 8, 8, 8, 8, 2 });
            }
            /*for (; i < 44; i++)
            {
                _all_nets[i] = new NeuralNetwork(new int[] { 11, 6, 6, 2 });
            }*/
            /*for (; i < _all_nets.Length; i++)
            {
                _all_nets[i] = new NeuralNetwork(new int[] { 11, 8, 8, 8, 2 });
            }*/

        }

        _prepareAndStartDisputes();
    }

    void _prepareAndStartDisputes()
    {

        runningMatches = _training_units.Count;

        for (int i = 0; i < _training_units.Count; i++)
        {
            _training_units[i].setNetworks(_all_nets[i], boss);
            _training_units[i].StartTraining();
            _training_units[i].setLabel($"{i} VS Boss");

        }
    }

    void endMatch(int index)
    {
        runningMatches--;

        Vector2 pontos = _training_units[index].result;
        _points[index] = (pontos.x * scoreReward) - (pontos.y * enemyScorePunition);

        if (runningMatches == 0)
        {
            _collectResults();
        }
    }


    void _collectResults()
    {
        for (int i = 0; i < _training_units.Count; i++)
        {
            Vector2 pontos = _training_units[i].result;
            _points[i] = (pontos.x * scoreReward) - (pontos.y * enemyScorePunition);

        }



        int bestIndex = -1;
        float bestResult = -999999;
        for (int i = 0; i < _all_nets.Length; i++)
        {
            if (_points[i] > bestResult)
            {
                bestIndex = i;
                bestResult = _points[i];
            }
        }


        Debug.Log($"the winner is {bestIndex} with profit of {bestResult}=======================");
        if (bestIndex != 0)
        {
            Debug.Log($"0 had the score {_points[0]}=======================");
        }
        if (bestResult == 0)
        {
            Debug.Log("quee???");
            Debug.Break();
        }
        best = _all_nets[bestIndex];
        best.saveToFile("/Assisted/" + _theBest);


        if (bestResult > scoreToPromotion)
        {
            best.saveToFile("/Assisted/Boss");
            boss = best;
            Debug.Log("the boss is dead, long live the boss");
        }
        else
        {
            Debug.Log("ainda sem um novo boss");
        }



        //updanting generation counter
        int generationCount = 1;
        if (File.Exists(Application.streamingAssetsPath + "/NNs/Assisted/" + "generationCount"))
        {
            string count_s = File.ReadAllText(Application.streamingAssetsPath + "/NNs/Assisted/" + "generationCount");
            generationCount = int.Parse(count_s) + 1;
        }
        File.WriteAllText(Application.streamingAssetsPath + "/NNs/Assisted/" + "generationCount", generationCount.ToString());


        StartCoroutine(waitToStart(timeToNext));


    }
    IEnumerator waitToStart(float time)
    {
        yield return new WaitForSeconds(time * timeScale);
        _prepareBrawl(best);
    }

}
