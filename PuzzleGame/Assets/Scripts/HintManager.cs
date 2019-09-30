using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    private Board board;
    public float hintDelay;
    private float hintDelaySecond;
    public GameObject hintParticle;
    public GameObject currentHint;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        hintDelaySecond = hintDelay;
    }

    // Update is called once per frame
    void Update()
    {
        hintDelaySecond -= Time.deltaTime;
        if(hintDelaySecond <= 0 && currentHint == null)
        {
            MarkHint();
            hintDelaySecond = hintDelay;
        }
    }
    
    List<GameObject> FindAllMatches()
    {
        List<GameObject> possibleMoves = new List<GameObject>();

        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                if (board.dots[i, j] != null)
                {
                    if (i < board.width - 1)
                    {
                        if (board.SwitchAndCheck(i, j, Vector2.right))
                        {
                            possibleMoves.Add(board.dots[i, j]);
                        }
                    }
                    if (j < board.height - 1)
                    {
                        if (board.SwitchAndCheck(i, j, Vector2.up))
                        {
                            possibleMoves.Add(board.dots[i, j]);
                        }
                    }
                }
            }
        }
        return possibleMoves;
    }
    
    GameObject PickOneRandomly()
    {
        List<GameObject> possibleMoves = new List<GameObject>();
        possibleMoves = FindAllMatches();

        if(possibleMoves.Count > 0)
        {
            int pieceToUse = Random.Range(0, possibleMoves.Count);
            return possibleMoves[pieceToUse];
        }
        return null;
    }
    
    private void MarkHint()
    {
        GameObject move = PickOneRandomly();
        if(move != null)
        {
            if (hintParticle != null)
            {
                currentHint = Instantiate(hintParticle, move.transform.position, Quaternion.identity);
            }
        }
    }
    
    public void DestroyHint()
    {
        if(currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
            hintDelaySecond = hintDelay;
        }
    }
}
