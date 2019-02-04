using System.Collections;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine;
using System.Linq;

namespace UnityStandardAssets.Vehicles.Car
{
    public class Genome : MonoBehaviour
    {
        public Dictionary<int, NodeGene> Nodes { get; set; }
        public Dictionary<int, ConnectionGene> Connections { get; set; }
        private readonly float PROBABILITY_PERTUBING = 0.8f;
        public float fitness { get; set; }

        public Genome()
        {
            Nodes = new Dictionary<int, NodeGene>();
            Connections = new Dictionary<int, ConnectionGene>();
            fitness = 0;
        }

        /**
         * Create copy of Genome
         */
        public Genome(Genome toBeCopied)
        {
            Nodes = new Dictionary<int, NodeGene>();
            Connections = new Dictionary<int, ConnectionGene>();
            fitness = 0;

            foreach (int index in toBeCopied.Nodes.Keys)
            {
                Nodes.Add(index, new NodeGene(toBeCopied.Nodes[index]));
            }

            foreach (int index in toBeCopied.Connections.Keys)
            {
                Connections.Add(index, new ConnectionGene(toBeCopied.Connections[index]));
            }
        }
        /**
         * Create a Genome with an arbitrary number of inputs and outputs
         */
        public Genome(int inputs, int outputs, Counter nodeInnovation, Counter connectionInnovation)
        {
            Nodes = new Dictionary<int, NodeGene>();
            Connections = new Dictionary<int, ConnectionGene>();
            fitness = 0;
            int i;

            // add input nodes
            for (i = 0; i < inputs; i++)
            {
                AddNodeGene(new NodeGene(NodeGene.NodeType.INPUT, nodeInnovation.GetInnovation()));
            }
            // add output nodes
            for (int j = i; j < i + outputs; j++)
            {
                AddNodeGene(new NodeGene(NodeGene.NodeType.OUTPUT, nodeInnovation.GetInnovation()));
            }
            // add connections, fully connected input and output layers
            for (i = 0; i < inputs; i++)
            {
                for (int j = inputs; j < inputs + outputs; j++)
                {
                    AddConnectionGene(new ConnectionGene(i, j, RandomWeight(), true, connectionInnovation.GetInnovation()));
                }
            }
        }

        private float RandomWeight()
        {
            return Random.Range(-0.5f, 0.5f);
        }

        public void AddNodeGene(NodeGene gene)
        {
            Nodes.Add(gene.ID, gene);
        }

        public void AddConnectionGene(ConnectionGene gene)
        {
            Connections.Add(gene.Innovation, gene);
        }

        public void Mutation()
        {

            foreach (ConnectionGene con in Connections.Values)
            {
                if ((Random.Range(0f, 1f) <= PROBABILITY_PERTUBING)) // uniformly pertubing weights
                {
                    con.Weight += Random.Range(-0.5f, 0.5f);
                }
                else // assign new weight
                {
                    con.Weight = Random.Range(-0.5f, 0.5f);
                }
            }
        }

        public void AddConnectionMutation(Counter innovation, int maxAttempts)
        {

            int tries = 0;
            bool success = false;
            while (tries < maxAttempts && success == false)
            {
                tries++;


                int listSize = Nodes.Count;

                int randomNodeKey1 = Random.Range(0, listSize - 1);
                int randomNodeKey2 = Random.Range(0, listSize - 1);

                while (!Nodes.ContainsKey(randomNodeKey1) || !Nodes.ContainsKey(randomNodeKey2))
                {
                    randomNodeKey1 = Random.Range(0, listSize - 1);
                    randomNodeKey2 = Random.Range(0, listSize - 1);
                }

                NodeGene node1 = Nodes[randomNodeKey1];
                NodeGene node2 = Nodes[randomNodeKey2];

                bool reversed = false;
                if (node1.GetType().Equals(NodeGene.NodeType.HIDDEN) && node2.GetType().Equals(NodeGene.NodeType.INPUT))
                {
                    reversed = true;
                }
                else if (node1.GetType().Equals(NodeGene.NodeType.OUTPUT) && node2.GetType().Equals(NodeGene.NodeType.HIDDEN))
                {
                    reversed = true;
                }
                else if (node1.GetType().Equals(NodeGene.NodeType.OUTPUT) && node2.GetType().Equals(NodeGene.NodeType.INPUT))
                {
                    reversed = true;
                }

                bool connectionImpossible = false;
                if (node1.GetType().Equals(NodeGene.NodeType.INPUT) && node2.GetType().Equals(NodeGene.NodeType.INPUT))
                {
                    connectionImpossible = true;
                }
                else if (node1.GetType().Equals(NodeGene.NodeType.OUTPUT) && node2.GetType().Equals(NodeGene.NodeType.OUTPUT))
                {
                    connectionImpossible = true;
                }

                bool connectionExists = false;
                foreach (ConnectionGene con in Connections.Values)
                {
                    if (con.InNode == node1.ID && con.OutNode == node2.ID)
                    {
                        connectionExists = true;
                        break;
                    }
                    else if (con.InNode == node2.ID && con.OutNode == node1.ID)
                    {
                        connectionExists = true;
                        break;
                    }
                }

                if (connectionExists || connectionImpossible)
                {
                    continue;
                }

                ConnectionGene newCon = new ConnectionGene(reversed ? node2.ID : node1.ID, reversed ? node1.ID : node2.ID, Random.Range(0f, 1f), true, innovation.GetInnovation());
                //Connections.Add(newCon.Innovation, newCon);
                AddConnectionGene(newCon);
                success = true;
            }
            if (success == false)
            {
                Debug.Log("Tried, but could not add more connections");
            }
        }

        /**
         * Disable random ConnectionGene and add a new NodeGene in the middle
         */
        public void AddNodeMutation(Counter nodeInnovation, Counter connectionInnovation)
        {

            int listSize = Connections.Count;
            int randomConKey = Random.Range(0, listSize - 1);

            while(!Connections.ContainsKey(randomConKey))
            {
                randomConKey = Random.Range(0, listSize - 1);
            }

            ConnectionGene con = Connections[randomConKey];

            NodeGene inNode = Nodes[con.InNode];
            NodeGene outNode = Nodes[con.OutNode];

            con.Expressed = false;

            NodeGene newNode = new NodeGene(NodeGene.NodeType.HIDDEN, nodeInnovation.GetInnovation());
            ConnectionGene inToNew = new ConnectionGene(inNode.ID, newNode.ID, 1f, true, connectionInnovation.GetInnovation());
            ConnectionGene newToOut = new ConnectionGene(newNode.ID, outNode.ID, con.Weight, true, connectionInnovation.GetInnovation());

            //Nodes.Add(newNode.ID, newNode);
            AddNodeGene(newNode);
            //Connections.Add(inToNew.Innovation, inToNew);
            AddConnectionGene(inToNew);
            //Connections.Add(newToOut.Innovation, newToOut);
            AddConnectionGene(newToOut);
        }

        /**
         * @param parent1   More fit parent
         * @param parent2   Less fit parent
         * Operating on a Genome object
         */
        public void Crossover(Genome parent1, Genome parent2)
        {

            // Genome child = new Genome();

            foreach (NodeGene parentNode in parent1.Nodes.Values)
            {
                AddNodeGene(parentNode.Copy());
            }
            foreach (ConnectionGene parentNode in parent1.Connections.Values)
            {


          //      if (parent2.Connections.ContainsValue(parentNode)) // matching gene
                    if (parent2.Connections.ContainsKey(parentNode.Innovation)) // matching gene
                    {
                    //                    ConnectionGene childConGene = (Random.Range(0f, 1f) >= 0.5) ? parentNode.Copy() : parent2.Connections[parentNode.Innovation].Copy();

                    //                   int lol = parentNode.Innovation;


                    ConnectionGene childConGene = (Random.Range(0f, 1f) >= 0.0) ? new ConnectionGene(parentNode) : new ConnectionGene(parent2.Connections[parentNode.Innovation]);

                    AddConnectionGene(childConGene);
                }
                else // disjoint or excess gene
                {
                    ConnectionGene childConGene = parentNode.Copy();
                    AddConnectionGene(childConGene);
                }
            }
            //return child;
        }

        public static float CompatibilityDistance(Genome genome1, Genome genome2, float c1, float c2, float c3)
        {

            int matchingGenes = 0;
            int disjointGenes = 0;
            int excessGenes = 0;
            float weightDifference = 0;
            float avgWeightDiff = 0;
            float compatibilityDistance = 0;

            int highestNodeInnovation1 = 0; // find largest innovation number 
            int highestNodeInnovation2 = 0; // find largest innovation number 


            //     if (genome1 == null)
            //         Debug.Log("genome1 == null");

//            Debug.Log("genome1.Nodes.Count 1 == " + genome1.Nodes.Count);
//            Debug.Log("genome2.Nodes.Count 1 == " + genome2.Nodes.Count);

            foreach (var node in genome1.Nodes)
            {

            //    Debug.Log("genome1.Nodes.Count 2 == " + genome1.Nodes.Count);

                if(node.Key > highestNodeInnovation1)
                {
                    highestNodeInnovation1 = node.Key;
                }

            }

            foreach (var node in genome2.Nodes)
            {
                if (node.Key > highestNodeInnovation2)
                {
                    highestNodeInnovation2 = node.Key;
                }
            }


            int indicesNode = Mathf.Max(highestNodeInnovation1, highestNodeInnovation2);

            bool node1ContainsKey, node2ContainsKey = false;

            for (int i = 0; i <= indicesNode; i++)
            {                   // loop through genes -> i is innovation numbers
                if (genome1.Nodes.ContainsKey(i))
                {
                    node1ContainsKey = true;
                }
                else
                    node1ContainsKey = false;

                if (genome2.Nodes.ContainsKey(i))
                {
                    node2ContainsKey = true;
                }
                else
                    node2ContainsKey = false;


                if (node1ContainsKey && node2ContainsKey)
                {
                    // both genomes has the gene w/ this innovation number
                //    matchingGenes++;
                }
                else if (!node1ContainsKey && node2ContainsKey && highestNodeInnovation1 > i)
                {
                    // genome 1 lacks gene, genome 2 has gene, genome 1 has more genes w/ higher innovation numbers
                    disjointGenes++;
                }
                else if (node1ContainsKey && !node2ContainsKey && highestNodeInnovation2 > i)
                {
                    // genome 2 lacks gene, genome 1 has gene, genome 2 has more genes w/ higher innovation numbers
                    disjointGenes++;
                }
                else if (!node1ContainsKey && node2ContainsKey && highestNodeInnovation1 < i)
                {
                    // genome 1 lacks gene, genome 2 has gene, genome 2 has more genes w/ higher innovation numbers
                    excessGenes++;
                }
                else if (node1ContainsKey && !node2ContainsKey && highestNodeInnovation2 < i)
                {
                    // genome 2 lacks gene, genome 1 has gene, genome 1 has more genes w/ higher innovation numbers
                    excessGenes++;
                }
            }

            int highestConnectionInnovation1 = 0; // find largest innovation number 
            int highestConnectionInnovation2 = 0; // find largest innovation number 

            foreach (var node in genome1.Connections)
            {
                if (node.Key > highestConnectionInnovation1)
                {
                    highestConnectionInnovation1 = node.Key;
                }
            }
            foreach (var node in genome2.Connections)
            {
                if (node.Key > highestConnectionInnovation2)
                {
                    highestConnectionInnovation2 = node.Key;
                }
            }
            int indicesCon = Mathf.Max(highestConnectionInnovation1, highestConnectionInnovation2);

            bool connection1ContainsKey, connection2ContainsKey = false;

            for (int i = 0; i <= indicesCon; i++)
            {                   // loop through genes -> i is innovation numbers

                if (genome1.Connections.ContainsKey(i))
                {
                    connection1ContainsKey = true;
                }
                else
                    connection1ContainsKey = false;

                if (genome2.Connections.ContainsKey(i))
                {
                    connection2ContainsKey = true;
                }
                else
                    connection2ContainsKey = false;

                if (connection1ContainsKey && connection2ContainsKey)
                {
                    // both genomes has the gene w/ this innovation number
                    matchingGenes++;
                    weightDifference += Mathf.Abs(genome1.Connections[i].Weight - genome2.Connections[i].Weight);
                }
                else if (!connection1ContainsKey && connection2ContainsKey && highestConnectionInnovation1 > i)
                {
                    // genome 1 lacks gene, genome 2 has gene, genome 1 has more genes w/ higher innovation numbers
                    disjointGenes++;
                }
                else if (connection1ContainsKey && !connection2ContainsKey && highestConnectionInnovation2 > i)
                {
                    // genome 2 lacks gene, genome 1 has gene, genome 2 has more genes w/ higher innovation numbers
                    disjointGenes++;
                }
                else if (!connection1ContainsKey && connection2ContainsKey && highestConnectionInnovation1 < i)
                {
                    // genome 1 lacks gene, genome 2 has gene, genome 1 has more genes w/ higher innovation numbers
                    excessGenes++;
                }
                else if (connection1ContainsKey && !connection2ContainsKey && highestConnectionInnovation2 < i)
                {
                    // genome 2 lacks gene, genome 1 has gene, genome 2 has more genes w/ higher innovation numbers
                    excessGenes++;
                }
            }
            avgWeightDiff = weightDifference / matchingGenes;
            float N = 1; // number of genes in largest parent, 1 if less than 20 genes

        //    Debug.Log("excessGenes = " + excessGenes);
        //    Debug.Log("disjointGenes = " + disjointGenes);
        //    Debug.Log("avgWeightDiff = " + avgWeightDiff);
        //    Debug.Log("weightDifference = " + weightDifference);
        //    Debug.Log("matchingGenes = " + matchingGenes);
            
            compatibilityDistance = excessGenes * c1/N + disjointGenes * c2/N + avgWeightDiff * c3; 

            return compatibilityDistance;
        }
    } 
}