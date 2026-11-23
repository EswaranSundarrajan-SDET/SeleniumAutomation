using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using AutomationFramework.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AutomationFramework.TestSupport;

// A fresh loopback-only fixture and data store are created for each scenario.
public static class DemoApp
{
    public static WebApplication Create()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var app = builder.Build();
        var settlements = new ConcurrentDictionary<string, Settlement>();
        var session = Guid.NewGuid().ToString("N");
        app.MapGet("/", () => Html("""
            <form method="post" action="/login">
              <label>User <input id="user" name="user"></label>
              <label>Password <input type="password" id="password" name="password"></label>
              <button id="login">Sign in</button>
            </form>
            """));
        app.MapPost("/login", async (HttpContext ctx) =>
        {
            var form = await ctx.Request.ReadFormAsync();
            if (form["user"] != "demo" || form["password"] != "demo-only")
                return Html("<p id='error'>Invalid credentials</p>");

            ctx.Response.Cookies.Append("demo-session", session, new CookieOptions { HttpOnly = true });
            return Results.Redirect("/settlement");
        });
        app.MapGet("/settlement", (HttpContext ctx) =>
            ctx.Request.Cookies["demo-session"] == session ? Html("""
            <form id="settlement" method="post">
              <label>Reference <input required id="reference" name="reference"></label>
              <label>Amount <input required id="amount" name="amount"></label>
              <button id="submit">Submit settlement</button>
            </form>
            """) : Results.Redirect("/"));
        app.MapPost("/settlement", async (HttpContext ctx) =>
        {
            if (ctx.Request.Cookies["demo-session"] != session) return Results.Unauthorized();
            var form = await ctx.Request.ReadFormAsync();
            string reference = form["reference"].ToString();
            if (string.IsNullOrWhiteSpace(reference) ||
                !decimal.TryParse(form["amount"], NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ||
                amount <= 0) return Results.BadRequest();
            if (!settlements.TryAdd(reference, new Settlement(reference, amount))) return Results.Conflict();
            return Html($"<p id='confirmation'>Submitted {WebUtility.HtmlEncode(reference)}</p>");
        });
        app.MapGet("/api/settlements/{reference}", (string reference) =>
            settlements.TryGetValue(reference, out var item) ? Results.Ok(item) : Results.NotFound());
        return app;
    }

    private static IResult Html(string body) => Results.Content(body, "text/html; charset=utf-8");
}
