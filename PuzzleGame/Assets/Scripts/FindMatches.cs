using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{
    private Board mBorad;
    public List<GameObject> currentMatches = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        mBorad = FindObjectOfType<Board>();
    }

    public void FineAllMatches()
    {
        StartCoroutine("FindAllMatchesCo");
    }

    private List<GameObject> IsAdjacentBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();

        if (dot1.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot1.column, dot1.row));
        }

        if (dot2.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot2.column, dot2.row));
        }

        if (dot3.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot3.column, dot3.row));
        }

        return currentDots;
    }

    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();

        if (dot1.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot1.row));
        }

        if (dot2.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot2.row));
        }

        if (dot3.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot3.row));
        }

        return currentDots;
    }

    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();

        if (dot1.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot1.column));
        }

        if (dot2.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot2.column));
        }

        if (dot3.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(dot3.column));
        }

        return currentDots;
    }

    private void AddToListAndMatch(GameObject dot)
    {
        if(!currentMatches.Contains(dot))
        {
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
    {
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
    }

    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(0.2f);

        for(int i = 0; i < mBorad.width; i++)
        {
            for(int j = 0; j < mBorad.height; j++)
            {
                GameObject currentDot = mBorad.dots[i, j];
                
                if(currentDot != null)
                {
                    Dot currentDotDot = currentDot.GetComponent<Dot>();
                    if (i > 0 && i < mBorad.width - 1)
                    {
                        GameObject leftDot = mBorad.dots[i - 1, j];
                        GameObject rightDot = mBorad.dots[i + 1, j];
                        
                        if (leftDot != null && rightDot != null)
                        {
                            Dot leftDotDot = leftDot.GetComponent<Dot>();
                            Dot rightDotDot = rightDot.GetComponent<Dot>();

                            if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                            {
                                currentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot));

                                currentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rightDotDot));

                                currentMatches.Union(IsAdjacentBomb(leftDotDot, currentDotDot, rightDotDot));

                                GetNearbyPieces(leftDot, currentDot, rightDot);
                            }
                        }
                    }
                    if (j > 0 && j < mBorad.height - 1)
                    {
                        GameObject upDot = mBorad.dots[i, j + 1];
                        GameObject downDot = mBorad.dots[i, j - 1];
                        
                        if (upDot != null && downDot != null)
                        {
                            Dot upDotDot = upDot.GetComponent<Dot>();
                            Dot downDotDot = downDot.GetComponent<Dot>();

                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                            {
                                currentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot));

                                currentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot));

                                currentMatches.Union(IsAdjacentBomb(upDotDot, currentDotDot, downDotDot));

                                GetNearbyPieces(upDot, currentDot, downDot);
                            }
                        }
                    }
                }
            }
        }
    }
    
    public void MatchPiecesOfColor(string color)
    {
        for(int i = 0; i < mBorad.width; i++)
        {
            for(int j = 0; j < mBorad.height; j++)
            {
                if(mBorad.dots[i, j] != null)
                {
                    if(mBorad.dots[i, j].tag == color)
                    {
                        mBorad.dots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetAdjacentPieces(int column, int row)
    {
        List<GameObject> dots = new List<GameObject>();
        
        for(int i = column - 1; i <= column + 1; i++)
        {
            for(int j = row - 1; j <= row + 1; j++)
            {
                if(i >= 0 && i < mBorad.width && j >= 0 && j < mBorad.height)
                {
                    if (mBorad.dots[i, j] != null)
                    {
                        dots.Add(mBorad.dots[i, j]);
                        mBorad.dots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
        return dots;
    }

    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();

        for(int i = 0; i < mBorad.height; i++)
        {
            if(mBorad.dots[column, i] != null)
            {
                Dot dot = mBorad.dots[column, i].GetComponent<Dot>();

                if(dot.isRowBomb)
                {
                    dots.Union(GetRowPieces(i)).ToList();
                }

                dots.Add(mBorad.dots[column, i]);
                dot.isMatched = true;
            }
        }

        return dots;
    }

    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();

        for (int i = 0; i < mBorad.width; i++)
        {
            if (mBorad.dots[i, row] != null)
            {
                Dot dot = mBorad.dots[i, row].GetComponent<Dot>();

                if (dot.isColumnBomb)
                {
                    dots.Union(GetColumnPieces(i)).ToList();
                }

                dots.Add(mBorad.dots[i, row]);
                dot.isMatched = true;
            }
        }

        return dots;
    }

    public void CheckBombs()
    {
        if(mBorad.currentDot != null)
        {
            if(mBorad.currentDot.isMatched)
            {
                mBorad.currentDot.isMatched = false;
                /*int typeOfBomb = Random.Range(0, 100);
                if(typeOfBomb < 50)
                {
                    mBorad.currentDot.MakeRowBomb();
                }
                else if(typeOfBomb >= 50)
                {
                    mBorad.currentDot.MakeColumnBomb();
                }*/
                if(mBorad.currentDot.swipAngle > -45 && mBorad.currentDot.swipAngle <= 45
                   ||mBorad.currentDot.swipAngle < -135 || mBorad.currentDot.swipAngle >= 135)
                {
                    mBorad.currentDot.MakeRowBomb();
                }
                else
                {
                    mBorad.currentDot.MakeColumnBomb();
                }
            }
            else if(mBorad.currentDot.mOtherDot != null)
            {
                Dot otherDot = mBorad.currentDot.mOtherDot.GetComponent<Dot>();
                
                if(otherDot.isMatched)
                {
                    otherDot.isMatched = false;
                    
                    /*int typeOfBomb = Random.Range(0, 100);
                    if (typeOfBomb < 50)
                    {
                        // make a row bomb
                        otherDot.MakeRowBomb();
                    }
                    else if (typeOfBomb >= 50)
                    {
                        // make a column bomb
                        otherDot.MakeColumnBomb();
                    }*/

                    if (otherDot.swipAngle > -45 && otherDot.swipAngle <= 45
                        || otherDot.swipAngle < -135 || otherDot.swipAngle >= 135)
                    {
                        otherDot.MakeRowBomb();
                    }
                    else
                    {
                        otherDot.MakeColumnBomb();
                    }
                }
            }
        }
    }
}
