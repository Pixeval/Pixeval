using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mako.Model;
using Pixeval.Util;

namespace Pixeval.Controls;

public partial class PixivIllustrationItemViewModel(Illustration illustration) : IllustrationItemViewModel(illustration)
{
    private new Illustration Entry => (Illustration) base.Entry;

    protected override Task<bool> SetBookmarkAsync(bool privately = false, IEnumerable<string>? tags = null) => MakoHelper.SetIllustrationBookmarkAsync(Entry, privately, tags);

    public override Uri PixEzUri => MakoHelper.GenerateIllustrationPixEzUri(Entry.Id);
}
