using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisBlockManager
{
    private GameController gameController;
    public TetrisBlock currBlock;
    private GhostBlock ghostBlock;
    private int nextBlock;

    public TetrisBlockManager( GameController controller )
    {
        gameController = controller;
    }

    public void NewBlock( )
    {
        currBlock = GameObject.Instantiate ( gameController.Blocks [ nextBlock ] , gameController.startPos , Quaternion.identity );
        currBlock.Init ( gameController.grid );
        currBlock.pivot.position += gameController.Pivots [ nextBlock ];
        
        currBlock.HardDrop ();
        GhostBlockImgUpdate ();

    }

    public void EndTurn( )
    {
        // Логика окончания хода
    }

    public void GameOver( )
    {
        // Логика окончания игры
    }

    public void GameClear( )
    {
        // Логика победы в игре
    }

    public void NextBlock( )
    {
        nextBlock = Random.Range ( 0 , gameController.Blocks.Length );
        GhostBlockImgUpdate ();
    }

    public void HorizontalMove( Vector3 direction )
    {
        Debug.Log ( "Horizontal Move called with direction: " + direction );

        // Сначала передвигаем блок
        currBlock.Move ( direction );

        // Проверяем, валидна ли новая позиция
        if ( !currBlock.IsValidPosition () )
        {
            // Если позиция не валидна, отменяем движение
            currBlock.Move ( -direction );
            Debug.Log ( "Invalid position! Move cancelled." );
        }
        else
        {
            GhostBlockImgUpdate ();
        }
    }

    public void VerticalMove( Vector3 direction )
    {
        Debug.Log ( "Vertical Move called with direction: " + direction );

        // Передвигаем блок
        currBlock.Move ( direction );

        // Проверка допустимости
        if ( !currBlock.IsValidPosition () )
        {
            // Если движение не допустимо, отменяем
            currBlock.Move ( -direction );
            Debug.Log ( "Invalid position! Move cancelled." );
        }

    }

    public void Rotate( )
    {
        currBlock.Rotate ();
    }

    public bool ValidMove( )
    {
        return currBlock.IsValidPosition ();
    }

    public void GhostBlockImgUpdate( )
    {
        // Обновление визуализации Ghost блока
    }

    public Block GetDeadBlock( )
    {
        return currBlock.GetDeadBlock ();
    }
}
