using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class NeuralNetwork
{
    private int[] layers_sizes;
    public int[] Layers_Sizes
    {
        get
        {
            return layers_sizes;
        }
    }
    private float[] weight_range = { -1, 1 };
    private float[][][] weights;

    public NeuralNetwork(int[] layers_sizes)
    {
        this.layers_sizes = layers_sizes;


        //INITIALIZIN RANDOM WEIGHTS
        this.weights = new float[layers_sizes.Length - 1][][];

        //iterating for every layer (except input layer, which don't have weights)
        for (int i = 1; i <= layers_sizes.Length - 1; i++)
        {

            int size_of_this_layer = layers_sizes[i];
            float[][] layer = new float[size_of_this_layer][];

            //iterating for every neuron in the layer
            for (int j = 0; j < size_of_this_layer; j++)
            {
                int size_of_previous_layer = layers_sizes[i - 1];

                float[] neuron = new float[size_of_previous_layer + 1];//+1 is for bias

                //generating every weight on a neuron
                for (int k = 0; k < neuron.Length; k++)
                {
                    neuron[k] = Random.Range(weight_range[0], weight_range[1]);

                }
                layer[j] = neuron;
            }


            weights[i - 1] = layer;
        }


    }

    //TODO
    public static NeuralNetwork fromFile(string filename)
    {
        if (!File.Exists(Application.streamingAssetsPath + "/NNs/" + filename))
        {   
            return null;
        }
        
        NeuralNetwork retorno;
        string[] lines;
        lines = File.ReadAllLines(Application.streamingAssetsPath + "/NNs/" + filename);
        lines = lines.Where(val => val[0] != '#').ToArray();
        
        
        //init sizes
        string[] str_sizes = lines[0].Split(";");
        int [] layers_sizes = new int[str_sizes.Length];
        for (int i =0; i< layers_sizes.Length;i++)
        {
            layers_sizes[i] = int.Parse(str_sizes[i]);
        }
        retorno = new NeuralNetwork(layers_sizes);
        
        //init ranges
        string[] str_range = lines[1].Split(";");
        float []weight_range = new float[2];
        weight_range[0] = float.Parse(str_range[0]);
        weight_range[1] = float.Parse(str_range[1]);
        retorno.weight_range = weight_range;

        //skip sizes and range lines
        lines = lines.Skip(2).ToArray();
        
        //get all the weights
        List<float> weight_list = new List<float>();
        foreach (string line in lines)
        {
            string[] wei_str = line.Split(';');
            foreach(string wei in wei_str)
            {
                //Debug.Log(wei);
                weight_list.Add(float.Parse(wei));
            }


        }
        
        //INITIALIZIN WEIGHTS
        float [][][] weights = new float[layers_sizes.Length - 1][][];
        int weight_list_index = 0;
        //iterating for every layer (except input layer, which don't have weights)
        for (int i = 1; i <= layers_sizes.Length - 1; i++)
        {

            int size_of_this_layer = layers_sizes[i];
            float[][] layer = new float[size_of_this_layer][];

            //iterating for every neuron in the layer
            for (int j = 0; j < size_of_this_layer; j++)
            {
                int size_of_previous_layer = layers_sizes[i - 1];

                float[] neuron = new float[size_of_previous_layer + 1];//+1 is for bias

                //generating every weight on a neuron
                for (int k = 0; k < neuron.Length; k++)
                {
                    neuron[k] = Mathf.Clamp(weight_list[weight_list_index], weight_range[0], weight_range[1]);
                    weight_list_index++;

                }
                layer[j] = neuron;
            }


            weights[i - 1] = layer;
        }

        retorno.weights = weights;
        return retorno;



    }


    public float[] feedInputs(float[] inputs)
    {
        if (inputs.Length != layers_sizes[0])
        {
            Debug.Log($"incompatble net, this net have {layers_sizes[0]} inputs and you are giving it {inputs.Length}");
        }
        List<float> carry = new List<float>(inputs);

        foreach (float[][] layer in weights)
        {
            List<float> output = new List<float>();

            foreach (float[] neuron in layer)
            {
                float value = 0;

                //foreach weight in neuron
                for (int i = 0; i < neuron.Length - 1; i++)
                {
                    value += carry[i] * neuron[i];
                }
                value += neuron[neuron.Length - 1];//adding bias

                value = Mathf.Clamp(value, -1, 1);

                output.Add(value);
            }

            carry = output;

        }

        for (int i = 0; i < carry.Count; i++)
        {
            carry[i] = Mathf.Clamp(carry[i], -1, 1);
        }

        return carry.ToArray();
    }

    public NeuralNetwork mutate(float mutation_degree = 0.2f, float mutation_chance = 0.1f)
    {
        NeuralNetwork novo = new NeuralNetwork(layers_sizes);

        float[][][] novos_weights = new float[weights.Length][][];

        //for every layer
        for (int i = 0; i < weights.Length; i++)
        {
            int size_of_current_layer = weights[i].Length;
            novos_weights[i] = new float[size_of_current_layer][];

            //for every neuron in the layer
            for (int j = 0; j < size_of_current_layer; j++)
            {
                int neuron_size_in_layer = weights[i][j].Length;
                novos_weights[i][j] = new float[neuron_size_in_layer];

                //for every weight in neuron
                for (int k = 0; k < neuron_size_in_layer; k++)
                {
                    float value = weights[i][j][k];
                    if (Random.Range(0f, 1f) <= mutation_chance)
                    {
                        value += Random.Range(-mutation_degree, mutation_degree);
                        Mathf.Clamp(value, weight_range[0], weight_range[1]);
                    }
                    novos_weights[i][j][k] = value;
                }
            }


        }



        novo.weights = novos_weights;
        novo.weight_range = this.weight_range;
        return novo;
    }

    public void saveToFile(string filename)
    {
       
        File.WriteAllText(Application.streamingAssetsPath + "/NNs/" + filename, ToString());
    }

    public override string ToString()
    {
        List<string> lines = new List<string>();
        string sizes = "";
        foreach (int size in layers_sizes)
        {
            sizes += size + ";";
        }
        sizes = sizes.Remove(sizes.Length - 1);
        string range = $"{weight_range[0]};{weight_range[1]}";
        
        
        
        string wei = "";
        foreach (float[][] layer in this.weights)
        {

            string lay = "#\n";
            foreach (float[] neuron in layer)
            {

                string neu = "";
                foreach (float weight in neuron)
                {
                    neu += weight + "; ";
                }
                neu = neu.Remove(neu.Length - 1);
                neu = neu.Remove(neu.Length - 1);
                neu += "\n";
                lay += neu;
            }
            wei += lay;
        }

        return sizes + "\n" +range+"\n"+ wei;
    }



}
