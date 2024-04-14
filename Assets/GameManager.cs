using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform gameTransform;
    [SerializeField] private Transform piecePrefab;
    public int size = 3;
    private Transform[,] piecesArray;
    public Material normalMaterial; // Material normal de las piezas
    public Material highlightMaterial; // Material para resaltar la pieza seleccionada
    private Renderer objectRenderer;
    private bool selected1 = false;
    private string selectedPosition1 = "";
    private bool selected2 = false;
    private string selectedPosition2 = "";

    private void CreateGamePieces(float gapThickness) {
        // This is the width of each tile.
        piecesArray = new Transform[size, size];
        float width = 1 / (float)size;
        for (int row = 0; row < size; row++) {
            for (int col = 0; col < size; col++) {
                Transform piece = Instantiate(piecePrefab, gameTransform);
                // pieces.Add(piece);
                // Pieces will be in a game board going from -1 to +1.
                piece.localPosition = new Vector3(-1 + (2 * width * col) + width,
                                                +1 - (2 * width * row) - width,
                                                0);
                piece.localScale = ((2 * width) - gapThickness) * Vector3.one;
                piece.name = $"{(row * size) + col}_{row}_{col}";
            
                // We want to map the UV coordinates appropriately, they are 0->1.
                float gap = gapThickness / 2;
                Mesh mesh = piece.GetComponent<MeshFilter>().mesh;
                Vector2[] uv = new Vector2[4];
                // UV coord order: (0, 1), (1, 1), (0, 0), (1, 0)
                uv[0] = new Vector2((width * col) + gap, 1 - ((width * (row + 1)) - gap));
                uv[1] = new Vector2((width * (col + 1)) - gap, 1 - ((width * (row + 1)) - gap));
                uv[2] = new Vector2((width * col) + gap, 1 - ((width * row) + gap));
                uv[3] = new Vector2((width * (col + 1)) - gap, 1 - ((width * row) + gap));
                // Assign our new UVs to the mesh.
                mesh.uv = uv;
                piecesArray[row, col] = piece;
            }
        }
    }

    private void HighlightPiece(GameObject pieceObject)
    {
        objectRenderer = pieceObject.GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            objectRenderer.material = highlightMaterial;
        }
        else {
            Debug.Log("Renderer not found");
        }
    }

    private void unHighlightPiece(GameObject pieceObject)
    {
        objectRenderer = pieceObject.GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            objectRenderer.material = normalMaterial;
        }
        else {
            Debug.Log("Renderer not found");
        }
    }

    private void SwapPieces(string piece1, string piece2) 
    {
        selected1 = false;
        selected2 = false;
        GameObject piece1Object = piecesArray[ObtenerFila(piece1), ObtenerColumna(piece1)].gameObject;
        GameObject piece2Object = piecesArray[ObtenerFila(piece2), ObtenerColumna(piece2)].gameObject;
        unHighlightPiece(piece1Object);
        unHighlightPiece(piece2Object);

        Vector3 piece1Position = piece1Object.transform.position;
        Vector3 piece2Position = piece2Object.transform.position;

        piece1Object.transform.position = piece2Position;
        piece2Object.transform.position = piece1Position;

        // Actualizar las referencias en el arreglo piecesArray si es necesario
        UpdatePieceReference(piece1Object, piece2Object);
    }

    private void UpdatePieceReference(GameObject pieza1, GameObject pieza2)
    {
        // Buscar la posición de las piezas en el arreglo piecesArray
        int fila1 = ObtenerFila(pieza2.name), columna1 = ObtenerColumna(pieza2.name), fila2 = ObtenerFila(pieza1.name), columna2 = ObtenerColumna(pieza1.name);
        for (int fila = 0; fila < size; fila++)
        {
            for (int columna = 0; columna < size; columna++)
            {
                if (piecesArray[fila, columna] == pieza1.transform) // Cambio aquí
                {
                    fila1 = fila;
                    columna1 = columna;
                }
                else if (piecesArray[fila, columna] == pieza2.transform) // Cambio aquí
                {
                    fila2 = fila;
                    columna2 = columna;
                }
            }
        }

        // Intercambiar las referencias en el arreglo
        piecesArray[fila1, columna1] = pieza2.transform; // Cambio aquí
        piecesArray[fila2, columna2] = pieza1.transform; // Cambio aquí
    }


    private void SelectPieces()
    {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            // Verificar si el rayo impacta con algún objeto en el escenario
            if (Physics.Raycast(ray, out hitInfo))
            {
                // Obtener el objeto impactado por el rayo
                GameObject hitObject = hitInfo.collider.gameObject;
                
                if (hitObject.tag == "PuzzlePiece")
                {
                    if (!selected1)
                    {
                        selected1 = true;
                        selectedPosition1 = hitObject.name;
                        HighlightPiece(hitObject);
                    }
                    if (selected1 && !selected2)
                    {
                        if (selectedPosition1 != hitObject.name)
                        {
                            selected2 = true;
                            selectedPosition2 = hitObject.name;
                            HighlightPiece(hitObject);
                            SwapPieces(selectedPosition1, selectedPosition2);
                        }
                    }
                }
            }
        }
    }

    private int ObtenerFila(string nombrePieza)
    {
        string[] partes = nombrePieza.Split('_');
        return int.Parse(partes[1]);
    }

    // Método para obtener la columna de una pieza a partir de su nombre
    private int ObtenerColumna(string nombrePieza)
    {
        string[] partes = nombrePieza.Split('_');
        return int.Parse(partes[2]);
    }

    void Start()
    {
        CreateGamePieces(0.01f);
    }

    void Update()
    {
        SelectPieces();
    }
}
