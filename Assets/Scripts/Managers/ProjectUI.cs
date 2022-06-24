using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectUI : MonoBehaviour
{
    ProjectInitializer projectInitializer;
    ProjectSaveLoad projectSaveLoad;

    [Header("Полотна")]
    public Canvas canvasPause;
    public Canvas canvasGame;
    public Canvas canvasMenuEsc;

    private bool isPause = false;

    private void Start()
    {
        projectInitializer = GetComponent<ProjectInitializer>();
        projectSaveLoad = GetComponent<ProjectSaveLoad>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Pause))
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

    public void ChooseScene() //!!!доделать!!!
    {
        Debug.LogWarning("Отсутствует выбор сцен");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
