using UnityEngine;

public class TetrisBlock : MonoBehaviour
{

    public Transform pivot;
    Block [,] gridTetris;
    public void Rotate( )
    {
        // Реализация вращения
    }

    public void Move( Vector3 direction )
    {
        transform.position += direction;  // Добавляем направление к текущей позиции
        Debug.Log ( "New block position: " + transform.position );  // Лог для проверки новой позиции

    }

    public bool IsValidPosition( )
    {
        foreach ( Transform child in transform )
        {
            Vector3 pos = child.position;

            // Проверка на выход за границы поля
            if ( pos.x < 0 || pos.x >= Helper.WIDTH || pos.y < 0 || pos.y >= Helper.HEIGHT )
            {
                return false;
            }

            // Проверка на пересечение с другими блоками
            if ( gridTetris [ ( int ) pos.y , ( int ) pos.x ] != null )
            {
                return false;
            }
        }
        return true;
    }


    public void Init( Block [ , ] grid )
    {
        pivot = transform;
        gridTetris = grid;
        // Логика инициализации
    }

    public void HardDrop( )
    {
        // Логика быстрой посадки блока
    }

    public Block GetDeadBlock( )
    {
        // Логика получения "мёртвого" блока
        return new Block ();
    }

}
