using System;
using System.Collections;
using System.Collections.Generic;
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
                MoveBoost = (Move move) => {

                    return move.Base.Type == PokemonType.Normal ? move.Base.Power * 1.2f : move.Base.Power;
                }
            }
        }

        
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
