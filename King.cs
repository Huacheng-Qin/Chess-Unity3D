using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : GamePiece
{
    public enum CastleCode{unknown, whiteQueen, whiteKing, blackQueen, blackKing}

    override public PieceType GetPieceType() {
        return PieceType.king;
    }

    override public void ShowAvailableMoves() {
        CheckAvailabilityAt(new Vector2Int(m_coord.x + 1, m_coord.y));
        CheckAvailabilityAt(new Vector2Int(m_coord.x + 1, m_coord.y + 1));
        CheckAvailabilityAt(new Vector2Int(m_coord.x, m_coord.y + 1));
        CheckAvailabilityAt(new Vector2Int(m_coord.x - 1, m_coord.y + 1));
        CheckAvailabilityAt(new Vector2Int(m_coord.x - 1, m_coord.y));
        CheckAvailabilityAt(new Vector2Int(m_coord.x - 1, m_coord.y - 1));
        CheckAvailabilityAt(new Vector2Int(m_coord.x, m_coord.y - 1));
        CheckAvailabilityAt(new Vector2Int(m_coord.x + 1, m_coord.y - 1));

        // If the king hasn't moved, check castling availability
        if (!m_moved) {
            if (m_team == Team.white) {
                m_manager.CastleAt(CastleCode.whiteQueen);
                m_manager.CastleAt(CastleCode.whiteKing);
            } else if (m_team == Team.black) {
                m_manager.CastleAt(CastleCode.blackQueen);
                m_manager.CastleAt(CastleCode.blackKing);
            }
        }
    }
} // King
