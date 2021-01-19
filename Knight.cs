using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : GamePiece
{
    override public PieceType GetPieceType() {
        return PieceType.knight;
    }

    override public void ShowAvailableMoves() {
        CheckAvailabilityAt(new Vector2Int(m_coord.x + 2, m_coord.y + 1));
        CheckAvailabilityAt(new Vector2Int(m_coord.x + 2, m_coord.y - 1));
        CheckAvailabilityAt(new Vector2Int(m_coord.x + 1, m_coord.y + 2));
        CheckAvailabilityAt(new Vector2Int(m_coord.x + 1, m_coord.y - 2));
        CheckAvailabilityAt(new Vector2Int(m_coord.x - 1, m_coord.y + 2));
        CheckAvailabilityAt(new Vector2Int(m_coord.x - 1, m_coord.y - 2));
        CheckAvailabilityAt(new Vector2Int(m_coord.x - 2, m_coord.y + 1));
        CheckAvailabilityAt(new Vector2Int(m_coord.x - 2, m_coord.y - 1));
    }
} // Knight
