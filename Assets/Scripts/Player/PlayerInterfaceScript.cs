using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInterfaceScript : MonoBehaviour
{
    //PlayerController playerController;
    PlayerBuilderScript playerBuilder;

    [Header("������������ ��������")]
    public Image healthImage;
    public TMP_Text healthText;

    [Header("��������� ��������")]
    public Canvas canvasDebug;
    public Image visualGasImage;
    public Canvas canvasMouseRadiusVisual;

    private void Start()
    {
        //playerController = GetComponent<PlayerController>();
        playerBuilder = GetComponent<PlayerBuilderScript>();

        //���������� ��������� ���� ��� ��������
        if (playerBuilder.tilesArray.isUpdateVisual)
        {
            ToggleGasVisualize();
            ToggleMouseRadiusVisualize();
        }
        if (playerBuilder.isDebugMode)
        {
            ToggleDebug();
        }
    }

    public void ToggleDebug()
    {
        playerBuilder.isDebugMode = !playerBuilder.isDebugMode;
        canvasDebug.gameObject.SetActive(playerBuilder.isDebugMode);
    }

    public void ToggleGasVisualize()
    {
        playerBuilder.tilesArray.ToggleVisual();
        if (playerBuilder.tilesArray.isUpdateVisual)
        {
            visualGasImage.color = new Color(155, 155, 255);
        }
        else
        {
            visualGasImage.color = new Color(255, 255, 255);
        }
    }

    public void ToggleMouseRadiusVisualize()
    {
        canvasMouseRadiusVisual.gameObject.SetActive(!canvasMouseRadiusVisual.isActiveAndEnabled);
    }

    /// <summary>
    /// ���������� ����������� �������� ���������
    /// </summary>
    /// <param name="current_health"></param>
    /// <param name="max_health"></param>
    public void ChangeHealthUI(int current_health, int max_health)
    {
        healthText.text = $"��:\n{current_health}/{max_health}";

        //healthImage.color = new Color(
        //    Mathf.Clamp(current_health, ),
        //    );
    }


    //==============��������� ���� ���������==============
    public void SetWallType()
    {
        playerBuilder.current_build_type = PlayerBuilderScript.BuildType.Wall;
    }
    public void SetWindowType()
    {
        playerBuilder.current_build_type = PlayerBuilderScript.BuildType.Window;
    }
    public void SetDoorType()
    {
        playerBuilder.current_build_type = PlayerBuilderScript.BuildType.Door;
    }

    //==============���� ��� ��������� �� �������==============
    public void AssemblyMode()
    {
        playerBuilder.LCMode = PlayerBuilderScript.LeftClickMode.Assembly;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerBuilder.LCMode}");
    }
    public void DisassemblyMode()
    {
        playerBuilder.LCMode = PlayerBuilderScript.LeftClickMode.Disassembly;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerBuilder.LCMode}");
    }
    public void InteractMode()
    {
        playerBuilder.LCMode = PlayerBuilderScript.LeftClickMode.Interact;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerBuilder.LCMode}");
    }

    //==============����� ������==============
    public void CreateMode()
    {
        playerBuilder.LCMode = PlayerBuilderScript.LeftClickMode.DebugCreate;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerBuilder.LCMode}");
    }
    public void DeleteMode()
    {
        playerBuilder.LCMode = PlayerBuilderScript.LeftClickMode.DebugDelete;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerBuilder.LCMode}");
    }
    public void AddGasMode()
    {
        playerBuilder.LCMode = PlayerBuilderScript.LeftClickMode.DebugAddGas;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerBuilder.LCMode}");
    }
    public void DefaultMode()
    {
        playerBuilder.LCMode = PlayerBuilderScript.LeftClickMode.None;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerBuilder.LCMode}");
    }
}
