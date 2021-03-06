﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InputManager : MonoBehaviour
{

    private SelectableObject _selectedUnit;

    public SelectableObject selectedUnit
    {
        get { return _selectedUnit;}
        set { _selectedUnit = value;

        uiManager.UpdateUI(_selectedUnit);
        }
    }
    
    public ClickMode currentMode = ClickMode.Selection;
    public bool isCommanding { get { return (ClickMode.Selection != currentMode); } }

    [SerializeField]
    private GameObject selectionCube;

    [SerializeField]
    private LayerMask selectionMask;
    [SerializeField]
    private LayerMask spaceMask;
    [SerializeField]
    private LayerMask structureMask;
    [SerializeField]
    private LayerMask unitMask;

    private Vector3 mousePosition;
    private Vector3 clickDestination;

    private Grid grid;
    private TaskManager taskManager;
    private UIManager uiManager;
    

    void Awake ()
    {
        grid = GetComponent<Grid>();
        taskManager = GetComponent<TaskManager>();
        uiManager = GetComponent<UIManager>();
    }

    void Update ()
    {
        if (Input.GetButtonDown("Fire1"))
        { ClickPrimary(); }
        if (Input.GetButtonDown("Fire2"))
        { ClickSecondary(); }
        if (Input.GetButtonDown("Cancel"))
        {
            if (isCommanding)
            { CancelCommand(); }
            else
            { ClearSelection(); }
        }
    }

    void ClickPrimary ()
    {
        mousePosition = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        LayerMask maskInUse;
    
        switch (currentMode)
        {
       case ClickMode.Move:
        case ClickMode.Build:
        case ClickMode.Attackmove:
            maskInUse = spaceMask;
            break;
        case ClickMode.Unbuild:
            maskInUse = structureMask;
            break;
        case ClickMode.Attack:
            maskInUse = unitMask;
            break;
        case ClickMode.Selection:
        default:
            maskInUse = selectionMask;
            break;
        
        }

        if (Physics.Raycast(ray, out hit, 1000f, maskInUse))
        {
            Transform other = hit.collider.transform;
            switch (currentMode)
            {
            case ClickMode.Selection:
                UpdateSelection(other);
                break;
            case ClickMode.Move:
                MoveUnit(hit.point);
                break;
            case ClickMode.Build:
                BuildUnit(hit.point);
                break;
            }
        }
    }

    void ClickSecondary ()
    {
        mousePosition = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (isCommanding)
        {
            CancelCommand();
        }
        else if (null != selectedUnit)
        {
            //if (null != selectedUnit.GetComponent<Unit>())
            //{
            if (Physics.Raycast(ray, out hit, 1000f, spaceMask))
            {
                MoveUnit(hit.point);
            }
            //}

        }
    }

    public void UpdateSelection (Transform other)
    {
        selectedUnit = other.GetComponent<SelectableObject>();
        selectionCube.transform.SetParent(other, false);

        Vector3 selection = selectionCube.transform.position;
        selectionCube.transform.position = new Vector3(selection.x, selectionCube.transform.position.y, selection.z);

        selectionCube.SetActive(true);

        //selectedUnit.Select();

        // display Panel UI
        uiManager.DisplayUI(selectedUnit);
    }

    public void CancelCommand ()
    {
        StopCoroutine("FollowPath");
        StopCoroutine("DrawPath");
        StopCoroutine("BuildJob");
        
        currentMode = ClickMode.Selection;
    }

    void MoveUnit (Vector3 destination)
    {
        Node node = grid.WorldToNode(destination);
        Vector3 worldpoint = node.worldPosition;

        selectedUnit.GetComponent<Unit>().Move(worldpoint);
    }
    void BuildUnit (Vector3 destination)
    {
        Node node = grid.WorldToNode(destination);
        Vector3 worldpoint = node.worldPosition;
        
        // set placeholder
        GameObject placeholder;
        taskManager.MakePlaceholder(worldpoint, out placeholder);

        // start coroutine
        selectedUnit.GetComponent<Builder>().Build(placeholder);
    }

    void ClearSelection ()
    {
        selectedUnit = null;
        selectionCube.SetActive(false);
        selectionCube.transform.SetParent(null, false);
        selectionCube.transform.parent = null;
        uiManager.ResetUI();

    }

}

[System.Serializable]
public enum ClickMode
{
    Selection,
    Move,
    Build,
    Unbuild,
    Attackmove,
    Attack
    
}

//[System.Serializable]
//public enum MButton
//{
//    Primary,
//    Secondary,
//    Tertiary,
//    Quaternary, 
//    Quinary
//}
