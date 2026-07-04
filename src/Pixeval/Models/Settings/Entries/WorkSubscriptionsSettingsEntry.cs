// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq.Expressions;
using AutoSettingsPage.Models;
using Pixeval.AppManagement;

namespace Pixeval.Models.Settings.Entries;

public class WorkSubscriptionsSettingsEntry(Expression<Func<DownloadSettingsGroup, byte>> expression)
    : ObservableSettingsEntry(expression);
