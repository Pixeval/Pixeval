using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using AutoSettingsPage.Models;

namespace Pixeval.Settings.Models;

public partial class IPSetSettingsEntry<TSettings>(
    TSettings settings,
    Expression<Func<TSettings, ObservableCollection<string>>> property)
    : CollectionSettingsEntry<TSettings, string>(settings, property);
