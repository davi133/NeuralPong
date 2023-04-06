using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class AllVsAllManager : MonoBehaviour
{
    //[SerializeField] bool loadOnGoing = false;

    [SerializeField] float scoreReward = 1;
    [SerializeField] float enemyScorePunition = 1;

    NeuralNetwork best;
    [SerializeField] string _theBest = "bestAvsA";

    NeuralNetwork[] _all_nets = new NeuralNetwork[64];
    //_points foi movido pra baixo pra ficar legal no inspector
    public bool[] finalizados;

    [SerializeField] Transform group1;
    [SerializeField] Transform group2;

    public List<TrainingUnit> _training_units1;
    public List<TrainingUnit> _training_units2;
    int totalTrainingUnits;
    int sizeOfUnitGroup;

    //logic variables
    [SerializeField] float timeScale = 10;
    [ReadOnly] public float actualTimeScale = 1;
    float lastTimeScale = 1;
    int currentRound = 1;
    [ReadOnly] public int runningMatches = 0;
    Vector2Int[] matchTableRight;
    Vector2Int[] matchTableLeft;
    
    [SerializeField] float[] _points = new float[64];

   
    void Start()
    {
        Application.runInBackground = true;
        Time.timeScale = timeScale;


        //_training_units = new List<TrainingUnit>(FindObjectsOfType<TrainingUnit>());
        //_training_units = _training_units.OrderBy(g => g.gameObject.name).ToList();

        _training_units1 = new List<TrainingUnit>(group1.GetComponentsInChildren<TrainingUnit>());
        _training_units1 = _training_units1.OrderBy(g => g.gameObject.name).ToList();
        _training_units2 = new List<TrainingUnit>(group2.GetComponentsInChildren<TrainingUnit>());
        _training_units2 = _training_units2.OrderBy(g => g.gameObject.name).ToList();
        sizeOfUnitGroup = _training_units1.Count;
        totalTrainingUnits = _training_units1.Count + _training_units2.Count;

        int index = 0;
        foreach (TrainingUnit tu in _training_units1)
        {
            tu.onEnd += endMatch;
            tu.index = index;
            index++;
        }
        foreach (TrainingUnit tu in _training_units2)
        {
            tu.index = index;
            index++;
            tu.onEnd += endMatch;
        }

        best = NeuralNetwork.fromFile("/AllvsAll/" + _theBest);

        _prepareBrawl(best);
    }

    private void Update()
    {
        if (lastTimeScale != timeScale)
        {
            if (runningMatches >= 32)
            {
                Time.timeScale = timeScale;
            }
            else
            {
                Time.timeScale = timeScale*2;
            }

            lastTimeScale = timeScale;
        }

        actualTimeScale = Time.timeScale;
    }

    void _prepareBrawl(NeuralNetwork champion)
    {
        Debug.Log("starting brawl");
        
       
        _all_nets = new NeuralNetwork[sizeOfUnitGroup * 2];
        _points = new float[sizeOfUnitGroup * 2];
        if (champion == null)
        {
            Debug.Log("champion is null");
            int i = 0;
            /*for (i = 0; i < 32; i++)
            {
                _all_nets[i] = new NeuralNetwork(new int[] { 11, 6, 6, 2 });
            }*/
            for (; i < _all_nets.Length; i++)
            {
                _all_nets[i] = new NeuralNetwork(new int[] { 6, 8, 8, 8, 8, 2 });
            }
        }
        else
        {
            Debug.Log("we have achampion");
            _all_nets[0] = champion;
            int i = 1;
            for (; i <= 10; i++)//10
            {
                _all_nets[i] = champion.mutate(mutation_degree:.1f, mutation_chance: .1f);
            }
            for (; i <= 25; i++)//15
            {
                _all_nets[i] = champion.mutate();
            }
            for (; i <= 35; i++)//10
            {
                _all_nets[i] = champion.mutate(mutation_chance:0.5f, mutation_degree: .5f);
            }
            for (; i < _all_nets.Length; i++)//28
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
        _points = new float[_all_nets.Length];
        currentRound = 0;


        
        _prepareAndStartDisputes();
    }

    void _prepareAndStartDisputes()
    {
        Time.timeScale = timeScale;
        finalizados = new bool[64];
        runningMatches = totalTrainingUnits;
        //doing 2 rounds at once
        currentRound++;
        matchTableRight = distributeOpponents(_all_nets.Length, currentRound);
        currentRound++;
        matchTableLeft = distributeOpponents(_all_nets.Length, currentRound);

        runningMatches = currentRound <= _all_nets.Length - 1? totalTrainingUnits: sizeOfUnitGroup;

        /*for (int i=0; i< matchTableRight.Length;i++)
        {
            Debug.Log(matchTableRight[i]);
            Debug.Log(matchTableRight[i]);
        }*/

        for (int i = 0; i < matchTableRight.Length; i++)
        {
            //Debug.Log(matchTableRight[i].x);
            //Debug.Log(matchTableRight[i].y);
            _training_units1[i].setNetworks(_all_nets[matchTableRight[i].x], _all_nets[matchTableRight[i].y]);
            _training_units1[i].StartTraining();
            _training_units1[i].setLabel($"{matchTableRight[i].x}({_points[matchTableRight[i].x]}) VS ({_points[matchTableRight[i].y]}){matchTableRight[i].y}");

        }
        if (currentRound <= _all_nets.Length - 1)
        {
            for (int i = 0; i < matchTableLeft.Length; i++)
            {
                _training_units2[i].setNetworks(_all_nets[matchTableLeft[i].x], _all_nets[matchTableLeft[i].y]);
                _training_units2[i].StartTraining();
                _training_units2[i].setLabel($"{matchTableLeft[i].x}({_points[matchTableLeft[i].x]}) VS ({_points[matchTableLeft[i].y]}){matchTableLeft[i].y}");

            }
        }
        else
        {
            Debug.Log("it is ending");
        }
        Debug.Log($"current rounds are :{currentRound - 1} and {currentRound}");
    }

    void endMatch(int i)
    {
        finalizados[i] = true;
        runningMatches--;
        if (runningMatches < 32)
        {
            Time.timeScale = timeScale * 2;
        }
        if (runningMatches == 0)
        {
            _collectResults();
        }
    }

    void _collectResults()
    {
        for (int i = 0; i < matchTableRight.Length; i++)
        {
            Vector2 pontos = _training_units1[i].result;
            _points[matchTableRight[i].x] += (pontos.x * scoreReward) - (pontos.y * enemyScorePunition);
            _points[matchTableRight[i].y] += (pontos.y * scoreReward) - (pontos.x * enemyScorePunition);
        }

        if (currentRound <= _all_nets.Length - 1)
        {
            for (int i = 0; i < matchTableLeft.Length; i++)
            {
                Vector2 pontos = _training_units2[i].result;
                _points[matchTableLeft[i].x] += (pontos.x * scoreReward) - (pontos.y * enemyScorePunition);
                _points[matchTableLeft[i].y] += (pontos.y * scoreReward) - (pontos.x * enemyScorePunition);
            }
        }
            


        if (currentRound >= _all_nets.Length - 1)
        {
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
            if (bestIndex !=0)
            {
                Debug.Log($"0 had the score {_points[0]}=======================");
            }
            best = _all_nets[bestIndex];
            best.saveToFile("/AllvsAll/" +_theBest);
            int generationCount = 1;
            if (File.Exists(Application.streamingAssetsPath + "/NNs/AllvsAll/" + "generationCount"))
            {
                string count_s = File.ReadAllText(Application.streamingAssetsPath + "/NNs/AllvsAll/" + "generationCount");
                generationCount = int.Parse(count_s) + 1;
            }
            File.WriteAllText(Application.streamingAssetsPath + "/NNs/AllvsAll/" + "generationCount", generationCount.ToString());


            _prepareBrawl(best);
        }
        else
        {
            _prepareAndStartDisputes();
        }

    }

    /// <summary>
    /// <para>
    ///     Distributes opponents in a brawl where everyone plays against everyone at least and only once
    /// </para>
    /// <para>
    ///     Parameters:<br></br>
    ///     <paramref name="quantity"/>
    ///     <param name="quantity">Number of entities playing [unfortunely, must be a power of 2 (e.g 2,4,8...)]</param>
    ///     <br></br>
    ///     <paramref name="round"/>
    ///     <param name="round">The current round of the brawl (is always less than <paramref name="quantity"/>)</param>
    /// </para>
    /// <para>
    ///     <returns>
    ///     Returns: A Vector2Int list where each: <br></br>
    ///     index-> index of the match       <br></br>
    ///     .x-> index of entity A           <br></br>
    ///     .y-> index of the aversary for A <br></br>
    ///     </returns>
    /// </para>
    /// </summary>
    Vector2Int[] distributeOpponents(int quantity, int round)
    {
        Vector2Int[] matchList = new Vector2Int[quantity / 2];
        int selector = round;
        if (round % 2 != 0) selector++;
        //Debug.Log($"selector is { selector}");

        int arenaIndex = 0;
        int flipperAux = round % 2 == 0 ? -1 : 1;
        int offsetAux = (selector / 2);
        //Debug.Log($"offsetAux is { offsetAux*flipperAux}");
        for (int challanger = 0; challanger < quantity; challanger++)
        {
            //Debug.Log($"matches made: {arenaIndex}");
            //Debug.Log($"for {challanger}: challanger % selector  {challanger % selector}, so {challanger % selector < selector / 2}");
            //this should only be true for halfe of the challangers
            if (challanger % selector < selector / 2)
            {
                //this calculates who the challanger will face
                //the flipperAux is needed to work
                int offset = offsetAux * flipperAux;
                //Debug.Log($"offset is {offset}");

                //now imagine a line with all challangers
                //challanger + offset < 0 means the challanger faces someone in the other end of the line
                if (challanger + offset < 0)
                {
                    offset += quantity;
                }
                //Debug.Log($"so challanging {challanger + offset}");

                //this also means the challanger will face someone from the other end of the line
                //but if it is in the ascending direction the challanger is actually the challanged one
                //from the other end
                if ((challanger + offset) < (quantity))
                {

                    matchList[arenaIndex].x = challanger;
                    matchList[arenaIndex].y = challanger + offset;
                    arenaIndex++;
                }
                else
                {
                    //Debug.Log($"but,(challanger + offset) is {(challanger + offset)}");
                    //Debug.Log($"and {(challanger + offset)} is bigger than {quantity-1} as you can see {(challanger + offsetAux * flipperAux) > (quantity - 1)}");
                }
                //flipper aux needs to be flipped because I said so
                flipperAux *= -1;
            }
            else
            {
                //resetting the flipper for the next cluster of selected challangers in the line
                flipperAux = round % 2 == 0 ? -1 : 1;
            }
        }

        return matchList;

    }

  
}

