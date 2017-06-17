﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ship
{

    public partial class GenericShip
    {
 
        private List<CriticalHitCard.GenericCriticalHit> AssignedCritCards = new List<CriticalHitCard.GenericCriticalHit>();
        private List<CriticalHitCard.GenericCriticalHit> AssignedDamageCards = new List<CriticalHitCard.GenericCriticalHit>();

        // EVENTS

        public event EventHandlerShip OnCombatPhaseStart;

        public event EventHandlerBool OnTryPerformAttack;

        public event EventHandler OnAttack;
        public event EventHandler OnDefence;

        public event EventHandlerInt AfterGotNumberOfPrimaryWeaponAttackDices;
        public event EventHandlerInt AfterGotNumberOfPrimaryWeaponDefenceDices;
        public event EventHandlerInt AfterGotNumberOfAttackDices;

        public event EventHandlerShip AfterAssignedDamageIsChanged;

        public event EventHandlerBool OnCheckFaceupCrit;
        public event EventHandlerShipCrit OnAssignCrit;

        public event EventHandler OnDestroyed;

        public event EventHandlerShip AfterAttackWindow;

        public event EventHandlerShip AfterCombatEnd;

        // TRIGGERS

        public void CallOnCombatPhaseStart()
        {
            if (OnCombatPhaseStart != null) OnCombatPhaseStart(this);
        }

        public bool CallTryPerformAttack(bool result = true)
        {
            if (OnTryPerformAttack != null) OnTryPerformAttack(ref result);

            return result;
        }

        public void CallAttackStart()
        {
            if (Combat.Attacker.ShipId == this.ShipId) IsAttackPerformed = true;
            if (OnAttack != null) OnAttack();
        }

        public void CallDefenceStart()
        {
            if (OnDefence != null) OnDefence();
        }

        public void CallAfterAttackWindow()
        {
            if (AfterAttackWindow != null) AfterAttackWindow(this);
        }

        public void CallCombatEnd()
        {
            if (AfterCombatEnd != null) AfterCombatEnd(this);
        }

        // DICES

        public int GetNumberOfAttackDices(GenericShip targetShip)
        {
            int result = 0;

            if (Combat.SecondaryWeapon == null)
            {
                result = Firepower;
                if (AfterGotNumberOfPrimaryWeaponAttackDices != null) AfterGotNumberOfPrimaryWeaponAttackDices(ref result);
            }
            else
            {
                result = Combat.SecondaryWeapon.GetAttackValue();
            }

            if (AfterGotNumberOfAttackDices != null) AfterGotNumberOfAttackDices(ref result);

            if (result < 0) result = 0;
            return result;
        }

        public int GetNumberOfDefenceDices(GenericShip attackerShip)
        {
            int result = Agility;

            if (Combat.SecondaryWeapon == null)
            {
                if (AfterGotNumberOfPrimaryWeaponDefenceDices != null) AfterGotNumberOfPrimaryWeaponDefenceDices(ref result);
            }
            return result;
        }

        // REGEN

        public bool TryRegenShields()
        {
            bool result = false;
            if (Shields < MaxShields)
            {
                result = true;
                Shields++;
                AfterAssignedDamageIsChanged(this);
            };
            return result;
        }

        public bool TryRegenHull()
        {
            bool result = false;
            if (Hull < MaxHull)
            {
                result = true;
                Hull++;
                AfterAssignedDamageIsChanged(this);
            };
            return result;
        }

        // DAMAGE

        public IEnumerator SufferDamage(DiceRoll damage)
        {

            int shieldsBefore = Shields;

            Shields = Mathf.Max(Shields - damage.Successes, 0);

            damage.CancelHits(shieldsBefore - Shields);

            if (damage.Successes != 0)
            {
                foreach (Dice dice in damage.DiceList)
                {
                    if ((dice.Side == DiceSide.Success) || (dice.Side == DiceSide.Crit))
                    {
                        if (CheckFaceupCrit(dice))
                        {
                            Triggers.AddTrigger("Draw faceup damage card", TriggerTypes.OnDamageCardIsDealt, CriticalHitsDeck.DrawCrit);
                            //CriticalHitsDeck.DrawCrit(this);
                        }
                        else
                        {
                            Triggers.AddTrigger("Draw faceup damage card", TriggerTypes.OnDamageCardIsDealt, CriticalHitsDeck.DrawRegular);
                            //SufferHullDamage();
                        }
                    }
                }
            }

            yield return Triggers.ResolveAllTriggers(TriggerTypes.OnDamageCardIsDealt);

            CallAfterAssignedDamageIsChanged();
        }

        public void CallAfterAssignedDamageIsChanged()
        {
            if (AfterAssignedDamageIsChanged != null) AfterAssignedDamageIsChanged(this);
        }

        private bool CheckFaceupCrit(Dice dice)
        {
            bool result = false;

            if (dice.Side == DiceSide.Crit) result = true;

            if (OnCheckFaceupCrit != null) OnCheckFaceupCrit(ref result);

            return result;
        }

        public void SufferHullDamage()
        {
            Hull--;
            Hull = Mathf.Max(Hull, 0);

            CallAfterAssignedDamageIsChanged();

            IsHullDestroyedCheck();
        }

        public void SufferCrit(CriticalHitCard.GenericCriticalHit crit)
        {
            if (OnAssignCrit != null) OnAssignCrit(this, ref crit);

            if (crit != null)
            {
                SufferHullDamage();

                AssignedCritCards.Add(crit);
                crit.AssignCrit(this);
            }
        }

        public void IsHullDestroyedCheck()
        {
            if (Hull == 0)
            {
                DestroyShip();
            }
        }

        public void DestroyShip()
        {
            if (!IsDestroyed)
            {
                Game.UI.AddTestLogEntry(PilotName + "\'s ship is destroyed");
                Roster.DestroyShip(this.GetTag());
                OnDestroyed();
                IsDestroyed = true;
            }
        }

        public List<CriticalHitCard.GenericCriticalHit> GetAssignedCritCards()
        {
            return AssignedCritCards;
        }

        // ATTACK TYPES

        //Todo: Rework

        public int GetAttackTypes(int distance, bool inArc)
        {
            int result = 0;

            if (InPrimaryWeaponFireZone(distance, inArc)) result++;

            foreach (var upgrade in InstalledUpgrades)
            {
                if (upgrade.Value.Type == Upgrade.UpgradeSlot.Torpedoes)
                {
                    if ((upgrade.Value as Upgrade.GenericSecondaryWeapon).IsShotAvailable(Selection.AnotherShip)) result++;
                }
            }

            return result;
        }

        public bool InPrimaryWeaponFireZone(GenericShip anotherShip)
        {
            bool result = true;
            int distance = Actions.GetFiringRange(this, anotherShip);
            bool inArc = Actions.InArcCheck(this, anotherShip);
            result = InPrimaryWeaponFireZone(distance, inArc);
            return result;
        }

        public bool InPrimaryWeaponFireZone(int distance, bool inArc)
        {
            bool result = true;
            if (distance > 3) return false;
            if (!inArc) return false;
            return result;
        }

    }

}
