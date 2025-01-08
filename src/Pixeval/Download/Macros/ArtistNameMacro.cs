// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Controls;
using Pixeval.Download.MacroParser;
using Pixeval.Util.IO;
using Pixeval.Utilities;

namespace Pixeval.Download.Macros;

[MetaPathMacro<IWorkViewModel>]
public class ArtistNameMacro : ITransducer<IWorkViewModel>
{
    public string Name => "artist_name";

    public string Substitute(IWorkViewModel context)
    {
        return context.User.Name.Let(IoHelper.NormalizePathSegment);
    }
}
