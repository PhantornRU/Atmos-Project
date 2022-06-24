using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Experimental.Rendering.Universal;

public class PlayerInterfaceScript : MonoBehaviour
{
    PlayerController playerController;
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
        playerController = GetComponent<PlayerController>();
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

        cur_time_blink = timeBlinkHealthIcon;
    }

    Color32 saveColor;
    float timeBlinkHealthIcon = 1.5f;
    float cur_time_blink = 0f;
    bool isNeedBlinkHealth = false;
    bool checkHalfBlink = false;
    private void Update()
    {
        //������� ������� ����� ���� ��������
        if (isNeedBlinkHealth && playerController.isActive)
        {
            cur_time_blink -= Time.deltaTime;
            if (cur_time_blink <= 0)
            {
                healthImage.color = saveColor;
                checkHalfBlink = false;
                cur_time_blink = timeBlinkHealthIcon;
            }   
            else if (!checkHalfBlink && cur_time_blink - timeBlinkHealthIcon / 2 <= 0)
            {
                healthImage.color = Color.black;
                checkHalfBlink = true;
            }
        }
    }

    public void ToggleDebug()
    {
        playerBuilder.isDebugMode = !playerBuilder.isDebugMode;
        canvasDebug.gameObject.SetActive(playerBuilder.isDebugMode);
    }


    public Light2D globalLight;
    [HideInInspector] public bool isLight = false;
    public void ToggleLight()
    {
        isLight = !isLight;
        foreach (Light2D light in FindObjectsOfType<Light2D>())
        {
            light.intensity = isLight ? 0f : 0.5f;
            if (light.GetComponentInParent<MobController>() || light.GetComponentInParent<PlayerController>())
            {
                light.intensity = isLight ? 0.5f : 0.65f;
            }
            else if (light.GetComponentInParent<DamageTriggerZone>())
            {
                light.intensity = isLight ? 0.25f : 0.5f;
            }
        }
        globalLight.intensity = isLight ? 1f : 0.15f;
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
    public void ChangeHealthUI(float current_health, float max_health)
    {
        healthText.text = $"��:\n{current_health}/{max_health}";

        //��������� ��� ����� ������ ������������ �������� ��� ���������� 20% ��������
        if (current_health / max_health <= 0.2)
        {
            isNeedBlinkHealth = true;
        }
        else
        {
            isNeedBlinkHealth = false;
        }

        Vector3 greenV = new Vector3(0, 255f, 0);
        Vector3 redV = new Vector3(255f, 0, 0);
        Vector3 colorV = Vector3.Lerp(redV, greenV, current_health/max_health);
        saveColor = new Color32((byte)colorV.x, (byte)colorV.y, (byte)colorV.z, (byte)255f);
        if (!isNeedBlinkHealth) //���� �� ������, �� ���� ���������� �����, ����� ���������� ����� ���� �� ����� �������
        {
            healthImage.color = saveColor;
        }
    }


    //==============��������� ���� ���������==============
    public void SetWallType()
    {
        playerBuilder.current_build_type = PlayerBuilderScript.BuildType.Wall;
        playerBuilder.curBuildingType = TileBlock.BuildingType.Wall;
        playerBuilder.LCMode = PlayerBuilderScript.LeftClickMode.Assembly;
    }
    public void SetWindowType()
    {
        playerBuilder.current_build_type = PlayerBuilderScript.BuildType.Window;
        playerBuilder.curBuildingType = TileBlock.BuildingType.Window;
        playerBuilder.LCMode = PlayerBuilderScript.LeftClickMode.Assembly;
    }
    public void SetDoorType()
    {
        playerBuilder.current_build_type = PlayerBuilderScript.BuildType.Door;
        playerBuilder.curBuildingType = TileBlock.BuildingType.Door;
        playerBuilder.LCMode = PlayerBuilderScript.LeftClickMode.Assembly;
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
