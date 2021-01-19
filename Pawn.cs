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

        // Determine the direction the pawn will move based on their team
        if (m_team == Team.white) {
            target_y = 1;
        } else {
            target_y = -1;
        }

        // Check spaces directly in front
        Vector2Int targetCoord = new Vector2Int(m_coord.x, m_coord.y + target_y);
        targetPiece = m_manager.GetPieceAt(targetCoord);
        if (targetPiece == null) {
            m_manager.MakeAvailableAt(m_coord, targetCoord);

            // Check 2 spaces if the pawn hasn't moved
            if (!m_moved) {
                targetCoord = new Vector2Int(m_coord.x, m_coord.y + 2 * target_y);
                targetPiece = m_manager.GetPieceAt(targetCoord);
                if (targetPiece == null) {
                    m_manager.MakeAvailableAt(m_coord, targetCoord);
                }
            }
        }

        // Check diagonally for enemy pieces
        targetCoord = new Vector2Int(m_coord.x - 1, m_coord.y + target_y);
        targetPiece = m_manager.GetPieceAt(targetCoord);
        if ((targetPiece != null && targetPiece.GetTeam() != m_team) 
            || m_manager.CanEnPassantAt(targetCoord)) {
            m_manager.MakeAvailableAt(m_coord, targetCoord);
        }
        targetCoord = new Vector2Int(m_coord.x + 1, m_coord.y + target_y);
        targetPiece = m_manager.GetPieceAt(targetCoord);
        if ((targetPiece != null && targetPiece.GetTeam() != m_team) 
            || m_manager.CanEnPassantAt(targetCoord)) {
            m_manager.MakeAvailableAt(m_coord, targetCoord);
        }
    } // ShowAvailableMoves()
} // Pawn
