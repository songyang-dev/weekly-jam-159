﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

/// <summary>
/// Script controlling how a person interacts with the environment
/// </summary>
public abstract class Person : MonoBehaviour
{
    /// <summary>
    /// Used in finding the tile that the customer collides with
    /// </summary>
    [Tooltip("Used to find which tile is colliding with, usually the value is 1/2 the size of a tile")]
    public float CollisionStep = 0.5f;

    /// <summary>
    /// Whether this person is selected or not
    /// </summary>
    protected bool isSelected;

    /// <summary>
    /// Ai path setter
    /// </summary>
    [SerializeField]
    protected AIDestinationSetter pathSetter;

    /// <summary>
    /// Reference to the behavioral state machine
    /// </summary>
    public Animator behavior;

    /// <summary>
    /// Paired empty object for path finding.
    /// Destroyed when customer is destroyed.
    /// </summary>
    protected GameObject pairedEmpty;

    
    /// <summary>
    /// Standard Unity collision listener method
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionEnter2D(Collision2D other)
    {
        switch (other.gameObject.name)
        {

            // colliding with something in the tables tilemap
            case "Tables":
                CollideTable(other);
                break;

            case "Machines":
                CollideMachine(other);
                break;
        }
    }

    private void CollideMachine(Collision2D other)
    {
        var cell = GetCollisionCell(other);

        // custom action on the collided machine
        ActOnMachineCollision(cell);
    }

    /// <summary>
    /// Method of what to do when colliding with a machine
    /// </summary>
    /// <param name="cell"></param>
    protected abstract void ActOnMachineCollision(Vector3Int cell);

    /// <summary>
    /// Triggers an interaction between the customer and the table
    /// </summary>
    /// <param name="other">Unity's detected collision</param>
    private void CollideTable(Collision2D other)
    {
        Vector3Int cell = GetCollisionCell(other);

        // Custom action on the collided table
        ActOnTableCollision(cell);
        AkSoundEngine.PostEvent("CoinPay", gameObject);
    }

    /// <summary>
    /// Method of what to do to the collided table
    /// </summary>
    /// <param name="cell">Position of the table in tilemap cell coords</param>
    protected abstract void ActOnTableCollision(Vector3Int cell);

    /// <summary>
    /// Finds the cell coordinate of the collision on the collided tilemap
    /// </summary>
    /// <param name="other">Collision with a tilemap</param>
    /// <returns></returns>
    private Vector3Int GetCollisionCell(Collision2D other)
    {
        var tablesTilemap = other.gameObject.GetComponent<Tilemap>();
        var contact = other.GetContact(0); // first contact point

        // To get the tile, we need to get a world position inside a cell.
        // But the collision position is not inside the cell all the time.
        // So we'll lengthen the path taken by the customer by a step to get a
        // position inside the cell.

        // Find the position that is slightly ahead of the customer
        // Use the opposite direction of the normal of the surface of collision 
        var oppositeNormal = contact.normal.normalized * -1;

        var simulatedPosition = oppositeNormal * CollisionStep + contact.point;
        var cell = tablesTilemap.WorldToCell(simulatedPosition);
        return cell;
    }

    /// <summary>
    /// Link an empty object to be this person's AI path destination,
    /// must not be a child object of the person
    /// </summary>
    /// <param name="empty"></param>
    public void PairEmpty(GameObject empty) {
        pairedEmpty = empty;
    }

    /// <summary>
    /// Sets the destination of the pathfinding algorithm
    /// </summary>
    /// <param name="destination"></param>
    protected void SetPathDestination(Vector3 destination)
    {
        pairedEmpty.transform.position = destination;
        pathSetter.target = pairedEmpty.transform;
    }

    /// <summary>
    /// Get current destination of the A* algorithm
    /// </summary>
    /// <returns></returns>
    public Vector3 GetPathDestination() => this.pairedEmpty.transform.position;

    private void OnDestroy()
    {
        Destroy(pairedEmpty);
    }
}