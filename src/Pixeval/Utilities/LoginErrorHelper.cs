// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using Pixeval.AppManagement;
using Pixeval.I18N;
using SQLite;

namespace Pixeval.Utilities;

internal static class LoginErrorHelper
{
    public static string GetMessage(Exception exception) => IsStorageFailure(exception)
        ? I18NManager.GetResource(LoginPageResources.StorageFailureFormatted,
            AppInfo.ApplicationFolderPath, exception.GetBaseException().Message)
        : exception.GetBaseException().Message;

    internal static bool IsStorageFailure(Exception exception)
    {
        var hasIoFailure = false;
        var hasNetworkFailure = false;
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is SQLiteException or UnauthorizedAccessException)
                return true;
            hasIoFailure |= current is IOException;
            hasNetworkFailure |= current is HttpRequestException or HttpIOException or SocketException or AuthenticationException;
        }

        return hasIoFailure && !hasNetworkFailure;
    }
}
