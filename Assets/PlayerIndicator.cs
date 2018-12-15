using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIndicator : MonoBehaviour {

    public Quaternion Player_1;
    public Quaternion Player_2;
    public Quaternion CurrentRotation;

    public GameObject Player_1Go;
    public GameObject Player_2Go;

    // Use this for initialization
    void Start () {
        Player_1 = new Quaternion(0, 0, 0, 0);
        Player_2 = new Quaternion(2 * Mathf.PI, 0, 0, 0);

    }
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.right);
	}

    public void SetPlayerIndicator(PlayerPiece.Player Player)
    {
        CurrentRotation = Player == PlayerPiece.Player.PLAYER_1 ? Player_1 : Player_2;

        Player_1Go.SetActive(Player == PlayerPiece.Player.PLAYER_1);
        Player_2Go.SetActive(Player == PlayerPiece.Player.PLAYER_2);
    }
}

