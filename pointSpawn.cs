using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class pointSpawn : MonoBehaviour
{
    List<List<float[]>> particlesList = new List<List<float[]>>();
    List<List<GameObject>> objectsList = new List<List<GameObject>>();
    // Adding elements to the list

    public GameObject pointPrefab;
    public float damping = 0f;
    // Number of lists to add
    int n = 3;

    int[] counts = new int[] {50,50,100};
    List<Color> colors = new List<Color>();
    int colorsCount = 5; // replace 5 with the number of colors you want

    public float width = 1f;
    public float height = 1f;
    public float bounceDamping = 0.9f;
    // public float bounceCorrection = 0.1f;
    public float forceMultiplier = 1f;
    public float collisionMultiplier = 0f;
    // Start is called before the first frame update
    public int meshResolution = 1;
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
            particlesList.Add(new List<float[]>());
            objectsList.Add(new List<GameObject>());
        }

        // These loop through every combination of points
        // The first iterates through each list of particles
        for(int ilist = 0; ilist < particlesList.Count; ilist++){
            // This iterates through each point in that list
            for(int ipoint = 0; ipoint < counts[ilist]; ipoint++){

                float[] point = new float[5];
                // Generate random x and y values
                x = Random.Range(0f, 10f);
                y = Random.Range(0f, 10f);

                // Set velocities to 0
                vx = 0f;
                vy = 0f;

                // Add the random coordinate to the point list
                point[0] = x; // x
                point[1] = y; // y
                point[2] = vx; // vx (velocity in x direction)
                point[3] = vy; // vy (velocity in y direction)
                point[4] = ilist; // type of point

                // Add the point to the respective list of particles
                particlesList[ilist].Add(point);

                // Create a circle sprite object primitive
                Vector2 spawnPosition = new Vector2(point[0], point[1]);        
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
        float[] ilistPoint, jlistPoint;


        List<float[]>[,] particlesGrid = createParticlesGroup(meshResolution);

        // These loop through every combination of points
        // The first iterates through each list of particles
        for(int ilist = 0; ilist < particlesList.Count; ilist++){
            // This iterates through each point in that list
            for(int ipoint = 0; ipoint < particlesList[ilist].Count; ipoint++){
                
                // // This iterates through every list of particles from the current list onwards
                // for(int jlist = 0; jlist < particlesList[jlist].Count; jlist++){
                //     // This iterates through every point in the other list of particles
                //     for(int jpoint = 0; jpoint < counts[jlist]; jpoint++){
                //         // If the two lists are the same, then we only want to iterate through the points after the current point to prevent duplicates
                //         if(ilist == jlist & jpoint ==ipoint){
                //             continue;
                //         }
                //         ilistPoint = particlesList[ilist][ipoint];
                //         jlistPoint = particlesList[jlist][jpoint];
                        
                        
                //     }
                // }

                List<float[]> neighbors = getNeighbors(particlesList[ilist][ipoint][0], particlesList[ilist][ipoint][1], particlesGrid, 1);
                // Debug.Log(neighbors.Count);

                // Debug.Log("NEIGHBORS COUNT: " + neighbors.Count);
                
                if(neighbors.Count == 0){
                    continue;
                }

                attractionForce = new float[]{0.0f, 0.0f};
                ilistPoint = particlesList[ilist][ipoint];
                // Debug.Log(ilistPoint[0] + ", " + ilistPoint[1]);
                float[] attractionForceTmp = new float[2];

                for(int i = 0; i < neighbors.Count; i++){
                    
                    jlistPoint = neighbors[i];
                    // Debug.Log("JLISTPOINT"+ jlistPoint[0] + ", " + jlistPoint[1]);
                    attractionForceTmp = attraction(ilistPoint[0], ilistPoint[1], jlistPoint[0], jlistPoint[1], (int) ilistPoint[4], (int) jlistPoint[4]);
                    // Debug.Log("Attraction Force Tmp: " + attractionForceTmp[0] + ", " + attractionForceTmp[1]);

                    attractionForce[0] += attractionForceTmp[0];
                    attractionForce[1] += attractionForceTmp[1];
                }
                // Debug.Log("Attraction Force: " + attractionForce[0] + ", " + attractionForce[1]);
                
                // Update the velocity of the current point
                particlesList[ilist][ipoint][2] += attractionForce[0]*Time.deltaTime;
                particlesList[ilist][ipoint][3] += attractionForce[1]*Time.deltaTime;

                // Dampen the velocity of the current point
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

    public List<float[]>[,] createParticlesGroup(int n) {
        List<float[]>[,] particlesGrid = new List<float[]>[n,n];

        for(int ilist = 0; ilist < particlesList.Count; ilist++){
            for(int ipoint = 0; ipoint < particlesList[ilist].Count; ipoint++){
                int x = (int) (particlesList[ilist][ipoint][0] * (n / width));
                int y = (int) (particlesList[ilist][ipoint][1] * (n / height));
                if (x < 0) x = 0;
                if (y < 0) y = 0;
                if (x >= n) x = n-1;
                if (y >= n) y = n-1;

                if(particlesGrid[(int) (x),(int) (y)] == null){
                    particlesGrid[(int) (x),(int) (y)] = new List<float[]> {particlesList[ilist][ipoint]};
                } else {
                    particlesGrid[(int) (x),(int) (y)].Add(particlesList[ilist][ipoint]);
                }
            }
        }

        return particlesGrid;
    }

    public List<float[]> getNeighbors(float x, float y, List<float[]>[,] particlesGroup, int closestNeighbors = 1){
        List<float[]> neighbors = new List<float[]>();

        int xIndex = (int) (x*(particlesGroup.GetLength(0)/width));
        int yIndex = (int) (y*(particlesGroup.GetLength(1)/height));

        for(int i = -closestNeighbors; i <= closestNeighbors; i++){
            for(int j = -closestNeighbors; j <= closestNeighbors; j++){
                if(xIndex+i < 0 || xIndex+i >= n/width || yIndex+j < 0 || yIndex+j >= n/height){
                    continue;
                }
                if(particlesGroup[xIndex+i, yIndex+j] != null){
                    for(int k = 0; k < particlesGroup[xIndex+i, yIndex+j].Count; k++){
                        neighbors.Add(particlesGroup[xIndex+i, yIndex+j][k]);
                    }
                }
            }
        }

        return neighbors;
    }

    public float[] attraction(float x1, float y1, float x2, float y2, int pointType1 = 0, int pointType2 = 0)
    {
        float r = Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2));
        // Prevents division by 0
        if(r < 0.1f){
            r = 0.1f;
        }

        float F = 1 / r;

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
        // Debug.Log("R: " + r);
        // Debug.Log("XFORCE: " + xForce + ", YFORCE: " + yForce);
        return new float[] { xForce, yForce };
    }


    public float[] AddArrays(float[] array1, float[] array2)
    {
        float[] result = array1.Zip(array2, (x, y) => x + y).ToArray();
        return result;
    }
}
