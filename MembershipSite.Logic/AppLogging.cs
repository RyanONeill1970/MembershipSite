namespace MembershipSite.Logic;

using Sentry;
using System;

/// <summary>
/// Central logging point for raising logged data to the logging framework.
/// </summary>
public static class AppLogging
{
    public static void Write(string message)
    {
        var ex = new Exception(message);
        SentrySdk.CaptureException(ex);
    }

    public static void Write(Exception ex)
    {
        SentrySdk.CaptureException(ex);
    }
}