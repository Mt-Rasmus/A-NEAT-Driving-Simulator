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

        public void AddConnectionMutation(Random r)
        {
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
            Connections.Add(newCon.Innovation, newCon);
        }

        public void AddNodeMutation(Counter innovation)
        {
            int listSize = Connections.Count;
            ConnectionGene con = Connections[Random.Range(0, listSize)];

            NodeGene inNode = Nodes[con.InNode];
            NodeGene outNode = Nodes[con.OutNode];

            con.Expressed = false;

            NodeGene newNode = new NodeGene(NodeGene.NodeType.HIDDEN, Nodes.Count);
            ConnectionGene inToNew = new ConnectionGene(inNode.ID, newNode.ID, 1f, true, innovation.GetInnovation());
            ConnectionGene newToOut = new ConnectionGene(newNode.ID, outNode.ID, con.Weight, true, innovation.GetInnovation());

            Nodes.Add(newNode.ID, newNode);
            Connections.Add(inToNew.Innovation, inToNew);
            Connections.Add(newToOut.Innovation, newToOut);
        }

        /**
         * @param parent1   More fit parent
         * @param parent2   Less fit parent
         */
        public Genome Crossover(Genome parent1, Genome parent2)
        {
            Genome child = new Genome();

            foreach (NodeGene parentNode in parent1.Nodes.Values)
            {
                child.AddNodeGene(parentNode.Copy());
            }
            foreach (ConnectionGene parentNode in parent1.Connections.Values)
            {
                if (parent2.Connections.ContainsValue(parentNode)) // matching gene
                {
                    ConnectionGene childConGene = (Random.Range(0f, 1f) >= 0.5) ? parentNode.Copy() : parent2.Connections[parentNode.Innovation].Copy();
                    child.AddConnectionGene(childConGene);
                }
                else // disjoint or excess gene
                {
                    ConnectionGene childConGene = parentNode.Copy();
                    child.AddConnectionGene(childConGene);
                }
            }
            return child;
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
            compatibilityDistance = excessGenes * c1 + disjointGenes * c2 + avgWeightDiff * c3;

            return compatibilityDistance;
        }

        //public static int CountDisjointGenes(Genome genome1, Genome genome2)
        //{
        //    int disjointGenes = 0;

        //    int highestNodeInnovation1 = genome1.Nodes.Keys.Max(); // find largest innovation number 
        //    int highestNodeInnovation2 = genome2.Nodes.Keys.Max();
        //    int indicesNode = Mathf.Max(highestNodeInnovation1, highestNodeInnovation2);

        //    for (int i = 0; i <= indicesNode; i++)
        //    {                   // loop through genes -> i is innovation numbers
        //        NodeGene node1 = genome1.Nodes[i];
        //        NodeGene node2 = genome2.Nodes[i];
        //        if (node1 == null && node2 != null && highestNodeInnovation1 > i)
        //        {
        //            // genome 1 lacks gene, genome 2 has gene, genome 1 has more genes w/ higher innovation numbers
        //            disjointGenes++;
        //        }
        //        else if (node1 != null && node2 == null && highestNodeInnovation2 > i)
        //        {
        //            // genome 2 lacks gene, genome 1 has gene, genome 2 has more genes w/ higher innovation numbers
        //            disjointGenes++;
        //        }
        //    }

        //    int highestConnectionInnovation1 = genome1.Connections.Keys.Max(); // find largest innovation number 
        //    int highestConnectionInnovation2 = genome2.Connections.Keys.Max();
        //    int indicesCon = Mathf.Max(highestConnectionInnovation1, highestConnectionInnovation2);

        //    for (int i = 0; i <= indicesCon; i++)
        //    {                   // loop through genes -> i is innovation numbers
        //        ConnectionGene connection1 = genome1.Connections[i];
        //        ConnectionGene connection2 = genome2.Connections[i];
        //        if (connection1 == null && connection2 != null && highestConnectionInnovation1 > i)
        //        {
        //            // genome 1 lacks gene, genome 2 has gene, genome 1 has more genes w/ higher innovation numbers
        //            disjointGenes++;
        //        }
        //        else if (connection1 != null && connection2 == null && highestConnectionInnovation2 > i)
        //        {
        //            // genome 2 lacks gene, genome 1 has gene, genome 2 has more genes w/ higher innovation numbers
        //            disjointGenes++;
        //        }
        //    }

        //    return disjointGenes;
        //}

        //public static int CountExcessGenes(Genome genome1, Genome genome2)
        //{
        //    int excessGenes = 0;

        //    int highestNodeInnovation1 = genome1.Nodes.Keys.Max(); // find largest innovation number 
        //    int highestNodeInnovation2 = genome2.Nodes.Keys.Max();
        //    int indicesNode = Mathf.Max(highestNodeInnovation1, highestNodeInnovation2);

        //    for (int i = 0; i <= indicesNode; i++)
        //    {                   // loop through genes -> i is innovation numbers
        //        NodeGene node1 = genome1.Nodes[i];
        //        NodeGene node2 = genome2.Nodes[i];
        //        if (node1 == null && node2 != null && highestNodeInnovation1 < i)
        //        {
        //            // genome 1 lacks gene, genome 2 has gene, genome 2 has more genes w/ higher innovation numbers
        //            excessGenes++;
        //        }
        //        else if (node1 != null && node2 == null && highestNodeInnovation2 < i)
        //        {
        //            // genome 2 lacks gene, genome 1 has gene, genome 1 has more genes w/ higher innovation numbers
        //            excessGenes++;
        //        }
        //    }

        //    int highestConnectionInnovation1 = genome1.Connections.Keys.Max(); // find largest innovation number 
        //    int highestConnectionInnovation2 = genome2.Connections.Keys.Max();
        //    int indicesCon = Mathf.Max(highestConnectionInnovation1, highestConnectionInnovation2);

        //    for (int i = 0; i <= indicesCon; i++)
        //    {                   // loop through genes -> i is innovation numbers
        //        ConnectionGene connection1 = genome1.Connections[i];
        //        ConnectionGene connection2 = genome2.Connections[i];
        //        if (connection1 == null && connection2 != null && highestConnectionInnovation1 < i)
        //        {
        //            // genome 1 lacks gene, genome 2 has gene, genome 1 has more genes w/ higher innovation numbers
        //            excessGenes++;
        //        }
        //        else if (connection1 != null && connection2 == null && highestConnectionInnovation2 < i)
        //        {
        //            // genome 2 lacks gene, genome 1 has gene, genome 2 has more genes w/ higher innovation numbers
        //            excessGenes++;
        //        }
        //    }

        //    return excessGenes;
        //}

        //public static float averageWeightDiff(Genome genome1, Genome genome2)
        //{
        //    int matchingGenes = 0;
        //    float weightDifference = 0;

        //    int highestConnectionInnovation1 = genome1.Connections.Keys.Max(); // find largest innovation number 
        //    int highestConnectionInnovation2 = genome2.Connections.Keys.Max();
        //    int indicesCon = Mathf.Max(highestConnectionInnovation1, highestConnectionInnovation2);

        //    for (int i = 0; i <= indicesCon; i++)
        //    {                   // loop through genes -> i is innovation numbers
        //        ConnectionGene connection1 = genome1.Connections[i];
        //        ConnectionGene connection2 = genome2.Connections[i];
        //        if (connection1 != null && connection2 != null)
        //        {
        //            // both genomes has the gene w/ this innovation number
        //            matchingGenes++;
        //            weightDifference += Mathf.Abs(connection1.Weight - connection2.Weight);
        //        }
        //    }

        //    return weightDifference / matchingGenes;
        //}

    } 
}