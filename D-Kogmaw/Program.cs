using System;
using System.Drawing;
using System.IO;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;

namespace D_Kogmaw
{
    public static class Program
    {
        private const string ChampionName = "KogMaw";


        private static AIHeroClient _player
        {
            get { return ObjectManager.Player; }
        }

        public static AttackableUnit AfterAttackTarget { get; private set; }
        private static Spell.Skillshot _q;
        private static Spell.Active _w;
        private static Spell.Skillshot _e;
        private static Spell.Skillshot _r;

        private static Menu _miscmenu, _drawmenu, _combo, _config, _harass, _farmmenu, _junglemenu;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            //TargetSelector.Init();
            Bootstrap.Init(null);

            if (Player.Instance.ChampionName != "KogMaw")
                return;

            _q = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 500, 1200, 70);
            _q.AllowedCollisionCount = 0x0;
            _w = new Spell.Active(SpellSlot.W, (uint) (760 + 20*_player.Spellbook.GetSpell(SpellSlot.W).Level));
            _e = new Spell.Skillshot(SpellSlot.E, 1300, SkillShotType.Linear, 500, 1200, 120);
            _r = new Spell.Skillshot(SpellSlot.R, (uint) (800 + 300*_player.Spellbook.GetSpell(SpellSlot.R).Level),
                SkillShotType.Circular, 1200, Int32.MaxValue, 120);

            //D kogmaw
            _config = MainMenu.AddMenu("D-KogMaw", "D-KogMaw");
            _config.AddGroupLabel("D-KogMow");
            _config.AddLabel("Made by Diabaths");
            _config.AddSeparator();

            //Combo
            _combo = _config.AddSubMenu("Combo", "Combo");
            _combo.AddLabel("Combo Mode Settings");
            _combo.Add("UseQC", new CheckBox("Use Q"));
            _combo.Add("UseWC", new CheckBox("Use W"));
            _combo.Add("UseEC", new CheckBox("Use E"));
            _combo.Add("UseRC", new CheckBox("Use R"));
            _combo.Add("RlimC", new Slider("R Max Stuck", 3, 1, 5));
            _combo.AddSeparator();

            //Harass
            _harass = _config.AddSubMenu("Harass", "Harass");
            _harass.AddLabel("Harass Mode Settings");
            _harass.Add("UseQH", new CheckBox("Use Q"));
            _harass.Add("UseWH", new CheckBox("Use W"));
            _harass.Add("UseEH", new CheckBox("Use E"));
            _harass.AddSeparator();
            _harass.Add("UseRH", new CheckBox("Use R"));
            _harass.Add("RlimH", new Slider("R Max Stuck", 1, 1, 5));
            _harass.Add("Harrasmana", new Slider("Minimum Mana", 70, 1, 100));
            _harass.Add("harasstoggle", new KeyBind("Harass Toggle", false, KeyBind.BindTypes.PressToggle, 'U'));
            _harass.AddSeparator();
            //Jungle Clear
            _junglemenu = _config.AddSubMenu("JungleClear", "JungleClear");
            _junglemenu.AddLabel("Jungle Clear Settings");
            _junglemenu.Add("UseQJ", new CheckBox("Use Q"));
            _junglemenu.Add("UseWJ", new CheckBox("Use W"));
            _junglemenu.Add("UseEJ", new CheckBox("Use E"));
            _junglemenu.Add("UseRJ", new CheckBox("Use R"));
            _junglemenu.Add("RlimJ", new Slider("R Max Stuck", 1, 1, 5));
            _junglemenu.Add("junglemana", new Slider("Minimum Mana", 35, 1, 100));
            _junglemenu.AddSeparator();
            //LaneClear
            _farmmenu = _config.AddSubMenu("FarmLane", "FarmLane");
            _farmmenu.AddLabel("Lane Clear Settings");
            _farmmenu.Add("UseQL", new CheckBox("Use Q"));
            _farmmenu.Add("UseWL", new CheckBox("Use W"));
            _farmmenu.Add("UseEL", new CheckBox("Use E"));
            _farmmenu.Add("UseRL", new CheckBox("Use R"));
            _farmmenu.Add("RlimL", new Slider("R Max Stuck", 1, 1, 5));
            _farmmenu.Add("Lanemana", new Slider("Minimum Mana", 35, 1, 100));
            _farmmenu.AddSeparator();

            //Misc
            _miscmenu = _config.AddSubMenu("Misc", "Misc");
            _miscmenu.Add("UseRM", new CheckBox("Use R KillSteal"));
            //_miscmenu.Add("Gap_E", new CheckBox("GapClosers E"));
            _miscmenu.AddSeparator();

            //Drawings
            _drawmenu = _config.AddSubMenu("Drawings", "Drawings");
            _drawmenu.AddLabel("Draw Settings");
            _drawmenu.Add("DrawQ", new CheckBox("Draw Q"));
            _drawmenu.Add("DrawW", new CheckBox("Draw W"));
            _drawmenu.Add("DrawE", new CheckBox("Draw E"));
            _drawmenu.Add("DrawR", new CheckBox("Draw R"));
            _drawmenu.Add("Drawharass", new CheckBox("Draw AutoHarass"));

            Chat.Print("<font color='#881df2'>D-KogMaw by Diabaths</font> Loaded.");
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Chat.Print(
                "<font color='#f2f21d'>If You like my work and want to support me,  plz donate via paypal in </font> <font color='#00e6ff'>ssssssssssmith@hotmail.com</font> (10) S");
        }

        private static void Game_OnTick(EventArgs args)
        {

            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)

            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass)
            {
                ;
                var useW = _harass["UseWH"].Cast<CheckBox>().CurrentValue;
                var eTarget = TargetSelector.GetTarget(_e.Range, DamageType.Physical);
                if (useW && _w.IsReady() && eTarget.IsValidTarget(_e.Range))
                {
                    _w.Cast();
                }
                if (_player.ManaPercent >
                    _harass["Harrasmana"].Cast<Slider>().CurrentValue)
                {
                    Harass();
                }
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
            {
                var minion =
                    (EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, _player.Position.To2D(), 1000f));

                var useW = _farmmenu["UseWL"].Cast<CheckBox>().CurrentValue;
                if (minion != null)
                {
                    if (useW && _w.IsReady())
                    {
                        _w.Cast();
                    }
                }
                if (_player.ManaPercent > _farmmenu["Lanemana"].Cast<Slider>().CurrentValue)
                {
                    Laneclear();

                }
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.JungleClear)
            {
                var minion = (EntityManager.GetJungleMonsters(_player.Position.To2D(), 1000f));

                var useW = _junglemenu["UseWJ"].Cast<CheckBox>().CurrentValue;
                if (minion != null)
                {
                    if (useW && _w.IsReady())
                    {
                        _w.Cast();
                    }
                }

                if (_player.ManaPercent > _junglemenu["junglemana"].Cast<Slider>().CurrentValue)
                {
                    JungleClear();

                }
            }
            KillSteal();
        }

        private static void Combo()
        {
            var useQ = _combo["UseQC"].Cast<CheckBox>().CurrentValue;
            var useW = _combo["UseWC"].Cast<CheckBox>().CurrentValue;
            var useE = _combo["UseEC"].Cast<CheckBox>().CurrentValue;
            var useR = _combo["UseRC"].Cast<CheckBox>().CurrentValue;
            var rLim = _combo["RlimC"].Cast<Slider>().CurrentValue;

            if (useW)
            {
                var tw = TargetSelector.GetTarget(_e.Range, DamageType.Magical);
                if (tw.IsValidTarget(_e.Range) && _w.IsReady())
                {
                    _w.Cast();
                }
            }
            if (useQ && _q.IsReady())
            {
                var t = TargetSelector.GetTarget(_q.Range, DamageType.Magical);
                var prediction = _q.GetPrediction(t);
                if (t.IsValidTarget(_q.Range) && prediction.HitChance >= HitChance.High)
                    _q.Cast(t);
            }
            if (useE && _e.IsReady())
            {
                var t = TargetSelector.GetTarget(_e.Range, DamageType.Magical);
                var predictione = _e.GetPrediction(t);
                if (t.IsValidTarget(_e.Range) && predictione.HitChance >= HitChance.High)
                    _e.Cast(t);
            }
            if (useR && _r.IsReady() && GetBuffStacks() < rLim)
            {
                var t = TargetSelector.GetTarget(_r.Range, DamageType.Magical);
                var predictionr = _r.GetPrediction(t);
                if (t.IsValidTarget(_r.Range) && predictionr.HitChance >= HitChance.High)
                    _r.Cast(t);
            }
        }


        private static void Harass()
        {
            var useQ = _harass["UseQH"].Cast<CheckBox>().CurrentValue;
            var useE = _harass["UseEH"].Cast<CheckBox>().CurrentValue;
            var useR = _harass["UseRH"].Cast<CheckBox>().CurrentValue;
            var rLimH = _harass["RlimH"].Cast<Slider>().CurrentValue;

            if (useQ && _q.IsReady())
            {
                var t = TargetSelector.GetTarget(_q.Range, DamageType.Magical);
                if (t.IsValidTarget(_q.Range) && _q.GetPrediction(t).HitChance >= HitChance.High)
                    _q.Cast(t);
            }

            if (useE && _e.IsReady())
            {
                var t = TargetSelector.GetTarget(_e.Range, DamageType.Magical);
                if (t.IsValidTarget(_e.Range) && _e.GetPrediction(t).HitChance >= HitChance.High)
                    _e.Cast(t);
            }

            if (useR && _r.IsReady() && GetBuffStacks() < rLimH)
            {
                var t = TargetSelector.GetTarget(_r.Range, DamageType.Magical);
                if (t.IsValidTarget(_r.Range) && _r.GetPrediction(t).HitChance >= HitChance.High)
                    _r.Cast(t);
            }
        }

        private static void Laneclear()
        {
            var minion = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, _player.Position.To2D(),
                _e.Range);
            var useQ = _farmmenu["UseQL"].Cast<CheckBox>().CurrentValue;
            var useE = _farmmenu["UseEL"].Cast<CheckBox>().CurrentValue;
            var useR = _farmmenu["UseRL"].Cast<CheckBox>().CurrentValue;
            var rLimL = _farmmenu["RlimL"].Cast<Slider>().CurrentValue;

            foreach (var mobs in minion)
            {
                if (_q.IsReady() && useQ)
                {
                    if (minion.Count >= 3)
                    {
                        _q.Cast(mobs);
                    }

                    else if (mobs.Distance(_player) > _player.GetAutoAttackRange(mobs) &&
                             mobs.Health < 0.75*_player.GetSpellDamage(mobs, SpellSlot.Q))
                        _q.Cast(mobs);
                }
                if (_e.IsReady() && useE)
                {
                    if (minion.Count >= 3)
                    {
                        _e.Cast(mobs);
                    }
                    else if (mobs.Distance(_player) > _player.GetAutoAttackRange(mobs) &&
                             mobs.Health < 0.75*_player.GetSpellDamage(mobs, SpellSlot.E))
                        _e.Cast(mobs);
                }
                if (_r.IsReady() && useR && GetBuffStacks() < rLimL)
                {


                    if (minion.Count >= 3)
                    {
                        _r.Cast(mobs);
                    }

                    else if (mobs.Distance(_player) > _player.GetAutoAttackRange(mobs) &&
                             mobs.Health < 0.75*_player.GetSpellDamage(mobs, SpellSlot.R))
                        _r.Cast(mobs);
                }
            }
        }


        private static void JungleClear()
        {
            var mininions = EntityManager.GetJungleMonsters(_player.Position.To2D(), _e.Range);
            var useQ = _junglemenu["UseQJ"].Cast<CheckBox>().CurrentValue;
            var useE = _junglemenu["UseEJ"].Cast<CheckBox>().CurrentValue;
            var useR = _junglemenu["UseRJ"].Cast<CheckBox>().CurrentValue;
            var rLimJ = _junglemenu["RlimJ"].Cast<Slider>().CurrentValue;

            foreach (var minion in mininions.Where(m => m.IsValid))
            {

                if (useQ && _q.IsReady() && minion.IsValidTarget(_q.Range))
                {
                    _q.Cast(minion);
                }
                if (_e.IsReady() && useE && minion.IsValidTarget(_e.Range))
                {
                    _e.Cast(minion);
                }
                if (_r.IsReady() && useR && GetBuffStacks() < rLimJ && minion.IsValidTarget(_r.Range))
                {
                    _r.Cast(minion);
                }

            }
        }

        private static int GetBuffStacks()
        {
            if (_player.HasBuff("KogMawLivingArtillery"))
            {
                return _player.Buffs
                    .Where(x => x.DisplayName == "KogMawLivingArtillery")
                    .Select(x => x.Count)
                    .First();
            }
            else
            {
                return 0;
            }
        }

        private static void KillSteal()
        {
            foreach (
                var target in
                    HeroManager.Enemies.Where(hero => hero.IsValidTarget(_r.Range) && !hero.IsDead && !hero.IsZombie))
            {
                if (_miscmenu["UseRM"].Cast<CheckBox>().CurrentValue && _r.IsReady() && target.Health < RDamage(target))
                {
                    _r.Cast(target);
                }
            }
        }

        private static float RDamage(Obj_AI_Base target)
        {
            return _player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)
                    (new[] {80, 120, 160}[Program._r.Level] + 1.3*_player.FlatMagicDamageMod +
                     1.5*_player.FlatPhysicalDamageMod));
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var harass = _harass["harasstoggle"].Cast<KeyBind>().CurrentValue;

            if (_drawmenu["Drawharass"].Cast<CheckBox>().CurrentValue)
            {
                if (harass)
                {
                    Drawing.DrawText(Drawing.Width*0.02f, Drawing.Height*0.92f, Color.GreenYellow,
                        "Auto harass Enabled");
                }
                else
                    Drawing.DrawText(Drawing.Width*0.02f, Drawing.Height*0.92f, Color.OrangeRed,
                        "Auto harass Disabled");
            }

            if (_drawmenu["DrawQ"].Cast<CheckBox>().CurrentValue && _q.Level > 0)
            {
                new Circle() {Color = Color.GreenYellow, BorderWidth = 1, Radius = _q.Range}.Draw(_player.Position);
            }
            if (_drawmenu["DrawW"].Cast<CheckBox>().CurrentValue && _w.Level > 0)
            {
                new Circle()
                {
                    Color = Color.GreenYellow,
                    BorderWidth = 1,
                    Radius = (uint) (760 + 20*_player.Spellbook.GetSpell(SpellSlot.W).Level)
                }.Draw(_player.Position);
            }
            if (_drawmenu["DrawE"].Cast<CheckBox>().CurrentValue && _e.Level > 0)
            {
                new Circle() {Color = Color.GreenYellow, BorderWidth = 1, Radius = _e.Range}.Draw(_player.Position);
            }
            if (_drawmenu["DrawR"].Cast<CheckBox>().CurrentValue && _r.Level > 0)
            {
                new Circle()
                {
                    Color = Color.GreenYellow,
                    BorderWidth = 1,
                    Radius = (uint) (800 + 300*_player.Spellbook.GetSpell(SpellSlot.R).Level)
                }.Draw(_player.Position);
            }
        }
    }
}


