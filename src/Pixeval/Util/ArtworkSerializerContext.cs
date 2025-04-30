using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Imouto.BooruParser;
using Mako.Model;
using Misaki;

namespace Pixeval.Util;

public class ArtworkSerializerTable
{
    public static FrozenDictionary<string, Type> ArtworkTypeNamesTable { get; } = new Dictionary<string, Type>
    {
        [typeof(Illustration).FullName!] = typeof(Illustration),
        [typeof(Novel).FullName!] = typeof(Novel),
        [typeof(Post).FullName!] = typeof(Post)
    }.ToFrozenDictionary();

    public static FrozenDictionary<string, Func<string, ISerializable>> ArtworkTypeMethodsTable { get; } = new Dictionary<string, Func<string, ISerializable>>
    {
        [typeof(Illustration).FullName!] = Illustration.Deserialize,
        [typeof(Novel).FullName!] = Novel.Deserialize,
        [typeof(Post).FullName!] = Post.Deserialize
    }.ToFrozenDictionary();
}
