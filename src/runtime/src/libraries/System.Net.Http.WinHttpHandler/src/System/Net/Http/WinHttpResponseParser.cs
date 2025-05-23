// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;

using SafeWinHttpHandle = Interop.WinHttp.SafeWinHttpHandle;

namespace System.Net.Http
{
    internal static class WinHttpResponseParser
    {
        public static HttpResponseMessage CreateResponseMessage(WinHttpRequestState state)
        {
            HttpRequestMessage? request = state.RequestMessage;
            SafeWinHttpHandle? requestHandle = state.RequestHandle;
            Debug.Assert(request != null);
            Debug.Assert(requestHandle != null);

            var response = new HttpResponseMessage();
            bool stripEncodingHeaders = false;

            // Create a single buffer to use for all subsequent WinHttpQueryHeaders string interop calls.
            // This buffer is the length needed for WINHTTP_QUERY_RAW_HEADERS_CRLF, which includes the status line
            // and all headers separated by CRLF, so it should be large enough for any individual status line or header queries.
            int bufferLength = GetResponseHeaderCharBufferLength(requestHandle, isTrailingHeaders: false);
            char[] buffer = ArrayPool<char>.Shared.Rent(bufferLength);
            try
            {
                // Get HTTP version, status code, reason phrase from the response headers.

                if (IsResponseHttp2(requestHandle))
                {
                    response.Version = WinHttpHandler.HttpVersion20;
                }
                else
                {
                    int versionLength = GetResponseHeader(requestHandle, Interop.WinHttp.WINHTTP_QUERY_VERSION, buffer);
                    ReadOnlySpan<char> versionSpan = buffer.AsSpan(0, versionLength);
                    response.Version =
                        versionSpan.Equals("HTTP/1.1".AsSpan(), StringComparison.OrdinalIgnoreCase) ? HttpVersion.Version11 :
                        versionSpan.Equals("HTTP/1.0".AsSpan(), StringComparison.OrdinalIgnoreCase) ? HttpVersion.Version10 :
                        WinHttpHandler.HttpVersionUnknown;
                }

                response.StatusCode = (HttpStatusCode)GetResponseHeaderNumberInfo(
                    requestHandle,
                    Interop.WinHttp.WINHTTP_QUERY_STATUS_CODE);

                int reasonPhraseLength = GetResponseHeader(requestHandle, Interop.WinHttp.WINHTTP_QUERY_STATUS_TEXT, buffer);
                response.ReasonPhrase = reasonPhraseLength > 0 ?
                    GetReasonPhrase(response.StatusCode, buffer, reasonPhraseLength) :
                    string.Empty;

                // Create response stream and wrap it in a StreamContent object.
                var responseStream = new WinHttpResponseStream(requestHandle, state, response);
                state.RequestHandle = null; // ownership successfully transferred to WinHttpResponseStream.
                response.Content = new NoWriteNoSeekStreamContent(responseStream);
                response.RequestMessage = request;

                // Parse raw response headers and place them into response message.
                ParseResponseHeaders(requestHandle, response, buffer, stripEncodingHeaders);

                if (response.RequestMessage.Method != HttpMethod.Head)
                {
                    state.ExpectedBytesToRead = response.Content.Headers.ContentLength;
                }

                return response;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Returns the first header or throws if the header isn't found.
        /// </summary>
        public static uint GetResponseHeaderNumberInfo(SafeWinHttpHandle requestHandle, uint infoLevel)
        {
            uint result = 0;
            uint resultSize = sizeof(uint);

            if (!Interop.WinHttp.WinHttpQueryHeaders(
                requestHandle,
                infoLevel | Interop.WinHttp.WINHTTP_QUERY_FLAG_NUMBER,
                Interop.WinHttp.WINHTTP_HEADER_NAME_BY_INDEX,
                ref result,
                ref resultSize,
                IntPtr.Zero))
            {
                WinHttpException.ThrowExceptionUsingLastError(nameof(Interop.WinHttp.WinHttpQueryHeaders));
            }

            return result;
        }

        public static unsafe bool GetResponseHeader(
            SafeWinHttpHandle requestHandle,
            uint infoLevel,
            ref char[]? buffer,
            ref uint index,
            [NotNullWhen(true)] out string? headerValue)
        {
            const int StackLimit = 128;

            Debug.Assert(buffer == null || (buffer != null && buffer.Length > StackLimit));

            int bufferLength;
            uint originalIndex = index;

            if (buffer == null)
            {
                bufferLength = StackLimit;
                char* pBuffer = stackalloc char[bufferLength];
                if (QueryHeaders(requestHandle, infoLevel, pBuffer, ref bufferLength, ref index))
                {
                    headerValue = new string(pBuffer, 0, bufferLength);
                    return true;
                }
            }
            else
            {
                bufferLength = buffer.Length;
                fixed (char* pBuffer = &buffer[0])
                {
                    if (QueryHeaders(requestHandle, infoLevel, pBuffer, ref bufferLength, ref index))
                    {
                        headerValue = new string(pBuffer, 0, bufferLength);
                        return true;
                    }
                }
            }

            int lastError = Marshal.GetLastWin32Error();

            if (lastError == Interop.WinHttp.ERROR_WINHTTP_HEADER_NOT_FOUND)
            {
                headerValue = null;
                return false;
            }

            if (lastError == Interop.WinHttp.ERROR_INSUFFICIENT_BUFFER)
            {
                // WinHttpQueryHeaders may advance the index even when it fails due to insufficient buffer,
                // so we set the index back to its original value so we can retry retrieving the same
                // index again with a larger buffer.
                index = originalIndex;

                buffer = new char[bufferLength];
                fixed (char* pBuffer = &buffer[0])
                {
                    if (QueryHeaders(requestHandle, infoLevel, pBuffer, ref bufferLength, ref index))
                    {
                        headerValue = new string(pBuffer, 0, bufferLength);
                        return true;
                    }
                }

                lastError = Marshal.GetLastWin32Error();
            }

            throw WinHttpException.CreateExceptionUsingError(lastError, nameof(Interop.WinHttp.WinHttpQueryHeaders));
        }

        /// <summary>
        /// Fills the buffer with the header value and returns the length, or returns 0 if the header isn't found.
        /// </summary>
        private static unsafe int GetResponseHeader(SafeWinHttpHandle requestHandle, uint infoLevel, char[] buffer)
        {
            Debug.Assert(buffer != null, "buffer must not be null.");
            Debug.Assert(buffer.Length > 0, "buffer must not be empty.");

            int bufferLength = buffer.Length;
            uint index = 0;

            fixed (char* pBuffer = &buffer[0])
            {
                if (!QueryHeaders(requestHandle, infoLevel, pBuffer, ref bufferLength, ref index))
                {
                    int lastError = Marshal.GetLastWin32Error();

                    if (lastError == Interop.WinHttp.ERROR_WINHTTP_HEADER_NOT_FOUND)
                    {
                        return 0;
                    }

                    Debug.Assert(lastError != Interop.WinHttp.ERROR_INSUFFICIENT_BUFFER, "buffer must be of sufficient size.");

                    throw WinHttpException.CreateExceptionUsingError(lastError, nameof(Interop.WinHttp.WinHttpQueryHeaders));
                }
            }

            return bufferLength;
        }

        /// <summary>
        /// Returns the size of the char array buffer.
        /// </summary>
        public static unsafe int GetResponseHeaderCharBufferLength(SafeWinHttpHandle requestHandle, bool isTrailingHeaders)
        {
            char* buffer = null;
            int bufferLength = 0;
            uint index = 0;

            uint infoLevel = Interop.WinHttp.WINHTTP_QUERY_RAW_HEADERS_CRLF;
            if (isTrailingHeaders)
            {
                infoLevel |= Interop.WinHttp.WINHTTP_QUERY_FLAG_TRAILERS;
            }

            if (!QueryHeaders(requestHandle, infoLevel, buffer, ref bufferLength, ref index))
            {
                int lastError = Marshal.GetLastWin32Error();

                Debug.Assert(isTrailingHeaders || lastError != Interop.WinHttp.ERROR_WINHTTP_HEADER_NOT_FOUND);

                if (lastError != Interop.WinHttp.ERROR_INSUFFICIENT_BUFFER &&
                    (!isTrailingHeaders || lastError != Interop.WinHttp.ERROR_WINHTTP_HEADER_NOT_FOUND))
                {
                    throw WinHttpException.CreateExceptionUsingError(lastError, nameof(Interop.WinHttp.WinHttpQueryHeaders));
                }
            }

            return bufferLength;
        }

        private static unsafe bool QueryHeaders(
            SafeWinHttpHandle requestHandle,
            uint infoLevel,
            char* buffer,
            ref int bufferLength,
            ref uint index)
        {
            Debug.Assert(bufferLength >= 0, "bufferLength must not be negative.");

            // Convert the char buffer length to the length in bytes.
            uint bufferLengthInBytes = (uint)bufferLength * sizeof(char);

            // The WinHttpQueryHeaders buffer length is in bytes,
            // but the API actually returns Unicode characters.
            bool result = Interop.WinHttp.WinHttpQueryHeaders(
                requestHandle,
                infoLevel,
                Interop.WinHttp.WINHTTP_HEADER_NAME_BY_INDEX,
                new IntPtr(buffer),
                ref bufferLengthInBytes,
                ref index);

            // Convert the byte buffer length back to the length in chars.
            bufferLength = (int)bufferLengthInBytes / sizeof(char);

            return result;
        }

        private static string GetReasonPhrase(HttpStatusCode statusCode, char[] buffer, int bufferLength)
        {
            CharArrayHelpers.DebugAssertArrayInputs(buffer, 0, bufferLength);
            Debug.Assert(bufferLength > 0);

            // If it's a known reason phrase, use the known reason phrase instead of allocating a new string.

            string? knownReasonPhrase = HttpStatusDescription.Get(statusCode);

            return (knownReasonPhrase != null && knownReasonPhrase.AsSpan().SequenceEqual(buffer.AsSpan(0, bufferLength))) ?
                knownReasonPhrase :
                new string(buffer, 0, bufferLength);
        }

        private static void ParseResponseHeaders(
            SafeWinHttpHandle requestHandle,
            HttpResponseMessage response,
            char[] buffer,
            bool stripEncodingHeaders)
        {
            HttpResponseHeaders responseHeaders = response.Headers;
            HttpContentHeaders contentHeaders = response.Content.Headers;

            int bufferLength = GetResponseHeader(
                requestHandle,
                Interop.WinHttp.WINHTTP_QUERY_RAW_HEADERS_CRLF,
                buffer);

            var reader = new WinHttpResponseHeaderReader(buffer, 0, bufferLength);

            // Skip the first line which contains status code, etc. information that we already parsed.
            reader.ReadLine();

            // Parse the array of headers and split them between Content headers and Response headers.
            while (reader.ReadHeader(out string? headerName, out string? headerValue))
            {
                if (!responseHeaders.TryAddWithoutValidation(headerName, headerValue))
                {
                    if (stripEncodingHeaders)
                    {
                        // Remove Content-Length and Content-Encoding headers if we are
                        // decompressing the response stream in the handler (due to
                        // WINHTTP not supporting it in a particular downlevel platform).
                        // This matches the behavior of WINHTTP when it does decompression itself.
                        if (string.Equals(HttpKnownHeaderNames.ContentLength, headerName, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(HttpKnownHeaderNames.ContentEncoding, headerName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }

                    contentHeaders.TryAddWithoutValidation(headerName, headerValue);
                }
            }
        }

        public static void ParseResponseTrailers(
            SafeWinHttpHandle requestHandle,
            HttpResponseMessage response,
            char[] buffer)
        {
            HttpHeaders responseTrailers = WinHttpTrailersHelper.GetResponseTrailers(response);

            int bufferLength = GetResponseHeader(
                requestHandle,
                Interop.WinHttp.WINHTTP_QUERY_RAW_HEADERS_CRLF | Interop.WinHttp.WINHTTP_QUERY_FLAG_TRAILERS,
                buffer);

            var reader = new WinHttpResponseHeaderReader(buffer, 0, bufferLength);

            // Parse the array of headers and split them between Content headers and Response headers.
            while (reader.ReadHeader(out string? headerName, out string? headerValue))
            {
                responseTrailers.TryAddWithoutValidation(headerName, headerValue);
            }
        }

        private static bool IsResponseHttp2(SafeWinHttpHandle requestHandle)
        {
            uint data = 0;
            uint dataSize = sizeof(uint);

            if (Interop.WinHttp.WinHttpQueryOption(
                requestHandle,
                Interop.WinHttp.WINHTTP_OPTION_HTTP_PROTOCOL_USED,
                ref data,
                ref dataSize))
            {
                if ((data & Interop.WinHttp.WINHTTP_PROTOCOL_FLAG_HTTP2) != 0)
                {
                    if (NetEventSource.Log.IsEnabled()) NetEventSource.Info(requestHandle, nameof(Interop.WinHttp.WINHTTP_PROTOCOL_FLAG_HTTP2));
                    return true;
                }
            }

            return false;
        }
    }
}
