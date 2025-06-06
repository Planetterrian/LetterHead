﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DawgSharp;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class WordManager : Singleton<WordManager>
{
    public string wordListFileName;
    public bool debugValid;

    private Dawg<bool> dawg;

    private static string badWordCsv = "2GIRLS1CUP,ABO,ABOS,ACROTOMOPHILIA,ADASSED,ALABAMAHOTPOCKET,ALASKANPIPELINE,ANAL,ANILINGUS,ANUS,APESHIT,ARSE,ARSEHOLE,ARSEHOLES,ARSES,ASKHOLE,ASSHOLE,ASSHOLES,ASSMUNCH,AUTOEROTIC,BABELAND,BABYBAG,BABYBATT,BABYBATTER,BABYGAG,BABYGRAVY,BABYJUICE,BABYLICK,BABYSACK,BABYSUCK,BADASS,BADASSES,BALLBUSTER,BALLBUSTERS,BALLGAG,BALLGRAVY,BALLKICKING,BALLLICKING,BALLOCKS,BALLSACK,BALLSIER,BALLSIEST,BALLSUCKING,BALLSY,BAMPOT,BANGBROS,BARELYLEGAL,BARENAKED,BASTARD,BASTARDO,BASTINADO,BATSHIT,BAYWOP,BAYWOPS,BAZOOM,BAZOOMS,BBW,BCRAP,BDSM,BEANER,BEANERS,BEASTIAL,BEAVERCLEAVER,BEAVERLIPS,BELLEND,BESTIALITY,BEWB,BIATCH,BIGBLACK,BIGBREASTS,BIGKNOCKERS,BIGTITS,BIMBOS,BIRDLOCK,BLACKCOCK,BLONDEACTION,BLONDEONBLONDEACTION,BLOWJOB,BLOWJOBS,BLOWYOURLOAD,BLUEWAFFLE,BLUMPKIN,BOCHE,BOCHES,BOFF,BOFFED,BOFFING,BOFFS,BOGOFF,BOHUNK,BOHUNKS,BOINK,BOINKED,BOINKING,BOINKS,BOIOLAS,BOLLOCK,BOLLOCKS,BOLLOK,BOLLOKS,BOLLOX,BONDAGE,BONER,BOOBIE,BOOBIES,BOODIES,BOODY,BOOTYCALL,BROWNSHOWERS,BRUNETTEACTION,BUBBA,BUBBAS,BUBBIES,BUBBY,BUCKRA,BUCKRAS,BUGGERY,BUKKAKE,BULLDIKE,BULLDIKES,BULLDYKE,BULLDYKES,BULLETVIBE,BULLSHAT,BULLSHIT,BULLSHITS,BULLSHITTED,BULLSHITTING,BUMBOY,BUMBOYS,BUMWAD,BUMWADS,BUNG,BUNGHOLE,BUSTY,BUTCH,BUTCHES,BUTCHEST,BUTTCHEEK,BUTTCHEEKS,BUTTHOLE,BUTTLUG,BUTTMUCH,BUTTPIRATE,BUTTPLUG,CAMELTOE,CAMGIRL,CAMSLUT,CAMWHORE,CARPETMUNCH,CARPETMUNCHER,CAWK,CHICKENSHIT,CHICKENSHITS,CHINC,CHINK,CHOAD,CHOCOLATEROSEBUDS,CHODE,CHOLA,CHOLAS,CHOLO,CHOLOS,CIPA,CIRCLEJERK,CLEVELANDSTEAMER,CLIT,CLITORIS,CLITS,CLOVERCLAMPS,CLUSTERFUCK,CNUT,COCK,COCKS,COCKSMAN,COCKSMEN,COCKSUCKER,COCKSUCKERS,COJONES,COLOREDS,COMSYMP,COMSYMPS,COOCH,COON,COONS,COONSHIT,COONSHITS,COOTER,COPROLAGNIA,COPROPHILIA,CORNHOLE,COX,CRACKER,CRAP,CRAPPER,CRAPPERS,CREAMPIE,CRIP,CRIPS,CRUMBLIES,CULCHIE,CULCHIER,CULCHIES,CULCHIEST,CULSHIE,CULSHIER,CULSHIES,CULSHIEST,CUM,CUMMING,CUMS,CUNNIE,CUNNILING,CUNNILINGUS,CUNT,CUNTHORPE,CUNTS,DAGO,DAGOES,DAGOS,DAMN,DARKEY,DARKEYS,DARKIE,DARKIES,DARKY,DATERAPE,DEEPTHROAT,DEGGO,DENDROPHILIA,DEUCED,DICK,DICKED,DICKHEAD,DICKHEADS,DICKING,DICKS,DIKE,DIKEY,DILDO,DINGLEBERRIES,DINGLEBERRY,DINK,DIPSHIT,DIPSHITS,DIRSA,DIRTYPILLOWS,DIRTYSANCHEZ,DOGAN,DOGANS,DOGGIESTYLE,DOGGING,DOGGYSTYLE,DOGSTYLE,DOLCETT,DOMINATION,DOMINATRIX,DOMMES,DONG,DONKEYPUNCH,DOOKIE,DOOSH,DOOSHBAG,DOUBLEDONG,DOUBLEPENETRATION,DOUCHE,DPACTION,DRYHUMP,DUCHE,DVDA,DYKE,DYKEY,EATMYASS,ECCHI,EFFING,EJACKULATE,EJACULATION,EROTISM,EUNUCH,FAG,FAGGIER,FAGGIEST,FAGGOT,FAGGOTS,FAGGOTRIES,FAGGOTRY,FAGGOTY,FAGGY,FATSO,FATSOES,FATSOS,FECK,FELCH,FELCHING,FELLATE,FELLATIO,FELTCH,FEMALESQUIRTING,FEMDOM,FEMINAZI,FEMINAZIS,FIGGING,FINGERBANG,FINGERING,FISTING,FISTINGS,FLAMER,FOOTFETISH,FOOTJOB,FORESKIN,FROTTING,FUBAR,FUCK,FUCKBUTTONS,FUCKED,FUCKER,FUCKERS,FUCKFACE,FUCKFACES,FUCKHEAD,FUCKHEADS,FUCKIN,FUCKING,FUCKOFF,FUCKOFFS,FUCKS,FUCKTARDS,FUCKUP,FUCKUPS,FUCKWIT,FUCKWITS,FUCT,FUDGEPACKER,FUTANARI,FUX,GADJE,GADJO,GANGBANG,GAYSEX,GAZOO,GAZOOS,GIANTCOCK,GINO,GINZO,GINZOES,GIRLIE,GIRLIES,GIRLON,GIRLONTOP,GIRLSGONEWILD,GITFACE,GLORYHOLE,GOATCX,GOATSE,GODAM,GODDAM,GODDAMMED,GODDAMMING,GODDAMN,GODDAMNDEST,GODDAMNED,GODDAMNEDEST,GODDAMNING,GODDAMNS,GODDAMS,GOK,GOKKUN,GOLDENSHOWER,GOODPOOP,GOOGIRL,GOOK,GOOLIE,GOOLIES,GOOLY,GOREGASM,GOY,GOYIM,GOYISH,GOYISHE,GOYS,GRINGA,GRINGAS,GRINGO,GRINGOS,GROUPSEX,GSPOT,G-SPOT,GUIDO,GURO,GYPO,HANDJOB,HAOLE,HAOLES,HARDASS,HARDASSES,HARDCORE,HARDN,HEBE,HEBES,HEEB,HENTAI,HESHE,HOMO,HOMOEROTIC,HONKEY,HONKEYS,HONKIE,HONKIES,HONKY,HOOKER,HORE,HORNIEST,HORNY,HORSESHIT,HORSESHITS,HOS,HOTCARL,HOTCHICK,HOTSEX,HOWTOKILL,HOWTOMURDER,HUGEFAT,HUMPING,HUNKEY,HUNKEYS,HUNKIE,HUNKIES,HUNKY,INCEST,INTERCOURSE,JACKASS,JACKASSES,JACKOFF,JAGOFF,JAILBAIT,JAILBAITS,JAP,JERKOFF,JESUIT,JESUITIC,JESUITICAL,JESUITICALLY,JESUITISM,JESUITISMS,JESUITRIES,JESUITRY,JESUITS,JEW,JEWBAG,JEWED,JEWING,JEWS,JIGABOO,JIGABOOS,JIGGABOO,JIGGERBOO,JISM,JISMS,JISS,JIVEASS,JIZZ,JIZZES,JOHNSON,JOHNSONS,JUGG,JUGGS,JUNGLEBUNNY,KAFFIR,KANAKA,KANAKAS,KAWK,KIKE,KIKES,KINBAKU,KINKSTER,KINKY,KNOBBING,KNOBEND,KRAUT,KYKE,LABIA,LEATHERRESTRAINT,LEATHERSTRAIGHTJACKET,LEMONPARTY,LES,LESBIAN,LESBO,LESBOS,LESES,LEZ,LEZZES,LEZZIE,LEZZIES,LEZZY,LIBBER,LIBBERS,LOLITA,LOVEMAK,LOVEMAKING,MAKEMECOME,MALESQUIRTING,MASOCHIST,MASTERBAT,MASTERBATE,MASTURBAT,MASTURBATE,MCFAGGET,MENAGEATROIS,MERDE,MERDES,MICK,MICKS,MILF,MINDFUCK,MINDFUCKS,MINGE,MISSIONARYPOSITION,MOFO,MOFOS,MOLEST,MOOF,MOTHERFUCKER,MOTHERFUCKERS,MOTHERFUCKING,MOUNDOFVENUS,MRHANDS,MUFF,MUFFDIV,MUFFDIVER,MUFFDIVING,MUFUGLY,MUNGING,MURDER,MUTHA,MUTHAS,NAMBLA,NANCE,NANCES,NANCIER,NANCIES,NANCIEST,NANCY,NAWASHI,NEGRO,NEONAZI,NIG NOG,NIGGA,NIGGER,NIGGERS,NIGLET,NIGNOG,NIMPHOMANIA,NITCHIE,NITCHIES,NONCE,NONPAPIST,NONPAPISTS,NOOB,NOOKIE,NOOKIES,NOOKY,NSFWIMAGES,NUMBNUT,NUMBNUTS,NUMBNUTSES,NUTACK,NUTSACK,NUTTER,NYMPHO,NYMPHOMANIA,OCTOPUSSY,OFAY,OFAYS,OMORASHI,ONECUPTWOGIRLS,ONEGUYONEJAR,ORGASIM,ORGASM,PAEDO,PAEDOPHILE,PAKI,PANCH,PAPISM,PAPISMS,PAPIST,PAPISTIC,PAPISTRIES,PAPISTRY,PAPISTS,PECKER,PECKERHEAD,PEDOBEAR,PEDOPHILE,PEEDO,PEGGING,PEPSI,PEPSIS,PHONESEX,PHUQ,PICANINNIES,PICANINNY,PICKANINNIES,PICKANINNY,PICKNEY,PICKNEYS,PIECEOFSHIT,PIMPIS,PISS,PISSANT,PISSANTS,PISSED,PISSER,PISSERS,PISSES,PISSHOLE,PISSHOLES,PISSIER,PISSIEST,PISSING,PISSPIG,PISSY,PLAYBOY,PLEASURECHEST,POLACK,POLACKS,POLESMOKER,POLLOCK,POM,POMMIE,POMMIES,POMMY,POMS,PONCEY,PONCIER,PONCIEST,PONCY,PONYPLAY,POO,POOED,POOFTAH,POOFTAHS,POOFTER,POOFTERS,POOING,POON,POONTANG,POONTANGS,POOP,POOP CHUTE,POOPCHUTE,POOS,POOVE,POOVES,POPERIES,POPERY,POPISH,POPISHLY,PORCHMONKEY,PORN,PORNO,PORNOGRAPHY,PRICK,PRINCEALBERTPIERCING,PROSTITUTE,PTHC,PUBE,PUBES,PUNAN,PUNANY,PUNTA,PUSSE,PUSSY,PUTO,QUEAF,QUEEF,QUIM,RACKOFF,RAGHEAD,RAGHEADS,RAGINGBONER,RAPE,RAPING,RAPIST,RECTAL,RECTUM,REDNECK,REDNECKS,REDSKIN,REDSKINS,RENOB,RETARD,REVERSECOWGIRL,RIMJAW,RIMJOB,RIMMING,ROSYPALM,ROSYPALMANDHER5SISTERS,RUSKI,RUSTYTROMBONE,S&M,S.O.B.,S_H_I_T,SADISM,SADIST,SANTORUM,SCAT,SCHLONG,SCHLONGS,SCHTUP,SCHTUPPED,SCHTUPPING,SCHTUPS,SCHVARTZE,SCHVARTZES,SCHWARTZE,SCHWARTZES,SCISSORING,SCREWING,SCROAT,SCROTE,SCROTUM,SCRUBBER,SCUMBAG,SECHS,SECKS,SEMEN,SEXO,SH1T,SHAGG,SHAGGER,SHAGGERS,SHAT,SHAVEDBEAVER,SHAVEDPUSSY,SHEENEY,SHEENEYS,SHEENIE,SHEENIES,SHEGETZ,SHEMALE,SHEMALES,SHIBARI,SHICKSA,SHICKSAS,SHIKSA,SHIKSAS,SHIKSE,SHIKSEH,SHIKSEHS,SHIKSES,SHINOLA,SHINOLAS,SHIT,SHITBAG,SHITBAGS,SHITBLIMP,SHITCAN,SHITCANNED,SHITCANNING,SHITCANS,SHITE,SHITES,SHITFACE,SHITFACED,SHITFACES,SHITHEAD,SHITHEADS,SHITHEEL,SHITHEELS,SHITHOLE,SHITHOLES,SHITLESS,SHITLIST,SHITLISTS,SHITLOAD,SHITLOADS,SHITS,SHITTED,SHITTER,SHITTERS,SHITTIER,SHITTIEST,SHITTING,SHITTY,SHITWORK,SHITWORKS,SHKOTZIM,SHLONG,SHLONGS,SHOTA,SHRIMPING,SHTUP,SHTUPPED,SHTUPPING,SHTUPS,SHUM,SHVARTZE,SHVARTZES,SKANK,SKANKY,SKEET,SKIMO,SKIMOS,SLANTEYE,SLUT,SLUTS,SMEG,SMEGMA,SMUT,SODOM,SODOMIZE,SODOMY,SPAC,SPAZ,SPAZZ,SPAZZES,SPERG,SPERM,SPIC,SPICK,SPICKS,SPICS,SPIK,SPIKS,SPLOOGE,SPLOOGEMOOSE,SPOOGE,SPOOK,SPREADLEGS,SPUNK,STIFFIE,STIFFIES,STIFFY,STRAP ON,STRAPON,STRAPPADO,STRIPCLUB,STYLEDOGGY,SUCKHOLE,SUCKHOLED,SUCKHOLES,SUCKHOLING,SUICIDEGIRLS,SULTRYWOMEN,SUMBITCH,SUMBITCHES,SUPERBITCH,SUPERBITCHES,SWASTIKA,TAINTEDLOVE,TARD,TASTEMY,TEABAG,TEABAGGING,TEEZ,TERRORIST,THREESOME,THROATING,TIEDUP,TIGHTWHITE,TITT,TITTIES,TITTY,TITWANK,TOMMED,TOMMING,TONGUEINA,TOSSER,TOWELHEAD,TRANNY,TRIBADISM,TUBGIRL,TURD,TURDS,TUSHY,TWAT,TWATS,TWINK,TWINKIE,TWINKY,TWOGIRLSONECUP,TWUNT,UPSKIRT,URETHRAPLAY,UROPHILIA,VAG,VAGINA,VAGINAL,VENDU,VENDUE,VENDUES,VENDUS,VENUSMOUND,VIBRATOR,VIOLETWAND,VORAREPHILIA,VULVA,WANG,WANK,WANKED,WANKER,WANKERS,WANKING,WANKS,WETBACK,WETBACKS,WETDREAM,WHITEPOWER,WHITEY,WHITEYS,WHITIES,WHITY,WHOR,WHORE,WILLIE,WILLIES,WOG,WOGGISH,WOGS,WOP,WOPS,WRAPPINGMEN,WRAT,WRINKLEDSTARFISH,WRINKLIE,WRINKLIES,WTF,XX,XXX,YAOI,YELLOWSHOWERS,YID,YIDS,YIFFY,ZOOPHILIA";
    private string[] badWords; 

    public class SpelledWord
    {
        public LinkedList<Tile> tiles; 
        public string word;
        public int score;
    }

    public Dawg<bool> DawgObj
    {
        get { return dawg; }
        set { dawg = value; }
    }

    // Use this for initialization
	void Start ()
	{
        //BuildDawg();
        //BuildPowrWordDawg();

	    badWords = badWordCsv.Split(',');
	    for (int index = 0; index < badWords.Length; index++)
	    {
	        var badWord = badWords[index];
	        badWords[index] = badWord.ToLower();
	    }

	    StartCoroutine(LoadWordList());
    }

    public bool IsBadWord(string word)
    {
        return badWords.Contains(word.ToLower());
    }

    private void BuildDawg()
    {
        var dawgBuilder = new DawgBuilder<bool>();
        var filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "sowpods.txt");

        var result = "";
        result = System.IO.File.ReadAllText(filePath);


        var lines = result.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

        var lineLen = lines.Length;
        for (int index = 0; index < lineLen; index++)
        {
            var s = lines[index];
            if (s.Length > 2)
                dawgBuilder.Insert(s, true);
        }

        var dawg = dawgBuilder.BuildDawg();

        dawg.SaveTo(File.Create(System.IO.Path.Combine(Application.streamingAssetsPath, "DAWG.bytes")), (writer, value) => writer.Write(value)); // explained below
    }
    
    private IEnumerator LoadWordList()
    {
        var filePath = System.IO.Path.Combine(Application.streamingAssetsPath, wordListFileName);

        byte[] result;
        if (filePath.Contains("://"))
        {
            WWW www = new WWW(filePath);
            yield return www;
            result = www.bytes;
        }
        else
            result = System.IO.File.ReadAllBytes(filePath);

        DawgObj = Dawg<bool>.Load(new MemoryStream(result), reader => reader.ReadBoolean());           // explained below
    }
    


    public bool IsWordValid(string word)
    {
        if (debugValid && Application.isEditor)
            return true;

        word = word.ToUpper();
        return DawgObj[word];
    }

    internal IEnumerator GetLongestWord(Action<string> callback)
    {
        var yoDawn = DawgObj;

        var letters = new List<char>();

        var tiles = BoardManager.Instance.Tiles(true, true);
        for (int index = 0; index < tiles.Count; index++)
        {
            var tile = tiles[index];
            letters.Add(tile.letterDefinition.letter.ToUpper()[0]);
        }

        var prefixes = new HashSet<string>();

        for (int i = 0; i < letters.Count; i++)
        {
            var firstLetter = letters[i];

            for (int x = 0; x < letters.Count; x++)
            {
                if (x == i)
                    continue;

                var secondLetter = letters[x];

                var prefix = firstLetter.ToString() + secondLetter;
                if(!prefixes.Contains(prefix))
                    prefixes.Add(prefix);
            }
        }

        yield return new WaitForEndOfFrame();

        var longestLength = 0;
        var longestWord = "";
        var prefixCount = prefixes.Count;

        for (int i = 0; i < prefixCount; i++)
        {
            var prefix = prefixes.ElementAt(i);

            var words = yoDawn.MatchPrefix(prefix).ToList();
            for (int index = 0; index < words.Count; index++)
            {
                Debug.Log(words.Count);
                var entry = words[index];
                var word = entry.Key;

                // Ignore words that are shorter than our existing longest
                if (word.Length <= longestLength)
                    continue;


                var availableLetters = new List<char>(letters);
                bool canSpell = true;
                for (int index1 = 0; index1 < word.Length; index1++)
                {
                    var wordLetter = word[index1];
                    var indexOfLetter = availableLetters.IndexOf(wordLetter);
                    if (indexOfLetter == -1)
                    {
                        canSpell = false;
                        break;
                    }

                    availableLetters.RemoveAt(indexOfLetter);
                }

                if (canSpell)
                {
                    yield return new WaitForEndOfFrame();
                    longestLength = word.Length;
                    longestWord = word;
                }
            }
        }

        callback(longestWord);
    }
}
