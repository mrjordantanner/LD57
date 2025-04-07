using UnityEngine;
using DG.Tweening;
using System.Collections;


public interface ITarget
{
    public void OnTargetHit() { }
    public GameObject Entity { get; }

}
