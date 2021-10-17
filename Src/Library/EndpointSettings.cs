﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace FastEndpoints;

internal class EndpointSettings
{
    internal string[]? Routes;
    internal string[]? Verbs;
    internal string[]? AnonymousVerbs;
    internal bool ThrowIfValidationFails = true;
    internal string[]? PreBuiltUserPolicies;
    internal string[]? Roles;
    internal string[]? Permissions;
    internal bool AllowAnyPermission;
    internal string[]? Claims;
    internal bool AllowAnyClaim;
    internal bool AllowFileUploads;
    internal Action<RouteHandlerBuilder>? InternalConfigAction;
    internal Action<RouteHandlerBuilder>? UserConfigAction;
    internal object? PreProcessors;
    internal object? PostProcessors;
    internal ResponseCacheAttribute? ResponseCacheSettings;
}