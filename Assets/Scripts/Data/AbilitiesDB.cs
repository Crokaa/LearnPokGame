using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AbilitiesDB
{
    public static void Init()
    {
        foreach (var kvp in Abilities)
        {

            var abilityID = kvp.Key;
            var ability = kvp.Value;

            ability.Id = abilityID;
        }
    }

    public static Dictionary<AbilityID, Ability> Abilities { get; set; } = new Dictionary<AbilityID, Ability>{

        {
            AbilityID.adaptability,
            new Ability()
            {
                Name = "Adaptability",
                Description = "Powers up moves of the same type as the Pokémon.",
                OnApplyStab = (float stab) => {
                    return stab == 1.5f? 2f : 1f;
                }
            }
        },

        {
            AbilityID.aerilate,
            new Ability()
            {
                Name = "Adaptability",
                Description = "Normal-type moves become Flying-type moves. The power of those moves is boosted a little.",
                ChangeType = (Move move) => {

                    return move.Base.Type == PokemonType.Normal ? PokemonType.Flying : move.Base.Type;
                },
                MoveBoost = (Move move, Pokemon source) => {

                    return move.Base.Type == PokemonType.Normal ? move.Base.Power * 1.2f : move.Base.Power;
                }
            }
        },

        {
            AbilityID.aftermath,
            new Ability()
            {
                Name = "Aftermath",
                Description = "Damages the attacker if it knocks out the Pokémon with a move that makes direct contact.",
                OnDamaged = (Move move, Pokemon attacker, Pokemon target) => {

                    if(move.Base.HasFlag(MoveFlag.Contact) && target.HP <= 0)
                    {
                        target.StatusChanges.Enqueue($"{attacker.Base.Name} was hurt!");
                        target.UpdateHP(attacker.Base.MaxHp / 4);
                    }
                }
            }
        },

        {
            // I need to think about this and forecast as they have a unique way of working together
            AbilityID.airlock,
            new Ability()
            {
                Name = "Airlock",
                Description = "Eliminates the effects of weather.",
                NegateWeather = true
            }
        },

        {
            AbilityID.analytic,
            new Ability()
            {
                Name = "Analytic",
                Description = "Boosts the power of the Pokémon’s move if it is the last to act that turn.",
                //Later
            }
        },

        {
            AbilityID.angerpoint,
            new Ability()
            {
                Name = "Anger Point",
                Description = "The Pokémon is angered when it takes a critical hit, and that maxes its Attack stat.",
                OnCriticalReceive = (Pokemon target) => {

                    target.StatBoosts[Stat.Attack] = 6;
                    target.StatusChanges.Enqueue($"{target.Base.Name} maxed its Attack");
                }

            }
        },

        {
            AbilityID.angershell,
            new Ability()
            {
                Name = "Anger Shell",
                Description = "When an attack causes its HP to drop to half or less, the Pokémon gets angry. This lowers its Defense and Sp. Def stats but boosts its Attack, Sp. Atk, and Speed stats.",
                OnDropHalf = (Pokemon attacker, Pokemon target) => {

                    List<StatBoost> boosts = new List<StatBoost> { new StatBoost{stat = Stat.Attack, boost = 1},
                        new StatBoost{stat = Stat.SpAttack, boost = 1},
                        new StatBoost{stat = Stat.Speed, boost = 1},
                        new StatBoost{stat = Stat.Defense, boost = -1},
                        new StatBoost{stat = Stat.SpDefense, boost = -1} };
                        target.ApplyBoost(boosts);
                }

            }
        },

        {
            AbilityID.anticipation,
            new Ability()
            {
                Name = "Anticipation",
                Description = "The Pokémon can sense an opposing Pokémon’s dangerous moves.",
                OnSwitchIn = (Pokemon source, Pokemon enemy) => {

                    var type1 = source.Base.Type1;
                    var type2 = source.Base.Type2;

                    for(int i = 0; i < enemy.Moves.Count; i++)
                    {
                        //When OHKO moves are added, put it in the condition
                        if(TypeChart.GetEffectiveness(enemy.Moves[i].Base.Type, type1) == 2f || TypeChart.GetEffectiveness(enemy.Moves[i].Base.Type, type2) == 2)
                        {
                            source.StatusChanges.Enqueue($"The {source.Base.Name} shuddered");
                            break;
                        }
                    }

                }

            }
        },

        {
            AbilityID.arenatrap,
            new Ability()
            {
                Name = "Arena Trap",
                Description = "Prevents opposing Pokémon from fleeing from battle.",
                CanRun = (Pokemon target) => {

                    var type1 = target.Base.Type1;
                    var type2 = target.Base.Type2;

                    if(type1 == PokemonType.Flying || type2 == PokemonType.Flying || target.Ability.Id == AbilityID.levitate)
                        return true;

                    return false;
                }

            }
        },

        {
            AbilityID.armortail,
            new Ability()
            {
                Name = "Armor Tail",
                Description = "The mysterious tail covering the Pokémon’s head makes opponents unable to use priority moves against the Pokémon or its allies.",
                CanUseMove = (Move move, Pokemon source, Pokemon target) => {

                    if(move.Base.Priority > 0)
                        return false;

                    source.StatusChanges.Enqueue($"{source.Base.Name} cannot use {move.Base.Name}");
                    return true;
                }

            }
        },

        {
            AbilityID.aromaveil,
            new Ability()
            {
                Name = "Aroma Veil",
                Description = "Protects the Pokémon and its allies from effects that prevent the use of moves.",
                // Later
            }
        },

        {
            AbilityID.asone,
            new Ability()
            {
                Name = "As One",
                Description = "This Ability combines the effects of both Calyrex’s Unnerve Ability and Glastrier’s/Spectrier’s Chilling Neigh/Grim Neigh Ability.",
                OnDamaged = (Move move, Pokemon attacker, Pokemon target) => {

                    if(target.HP <= 0){
                        if(attacker.StatBoosts[Stat.Attack] < 6)
                            attacker.ApplyBoost( new List<StatBoost> { new StatBoost{ stat = Stat.Attack, boost = 1 } } );
                    }
                }
                //Implement when berries (items) are implemented too
            }
        },

        {
            AbilityID.aurabreak,
            new Ability()
            {
                Name = "Aura Break",
                Description = "The effects of “Aura” Abilities are reversed to lower the power of affected moves.",
                MoveBoost  = (Move move, Pokemon source) => {

                    return move.Base.Type == PokemonType.Dark || move.Base.Type == PokemonType.Fairy ? move.Base.Power - move.Base.Power * 0.25f : move.Base.Power;
                }
                //Cancel fairy and dark auras when implemented
            }
        },

        {
            AbilityID.baddreams,
            new Ability()
            {
                Name = "Bad Dreams",
                Description = "Damages opposing Pokémon that are asleep.",
                OnTurnEnd  = (Pokemon source, Pokemon enemy) => {

                    if (enemy.Status.Id == ConditionID.slp){
                        enemy.UpdateHP(enemy.MaxHp / 8);
                        enemy.StatusChanges.Enqueue($"The {enemy.Base.Name} is tormented.");
                    }

                }
            }
        },

        {
            AbilityID.ballfetch,
            new Ability()
            {
                Name = "Ball Fetch",
                Description = "If the Pokémon is not holding an item, it will fetch the Poké Ball from the first failed throw of the battle.",
                //Later
            }
        },

        {
            AbilityID.battery,
            new Ability()
            {
                Name = "Battery",
                Description = "Powers up ally Pokémon’s special moves.",
                //Later when doubles implemented
            }
        },

        {
            AbilityID.battlearmor,
            new Ability()
            {
                Name = "Battle Armor",
                Description = "Hard armor protects the Pokémon from critical hits.",
                CalculateCritical = () => {

                    return 1f;
                }
            }
        },

        {
            AbilityID.battlebond,
            new Ability()
            {
                Name = "Battle Bond",
                Description = "When the Pokémon knocks out a target, its bond with its Trainer is strengthened, and its Attack, Sp. Atk, and Speed stats are boosted.",
                OnDamaged = (Move move, Pokemon attacker, Pokemon target) => {

                    if(Ability.Activated())
                        return;

                    if(target.HP <= 0 && attacker.StatBoosts[Stat.Attack] != 6 && attacker.StatBoosts[Stat.SpAttack] != 6 && attacker.StatBoosts[Stat.Speed] != 6)
                    {
                        List<StatBoost> boosts = new List<StatBoost> { new StatBoost{stat = Stat.Attack, boost = 1},
                        new StatBoost{stat = Stat.SpAttack, boost = 1},
                        new StatBoost{stat = Stat.Speed, boost = 1} };
                        target.ApplyBoost(boosts);
                        Ability.Activate();
                    }
                },
                BattleEnded = () => {
                    Ability.EndBattleActivate();
                }
            }
        },

        {
            AbilityID.beadsofruin,
            new Ability()
            {
                Name = "Beads Of Ruin",
                Description = "The power of the Pokémon’s ruinous beads lowers the Sp. Def stats of all Pokémon except itself.",
                //Later
            }
        },

        {
            AbilityID.beastboost,
            new Ability()
            {
                Name = "Beast Boost",
                Description = "The power of the Pokémon’s ruinous beads lowers the Sp. Def stats of all Pokémon except itself.",
                OnDamaged = (Move move, Pokemon attacker, Pokemon target) => {

                    if (target.HP <= 0)
                    {
                        List<Stat> statToBoost = new List<Stat> { Stat.Attack, Stat.Defense, Stat.SpAttack, Stat.SpDefense, Stat.Speed };
                        int [] attackerStats = new int[] { attacker.Attack, attacker.Defense, attacker.SpAttack, attacker.SpDefense, attacker.Speed };
                        int indexHigherStat = 0;
                        for (int i = 0; i < attackerStats.Length; i++)
                        {
                            if (attackerStats[i] > attackerStats[indexHigherStat])
                                indexHigherStat = i;
                        }

                        if (attacker.StatBoosts[statToBoost[indexHigherStat]] == 6)
                            return;

                        target.ApplyBoost( new List<StatBoost> { new StatBoost{ stat = statToBoost[indexHigherStat], boost = 1 } } );
                    }
                },
            }
        },

        {
            AbilityID.berserk,
            new Ability()
            {
                Name = "Berserk",
                Description = "Boosts the Pokémon’s Sp. Atk stat when it takes a hit that causes its HP to drop to half or less.",
                OnDropHalf = (Pokemon attacker, Pokemon target) => {

                    List<StatBoost> boosts = new List<StatBoost> { new StatBoost{stat = Stat.Attack, boost = 1},
                        new StatBoost{stat = Stat.SpAttack, boost = 1},
                        new StatBoost{stat = Stat.Speed, boost = 1},
                        new StatBoost{stat = Stat.Defense, boost = -1},
                        new StatBoost{stat = Stat.SpDefense, boost = -1} };
                    target.ApplyBoost(boosts);

                },
            }
        },

        {
            AbilityID.bigpecks,
            new Ability()
            {
                Name = "Big Pecks",
                Description = "Prevents the Pokémon from having its Defense stat lowered.",
                OnBoost = (Dictionary<Stat, int> boosts, Pokemon target, Pokemon source) =>
                {
                    // If it's self boost then return
                    if (source != null && target == source) return;

                    if (boosts.ContainsKey(Stat.Defense) && boosts[Stat.Defense] < 0)
                    {
                        boosts.Remove(Stat.Defense);

                        target.StatusChanges.Enqueue($"{target.Base.Name}'s was not lowered");
                    }
                },
            }
        },

        {
            AbilityID.blaze,
            new Ability()
            {
                Name = "Blaze",
                Description = "Powers up Fire-type moves when the Pokémon’s HP is low.",
                MoveBoost = (Move move, Pokemon source) =>
                {
                    return move.Base.Type == PokemonType.Fire && source.HP <= source.MaxHp ? move.Base.Power * 1.5f : move.Base.Power;
                },
            }
        },

        {
            AbilityID.bulletproof,
            new Ability()
            {
                Name = "Bulletproof",
                Description = "Protects the Pokémon from ball and bomb moves.",
                CanUseMove = (Move move, Pokemon source, Pokemon target) => {

                    if(move.Base.Priority > 0)
                        return false;

                    source.StatusChanges.Enqueue($"{source.Base.Name} cannot use {move.Base.Name}");
                    return true;
                }
            }
        },

        {
            AbilityID.cheekpouch,
            new Ability()
            {
                Name = "Cheek Pouch",
                Description = "The Pokémon’s HP is restored when it eats any Berry, in addition to the Berry’s usual effect.",
                OnBerryEat = (Pokemon source) => {

                    source.HealHP(source.MaxHp / 3);
                }
            }
        },

        {
            AbilityID.chillingneigh,
            new Ability()
            {
                Name = "Chilling Neigh",
                Description = "When the Pokémon knocks out a target, it utters a chilling neigh, which boosts its Attack stat.",
                OnDamaged = (Move move, Pokemon attacker, Pokemon target) => {

                    if(target.HP <= 0){
                        if(attacker.StatBoosts[Stat.Attack] < 6)
                            attacker.ApplyBoost( new List<StatBoost> { new StatBoost{ stat = Stat.Attack, boost = 1 } } );
                    }
                }
            }
        },

    
    };

}


public enum AbilityID
{
    adaptability, aerilate, aftermath, airlock, analytic, angerpoint, angershell, anticipation, arenatrap, armortail,
    aromaveil, asone, aurabreak, baddreams, ballfetch, battery, battlearmor, battlebond, beadsofruin, beastboost,
    berserk, bigpecks, blaze, bulletproof, cheekpouch, chillingneigh, chlorophyll, clearbody, cloudnine, colorchange,
    comatose, commander, competitive, compoundeyes, contrary, corrosion, costar, cottondown, cudchew, curiousmedicine,
    cursedbody, cutecharm, damp, dancer, darkaura, dauntlessshield, dazzling, defeatist, defiant, deltastream,
    desolateland, disguise, download, dragonsmaw, drizzle, drought, dryskin, earlybird, eartheater, effectspore,
    electricsurge, electromorphosis, embodyaspect, emergencyexit, fairyaura, filter, flamebody, flareboost, flashfire, flowergift,
    flowerveil, fluffy, forecast, forewarn, friendguard, frisk, fullmetalbody, furcoat, galewings, galvanize,
    gluttony, goodasgold, gooey, gorillatactics, grasspelt, grassysurge, grimneigh, guarddog, gulpmissile, guts,
    hadronengine, harvest, healer, heatproof, heavymetal, honeygather, hospitality, hugepower, hungerswitch, hustle,
    hydration, hypercutter, icebody, iceface, icescales, illuminate, illusion, immunity, imposter, infiltrator,
    innardsout, innerfocus, insomnia, intimidate, intrepidsword, ironbarbs, ironfist, justified, keeneye, klutz,
    leafguard, levitate, libero, lightmetal, lightningrod, limber, lingeringaroma, liquidooze, liquidvoice, longreach,
    magicbounce, magicguard, magician, magmaarmor, magnetpull, marvelscale, megalauncher, merciless, mimicry, mindseye,
    minus, mirrorarmor, mistysurge, moldbreaker, moody, motordrive, moxie, multiscale, multitype, mummy,
    myceliummight, naturalcure, neuroforce, neutralizinggas, noguard, normalize, oblivious, opportunist, orichalcumpulse, overcoat,
    overgrow, owntempo, parentalbond, pastelveil, perishbody, pickpocket, pickup, pixilate, plus, poisonheal,
    poisonpoint, poisonpuppeteer, poisontouch, powerconstruct, powerofalchemy, powerspot, prankster, pressure, primordialsea, prismarmor,
    propellertail, protean, protosynthesis, psychicsurge, punkrock, purepower, purifyingsalt, quarkdrive, queenlymajesty, quickdraw,
    quickfeet, raindish, rattled, receiver, reckless, refrigerate, regenerator, ripen, rivalry, rkssystem,
    rockhead, rockypayload, roughskin, runaway, sandforce, sandrush, sandspit, sandstream, sandveil, sapsipper,
    schooling, scrappy, screencleaner, seedsower, serenegrace, shadowshield, shadowtag, sharpness, shedskin, sheerforce,
    shellarmor, shielddust, shieldsdown, simple, skilllink, slowstart, slushrush, sniper, snowcloak, snowwarning,
    solarpower, solidrock, soulheart, soundproof, speedboost, stakeout, stall, stalwart, stamina, stancechange,
    Static, steadfast, steamengine, steelworker, steelyspirit, stench, stickyhold, stormdrain, strongjaw, sturdy,
    suctioncups, superluck, supersweetsyrup, supremeoverlord, surgesurfer, swarm, sweetveil, swiftswim, swordofruin, symbiosis,
    synchronize, tabletsofruin, tangledfeet, tanglinghair, technician, telepathy, terashell, terashift, teraformzero, teravolt,
    thermalexchange, thickfat, tintedlens, torrent, toughclaws, toxicboost, toxicchain, toxicdebris, trace, transistor,
    triage, truant, turboblaze, unaware, unburden, unnerve, unseenfist, vesselofruin, victorystar, vitalspirit,
    voltabsorb, wanderingspirit, waterabsorb, waterbubble, watercompaction, waterveil, weakarmor, wellbakedbody, whitesmoke, wimpout,
    windpower, windrider, wonderguard, wonderskin, zenmode, zerotohero
}
