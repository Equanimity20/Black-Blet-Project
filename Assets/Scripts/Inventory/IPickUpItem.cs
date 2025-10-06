using System.ComponentModel;
using UnityEngine;

public interface IPickUpItem
{
    public void PickUp();
    public Vector3 SetProperEquipOrientation();
}
