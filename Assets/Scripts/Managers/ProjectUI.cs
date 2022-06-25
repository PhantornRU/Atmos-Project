using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProjectUI : MonoBehaviour
{
    ProjectInitializer projectInitializer;
    ProjectSaveLoad projectSaveLoad;

    [Header("Полотна")]
    public Canvas canvasPause;
    public Canvas canvasGame;
    public Canvas canvasMenuEsc;
    public Canvas canvasChooseScene;

    [Header("Кнопки")]
    public Button buttonChooseScene1;
    public Button buttonChooseScene2;
    public Button buttonChooseScene3;

    private bool isPause = false;

    private void Start()
    {
        projectInitializer = GetComponent<ProjectInitializer>();
        projectSaveLoad = GetComponent<ProjectSaveLoad>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Pause) && !canvasMenuEsc.isActiveAndEnabled && !canvasChooseScene.isActiveAndEnabled)
        {
            Pause();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MenuEsc();
        }
    }

    public void Continue()
    {
        PauseGameTimeAndActive();
        canvasMenuEsc.gameObject.SetActive(false);
        projectInitializer.ChangeStateActive(true);
    }

    public void MenuEsc()
    {
        PauseGameTimeAndActive();
        canvasPause.gameObject.SetActive(false);
        canvasChooseScene.gameObject.SetActive(false);
        canvasMenuEsc.gameObject.SetActive(isPause);
        projectInitializer.ChangeStateActive(!isPause);
    }

    public void Pause()
    {
        PauseGameTimeAndActive();
        projectInitializer.ChangeStateActive(!isPause);
        canvasPause.gameObject.SetActive(isPause);
    }

    void PauseGameTimeAndActive()
    {
        isPause = !isPause;
        Time.timeScale = isPause ? 0 : 1;
    }

    public void Save()
    {
        projectSaveLoad.SaveAllData();
    }

    public void Load()
    {
        projectSaveLoad.LoadAllData();
    }

    public void MenuChooseScene()
    {
        canvasMenuEsc.gameObject.SetActive(false);
        canvasChooseScene.gameObject.SetActive(true);
    }
    public void ButtonChooseScene()
    {
        if (buttonChooseScene1.interactable == false)
        {
            SceneManager.LoadScene(0);
        }
        else if (buttonChooseScene2.interactable == false)
        {
            SceneManager.LoadScene(1);
        }
        else if (buttonChooseScene3.interactable == false)
        {
            SceneManager.LoadScene(2);
        }
        else
        {
            Debug.LogWarning("Не выбрана сцена для загрузки");
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}
