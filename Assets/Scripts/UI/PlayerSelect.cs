using Rewired;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelect : MonoBehaviour
{
    private Texture textureImage;

    private Rewired.Player rwPlayer;

    [SerializeField] public Button teamButton;
    [SerializeField] public Button characterSelectButton;
    [SerializeField] public GameObject backgroundText;
    [SerializeField] public int playerNumber = -1;
    [SerializeField] public GameObject playerName;
    [SerializeField] public TeamEnum team = TeamEnum.Team1;
    [SerializeField] private Image clothes;
    [SerializeField] public Color[] team1Palette;
    [SerializeField] public Color[] team2Palette;
    [SerializeField] public Image imageCharacter;
    [SerializeField] public GameObject playerBoard;

    [SerializeField] PlayerAttributes[] players;
    [SerializeField] Material plarerImageMaterial;
    
    private GameObject player;
    private bool isReady = false;    
    private TextMeshProUGUI teamText;
    private TextMeshProUGUI characterSelectText;
    private TextMeshProUGUI playerNameText;
    private Image teamImageButton;
    private int colorIndex;
    private int characterIndex;

    // Start is called before the first frame update
    void Start()
    {
        teamText = teamButton.GetComponentInChildren<TextMeshProUGUI>();
        characterSelectText = characterSelectButton.GetComponentInChildren<TextMeshProUGUI>();
        teamImageButton = teamButton.GetComponentInChildren<Image>();
        playerNameText = playerName.GetComponentInChildren<TextMeshProUGUI>();
        colorIndex = 0;
        characterIndex = 0;
        player = players[0].playerPrefab;
        imageCharacter.sprite = players[0].selectSprite;
        characterSelectText.text = players[0].name;
        imageCharacter.material = Instantiate(plarerImageMaterial);
    }

    // Update is called once per frame
    void Update()
    {
        teamButton.gameObject.SetActive(playerNumber != -1);

        if (teamButton.gameObject.activeSelf)
        {
            teamText.text = team == TeamEnum.Team1 ? "Red Team" : "Blue Team";
            teamImageButton.color = team == TeamEnum.Team1 ? new Color(1, 0, 0, 1) : new Color(0, 0, 1, 1);
        }

        if (playerName != null && playerNameText != null && playerNumber != -1)
        {
            playerNameText.text = "Player " + (playerNumber + 1);
            playerName.SetActive(true);
        } else if (playerName != null)
        {
            playerName.SetActive(false);
        }

        if (rwPlayer == null)
        {
            return;
        }

        if (rwPlayer.GetButtonDown("Start"))
        {
            isReady = true;
            //image.color = new Color(1, 1, 1, 0.5f);
        }

        if (rwPlayer.GetButtonDown("UICancel"))
        {
            if (isReady)
            {
                //image.color = new Color(1, 1, 1, 1);
                isReady = false;
            }
            else
            {
                playerNumber = -1;
                backgroundText.SetActive(true);
                playerBoard.SetActive(false);
                rwPlayer = null;
            }
        }

        var canvasGroup = GetComponent<CanvasGroup>();
        if (isReady)
        {
            canvasGroup.alpha = .7f;
        } else
        {
            canvasGroup.alpha = 1f;
        }
    }

    public void SwitchTeam()
    {
        if (team == TeamEnum.Team1)
        {
            team = TeamEnum.Team2;
        } else
        {
            team = TeamEnum.Team1;
        }

        colorIndex = 0;
        ApplyColor();
    }

    public void PickNextColor()
    {
        var palette = players[characterIndex].colors;

        if (palette.Length == 0)
        {
            return;
        }
        
        // TODO REFACTO
        colorIndex = (colorIndex + 1) % palette.Length;
        
        imageCharacter.material.SetColor("_Color_replacing_01", palette[colorIndex].primaryColor);
        imageCharacter.material.SetColor("_Color_replacing_02", palette[colorIndex].secondaryColor);
        imageCharacter.material.SetColor("_Color_replacing_03", palette[colorIndex].tertiaryColor);
        // var palette = team1Palette;
        // if (team == TeamEnum.Team2)
        // {
        //     palette = team2Palette;
        // }
        //
        // colorIndex = (colorIndex + 1) % palette.Length;
        //
        // clothes.color = palette[colorIndex];
    }

    public void ApplyColor()
    {
        colorIndex = 0;
        var palette = players[characterIndex].colors;
        imageCharacter.material.SetColor("_Color_replacing_01", palette[colorIndex].primaryColor);
        imageCharacter.material.SetColor("_Color_replacing_02", palette[colorIndex].secondaryColor);
        imageCharacter.material.SetColor("_Color_replacing_03", palette[colorIndex].tertiaryColor);
        imageCharacter.material.SetColor("_Color_to_replace_01", players[characterIndex].sourcePrimaryColor);
        imageCharacter.material.SetColor("_Color_to_replace_02", players[characterIndex].sourceSecondaryColor);
        imageCharacter.material.SetColor("_Color_to_replace_03", players[characterIndex].sourceTertiaryColor);
    }

    public void WakeUp(int playerNumber)
    {
        this.playerNumber = playerNumber;
        backgroundText.SetActive(false);
        playerBoard.SetActive(true);
        rwPlayer = ReInput.players.GetPlayer(playerNumber);

        var cursors = FindObjectsOfType<CursorController>(true);

        foreach (CursorController cursor in cursors) {
            if (cursor.playerNumber == this.playerNumber)
            {
                cursor.gameObject.SetActive(true);
            }
        }

        ApplyColor();
    }
    public bool IsAwake()
    {
        return playerNumber != -1;
    }

    public bool IsReady()
    {
        return playerNumber != -1 && isReady;
    }

    public void ToggleTeam()
    {
        Debug.Log("BUTTON" + rwPlayer.GetButtonDown("UISubmit"));
        Debug.Log("toggling");
        team = team == TeamEnum.Team1 ? TeamEnum.Team2 : TeamEnum.Team1;
    }

    public PlayerSelectConfiguration GetPlayer()
    {
        var playerSelect = new PlayerSelectConfiguration();
        playerSelect.player = player;
        
        // Color
        var palette = players[characterIndex].colors;
        playerSelect.destColor1 = palette[colorIndex].primaryColor;
        playerSelect.destColor2 = palette[colorIndex].secondaryColor;
        playerSelect.destColor3 = palette[colorIndex].tertiaryColor;
        playerSelect.sourceColor1 = players[characterIndex].sourcePrimaryColor;
        playerSelect.sourceColor2 = players[characterIndex].sourceSecondaryColor;
        playerSelect.sourceColor3 = players[characterIndex].sourceTertiaryColor;
        
        playerSelect.team = team;
        playerSelect.playerNumber = playerNumber;

        return playerSelect;
    }

    public void PickNextCharacter()
    {
        characterIndex = (characterIndex + 1) % players.Length;
        player = players[characterIndex].playerPrefab;
        characterSelectText.text = players[characterIndex].name;
        imageCharacter.sprite = players[characterIndex].selectSprite;
        colorIndex = 0;
        imageCharacter.material.SetColor("_Color_to_replace_01", players[characterIndex].sourcePrimaryColor);
        imageCharacter.material.SetColor("_Color_to_replace_02", players[characterIndex].sourceSecondaryColor);
        imageCharacter.material.SetColor("_Color_to_replace_03", players[characterIndex].sourceTertiaryColor);
        var palette = players[characterIndex].colors;

        if (palette.Length > 0)
        {
            imageCharacter.material.SetColor("_Color_replacing_01", palette[0].primaryColor);
            imageCharacter.material.SetColor("_Color_replacing_02", palette[0].secondaryColor);
            imageCharacter.material.SetColor("_Color_replacing_03", palette[0].tertiaryColor);
        }

        
    }
}
