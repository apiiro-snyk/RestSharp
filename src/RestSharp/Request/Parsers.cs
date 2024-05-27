// Copyright (c) .NET Foundation and Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Text;
using System.Web;

namespace RestSharp;

static class Parsers {
    // ReSharper disable once CognitiveComplexity
    public static IEnumerable<KeyValuePair<string, string?>> ParseQueryString(string query, Encoding encoding) {
        Ensure.NotNull(query, nameof(query));
        Ensure.NotNull(encoding, nameof(encoding));
        var length      = query.Length;
        var startIndex1 = query[0] == '?' ? 1 : 0;

        if (length == startIndex1)
            yield break;

        while (startIndex1 <= length) {
            var startIndex2 = -1;
            var num         = -1;

            for (var index = startIndex1; index < length; ++index) {
                if (startIndex2 == -1 && query[index] == '=')
                    startIndex2 = index + 1;
                else if (query[index] == '&') {
                    num = index;
                    break;
                }
            }

            string? name;

            if (startIndex2 == -1) {
                name        = null;
                startIndex2 = startIndex1;
            }
            else
                name = HttpUtility.UrlDecode(query.Substring(startIndex1, startIndex2 - startIndex1 - 1), encoding);

            if (num < 0)
                num = query.Length;
            startIndex1 = num + 1;
            var str = HttpUtility.UrlDecode(query.Substring(startIndex2, num - startIndex2), encoding);
            yield return new KeyValuePair<string, string?>(name ?? "", str);
        }
    }
}