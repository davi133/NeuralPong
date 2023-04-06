using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class debugNN : MonoBehaviour
{
    public TextMeshProUGUI deb;
    NeuralNetwork nn;

    void Start()
    {
       
        deb = GetComponent<TextMeshProUGUI>();
        deb.SetText("asdasda\nasdasd"); 
        NeuralNetwork nn = new(new int[]{2,2,2 });
        NeuralNetwork nn2 = nn.mutate(mutation_chance:0.9f,mutation_degree:0.5f);


        float[] res1 = nn.feedInputs(new float[]{1f,0.5f});
        float[] res2 = nn2.feedInputs(new float[] { 1f, 0.5f });

        Debug.Log(arrayToString(res1));
        Debug.Log(arrayToString(res2)); 
        
        string str = nn.ToString() +"\n";
        str += nn2.mutate();
        deb.SetText($"{str}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    string arrayToString(float[]a)
    {
        string str = "[";
        foreach(float v in a)
        {
            str += v + ",";
        }
        str += "]";
        return str;
    }
}
