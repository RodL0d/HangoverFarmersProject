using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject[] piecePrefab;
    public Piece[,] pieces;
    private Piece selectedPiece;
    public Vector3 vector3Base = new Vector3(1, 1, 1); // Valor padr�o para a escala
    public GameObject obstaclePrefab;
    public TextMeshProUGUI JogadasText;
    public SpriteRenderer objetivo;
    public SpriteRenderer objetivoImage;
    public int maxObejetivo = 10;
    private int currentObjective;
    public TextMeshProUGUI obejectiveText;
    public int MaxJogadas = 10;
    private int currentJogadas;
    public GameManager gameManager;
    public bool cabo;
    private bool isRefilling = false; // Vari�vel para controlar o estado de refill
    public bool aumentei = false;

    void Start()
    {
        currentJogadas = MaxJogadas;
        pieces = new Piece[width, height];
        InitializeBoard();
    }

    private void Update()
    {
        UpdateJogadaText();
        UpdateObjectiveText();

        if (currentJogadas <= 0 && !cabo)
        {
            cabo = true;
            gameManager.gameOver();
            Debug.Log("Game Over");
        }
    }

    void InitializeBoard()
    {
        binaryArrayTest binaryArray = GetComponent<binaryArrayTest>(); // Obt�m o componente do BinaryArrayTest
        bool[] initialBools = binaryArray.GetInitialBools(); // Obt�m a matriz bin�ria

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x + y * width;
                if (index < initialBools.Length && initialBools[index])
                {
                    // Se a matriz bin�ria indica que h� um espa�o vazio aqui
                    pieces[x, y] = CreateEmptyPiece(x, y);
                }
                else
                {
                    // Caso contr�rio, crie uma pe�a normal
                    GameObject newPiece = Instantiate(piecePrefab[RandomFrut()], new Vector3(x, y, 0), Quaternion.identity);
                    if (newPiece != null)
                    {
                        pieces[x, y] = newPiece.GetComponent<Piece>();
                        if (pieces[x, y] != null)
                        {
                            pieces[x, y].Init(x, y, this);
                        }
                    }
                }
            }
        }

        List<Piece> piecesDestroyed = CheckForMatches(out int totalDestroyed);
        CheckObjective(piecesDestroyed);
    }

    Piece CreateEmptyPiece(int x, int y)
    {
        GameObject emptyObject = new GameObject("EmptyPiece");
        Piece emptyPiece = emptyObject.AddComponent<Piece>();
        emptyPiece.frutType = FrutType.Vazio;
        emptyPiece.Init(x, y, this);
        emptyPiece.SetVisibility(false); // Pe�a invis�vel
        return emptyPiece;
    }

    int RandomFrut()
    {
        return Random.Range(0, piecePrefab.Length);
    }

    private void UpdateJogadaText()
    {
        JogadasText.text = currentJogadas.ToString();
    }

    private void UpdateObjectiveText()
    {
        obejectiveText.text = $"{currentObjective}/{maxObejetivo}";
    }

    public void SelectPiece(Piece piece)
    {
        if (currentJogadas <= 0 || isRefilling) return; // Impede a sele��o durante o refill
        if (piece.frutType == FrutType.Obstacle) return; // Impede a sele��o de pe�as de obst�culo

        if (selectedPiece == null)
        {
            selectedPiece = piece;
            selectedPiece.AnimateScale(vector3Base * 1.2f, 0.2f);
        }
        else
        {
            if (IsAdjacent(selectedPiece, piece))
            {
                selectedPiece.AnimateScale(vector3Base, 0.2f);
                piece.AnimateScale(vector3Base, 0.2f);
                SwapPieces(selectedPiece, piece);
            }
            else
            {
                selectedPiece.AnimateScale(vector3Base, 0.2f);
                selectedPiece = piece;
                selectedPiece.AnimateScale(vector3Base * 1.2f, 0.2f);
            }
        }
    }

    bool IsAdjacent(Piece piece1, Piece piece2)
    {
        return (Mathf.Abs(piece1.x - piece2.x) == 1 && piece1.y == piece2.y) ||
               (Mathf.Abs(piece1.y - piece2.y) == 1 && piece1.x == piece2.x);
    }

    void SwapPieces(Piece piece1, Piece piece2)
    {
        if (piece1.frutType == FrutType.Obstacle || piece2.frutType == FrutType.Obstacle) return; // Impede a troca se uma das pe�as for um obst�culo

        int tempX = piece1.x;
        int tempY = piece1.y;

        pieces[piece1.x, piece1.y] = piece2;
        pieces[piece2.x, piece2.y] = piece1;

        piece1.Init(piece2.x, piece2.y, this);
        piece2.Init(tempX, tempY, this);

        Vector3 tempPosition = piece1.transform.position;
        piece1.transform.position = piece2.transform.position;
        piece2.transform.position = tempPosition;

        piece1.AnimateScale(vector3Base, 0.2f);
        piece2.AnimateScale(vector3Base, 0.2f);
        selectedPiece = null;
        currentJogadas--;
        List<Piece> piecesDestroyed = CheckForMatches(out int totalDestroyed);
        CheckObjective(piecesDestroyed);
    }

    List<Piece> CheckForMatches(out int totalDestroyed)
    {
        List<Piece> piecesToDestroy = new List<Piece>();
        totalDestroyed = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieces[x, y] == null) continue;

                // Verifica linha horizontal
                if (x < width - 2)
                {
                    int matchLength = 1;
                    FrutType currentType = pieces[x, y].frutType;
                    for (int k = 1; k < width - x; k++)
                    {
                        if (pieces[x + k, y] != null && pieces[x + k, y].frutType == currentType)
                        {
                            matchLength++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (matchLength >= 3)
                    {
                        for (int k = 0; k < matchLength; k++)
                        {
                            piecesToDestroy.Add(pieces[x + k, y]);
                            totalDestroyed++;
                        }
                    }
                }

                // Verifica coluna vertical
                if (y < height - 2)
                {
                    int matchLength = 1;
                    FrutType currentType = pieces[x, y].frutType;
                    for (int k = 1; k < height - y; k++)
                    {
                        if (pieces[x, y + k] != null && pieces[x, y + k].frutType == currentType)
                        {
                            matchLength++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (matchLength >= 3)
                    {
                        for (int k = 0; k < matchLength; k++)
                        {
                            piecesToDestroy.Add(pieces[x, y + k]);
                            totalDestroyed++;
                        }
                    }
                }
            }
        }

        foreach (Piece piece in piecesToDestroy)
        {
            if (piece != null)
            {
                pieces[piece.x, piece.y] = null;
                Destroy(piece.gameObject);
            }
        }

        DestroyAdjacentObstacles(piecesToDestroy);
        StartCoroutine(RefillBoard());

        return piecesToDestroy;
    }

    void CheckObjective(List<Piece> piecesDestroyed)
    {
        FrutType objetivoFrutType = objetivo.GetComponent<Piece>().frutType;
        int destroyedCount = 0;

        foreach (Piece piece in piecesDestroyed)
        {
            if (piece.frutType == objetivoFrutType)
            {
                destroyedCount++;
            }
        }

        if (destroyedCount > 0)
        {
            currentObjective += destroyedCount;
            // Verifica se o objetivo foi alcan�ado
            if (currentObjective >= maxObejetivo)
            {
               // gameManager.LevelComplete();
            }
        }
    }

    void DestroyAdjacentObstacles(List<Piece> piecesToDestroy)
    {
        foreach (Piece piece in piecesToDestroy)
        {
            if (piece != null && piece.frutType == FrutType.Obstacle)
            {
                for (int x = piece.x - 1; x <= piece.x + 1; x++)
                {
                    for (int y = piece.y - 1; y <= piece.y + 1; y++)
                    {
                        if (x >= 0 && x < width && y >= 0 && y < height && pieces[x, y] != null)
                        {
                            if (pieces[x, y].frutType != FrutType.Obstacle)
                            {
                                piecesToDestroy.Add(pieces[x, y]);
                            }
                        }
                    }
                }
            }
        }
    }

    IEnumerator AnimatePieceMovement(Piece piece, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = piece.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            piece.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        piece.transform.position = targetPosition;
    }

    IEnumerator RefillBoard()
    {
        yield return new WaitForSeconds(0.5f);

        binaryArrayTest binaryArray = GetComponent<binaryArrayTest>();
        bool[] initialBools = binaryArray.GetInitialBools();
        List<IEnumerator> animations = new List<IEnumerator>();

        // Primeiro movimento das pe�as existentes para os espa�os vazios
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x + y * width;
                if (pieces[x, y] == null && (index >= initialBools.Length || !initialBools[index])) // Evitar mover para espa�os "vazios"
                {
                    for (int k = y + 1; k < height; k++)
                    {
                        if (pieces[x, k] != null)
                        {
                            animations.Add(AnimatePieceMovement(pieces[x, k], new Vector3(x, y, 0), 0.3f));
                            pieces[x, k].Init(x, y, this);
                            pieces[x, y] = pieces[x, k];
                            pieces[x, k] = null;
                            break;
                        }
                    }
                }
            }
        }

        // Executa todas as anima��es de queda em paralelo
        foreach (IEnumerator animation in animations)
        {
            StartCoroutine(animation);
        }
        yield return new WaitForSeconds(0.3f);

        animations.Clear();

        // Agora faz o refill para os espa�os vazios que n�o s�o blocos vazios
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x + y * width;

                // Verifica se o espa�o pode ser preenchido e n�o � um espa�o que deve ficar vazio
                if (pieces[x, y] == null && (index >= initialBools.Length || !initialBools[index]))
                {
                    GameObject newPiece = Instantiate(piecePrefab[RandomFrut()], new Vector3(x, height, 0), Quaternion.identity); // Cria a pe�a fora da tela
                    pieces[x, y] = newPiece.GetComponent<Piece>();
                    if (pieces[x, y] != null)
                    {
                        pieces[x, y].Init(x, y, this);
                        animations.Add(AnimatePieceMovement(pieces[x, y], new Vector3(x, y, 0), 0.3f)); // Anima o movimento da pe�a at� o destino
                    }
                }
            }
        }

        // Executa todas as anima��es de refill em paralelo
        foreach (IEnumerator animation in animations)
        {
            StartCoroutine(animation);
        }
        yield return new WaitForSeconds(0.3f);

        // Ap�s o refill, verifica por novos matches
        List<Piece> piecesDestroyed = CheckForMatches(out int totalDestroyed);
        if (piecesDestroyed.Count > 0)
        {
            CheckObjective(piecesDestroyed);
            yield return StartCoroutine(RefillBoard()); // Certifica que checa por matches repetidamente at� n�o restar mais matches
        }
    }


}


