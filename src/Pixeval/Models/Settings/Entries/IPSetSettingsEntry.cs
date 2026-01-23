using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using AutoSettingsPage.Models;

namespace Pixeval.Models.Settings.Entries;

public class IPSetSettingsEntry<TSettings>(
    TSettings settings,
    Expression<Func<TSettings, ObservableCollection<string>>> property)
    : CollectionSettingsEntry<TSettings, string>(settings, property);
