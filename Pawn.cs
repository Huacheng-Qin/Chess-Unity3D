using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : GamePiece
{
    override public PieceType GetPieceType() {
        return PieceType.pawn;
    }

    override public void ShowAvailableMoves() {
        GamePiece targetPiece = null;
        int target_y = 0;

        if (m_team == Team.white) {
            target_y = 1;
        } else {
            target_y = -1;
        }

        Vector2Int targetCoord = new Vector2Int(m_coord.x, m_coord.y + target_y);
        targetPiece = m_manager.GetPieceAt(targetCoord);
        if (targetPiece == null) {
            m_manager.MakeAvailable(m_coord, targetCoord);
            if (!m_moved) {
                targetCoord = new Vector2Int(m_coord.x, m_coord.y + 2 * target_y);
                targetPiece = m_manager.GetPieceAt(targetCoord);
                if (targetPiece == null) {
                    m_manager.MakeAvailable(m_coord, targetCoord);
                }
            }
        }
        targetCoord = new Vector2Int(m_coord.x - 1, m_coord.y + target_y);
        targetPiece = m_manager.GetPieceAt(targetCoord);
        if ((targetPiece != null && targetPiece.GetTeam() != m_team) 
            || m_manager.CanEnPassantAt(targetCoord)) {
            m_manager.MakeAvailable(m_coord, targetCoord);
        }
        targetCoord = new Vector2Int(m_coord.x + 1, m_coord.y + target_y);
        targetPiece = m_manager.GetPieceAt(targetCoord);
        if ((targetPiece != null && targetPiece.GetTeam() != m_team) 
            || m_manager.CanEnPassantAt(targetCoord)) {
            m_manager.MakeAvailable(m_coord, targetCoord);
        }
    } // ShowAvailableMoves()
} // Pawn
