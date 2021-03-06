﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CriticalHitCard
{
    public class BlindedPilot : GenericCriticalHit
    {
        public BlindedPilot()
        {
            Name = "Blinded Pilot";
            Type = CriticalCardType.Pilot;
            ImageUrl = "http://i.imgur.com/keETw8q.jpg";
        }

        public override void ApplyEffect(object sender, EventArgs e)
        {
            Messages.ShowInfo("Cannot perform attack next time");
            Game.UI.AddTestLogEntry("Cannot perform attack next time");

            Host.OnTryPerformAttack += OnTryPreformAttack;
            Host.AssignToken(new Tokens.BlindedPilotCritToken());

            Host.AfterAttackWindow += DiscardEffect;

            Triggers.FinishTrigger();
        }

        private void OnTryPreformAttack(ref bool result)
        {
            Messages.ShowErrorToHuman("Blinded Pilot: Cannot perfom attack now");
            result = false;
        }

        public override void DiscardEffect(Ship.GenericShip host)
        {
            Messages.ShowInfo("Blinded Pilot: Crit is flipped, pilot can perfom attacks");
            Game.UI.AddTestLogEntry("Blinded Pilot: Crit is flipped, pilot can perfom attacks");

            host.OnTryPerformAttack -= OnTryPreformAttack;
            host.RemoveToken(typeof(Tokens.BlindedPilotCritToken));

            host.AfterAttackWindow -= DiscardEffect;
        }
    }

}