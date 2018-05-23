using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class Spline : MonoBehaviour {

    public MeshFilter splineMeshFilter;
    private Mesh splineMesh;

    public float splineWidth;
    public float splineHeight;

    public float incrementTowardSplinePoint;

    public Vector3[] newVertices;
    public Vector2[] newUV;
    public int[] newTriangles;

    private int currentVertex;
    private int currentUV;
    private int currentIndex;
    private int currentTrianglePoint;

    private float currentX;
    private float currentY;
    private float currentZ;

    private float currentU;
    private float currentV;

    private const int numFaces = 4;
    private const int rectSides = 4;
    private const int triangleFaces = 2;
    private const int triangleVertexNumber = 3;
    // Use this for initialization
    void Start ()
    {
        newVertices = new Vector3[numFaces * rectSides];
        newUV = new Vector2[numFaces * rectSides];
        newTriangles = new int[triangleFaces * triangleVertexNumber * numFaces];

        splineMesh = new Mesh();
        splineMesh.name = "Mesh";
        splineMesh.Clear();

        currentVertex = 0;
        currentUV = 0;
        currentIndex = 0;
        currentTrianglePoint = 0;

        BuildFrontSide();      
        BuildTopSide();
        BuildRightSide();
        BuildBottomSide();

        splineMesh.vertices = newVertices;
        splineMesh.uv = newUV;
        splineMesh.triangles = newTriangles;

        splineMeshFilter.mesh = splineMesh;

        splineMesh.RecalculateNormals();
        splineMesh.RecalculateBounds();
	}

    void AddTrianglesFromSide()
    {
        newTriangles[currentIndex++] = currentTrianglePoint;
        newTriangles[currentIndex++] = currentTrianglePoint + 2;
        newTriangles[currentIndex++] = currentTrianglePoint + 1;
        newTriangles[currentIndex++] = currentTrianglePoint + 1;
        newTriangles[currentIndex++] = currentTrianglePoint + 2;
        newTriangles[currentIndex++] = currentTrianglePoint + 3;

        currentTrianglePoint += 4;
    }

    void BuildFrontSide()
    {
        //Initialize points in space     
        currentX = -splineWidth / 2.0f;
        currentY = -splineHeight / 2.0f;
        currentZ = 0;

        //Initialize texture coordinate 
        currentU = 0.0f;
        currentV = 0.0f;

        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                newVertices[currentVertex++] = new Vector3(currentX, currentY, currentZ);
                newUV[currentUV++] = new Vector2(currentU, currentV);

                currentX += splineWidth;

                currentU += 1.0f;
            }
            currentX -= splineWidth * 2;
            currentU = 0.0f;

            currentY += splineHeight;
            currentV += 1.0f;
        }
        currentY -= splineHeight;

        AddTrianglesFromSide();
    }

    void BuildTopSide()
    {
        //Initialize texture coordinate 
        currentU = 0.0f;
        currentV = 0.0f;

        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                newVertices[currentVertex++] = new Vector3(currentX, currentY, currentZ);
                newUV[currentUV++] = new Vector2(currentU, currentV);

                currentX += splineWidth;

                currentU += 1.0f;
            }
            currentX -= splineWidth * 2;
            currentU = 0.0f;

            currentZ += incrementTowardSplinePoint;
            currentV += 1.0f;
        }
        currentX += splineWidth;
        currentZ -= incrementTowardSplinePoint;

        AddTrianglesFromSide();
    }

    void BuildRightSide()
    {
        //Initialize texture coordinate 
        currentU = 0.0f;
        currentV = 0.0f;

        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                newVertices[currentVertex++] = new Vector3(currentX, currentY, currentZ);
                newUV[currentUV++] = new Vector2(currentU, currentV);

                currentZ -= incrementTowardSplinePoint;

                currentU += 1.0f;
            }
            currentZ += incrementTowardSplinePoint * 2;
            currentU = 0.0f;

            currentY -= splineHeight;
            currentV += 1.0f;
        }
        currentY += splineHeight;

        AddTrianglesFromSide();
    }

    void BuildBottomSide()
    {
        //Initialize texture coordinate 
        currentU = 0.0f;
        currentV = 0.0f;

        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                newVertices[currentVertex++] = new Vector3(currentX, currentY, currentZ);
                newUV[currentUV++] = new Vector2(currentU, currentV);

                currentZ -= incrementTowardSplinePoint;

                currentU += 1.0f;
            }
            currentZ += incrementTowardSplinePoint * 2;
            currentU = 0.0f;

            currentX -= splineWidth;
            currentV += 1.0f;
        }
        currentX += splineWidth;

        AddTrianglesFromSide();
    }
}
