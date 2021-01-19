using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameObject Pawn;
    public GameObject Rook;
    public GameObject Knight;
    public GameObject Bishop;
    public GameObject Queen;
    public GameObject King;
    public Material TanPiece;
    public Material BlackPiece;
    private GameManager m_manager = null;
    private Vector2Int m_coord = new Vector2Int(-1, -1);
    private GamePiece m_piece = null;
    private bool m_available = false;
    private Color m_color = new Color(0, 0, 0);

    public void Initialize(GameManager manager, Vector2Int coord) {
        m_manager = manager;
        m_coord = coord;
        m_color = this.GetComponent<Renderer>().material.color;
        GameObject newPiece = null;
        Vector3 position = transform.position;
        position.Set(position.x, 0f, position.z);
        Quaternion rotation = transform.rotation;

        if (coord.y == 1 || coord.y == 6) {
            newPiece = Instantiate(Pawn, position, rotation);
            m_piece = newPiece.GetComponent<Pawn>();
        } else if (coord.y > 1 && coord.y < 6) {
            m_piece = null;
        } else {
            if (coord.x == 0 || coord.x == 7) {
                newPiece = Instantiate(Rook, position, rotation);
                m_piece = newPiece.GetComponent<Rook>();
            } else if (coord.x == 1 || coord.x == 6) {
                newPiece = Instantiate(Knight, position, rotation);
                m_piece = newPiece.GetComponent<Knight>();
            } else if (coord.x == 2 || coord.x == 5) {
                newPiece = Instantiate(Bishop, position, rotation);
                m_piece = newPiece.GetComponent<Bishop>();
            } else if (coord.x == 3) {
                newPiece = Instantiate(Queen, position, rotation);
                m_piece = newPiece.GetComponent<Queen>();
            } else if (coord.x == 4) {
                newPiece = Instantiate(King, position, rotation);
                m_piece = newPiece.GetComponent<King>();
            } else {
                m_piece = null;
            }
        } // determine piece type

        if (newPiece != null) {
            newPiece.name = this.name;
            newPiece.transform.SetParent(this.transform);
        }

        if (m_piece != null) {
            m_piece.Initialize(m_manager, m_coord);
            if (coord.y > 4) {
                m_piece.GetComponent<Renderer>().material = BlackPiece;
            } else {
                m_piece.GetComponent<Renderer>().material = TanPiece;
            }
        }
    } // constructor

    public void ShowAvailable() {
        if(m_available) {
            if (m_piece != null) {
                this.GetComponent<Renderer>().material.color = Color.red;
            } else {
                this.GetComponent<Renderer>().material.color = Color.cyan;
            }
        } else {
            this.GetComponent<Renderer>().material.color = m_color;
        }
    }
    public GamePiece GetPiece() {
        return m_piece;
    }

    public void SetPiece(GamePiece newPiece) {
        m_piece = newPiece;
    }

    public Vector2Int GetCoord() {
        return m_coord;
    }

    public void MakeAvailable() {
        m_available = true;
    }

    public void MakeUnavailable()  {
        m_available = false;
    }

    public bool IsAvailable() {
        return m_available;
    }
} // Tile
