using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public enum PieceType{unknown, pawn, rook, knight, bishop, king, queen};
    public enum Team{unknown, white, black};
    protected GameManager m_manager = null;
    protected Vector2Int m_coord = new Vector2Int(-1, -1);
    protected Team m_team = Team.unknown;
    protected bool m_moved = false;

    public void Initialize(GameManager manager, Vector2Int coord, bool promotion = false) {
        m_manager = manager;
        m_coord = coord;
        if (promotion) {
            if (m_coord.y < 3) {
                m_team = Team.black;
            } else {
                m_team = Team.white;
            }
        } else {
            if (m_coord.y < 3) {
                m_team = Team.white;
            } else {
                m_team = Team.black;
            }
        }
        Vector3 rotation = this.transform.eulerAngles;
        rotation.x = -90f;
        this.transform.eulerAngles = rotation;
    }

    virtual public PieceType GetPieceType() {
        return PieceType.unknown;
    }

    virtual public void ShowAvailableMoves(){}

    public Team GetTeam() {
        return m_team;
    }

    public void SetMoved() {
        m_moved = true;
    }

    public void UnsetMove() {
        m_moved = false;
    }

    public bool HasMoved() {
        return m_moved;
    }

    public void UpdateCoordinates(Vector2Int coord) {
        m_coord = coord;
    }

    protected bool RecursiveMove(Vector2Int coord) {
        if (coord.x < 0 || coord.y < 0 || coord.x >= 8 || coord.y >= 8) {
            return false;
        }
        GamePiece targetPiece = m_manager.GetPieceAt(coord);
        if (targetPiece == null) {
            m_manager.MakeAvailable(m_coord, coord);
        } else {
            if (targetPiece.GetTeam() != m_team) {
                m_manager.MakeAvailable(m_coord, coord);
            }
            return false;
        }
        return true;
    }
} // GamePiece
