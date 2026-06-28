// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Pixeval.Models.SauceNao;

/// <summary>
/// <code>
/// mask		index	server(s)	name (*disabled)
/// -------------------------------------------------------------------
/// 0x1         	#0	11		h-mags
/// 0x2         	#1	9		h-anime*
/// 0x4         	#2	10		hcg
/// 0x8         	#3	12		ddb-objects*
/// 0x10        	#4	13		ddb-samples*
/// 0x20        	#5	1,4,21,36	pixiv
/// 0x40        	#6	2		pixivhistorical
/// 0x80        	#7	3		anime*
/// 0x100       	#8	5		seiga_illust - nico nico seiga
/// 0x200       	#9	6		danbooru
/// 0x400       	#10	7		drawr
/// 0x800       	#11	8		nijie
/// 0x1000      	#12	6		yande.re
/// 0x2000      	#13	15		animeop*
/// 0x4000      	#14	16		IMDb*
/// 0x8000      	#15	17		Shutterstock*
/// 0x10000     	#16	18		FAKKU
/// 0x20000     	#18	20		H-MISC (nhentai)
/// 0x40000     	#19	22		2d_market
/// 0x80000     	#20	23		medibang
/// 0x100000    	#21	24,34		Anime
/// 0x200000    	#22	25		H-Anime
/// 0x400000    	#23	26		Movies
/// 0x800000    	#24	27		Shows
/// 0x1000000   	#25	6		gelbooru
/// 0x2000000   	#26	6		konachan
/// 0x4000000   	#27	6		sankaku
/// 0x8000000   	#28	6		anime-pictures
/// 0x10000000  	#29	6		e621
/// 0x20000000  	#30	6		idol complex
/// 0x40000000  	#31	28		bcy illust
/// 0x80000000  	#32	29		bcy cosplay
/// 0x100000000 	#33	30		portalgraphics
/// 0x200000000 	#34	31,38		dA
/// 0x400000000 	#35	32		pawoo
/// 0x800000000 	#36	33		madokami
/// 0x1000000000	#37	35,43		mangadex
/// 0x2000000000	#38	37		H-Misc (ehentai)
/// 0x4000000000	#39	39		ArtStation
/// 0x8000000000	#40	40		FurAffinity
/// 0x10000000000	#41	41		Twitter
/// 0x20000000000	#42	42		Furry Network
/// 0x40000000000	#43	44		Kemono
/// 0x80000000000	#44	45		Skeb
/// </code>
/// </summary>
/// <remarks>
/// <seealso href="https://saucenao.com/tools/examples/api/index_details.txt"/>
/// </remarks>
[Flags]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "CommentTypo")]
public enum SauceNaoIndex : long
{
    HMags = 0x1,
    // HAnime = 0x2,
    Hcg = 0x4,
    // DdbObjects = 0x8,
    // DdbSamples = 0x10,
    Pixiv = 0x20,
    PixivHistorical = 0x40,
    // Anime = 0x80,
    
    /// <summary>
    /// nico nico seiga
    /// </summary>
    SeigaIllust = 0x100,
    Danbooru = 0x200,
    Drawr = 0x400,
    Nijie = 0x800,
    YandeRe = 0x1000,
    // AnimeOp = 0x2000,
    // ImDb = 0x4000,
    // Shutterstock = 0x8000,
    Fakku = 0x10000,
    NHentai = 0x20000,
    _2DMarket = 0x40000,
    Medibang = 0x80000,
    Anime = 0x100000,
    HAnime = 0x200000,
    Movies = 0x400000,
    Shows = 0x800000,
    Gelbooru = 0x1000000,
    Konachan = 0x2000000,
    Sankaku = 0x4000000,
    AnimePictures = 0x8000000,
    E621 = 0x10000000,
    IdolComplex = 0x20000000,
    BcyIllust = 0x40000000,
    BcyCosplay = 0x80000000,
    PortalGraphics = 0x100000000,
    Da = 0x200000000,
    Pawoo = 0x400000000,
    Madokami = 0x800000000,
    Mangadex = 0x1000000000,
    EHentai = 0x2000000000,
    ArtStation = 0x4000000000,
    FurAffinity = 0x8000000000,
    Twitter = 0x10000000000,
    FurryNetwork = 0x20000000000,
    Kemono = 0x40000000000,
    Skeb = 0x80000000000,
    All = 999
}
