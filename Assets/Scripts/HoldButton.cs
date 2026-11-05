using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerController player;
    public float direction;

    public void OnPointerDown(PointerEventData e) { player.SetMove(direction); }
    public void OnPointerUp(PointerEventData e) { player.SetMove(0f); }
}