﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : GamePiece
{
    override public PieceType GetPieceType() {
        return PieceType.queen;
    }

    override public void ShowAvailableMoves() {
        for (int x = m_coord.x + 1; x < 8; x++) {
            if (!CheckAvailabilityAt(new Vector2Int(x, m_coord.y))) {
                break;
            }
        } // right beam

        for (int x = m_coord.x - 1; x >= 0; x--) {
            if (!CheckAvailabilityAt(new Vector2Int(x, m_coord.y))) {
                break;
            }
        } // left beam

        for (int y = m_coord.y + 1; y < 8; y++) {
            if (!CheckAvailabilityAt(new Vector2Int(m_coord.x, y))) {
                break;
            }
        } // up beam

        for (int y = m_coord.y - 1; y >= 0; y--) {
            if (!CheckAvailabilityAt(new Vector2Int(m_coord.x, y))) {
                break;
            }
        } // down beam

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
} // Queen
