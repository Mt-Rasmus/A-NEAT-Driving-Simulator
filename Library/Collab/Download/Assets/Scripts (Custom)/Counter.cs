using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT
{
    public class Counter : MonoBehaviour
    {

        private int currentInnovation = 0;

        public int GetInnovation()
        {
            return currentInnovation++;
        }
    }
}
