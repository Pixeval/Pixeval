// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebApiClientCore.Exceptions;

namespace Pixeval.Utilities;

internal static partial class NetworkExceptionFormatter
{
    private const int MaxResponseBodyLength = 4096;

    private static readonly HashSet<string> _SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Api-Key",
        "Authorization",
        "Cookie",
        "Proxy-Authorization",
        "Set-Cookie",
        "X-Api-Key"
    };

    public static Task<string?> TryFormatAsync(Exception exception, CancellationToken token = default)
    {
        try
        {
            return FormatAsync(exception, token);
        }
        catch (Exception e)
        {
            return Task.FromResult($"\tNetwork:\n\t\t<failed to format: {e.GetType().FullName}: {e.Message}>\n");
        }
    }

    private static async Task<string?> FormatAsync(Exception exception, CancellationToken token)
    {
        HttpRequestException? httpRequestException = null;
        SocketException? socketException = null;
        WebException? webException = null;
        AuthenticationException? authenticationException = null;
        HttpRequestMessage? request = null;
        HttpResponseMessage? response = null;

        for (var current = exception; current is not null; current = current.InnerException)
        {
            switch (current)
            {
                case HttpRequestException value:
                    httpRequestException ??= value;
                    break;
                case SocketException value:
                    socketException ??= value;
                    break;
                case WebException value:
                    webException ??= value;
                    break;
                case AuthenticationException value:
                    authenticationException ??= value;
                    break;
                case ApiResponseStatusException value:
                    response ??= value.ResponseMessage;
                    request ??= value.ResponseMessage.RequestMessage;
                    break;
                case ApiReturnNotSupportedExteption value:
                    response ??= value.Context.HttpContext.ResponseMessage;
                    request ??= value.Context.HttpContext.RequestMessage;
                    break;
            }
        }

        request ??= response?.RequestMessage;
        if (httpRequestException is null
            && socketException is null
            && webException is null
            && authenticationException is null
            && request is null
            && response is null)
            return null;

        var builder = new StringBuilder();
        _ = builder.AppendLine("\tNetwork:");

        if (httpRequestException is not null)
        {
            _ = builder.AppendLine($"\t\tHttpRequestError: {httpRequestException.HttpRequestError}");
            if (httpRequestException.StatusCode is { } statusCode)
                _ = builder.AppendLine($"\t\tExceptionStatusCode: {(int) statusCode} {statusCode}");
        }

        if (socketException is not null)
        {
            _ = builder.AppendLine($"\t\tSocketErrorCode: {socketException.SocketErrorCode}");
            _ = builder.AppendLine($"\t\tNativeErrorCode: {socketException.NativeErrorCode}");
        }

        if (webException is not null)
            _ = builder.AppendLine($"\t\tWebExceptionStatus: {webException.Status}");

        if (authenticationException is not null)
            _ = builder.AppendLine($"\t\tTlsError: {authenticationException.Message}");

        if (request is not null)
            AppendRequest(builder, request);

        if (response is not null)
            await AppendResponseAsync(builder, response, token).ConfigureAwait(false);

        return builder.ToString();
    }

    private static void AppendRequest(StringBuilder builder, HttpRequestMessage request)
    {
        _ = builder.AppendLine("\t\tRequest:");
        _ = builder.AppendLine($"\t\t\tMethod: {request.Method}");
        _ = builder.AppendLine($"\t\t\tUri: {GetSafeUri(request.RequestUri)}");
        _ = builder.AppendLine($"\t\t\tVersion: {request.Version}");
        _ = builder.AppendLine($"\t\t\tVersionPolicy: {request.VersionPolicy}");
        AppendHeaders(builder, request.Headers, request.Content?.Headers, 3);
    }

    private static async Task AppendResponseAsync(StringBuilder builder, HttpResponseMessage response, CancellationToken token)
    {
        _ = builder.AppendLine("\t\tResponse:");
        _ = builder.AppendLine($"\t\t\tStatusCode: {(int) response.StatusCode} {response.StatusCode}");
        _ = builder.AppendLine($"\t\t\tReasonPhrase: {response.ReasonPhrase}");
        _ = builder.AppendLine($"\t\t\tVersion: {response.Version}");
        AppendHeaders(builder, response.Headers, response.Content?.Headers, 3);

        if (response.Content is null)
            return;

        _ = builder.AppendLine("\t\t\tBody:");
        try
        {
            var body = await ReadResponseBodyAsync(response.Content, token).ConfigureAwait(false);
            _ = builder.Append("\t\t\t\t");
            _ = builder.AppendLine(body.ReplaceLineEndings(Environment.NewLine + "\t\t\t\t"));
        }
        catch (Exception e)
        {
            _ = builder.AppendLine($"\t\t\t\t<failed to read: {e.GetType().FullName}: {e.Message}>");
        }
    }

    private static void AppendHeaders(StringBuilder builder, HttpHeaders headers, HttpHeaders? contentHeaders, int indent)
    {
        _ = builder.AppendLine($"{Indent(indent)}Headers:");
        AppendHeaders(builder, headers, indent + 1);
        if (contentHeaders is not null)
            AppendHeaders(builder, contentHeaders, indent + 1);
    }

    private static void AppendHeaders(StringBuilder builder, HttpHeaders headers, int indent)
    {
        foreach (var header in headers)
        {
            var value = GetSafeHeaderValue(header.Key, string.Join(", ", header.Value));
            _ = builder.AppendLine($"{Indent(indent)}{header.Key}: {value}");
        }
    }

    private static async Task<string> ReadResponseBodyAsync(HttpContent content, CancellationToken token)
    {
        var mediaType = content.Headers.ContentType?.MediaType;
        if (!IsTextContent(mediaType))
            return $"<omitted non-text content: {mediaType ?? "unknown"}>";

        var stream = await content.ReadAsStreamAsync(token).ConfigureAwait(false);
        using var reader = new StreamReader(stream, GetEncoding(content.Headers.ContentType?.CharSet), true, leaveOpen: true);
        var buffer = new char[MaxResponseBodyLength + 1];
        var length = 0;
        while (length < buffer.Length)
        {
            var read = await reader.ReadAsync(buffer.AsMemory(length, buffer.Length - length), token).ConfigureAwait(false);
            if (read is 0)
                break;
            length += read;
        }

        var truncated = length > MaxResponseBodyLength;
        var body = new string(buffer, 0, Math.Min(length, MaxResponseBodyLength));
        body = SensitiveValueRegex.Replace(body, "${prefix}<redacted>");
        return truncated ? body + "<truncated>" : body;
    }

    private static bool IsTextContent(string? mediaType) =>
        mediaType is null
        || mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
        || mediaType.Contains("json", StringComparison.OrdinalIgnoreCase)
        || mediaType.Contains("xml", StringComparison.OrdinalIgnoreCase)
        || mediaType.Contains("javascript", StringComparison.OrdinalIgnoreCase);

    private static Encoding GetEncoding(string? charset)
    {
        if (string.IsNullOrWhiteSpace(charset))
            return Encoding.UTF8;

        try
        {
            return Encoding.GetEncoding(charset.Trim('"'));
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
    }

    private static string? GetSafeUri(Uri? uri) => uri?.GetLeftPart(UriPartial.Path);

    private static string GetSafeHeaderValue(string name, string value)
    {
        if (_SensitiveHeaders.Contains(name))
            return "<redacted>";

        if ((name.Equals("Location", StringComparison.OrdinalIgnoreCase)
             || name.Equals("Referer", StringComparison.OrdinalIgnoreCase))
            && Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return GetSafeUri(uri) ?? "";

        return SensitiveValueRegex.Replace(value, "${prefix}<redacted>");
    }

    private static string Indent(int indent) => new('\t', indent);

    [GeneratedRegex("(?<prefix>[\\\"']?(?:access_token|refresh_token|id_token|client_secret|code_verifier|authorization|proxy-authorization|cookie|set-cookie|x-api-key|api-key)[\\\"']?\\s*[:=]\\s*[\\\"']?)[^\\\"',&}\\]\\r\\n]+", RegexOptions.IgnoreCase)]
    private static partial Regex SensitiveValueRegex { get; }
}
