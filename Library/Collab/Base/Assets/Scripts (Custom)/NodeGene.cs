using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT
{
    public class NodeGene : MonoBehaviour
    {
        public enum NodeType
        {
            INPUT,
            HIDDEN,
            OUTPUT
        };

        public NodeType Type { get; set; }
        public int ID { get; set; }

        public NodeGene(NodeType type, int id)
        {
            //super();
            this.Type = type;
            this.ID = id;
        }

        public NodeGene(NodeGene gene)
        {
            this.Type = gene.Type;
            this.ID = gene.ID;
        }

        public NodeGene Copy()
        {
            return new NodeGene(Type, ID);
        }
    }
}
