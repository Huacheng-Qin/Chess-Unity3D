using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : GamePiece
{
    override public PieceType GetPieceType() {
        return PieceType.rook;
    }

    override public void ShowAvailableMoves() {
        for (int x = m_coord.x + 1; x < 8; x++) {
            if (!RecursiveMove(new Vector2Int(x, m_coord.y))) {
                break;
            }
        } // right beam

        for (int x = m_coord.x - 1; x >= 0; x--) {
            if (!RecursiveMove(new Vector2Int(x, m_coord.y))) {
                break;
            }
        } // left beam

        for (int y = m_coord.y + 1; y < 8; y++) {
            if (!RecursiveMove(new Vector2Int(m_coord.x, y))) {
                break;
            }
        } // up beam

        for (int y = m_coord.y - 1; y >= 0; y--) {
            if (!RecursiveMove(new Vector2Int(m_coord.x, y))) {
                break;
            }
        } // down beam
    } // ShowAvailableMoves()
} // Rook
