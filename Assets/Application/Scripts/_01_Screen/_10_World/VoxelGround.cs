using UnityEngine;

public class VoxelGround : MonoBehaviour
{
    private float sizeX = 50f;
    private float sizeY = 10f;
    private float sizeZ = 50f;
    private float sizeW = 5f;
    private void Awake()
    {
        var material = this.GetComponent<MeshRenderer>().material;
        for (float x = 0; x < sizeX; x++)
        {
            for (float z = 0; z < sizeZ; z++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(transform);
                float noise = Mathf.PerlinNoise(x/ sizeW, z/ sizeW);
                float y = Mathf.Round(sizeY * noise);
                cube.transform.localPosition = new Vector3(x, y, z);
            }
        }
        transform.localPosition = new Vector3(-sizeX / 2, 0, -sizeZ / 2);
    }
}