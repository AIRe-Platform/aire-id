// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


namespace Aire;

public static class AireIdEnvironment
{
    public static string? StorageConnectionString => Environment.GetEnvironmentVariable("StorageConnectionString");
    public static string? CommunicationServiceConnectionString => Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
    public static string? EmailSenderAddress => Environment.GetEnvironmentVariable("EmailSenderAddress");
    public static string? EmailDomainWhitelist => Environment.GetEnvironmentVariable("EmailDomainWhitelist");
    public static string? TokenIssuer => Environment.GetEnvironmentVariable("TOKEN_ISSUER");
    public static string? TokenAudience => Environment.GetEnvironmentVariable("TOKEN_AUDIENSE");
    public static string? GlobalRecoveryKey => Environment.GetEnvironmentVariable("GLOBAL_RECOVERY_KEY");
    public static string? OpenApiHost => Environment.GetEnvironmentVariable("OpenApi__HostNames");
}
