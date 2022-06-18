using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInterfaceScript : MonoBehaviour
{
    PlayerController playerController;

    [Header("������������ ��������")]
    public Image healthImage;
    public TMP_Text healthText;

    [Header("��������� ��������")]
    public Canvas canvasDebug;
    public Image visualGasImage;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void ToggleDebug()
    {
        canvasDebug.gameObject.SetActive(!canvasDebug.gameObject.activeInHierarchy);
    }

    public void ToggleGasVisualize()
    {
        playerController.tilesArray.ToggleVisual();
        if (playerController.tilesArray.isUpdateVisual)
        {
            visualGasImage.color = new Color(155, 155, 255);
        }
        else
        {
            visualGasImage.color = new Color(255, 255, 255);
        }
    }

    /// <summary>
    /// ���������� ����������� �������� ���������
    /// </summary>
    /// <param name="current_health"></param>
    /// <param name="max_health"></param>
    public void ChangeHealthUI(int current_health, int max_health)
    {
        healthText.text = $"��:\n{current_health}/{max_health}";
    }


    //==============���� ��� ��������� �� �������==============
    public void DisassemblyMode()
    {
        playerController.LCMode = PlayerController.LeftClickMode.Disassembly;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerController.LCMode}");
    }
    public void AssemblyMode()
    {
        playerController.LCMode = PlayerController.LeftClickMode.Assembly;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerController.LCMode}");
    }

    //==============��������� ���� ���������==============
    public void SetWallType()
    {

    }
    public void SetWindowType()
    {

    }
    public void SetDoorType()
    {

    }

    //==============����� ������==============
    public void CreateMode()
    {
        playerController.LCMode = PlayerController.LeftClickMode.DebugCreate;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerController.LCMode}");
    }
    public void DeleteMode()
    {
        playerController.LCMode = PlayerController.LeftClickMode.DebugDelete;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerController.LCMode}");
    }
    public void InteractMode()
    {
        playerController.LCMode = PlayerController.LeftClickMode.Interact;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerController.LCMode}");
    }
    public void AddGasMode()
    {
        playerController.LCMode = PlayerController.LeftClickMode.DebugAddGas;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerController.LCMode}");
    }
    public void DefaultMode()
    {
        playerController.LCMode = PlayerController.LeftClickMode.None;
        Debug.Log($"������� ����� ����� ������ ���� ������: {playerController.LCMode}");
    }
}
