using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class pointSpawn : MonoBehaviour
{
    List<List<List<float>>> particlesList = new List<List<List<float>>>();
    List<List<GameObject>> objectsList = new List<List<GameObject>>();
    // Adding elements to the list

    public GameObject pointPrefab;
    public float damping = 0f;
    // Number of lists to add
    int n = 3;

    int[] counts = new int[] {100,100,0};
    List<Color> colors = new List<Color>();
    int colorsCount = 5; // replace 5 with the number of colors you want

    public float width = 1f;
    public float height = 1f;
    public float bounceDamping = 0.9f;
    // public float bounceCorrection = 0.1f;
    public float forceMultiplier = 1f;
    public float collisionMultiplier = 0f;
    // Start is called before the first frame update
    void Start()
    {
        float x, y;
        float vx, vy;

        for (int i = 1; i <= colorsCount; i++)
        {

            float r = (float)(int)(i%8/4);
            float g = (float)(int)(i%4/2);
            float b = i%2;
            colors.Add(new Color(r, g, b));
        }

        // Adds n lists to the particlesList and objectsList
        for (int i = 0; i < n; i++)
        {
            particlesList.Add(new List<List<float>>());
            objectsList.Add(new List<GameObject>());
        }

        // These loop through every combination of points
        // The first iterates through each list of particles
        for(int ilist = 0; ilist < particlesList.Count; ilist++){
            // This iterates through each point in that list
            for(int ipoint = 0; ipoint < counts[ilist]; ipoint++){

                List<float> point = new List<float>();
                // Generate random x and y values
                x = Random.Range(0f, 10f);
                y = Random.Range(0f, 10f);

                // Set velocities to 0
                vx = 0f;
                vy = 0f;

                // Add the random coordinate to the point list
                point.Add(x);
                point.Add(y);
                point.Add(vx);
                point.Add(vy);
                // Add the point to the respective list of particles
                particlesList[ilist].Add(point);

                // Create a circle sprite object primitive
                Vector2 spawnPosition = new Vector2(0f, 0f);        
                Quaternion spawnRotation = Quaternion.identity;
                GameObject circle = Instantiate(pointPrefab, spawnPosition, spawnRotation);
                circle.GetComponent<SpriteRenderer>().color = colors[ilist];
                objectsList[ilist].Add(circle);
            }
        }
    }

     

    // Update is called once per frame
    void Update()
    {
        float[] attractionForce = new float[]{0.0f, 0.0f};
        List<float> ilistPoint, jlistPoint;

        // These loop through every combination of points
        // The first iterates through each list of particles
        for(int ilist = 0; ilist < particlesList.Count; ilist++){
            // This iterates through each point in that list
            for(int ipoint = 0; ipoint < particlesList[ilist].Count; ipoint++){
                attractionForce = new float[]{0.0f, 0.0f};
                // This iterates through every list of particles from the current list onwards
                for(int jlist = 0; jlist < particlesList[jlist].Count; jlist++){
                    // This iterates through every point in the other list of particles
                    for(int jpoint = 0; jpoint < counts[jlist]; jpoint++){
                        // If the two lists are the same, then we only want to iterate through the points after the current point to prevent duplicates
                        if(ilist == jlist & jpoint ==ipoint){
                            continue;
                        }
                        ilistPoint = particlesList[ilist][ipoint];
                        jlistPoint = particlesList[jlist][jpoint];
                        attractionForce = AddArrays(attractionForce, attraction(ilistPoint[0], ilistPoint[1], jlistPoint[0], jlistPoint[1], ilist, jlist));                       
                    }
                }

                // Update the velocity of the current point
                particlesList[ilist][ipoint][2] += attractionForce[0]*Time.deltaTime;
                particlesList[ilist][ipoint][3] += attractionForce[1]*Time.deltaTime;

                particlesList[ilist][ipoint][2] += (-Mathf.Pow((particlesList[ilist][ipoint][2]),2)*particlesList[ilist][ipoint][2]/
                                    Mathf.Abs(particlesList[ilist][ipoint][2])*Time.deltaTime*damping);

                particlesList[ilist][ipoint][3] += (-Mathf.Pow((particlesList[ilist][ipoint][3]),2)*particlesList[ilist][ipoint][3]/
                                    Mathf.Abs(particlesList[ilist][ipoint][3])*Time.deltaTime*damping);

                // Update the position of the current point
                particlesList[ilist][ipoint][0] += particlesList[ilist][ipoint][2]*Time.deltaTime;
                particlesList[ilist][ipoint][1] += particlesList[ilist][ipoint][3]*Time.deltaTime;

                if(particlesList[ilist][ipoint][0] is float.NaN){
                    particlesList[ilist][ipoint][0] = width/2f;
                }
                if(particlesList[ilist][ipoint][1] is float.NaN){
                    particlesList[ilist][ipoint][1] = height/2f;
                }

                // Restrict movement to the screen
                if( particlesList[ilist][ipoint][0] > width){
                    particlesList[ilist][ipoint][0] = width - (particlesList[ilist][ipoint][0] - width);
                    particlesList[ilist][ipoint][2] *= -bounceDamping;
                }
                if( particlesList[ilist][ipoint][0] < 0f){
                    particlesList[ilist][ipoint][0] = -particlesList[ilist][ipoint][0];
                    particlesList[ilist][ipoint][2] *= -bounceDamping;
                }
                if( particlesList[ilist][ipoint][1] > height){
                    particlesList[ilist][ipoint][1] = height - (particlesList[ilist][ipoint][1] - height);
                    particlesList[ilist][ipoint][3] *= -bounceDamping;
                }
                if( particlesList[ilist][ipoint][1] < 0f){
                    particlesList[ilist][ipoint][1] = -particlesList[ilist][ipoint][1];
                    particlesList[ilist][ipoint][3] *= -bounceDamping;
                }

                objectsList[ilist][ipoint].transform.position = new Vector2(particlesList[ilist][ipoint][0], particlesList[ilist][ipoint][1]);
            }
        }
    }

    public float[] attraction(float x1, float y1, float x2, float y2, int pointType1 = 0, int pointType2 = 0)
    {
        float r = Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2));
        float F = 1 / r;

        // if(r < 0.1f){
        //     r = 0.1f;
        // }
        // I believe this just ends up being an approximation
        float yForce = F*( y2 - y1 )/Mathf.Pow(r,2);
        float xForce = F*( x2 - x1 )/Mathf.Pow(r,2);

        if (r <= 0.1f)
        {
            xForce *= -collisionMultiplier;
            yForce *= -collisionMultiplier;
        }
        else if( pointType1 != pointType2){
            xForce *= -forceMultiplier;
            yForce *= -forceMultiplier;
        }
        else{
            xForce *= forceMultiplier;
            yForce *= forceMultiplier;
        }

        return new float[] { xForce*.1f, yForce*.1f };
    }


    public float[] AddArrays(float[] array1, float[] array2)
    {
        float[] result = array1.Zip(array2, (x, y) => x + y).ToArray();
        return result;
    }
}
