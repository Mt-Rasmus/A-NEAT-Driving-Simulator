using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace NEAT
{
    public class Genome : MonoBehaviour
    {
        private Dictionary<int, NodeGene> Nodes { get; set; }
        private Dictionary<int, ConnectionGene> Connections { get; set; }
        private readonly float PROBABILITY_PERTUBING = 0.8f;

        public Genome()
        {
            Nodes = new Dictionary<int, NodeGene>();
            Connections = new Dictionary<int, ConnectionGene>();
        }

        /**
         * Create copy of Genome
         */
        public Genome(Genome toBeCopied)
        {
            Nodes = new Dictionary<int, NodeGene>();
            Connections = new Dictionary<int, ConnectionGene>();

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
        public Genome(int inputs, int outputs)
        {
            Nodes = new Dictionary<int, NodeGene>();
            Connections = new Dictionary<int, ConnectionGene>();

            // add input nodes
            for (int i = 0; i < inputs; i++)
            {
                AddNodeGene(new NodeGene(NodeGene.NodeType.INPUT, i, 0));
            }
            // add output nodes
            for (int j = 0; j < outputs; j++)
            {
                AddNodeGene(new NodeGene(NodeGene.NodeType.OUTPUT, j, 0));
            }
            // add connections, fully connected input and output layers
            for (int i = 0; i < inputs; i++)
            {
                for (int j = 0; j < outputs; j++)
                {
                    AddConnectionGene(new ConnectionGene(i, j, RandomWeight(), true, 0));
                }
            }
        }

        private float RandomWeight()
        {
            return Random.Range(0.000001f, 1f);
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
                    con.Weight *= Random.Range(-2f, 2f);
                }
                else // assign new weight
                {
                    con.Weight = Random.Range(-2f, 2f);
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
                NodeGene node1 = Nodes[Random.Range(0, listSize)];
                NodeGene node2 = Nodes[Random.Range(0, listSize)];

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
                if (connectionExists)
                {
                    return;
                }

                ConnectionGene newCon = new ConnectionGene(reversed ? node2.ID : node1.ID, reversed ? node1.ID : node2.ID, Random.Range(0f, 1f), true, 0);
                //Connections.Add(newCon.Innovation, newCon);
                AddConnectionGene(newCon);
            }
            if (success == false)
            {
                Debug.Log("Tried, but could not add more connections");
            }
        }

        /**
         * Disable random ConnectionGene and add a new NodeGene in the middle
         */
        public void AddNodeMutation(Counter connectionInnovation, Counter nodeInnovation)
        {
            int listSize = Connections.Count;
            ConnectionGene con = Connections[Random.Range(0, listSize)];

            NodeGene inNode = Nodes[con.InNode];
            NodeGene outNode = Nodes[con.OutNode];

            con.Expressed = false;

            NodeGene newNode = new NodeGene(NodeGene.NodeType.HIDDEN, Nodes.Count, nodeInnovation.GetInnovation());
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
                if (parent2.Connections.ContainsValue(parentNode)) // matching gene
                {
                    ConnectionGene childConGene = (Random.Range(0f, 1f) >= 0.5) ? parentNode.Copy() : parent2.Connections[parentNode.Innovation].Copy();
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

            int highestNodeInnovation1 = genome1.Nodes.Keys.Max(); // find largest innovation number 
            int highestNodeInnovation2 = genome2.Nodes.Keys.Max();
            int indicesNode = Mathf.Max(highestNodeInnovation1, highestNodeInnovation2);

            for (int i = 0; i <= indicesNode; i++)
            {                   // loop through genes -> i is innovation numbers
                NodeGene node1 = genome1.Nodes[i];
                NodeGene node2 = genome2.Nodes[i];
                if (node1 != null && node2 != null)
                {
                    // both genomes has the gene w/ this innovation number
                    matchingGenes++;
                }
                else if (node1 == null && node2 != null && highestNodeInnovation1 > i)
                {
                    // genome 1 lacks gene, genome 2 has gene, genome 1 has more genes w/ higher innovation numbers
                    disjointGenes++;
                }
                else if (node1 != null && node2 == null && highestNodeInnovation2 > i)
                {
                    // genome 2 lacks gene, genome 1 has gene, genome 2 has more genes w/ higher innovation numbers
                    disjointGenes++;
                }
                else if (node1 == null && node2 != null && highestNodeInnovation1 < i)
                {
                    // genome 1 lacks gene, genome 2 has gene, genome 2 has more genes w/ higher innovation numbers
                    excessGenes++;
                }
                else if (node1 != null && node2 == null && highestNodeInnovation2 < i)
                {
                    // genome 2 lacks gene, genome 1 has gene, genome 1 has more genes w/ higher innovation numbers
                    excessGenes++;
                }
            }

            int highestConnectionInnovation1 = genome1.Connections.Keys.Max(); // find largest innovation number 
            int highestConnectionInnovation2 = genome2.Connections.Keys.Max();
            int indicesCon = Mathf.Max(highestConnectionInnovation1, highestConnectionInnovation2);

            for (int i = 0; i <= indicesCon; i++)
            {                   // loop through genes -> i is innovation numbers
                ConnectionGene connection1 = genome1.Connections[i];
                ConnectionGene connection2 = genome2.Connections[i];
                if (connection1 != null && connection2 != null)
                {
                    // both genomes has the gene w/ this innovation number
                    matchingGenes++;
                    weightDifference += Mathf.Abs(connection1.Weight - connection2.Weight);
                }
                else if (connection1 == null && connection2 != null && highestConnectionInnovation1 > i)
                {
                    // genome 1 lacks gene, genome 2 has gene, genome 1 has more genes w/ higher innovation numbers
                    disjointGenes++;
                }
                else if (connection1 != null && connection2 == null && highestConnectionInnovation2 > i)
                {
                    // genome 2 lacks gene, genome 1 has gene, genome 2 has more genes w/ higher innovation numbers
                    disjointGenes++;
                }
                else if (connection1 == null && connection2 != null && highestConnectionInnovation1 < i)
                {
                    // genome 1 lacks gene, genome 2 has gene, genome 1 has more genes w/ higher innovation numbers
                    excessGenes++;
                }
                else if (connection1 != null && connection2 == null && highestConnectionInnovation2 < i)
                {
                    // genome 2 lacks gene, genome 1 has gene, genome 2 has more genes w/ higher innovation numbers
                    excessGenes++;
                }
            }
            avgWeightDiff = weightDifference / matchingGenes;
            float N = 1; // number of genes in largest parent, 1 if less than 20 genes
            compatibilityDistance = excessGenes * c1/N + disjointGenes * c2/N + avgWeightDiff * c3; 

            return compatibilityDistance;
        }
    } 
}