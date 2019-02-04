using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT
{
    public class ConnectionGene : MonoBehaviour
    {

        public int InNode { get; set; } // id
        public int OutNode { get; set; } // id
        public float Weight { get; set; }
        public bool Expressed { get; set; } // if the connection is active
        public int Innovation { get; set; }

        public ConnectionGene(int inNode, int outNode, float weight, bool expressed, int innovation)
        {
            //super();
            this.InNode = inNode;
            this.OutNode = outNode;
            this.Weight = weight;
            this.Expressed = expressed;
            this.Innovation = innovation;
        }

        public ConnectionGene(ConnectionGene con)
        {
            this.InNode = con.InNode;
            this.OutNode = con.OutNode;
            this.Weight = con.Weight;
            this.Expressed = con.Expressed;
            this.Innovation = con.Innovation;
        }

        public ConnectionGene Copy()
        {
            return new ConnectionGene(InNode, OutNode, Weight, Expressed, Innovation);
        }

        private void SetRandomWeight()
        {
            this.Weight = Random.Range(0.000001f, 1f);
        }
    }
}
