using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using EnsoulSharp.SDK.Utility;
using Menu = EnsoulSharp.SDK.MenuUI.Menu;

namespace Stigma.Maokai
{
    class Program
    {
        public static Menu Config { get; set; }

        private static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        private static Spell Q, W, E, R;
        private static SpellSlot Ignite;
        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += GameEventOnOnGameLoad;
        }

        private static void GameEventOnOnGameLoad()
        {
            if (Player.CharacterName != "Maokai") return;
            CreateSpells();
            CreateMenu();
            CreateEvents();
            //Chat.Print(" Stigma: Maokai 9.19 #Arvore Braba V1.1.0.1");

        }

        //-----------------------------------------------------------------------------------
        private static void CreateSpells()
        {
            Q = new Spell(SpellSlot.Q, 600f);            
            W = new Spell(SpellSlot.W, 525f);
            E = new Spell(SpellSlot.E, 1100f);
            R = new Spell(SpellSlot.R, 3000f);
            Ignite = Player.GetSpellSlot("summonerdot");
            W.SetTargetted(0.5f, 525f);
            E.SetSkillshot(0.5f, 1100f, float.MaxValue, false, false, SkillshotType.Circle);
            Q.SetSkillshot(0.5f, 50f, 600f, true, false, SkillshotType.Line);
            R.SetSkillshot(0.25f, 1000F, 3000f, true, false, SkillshotType.Line);

        }
        //-------------------------------------------------------------------------------------
        public static bool CombouseQ { get { return MeuUI.GetMenuBoolValue(Config, "Combo", "CombouseQ"); } }
        public static bool CombouseW { get { return MeuUI.GetMenuBoolValue(Config, "Combo", "CombouseW"); } }
        public static bool CombouseE { get { return MeuUI.GetMenuBoolValue(Config, "Combo", "CombouseE"); } }
        public static bool CombouseR { get { return MeuUI.GetMenuBoolValue(Config, "Combo", "CombouseR"); } }

        public static bool HarassUseE { get { return MeuUI.GetMenuBoolValue(Config, "Harass", "HarassUseE"); } }
        public static bool HarassUseQ { get { return MeuUI.GetMenuBoolValue(Config, "Harass", "HarassUseQ"); } }

        public static bool LaneClearUseQ { get { return MeuUI.GetMenuBoolValue(Config, "LaneClear", "LaneClearUseQ"); } }
        public static bool LaneClearUseE { get { return MeuUI.GetMenuBoolValue(Config, "LaneClear", "LaneClearUseE"); } }

        public static bool LastHitUseQ { get { return MeuUI.GetMenuBoolValue(Config, "LastHit", "LastHitUseQ"); } }

        public static bool MiscUseIgnite { get { return MeuUI.GetMenuBoolValue(Config, "Misc", "MiscUseIgnite"); } }

        //---------------------------------------------------------------------------------------

        private static void CreateEvents()
        {
            Game.OnUpdate += GameOnOnUpdate;
            Drawing.OnDraw += DrawingOnOnDraw;
            Interrupter.OnInterrupterSpell += InterrupterOnOnInterrupterSpell;
        }

        private static void InterrupterOnOnInterrupterSpell(AIHeroClient sender, Interrupter.InterruptSpellArgs args)
        {
            if (sender.IsEnemy && sender.Distance(Player) < Q.Range)
             
            {
                Q.CastOnUnit(sender);
            }

            if (sender.IsEnemy && sender.Distance(Player) < W.Range)

            {
                W.CastOnUnit(sender);
            }
        }


        private static void DrawingOnOnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Q.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Green);
            }
            if (W.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.Green);
            }
            if (E.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, System.Drawing.Color.Crimson);
            }
            if (R.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Crimson);
            }


        }

        private static void GameOnOnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                Combo();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                Harass();
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                LaneClear();
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
            {
                LastHit();
            }

            
            KillSteal();
        }

        private static void LastHit()
        {
            if (LastHitUseQ && Q.IsReady())
            {
                var minion = GameObjects.EnemyMinions.FirstOrDefault(x =>
                    x.IsValidTarget(Q.Range) && x.IsEnemy && Player.GetSpellDamage(x, SpellSlot.Q) > x.Health);
                if (minion != null)
                {
                    Q.CastOnUnit(minion);
                }
            }
        }

        private static void LaneClear()
        {
            if (LaneClearUseQ && Q.IsReady() && Player.CountEnemyHeroesInRange(600) == 0)
            {
                var rMinions = GameObjects.Minions.Where(x => x.IsValidTarget(Q.Range) && x.IsEnemy).ToList();
                if (rMinions.Any())
                {
                    var minionLocation = Q.GetCircularFarmLocation(rMinions);
                    Q.Cast(minionLocation.Position);
                }
            }

            if (LaneClearUseE && E.IsReady() && Player.CountEnemyHeroesInRange(1100) == 0)
            {
                var rMinions = GameObjects.Minions.Where(x => x.IsValidTarget(E.Range) && x.IsEnemy).ToList();
                if (rMinions.Any())
                {
                    var minionLocation = E.GetCircularFarmLocation(rMinions);
                    E.Cast(minionLocation.Position);
                }
            }
        }

        private static void KillSteal()
        {
            if (MiscUseIgnite && Ignite.IsReady())
            {
                var target = TargetSelector.GetTarget(680, DamageType.True);
                if (target != null && CastIgnite(target))
                {
                    return;
                }
            }
        }
        public static bool CastIgnite(AIHeroClient target)
        {
            return Ignite.IsReady() && target.IsValidTarget(600)
                   && target.Health + 5 < Player.GetSummonerSpellDamage(target, SummonerSpell.Ignite)
                   && Player.Spellbook.CastSpell(Ignite, target);
        }
       
        private static void Harass()
        {
                if (E.IsReady() && HarassUseE)
                {
                    var Etarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                    if (Etarget != null)
                    {
                        return;
                    }
                }
            if (Q.IsReady() && HarassUseQ)
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (Qtarget != null)
                {
                    Q.CastIfHitchanceEquals(Qtarget, HitChance.VeryHigh);
                    return;
                }
            }

        }

        private static void Combo()
        {

            if (E.IsReady() && CombouseE)
            {
                var Etarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);
                if (Etarget != null)
                {
                    E.CastIfHitchanceEquals(Etarget, HitChance.VeryHigh);
                    return;
                }
            }
            if (W.IsReady() && CombouseW)
            {
                var Wtarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
                if (Wtarget != null)
                {
                    W.CastIfHitchanceEquals(Wtarget, HitChance.VeryHigh);
                    return;
                }
            }
            if (Q.IsReady() && CombouseQ)
            {
                var Qtarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (Qtarget != null)
                {
                    Q.CastIfHitchanceEquals(Qtarget, HitChance.VeryHigh);
                    return;
                }
            }
            if (R.IsReady() && CombouseR)
            {
                var Rtarget = TargetSelector.GetTarget(R.Range, DamageType.Magical);
                if (Rtarget != null)
                {
                    R.CastIfHitchanceEquals(Rtarget, HitChance.VeryHigh);
                    return;
                }
            }
        }
            private static void CreateMenu()
        {
            Config = new Menu("Stigma.Maokai", "Stigma.Maokai", true);

            //Combo Menu
            var comboMenu = new Menu("Combo", "Combo");
            MeuUI.AddMenuBool(comboMenu, "CombouseQ", "Use Q");
            MeuUI.AddMenuBool(comboMenu, "CombouseW", "Use W");
            MeuUI.AddMenuBool(comboMenu, "CombouseE", "Use E");
            MeuUI.AddMenuBool(comboMenu, "CombouseR", "Use R");
            Config.Add(comboMenu);

            //Harass Menu
            var harassMenu = new Menu("Harass", "Harass");
            MeuUI.AddMenuBool(harassMenu, "HarassUseQ", "Use Q");
            MeuUI.AddMenuBool(harassMenu, "HarassUseE", "Use E");
            Config.Add(harassMenu);

            //Lane Clear Menu
            var lClearMenu = new Menu("LaneClear", "LaneClear");
            MeuUI.AddMenuBool(lClearMenu, "LaneClearUseQ", "Use Q");
            MeuUI.AddMenuBool(lClearMenu, "LaneClearUseE", "Use E");

            Config.Add(lClearMenu);

            //Last Hit
            var lastHitMenu = new Menu("LastHit", "LastHit");
            MeuUI.AddMenuBool(lastHitMenu, "LastHitUseQ", "Use Q");
            Config.Add(lastHitMenu);

            //Misc Menu
            var miscMenu = new Menu("Misc", "Misc");
            MeuUI.AddMenuBool(miscMenu, "MiscUseIgnite", "KillSteal with Ignite.");
            Config.Add(miscMenu);
            Config.Attach();

        }            

    }
}
