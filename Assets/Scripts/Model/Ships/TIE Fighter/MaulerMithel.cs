﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ship
{
    namespace TIEFighter
    {
        public class MaulerMithel : TIEFighter
        {
            public MaulerMithel(Players.PlayerNo playerNo, int shipId, Vector3 position) : base(playerNo, shipId, position)
            {
                PilotName = "\"Mauler Mithel\"";
                isUnique = true;
                PilotSkill = 7;
                AddUpgradeSlot(Upgrade.UpgradeSlot.Elite);

                AfterGotNumberOfPrimaryWeaponAttackDices += MaulerMithelPilotAbility;

                InitializePilot();
            }

            private void MaulerMithelPilotAbility(ref int result)
            {
                if (Actions.GetRange(Combat.Attacker, Combat.Defender) == 1)
                {
                    Game.UI.ShowInfo("\"Mauler Mithel\": +1 attack die");
                    result++;
                }
            }

        }
    }
}
