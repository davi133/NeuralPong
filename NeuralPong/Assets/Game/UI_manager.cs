using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_manager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI placar;
    [SerializeField] TextMeshProUGUI status;
    [SerializeField] TextMeshProUGUI leftLabel;
    [SerializeField] TextMeshProUGUI rightLabel;



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdatePlacar(Vector2 pontos)
    {
        placar.SetText($"{pontos.x} x {pontos.y}");
    }

    public void setWinner(GameManager.Winner winner)
    {
       
        if (winner == GameManager.Winner.Left && leftLabel)
        {
            leftLabel.SetText("Vencedor");
        }
        else if (winner == GameManager.Winner.Right && rightLabel)
        {
            rightLabel.SetText("Vencedor");
        }
        else
        {
            leftLabel.SetText("");
            rightLabel.SetText("");
        }
    }

    public void setStatus(GameManager.GameState state)
    {
        if (status)
        {
            status.SetText(state.ToString());
        }
    }
}
