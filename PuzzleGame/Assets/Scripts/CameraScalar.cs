using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraScalar : MonoBehaviour
{
    private Normal.Board mBoard;
    private Hexa.Board mHexaBoard;

    public float cameraOffset = -20;
    public float aspectRatio = 0.625f;
    public float padding = 2;
    public float yOffset = 1;

    // Start is called before the first frame update
    void Start()
    {
        mBoard = FindObjectOfType<Normal.Board>();
        mHexaBoard = FindObjectOfType<Hexa.Board>();

        if (mBoard != null)
        {
            RepositionCamera(mBoard.width - 1, mBoard.height - 1, mBoard.width, mBoard.height);
        }
        else if(mHexaBoard != null)
        {
            RepositionCamera(mHexaBoard.totalWidth - 1, mHexaBoard.maxHeight - 1, mHexaBoard.totalWidth, mHexaBoard.maxHeight);
        }
    }

    void RepositionCamera(float x, float y, float max_x, float max_y)
    {
        Vector3 tmpPos = new Vector3(x / 2, y / 2 + yOffset, cameraOffset);
        transform.position = tmpPos;
        if(max_x >= max_y)
        {
            Camera.main.orthographicSize = (max_x / 2 + padding) / aspectRatio;
        }
        else
        {
            Camera.main.orthographicSize = max_y / 2 + padding;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
