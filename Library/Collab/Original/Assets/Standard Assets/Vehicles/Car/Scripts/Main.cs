using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace UnityStandardAssets.Vehicles.Car
{
 //   [RequireComponent(typeof(CarController))]
    public class Main : MonoBehaviour
    {
        /**
         * Car variables
         */ 
        private CarController m_Car;
        public GameObject carGameObject;
        [HideInInspector]
        public Cars cars;
        public int noCars = 2;
        bool goOn = true;
        private Dictionary<int, Vector3> carPos;
        private Dictionary<int, float> carDistance;
        private float initTime = 0;
        private float timeSinceInit = 0;
        private float slowPunish = 0;

        /**
         * NeuroEvolution variables
         */
        [HideInInspector]
        public Counter nodeInnovation, connectionInnovation;
        private int population = 2;
        private int nrInputs = 6;
        private int nrOutputs = 2;
        //private Dictionary<int, Dictionary<int, float>> CarRays;
        Evaluator NetworkEvaluator;
        private int generation = 0;

        // Use this for initialization
        void Start()
        {
            //  1. Create one initial network with only inputs and outputs
            noCars = 50;
            // global innovation counters
            nodeInnovation = new Counter();
            connectionInnovation = new Counter();

            //Create starting Genome
            Genome genome = new Genome(nrInputs, nrOutputs, nodeInnovation, connectionInnovation);

            // 2.Create a new population of networks through Evaluator by copying the first network
            NetworkEvaluator = new Evaluator(noCars, genome, nodeInnovation, connectionInnovation);

            // 3. Start one generation of Cars
            if (carGameObject != null)
            {
                //noCars = 3;
                initCars();         
            }

            StartCoroutine(carUpdate());
            

        }

        // Update is called once per frame
        void Update()
        {
            

        }

        public float Logistic(float x)
        {
            return 1 / (1 + (float)Math.Pow(Math.E, -x));
        }

        public void initCars()
        {
            // Instantiate cars
            cars = new Cars(noCars, carGameObject);
            carPos = new Dictionary<int, Vector3>();
            carDistance = new Dictionary<int, float>();
            initTime = Time.timeSinceLevelLoad;

            for (int i = 0; i < noCars; i++)
            {
                m_Car = cars.CarList[i].carObj.GetComponent<CarController>();
                carPos[i] = cars.CarList[i].carObj.transform.position;
                
                carDistance[i] = 0;

                cars.CarList[i].SteeringAngle = UnityEngine.Random.Range(-0.01f, 0.01f);

                cars.CarList[i].Velocity = 0.99f;

                m_Car.m_Rigidbody = cars.CarList[i].carObj.GetComponent<Rigidbody>();
            }
            timeSinceInit = Time.timeSinceLevelLoad;
        }

        IEnumerator carUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds(.1f);
                timeSinceInit = Time.timeSinceLevelLoad - initTime;

                foreach (int key in cars.CarList.Keys.ToList())
                {
                    
                    cars.CarList[key].UpdateRays();
                    
                    m_Car = cars.CarList[key].carObj.GetComponent<CarController>();

                    float rayValue;
                    Dictionary<int, int> outnodeIndex = new Dictionary<int, int>();
                    Dictionary<int, int> outnodeIndex2 = new Dictionary<int, int>();
                    List<int> outputNodeIndex = new List<int>();
                    // Loop through every "layer", the next loop iterates through the OutNodes of the first layer
                    //Input multiplied with input node weights
                    for (int i = 0; i < nrInputs; i++)
                    {
                        var node = NetworkEvaluator.genomes[key].Nodes[i]; // same as i? 
                                                                           //var next
                        // multiply each input value(rays) with the weight of each ConnectionGene and add result to sum in the NodeGene ConnectionGene.OutNode  
                        foreach (var con in NetworkEvaluator.genomes[key].Connections.Where(z => z.Value.InNode == node.ID && z.Value.Expressed
                            && NetworkEvaluator.genomes[key].Nodes[z.Value.InNode].Type.Equals(NodeGene.NodeType.INPUT)))
                        {
                            rayValue = cars.CarList[key].RayList[i]; // one ray
                            NetworkEvaluator.genomes[key].Nodes[con.Value.OutNode].sum += con.Value.Weight * rayValue; // add to OutNode sum
                            if (!outnodeIndex.ContainsKey(con.Value.OutNode))
                            {
                                outnodeIndex.Add(con.Value.OutNode, 0);
                            }
                        }
                        foreach (var con in NetworkEvaluator.genomes[key].Connections.Where(z => z.Value.InNode == node.ID && z.Value.Expressed
                            && NetworkEvaluator.genomes[key].Nodes[z.Value.InNode].Type.Equals(NodeGene.NodeType.INPUT)))
                                NetworkEvaluator.genomes[key].Nodes[con.Value.OutNode].sum = (float)Math.Tanh(NetworkEvaluator.genomes[key].Nodes[con.Value.OutNode].sum);
                    }
                    // HIDDEN NODES
                    int outputCounter = 0;
                    while (outputCounter < nrOutputs)
                    {
                        var matchingKeys = NetworkEvaluator.genomes[key].Connections.Keys.Intersect(outnodeIndex.Keys);
                        //foreach (var con in NetworkEvaluator.genomes[key].Connections.Where(z => outnodeIndex.ContainsKey(z.Value.InNode)
                        // && z.Value.Expressed
                        /* && NetworkEvaluator.genomes[key].Nodes[z.Value.InNode].Type.Equals(NodeGene.NodeType.HIDDEN) */
                        //foreach (var con in NetworkEvaluator.genomes[key].Connections) if(outnodeIndex.ContainsKey(con.Value.InNode) && con.Value.Expressed)
                        foreach(var con in matchingKeys)
                        {
                            NetworkEvaluator.genomes[key].Nodes[NetworkEvaluator.genomes[key].Connections[con].OutNode].sum += 
                                NetworkEvaluator.genomes[key].Connections[con].Weight * NetworkEvaluator.genomes[key].Nodes[NetworkEvaluator.genomes[key].Connections[con].InNode].sum;
                            outnodeIndex2[NetworkEvaluator.genomes[key].Connections[con].OutNode] = 0;
                            if (NetworkEvaluator.genomes[key].Nodes[NetworkEvaluator.genomes[key].Connections[con].OutNode].Type.Equals(NodeGene.NodeType.OUTPUT))
                            {
                                outputCounter++;
                                outputNodeIndex.Add(NetworkEvaluator.genomes[key].Connections[con].OutNode);
                            }
                        }
                        foreach (var con in matchingKeys)
                            NetworkEvaluator.genomes[key].Nodes[NetworkEvaluator.genomes[key].Connections[con].OutNode].sum = (float)Math.Tanh(NetworkEvaluator.genomes[key].Nodes[NetworkEvaluator.genomes[key].Connections[con].OutNode].sum);

                            outnodeIndex = outnodeIndex2;
                    }

                    // tanH activation function for steering output [-1,1]
                    
                    float steeringOutput = (float)Math.Tanh(NetworkEvaluator.genomes[key].Nodes[outputNodeIndex[0]].sum);

                   // if (key == 0)
                   //     Debug.log("steeringoutput = " + steeringOutput);

                    // Logistic (sigmoid) activation function for throttle output [0,1]

                    //     float throttleOutput = Logistic(NetworkEvaluator.genomes[key].Nodes[outputNodeIndex[1]].sum) > 0.5f ? Logistic(NetworkEvaluator.genomes[key].Nodes[outputNodeIndex[1]].sum) : 0.5f;
                    float throttleOutput = (float)Math.Tanh(NetworkEvaluator.genomes[key].Nodes[outputNodeIndex[1]].sum);

                    if (throttleOutput < 0.0f)
                    {                     
                        throttleOutput *= -1;
                    }

                    cars.CarList[key].totThrot += throttleOutput;

                    if (timeSinceInit > 7 && carDistance[key] < 5)
                    {
                        slowPunish = 2 * timeSinceInit;
                        NetworkEvaluator.genomes[key].fitness = 0;
                        throttleOutput = 0.99f;
                        Debug.Log(carDistance[key]);
                    }
                                    

                //    if (throttleOutput < 0.3f)
                //        throttleOutput = 0.99f;

                    // if (key == 0)
                    //    Debug.Log("throttleOutput = " + throttleOutput);


                    //  3.2 Use output of networks to control the Cars
                    cars.CarList[key].SteeringAngle = steeringOutput;
                    cars.CarList[key].Velocity = throttleOutput;
                    m_Car.Move(cars.CarList[key].SteeringAngle, cars.CarList[key].Velocity, cars.CarList[key].Velocity, 0f);

                    // update fitness
                    if(throttleOutput > 0)
                        carDistance[key] += Vector3.Distance(cars.CarList[key].carObj.transform.position, carPos[key]);
                    else
                        carDistance[key] -= Vector3.Distance(cars.CarList[key].carObj.transform.position, carPos[key]);

                    carPos[key] = cars.CarList[key].carObj.transform.position;

                    if (cars.CarList[key].hasTouched || (timeSinceInit > 7 && carDistance[key] < 5))
                    {
                        foreach(NodeGene node in NetworkEvaluator.genomes[key].Nodes.Values)
                        {
                            node.sum = 0;
                        }
                        
                        //  3.3 Save fitness of each network when Cars die
                        NetworkEvaluator.genomes[key].fitness = carDistance[key] * carDistance[key];

                        Destroy(cars.CarList[key].carObj);
                        cars.CarList.Remove(key);
                        slowPunish = 0;
                        //Debug.Log("key =  " + key);
                        //Debug.Log("cars.CarList.Count =  " + cars.CarList.Count);
                    }
                }

                if (cars.CarList.Count == 0)
                {
                    // evaluate
                    NetworkEvaluator.Evaluate();
                    initCars();
                    Debug.Log("Generation: " + generation++);
                    Debug.Log("\tHighest fitness: " + NetworkEvaluator.GetHighestFitness());
                    Debug.Log("\tAmount of species: " + NetworkEvaluator.GetSpeciesAmount());
                }
                
            }
        }
    }
}