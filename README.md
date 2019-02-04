# A NEAT Driving Simulator
3D simulation of cars learning to drive through a track using the NEAT algorithm (Neural Evolution through Augmenting Topologies) and the Unity game engine.

## Background

The NEAT algorithm was presented by Kenneth O. Stanley and Risto Miikkulainen, described in detail in their 2002 paper Evolving Neural Networks through Augmenting Topologies.  NEAT  is  an  algorithm that  is  in  the  area  of  Neuroevolution  (NE),  which is the concept of evolving artificial neural networks using a genetic algorithm. What sets the NEAT algorithm apart from most NE algorithms is that it allows not only for the weights of the networks to be changed, but the topology of the networks themselves, as the name suggests.  This means that hidden nodes and connections between nodes can be added/removed.  The result will not necessarily have to be a fully connected neural network.

The implementation of the NEAT algorithm was also inspired by the following video series by Hydrozoa: https://www.youtube.com/watch?v=1I1eG-WLLrY&list=PLdwBZvqyaXGZhGlL1H40qDoszkJctgzEC

The simulation is based on Udacity´s Self-Driving Car Simulator: https://github.com/udacity/self-driving-car-sim

## Program basics
The program allows for the spawning of an arbitrary amount of cars at the start of the track. Each car gets "assigned" a neural network evolved through the NEAT algorithm which tells the car when and how much to throttle and turn the wheel. Each car continuously measures the distance between the walls and five vectors pretruding at different angles from the front of the car. These five distances serve as the input to the respective neural networks, while the output is throttle and steering angle. Each car gets destroyed when hitting a side bank/wall. When all cars have been destroyed, the newly evolved and surviving networks gets assigned to a whole new batch of cars which will hopefully have learned something more about driving safely through the track. This process is repeated until the results are safisfying enough.


# Authors:
- [Rasmus Ståhl](https://github.com/Mt-Rasmus)
- [Anton Sterner](https://github.com/antonsterner)
