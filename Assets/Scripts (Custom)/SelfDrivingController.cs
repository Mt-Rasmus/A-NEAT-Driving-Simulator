using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NEAT
{


    public class SelfDrivingController : MonoBehaviour
    {
        // variables
        public Counter nodeInnovation, connectionInnovation;
        private int population = 10;
        private int nrInputs, nrOutputs;

        private void Start()
        {
            //  1. Create one initial network with only inputs and outputs
            
            // global innovation counters
            nodeInnovation = new Counter();
            connectionInnovation = new Counter();
            nrInputs = 5;
            nrOutputs = 2;


            // Create starting Genome 
            Genome genome = new Genome(nrInputs, nrOutputs);

            // 2. Create a new population of networks through Evaluator by copying the first network
            Evaluator NetworkEvaluator = new Evaluator(population, genome, nodeInnovation, connectionInnovation);

            
            // 3. Start one generation of Cars
            //  3.1 Use Raytracing input to calculate output of networks
            //  3.2 Use output of networks to control the Cars
            //  3.3 Save fitness of each network when Cars die

            // 4. Run NetworkEvaluator.Evaluate()

            // 5. Repeat 3-4 until satisfied with the result
        }




    }
}
