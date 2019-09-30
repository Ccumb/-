using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;      // 열
    public int row;         // 행
    public int prevColumn;
    public int prevRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;

    private HintManager hintManager;
    private FindMatches findMatches;
    private Vector2 mFirstTouchPos;
    private Vector2 mFinalTouchPos;
    private Board mBoard;
    public GameObject mOtherDot;
    private Vector2 mTmpPos;

    [Header("Swipe Stuff")]
    public float swipAngle = 0;
    public float swipResist = 1.0f;

    [Header("Powerup Stuff")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorBomb;
    public GameObject adjacentMarker;

    // Start is called before the first frame update
    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;

        hintManager = FindObjectOfType<HintManager>();
        mBoard = GameObject.FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();

        /*targetX = (int)transform.position.x;
        targetY = (int)transform.position.y;

        row = targetY;
        column = targetX;

        prevRow = row;
        prevColumn = column;*/
    }

    
    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(1))
        {
            isAdjacentBomb = true;
            GameObject obj = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            obj.transform.parent = this.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //FindMatches();

        /*if (isMatched)
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            sprite.color = new Color(0.0f, 0.0f, 0.0f, 0.2f);
        }*/

        UpdateDotPos();
    }

    private void OnMouseDown()
    {
        if(hintManager != null)
        {
            hintManager.DestroyHint();
        }

        if(mBoard.currentState == GameState.move)
        {
            mFirstTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        //Debug.Log(mFirstTouchPos);
    }

    private void OnMouseUp()
    {
        if (mBoard.currentState == GameState.move)
        {
            mFinalTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }

        //Debug.Log(mFinalTouchPos);
    }

    public IEnumerator CheckMoveCo()
    {
        if(isColorBomb)
        {
            findMatches.MatchPiecesOfColor(mOtherDot.tag);
            isMatched = true;
        }
        else if(mOtherDot.GetComponent<Dot>().isColorBomb)
        {
            findMatches.MatchPiecesOfColor(this.gameObject.tag);
            mOtherDot.GetComponent<Dot>().isMatched = true;
        }

        yield return new WaitForSeconds(0.3f);

        if(mOtherDot != null)
        {
            if(!isMatched && !mOtherDot.GetComponent<Dot>().isMatched)
            {
                mOtherDot.GetComponent<Dot>().row = row;
                mOtherDot.GetComponent<Dot>().column = column;

                row = prevRow;
                column = prevColumn;

                yield return new WaitForSeconds(0.5f);
                mBoard.currentDot = null;
                mBoard.currentState = GameState.move;
            }
            else
            {
                mBoard.DestroyMatches();
                
            }
            //mOtherDot = null;
        }
    }

    void CalculateAngle()
    {
        if (Mathf.Abs(mFinalTouchPos.y - mFirstTouchPos.y) > swipResist
            || Mathf.Abs(mFinalTouchPos.x - mFirstTouchPos.x) > swipResist)
        {
            mBoard.currentState = GameState.wait;
            swipAngle = Mathf.Atan2(mFinalTouchPos.y - mFirstTouchPos.y, mFinalTouchPos.x - mFirstTouchPos.x) * Mathf.Rad2Deg;
            MovePieces();
            
            mBoard.currentDot = this;
        }
        else
        {
            mBoard.currentState = GameState.move;
        }
    }

    void UpdateDotPos()
    {
        targetX = column;
        targetY = row;

        if (Mathf.Abs(targetX - transform.position.x) > 0.1f) // 타겟을 향해 이동할 때
        {
            mTmpPos = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, mTmpPos, 0.5f);
            if(mBoard.dots[column, row] != this.gameObject)
            {
                mBoard.dots[column, row] = this.gameObject;
            }
            findMatches.FineAllMatches();
        }
        else
        {
            mTmpPos = new Vector2(targetX, transform.position.y);
            transform.position = mTmpPos;
        }

        if (Mathf.Abs(targetY - transform.position.y) > 0.1f) // 타겟을 향해 이동할 때
        {
            mTmpPos = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, mTmpPos, 0.5f);
            if (mBoard.dots[column, row] != this.gameObject)
            {
                mBoard.dots[column, row] = this.gameObject;
            }
            findMatches.FineAllMatches();
        }
        else
        {
            mTmpPos = new Vector2(transform.position.x, targetY);
            transform.position = mTmpPos;
        }
    }

    void MovePieceActual(Vector2 direction)
    {
        mOtherDot = mBoard.dots[column + (int)direction.x, row + (int)direction.y];
        prevRow = row;
        prevColumn = column;

        if(mOtherDot != null)
        {
            mOtherDot.GetComponent<Dot>().column += (-1) * (int)direction.x;
            mOtherDot.GetComponent<Dot>().row += (-1) * (int)direction.y;
            column += 1 * (int)direction.x;
            row += 1 * (int)direction.y;
            StartCoroutine("CheckMoveCo");
        }
        else
        {
            mBoard.currentState = GameState.move;
        }
        
    }

    void MovePieces()
    {
        if((swipAngle > -45 && swipAngle <= 45) && column < mBoard.width - 1)  // 오른쪽
        {
            /*mOtherDot = mBoard.dots[column + 1, row];
            prevRow = row;
            prevColumn = column;
            mOtherDot.GetComponent<Dot>().column -= 1;
            column += 1;
            StartCoroutine("CheckMoveCo");*/
            MovePieceActual(Vector2.right);
        }
        else if ((swipAngle > 45 && swipAngle <= 135) && row < mBoard.height - 1)  // 위쪽
        {
            /*mOtherDot = mBoard.dots[column, row + 1];
            prevRow = row;
            prevColumn = column;
            mOtherDot.GetComponent<Dot>().row -= 1;
            row += 1;
            StartCoroutine("CheckMoveCo");*/
            MovePieceActual(Vector2.up);
        }
        else if ((swipAngle > 135 || swipAngle <= -135) && column > 0)  // 왼쪽
        {
            /*mOtherDot = mBoard.dots[column - 1, row];
            prevRow = row;
            prevColumn = column;
            mOtherDot.GetComponent<Dot>().column += 1;
            column -= 1;
            StartCoroutine("CheckMoveCo");*/
            MovePieceActual(Vector2.left);
        }
        else if ((swipAngle < -45 && swipAngle >= -135) && row > 0)  // 아래쪽
        {
            /*mOtherDot = mBoard.dots[column, row - 1];
            prevRow = row;
            prevColumn = column;
            mOtherDot.GetComponent<Dot>().row += 1;
            row -= 1;
            StartCoroutine("CheckMoveCo");*/
            MovePieceActual(Vector2.down);
        }
        else
        {
            mBoard.currentState = GameState.move;
        }
        //StartCoroutine("CheckMoveCo");
    }

    void FindMatches()
    {
        if(column > 0 && column < mBoard.width - 1)
        {
            GameObject leftDot1 = mBoard.dots[column - 1, row];
            GameObject rightDot1 = mBoard.dots[column + 1, row];

            if (leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }

        if (row > 0 && row < mBoard.height - 1)
        {
            GameObject upDot1 = mBoard.dots[column, row - 1];
            GameObject downDot1 = mBoard.dots[column, row + 1];

            if (upDot1 != null && downDot1 != null)
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }

    public void MakeRowBomb()
    {
        isRowBomb = true;
        GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColumnBomb()
    {
        isColumnBomb = true;
        GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColorBomb()
    {
        isColorBomb = true;
        GameObject obj = Instantiate(colorBomb, transform.position, Quaternion.identity);
        obj.transform.parent = this.transform;
        this.gameObject.tag = "Color";
    }

    public void MakeAdjacentBomb()
    {
        isAdjacentBomb = true;
        GameObject obj = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
        obj.transform.parent = this.transform;
    }
}
