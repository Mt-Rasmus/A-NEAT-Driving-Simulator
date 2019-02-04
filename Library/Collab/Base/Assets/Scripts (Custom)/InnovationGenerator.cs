using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT
{
    public class InnovationGenerator : MonoBehaviour
    {

        private int currentInnovation = 0;

        public int getInnovation()
        {
            return currentInnovation++;
        }
    }
}
