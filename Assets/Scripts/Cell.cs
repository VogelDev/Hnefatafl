using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {

    public enum Type
    {
        CELL,
        CORNER_ESCAPE,
        BASE_CAMP,
        CASTLE
    }

    public Type type;
    public Material material;
    public Material markedMaterial;
    public GameObject CellGo { set; get; }
    public PlayerPiece CurrentPiece;
    public bool IsMarked;
    public List<Cell> VerticalNeighbors;
    public List<Cell> HorizontalNeighbors;

    public int x, y;

    private Board GameBoard;
    private Renderer renderer;

    // Use this for initialization
    void Start () {
        renderer = GetComponent<Renderer>();
        renderer.material = this.material;
        GameBoard = GameObject.Find("Board").GetComponent<Board>();
    }
	
	// Update is called once per frame
	void Update ()
    {

    }

    public void UnMark()
    {
        IsMarked = false;
        renderer.material = this.material;
    }

    public void Mark()
    {
        IsMarked = true;
        renderer.material = this.markedMaterial;
    }

    bool isCellPassable(Cell cell, Cell initiator)
    {

        if(!(
            // cell has no piece
            cell.CurrentPiece == null &&
            // cell hasn't been marked already
            !cell.IsMarked &&
            // cell didn't start here
            cell != initiator))
        {
            return false;
        }

        switch (initiator.CurrentPiece.type)
        {
            case PlayerPiece.Type.ATTACKER:
                return cell.type != Type.CORNER_ESCAPE && cell.type != Type.CASTLE && (cell.type == Type.BASE_CAMP && initiator.CurrentPiece.PassableBaseCamps.Contains(cell) || cell.type != Type.BASE_CAMP);
            case PlayerPiece.Type.DEFENDER:
                return cell.type == Type.CELL;
            case PlayerPiece.Type.KING:
                return cell.type != Type.BASE_CAMP;
        }

        return (
            // cell has no piece
            cell.CurrentPiece == null &&
            // cell hasn't been marked already
            !cell.IsMarked &&
            // cell didn't start here
            cell != initiator &&
            // the initiator has no piece, or the cell is a base camp the piece is allowed to travel
            (cell.type == Type.BASE_CAMP && initiator.CurrentPiece.PassableBaseCamps.Contains(cell) || cell.type != Type.BASE_CAMP));
    }

    public void MarkVerticalNeighbors(Cell initiator)
    {
        foreach(var cell in VerticalNeighbors)
        {
            if(isCellPassable(cell, initiator))
            {
                cell.Mark();
                cell.MarkVerticalNeighbors(initiator);
            }
        }
    }

    public void MarkHorizontalNeighbors(Cell initiator)
    {
        foreach (var cell in HorizontalNeighbors)
        {
            if (isCellPassable(cell, initiator))
            {
                cell.Mark();
                cell.MarkHorizontalNeighbors(initiator);
            }
        }
    }

    void OnMouseUp()
    {
        // this object was clicked - do something

        // the board is animating, don't do anything
        if (GameBoard.IsAnimating)
        {
            return;
        }

        // player has not selected a piece
        if(CurrentPiece == null)
        {
            // piece can move here
            if (IsMarked)
            {
                GameBoard.MovePiece(this);
            }
            GameBoard.SetSelectedCell(null);
            return;
        }

        if(CurrentPiece.player == GameBoard.CurrentPlayer)
        {
            GameBoard.SetSelectedCell(this);
            MarkVerticalNeighbors(this);
            MarkHorizontalNeighbors(this);
        }
    }
}
