﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ship
{

    public partial class GenericShip
    {

        public Vector3 StartingPosition { get; private set; }

        public Movement.GenericMovement AssignedManeuver { get; set; }

        public bool IsLandedOnObstacle;
        public List<Collider> ObstaclesHit = new List<Collider>();

        public bool IsBumped
        {
            get { return ShipsBumped.Count != 0; }
        }

        public List<GenericShip> ShipsBumped = new List<GenericShip>();

        public GenericShip LastShipCollision { get; set; }

        public Dictionary<string, Movement.ManeuverColor> Maneuvers { get; private set; }
        public GenericAiTable HotacManeuverTable { get; protected set; }

        // EVENTS

        public event EventHandlerShipMovement AfterGetManeuverColorDecreaseComplexity;
        public event EventHandlerShipMovement AfterGetManeuverColorIncreaseComplexity;
        public event EventHandlerShipMovement AfterGetManeuverAvailablity;

        public event EventHandlerShip OnMovementStart;
        public event EventHandlerShip OnMovementExecuted;
        public event EventHandlerShip OnMovementFinish;

        public event EventHandlerShip OnPositionFinish;
        public static event EventHandler OnPositionFinishGlobal;

        // TRIGGERS

        public void StartMoving()
        {
            if (OnMovementStart != null) OnMovementStart(this);
        }

        public void CallExecuteMoving()
        {
            if (OnMovementExecuted != null) OnMovementExecuted(this);

            Triggers.ResolveTriggers(TriggerTypes.OnShipMovementExecuted, delegate() { Selection.ThisShip.CallFinishMovement(); });
        }

        public void CallFinishMovement()
        {
            if (OnMovementFinish != null) OnMovementFinish(this);

            Triggers.ResolveTriggers(TriggerTypes.OnShipMovementFinish, delegate () { Selection.ThisShip.FinishPosition(delegate () { Phases.FinishSubPhase(typeof(SubPhases.MovementExecutionSubPhase)); }); });
        }

        public void FinishPosition(System.Action callback)
        {
            if (OnPositionFinish != null) OnPositionFinish(this);
            if (OnPositionFinishGlobal != null) OnPositionFinishGlobal();

            Triggers.ResolveTriggers(TriggerTypes.OnPositionFinish, callback);
        }

        // MANEUVERS

        public Movement.ManeuverColor GetColorComplexityOfManeuver(string maneuverString)
        {
            return Maneuvers[maneuverString];
        }

        public Movement.ManeuverColor GetLastManeuverColor()
        {
            Movement.ManeuverColor result = Movement.ManeuverColor.None;

            result = AssignedManeuver.ColorComplexity;
            return result;
        }

        public Dictionary<string, Movement.ManeuverColor> GetManeuvers()
        {
            Dictionary<string, Movement.ManeuverColor> result = new Dictionary<string, Movement.ManeuverColor>();

            foreach (var maneuverHolder in Maneuvers)
            {
                Movement.MovementStruct movement = Game.Movement.ManeuverFromString(maneuverHolder.Key);
                if (AfterGetManeuverColorDecreaseComplexity != null) AfterGetManeuverColorDecreaseComplexity(this, ref movement);
                if (AfterGetManeuverColorIncreaseComplexity != null) AfterGetManeuverColorIncreaseComplexity(this, ref movement);
                if (AfterGetManeuverAvailablity != null) AfterGetManeuverAvailablity(this, ref movement);
                result.Add(maneuverHolder.Key, movement.ColorComplexity);
            }

            return result;
        }

    }

}
