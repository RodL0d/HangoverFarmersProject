using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverUI;

    private void Start()
    {
        
    }

   

    private void changeEnergy()
    {
        EnergySystem.instance.initializedscene();
    }

    public void updateScene(string Main)
    {
        SceneManager.LoadScene(Main);
        EnergySystem.instance.initializedscene();
    }
    public void doExitGame()
    {
        Application.Quit();
    }

    public void gameOver()
    {
        gameOverUI.SetActive(true);
        EnergySystem.instance.UseEnergy();
        Debug.Log("energia");
    }

    public void voltar(string FarmHub)
    {
        // Registra o m�todo que ser� chamado quando a cena terminar de carregar
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Carrega a cena
        SceneManager.LoadScene(FarmHub);
    }

    // M�todo que ser� chamado ap�s a cena ser carregada
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Verifica se a cena carregada � a cena FarmHub
        if (scene.name == "FarmHub")
        {
            // Chama o m�todo initializedscene do EnergySystem
            EnergySystem.instance.initializedscene();

            // Remove este m�todo do evento para evitar chamadas duplicadas
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

}
