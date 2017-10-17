﻿// ==========================================================================
//  MustBeAppEditorAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Shared.Identity;

namespace Squidex.Pipeline
{
    public sealed class MustBeAppEditorAttribute : ApiAuthorizeAttribute
    {
        public MustBeAppEditorAttribute()
        {
            Roles = SquidexRoles.AppEditor;
        }
    }
}
