using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;

public class Board : MonoBehaviour
{
    #region Key Support
    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[]
    {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G,
        KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N,
        KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U,
        KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z
    };
    #endregion

    #region Variables
    private Row[] rows;
    private string[] solutions;
    private string[] validWords;
    private string word;
    private int rowIndex;
    private int columnIndex;
    #endregion

    #region States
    [Header("State")]
    public Tile.State emptyState;
    public Tile.State occupiedState;
    public Tile.State correctState;
    public Tile.State wrongSpotState;
    public Tile.State incorrectState;
    #endregion

    #region UI Elements
    [Header("UI")]
    public Button newWordButton;
    public Button tryAgainButton;
    public Button exitButton;
    public Button giveUpButton;
    public Button instructionsButton;
    public TextMeshProUGUI correctWordText;
    public TextMeshProUGUI invalidWordText;
    public GameObject instructionsPanel;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();
    }

    private void Start()
    {
        LoadData();
        NewGame();

        exitButton.onClick.AddListener(ExitApplication);
        giveUpButton.onClick.AddListener(GiveUp);
        instructionsButton.onClick.AddListener(ToggleInstructions);

        instructionsPanel.SetActive(false);
    }
    #endregion

    #region Game Setup
    private void LoadData()
    {
        UnityEngine.TextAsset textFile = Resources.Load<UnityEngine.TextAsset>("word_list_10000");
        validWords = textFile.text.Split('\n');

        textFile = Resources.Load<UnityEngine.TextAsset>("word_list_2000");
        solutions = textFile.text.Split('\n');
    }

    private void SetRandomWord()
    {
        word = solutions[Random.Range(0, solutions.Length)].ToLower().Trim();
    }
    #endregion

    #region Game Logic
    private void Update()
    {
        if (rowIndex >= rows.Length) return;

        Row currentRow = rows[rowIndex];

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            HandleBackspace(currentRow);
        }
        else if (columnIndex >= currentRow.tiles.Length && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            SubmitRow(currentRow);
        }
        else
        {
            HandleLetterInput(currentRow);
        }
    }


    private void HandleBackspace(Row currentRow)
    {
        columnIndex = Mathf.Max(columnIndex - 1, 0);
        currentRow.tiles[columnIndex].SetLetter('\0');
        currentRow.tiles[columnIndex].SetState(emptyState);

        invalidWordText.gameObject.SetActive(false);
    }

    private void HandleLetterInput(Row currentRow)
    {
        for (int i = 0; i < SUPPORTED_KEYS.Length; i++)
        {
            if (Input.GetKeyDown(SUPPORTED_KEYS[i]))
            {
                if (columnIndex < currentRow.tiles.Length)
                {
                    currentRow.tiles[columnIndex].SetLetter((char)SUPPORTED_KEYS[i]);
                    currentRow.tiles[columnIndex].SetState(occupiedState);
                    columnIndex++;
                }
                break;
            }
        }
    }

    private void SubmitRow(Row row)
    {
        if (!IsValidWord(row.word))
        {
            invalidWordText.gameObject.SetActive(true);
            return;
        }

        string remaining = word;

        for (int i = 0; i < row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];

            if (tile.letter == word[i])
            {
                tile.SetState(correctState);
                remaining = remaining.Remove(i, 1);
                remaining = remaining.Insert(i, " ");
            }
            else if (!word.Contains(tile.letter))
            {
                tile.SetState(incorrectState);
            }
        }

        for (int i = 0; i < row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];

            if (tile.state != correctState && tile.state != incorrectState)
            {
                if (remaining.Contains(tile.letter))
                {
                    tile.SetState(wrongSpotState);

                    int index = remaining.IndexOf(tile.letter);
                    remaining = remaining.Remove(index, 1);
                    remaining = remaining.Insert(index, " ");
                }
                else
                {
                    tile.SetState(incorrectState);
                }
            }
        }

        if (HasWon(row))
        {
            GameManager.Instance.StopTimer();
            GameManager.Instance.RegisterBestTime();

            tryAgainButton.gameObject.SetActive(false);
            giveUpButton.gameObject.SetActive(false);

            enabled = false;
        }

        rowIndex++;
        columnIndex = 0;

        if (rowIndex >= rows.Length)
        {
            enabled = false;
        }
    }
    #endregion

    #region Helpers
    private void ClearBoard()
    {
        foreach (var row in rows)
        {
            foreach (var tile in row.tiles)
            {
                tile.SetLetter('\0');
                tile.SetState(emptyState);
            }
        }

        rowIndex = 0;
        columnIndex = 0;
    }

    private bool IsValidWord(string word)
    {
        foreach (string validWord in validWords)
        {
            if (validWord == word) return true;
        }
        return false;
    }

    private bool HasWon(Row row)
    {
        foreach (var tile in row.tiles)
        {
            if (tile.state != correctState) return false;
        }
        return true;
    }
    #endregion

    #region Button Functions
    public void NewGame()
    {
        EventSystem.current.SetSelectedGameObject(null);
        ClearBoard();
        SetRandomWord();
        enabled = true;

        GameManager.Instance.StartTimer();

        correctWordText.gameObject.SetActive(false);
        tryAgainButton.gameObject.SetActive(true);
        giveUpButton.gameObject.SetActive(true);
    }

    public void TryAgain()
    {
        EventSystem.current.SetSelectedGameObject(null);
        ClearBoard();
        enabled = true;
        correctWordText.gameObject.SetActive(false);
    }

    public void GiveUp()
    {
        correctWordText.text = $"The correct word was: {word.ToUpper()}";
        correctWordText.gameObject.SetActive(true);

        GameManager.Instance.StopTimer();

        tryAgainButton.gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ExitApplication()
    {
        Application.Quit();
        EventSystem.current.SetSelectedGameObject(null);
    }

    private bool isToggling = false;

    public void ToggleInstructions()
    {
        if (isToggling) return;

        isToggling = true;
        instructionsPanel.SetActive(!instructionsPanel.activeSelf);

        EventSystem.current.SetSelectedGameObject(null);

        Invoke(nameof(ResetToggle), 0.2f);
    }

    private void ResetToggle()
    {
        isToggling = false;
    }
    #endregion
}
