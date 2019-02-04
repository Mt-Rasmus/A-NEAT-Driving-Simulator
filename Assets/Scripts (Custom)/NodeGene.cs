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
        public int Innovation { get; set; }
        public float sum { get; set; }

        public NodeGene(NodeType type, int id, int innovation)
        {
            //super();
            this.Type = type;
            this.ID = id;
            this.Innovation = innovation;
        }

        public NodeGene(NodeGene gene)
        {
            this.Type = gene.Type;
            this.ID = gene.ID;
            this.Innovation = gene.Innovation;
        }

        public NodeGene Copy()
        {
            return new NodeGene(Type, ID, Innovation);
        }
    }
}
