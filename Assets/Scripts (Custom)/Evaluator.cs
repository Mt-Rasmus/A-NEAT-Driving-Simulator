using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;

namespace NEAT
{
    public class Evaluator : MonoBehaviour
    {

        /**
          * Class Evaluator
          */

        private Counter nodeInnovation;
        private Counter connectionInnovation;

        private Random random = new Random();

        // Constants for tuning 
        private float C1 = 1.0f;
        private float C2 = 1.0f;
        private float C3 = 0.4f;
        private float DT = 10.0f;
        private float MUTATION_RATE = 0.5f;
        private float ADD_CONNECTION_RATE = 0.1f;
        private float ADD_NODE_RATE = 0.1f;

        private int populationSize;

        private List<Genome> genomes; // all networks
        private List<Genome> nextGenGenomes; 
        private List<Species> species;
        private Dictionary<Genome, Species> speciesMap; // which species each Genome belong to 
        private Dictionary<Genome, float> fitnessMap; // fitness of each Genome
        private float highestFitness;
        private Genome fittestGenome;

        public Evaluator(int populationSize, Genome startingGenome, Counter nodeInnovation, Counter connectionInnovation)
        {
            this.populationSize = populationSize;
            this.nodeInnovation = nodeInnovation;
            this.connectionInnovation = connectionInnovation;
            genomes = new List<Genome>(populationSize);

            for (int i = 0; i < populationSize; i++)
            {
                genomes.Add(new Genome(startingGenome));
            }
            nextGenGenomes = new List<Genome>(populationSize);
            speciesMap = new Dictionary<Genome, Species>();
            fitnessMap = new Dictionary<Genome, float>();
            species = new List<Species>();
        }

        public int getSpeciesAmount()
        {
            return species.Count;
        }

        public float getHighestFitness()
        {
            return highestFitness;
        }

        public Genome getFittestGenome()
        {
            return fittestGenome;
        }

        // Runs one generation

        public void Evaluate()
        {
            // Reset everything for next generation
            foreach (Species s in species)
            {
                s.Reset(random);
            }
            fitnessMap.Clear();
            speciesMap.Clear();
            nextGenGenomes.Clear();
            highestFitness = float.MinValue;
            fittestGenome = null;

            // Place genomes into species
            foreach (Genome g in genomes)
            {
                bool foundSpecies = false;
                foreach (Species s in species)
                {
                    if (Genome.CompatibilityDistance(g, s.mascot, C1, C2, C3) < DT)
                    { // compatibility distance is less than DT, so genome belongs to this species
                        s.members.Add(g);
                        speciesMap[g] = s;
                        foundSpecies = true;
                        break;
                    }
                }
                if (!foundSpecies)
                { // if there is no appropiate species for genome, make a new one
                    Species newSpecies = new Species(g);
                    species.Add(newSpecies);
                    speciesMap[g] = newSpecies;
                }
            }

            // Remove unused species

            foreach (Species s in species)
            {
                if (!s.members.Any())
                {
                    species.Remove(s);
                }
            }


            // Evaluate genomes and assign score
            foreach (Genome g in genomes)
            {
                Species s = speciesMap[g];       // Get species of the genome

                float score = evaluateGenome(g); // fitness of Genome
                float adjustedScore = score / speciesMap[g].members.Count;

                s.AddAdjustedFitness(adjustedScore);
                s.fitnessPop.Add(new FitnessGenome(g, adjustedScore));
                fitnessMap[g] = adjustedScore;
                if (score > highestFitness)
                {
                    highestFitness = score;
                    fittestGenome = g;
                }
            }

           // put best genome from each species into next generation
           // species = species.OrderByDescending(s => s.fitnessPop).ToList();
            foreach (Species s in species)
            {
                FitnessGenome fittestInSpecies = s.fitnessPop.Max();
                nextGenGenomes.Add(fittestInSpecies.genome);
            }

            // Breed the rest of the genomes
            while (nextGenGenomes.Count < populationSize)
            { // replace removed genomes by randomly breeding
                Species s = getRandomSpeciesBiasedAjdustedFitness(random);

                Genome p1 = getRandomGenomeBiasedAdjustedFitness(s, random);
                Genome p2 = getRandomGenomeBiasedAdjustedFitness(s, random);

                Genome child = new Genome();
                if (fitnessMap[p1] >= fitnessMap[p2])
                {
                    child.Crossover(p1, p2);
                }
                else
                {
                    child.Crossover(p2, p1);
                }
                if (Random.Range(0f, 1f) < MUTATION_RATE)
                {
                    child.Mutation();
                }
                if (Random.Range(0f, 1f) < ADD_CONNECTION_RATE)
                {
                    //Debug.Log("Adding connection mutation...");
                    child.AddConnectionMutation(connectionInnovation, 10);
                }
                if (Random.Range(0f, 1f) < ADD_NODE_RATE)
                {
                    //Debug.Log("Adding node mutation...");
                    child.AddNodeMutation(connectionInnovation, nodeInnovation);
                }
                nextGenGenomes.Add(child);
            }

            genomes = nextGenGenomes;
            nextGenGenomes = new List<Genome>();
        }


        // Selects a random species from the species list, where species with a higher total adjusted fitness have a higher chance of being selected

        private Species getRandomSpeciesBiasedAjdustedFitness(Random random)
        {
            double completeWeight = 0.0;    // sum of probablities of selecting each species - selection is more probable for species with higher fitness
            foreach (Species s in species)
            {
                completeWeight += s.totalAdjustedFitness;
            }
            double r = Random.Range(0f, 1f) * completeWeight;
            double countWeight = 0.0;
            foreach (Species s in species)
            {
                countWeight += s.totalAdjustedFitness;
                if (countWeight >= r)
                {
                    return s;
                }
            }
            Debug.Log("Couldn't find a species... Number is species in total is " + species.Count + ", and the total adjusted fitness is " + completeWeight);
            return null;
        }


        // Selects a random genome from the species chosen, where genomes with a higher adjusted fitness have a higher chance of being selected

        private Genome getRandomGenomeBiasedAdjustedFitness(Species selectFrom, Random random)
        {
            double completeWeight = 0.0;    // sum of probablities of selecting each genome - selection is more probable for genomes with higher fitness
            foreach (FitnessGenome fg in selectFrom.fitnessPop)
            {
                completeWeight += fg.fitness;
            }
            double r = Random.Range(0f, 1f) * completeWeight;
            double countWeight = 0.0;
            foreach (FitnessGenome fg in selectFrom.fitnessPop)
            {
                countWeight += fg.fitness;
                if (countWeight >= r)
                {
                    return fg.genome;
                }
            }
            Debug.Log("Couldn't find a genome... Number is genomes in selected species is " + selectFrom.fitnessPop.Count + ", and the total adjusted fitness is " + completeWeight);
            return null;
        }

        

        // calculate fitness of Genome, must know result of each Car
        protected float evaluateGenome(Genome genome)
        {
            float fitness = 0;

            return fitness;
        }

        public class FitnessGenome
        {

            public float fitness { get; set; }
            public Genome genome { get; set; }

            public FitnessGenome(Genome genome, float fitness)
            {
                this.genome = genome;
                this.fitness = fitness;
            }
        }

        public class Species
        {

            public Genome mascot { get; set; }
            public List<Genome> members { get; set; }
            public List<FitnessGenome> fitnessPop { get; set; }
            public float totalAdjustedFitness = 0f;

            public Species(Genome mascot)
            {
                this.mascot = mascot;
                this.members = new List<Genome>();
                this.members.Add(mascot);
                this.fitnessPop = new List<FitnessGenome>();
            }

            public void AddAdjustedFitness(float adjustedFitness)
            {
                this.totalAdjustedFitness += adjustedFitness;
            }

            //	 Selects new random mascot + clear members + set totaladjustedfitness to 0f

            public void Reset(Random r)
            {
                int newMascotIndex = Random.Range(0, members.Count);
                this.mascot = members[newMascotIndex];
                members.Clear();
                fitnessPop.Clear();
                totalAdjustedFitness = 0f;
            }
        }

    }
}
