using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPiece : MonoBehaviour
{

    public enum Type
    {
        ATTACKER,
        DEFENDER,
        KING
    }

    public enum Player
    {
        PLAYER_1,
        PLAYER_2
    }

    public List<Cell> PassableBaseCamps;
    public Type type;
    public Player player;

    Board GameBoard;
    public Cell currentCell;
    public Cell targetCell;

    public Vector3 targetPosition;
    Vector3 velocity;
    float smoothTime = 0.25f;
    float smoothDistance = 0.01f;
    float smoothTimeVertical = 0.1f;
    float smoothHeight = -1f;

    // Use this for initialization
    void Start ()
    {
        GameBoard = GameObject.Find("Board").GetComponent<Board>();
        targetPosition = Vector3.zero;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!GameBoard.IsAnimating || targetPosition == Vector3.zero)
        {
            return;
        }

        if (Vector3.Distance(
       new Vector3(this.transform.position.x, this.transform.position.y, targetPosition.z),
       targetPosition) > smoothDistance)
        {
            // Normal movement (sideways)
            this.transform.position = Vector3.SmoothDamp(
                this.transform.position,
                new Vector3(targetPosition.x, targetPosition.y, smoothHeight),
                ref velocity,
                smoothTime);
        }
        else
        {
            targetPosition = Vector3.zero;
            GameBoard.IsAnimating = false;
            currentCell = targetCell;
            targetCell = null;
            if(this.currentCell.type == Cell.Type.CELL)
            {
                PassableBaseCamps.Clear();
            }
        }

    }
}
