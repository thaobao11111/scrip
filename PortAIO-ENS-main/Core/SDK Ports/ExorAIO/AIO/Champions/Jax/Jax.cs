﻿
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.Utility;
using HERMES_Kalista.MyLogic.Others;

#pragma warning disable 1587

namespace ExorAIO.Champions.Jax
{
    using System;

    using ExorAIO.Utilities;


    /// <summary>
    ///     The champion class.
    /// </summary>
    internal class Jax
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Called on do-cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        public static void OnSpellCast(AIBaseClient sender, AIBaseClientProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && Orbwalker.IsAutoAttack(args.SData.Name))
            {
                /// <summary>
                ///     Initializes the orbwalkingmodes.
                /// </summary>
                switch (Orbwalker.ActiveMode)
                {
                    case OrbwalkerMode.Combo:
                        Logics.Weaving(sender, args);
                        break;
                    case OrbwalkerMode.LaneClear:
                        Logics.Clear(sender, args);
                        Logics.BuildingClear(sender, args);
                        break;
                }
            }
        }

        /// <summary>
        ///     Fired on an incoming gapcloser.
        /// </summary>
        /// <param name="sender">The object.</param>
        /// <param name="args">The <see cref="Events.GapCloserEventArgs" /> instance containing the event data.</param>
        public static void OnGapCloser(AIHeroClient sender, AntiGapcloser.GapcloserArgs args)
        {
            if (GameObjects.Player.IsDead || Invulnerable.Check(sender, DamageType.Magical, false))
            {
                return;
            }

            if (Vars.E.IsReady()  && sender.IsValidTarget(Vars.Q.Range)
                && Vars.Menu["spells"]["e"]["gapcloser"].GetValue<MenuBool>().Enabled)
            {
                Vars.E.Cast();
            }
        }

        /// <summary>
        ///     Fired when the game is updated.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        public static void OnUpdate(EventArgs args)
        {
            if (GameObjects.Player.IsDead)
            {
                return;
            }

            /// <summary>
            ///     Initializes the Automatic actions.
            /// </summary>
            Logics.Automatic(args);

            /// <summary>
            ///     Initializes the Killsteal events.
            /// </summary>
            Logics.Killsteal(args);
            if (GameObjects.Player.Spellbook.IsAutoAttack)
            {
                return;
            }

            /// <summary>
            ///     Initializes the orbwalkingmodes.
            /// </summary>
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Logics.Combo(args);
                    break;
                case OrbwalkerMode.Harass:
                    Logics.Harass(args);
                    break;
                case OrbwalkerMode.LaneClear:
                    Logics.Clear(args);
                    break;
            }
        }

        /// <summary>
        ///     Loads Jax.
        /// </summary>
        public void OnLoad()
        {
            /// <summary>
            ///     Initializes the menus.
            /// </summary>
            Menus.Initialize();

            /// <summary>
            ///     Initializes the spells.
            /// </summary>
            Spells.Initialize();

            /// <summary>
            ///     Initializes the methods.
            /// </summary>
            Methods.Initialize();

            /// <summary>
            ///     Initializes the drawings.
            /// </summary>
            Drawings.Initialize();
        }

        #endregion
    }
}