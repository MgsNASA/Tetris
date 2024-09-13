using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Mode = Helper.Mode;

public class GameController : MonoBehaviour
{
    private readonly string STAGES_PATH = "Assets/Stages/";
    private readonly int [ ] scores = { 0 , 40 , 100 , 300 , 1200 };
    private readonly int NUM_OF_STAGES = 20;

    public float fallTime = 0.8f;
    private float N = 20;
    public Vector3 startPos = new Vector3 ();
    public readonly Vector3 [ ] Pivots = new [ ] { new Vector3 ( -0.33f , 0f , 0f ) , new Vector3 ( -0.27f , -0.15f , 0f ) , new Vector3 ( -0.27f , 0.1f , 0f ) , new Vector3 ( -0.12f , -0.1f , 0f ) , new Vector3 ( -0.22f , -0.1f , 0f ) , new Vector3 ( -0.02f , -0.1f , 0f ) , new Vector3 ( -0.2f , 0.1f , 0f ) };

    private float previousTime, previousToLeft, previousToRight;
    private int score = 0;
    private int linesDeleted = 0;
    private int numGems = 0;
    private float playTime;
    private int nextLevel;
    private List<int> deletingRow = new List<int> ();

    private int currStage = 0;

    private HashSet<int> deck = new HashSet<int> ();

    public Block [ , ] grid = new Block [ Helper.HEIGHT , Helper.WIDTH ];

    public TetrisBlock [ ] Blocks;

    public GhostBlock [ ] Ghosts;
    private int nextBlock;
    public GameObject nextBlockBackground, infoText, restartButton, resumeButton, pauseButton, speakerButton, muteButton;
    public GemBlock gemBlock;
    private bool hardDropped, gameOver, gameClear, isPaused, isShowingAnimation, isRowDown, isAnimating, isEndTurn;
    private ModeController controller;
    public Text timeValue, levelValue, linesValue, stageValue, scoreValue, gameModeValue;

    [SerializeField]
    // Ссылка на новый класс
    private TetrisBlockManager tetrisBlockManager;

    void Start( )
    {
        muteButton.SetActive ( true );
        speakerButton.SetActive ( false );
        tetrisBlockManager = new TetrisBlockManager ( this ); // Инициализация TetrisBlockManager
        InitGame ();
    }

    void InitGame( )
    {
        FindObjectOfType<AudioManager> ().Play ( "GameStart" );
        controller = GameObject.FindWithTag ( "ModeController" ).GetComponent<ModeController> ();
        gameModeValue.text = ( controller.GetMode () == Mode.stage ? "S T A G E" : "I N F I N I T E" ) + "  M O D E";
        infoText.SetActive ( false );
        restartButton.SetActive ( false );
        resumeButton.SetActive ( false );
        gameOver = false;
        gameClear = false;
        isShowingAnimation = false;
        isEndTurn = false;
        isAnimating = false;
        playTime = 0;
        linesDeleted = 0;
        score = 0;

        tetrisBlockManager.NextBlock ();
        if ( controller.GetMode () == Mode.stage )
            SetStage ();
        tetrisBlockManager.NewBlock ();
    }

    public void Pause( )
    {
        isPaused = true;
        pauseButton.SetActive ( false );
        resumeButton.SetActive ( true );
        FindObjectOfType<AudioManager> ().Mute ( "GameStart" , true );
    }

    public void Resume( )
    {
        isPaused = false;
        resumeButton.SetActive ( false );
        pauseButton.SetActive ( true );
        FindObjectOfType<AudioManager> ().Mute ( "GameStart" , false );
    }

    public void Mute( bool isMute )
    {
        FindObjectOfType<AudioManager> ().Mute ( "GameStart" , isMute );
        if ( isMute )
        {
            muteButton.SetActive ( false );
            speakerButton.SetActive ( true );
        }
        else
        {
            muteButton.SetActive ( true );
            speakerButton.SetActive ( false );
        }
    }

    void SetStage( )
    {
        for ( int y = 0; y < Helper.HEIGHT; y++ )
        {
            for ( int x = 0; x < Helper.WIDTH; x++ )
            {
                if ( grid [ y , x ] != null )
                    grid [ y , x ].Destroy ();
                int blockType = Helper.Stages [ currStage , Helper.HEIGHT - y - 1 , x ];
                switch ( blockType )
                {
                    case 0:
                        grid [ y , x ] = null;
                        break;
                    case 1:
                        grid [ y , x ] = Instantiate ( tetrisBlockManager.GetDeadBlock () , new Vector3 ( x , y , 0 ) , Quaternion.identity );
                        break;
                    case 2:
                        numGems++;
                        grid [ y , x ] = Instantiate ( gemBlock , new Vector3 ( x , y , 0 ) , Quaternion.identity );
                        break;
                }
            }
        }
    }

    void Update( )
    {
        if ( isPaused && Input.GetKeyDown ( KeyCode.P ) )
            Resume ();
        else if ( !isEndTurn && !gameOver && !gameClear && !isPaused && !isShowingAnimation )
        {
            if ( Input.GetKey ( KeyCode.LeftArrow ) && Time.time - previousToLeft > 0.1f )
            {
                tetrisBlockManager.HorizontalMove ( Vector3.left );
                previousToLeft = Time.time;
            }
            else if ( Input.GetKey ( KeyCode.RightArrow ) && Time.time - previousToRight > 0.1f )
            {
                tetrisBlockManager.HorizontalMove ( Vector3.right );
                previousToRight = Time.time;
            }
            else if ( Input.GetKeyDown ( KeyCode.UpArrow ) )
            {
                tetrisBlockManager.Rotate ();
            }
            else if ( Input.GetKeyDown ( KeyCode.Space ) )
            {
                while ( tetrisBlockManager.ValidMove () && !hardDropped )
                    tetrisBlockManager.VerticalMove ( Vector3.down );
            }
            else if ( Input.GetKeyUp ( KeyCode.Space ) )
            {
                hardDropped = false;
            }
            else if ( Input.GetKeyDown ( KeyCode.P ) )
            {
                Pause ();
            }

            if ( Time.time - previousTime > ( Input.GetKey ( KeyCode.DownArrow ) ? fallTime / 10 : fallTime ) )
            {
                tetrisBlockManager.VerticalMove ( Vector3.down );
                previousTime = Time.time;
            }
            if ( isAnimating && !isEndTurn )
            {
                EndTurn ();
                isEndTurn = false;
            }

            nextLevel = Mathf.RoundToInt ( ( linesDeleted / N ) + 1 );
            if ( Int16.Parse ( levelValue.text ) < nextLevel )
                fallTime /= 1f + ( Mathf.RoundToInt ( linesDeleted / N ) * 0.1f );

            playTime += Time.deltaTime;
            int minutes = Mathf.RoundToInt ( ( playTime % ( 60 * 60 * 60 ) ) / ( 60 * 60 ) ), seconds = Mathf.RoundToInt ( ( playTime % ( 60 * 60 ) ) / 60 ), microseconds = Mathf.RoundToInt ( playTime % 60 );
            timeValue.text = String.Format ( "{0}:{1}:{2}" , ( minutes < 10 ? "0" : "" ) + minutes.ToString () , ( seconds < 10 ? "0" : "" ) + seconds.ToString () , ( microseconds < 10 ? "0" : "" ) + microseconds.ToString () );

            tetrisBlockManager.GhostBlockImgUpdate ();
            InfoUpdate ();
        }
    }

    private void InfoUpdate( )
    {
        levelValue.text = nextLevel.ToString ();
        linesValue.text = linesDeleted.ToString ();
        stageValue.text = ( currStage + 1 ).ToString ();
        scoreValue.text = score.ToString ();
    }

    private void EndTurn( )
    {
        isEndTurn = true;
        tetrisBlockManager.EndTurn ();
        isEndTurn = false;
    }

    private void GameOver( )
    {
        gameOver = true;
        tetrisBlockManager.GameOver ();
        restartButton.SetActive ( true );
    }

    private void GameClear( )
    {
        gameClear = true;
        tetrisBlockManager.GameClear ();
        StartCoroutine ( CountDown () );
    }

    private IEnumerator CountDown( )
    {
        yield return new WaitForSeconds ( 0.5f );
        infoText.GetComponent<TextMeshProUGUI> ().text = "3";
        yield return new WaitForSeconds ( 0.5f );
        infoText.GetComponent<TextMeshProUGUI> ().text = "2";
        yield return new WaitForSeconds ( 0.5f );
        infoText.GetComponent<TextMeshProUGUI> ().text = "1";
        yield return new WaitForSeconds ( 0.5f );
        currStage++;
        InitGame ();
    }

    public void GoBack( )
    {
        SceneManager.LoadScene ( 0 );
    }
}
