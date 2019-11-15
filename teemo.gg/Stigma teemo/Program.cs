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

namespace Stigma.Teemo
{
    class Program
    {
        public static Menu Config { get; set; }

        private static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }

        private static Spell Q;
        private static readonly Spell W;
        private static Spell R;
        private static SpellSlot Ignite;
        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += GameEventOnOnGameLoad;
        }

        private static void GameEventOnOnGameLoad()
        {
            if (Player.CharacterName != "Teemo") return;
            CreateSpells();
            CreateMenu();
            CreateEvents();
            //Chat.Print(" Stigma: Teemo 9.19 #Bug R Fixed FARM");

        }

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
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Q.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Green);
            }
               if (R.IsReady())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.Green);
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
            if (FleeKey)
            {
                Flee();
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
            if (LaneClearUseQ && Q.IsReady())
            {
                var qMinions = GameObjects.Minions.Where(x => x.IsValidTarget(Q.Range) && !x.IsAlly);
                if (qMinions.Any())
                {
                    Q.CastOnUnit(qMinions.FirstOrDefault());
                }
            }
           
           
            if (LaneClearUseR && R.IsReady())
            {
                var rMinions = GameObjects.Minions.Where(x => x.IsValidTarget(R.Range) && !x.IsAlly);
                if (rMinions.Any())
                {
                    R.CastOnUnit(rMinions.FirstOrDefault());
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
        private static void Flee()
        {
            if (!FleeUseW)
                return;

            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

            if (W.IsReady())
            {
                W.Cast(Game.CursorPos);
            }
        }

        private static void Harass()
        {
            if (Q.IsReady() && HarassUseQ)
            {
                AIHeroClient target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

                if (target != null)
                {
                    if (target.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(target);
                    }
                }
            }

                       
        }

        private static void Combo()
        {
            if (Q.IsReady() && CombouseQ)
            {
                AIHeroClient target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (target != null)
                {
                    if (target.IsValidTarget(Q.Range))
                    {
                        Q.CastOnUnit(target);
                    }
                } 
            }

                      
            if (R.IsReady() && CombouseR)
            {
                AIHeroClient target = TargetSelector.GetTarget(R.Range, DamageType.Magical);

                var extraEnemies =
                    ObjectManager.Get<AIHeroClient>().Count(x => x != target && x.IsEnemy && x.Distance(target) < 800);

                if (target != null)
                {
                    if (target.IsValidTarget(R.Range) && extraEnemies < ComboDontR)
                    {
                        R.Cast(target.Position);
                    }
                }
            }
        }

        private static void CreateMenu()
        {
            Config = new Menu("Stigma.Teemo", "Stigma.Teemo", true);

            //Combo Menu
            var comboMenu = new Menu("Combo", "Combo");
            MeuUI.AddMenuBool(comboMenu, "CombouseQ", "Use Q");
            MeuUI.AddMenuBool(comboMenu, "CombouseR", "Use R");
            MeuUI.AddMenuSlider(comboMenu, "ComboDontR", "Don't R if >= X Enemies", 3, 1, 5);
            Config.Add(comboMenu);

            //Harass Menu
            var harassMenu = new Menu("Harass", "Harass");
            MeuUI.AddMenuBool(harassMenu, "HarassUseQ", "Use Q");
            Config.Add(harassMenu);

            //Lane Clear Menu
            var lClearMenu = new Menu("LaneClear", "LaneClear");
            MeuUI.AddMenuBool(lClearMenu, "LaneClearUseQ", "Use Q");
            MeuUI.AddMenuBool(lClearMenu, "LaneClearUseR", "Use R");

            Config.Add(lClearMenu);

            //Last Hit
            var lastHitMenu = new Menu("LastHit", "LastHit");
            MeuUI.AddMenuBool(lastHitMenu, "LastHitUseQ", "Use Q");
            Config.Add(lastHitMenu);

            //Flee Menu
            var fleeMenu = new Menu("Flee", "Flee");
            MeuUI.AddMenuBool(fleeMenu, "FleeUseW", "Use W");
            MeuUI.AddMenuKeyBind(fleeMenu, "FleeKey", "Flee !", Keys.Z, KeyBindType.Press);
            Config.Add(fleeMenu);

            //Misc Menu
            var miscMenu = new Menu("Misc", "Misc");
            MeuUI.AddMenuBool(miscMenu, "MiscUseIgnite", "KillSteal with Ignite.");
            Config.Add(miscMenu);
            Config.Attach();

        }

        private static void CreateSpells()
        {
            Q = new Spell(SpellSlot.Q, 650f);
            R = new Spell(SpellSlot.R, 550f);
            Ignite = Player.GetSpellSlot("summonerdot");
            Q.SetTargetted(0.5f, 1400f);
            R.SetSkillshot(0.5f, 150f, float.MaxValue, false, false, SkillshotType.Circle);
        }

        public static bool CombouseQ { get { return MeuUI.GetMenuBoolValue(Config, "Combo", "CombouseQ"); } }
        public static bool CombouseR { get { return MeuUI.GetMenuBoolValue(Config, "Combo", "CombouseR"); } }
        public static int CombouseW { get { return MeuUI.GetMenuSliderValue(Config, "Combo", "CombouseW"); } }
        public static int ComboDontR { get { return MeuUI.GetMenuSliderValue(Config, "Combo", "ComboDontR"); } }
        public static bool HarassUseQ { get { return MeuUI.GetMenuBoolValue(Config, "Harass", "HarassUseQ"); } }
        public static bool LaneClearUseQ { get { return MeuUI.GetMenuBoolValue(Config, "LaneClear", "LaneClearUseQ"); } }
        public static bool LaneClearUseR { get { return MeuUI.GetMenuBoolValue(Config, "LaneClear", "LaneClearUseR"); } }

        public static bool LastHitUseQ { get { return MeuUI.GetMenuBoolValue(Config, "LastHit", "LastHitUseQ"); } }
        public static bool FleeUseW { get { return MeuUI.GetMenuBoolValue(Config, "Flee", "FleeUseW"); } }
        public static bool FleeKey { get { return MeuUI.GetMenuKeyBindValue(Config, "Flee", "FleeKey"); } }
        public static bool MiscUseIgnite { get { return MeuUI.GetMenuBoolValue(Config, "Misc", "MiscUseIgnite"); } }

    }
}
