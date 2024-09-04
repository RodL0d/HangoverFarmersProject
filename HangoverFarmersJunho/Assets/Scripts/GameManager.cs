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
        // Registra o método que será chamado quando a cena terminar de carregar
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Carrega a cena
        SceneManager.LoadScene(FarmHub);
    }

    // Método que será chamado após a cena ser carregada
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Verifica se a cena carregada é a cena FarmHub
        if (scene.name == "FarmHub")
        {
            // Chama o método initializedscene do EnergySystem
            EnergySystem.instance.initializedscene();

            // Remove este método do evento para evitar chamadas duplicadas
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

}
