using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PlayArea : MonoBehaviour
{
    private GenerateStereoCubemaps gsc;
    public GameObject area;
    public int MinHeight;
    private int Width = 1000;
    private int Height = 1000;
    private int Depth = 1000;
    private int space = 100;
    private float raycastDistance = 100f;

    private void Awake()
    {
        gsc = Camera.main.GetComponent<GenerateStereoCubemaps>();
    }
    
    public void PlaceAreas()
    {
        for (int x = -Width; x < Width; x += space)
        {
            for (int y = MinHeight; y < Height; y += space)
            {
                for (int z = -Depth; z < Depth; z += space)
                {
                    if (x > -Width && x < Width - 1
                        && y > MinHeight && y < Height - 1 &&
                        z > -Depth && z < Depth - 1)
                    {
                        PositionRaycast(x, y, z);
                    }
                    continue;
                }
            }
        }
    }

    void PositionRaycast(int x, int y, int z)
    {
        RaycastHit hit;

        if (Physics.Raycast(new Vector3(x, y, z), Vector3.down, out hit, raycastDistance) && y > MinHeight)
        {
            Instantiate(area, hit.point, Quaternion.identity);
            transform.position = hit.point + hit.normal * 2;
            gsc.Generate();
        }
    }
}
