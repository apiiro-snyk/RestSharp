using System.Text;

namespace RestSharp.Tests;

/// <summary>
/// Note: These tests do not handle QueryString building, which is handled in Http, not RestClient
/// </summary>
public class UrlBuilderTests {
    [Fact]
    public void GET_with_empty_base_and_query_parameters_without_encoding() {
        var request = new RestRequest("http://example.com/resource?param1=value1")
            .AddQueryParameter("foo", "bar,baz", false);
        AssertUri(request, "http://example.com/resource?param1=value1&foo=bar,baz");
    }

    [Fact]
    public void GET_with_empty_base_and_resource_containing_tokens() {
        var request = new RestRequest("http://example.com/resource/{foo}").AddUrlSegment("foo", "bar");
        AssertUri(request, "http://example.com/resource/bar");
    }

    [Fact]
    public void GET_with_empty_request() {
        var request  = new RestRequest();
        AssertUri("http://example.com", request, "http://example.com/");
    }

    [Fact]
    public void GET_with_empty_request_and_bare_hostname() {
        var request  = new RestRequest();
        AssertUri("http://example.com", request, "http://example.com/");
    }

    [Fact]
    public void GET_with_empty_request_and_query_parameters_without_encoding() {
        var request = new RestRequest().AddQueryParameter("foo", "bar,baz", false);
        AssertUri("http://example.com/resource?param1=value1", request, "http://example.com/resource?param1=value1&foo=bar,baz");
    }

    [Fact]
    public void GET_with_Invalid_Url_string_throws_exception()
        => Assert.Throws<UriFormatException>(
            () => {
                var unused = new RestClient("invalid url");
            }
        );

    [Fact]
    public void GET_with_leading_slash() {
        var request = new RestRequest("/resource");
        AssertUri("http://example.com", request, "http://example.com/resource");
    }

    [Fact]
    public void GET_with_leading_slash_and_baseurl_trailing_slash() {
        var request = new RestRequest("/resource").AddParameter("foo", "bar");
        AssertUri("http://example.com", request, "http://example.com/resource?foo=bar");
    }

    [Fact]
    public void GET_with_multiple_instances_of_same_key() {
        var request = new RestRequest("v1/people/~/network/updates")
            .AddParameter("type", "STAT")
            .AddParameter("type", "PICT")
            .AddParameter("count", "50")
            .AddParameter("start", "50");
        AssertUri("https://api.linkedin.com", request, "https://api.linkedin.com/v1/people/~/network/updates?type=STAT&type=PICT&count=50&start=50");
    }

    [Fact]
    public void GET_with_resource_containing_null_token() {
        var request = new RestRequest("/resource/{foo}");
        Assert.Throws<ArgumentNullException>(() => request.AddUrlSegment("foo", null!));
    }

    [Fact]
    public void GET_with_resource_containing_slashes() {
        var request = new RestRequest("resource/foo");
        AssertUri("http://example.com", request, "http://example.com/resource/foo");
    }

    [Fact]
    public void GET_with_resource_containing_tokens() {
        var request = new RestRequest("resource/{foo}").AddUrlSegment("foo", "bar");
        AssertUri("http://example.com", request, "http://example.com/resource/bar");
    }

    [Fact]
    public void GET_with_Uri_and_resource_containing_tokens() {
        var request = new RestRequest("resource/{baz}")
            .AddUrlSegment("foo", "bar")
            .AddUrlSegment("baz", "bat");
        AssertUri("http://example.com/{foo}", request, "http://example.com/bar/resource/bat");
    }

    [Fact]
    public void GET_with_Uri_containing_tokens() {
        var request = new RestRequest().AddUrlSegment("foo", "bar");
        AssertUri("http://example.com/{foo}", request, "http://example.com/bar");
    }

    [Fact]
    public void GET_with_Url_string_and_resource_containing_tokens() {
        var request = new RestRequest("resource/{baz}")
            .AddUrlSegment("foo", "bar")
            .AddUrlSegment("baz", "bat");

        AssertUri("http://example.com/{foo}", request, "http://example.com/bar/resource/bat");
    }

    [Fact]
    public void GET_with_Url_string_containing_tokens() {
        var request = new RestRequest().AddUrlSegment("foo", "bar");

        AssertUri("http://example.com/{foo}", request, "http://example.com/bar");
    }

    [Fact]
    public void GET_wth_trailing_slash_and_query_parameters() {
        var request = new RestRequest("/resource/").AddParameter("foo", "bar");

        AssertUri("http://example.com", request, "http://example.com/resource/?foo=bar");
    }

    [Fact]
    public void POST_with_leading_slash() {
        var request = new RestRequest("/resource", Method.Post);

        AssertUri("http://example.com", request, "http://example.com/resource");
    }

    [Fact]
    public void POST_with_leading_slash_and_baseurl_trailing_slash() {
        var request = new RestRequest("/resource", Method.Post);

        AssertUri("http://example.com", request, "http://example.com/resource");
    }

    [Fact]
    public void POST_with_querystring_containing_tokens() {
        var request = new RestRequest("resource", Method.Post).AddParameter("foo", "bar", ParameterType.QueryString);

        AssertUri("http://example.com", request, "http://example.com/resource?foo=bar");
    }

    [Fact]
    public void POST_with_resource_containing_slashes() {
        var request = new RestRequest("resource/foo", Method.Post);

        AssertUri("http://example.com", request, "http://example.com/resource/foo");
    }

    [Fact]
    public void POST_with_resource_containing_tokens() {
        var request = new RestRequest("resource/{foo}", Method.Post);
        request.AddUrlSegment("foo", "bar");

        AssertUri("http://example.com", request, "http://example.com/resource/bar");
    }

    [Fact]
    public void Should_add_parameter_if_it_is_new() {
        var request = new RestRequest();
        request.AddOrUpdateParameter("param2", "value2");
        request.AddOrUpdateParameter("param3", "value3");

        AssertUri("http://example.com/resource?param1=value1", request, "http://example.com/resource?param1=value1&param2=value2&param3=value3");
    }

    [Fact]
    public void Should_build_uri_using_selected_encoding() {
        // adding parameter with o-slash character which is encoded differently between
        // utf-8 and iso-8859-1
        var request = new RestRequest().AddOrUpdateParameter("town", "Hillerød");

        const string expectedDefaultEncoding  = "http://example.com/resource?town=Hiller%c3%b8d";
        const string expectedIso89591Encoding = "http://example.com/resource?town=Hiller%f8d";

        AssertUri("http://example.com/resource", request, expectedDefaultEncoding);

        using var client2 = new RestClient(new RestClientOptions("http://example.com/resource") { Encoding = Encoding.GetEncoding("ISO-8859-1") });
        AssertUri(client2, request, expectedIso89591Encoding);
    }

    [Fact]
    public void Should_build_uri_with_resource_full_uri() {
        var request = new RestRequest("https://www.example1.com/connect/authorize");

        AssertUri("https://www.example1.com/", request, "https://www.example1.com/connect/authorize");
    }

    [Fact]
    public void Should_encode_colon() {
        // adding parameter with o-slash character which is encoded differently between
        // utf-8 and iso-8859-1
        var request = new RestRequest().AddOrUpdateParameter("parameter", "some:value");

        AssertUri("http://example.com/resource", request, "http://example.com/resource?parameter=some%3avalue");
    }

    [Fact]
    public void Should_not_duplicate_question_mark() {
        var request = new RestRequest().AddParameter("param2", "value2");

        AssertUri("http://example.com/resource?param1=value1", request, "http://example.com/resource?param1=value1&param2=value2");
    }

    [Fact]
    public void Should_not_touch_request_url() {
        const string baseUrl    = "http://rs.test.org";
        const string requestUrl = "reportserver?/Prod/Report";

        var req = new RestRequest(requestUrl, Method.Post);

        AssertUri(baseUrl, req, $"{baseUrl}/{requestUrl}");
    }

    [Fact]
    public void Should_update_parameter_if_it_already_exists() {
        var request = new RestRequest()
            .AddOrUpdateParameter("param2", "value2")
            .AddOrUpdateParameter("param2", "value2-1");

        AssertUri("http://example.com/resource?param1=value1", request, "http://example.com/resource?param1=value1&param2=value2-1");
    }

    [Fact]
    public void Should_use_ipv6_address() {
        var baseUrl = new Uri("https://[fe80::290:e8ff:fe8b:2537%en10]:8443");
        var request = new RestRequest("api/v1/auth");

        using var client = new RestClient(baseUrl);

        var actual = client.BuildUri(request);
        actual.HostNameType.Should().Be(UriHostNameType.IPv6);
        actual.AbsoluteUri.Should().Be("https://[fe80::290:e8ff:fe8b:2537]:8443/api/v1/auth");
    }

    [Fact]
    public void Should_not_encode_pipe() {
        var request = new RestRequest("resource").AddQueryParameter("ids", "in:001|116", false);
        AssertUri("http://example.com/", request, "http://example.com/resource?ids=in:001|116");
    }

    static void AssertUri(RestClient client, RestRequest request, string expected) {
        var actual = client.BuildUri(request);
        actual.AbsoluteUri.Should().Be(expected);
    }

    static void AssertUri(string basePath, RestRequest request, string expected) {
        using var client = new RestClient(basePath);
        AssertUri(client, request, expected);
    }

    static void AssertUri(RestRequest request, string expected) {
        using var client = new RestClient();
        AssertUri(client, request, expected);
    }
}