using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT
{
    public class ConnectionGene : MonoBehaviour
    {

        public int inNode { get; set; }
        public int outNode { get; set; }
        public float weight { get; set; }
        public bool expressed { get; set; }
        public int innovation { get; set; }

        public ConnectionGene(int inNode, int outNode, float weight, bool expressed, int innovation)
        {
            //super();
            this.inNode = inNode;
            this.outNode = outNode;
            this.weight = weight;
            this.expressed = expressed;
            this.innovation = innovation;
        }

        public ConnectionGene(ConnectionGene con)
        {
            this.inNode = con.inNode;
            this.outNode = con.outNode;
            this.weight = con.weight;
            this.expressed = con.expressed;
            this.innovation = con.innovation;
        }

        public ConnectionGene copy()
        {
            return new ConnectionGene(inNode, outNode, weight, expressed, innovation);
        }
    }
}
