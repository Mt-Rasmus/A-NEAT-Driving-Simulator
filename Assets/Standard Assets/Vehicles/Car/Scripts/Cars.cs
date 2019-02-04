using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{

    public class Cars : MonoBehaviour
    {
        private GameObject CarPrefab;
        public int nr_of_cars;
        public Dictionary<int, CarClass> CarList = new Dictionary<int, CarClass>();
        GameObject CarSpawnPoint = GameObject.FindGameObjectWithTag("CarSpawnPt");

        // Constructor
        public Cars(int nr_of_cars_, GameObject carGameObject)
        {
            Vector3 spawn = new Vector3(CarSpawnPoint.transform.position.x + 0.001f, CarSpawnPoint.transform.position.y, CarSpawnPoint.transform.position.z);

            nr_of_cars = nr_of_cars_;
            CarPrefab = carGameObject;

            for (int i = 0; i < nr_of_cars; i++)
            {
                spawn = new Vector3(spawn.x - 0.001f, spawn.y, spawn.z);

                GameObject spawnedCar = Instantiate(CarPrefab, spawn, CarSpawnPoint.transform.rotation);
                CarClass newcar = new CarClass(spawnedCar, 0.0f, 0.0f);

                CarList.Add(i, newcar);
            }
        }

        public CarClass Get(int key)
        {
            CarClass result = null;

            if (CarList.ContainsKey(key))
            {
                result = CarList[key];
            }

            return result;
        }

        // Use this for initialization
        void Start()
        {
/*
            nr_of_cars = 5;

            for (int i = 0; i < nr_of_cars; i++)
            {
                GameObject spawnedCar = Instantiate(CarPrefab, transform.position, transform.rotation);
                CarClass newcar = new CarClass(spawnedCar, 0.0f, 0.0f);

                CarList.Add(i, newcar);
            }
*/
        }

        // Update is called once per frame
        void Update()
        {
            Debug.Log("AAAAACCCCCCCCCCCCCCCCCCCCCCCCCAAAAAA");
        }
    }

}