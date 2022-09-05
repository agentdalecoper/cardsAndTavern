using System;
using Client;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class MapControlManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public float dragSpeed = 1;
    private bool onDrug;

    private static MapControlManager instance;
    public static MapControlManager Instance => instance;


    private void Awake()
    {
        instance = this;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        onDrug = true;

        dragAndInteraction();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Drag");
        float x = Input.GetAxis("Mouse X") * dragSpeed;
        float y = Input.GetAxis("Mouse Y") * dragSpeed;

        Vector3 move = new Vector3(-x, -y, 0);
        // sceneConfiguration.uiCamera.transform.Translate(move);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        onDrug = false;
    }

    private void dragAndInteraction()
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onDrug)
        {
            return;
        }


        // if (unitSelected)
        // {
        //     Place destinationPlace = PlaceController.Instance.FindPlace(eventData.pointerCurrentRaycast.worldPosition);
        //
        //     if (destinationPlace != null && destinationPlace.Visibility == Visibility.VisitedVisible)
        //     {
        //         unitSelected.testMoveComponent.MoveToDistinctPlace(eventData.pointerCurrentRaycast.worldPosition);
        //         unitSelected = null;
        //     }
        // }
        //
        // log.Debug("on click");
    }
    //
    // public void OnPlaceClicked(Place place)
    // {
    //     unitSelected.testMoveComponent.MoveToPlace(place);
    // }

//    private static void CheckIfToAttack(Vector3Int tilePos)
//    {
//        if (TurnBasedManager.Instance.PlayerTurns < 1)
//        {
//            return;
//        }
//
//        UnitEntity unitEntity = CheckEnemy(tilePos, out UnitEntity enemy);
//
//        if (enemy != null)
//        {
//            unitEntity.attackingUnitEntity = enemy;
//            UnitController.Instance.AttackEnemy(unitEntity, enemy);
//            TurnBasedManager.Instance.PlayerTurns -= 1;
//        }
//    }

//    //ToDo refactor
//    private static UnitEntity CheckEnemy(Vector3Int tilePos, out UnitEntity enemy)
//    {
//        var unitEntity = PlayerManager.Instance.player.gameObject.GetComponent<UnitEntity>();
//        Place place = PlaceController.Instance.GetPlace(tilePos);
//        enemy = AttackDecision.CheckEnemiesInPlace(place, unitEntity);
//        return unitEntity;
//    }

//    private static (Vector3Int tilePos, bool canGo, int distance) GetMapClickData()
//    {
////        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
////        Vector3 worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);
////        var tilePos = HexGridController.Instance.GetTilePosition(worldPoint);
////        Vector3Int placeTilePos = PlayerManager.Instance.player.moveEntity.currentPlace.pos;
////        int distance = HexGridController.Instance.CubeDistance(tilePos, placeTilePos);
////
////        var canGo = TurnBasedManager.Instance.PlayerTurns >= 1 && distance == 1;
////        return (tilePos, canGo, distance);
//    }

//    private void MovePlayer(Vector3Int tilePos)
//    {
//        UnitEntity unitEntity = CheckEnemy(tilePos, out UnitEntity enemy);
//
//        if (enemy != null)
//        {
//            return;
//        }
//
//        Place place = PlaceController.Instance.GetPlace(tilePos);
//        PlayerManager.Instance.player.ChangePlace(place);
//
//        TurnBasedManager.Instance.PlayerTurns -= 1;
//    }
}