using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
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
        public float sum { get; set; }
        public float rank { get; set; }

        public NodeGene(NodeType type, int id)
        {
            this.Type = type;
            this.ID = id;
            this.sum = 0;
        }

        public NodeGene(NodeGene gene)
        {
            this.Type = gene.Type;
            this.ID = gene.ID;
            this.sum = 0;
        }

        public NodeGene Copy()
        {
            return new NodeGene(Type, ID);
        }
    }
}
