namespace TaskService.IntegrationTests;

public static class JwtTestHelper
{
    public static void SetUserId(HttpClient client, Guid userId) =>
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());

    public static void ClearUserId(HttpClient client) =>
        client.DefaultRequestHeaders.Remove("X-User-Id");
}
