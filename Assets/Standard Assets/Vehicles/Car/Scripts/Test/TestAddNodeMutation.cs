using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace UnityStandardAssets.Vehicles.Car
{
    public class TestAddNodeMutation : MonoBehaviour
    {
        public Counter nodeInnovation;
        public Counter connectionInnovation;
        private int population = 20;
        private int nrInputs = 5;
        private int nrOutputs = 2;
        Evaluator NetworkEvaluator;

        // Use this for initialization
        void Start()
        {
            nodeInnovation = new Counter();
            connectionInnovation = new Counter();

            // Create starting Genome 
            Genome genome = new Genome(nrInputs, nrOutputs, nodeInnovation, connectionInnovation);

            // 2. Create a new population of networks through Evaluator by copying the first network
            NetworkEvaluator = new Evaluator(population, genome, nodeInnovation, connectionInnovation);

            for (int i = 0; i < 20; i++)
            {
                NetworkEvaluator.Evaluate();
                Debug.Log("Generation: " + i);
                Debug.Log("\tHighest fitness: " + NetworkEvaluator.GetHighestFitness());
                Debug.Log("\tAmount of species: " + NetworkEvaluator.GetSpeciesAmount());
                Debug.Log("\n");

            }
    //Counter nodeInnovation = new Counter();
    //Counter connectionInnovation = new Counter();
    //System.Random r = new System.Random();

    //Genome genome = new Genome();

    //NodeGene input1 = new NodeGene(NodeGene.NodeType.INPUT, 0, 0);
    //NodeGene input2 = new NodeGene(NodeGene.NodeType.INPUT, 1, 0);
    //NodeGene output = new NodeGene(NodeGene.NodeType.OUTPUT, 2, 0);

    //genome.AddNodeGene(input1);
    //genome.AddNodeGene(input2);
    //genome.AddNodeGene(output);

    //ConnectionGene con1 = new ConnectionGene(0, 2, 0.5f, true, connectionInnovation.GetInnovation());
    //ConnectionGene con2 = new ConnectionGene(1, 2, 1f, true, connectionInnovation.GetInnovation());

    //genome.AddConnectionGene(con1);
    //genome.AddConnectionGene(con2);

    ////GenomePrinter.printGenome(genome, "output/add_nod_mut_test_before.png");
    //foreach(NodeGene node in genome.Nodes.Values)
    //{
    //    Debug.Log("Node ID: " + node.ID);
    //    Debug.Log("Node Type: " + node.Type);
    //    Debug.Log("Node Innovation: " + node.Innovation);
    //}
    //foreach (ConnectionGene node in genome.Connections.Values)
    //{
    //    Debug.Log("InNode: " + node.InNode);
    //    Debug.Log("OutNode: " + node.OutNode);
    //    Debug.Log("Weight: " + node.Weight);
    //}

    //genome.AddNodeMutation(nodeInnovation, connectionInnovation);

    //GenomePrinter.printGenome(genome, "output/add_nod_mut_test_after.png");
    // global innovation counters




}

        // Update is called once per frame
        void Update()
        {
            
            
        }
    }
}
