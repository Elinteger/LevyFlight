using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

/*
a "just for fun" implementation of Lévy flight --- https://en.wikipedia.org/wiki/L%C3%A9vy_flight
*/

public class Levi : MonoBehaviour
{
	Camera cam;
	UnityEngine.Vector3 viewportPos;

	// to determine how often the simulation plays, time between each calculation
	public float simulationTime = 0.5f;  // 0,5s
	float timePassed = 0; 

	// levy exponent
	// a == 1, heavy tail, lots of large jumps
	// a == 2, light tail, close to gaussian like behavior (brownian motion?)
	// 0 < alpha < 2, otherwise no variance as <x^2> == inf
	public float alpha = 1.5f;

	// [0, 2pi)
	float direction;

	// current position + position to walk to 
	UnityEngine.Vector2 curPosition;
	UnityEngine.Vector2 newPosition;

	// the actual step 
	UnityEngine.Vector2 step;

	// for mategnas algorithm (algo to create a stable levy distribution)
	System.Random rand;  // to generate random Numbers for randomGaussian()
	UnityEngine.Vector2 gaussValues;
	float stepLength;
	float u, v;  // random gaussian values
	public float scale = 1;

	// makes the path drawn by the dot visible
	LineRenderer lr;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		if (cam  == null) {
			 Debug.LogError("No Camera with Tag  \"MainCamera\" found in scene.");
			return;
		}
		lr = gameObject.GetComponentInChildren<LineRenderer>();
		if (lr == null) {
			Debug.LogError("No LineRenderer found as a child of Levi.");
			return;
		}

		rand = new System.Random();
		lr.positionCount++;
		lr.SetPosition(lr.positionCount - 1, transform.position);
	}


	// Update is called once per frame
	void Update()
	{
		// only simulate every <simulationTime>
		timePassed += Time.deltaTime;
		if (timePassed >= simulationTime) {
			timePassed = 0;
			ComputeLevyFlight();
		}
	}


	void ComputeLevyFlight() {
		// only take a step into a direction that is inside the camera's fov
		do {
			/// start
			// everything inbetween start and end is bad performancewise, but its a fast workaround to stop edge cases in which it would freeze
			// freezes because step size is so big, it cant go anywhere

			// 0. draw two random values following a gaussian distributions and get current values
			gaussValues = RandomGaussianMPM();
			u = gaussValues.x;
			v = gaussValues.y;

			curPosition = transform.position;

			// 1. determine a step length using mantegnas algorithm 
			stepLength = scale * (float)(u / Math.Pow(Math.Abs(v), 1 / alpha)); 

			///end

			// 2. choose a random direction [0, 2*Pi)
			do {
				direction = (float)(rand.NextDouble() * (2 * Math.PI));
			} while (direction == 0);
			
			// 3. calculate new step
			step =  new UnityEngine.Vector2((float)(stepLength * Math.Cos(direction)), (float)(stepLength * Math.Sin(direction)));
			
			// 3.5 calculate new position and put it into the viewport (to check if inside or not)
			newPosition = curPosition + step;
			viewportPos = cam.WorldToViewportPoint(newPosition); 
		} while (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 ||  viewportPos.y > 1);

		// 4. apply transform 
		transform.position = UnityEngine.Vector2.MoveTowards(curPosition, newPosition, float.PositiveInfinity);

		// (5. draw line)
		lr.positionCount++;
		lr.SetPosition(lr.positionCount - 1, transform.position);
	}


	// a function to calculate two random numbers following a gaussian distribution - not used in the current code, just done out of interest
	// implementation of the Box-Muller transform: https://en.wikipedia.org/wiki/Box%E2%80%93Muller_transform
	// using this explanation: https://www.alanzucconi.com/2015/09/16/how-to-sample-from-a-gaussian-distribution/
	UnityEngine.Vector2 RandomGaussianBM() {
		// generate two random numbers -> (0, 1]
		float u1 = 1f - (float)rand.NextDouble();
		float u2 = 1f - (float)rand.NextDouble();
		// use those to calculate radius and angle of our random point 
		float r = MathF.Sqrt(-2f * MathF.Log(u1));
		float a = 2 * MathF.PI * u2;
		// convert polar coordiantes (r,a) to cartesian coordinates
		return new UnityEngine.Vector2(r * MathF.Cos(a), r * MathF.Sin(a));
	}


	// another function to calculate a pair of random numbers following a gaussian distribution
	// more efficient as it doesn't use trigonometric functions
	// implementation of the Marsaglia polar method: https://en.wikipedia.org/wiki/Marsaglia_polar_method
	UnityEngine.Vector2 RandomGaussianMPM() {
		float v1, v2, s;
		// create a uniformily distributed point in the interval (-1, 1) which 
		// cannot be the origin (0, 0)
		do {
			v1 = 2f * UnityEngine.Random.Range(0f, 1f) - 1f;
			v2 = 2f * UnityEngine.Random.Range(0f, 1f) - 1f;
			s = v1 * v1 + v2 * v2;
		 } while (s >= 1.0f || s == 0f);

		s = MathF.Sqrt(-2f * Mathf.Log(s)) / s;
		return new UnityEngine.Vector2(v1 * s, v2 * s);
	}
}
