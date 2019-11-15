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

namespace Stigma.Mundo
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
            if (Player.CharacterName != "DrMundo") return;
            CreateSpells();
            CreateMenu();
            CreateEvents();
            //Chat.Print(" Stigma: Cutelada nas costa dos cara #1");

        }

        //-----------------------------------------------------------------------------------
        private static void CreateSpells()
        {
            Q = new Spell(SpellSlot.Q, 1000f);            
            W = new Spell(SpellSlot.W, 162f);

            Ignite = Player.GetSpellSlot("summonerdot");
            Q.SetSkillshot(0.5f, 50f, 1000f, true, false, SkillshotType.Line);
            W.SetTargetted(0.5f, 162f);
          //  E.SetSkillshot();
          //  R.SetSkillshot(0.25f, 1000F, 3000f, true, false, SkillshotType.Line);

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
            
        }
            private static void CreateMenu()
        {
            Config = new Menu("Stigma.Mundo", "Stigma.Mundo", true);

            //Combo Menu
            var comboMenu = new Menu("Combo", "Combo");
            MeuUI.AddMenuBool(comboMenu, "CombouseQ", "Use Q");
            MeuUI.AddMenuBool(comboMenu, "CombouseW", "Use W");

            Config.Add(comboMenu);

            //Harass Menu
            var harassMenu = new Menu("Harass", "Harass");
            MeuUI.AddMenuBool(harassMenu, "HarassUseQ", "Use Q");
            Config.Add(harassMenu);

            //Lane Clear Menu
            var lClearMenu = new Menu("LaneClear", "LaneClear");
            MeuUI.AddMenuBool(lClearMenu, "LaneClearUseQ", "Use Q");

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
