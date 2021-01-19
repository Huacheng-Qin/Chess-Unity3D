using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : GamePiece
{
    override public PieceType GetPieceType() {
        return PieceType.bishop;
    }

    override public void ShowAvailableMoves() {
        for (int x = m_coord.x + 1, y = m_coord.y + 1; (x < 8 && y < 8); x++, y++) {
            if (!CheckAvailabilityAt(new Vector2Int(x, y))) {
                break;
            }
        } // up-right beam

        for (int x = m_coord.x + 1, y = m_coord.y - 1; (x < 8 && y >= 0); x++, y--) {
            if (!CheckAvailabilityAt(new Vector2Int(x, y))) {
                break;
            }
        } // down-right beam

        for (int x = m_coord.x - 1, y = m_coord.y + 1; (x >= 0 && y < 8); x--, y++) {
            if (!CheckAvailabilityAt(new Vector2Int(x, y))) {
                break;
            }
        } // up-left beam

        for (int x = m_coord.x - 1, y = m_coord.y - 1; (x >= 0 && y >= 0); x--, y--) {
            if (!CheckAvailabilityAt(new Vector2Int(x, y))) {
                break;
            }
        } // down-left beam
    } // ShowAvailableMoves()
} // Bishop
