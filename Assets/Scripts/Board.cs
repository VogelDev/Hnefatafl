using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{

    public GameObject CellPrefab;
    public Material CellMaterial;
    public Material BaseCampMaterial;
    public Material CornerEscapeMaterial;
    public Material CastleMaterial;

    public GameObject Attacker;
    public GameObject Defender;
    public GameObject King;

    public Cell SelectedCell;

    public List<List<Cell>> Cells;
    public List<PlayerPiece> LivePieces;
    public List<PlayerPiece> DeadPieces;

    public MenuManager Menu;


    public bool IsAnimating = false;

    bool checkPieces = false;
    public PlayerPiece.Player CurrentPlayer;

    // Use this for initialization
    void Start()
    {

        Cells = new List<List<Cell>>();
        CurrentPlayer = PlayerPiece.Player.PLAYER_1;

        CreateBoardCells();

        SpawnPlayerPieces();

        SetBoardNeighbors();
    }

    public void NewGame()
    {
        ClearPlayerPiecesFromBoard();
        SpawnPlayerPieces();
        SetBoardNeighbors();
        Menu.gameObject.SetActive(false);
    }

    private void CreateBoardCells()
    {
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float offSetX = 0;
        float offSetY = 0;

        if (Math.Round(aspectRatio, 2) == Math.Round(16.0f / 9.0f, 2))
        {
            offSetX = 6;
            offSetY = 4;
        }

        for (var i = 0; i < 13; i++)
        {
            Cells.Add(new List<Cell>());
            for (var j = 0; j < 13; j++)
            {
                GameObject cellGo = Instantiate(CellPrefab);
                cellGo.transform.parent = gameObject.transform;
                Cell cell = cellGo.GetComponent<Cell>();
                cell.x = i;
                cell.y = j;

                // corners
                if (i == 0 && j == 0 || i == 0 && j == 12 || i == 12 && j == 0 || i == 12 && j == 12)
                {
                    cell.type = Cell.Type.CORNER_ESCAPE;
                    cell.material = CornerEscapeMaterial;
                }
                // center
                else if (i == 6 && j == 6)
                {
                    cell.type = Cell.Type.CASTLE;
                    cell.material = CastleMaterial;
                }
                else if (
                        (i == 4 && j == 6) ||
                        (i == 5 && (j == 5 || j == 6 || j == 7)) ||
                        (i == 6 && (j == 4 || j == 5 || j == 7 || j == 8)) ||
                        (i == 7 && (j == 5 || j == 6 || j == 7)) ||
                        (i == 8 && j == 6)
                        )
                {
                    cell.type = Cell.Type.CELL;
                    cell.material = CellMaterial;
                }
                else if (
                    // left and right base camp cells
                    (i == 0 || i == 12) && (j >= 4 && j <= 8) ||
                    // top and bottom base camp cells
                    (j == 0 || j == 12) && (i >= 4 && i <= 8) ||
                    // interior base camp cells
                    (i == 1 && j == 6 || i == 6 && j == 1 || i == 6 && j == 11 || i == 11 && j == 6)
                    )
                {
                    cell.type = Cell.Type.BASE_CAMP;
                    cell.material = BaseCampMaterial;
                }
                // everything else
                else
                {
                    cell.type = Cell.Type.CELL;
                    cell.material = CellMaterial;
                }

                cellGo.transform.position = new Vector3(i - offSetX, j + offSetY, 0);

                Cells[i].Add(cell);
            }
        }
    }

    private void SetBoardNeighbors()
    {
        for (var i = 0; i < Cells.Count; i++)
        {
            for (var j = 0; j < Cells[i].Count; j++)
            {
                var cell = Cells[i][j];

                if (cell.type == Cell.Type.BASE_CAMP)
                {
                    //Debug.Log("found base camp: " + i + ", " + j);
                    CheckBaseNeighbors(i, j, cell);
                }

                SetNeighbors(i, j, cell);
            }
        }
    }

    private void ClearPlayerPiecesFromBoard()
    {

        for (var i = 0; i < Cells.Count; i++)
        {
            for (var j = 0; j < Cells[i].Count; j++)
            {
                var cell = Cells[i][j];
                if(cell.CurrentPiece != null)
                {
                    Destroy(cell.CurrentPiece.gameObject);
                    cell.CurrentPiece = null;
                }
            }
        }

    }

    // TODO: Refactor this so the board size can be variable
    private void SpawnPlayerPieces()
    {
        LivePieces = new List<PlayerPiece>();
        DeadPieces = new List<PlayerPiece>();

        for (var i = 0; i < Cells.Count; i++)
        {
            for (var j = 0; j < Cells[i].Count; j++)
            {
                Cell cell = Cells[i][j];
                if (i == 6 && j == 6)
                {
                    var kingGo = Instantiate(King);
                    kingGo.transform.position = cell.transform.position + Vector3.back;
                    cell.CurrentPiece = kingGo.GetComponent<PlayerPiece>();
                    var piece = kingGo.GetComponent<PlayerPiece>();
                    piece.currentCell = cell;
                    piece.type = PlayerPiece.Type.KING;
                    piece.player = PlayerPiece.Player.PLAYER_1;
                    LivePieces.Add(piece);
                }
                else if (
                        (i == 4 && j == 6) ||
                        (i == 5 && (j == 5 || j == 6 || j == 7)) ||
                        (i == 6 && (j == 4 || j == 5 || j == 7 || j == 8)) ||
                        (i == 7 && (j == 5 || j == 6 || j == 7)) ||
                        (i == 8 && j == 6)
                        )
                {
                    var defenderGo = Instantiate(Defender);
                    defenderGo.transform.position = cell.transform.position + Vector3.back;
                    cell.CurrentPiece = defenderGo.GetComponent<PlayerPiece>();
                    var piece = defenderGo.GetComponent<PlayerPiece>();
                    piece.currentCell = cell;
                    piece.type = PlayerPiece.Type.DEFENDER;
                    piece.player = PlayerPiece.Player.PLAYER_1;
                    LivePieces.Add(piece);

                }
                else if (
                    // left and right base camp cells
                    (i == 0 || i == 12) && (j >= 4 && j <= 8) ||
                    // top and bottom base camp cells
                    (j == 0 || j == 12) && (i >= 4 && i <= 8) ||
                    // interior base camp cells
                    (i == 1 && j == 6 || i == 6 && j == 1 || i == 6 && j == 11 || i == 11 && j == 6)
                    )
                {
                    var attackerGO = Instantiate(Attacker);
                    attackerGO.transform.position = cell.transform.position + Vector3.back;
                    cell.CurrentPiece = attackerGO.GetComponent<PlayerPiece>();
                    var piece = attackerGO.GetComponent<PlayerPiece>();
                    piece.currentCell = cell;
                    piece.type = PlayerPiece.Type.ATTACKER;
                    piece.player = PlayerPiece.Player.PLAYER_2;
                    LivePieces.Add(piece);
                }
            }
        }

    }

    public void MovePiece(Cell cell)
    {
        SelectedCell.CurrentPiece.targetCell = cell;
        SelectedCell.CurrentPiece.targetPosition = cell.transform.position;
        var piece = SelectedCell.CurrentPiece;
        SelectedCell.CurrentPiece = null;
        cell.CurrentPiece = piece;
        piece.targetCell = cell;
        IsAnimating = true;
        UnMarkAll();
    }

    // this game only supports vertical and horizontal movement, so diagonal checks are not needed
    void SetNeighbors(int i, int j, Cell cell)
    {
        var horCells = new List<Cell>();
        var vertCells = new List<Cell>();

        // left
        if (i > 0)
        {
            horCells.Add(this.Cells[i - 1][j]);
        }
        // right
        if (i < this.Cells.Count - 1)
        {
            horCells.Add(this.Cells[i + 1][j]);
        }
        // up
        if (j > 0)
        {
            vertCells.Add(this.Cells[i][j - 1]);
        }
        // down
        if (j < this.Cells[i].Count - 1)
        {
            vertCells.Add(this.Cells[i][j + 1]);
        }

        cell.VerticalNeighbors = vertCells;
        cell.HorizontalNeighbors = horCells;
    }

    void CheckBaseNeighbors(int i, int j, Cell cell)
    {
        if (i < 0 || j < 0 || i >= Cells.Count || j >= Cells[i].Count)
        {
            return;
        }

        if (Cells[i][j].type != Cell.Type.BASE_CAMP)
        {
            return;
        }

        var piece = cell.CurrentPiece;
        if (piece.PassableBaseCamps == null)
        {
            piece.PassableBaseCamps = new List<Cell>();
        }

        if (piece.PassableBaseCamps.Contains(Cells[i][j]))
        {
            return;
        }

        //Debug.Log("added: " + i + ", " + j);
        piece.PassableBaseCamps.Add(Cells[i][j]);

        CheckBaseNeighbors(i - 1, j, cell);
        CheckBaseNeighbors(i + 1, j, cell);
        CheckBaseNeighbors(i, j - 1, cell);
        CheckBaseNeighbors(i, j + 1, cell);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsAnimating)
        {
            checkPieces = true;
        }

        if (checkPieces && !IsAnimating)
        {
            Debug.Log("pieces have settled");
            CheckPieces();
            SwitchPlayers();
        }
    }

    void CheckPieces()
    {
        checkPieces = false;
        Cells.ForEach(cellArray =>
        {
            cellArray.ForEach(cell =>
            {
                if (cell.CurrentPiece != null && cell.CurrentPiece.type != PlayerPiece.Type.KING)
                {
                    // checking neighbor lists separately for defenders and attackers because the orientation of the oppenonents matter,
                    // the two capturing pieces need to be in a straight line surrounding the captured piece
                    if (cell.HorizontalNeighbors.Count == 2)
                    {
                        var neighborOne = cell.HorizontalNeighbors[0];
                        var neighborTwo = cell.HorizontalNeighbors[1];
                        CheckNeighborsForCapture(cell, neighborOne, neighborTwo);
                    }
                    if (cell.VerticalNeighbors.Count == 2)
                    {
                        var neighborOne = cell.VerticalNeighbors[0];
                        var neighborTwo = cell.VerticalNeighbors[1];
                        CheckNeighborsForCapture(cell, neighborOne, neighborTwo);
                    }
                }
                else if (cell.CurrentPiece != null && cell.CurrentPiece.type == PlayerPiece.Type.KING)
                {
                    // get all of the neighbors, I can get all of the neighbors in a single list because the orientation doesn't matter
                    // because we need to check all of the non-diagonal cells surrounding the king
                    var neighbors = new List<Cell>();
                    neighbors.AddRange(cell.HorizontalNeighbors);
                    neighbors.AddRange(cell.VerticalNeighbors);

                    // are there more than two cells?
                    if (neighbors.Count > 2)
                    {
                        // more than two neighbors, how many have attackers?
                        var neighboringAttackerCount = neighbors.Where(c => c.CurrentPiece != null && c.CurrentPiece.type == PlayerPiece.Type.ATTACKER).Count();

                        // if all the neighboring cells have attackers, the king loses
                        if (neighboringAttackerCount == neighbors.Count())
                        {
                            // TODO: Game Over, Attackers win
                            Debug.Log("Attackers Win");
                            CapturePiece(cell);
                            GameOver(PlayerPiece.Player.PLAYER_2);
                        }
                        // not all neighbors have attackers 
                        else
                        {
                            // are any of the neighboring cells special cells?
                            var specialCellCount = neighbors.Where(c => c.type != Cell.Type.CELL && c.type != Cell.Type.CORNER_ESCAPE).Count();
                            // if you add the special cells with the attacker cells and it is the same number as neigboring cells, the king is surrounded
                            if (specialCellCount + neighboringAttackerCount == neighbors.Count())
                            {
                                // TODO: Game Over, Attackers win
                                Debug.Log("Attackers Win");
                                CapturePiece(cell);
                                GameOver(PlayerPiece.Player.PLAYER_2);
                            }
                        }
                    }
                    // if not, the cell is a corner, and the king escapes
                    else
                    {
                        // TODO: Game Over, Defenders win
                        Debug.Log("Defenders Win");
                        CapturePiece(cell);
                        GameOver(PlayerPiece.Player.PLAYER_1);
                    }
                }
            });
        });
    }

    private void GameOver(PlayerPiece.Player player)
    {
        Menu.gameObject.SetActive(true);
        Text text = Menu.WinnerText.GetComponent<Text>();
        if (player == PlayerPiece.Player.PLAYER_1)
        {
            text.text = "The king has escaped!";
        }
        else
        {
            text.text = "The king has been captured!";
        }
    }

    private void CheckNeighborsForCapture(Cell cell, Cell neighborOne, Cell neighborTwo)
    {
        // do both of the neighbors have cells?
        if (neighborOne.CurrentPiece != null && neighborTwo.CurrentPiece != null)
        {
            // are the neighbors the same team, while the center is different?
            if (neighborOne.CurrentPiece.player == neighborTwo.CurrentPiece.player && neighborTwo.CurrentPiece.player != cell.CurrentPiece.player)
            {
                CapturePiece(cell);
            }

        }
        // does one of the neighbors have a piece from the opposite team while the other neighbor is an exit cell?
        else if ((neighborOne.type == Cell.Type.CORNER_ESCAPE && neighborTwo.CurrentPiece != null && neighborTwo.CurrentPiece.player != cell.CurrentPiece.player) ||
                (neighborTwo.type == Cell.Type.CORNER_ESCAPE && neighborOne.CurrentPiece != null && neighborOne.CurrentPiece.player != cell.CurrentPiece.player))
        {
            CapturePiece(cell);
        }
    }

    private void CapturePiece(Cell cell)
    {
        Debug.Log("Capturing piece");
        LivePieces.Remove(cell.CurrentPiece);
        DeadPieces.Add(cell.CurrentPiece);
        cell.CurrentPiece.transform.position = Vector3.zero;
        cell.CurrentPiece = null;
    }

    public Cell GetSelectedCell()
    {
        return SelectedCell;
    }

    public void SetSelectedCell(Cell cellSelected)
    {

        if (SelectedCell != null || cellSelected == null)
        {
            UnMarkAll();
        }

        SelectedCell = cellSelected;

    }

    void UnMarkAll()
    {
        var cells = GetComponentsInChildren<Cell>();
        foreach (var cell in cells)
        {
            cell.UnMark();
        }
    }

    void SwitchPlayers()
    {
        CurrentPlayer = CurrentPlayer == PlayerPiece.Player.PLAYER_1 ? PlayerPiece.Player.PLAYER_2 : PlayerPiece.Player.PLAYER_1;
    }
}
