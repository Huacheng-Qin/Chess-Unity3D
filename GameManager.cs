﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private class MoveHistory {
        private Vector2Int m_attacker;
        private Vector2Int m_defender;
        private GamePiece m_defendingPiece;
        private bool m_promoted = false;
        private King.CastleCode m_castle = King.CastleCode.unknown;
        private bool m_firstMove = false;
        private int m_enPassant = 0;

        public MoveHistory(Vector2Int attacker, Vector2Int defender, GamePiece piece) {
            m_attacker = attacker;
            m_defender = defender;
            m_defendingPiece = piece;
        }

        public Vector2Int Attacker() {
            return m_attacker;
        }

        public Vector2Int Defender() {
            return m_defender;
        }

        public GamePiece DefendingPiece() {
            return m_defendingPiece;
        }

        public void Promotion() {
            m_promoted = true;
        }

        public bool Promoted() {
            return m_promoted;
        }
    
        public void CastleTurn(King.CastleCode key) {
            m_castle = key;
        }
    
        public King.CastleCode IsCastleTurn() {
            return m_castle;
        }

        public void FirstMove() {
            m_firstMove = true;
        }

        public bool IsFirstMove() {
            return m_firstMove;
        }

        public void EnPassant(GamePiece piece, int dir) {
            m_enPassant = dir;
            m_defendingPiece = piece;
        }

        public int IsEnPassant() {
            return m_enPassant;
        }
    } // MoveHistory

    private UIScript m_ui = new UIScript();
    private Dictionary<Vector2Int, Tile> m_grid = new Dictionary<Vector2Int, Tile>();
    private bool m_whiteTurn = true; //black turn when false
    private bool m_showingAvailableSpaces = false;
    private Vector2Int m_attackingPiece = new Vector2Int(-1, -1);
    private Vector2Int m_whiteKing = new Vector2Int(4, 0);
    private Vector2Int m_blackKing = new Vector2Int(4, 7);
    private Vector2Int m_enPassantSpace = new Vector2Int(-1, -1);
    private bool m_enPassantCounter = false;
    private Stack<MoveHistory> m_history = new Stack<MoveHistory>();
    private bool m_pause = false;
    private int m_counter = 0;
    private Vector3 m_velocity = new Vector3(0f, 0f, 0f);

    private const int MOVEMENT_TIMER = 25;

    public GamePiece GetPieceAt(Vector2Int coord) {
        if (coord.x < 0 || coord.y < 0 || coord.x >= 8 || coord.y >= 8) {
            return null;
        }
        return m_grid[coord].GetPiece();
    }

    public void MakeAvailableAt(Vector2Int from, Vector2Int to) {
        if (to.x < 0 || to.y < 0 || to.x >= 8 || to.y >= 8) {
            return;
        }

        GamePiece piece = m_grid[from].GetPiece();

        // Check if the it's the right turn
        if (piece != null) {
            if ((piece.GetTeam() == GamePiece.Team.white && !m_whiteTurn)
                || (piece.GetTeam() == GamePiece.Team.black && m_whiteTurn)) {
                return;
            }
        }

        // Perform a virtual move to see if the move is viable
        // If said move causes the king to be in check, the move is not viable
        MovePiece(from, to, true);
        if (piece != null) {
            if (piece.GetTeam() == GamePiece.Team.white) {
                if (SpaceInCheck(m_whiteKing, GamePiece.Team.white)) {
                    UndoMove();
                    return;
                }
            } else {
                if (SpaceInCheck(m_blackKing, GamePiece.Team.black)) {
                    UndoMove();
                    return;
                }
            }
        }
        UndoMove();


        m_grid[to].MakeAvailable();
    } // MakeAvailableAt

    public void MakeAllUnavailable() {
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                m_grid[new Vector2Int(x, y)].MakeUnavailable();
            }
        }
        m_attackingPiece = new Vector2Int(-1, -1);
    }

    // Checks the castleing availability based on a key
    public void CastleAt(King.CastleCode key) {
        int y;
        int deltaX;
        GamePiece piece = null;

        // Determine the y position based on castle code
        if (key == King.CastleCode.whiteQueen || key == King.CastleCode.whiteKing) {
            y = 0;
        } else {
            y = 7;
        }

        // Fetch a rook and determine x direction based on castle code
        if (key == King.CastleCode.whiteQueen || key == King.CastleCode.blackQueen) {
            piece = m_grid[new Vector2Int(0, y)].GetPiece();
            deltaX = -1;
        } else {
            piece = m_grid[new Vector2Int(7, y)].GetPiece();
            deltaX = 1;
        }

        // If the rook is not at the initial position, or has already moved,
        // then castling is not available
        if (piece == null || piece.HasMoved()) {
            return;
        }

        // Check from the king's position to where it will end up for checks
        // if any of those spaces are in check, castling is not available
        for (int x = 4; (x > 2 && x < 6); x += deltaX)  {
            if (y < 4) {
                if (SpaceInCheck(new Vector2Int(x, y), GamePiece.Team.white)) {
                    return;
                }
            } else {
                if (SpaceInCheck(new Vector2Int(x, y), GamePiece.Team.black)) {
                    return;
                }
            }
        }

        // Check from the king's position to the rook's position,
        // if any spaces are occupied, castling is not available
        for (int x = 4 + deltaX; (x > 0 && x < 7); x += deltaX) {
            if (m_grid[new Vector2Int(x, y)].GetPiece() != null) {
                return;
            }
        }

        // Make target coordinate available based on castle code
        if (key == King.CastleCode.whiteQueen) {
            MakeAvailableAt(new Vector2Int(4, 0), new Vector2Int(2, 0));
        } else if (key == King.CastleCode.whiteKing) {
            MakeAvailableAt(new Vector2Int(4, 0), new Vector2Int(6, 0));
        } else if (key == King.CastleCode.blackQueen) {
            MakeAvailableAt(new Vector2Int(4, 7), new Vector2Int(2, 7));
        } else if (key == King.CastleCode.blackKing) {
            MakeAvailableAt(new Vector2Int(4, 7), new Vector2Int(6, 7));
        }
    }

    // Moves the piece and changes its position, but not visually
    // fakeMove skips some checks needed to modify the MoveHistory that turn
    private void MovePiece(Vector2Int from, Vector2Int to, bool fakeMove = false) {
        m_history.Push(new MoveHistory(from, to, m_grid[to].GetPiece()));

        GamePiece piece = m_grid[from].GetPiece();
        m_grid[to].SetPiece(piece);
        m_grid[from].SetPiece(null);

        if (piece == null) {
            return;
        } else {
            piece.UpdateCoordinates(to);
            if (!piece.HasMoved()) {
                piece.SetMoved();
                m_history.Peek().FirstMove();
            }
        }

        // Update the king's position in manager
        if (piece.GetPieceType() == GamePiece.PieceType.king) {
            if (piece.GetTeam() == GamePiece.Team.white) {
                m_whiteKing = to;
            } else {
                m_blackKing = to;
            }
        }

        // Perform additional checks if the move is not virtual
        // These modifications for MoveHistory are not needed for virtual moves
        if (!fakeMove) {
            if (piece.GetPieceType() == GamePiece.PieceType.pawn) {
                GamePiece.Team team = piece.GetTeam();

                // If the pawn moved 2 spaces at once, set en passant available
                if (team == GamePiece.Team.white && (to.y - from.y) == 2) {
                    SetEnPassantSpace(new Vector2Int(to.x, to.y - 1));
                    m_enPassantCounter = true;
                } else if (team == GamePiece.Team.black && (to.y - from.y) == -2) {
                    SetEnPassantSpace(new Vector2Int(to.x, to.y + 1));
                    m_enPassantCounter = true;
                }

                // If the pawn has reached the end, promote
                if ((team == GamePiece.Team.white && to.y == 7) 
                    || (team == GamePiece.Team.black && to.y == 0)) {
                    Promotion(to);
                    m_history.Peek().Promotion();
                }

                // If the pawn is performing an en passant,
                // remove the defending piece and commit to MoveHistory
                if (to.Equals(m_enPassantSpace)) {
                    Tile enPasTile = null;
                    if (team == GamePiece.Team.white) {
                        enPasTile = m_grid[new Vector2Int(to.x, to.y - 1)];
                        m_history.Peek().EnPassant(enPasTile.GetPiece(), -1);
                    } else if (team == GamePiece.Team.black) {
                        enPasTile = m_grid[new Vector2Int(to.x, to.y + 1)];
                        m_history.Peek().EnPassant(enPasTile.GetPiece(), 1);
                    }
                    if (enPasTile != null) {
                        enPasTile.SetPiece(null);
                    }
                }
            } // Pawn checks
            if (piece.GetPieceType() == GamePiece.PieceType.king) {
                // Commit castling to MoveHistory if the king moved more than 1 space
                if (Mathf.Abs((float)(to.x - from.x)) > 1.5f) {
                    King.CastleCode key;
                    GamePiece rook = null;
                    Vector2Int coord = new Vector2Int(-1, to.y);

                    // Determine the castle code and rook position based on the king position
                    if (to.x < 4) {
                        if (to.y == 0) {
                            key = King.CastleCode.whiteQueen;
                        } else {
                            key = King.CastleCode.blackQueen;
                        }
                        coord.x = 0;
                        rook = m_grid[coord].GetPiece();
                        m_grid[coord].SetPiece(null);
                        coord.x = 3;
                    } else {
                        if (to.y == 0) {
                            key = King.CastleCode.whiteKing;
                        } else {
                            key = King.CastleCode.blackKing;
                        }
                        coord.x = 7;
                        rook = m_grid[coord].GetPiece();
                        m_grid[coord].SetPiece(null);
                        coord.x = 5;
                    }

                    // Update rook position
                    m_grid[coord].SetPiece(rook);
                    rook.transform.name = m_grid[coord].transform.name;
                    rook.UpdateCoordinates(coord);
                    rook.SetMoved();
                    m_history.Peek().CastleTurn(key);
                }
            } // Castle check
        } // special checks
    } // MovePiece

    private void UndoMove(bool visual = false) {
        if (m_history.Count == 0) {
            return;
        }

        MoveHistory lastMove = m_history.Pop();

        // Perform additional undo sequences based on MoveHistory
        if (lastMove.Promoted()) {
            GamePiece newPawn = new Pawn();
            newPawn.SetMoved();
            m_grid[lastMove.Attacker()].SetPiece(newPawn);
        } else {
            m_grid[lastMove.Attacker()].SetPiece(m_grid[lastMove.Defender()].GetPiece());
        }

        if (lastMove.IsEnPassant() != 0) {
            m_grid[new Vector2Int(lastMove.Defender().x, lastMove.Defender().y + lastMove.IsEnPassant())].SetPiece(lastMove.DefendingPiece());
        } else {
            m_grid[lastMove.Defender()].SetPiece(lastMove.DefendingPiece());
        }

        GamePiece piece = m_grid[lastMove.Attacker()].GetPiece();
        if (piece != null) {
            piece.UpdateCoordinates(lastMove.Attacker());
        }
        // Update king position in manager
        if (piece != null && piece.GetPieceType() == GamePiece.PieceType.king) {
            if (piece.GetTeam() == GamePiece.Team.white) {
                m_whiteKing = lastMove.Attacker();
            } else {
                m_blackKing = lastMove.Attacker();
            }
        }

        if (lastMove.IsFirstMove()) {
            piece.UnsetMove();
        }

        // If it was a castle turn, move rook as well
        if (lastMove.IsCastleTurn() != King.CastleCode.unknown) {
            King.CastleCode key = lastMove.IsCastleTurn();
            Vector2Int coord = new Vector2Int(-1, -1);
            GamePiece rook = null;

            // Determine y coordinate based on castle code
            if (key == King.CastleCode.whiteQueen || key == King.CastleCode.whiteKing) {
                coord.y = 0;
            } else {
                coord.y = 7;
            }

            // Determine x coordinate based on castle code, fetch rook
            if (key == King.CastleCode.whiteQueen || key == King.CastleCode.blackQueen) {
                coord.x = 3;
                rook = m_grid[coord].GetPiece();
                m_grid[coord].SetPiece(null);
                coord.x = 0;
            } else {
                coord.x = 5;
                rook = m_grid[coord].GetPiece();
                m_grid[coord].SetPiece(null);
                coord.x = 7;
            }
            m_grid[coord].SetPiece(rook);
            rook.UpdateCoordinates(coord);
            rook.UnsetMove();
        }

        // Actually move the pieces if this was not part of a virtual move
        if (visual) {
            UndoVisual(lastMove);
        }
    }

    // Move the actual GameObjects tied to the pieces
    private void UndoVisual(MoveHistory history) {
        GamePiece deadPiece = history.DefendingPiece();
        Tile target = m_grid[history.Attacker()];
        GamePiece piece = target.GetPiece();

        if (deadPiece != null) {
            Vector3 pos = deadPiece.transform.position;
            pos.y = 0f;
            deadPiece.transform.position = pos;
        }
        if (piece != null) {
            Vector3 pos = target.transform.position;
            pos.y = 0f;
            piece.transform.position = pos;
            piece.name = target.name;
        }

        // Unset en passant availability
        if (m_enPassantSpace.x != -1) {
            m_enPassantSpace = new Vector2Int(-1, -1);
        }

        // Set rook position if the the move was a castle
        if (history.IsCastleTurn() != King.CastleCode.unknown) {
            if (history.IsCastleTurn() == King.CastleCode.whiteQueen) {
                target = m_grid[new Vector2Int(0, 0)];
            } else if (history.IsCastleTurn() == King.CastleCode.whiteKing) {
                target = m_grid[new Vector2Int(7, 0)];
            } else if (history.IsCastleTurn() == King.CastleCode.blackQueen) {
                target = m_grid[new Vector2Int(0, 7)];
            } else if (history.IsCastleTurn() == King.CastleCode.blackKing) {
                target = m_grid[new Vector2Int(7, 7)];
            }
            piece = target.GetPiece();
            Vector3 pos = target.transform.position;
            pos.y = 0f;
            piece.transform.position = pos;
            piece.name = target.name;
        }
        m_pause = true;
    }

    // Calculates how far a piece moves per frame
    private void CalculateVelocity() {
        if (m_history.Count == 0) {
            return;
        }

        MoveHistory lastMove = m_history.Peek();
        Tile tile = m_grid[lastMove.Defender()];
        GamePiece piece = tile.GetPiece();

        if (piece != null) {
            Vector3 pos = m_grid[lastMove.Attacker()].transform.position;
            Vector3 targetPos = tile.transform.position;
            piece.name = tile.name;
            piece.transform.SetParent(tile.transform);
            m_velocity = new Vector3((targetPos.x - pos.x)/MOVEMENT_TIMER, 0f, (targetPos.z - pos.z)/MOVEMENT_TIMER);
        }
    }

    // Moves the GameObject for the attacking piece based on the velocity
    private void UpdatePiecePosition() {
        if (m_history.Count == 0) {
            return;
        }

        GamePiece piece = m_grid[m_history.Peek().Defender()].GetPiece();

        if (piece != null) {
            Vector3 pos = piece.transform.position;
            pos.Set(pos.x + m_velocity.x, pos.y, pos.z + m_velocity.z);
            piece.transform.position = pos;
        }

        // Move the rook as well if it's a castle turn
        if (m_history.Peek().IsCastleTurn() != King.CastleCode.unknown) {
            King.CastleCode key = m_history.Peek().IsCastleTurn();
            if (key == King.CastleCode.whiteQueen) {
                piece = m_grid[new Vector2Int(3, 0)].GetPiece();
            } else if (key == King.CastleCode.whiteKing) {
                piece = m_grid[new Vector2Int(5, 0)].GetPiece();
            } else if (key == King.CastleCode.blackQueen) {
                piece = m_grid[new Vector2Int(3, 7)].GetPiece();
            } else if (key == King.CastleCode.blackKing) {
                piece = m_grid[new Vector2Int(5, 7)].GetPiece();
            }
            if (piece == null) {
                return;
            }
            Vector3 pos = piece.transform.position;
            if (key == King.CastleCode.whiteQueen || key == King.CastleCode.blackQueen) {
                pos.z = pos.z - m_velocity.z * 3f/2f;
            } else {
                pos.z = pos.z - m_velocity.z;
            }
            piece.transform.position = pos;
        }
    }

    // At the end of the moving turn, set the position of the attacking
    // piece to be exactly at the right tile, in case of floating-point errors
    // Also moves the defending piece below the board
    private void PositionAdjustment() {
        if (m_history.Count < 1) {
            return;
        }

        MoveHistory LastMove = m_history.Peek();
        GamePiece deadPiece = LastMove.DefendingPiece();
        Tile target = m_grid[LastMove.Defender()];
        GamePiece piece = target.GetPiece();

        if (deadPiece != null) {
            Vector3 pos = deadPiece.transform.position;
            pos.Set(pos.x, -5.0f, pos.z);
            deadPiece.transform.position = pos;
        }
        if (piece != null) {
            Vector3 pos = target.transform.position;
            pos.Set(pos.x, 0f, pos.z);
            piece.transform.position = pos;
        }
    }

    private void Promotion(Vector2Int place) {
        //void
    }

    private void SetEnPassantSpace(Vector2Int coord) {
        m_enPassantSpace = coord;
    }

    public bool CanEnPassantAt(Vector2Int coord) {
        if (coord.Equals(m_enPassantSpace)) {
            return true;
        }
        return false;
    }

    // Checks if the given coordinate is in check for a given team
    public bool SpaceInCheck(Vector2Int coord, GamePiece.Team team) {
        bool singleSpace = false;
        int helper2 = 1;    // code for CheckHelper2
        GamePiece.PieceType targetPieceType = 0;

        // Check for cardinal directions recursively
        // straight directions
        targetPieceType = CheckHelper(coord, 1, 0, team, ref singleSpace);
        if (CheckHelper2(targetPieceType, helper2, singleSpace)) {return true;}
        targetPieceType = CheckHelper(coord, 0, 1, team, ref singleSpace);
        if (CheckHelper2(targetPieceType, helper2, singleSpace)) {return true;}
        targetPieceType = CheckHelper(coord, -1, 0, team, ref singleSpace);
        if (CheckHelper2(targetPieceType, helper2, singleSpace)) {return true;}
        targetPieceType = CheckHelper(coord, 0, -1, team, ref singleSpace);
        if (CheckHelper2(targetPieceType, helper2, singleSpace)) {return true;}

        // diagonal directions
        targetPieceType = CheckHelper(coord, 1, 1, team, ref singleSpace);
        if (team == GamePiece.Team.black) {
            helper2 = 2;
        } else if (team == GamePiece.Team.white) {
            helper2 = 3;
        }
        if (CheckHelper2(targetPieceType, helper2, singleSpace)) {return true;}
        targetPieceType = CheckHelper(coord, -1, 1, team, ref singleSpace);
        if (CheckHelper2(targetPieceType, helper2, singleSpace)) {return true;}
        targetPieceType = CheckHelper(coord, -1, -1, team, ref singleSpace);
        if (team == GamePiece.Team.black) {
            helper2 = 2;
        } else if (team == GamePiece.Team.white) {
            helper2 = 3;
        }
        if (CheckHelper2(targetPieceType, helper2, singleSpace)) {return true;}
        targetPieceType = CheckHelper(coord, 1, -1, team, ref singleSpace);
        if (CheckHelper2(targetPieceType, helper2, singleSpace)) {return true;}

        // Check for knight positions
        GamePiece targetPiece = GetPieceAt(new Vector2Int(coord.x + 2, coord.y + 1));
        if (targetPiece != null && targetPiece.GetTeam() != team 
            && targetPiece.GetPieceType() == GamePiece.PieceType.knight) {
            return true;
        }
        targetPiece = GetPieceAt(new Vector2Int(coord.x + 2, coord.y - 1));
        if (targetPiece != null && targetPiece.GetTeam() != team 
            && targetPiece.GetPieceType() == GamePiece.PieceType.knight) {
            return true;
        }
        targetPiece = GetPieceAt(new Vector2Int(coord.x + 1, coord.y + 2));
        if (targetPiece != null && targetPiece.GetTeam() != team 
            && targetPiece.GetPieceType() == GamePiece.PieceType.knight) {
            return true;
        }
        targetPiece = GetPieceAt(new Vector2Int(coord.x + 1, coord.y - 2));
        if (targetPiece != null && targetPiece.GetTeam() != team 
            && targetPiece.GetPieceType() == GamePiece.PieceType.knight) {
            return true;
        }
        targetPiece = GetPieceAt(new Vector2Int(coord.x - 1, coord.y + 2));
        if (targetPiece != null && targetPiece.GetTeam() != team 
            && targetPiece.GetPieceType() == GamePiece.PieceType.knight) {
            return true;
        }
        targetPiece = GetPieceAt(new Vector2Int(coord.x - 1, coord.y - 2));
        if (targetPiece != null && targetPiece.GetTeam() != team 
            && targetPiece.GetPieceType() == GamePiece.PieceType.knight) {
            return true;
        }
        targetPiece = GetPieceAt(new Vector2Int(coord.x - 2, coord.y + 1));
        if (targetPiece != null && targetPiece.GetTeam() != team 
            && targetPiece.GetPieceType() == GamePiece.PieceType.knight) {
            return true;
        }
        targetPiece = GetPieceAt(new Vector2Int(coord.x - 2, coord.y - 1));
        if (targetPiece != null && targetPiece.GetTeam() != team 
            && targetPiece.GetPieceType() == GamePiece.PieceType.knight) {
            return true;
        }
        return false;
    } // SpaceInCheck

    // Loops in a given direction and returns the first GamePiece hit
    // singleSpace is used to determine whether or not CheckHelper2 needs
    // to check for king and pawn
    private GamePiece.PieceType CheckHelper(Vector2Int startingCoord, int deltaX, int deltaY, GamePiece.Team team, ref bool singleSpace) {
        int x = startingCoord.x + deltaX;
        int y = startingCoord.y + deltaY;
        for (; (x >= 0 && x < 8 && y >= 0 && y < 8); x += deltaX, y += deltaY) {
            GamePiece targetPiece = GetPieceAt(new Vector2Int(x, y));
            if (targetPiece != null) {
                if (targetPiece.GetTeam() != team) {
                    if (x == startingCoord.x + deltaX && y == startingCoord.y + deltaY) {
                        singleSpace = true;
                    } else {
                        singleSpace = false;
                    }
                    return targetPiece.GetPieceType();
                }
                break;
            }
        }
        return GamePiece.PieceType.unknown;
    }

    // The direction parameter determines what types need to be checked
    // 1 - rook check, 2 - bishop check (including pawn), 3 - bishop check (no pawn)
    private bool CheckHelper2(GamePiece.PieceType type, int direction, bool singleSpace) {
        switch(direction) {
        case 1:
            if (type == GamePiece.PieceType.queen || type == GamePiece.PieceType.rook
                || (singleSpace && type == GamePiece.PieceType.king)) {
                return true;
            } break;
        case 2: 
            if (type == GamePiece.PieceType.queen || type == GamePiece.PieceType.bishop
                || (singleSpace && (type == GamePiece.PieceType.pawn || type == GamePiece.PieceType.king))) {
                return true;
            } break;
        case 3:
            if (type == GamePiece.PieceType.queen || type == GamePiece.PieceType.bishop
                || (singleSpace && type == GamePiece.PieceType.king)) {
                return true;
            } break;
        default:
            return false;
        }
        return false;
    }

    // Based on whose turn it is, check for available moves
    private bool CheckMate() {
        GamePiece.Team team = m_whiteTurn ? GamePiece.Team.white: GamePiece.Team.black;

        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                GamePiece piece = m_grid[new Vector2Int(x, y)].GetPiece();
                if (piece != null && piece.GetTeam() == team) {
                    piece.ShowAvailableMoves();
                }
            }
        }
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                if (m_grid[new Vector2Int(x, y)].IsAvailable()) {
                    MakeAllUnavailable();
                    return false;
                }
            }
        }

        MakeAllUnavailable();
        return true;
    }

    // Determine if the result is a checkmate or stalemate,
    // and prints the result to the screen
    private void PrintGameOver() {
        Vector2Int coord = m_whiteTurn ? m_whiteKing : m_blackKing;

        if (SpaceInCheck(coord, m_whiteTurn ? GamePiece.Team.white: GamePiece.Team.black)) {
            m_ui.PrintGameOver(true, m_whiteTurn ? GamePiece.Team.black : GamePiece.Team.white);
        } else {
            m_ui.PrintGameOver(false, GamePiece.Team.unknown);
        }
    }

    // Highlight the available spaces
    private void ShowAvailableSpaces() {
        bool flag = false;
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                Tile tile = m_grid[new Vector2Int(x, y)];
                tile.ShowAvailable();
                if (tile.IsAvailable() && !flag) {
                    flag = true;
                }
            }
        }
        if (!flag) {
            m_showingAvailableSpaces = false;
        }
    }

    // Perform on click actions based on the name of the GameObject clicked
    private void OnClick(string name) {
        // perform undo
        if (name == "Board" && m_history.Count > 0) {
            m_whiteTurn = !m_whiteTurn;
            UndoMove(true);
            if(m_enPassantSpace.x != -1) {
                m_enPassantSpace = new Vector2Int(-1, -1);
            }
            return;
        }

        Vector2Int coord = ParseCoordinates(name);

        if (!m_grid.ContainsKey(coord)) {
            return;
        }

        // m_showingAvailableSpaces is true when a piece is already selected
        // and displaying viable moves
        if (m_showingAvailableSpaces) {
            // Check availability at target coordinate
            if (m_grid[coord].IsAvailable()) {
                // Move piece and perform all end of turn actions
                MovePiece(m_attackingPiece, coord);
                CalculateVelocity();
                MakeAllUnavailable();
                ShowAvailableSpaces();
                if (m_enPassantCounter) {
                    m_enPassantCounter = false;
                } else if(m_enPassantSpace.x != -1) {
                    m_enPassantSpace = new Vector2Int(-1, -1);
                }
                m_pause = true;
                m_whiteTurn = !m_whiteTurn;
                m_showingAvailableSpaces = false;
                m_attackingPiece = new Vector2Int(-1, -1);
                if (CheckMate()) {
                    PrintGameOver();
                }
            } else {
                // Since the target position is not available,
                // stop showing available moves or show available
                // moves for the target piece instead
                GamePiece piece = m_grid[coord].GetPiece();
                if (coord.Equals(m_attackingPiece)) {
                    piece = null;
                }
                MakeAllUnavailable();
                if (piece == null) {
                    m_showingAvailableSpaces = false;
                    m_attackingPiece = new Vector2Int(-1, -1);
                } else {
                    if ((piece.GetTeam() == GamePiece.Team.white && m_whiteTurn)
                        || (piece.GetTeam() == GamePiece.Team.black && !m_whiteTurn)) {
                        piece.ShowAvailableMoves();
                        m_attackingPiece = coord;
                    } else {
                        m_showingAvailableSpaces = false;
                        m_attackingPiece = new Vector2Int(-1, -1);
                    }
                }
            }
        } else {
            GamePiece piece = m_grid[coord].GetPiece();
            if (piece != null) {
                piece.ShowAvailableMoves();
                m_attackingPiece = coord;
                m_showingAvailableSpaces = true;
            }
        }
        ShowAvailableSpaces();
    }

    private Vector2Int ParseCoordinates(string key) {
        return new Vector2Int((int)(key[0] - 'a'), (int)(key[1] - '1'));
    }

    // Turns the camera by 180 degrees, in 2*MOVEMENT_TIMER frames
    private void TurnCamera() {
        Vector3 angle = transform.eulerAngles;
        
        if (!m_whiteTurn) {
            angle.Set(angle.x, 0.5f * 180.0f * (m_counter - MOVEMENT_TIMER)/(float)MOVEMENT_TIMER, angle.z);
        } else {
            angle.Set(angle.x, 180.0f + 0.5f * 180.0f * (m_counter - MOVEMENT_TIMER)/(float)MOVEMENT_TIMER, angle.z);
        }
        transform.eulerAngles = angle;
    }

    public void Exit() {
        Application.Quit();
    }

    // Start is called before the first frame update
    void Start() {
        for (char x = 'a'; x <= 'h'; x++) {
            for (char y = '1'; y <= '8'; y++) {
                string stringCoord = x.ToString() + y.ToString();
                Vector2Int coord = ParseCoordinates(stringCoord);
                m_grid.Add(coord, GameObject.Find(stringCoord).GetComponent<Tile>());
                m_grid[coord].Initialize(this, coord);
            }
        }
    }

    // If the game is not paused, check if the player has clicked on an object
    void Update() {
        if (!m_pause && Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 20.0f)) {
                if (hit.transform != null) {
                    string name = hit.transform.gameObject.name;
                    OnClick(name);
                }
            }
        }
    }

    // If the game is paused, turn the camera and
    // move the pieces for this turn
    private void FixedUpdate() {
        if (m_pause) {
            m_counter++;
            if (m_counter > MOVEMENT_TIMER) {
                TurnCamera();
                if (m_counter >= MOVEMENT_TIMER * 3) {
                    m_counter = 0;
                    m_pause = false;
                    m_velocity = new Vector3(0f, 0f, 0f);
                }
            } else {
                UpdatePiecePosition();
                if (m_counter == MOVEMENT_TIMER) {
                    PositionAdjustment();
                }
            }
        }
    }

} // GameManager
