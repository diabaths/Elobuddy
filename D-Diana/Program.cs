using System;
using System.IO;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace D_Diana
{
    internal static class Program
    {
        private const string ChampionName = "Diana";

        private static AIHeroClient _player
        {
            get { return ObjectManager.Player; }
        }

        private static Obj_SpellMissile _qpos;

        private static int _lastTick;

        private static bool _qcreated = false;

        private static Menu _config, _combo, _harass, _farmmenu, _junglemenu, _miscmenu, _drawmenu, _smitemenu;

        private static Spell.Skillshot _q;
        private static Spell.Active _w;
        private static Spell.Active _e;
        private static Spell.Targeted _r;

        private static readonly Spell.Targeted Ignite =
            new Spell.Targeted(_player.GetSpellSlotFromName("summonerdot"), 600);

        private static SpellSlot _smiteSlot = SpellSlot.Unknown;

        private static Spell.Targeted _smite;
       


        //Credits to Kurisu
        private static readonly int[] SmitePurple = {3713, 3726, 3725, 3724, 3723, 3933};
        private static readonly int[] SmiteGrey = {3711, 3722, 3721, 3720, 3719, 3932};
        private static readonly int[] SmiteRed = {3715, 3718, 3717, 3716, 3714, 3931};
        private static readonly int[] SmiteBlue = {3706, 3710, 3709, 3708, 3707, 3930};

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);
            if (Player.Instance.ChampionName != ChampionName)
                return;
            _q = new Spell.Skillshot(SpellSlot.Q, 830, SkillShotType.Linear, 350, 1800, 190);
            _q.AllowedCollisionCount = int.MaxValue;
            _w = new Spell.Active(SpellSlot.W, 250);
            _e = new Spell.Active(SpellSlot.E, 450);
            _r = new Spell.Targeted(SpellSlot.R, 825);
            SetSmiteSlot();

            //D Diana
            _config = MainMenu.AddMenu("D-Diana", "D-Diana");
            _config.AddGroupLabel("D-Diana");
            _config.AddLabel("Made by Diabaths");

            _config.AddSeparator();
            //combo
            _combo = _config.AddSubMenu("Combo", "Combo");
            _combo.AddLabel("Combo Mode Settings");
            var comboCardChooserSlider = _combo.Add("ComboPrio", new Slider("Select Combo", 0, 0, 1));
            var comboCardArray = new[] {"Q-R", "R-Q"};
            comboCardChooserSlider.DisplayName = comboCardArray[comboCardChooserSlider.CurrentValue];
            comboCardChooserSlider.OnValueChange +=
                delegate(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs changeArgs)
                {
                    sender.DisplayName = comboCardArray[changeArgs.NewValue];
                };
            _combo.AddSeparator();
            _combo.Add("UseIgnitecombo", new CheckBox("Use Ignite"));
            _combo.Add("smitecombo", new CheckBox("Use Smite in target"));
            _combo.Add("UseQCombo", new CheckBox("Use Q"));
            _combo.Add("UseWCombo", new CheckBox("Use W"));
            _combo.Add("UseECombo", new CheckBox("Use E"));
            _combo.Add("UseRCombo", new CheckBox("Use R"));
            _combo.Add("UseRSecond", new CheckBox("Use Second R"));
            _combo.AddSeparator();

            _harass = _config.AddSubMenu("Harass", "Harass");
            _harass.Add("UseQHarass", new CheckBox("Use Q"));
            _harass.Add("UseWHarass", new CheckBox("Use W"));
            _harass.Add("UseRHarass", new CheckBox("Use R"));
            _harass.Add("Harrasmana", new Slider("Minimum Mana", 70, 1, 100));
            _harass.Add("harasstoggle", new KeyBind("Harass Toggle", false, KeyBind.BindTypes.PressToggle, 'U'));
            _harass.AddSeparator();

            _farmmenu = _config.AddSubMenu("Lane Clear", "Lane Clear");
            _farmmenu.AddLabel("Lane Clear Settings");
            _farmmenu.Add("UseQLane", new CheckBox("Use Q"));
            _farmmenu.Add("UseWLane", new CheckBox("Use W"));
            _farmmenu.Add("UseRLane", new CheckBox("Use R"));
            _farmmenu.Add("Lanemana", new Slider("Minimum Mana", 35, 1, 100));
            _farmmenu.AddSeparator();

            //Jungle Clear
            _junglemenu = _config.AddSubMenu("JungleClear", "JungleClear");
            _junglemenu.AddLabel("Jungle Clear Settings");
            _junglemenu.Add("UseQJungle", new CheckBox("Use Q"));
            _junglemenu.Add("UseWJungle", new CheckBox("Use W"));
            _junglemenu.Add("UseRJungle", new CheckBox("Use R"));
            _junglemenu.Add("Junglemana", new Slider("Minimum Mana", 35, 1, 100));
            _junglemenu.AddSeparator();

            //Smite
            _smitemenu = _config.AddSubMenu("Smite", "Smite");
            _smitemenu.AddLabel("Smite Settings");
            _smitemenu.Add("Usesmite", new KeyBind("Use Smite(toggle)", true, KeyBind.BindTypes.PressToggle, 'H'));
            _smitemenu.Add("Useblue", new CheckBox("Smite Blue Early"));
            _smitemenu.Add("manaJ", new Slider("Smite Blue Early if MP% <", 35, 1, 100));
            _smitemenu.Add("Usered", new CheckBox("Smite Red Early"));
            _smitemenu.Add("healthJ", new Slider("Smite Red Early if HP% <", 35, 1, 100));
            _smitemenu.AddSeparator();

            //Misc
            _miscmenu = _config.AddSubMenu("Misc", "Misc");
            _miscmenu.AddLabel("Misc Settings");
            _miscmenu.Add("AutoShield", new CheckBox("Auto W"));
            _miscmenu.Add("Shieldper", new Slider("Use W if HP < %", 35, 1, 100));
            _miscmenu.Add("Inter_E", new CheckBox("Use E to Interrupter"));
            _miscmenu.Add("Gap_W", new CheckBox("Use W to GapClosers"));
            _miscmenu.Add("UseQKs", new CheckBox("Use Q KillSteal"));
            _miscmenu.Add("UseRKs", new CheckBox("Use R KillSteal"));
            _miscmenu.Add("TargetRange", new Slider("Use R  if Target Range >= ", 400, 200, 825));
            _miscmenu.Add("UseIgnite", new CheckBox("Use UseIgnite"));
            _miscmenu.AddSeparator();


            //Drawings
            _drawmenu = _config.AddSubMenu("Drawings", "Drawings");
            _drawmenu.AddLabel("Draw Settings");
            _drawmenu.Add("DrawQ", new CheckBox("Draw Q"));
            _drawmenu.Add("DrawW", new CheckBox("Draw W"));
            _drawmenu.Add("DrawE", new CheckBox("Draw E"));
            _drawmenu.Add("DrawR", new CheckBox("Draw R"));
            _drawmenu.Add("Drawharass", new CheckBox("Draw AutoHarass"));
            _drawmenu.Add("Drawsmite", new CheckBox("Draw Smite Stage"));
            _drawmenu.Add("ShowPassive", new CheckBox("Draw Passive Stage"));


            Chat.Print("<font color='#881df2'>D-KogMaw by Diabaths</font> Loaded.");
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Chat.Print(
                "<font color='#f2f21d'>If You like my work and want to support me,  plz donate via paypal in </font> <font color='#00e6ff'>ssssssssssmith@hotmail.com</font> (10) S");
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (_miscmenu["AutoShield"].Cast<CheckBox>().CurrentValue)
            {
                AutoW();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)

            {
                Combomode();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass ||
                _harass["harasstoggle"].Cast<KeyBind>().CurrentValue)
            {
                if (_player.ManaPercent >
                    _harass["Harrasmana"].Cast<Slider>().CurrentValue)
                {
                    Harass();
                }
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear)
            {

                if (_player.ManaPercent > _farmmenu["Lanemana"].Cast<Slider>().CurrentValue)
                {
                    Farm();

                }
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.JungleClear)
            {
                if (_player.ManaPercent > _junglemenu["junglemana"].Cast<Slider>().CurrentValue)
                {
                    JungleClear();

                }
            }
            if (_smitemenu["Usesmite"].Cast<KeyBind>().CurrentValue)
            {
                Smiteuse();
            }
            if (Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Flee)
            {
              Tragic();
            }
            killsteal();
        }

        private static void killsteal()
        {
           
                var target = TargetSelector.GetTarget(_q.Range, DamageType.Magical);
            foreach (AIHeroClient hero in HeroManager.Enemies)
            {
                var igniteDmg = _player.GetSummonerSpellDamage(hero, DamageLibrary.SummonerSpells.Ignite);
                var qhDmg = _player.GetSpellDamage(hero, SpellSlot.Q);
                var rhDmg = _player.GetSpellDamage(hero, SpellSlot.R);
                var rRange = _player.Distance(hero) >= _miscmenu["TargetRange"].Cast<Slider>().CurrentValue;
                if (hero.IsValidTarget(600) && _miscmenu["UseIgnite"].Cast<CheckBox>().CurrentValue &&
                  target.IsValidTarget(Ignite.Range)&&Ignite.IsReady())
                {
                    if (igniteDmg > hero.Health)
                    {
                        Ignite.Cast(hero);
                    }
                    if (_q.IsReady() && hero.IsValidTarget(_q.Range) && _miscmenu["UseQKs"].Cast<CheckBox>().CurrentValue)
                    {
                        if (hero.Health <= qhDmg)
                        {
                            var predQ = _q.GetPrediction(hero);
                            if (predQ.HitChance >= HitChance.High)
                                _q.Cast(hero);
                        }
                    }
                    if (_r.IsReady() && hero.IsValidTarget(_r.Range) && rRange && _miscmenu["UseRKs"].Cast<CheckBox>().CurrentValue)
                    {
                        if (hero.Health <= rhDmg)
                        {
                            _r.Cast(hero);
                        }
                    }
                }
            }
        }
        private static void Tragic()
        {
           var allMinionsQ = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, _player.Position.To2D(),
                    _q.Range);
            var mobs = EntityManager.GetJungleMonsters(_player.Position.To2D(), _q.Range);
            Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
           if (_q.IsReady()) _q.Cast(Game.CursorPos);
            if (_r.IsReady())
            {
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];

                    _r.Cast(mob);
                }
                else if (allMinionsQ.Count >= 1)
                {
                    _r.Cast(allMinionsQ[0]);
                }
            }
        }
        private static void Smiteontarget(Obj_AI_Base hero)
        {
            var smitered = Player.Spells.FirstOrDefault(spell => spell.Name.ToLower().Contains("s5_summonersmiteduel"));
            var smiteblue =
                Player.Spells.FirstOrDefault(spell => spell.Name.ToLower().Contains("s5_summonersmiteplayerganker"));

            if (smiteblue !=null|| smitered !=null)
            {
                
                    var smiteDmg = _player.GetSummonerSpellDamage(hero, DamageLibrary.SummonerSpells.Smite);
                    var usesmite = _combo["smitecombo"].Cast<CheckBox>().CurrentValue;
                    if (SmiteBlue.Any(i => Item.HasItem(i)) && usesmite &&
                        ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready &&
                        hero.IsValidTarget(_smite.Range))
                    {
                        if (!hero.HasBuffOfType(BuffType.Stun) || !hero.HasBuffOfType(BuffType.Slow))
                        {
                            ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                        }
                        else if (smiteDmg >= hero.Health)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                        }
                    }
                    if (SmiteRed.Any(i => Item.HasItem(i)) && usesmite &&
                        ObjectManager.Player.Spellbook.CanUseSpell(_smiteSlot) == SpellState.Ready &&
                        hero.IsValidTarget(_smite.Range))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, hero);
                    }
                }
            }
        

        private static string Smitetype()
        {
            if (SmiteBlue.Any(i => Item.HasItem(i)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(i => Item.HasItem(i)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(i => Item.HasItem(i)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(i => Item.HasItem(i)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }
        private static void SetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, Smitetype(), StringComparison.CurrentCultureIgnoreCase)))
            {
                _smiteSlot = spell.Slot;
                _smite = new Spell.Targeted(_smiteSlot, 700);
                return;
            }
        }
        private static int GetSmiteDmg()
        {
            int level = _player.Level;
            int index = _player.Level / 5;
            float[] dmgs = { 370 + 20 * level, 330 + 30 * level, 240 + 40 * level, 100 + 50 * level };
            return (int)dmgs[index];
        }

        private static void Smiteuse()
        {
            var jungle = Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.JungleClear;
            var useblue = _smitemenu["Useblue"].Cast<CheckBox>().CurrentValue;
            var usered = _smitemenu["Usered"].Cast<CheckBox>().CurrentValue;
            var health = _player.HealthPercent < _smitemenu["healthJ"].Cast<Slider>().CurrentValue;
            var mana = _player.ManaPercent < _smitemenu["manaJ"].Cast<Slider>().CurrentValue;
            string[] jungleMinions;

            jungleMinions = new string[]
            {
                "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon",
                "SRU_Baron"
            };

            var minions = EntityManager.GetJungleMonsters(_player.Position.To2D(), 1000);
            if (minions != null)
            {
                int smiteDmg = GetSmiteDmg();

                foreach (Obj_AI_Minion minion in minions)
                {
                    if (minion.Health <= smiteDmg && jungleMinions.Any(name => minion.Name.StartsWith(name)) &&
                        !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && useblue && mana && minion.Health >= smiteDmg &&
                             jungleMinions.Any(name => minion.Name.StartsWith("SRU_Blue")) &&
                             !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                    else if (jungle && usered && health && minion.Health >= smiteDmg &&
                             jungleMinions.Any(name => minion.Name.StartsWith("SRU_Red")) &&
                             !jungleMinions.Any(name => minion.Name.Contains("Mini")))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(_smiteSlot, minion);
                    }
                }
            }
        }
        public static bool IsUnderTurret(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950) && turret.IsEnemy);
        }
        private static bool InFountain(this AIHeroClient hero)
        {
            var fountainRange = 1050;
            return hero.IsVisible
                   && ObjectManager.Get<Obj_SpawnPoint>().Any(sp => hero.Distance(sp.Position) < fountainRange);
        }
        private static void AutoW()
        {
            if (_player.HasBuff("Recall") || _player.InFountain()) return;
            if (_w.IsReady() &&
                _player.HealthPercent <=  _miscmenu["Shieldper"].Cast<Slider>().CurrentValue)
            {
                _w.Cast();
            }

        }
        private static void Combomode()
        {
            var comboswitch = _combo["ComboPrio"].DisplayName;
            switch (comboswitch)
            {
                case "Q-R":
                    Combo();
                    break;
                case "R-Q":
                    Misaya();
                    break;

            }
        }

        private static float ComboDamage(Obj_AI_Base hero)
        {
            var dmg = 0d;

            if (_q.IsReady())
                dmg += _player.GetSpellDamage(hero, SpellSlot.Q)*2;
            if (_w.IsReady())
                dmg += _player.GetSpellDamage(hero, SpellSlot.W);
            if (_r.IsReady())
                dmg += _player.GetSpellDamage(hero, SpellSlot.R);

            if (Ignite.IsReady())
            {
                dmg += _player.GetSummonerSpellDamage(hero, DamageLibrary.SummonerSpells.Ignite);
            }
            dmg += _player.GetAutoAttackDamage(hero, true) * 2;
            if (_player.HasBuff("dianaarcready"))
            {
                dmg += 15 + 5 * _player.Level;
            }
            if (_player.HasBuff("LichBane"))
            {
                dmg += _player.BaseAttackDamage * 0.75 + _player.FlatMagicDamageMod * 0.5;
            }
            return (float)dmg;
        }
        private static void Misaya()
        {
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Magical);
            var useQ = _combo["UseQCombo"].Cast<CheckBox>().CurrentValue;
            var useW = _combo["UseWCombo"].Cast<CheckBox>().CurrentValue;
            var useE = _combo["UseECombo"].Cast<CheckBox>().CurrentValue;
            var useR = _combo["UseRCombo"].Cast<CheckBox>().CurrentValue;
            var ignitecombo = _combo["UseIgnitecombo"].Cast<CheckBox>().CurrentValue;
         //   var qmana = _player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
         //   var wmana = _player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
        //    var rmana = _player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
           
            Smiteontarget(target);

            if (target.IsValidTarget(Ignite.Range) &&  ignitecombo && Ignite.IsReady() && Ignite!=null)
            {
               if (target.Health <= ComboDamage(target))
              {
                    Ignite.Cast(target);
               }
            }

            if (target.IsValidTarget(_r.Range) && useQ && useR && _q.IsReady() && _r.IsReady())
            {
                if (_q.GetPrediction(target).HitChance >= HitChance.High)
                {
                    _r.Cast(target);
                    _q.Cast(target);

                }
            }
            if (target.IsValidTarget(_w.Range) && useW && _w.IsReady())
            {
                _w.Cast();
            }
            if (target.IsValidTarget(_e.Range) && !target.IsValidTarget(_w.Range) &&
                useE && _e.IsReady() && !_w.IsReady())
            {
                _e.Cast();
            }
           if (_combo["UseRSecond"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(_r.Range))
            {
                if (target.Health <=
                    _player.GetSpellDamage(target, SpellSlot.R) +_player.GetSpellDamage(target, SpellSlot.W)+
                    _player.GetAutoAttackDamage(target, true) && _r.IsReady() && _w.IsReady())
                {
                    _r.Cast(target);
                    _w.Cast();
                }
                if (target.Health <=
                    _player.GetSpellDamage(target, SpellSlot.R) + _player.GetAutoAttackDamage(target, true) &&
                    _r.IsReady())
                {
                    _r.Cast(target);
                }
            }
          }

        private static void Combo()
        {
            var changetime = Environment.TickCount - _lastTick;
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Magical);
            var ignitecombo = _combo["UseIgnitecombo"].Cast<CheckBox>().CurrentValue;
            var _ignite = Player.Spells.FirstOrDefault(spell => spell.Name.ToLower().Contains("summonerheal"));
            Smiteontarget(target);

            if (target.IsValidTarget(Ignite.Range) && ignitecombo && Ignite.IsReady() && _ignite !=null)
            {
               if (target.Health <= ComboDamage(target))
                {
                    Ignite.Cast(target);
                }
            }
            if (target.IsValidTarget(_q.Range) && _combo["UseQCombo"].Cast<CheckBox>().CurrentValue && _q.IsReady())
            {
                var predQ = _q.GetPrediction(target);
                if (predQ.HitChance >= HitChance.High)
                    _q.Cast(predQ.CastPosition);
            }
            if (changetime>=1500&&target.IsValidTarget(_r.Range) && _combo["UseRCombo"].Cast<CheckBox>().CurrentValue && _r.IsReady() &&
                ((_qcreated == true) || target.HasBuff("dianamoonlight")))
            {
                _r.Cast(target);
                _lastTick = Environment.TickCount;
            }
            if (target.IsValidTarget(_w.Range) && _combo["UseWCombo"].Cast<CheckBox>().CurrentValue && _w.IsReady() &&
                !_q.IsReady())
            {
                _w.Cast();
            }
            if (target.IsValidTarget(_e.Range) && _player.Distance(target) >= _w.Range &&
               _combo["UseECombo"].Cast<CheckBox>().CurrentValue && _e.IsReady() && !_w.IsReady())
            {
                _e.Cast();
            }
           if (_combo["UseRSecond"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(_r.Range))
            {
                if (target.Health <=
                  _player.GetSpellDamage(target, SpellSlot.R)+_player.GetSpellDamage(target, SpellSlot.W) +
                    _player.GetAutoAttackDamage(target, true) && _r.IsReady() && _w.IsReady())
                {
                    _r.Cast(target);
                    _w.Cast();
                }
                if (target.Health <=
                   _player.GetSpellDamage(target, SpellSlot.R)+ _player.GetAutoAttackDamage(target, true) &&
                    _r.IsReady())
                {
                    _r.Cast(target);
                }
            }
            
        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_q.Range,DamageType.Magical);

            if (target.IsValidTarget(_q.Range) && _harass["UseQHarass"].Cast<CheckBox>().CurrentValue && _q.IsReady())
            {
                var predQ = _q.GetPrediction(target);
                if (predQ.HitChance >= HitChance.High)
                    _q.Cast(predQ.CastPosition);
            }
            if (target.IsValidTarget(_w.Range) && _harass["UseWHarass"].Cast<CheckBox>().CurrentValue && _w.IsReady())
            {
                _w.Cast();
            }
            if (IsUnderTurret(target.ServerPosition) && target.IsValidTarget(_r.Range) && _harass["UseRHarass"].Cast<CheckBox>().CurrentValue && _r.IsReady() && target.HasBuff("dianamoonlight"))
            {
                _r.Cast(target);
            }
        }
        private static void Farm()
        {
            var minion = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, _player.Position.To2D(),
                    _q.Range);
          
            var useQ = _farmmenu["UseQLane"].Cast<CheckBox>().CurrentValue;
            var useW = _farmmenu["UseWLane"].Cast<CheckBox>().CurrentValue;
            var useR = _farmmenu["UseRLane"].Cast<CheckBox>().CurrentValue;
            
            foreach (var mobs in minion)
            {
                if (_q.IsReady() && useQ)
                {
                    if (minion.Count >= 3)
                    {
                        _q.Cast(mobs);
                    }

                    else if (mobs.Distance(_player) > _player.GetAutoAttackRange(mobs) &&
                             mobs.Health < 0.75 * _player.GetSpellDamage(mobs, SpellSlot.Q))
                        _q.Cast(mobs);
                }
                if (_w.IsReady() && useW&& mobs.IsValidTarget(_w.Range))
                {
                    if (minion.Count >= 3)
                    {
                        _w.Cast();
                    }
                    else
                        if (mobs.Distance(_player) > _player.GetAutoAttackRange(mobs) &&
                            mobs.Health < 0.75 * _player.GetSpellDamage(mobs, SpellSlot.W))
                            _w.Cast();
                }
                if (_r.IsReady() && useR && mobs.HasBuff("dianamoonlight"))
                {
                   if (mobs.Distance(_player) > _player.GetAutoAttackRange(mobs) &&
                            mobs.Health < 0.75 * _player.GetSpellDamage(mobs, SpellSlot.R))
                            _r.Cast(mobs);
                }
            }
        }
        private static void JungleClear()
        {
            var mininions = EntityManager.GetJungleMonsters(_player.Position.To2D(), _q.Range);
            var useQ = _junglemenu["UseQJungle"].Cast<CheckBox>().CurrentValue;
            var useW = _junglemenu["UseWJungle"].Cast<CheckBox>().CurrentValue;
            var useR = _junglemenu["UseRJungle"].Cast<CheckBox>().CurrentValue;
            var changetime = Environment.TickCount - _lastTick;

            foreach (var minion in mininions)
            {

                if (useQ && _q.IsReady() && minion.IsValidTarget(_q.Range))
                {
                    _q.Cast(minion);
                    _lastTick = Environment.TickCount;
                }
                if (_w.IsReady() && useW && minion.IsValidTarget(_w.Range))
                {
                    _w.Cast();
                }
                if (changetime >= 1500&&_r.IsReady() && useR && minion.HasBuff("dianamoonlight")&& !mininions.Any(name => minion.Name.Contains("Mini")))
                {
                    _r.Cast(minion);
                    _lastTick = Environment.TickCount;
                }

            }
        }
        private static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs args)
        {
            if (_w.IsReady() && sender.IsValidTarget(_w.Range) && _miscmenu["Gap_W"].Cast<CheckBox>().CurrentValue)
            {
                _w.Cast();
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spell = args.SData;
            if (!sender.IsMe)
            {
                return;
            }
            if (spell.Name.ToLower().Contains("dianaarc") || spell.Name.ToLower().Contains("dianateleport"))
            {
                Core.DelayAction(Orbwalker.ResetAutoAttack, 250);
            }
            /*if (sender.IsMe)
             {
                  Game.PrintChat("Spell name: " + args.SData.Name.ToString());
             }*/
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,Interrupter.InterruptableSpellEventArgs args)
        {
            if (_e.IsReady() && sender.IsValidTarget(_e.Range) && _miscmenu["Inter_E"].Cast<CheckBox>().CurrentValue)
                //Console.WriteLine("Cast E");
                _e.Cast();
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var harass = _harass["harasstoggle"].Cast<KeyBind>().CurrentValue;
            var diana = Drawing.WorldToScreen(_player.Position);
           
            if (_drawmenu["Drawharass"].Cast<CheckBox>().CurrentValue)
            {
                if (harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.02f, Drawing.Height * 0.92f, System.Drawing.Color.GreenYellow,
                        "Auto harass Enabled");
                }
                else
                    Drawing.DrawText(Drawing.Width * 0.02f, Drawing.Height * 0.92f, System.Drawing.Color.OrangeRed,
                        "Auto harass Disabled");
            }
            if (_drawmenu["Drawsmite"].Cast<CheckBox>().CurrentValue)
            {
                if (_smitemenu["Usesmite"].Cast<KeyBind>().CurrentValue)
                {
                    Drawing.DrawText(Drawing.Width * 0.02f, Drawing.Height * 0.88f, System.Drawing.Color.GreenYellow,
                        "Smite Jungle On");
                }
                else
                    Drawing.DrawText(Drawing.Width * 0.02f, Drawing.Height * 0.88f, System.Drawing.Color.OrangeRed,
                        "Smite Jungle Off");
                if (SmiteBlue.Any(i => Item.HasItem(i)) || SmiteRed.Any(i => Item.HasItem(i)))
                {
                    if (_combo["smitecombo"].Cast<CheckBox>().CurrentValue)
                    {
                        Drawing.DrawText(Drawing.Width * 0.02f, Drawing.Height * 0.90f, System.Drawing.Color.GreenYellow,
                            "Smite Target On");
                    }
                    else
                        Drawing.DrawText(Drawing.Width * 0.02f, Drawing.Height * 0.90f, System.Drawing.Color.OrangeRed,
                            "Smite Target Off");
                }
            }
           /* if (_drawmenu["ShowPassive"].Cast<CheckBox>().CurrentValue)
            {
                if (_player.HasBuff("dianaarcready"))
                    Drawing.DrawText(diana[0] - 10, diana[1], Color.GreenYellow, "P On");
                else if (!_player.HasBuff("dianaarcready"))
                    Drawing.DrawText(diana[0] - 10, diana[1], Color.OrangeRed, "P Off");
            }*/
            if (_drawmenu["DrawQ"].Cast<CheckBox>().CurrentValue && _q.Level > 0)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = _q.Range }.Draw(_player.Position);
            }
            if (_drawmenu["DrawW"].Cast<CheckBox>().CurrentValue && _w.Level > 0)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = _w.Range }.Draw(_player.Position);
            }
            if (_drawmenu["DrawE"].Cast<CheckBox>().CurrentValue && _e.Level > 0)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = _e.Range }.Draw(_player.Position);
            }
            if (_drawmenu["DrawR"].Cast<CheckBox>().CurrentValue && _r.Level > 0)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = _r.Range }.Draw(_player.Position);
            }
        }
        private static void OnCreate(GameObject sender, EventArgs args)
        {
            var missile = sender as Obj_SpellMissile;
            if (missile != null)
            {
                var spell = missile;
                var unit = spell.SpellCaster.Name;
                var name = spell.SData.Name;
                var caster = spell.SpellCaster;

                if (unit == ObjectManager.Player.Name && (name == "dianaarcthrow"))
                {
                    _qpos = spell;
                    _qcreated = true;
                    return;
                }
            }
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {//if (sender is Obj_SpellMissile)
            var missile = sender as Obj_SpellMissile;
            if (missile == null) return;
            var spell = missile;
            var unit = spell.SpellCaster.Name;
            var name = spell.SData.Name;

            if (unit == ObjectManager.Player.Name && (name == "dianaarcthrow"))
            {
                _qpos = null;
                _qcreated = false;
                return;
            }
        }
    }
} 