using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
// 장애물, 차감 횟수를 관리, 
// Dot과 같은 오브젝트에 들어가, 왜? 장애물이라도 전반적으로 이동도 하고 사라지는 것도 같기 때문에 Dot이라는 특성도 필요하므로.
public class Obstacle : MonoBehaviour
{
    public int count = 3;   // 터지는 횟수
    private GameObject board;

    // Start is called before the first frame update
    void Start()
    {
        if(FindObjectOfType<Hexa.Board>())
        {
            board = FindObjectOfType<Hexa.Board>().gameObject;
            CheckNearNodes_Hexa();
        }
        else
        {
            board = FindObjectOfType<Normal.Board>().gameObject;
        }
        // 주변을 체크해
        
    }

    // Update is called once per frame
    void Update()
    {
        //CheckNearNodes();
    }

    public void CheckNearDots_Normal()
    {
        Normal.Dot dot = this.gameObject.GetComponent<Normal.Dot>();
        Normal.Board normalBoard = board.GetComponent<Normal.Board>();

        if(dot != null)
        {
            if (count <= 0)
            {
                // 보드에서 장애물 갯수를 하나 줄이고
                normalBoard.obstacles.Dequeue();
                // 나의 매치 상태를 true로 -> 사라질때 처리를 Dot과 한꺼번에
                dot.isMatched = true;
            }

            if (dot.column - 1 > 0)
            {
                if(normalBoard.dots[dot.column - 1, dot.row] != null && normalBoard.dots[dot.column - 1, dot.row].GetComponent<Normal.Dot>() != null)
                {
                    if(normalBoard.dots[dot.column - 1, dot.row].GetComponent<Normal.Dot>().isMatched == true)
                    {
                        this.gameObject.GetComponent<SpriteRenderer>().color *= 0.6f;
                        count--;

                        return;
                    }
                }
            }
            if(dot.column + 1 < normalBoard.width)
            {
                if (normalBoard.dots[dot.column + 1, dot.row] != null && normalBoard.dots[dot.column + 1, dot.row].GetComponent<Normal.Dot>() != null)
                {
                    if (normalBoard.dots[dot.column + 1, dot.row].GetComponent<Normal.Dot>().isMatched == true)
                    {
                        this.gameObject.GetComponent<SpriteRenderer>().color *= 0.6f;
                        count--;

                        return;
                    }
                }
            }
            if(dot.row - 1 > 0)
            {
                if (normalBoard.dots[dot.column, dot.row - 1] != null && normalBoard.dots[dot.column, dot.row - 1].GetComponent<Normal.Dot>() != null)
                {
                    if (normalBoard.dots[dot.column, dot.row - 1].GetComponent<Normal.Dot>().isMatched == true)
                    {
                        this.gameObject.GetComponent<SpriteRenderer>().color *= 0.6f;
                        count--;

                        return;
                    }
                }
            }
            if (dot.row + 1 < normalBoard.height)
            {
                if (normalBoard.dots[dot.column, dot.row + 1] != null && normalBoard.dots[dot.column, dot.row + 1].GetComponent<Normal.Dot>() != null)
                {
                    if (normalBoard.dots[dot.column, dot.row + 1].GetComponent<Normal.Dot>().isMatched == true)
                    {
                        this.gameObject.GetComponent<SpriteRenderer>().color *= 0.6f;
                        count--;

                        return;
                    }
                }
            }
        }
    }

    // 주변 노드들 탐지
    public void CheckNearNodes_Hexa()
    {
        Hexa.Dot dot = this.gameObject.GetComponent<Hexa.Dot>();
        Hexa.Board hexaBoard = board.GetComponent<Hexa.Board>();

        if(dot != null)
        {
            GameObject nodeObj = hexaBoard.nodes[dot.column][dot.row];

            // 차감 횟수가 0, 즉 다 터졌으면
            if (count <= 0)
            {
                // 보드에서 장애물 갯수를 하나 줄이고
                hexaBoard.obstacles.Dequeue();
                // 나의 매치 상태를 true로 -> 사라질때 처리를 Dot과 한꺼번에
                dot.isMatched = true;
            }

            // 나의 node정보가 있다면, 혹시모를 null 오류 방지
            if (nodeObj != null)
            {
                Hexa.Node node = nodeObj.GetComponent<Hexa.Node>();

                for (int i = 0; i < 6; i++)
                {
                    // 주변 노드가 있다면
                    if (node.nearNodes[i] != null)
                    {
                        // 주변에서 블럭이 터지면
                        if (node.nearNodes[i].GetComponent<Hexa.Node>().dot == null || node.nearNodes[i].GetComponent<Hexa.Node>().dot.GetComponent<Hexa.Dot>().isMatched == true)
                        {
                            // 나의 색깔을 0.6%정도로 변화시키고
                            this.gameObject.GetComponent<SpriteRenderer>().color *= 0.6f;
                            // 차감 횟수를 낮춘 후
                            count--;
                            // 나가, 왜? 주변에서 여러개가 한꺼번에 터져도 횟수는 한번만 차감되기 때문.
                            break;
                        }
                    }
                }
            }
        }
    }
}
